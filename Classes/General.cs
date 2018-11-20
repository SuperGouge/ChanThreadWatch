using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;

namespace JDP {
    public static class General {
        public static string Version {
            get {
                Version ver = Assembly.GetExecutingAssembly().GetName().Version;
                return ver.Major + "." + ver.Minor + "." + ver.Revision;
            }
        }

        public static string ReleaseDate {
            get { return "2018-Nov-21"; }
        }

        public static string ProgramURL {
            get { return "https://github.com/SuperGouge/ChanThreadWatch/releases"; }
        }

        public static string WikiURL {
            get { return "https://github.com/SuperGouge/ChanThreadWatch/wiki"; }
        }

        public static Action DownloadAsync(string url, string auth, string referer, string connectionGroupName, DateTime? cacheLastModifiedTime, Action<HttpWebResponse> onResponse, Action<byte[], int> onDownloadChunk, Action onComplete, Action<Exception> onException) {
            const int readBufferSize = 8192;
            const int requestTimeoutMS = 60000;
            const int readTimeoutMS = 60000;
            object sync = new object();
            bool aborting = false;
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            Stream responseStream = null;
            Action cleanup = () => {
                if (request != null) {
                    request.Abort();
                    request = null;
                }
                if (responseStream != null) {
                    try { responseStream.Close(); }
                    catch { }
                    responseStream = null;
                }
                if (response != null) {
                    try { response.Close(); }
                    catch { }
                    response = null;
                }
            };
            Action<Exception> abortDownloadInternal = (ex) => {
                lock (sync) {
                    if (aborting) return;
                    aborting = true;
                    cleanup();
                    onException(ex);
                }
            };
            Action abortDownload = () => {
                ThreadPool.QueueUserWorkItem((s) => {
                    abortDownloadInternal(new Exception("Download has been aborted."));
                });
            };
            lock (sync) {
                try {
                    request = BuildWebRequest(url: url, auth: auth, connectionGroupName: connectionGroupName, cacheLastModifiedTime: cacheLastModifiedTime, referer: referer);
                    // Unfortunately BeginGetResponse blocks until the DNS lookup has finished
                    IAsyncResult requestResult = request.BeginGetResponse((requestResultParam) => {
                        lock (sync) {
                            try {
                                if (aborting) return;
                                response = (HttpWebResponse)request.EndGetResponse(requestResultParam);
                                if (GetMIMETypeFromContentType(response.ContentType) == "text/html") {
                                    var memoryStream = new MemoryStream();
                                    CopyStream(new ThrottledStream(response.GetResponseStream(), Settings.MaximumBytesPerSecond ?? ThrottledStream.Infinite), memoryStream);
                                    memoryStream.Position = 0;
                                    byte[] redirectPageBytes = memoryStream.ToArray();
                                    Encoding pageEncoding = DetectHTMLEncoding(redirectPageBytes, response.ContentType);
                                    string metaRedirectHtml = pageEncoding.GetString(redirectPageBytes);
                                    memoryStream.Position = 0;
                                    responseStream = memoryStream;
                                    string redirectUrl = GetRedirectUrl(metaRedirectHtml, response.ResponseUri.AbsoluteUri);
                                    if (!string.IsNullOrEmpty(redirectUrl)) {
                                        HttpWebRequest redirectionRequest = BuildWebRequest(url: redirectUrl, auth: auth, connectionGroupName: connectionGroupName, cacheLastModifiedTime: cacheLastModifiedTime);
                                        response = (HttpWebResponse)redirectionRequest.GetResponse();
                                        responseStream = new ThrottledStream(response.GetResponseStream(), Settings.MaximumBytesPerSecond ?? ThrottledStream.Infinite);
                                    }
                                }
                                else {
                                    responseStream = new ThrottledStream(response.GetResponseStream(), Settings.MaximumBytesPerSecond ?? ThrottledStream.Infinite);
                                }
                                onResponse(response);
                                byte[] buff = new byte[readBufferSize];
                                AsyncCallback readCallback = null;
                                readCallback = (readResultParam) => {
                                    lock (sync) {
                                        try {
                                            if (aborting) return;
                                            if (readResultParam != null) {
                                                int bytesRead = responseStream.EndRead(readResultParam);
                                                if (bytesRead == 0) {
                                                    request = null;
                                                    onComplete();
                                                    aborting = true;
                                                    cleanup();
                                                    return;
                                                }
                                                onDownloadChunk(buff, bytesRead);
                                            }
                                            IAsyncResult readResult = responseStream.BeginRead(buff, 0, buff.Length, readCallback, null);
                                            ThreadPool.RegisterWaitForSingleObject(readResult.AsyncWaitHandle,
                                                (state, timedOut) => {
                                                    if (!timedOut) return;
                                                    abortDownloadInternal(new Exception("Timed out while reading response."));
                                                }, null, readTimeoutMS, true);
                                        }
                                        catch (Exception ex) {
                                            abortDownloadInternal(ex);
                                        }
                                    }
                                };
                                readCallback(null);
                            }
                            catch (Exception ex) {
                                if (ex is WebException) {
                                    WebException webEx = (WebException)ex;
                                    if (webEx.Status == WebExceptionStatus.ProtocolError) {
                                        HttpStatusCode code = ((HttpWebResponse)webEx.Response).StatusCode;
                                        if (code == HttpStatusCode.NotFound) {
                                            ex = new HTTP404Exception();
                                        }
                                        else if (code == HttpStatusCode.NotModified) {
                                            ex = new HTTP304Exception();
                                        }
                                    }
                                }
                                abortDownloadInternal(ex);
                            }
                        }
                    }, null);
                    ThreadPool.RegisterWaitForSingleObject(requestResult.AsyncWaitHandle,
                        (state, timedOut) => {
                            if (!timedOut) return;
                            abortDownloadInternal(new Exception("Timed out while waiting for response."));
                        }, null, requestTimeoutMS, true);
                }
                catch (Exception ex) {
                    abortDownloadInternal(ex);
                }
            }
            return abortDownload;
        }

        public static string GetRedirectUrl(string html, string currentPage) {
            try {
                HTMLParser parser = new HTMLParser(html);
                foreach (HTMLTag metaTag in parser.FindStartTags(parser.CreateTagRange(parser.FindStartTag("head")), "meta")) {
                    if (!string.Equals(metaTag.GetAttributeValueOrEmpty("http-equiv"), "Refresh", StringComparison.OrdinalIgnoreCase)) {
                        continue;
                    }
                    string metaContent = metaTag.GetAttributeValueOrEmpty("Content");
                    if (string.IsNullOrEmpty(metaContent)) {
                        continue;
                    }
                    int currentPosition = 0;
                    currentPosition = GetNextNonWhiteSpaceCharacterPosition(metaContent, currentPosition);
                    StringBuilder timeString = new StringBuilder();
                    while (metaContent.Length > currentPosition && (metaContent[currentPosition] >= 48 && metaContent[currentPosition] <= 57 || metaContent[currentPosition] == 46)) {
                        timeString.Append(metaContent[currentPosition++]);
                    }
                    int time;
                    int.TryParse(timeString.ToString(), out time);
                    if (time < 0) {
                        return null;
                    }
                    currentPosition = GetNextNonWhiteSpaceCharacterPosition(metaContent, currentPosition);
                    if (!IsCharacterMatch(metaContent[currentPosition++], '\u003B')) {
                        return null;
                    }
                    currentPosition = GetNextNonWhiteSpaceCharacterPosition(metaContent, currentPosition);
                    if (!IsCharacterMatch(metaContent[currentPosition++], '\u0055')) {
                        return null;
                    }
                    if (!IsCharacterMatch(metaContent[currentPosition++], '\u0052')) {
                        return null;
                    }
                    if (!IsCharacterMatch(metaContent[currentPosition++], '\u004C')) {
                        return null;
                    }
                    currentPosition = GetNextNonWhiteSpaceCharacterPosition(metaContent, currentPosition);
                    if (!IsCharacterMatch(metaContent[currentPosition++], '\u003D')) {
                        return null;
                    }
                    currentPosition = GetNextNonWhiteSpaceCharacterPosition(metaContent, currentPosition);
                    char quote = new char();
                    if (IsCharacterMatch(metaContent[currentPosition], '\u0027') || IsCharacterMatch(metaContent[currentPosition], '\u0022')) {
                        quote = metaContent[currentPosition++];
                    }
                    string redirectUrl = metaContent.Substring(currentPosition);
                    if (!string.IsNullOrEmpty(quote.ToString())) {
                        redirectUrl = redirectUrl.TrimEnd(quote);
                    }
                    redirectUrl = redirectUrl.TrimEnd('\u0020', '\u0009', '\u000A', '\u000C', '\u000D');
                    redirectUrl = redirectUrl.Replace('\u0009', '\0').Replace('\u000A', '\0').Replace('\u000D', '\0');
                    redirectUrl = GetAbsoluteURL(currentPage, redirectUrl);
                    return redirectUrl;
                }
                return null;
            }
            catch {
                return null;
            }
        }

        public static string DownloadPageToString(string url) {
            HttpWebRequest request = BuildWebRequest(url: url);
            HttpWebResponse response = null;
            Stream responseStream = null;
            MemoryStream memoryStream = null;
            try {
                response = (HttpWebResponse)request.GetResponse();
                responseStream = response.GetResponseStream();
                memoryStream = new MemoryStream();
                CopyStream(responseStream, memoryStream);
                byte[] pageBytes = memoryStream.ToArray();
                Encoding encoding = DetectHTMLEncoding(pageBytes, response.ContentType);
                return encoding.GetString(pageBytes);
            }
            finally {
                if (responseStream != null)
                    try { responseStream.Close(); }
                    catch { }
                if (response != null)
                    try { response.Close(); }
                    catch { }
                if (memoryStream != null)
                    try { memoryStream.Close(); }
                    catch { }
            }
        }

        private static HttpWebRequest BuildWebRequest(string url, string auth = null, string connectionGroupName = null, string referer = null, DateTime? cacheLastModifiedTime = null) {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            if (connectionGroupName != null) {
                request.ConnectionGroupName = connectionGroupName;
            }
            request.UserAgent = (Settings.UseCustomUserAgent == true) ? Settings.CustomUserAgent : ("Chan Thread Watch " + Version);
            if (cacheLastModifiedTime != null) {
                request.IfModifiedSince = cacheLastModifiedTime.Value;
            }
            if (!String.IsNullOrEmpty(auth)) {
                Encoding encoding = Encoding.GetEncoding("iso-8859-1");
                request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(encoding.GetBytes(auth)));
            }
            if (!String.IsNullOrEmpty(auth)) {
                request.Referer = referer;
            }
            return request;
        }

        private static void CopyStream(Stream srcStream, params Stream[] dstStreams) {
            byte[] data = new byte[8192];
            while (true) {
                int dataLen = srcStream.Read(data, 0, data.Length);
                if (dataLen == 0) break;
                foreach (Stream dstStream in dstStreams) {
                    if (dstStream != null) {
                        dstStream.Write(data, 0, dataLen);
                    }
                }
            }
        }

        public static DateTime? GetResponseLastModifiedTime(HttpWebResponse response) {
            DateTime? lastModified = null;
            if (response.Headers["Last-Modified"] != null) {
                try {
                    // Parse the time string ourself instead of using .LastModified because
                    // older versions of Mono don't convert it from GMT to local.
                    lastModified = DateTime.ParseExact(response.Headers["Last-Modified"],
                        new[] { "r", "dddd, dd-MMM-yy HH:mm:ss G\\MT", "ddd MMM d HH:mm:ss yyyy" },
                        CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal);
                }
                catch { }
            }
            return lastModified;
        }

        public static int GetNextNonWhiteSpaceCharacterPosition(string str, int currentPosition) {
            while (char.IsWhiteSpace(str[currentPosition])) {
                currentPosition++;
            }
            return currentPosition;
        }

        public static bool IsCharacterMatch(char inputChar, char comparisonCharacter) {
            return char.ToUpperInvariant(inputChar).Equals(char.ToUpperInvariant(comparisonCharacter));
        }

        public static Encoding DetectHTMLEncoding(byte[] bytes, string httpContentType) {
            string charSet =
                GetCharSetFromContentType(httpContentType) ??
                DetectCharacterSetFromBOM(bytes) ??
                DetectCharacterSetFromContent(bytes, httpContentType);
            if (charSet != null) {
                if (IsUTF8(charSet)) {
                    return new UTF8Encoding(HasBOM(bytes));
                }
                else if (IsUTF16(charSet)) {
                    return new UnicodeEncoding(IsUTFBigEndian(charSet) ?? false, HasBOM(bytes));
                }
                else {
                    try {
                        return Encoding.GetEncoding(charSet);
                    }
                    catch { }
                }
            }
            return Encoding.GetEncoding("Windows-1252");
        }

        private static string DetectCharacterSetFromBOM(byte[] bytes) {
            switch (GetBOMType(bytes)) {
                case BOMType.UTF8: return "UTF-8";
                case BOMType.UTF16LE: return "UTF-16LE";
                case BOMType.UTF16BE: return "UTF-16BE";
                default: return null;
            }
        }

        private static string DetectCharacterSetFromContent(byte[] bytes, string httpContentType) {
            string text = UnknownEncodingToString(bytes, 4096);
            HTMLParser htmlParser = new HTMLParser(text);
            string mimeType = GetMIMETypeFromContentType(httpContentType) ?? String.Empty;
            string charSet;

            if (mimeType.Equals("application/xhtml+xml", StringComparison.OrdinalIgnoreCase) ||
                mimeType.Equals("application/xml", StringComparison.OrdinalIgnoreCase) ||
                mimeType.Equals("text/xml", StringComparison.OrdinalIgnoreCase))
            {
                if (text.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase)) {
                    // XML declaration
                    HTMLParser xmlParser = new HTMLParser("<" + text.Substring(2));
                    HTMLTag xmlTag = xmlParser.Tags.Count >= 1 ? xmlParser.Tags[0] : null;
                    if (xmlTag != null && xmlTag.NameEquals("xml") && xmlTag.Offset == 0) {
                        charSet = xmlTag.GetAttributeValue("encoding");
                        if (!String.IsNullOrEmpty(charSet)) return charSet;
                    }
                }

                // Default
                return "UTF-8";
            }

            foreach (HTMLTag tag in htmlParser.FindStartTags("meta")) {
                // charset attribute
                charSet = tag.GetAttributeValue("charset");
                if (!String.IsNullOrEmpty(charSet)) return charSet;

                // http-equiv and content attributes
                if (tag.GetAttributeValueOrEmpty("http-equiv").Trim().Equals("Content-Type", StringComparison.OrdinalIgnoreCase)) {
                    charSet = GetCharSetFromContentType(tag.GetAttributeValue("content"));
                    if (!String.IsNullOrEmpty(charSet)) return charSet;
                }
            }

            return null;
        }

        public static string GetMIMETypeFromContentType(string contentType) {
            if (contentType == null) return null;
            int pos = contentType.IndexOf(';');
            if (pos != -1) {
                contentType = contentType.Substring(0, pos);
            }
            contentType = contentType.Trim();
            return contentType.Length != 0 ? contentType : null;
        }

        public static string GetCharSetFromContentType(string contentType) {
            if (contentType == null) return null;
            foreach (string part in contentType.Split(';')) {
                int pos = part.IndexOf('=');
                if (pos == -1) continue;
                string name = part.Substring(0, pos).Trim();
                if (!name.Equals("charset", StringComparison.OrdinalIgnoreCase)) continue;
                string value = part.Substring(pos + 1).Trim();
                bool isQuoted = value.Length >= 1 && (value[0] == '"' || value[0] == '\'');
                if (isQuoted) {
                    pos = value.IndexOf(value[0], 1);
                    if (pos == -1) pos = value.Length;
                    value = value.Substring(1, pos - 1).Trim();
                }
                return value.Length != 0 ? value : null;
            }
            return null;
        }

        public static string UnknownEncodingToString(byte[] src, int maxLength) {
            byte[] dst = new byte[maxLength > 0 ? Math.Min(maxLength, src.Length) : src.Length];
            int iDst = 0;
            for (int iSrc = 0; iSrc < src.Length; iSrc++) {
                if (src[iSrc] == 0) continue;
                dst[iDst++] = src[iSrc];
                if (iDst >= dst.Length) break;
            }
            return Encoding.ASCII.GetString(dst, 0, iDst);
        }

        private static BOMType GetBOMType(byte[] bytes) {
            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF) return BOMType.UTF8;
            if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE) return BOMType.UTF16LE;
            if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF) return BOMType.UTF16BE;
            return BOMType.None;
        }

        private static bool HasBOM(byte[] bytes) {
            return GetBOMType(bytes) != BOMType.None;
        }

        private static bool IsUTF8(string charSet) {
            return charSet.Equals("UTF-8", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsUTF16(string charSet) {
            return charSet.Equals("UTF-16", StringComparison.OrdinalIgnoreCase) ||
                   charSet.Equals("UTF-16BE", StringComparison.OrdinalIgnoreCase) ||
                   charSet.Equals("UTF-16LE", StringComparison.OrdinalIgnoreCase);
        }

        private static bool? IsUTFBigEndian(string charSet) {
            if (charSet.EndsWith("BE", StringComparison.OrdinalIgnoreCase)) return true;
            if (charSet.EndsWith("LE", StringComparison.OrdinalIgnoreCase)) return false;
            return null;
        }

        public static bool ArraysAreEqual<T>(T[] a, T[] b) where T : IComparable {
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++) {
                if (a[i].CompareTo(b[i]) != 0) return false;
            }
            return true;
        }

        public static string GetAbsoluteURL(string baseURL, string relativeURL) {
            try {
                Uri uri;
                if (!Uri.TryCreate(new Uri(baseURL), relativeURL, out uri)) {
                    return null;
                }
                // AbsoluteUri can throw undocumented Exception (e.g. for "mailto:+")
                return uri.AbsoluteUri;
            }
            catch {
                return null;
            }
        }

        public static string StripFragmentFromURL(string url) {
            int pos = url.IndexOf('#');
            return pos != -1 ? url.Substring(0, pos) : url;
        }

        public static string CleanPageURL(string url) {
            url = url.Trim();
            url = StripFragmentFromURL(url);
            if (url.Length == 0) return null;
            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                url = "http://" + url;
            }
            if (url.IndexOf('/', url.IndexOf("//", StringComparison.Ordinal) + 2) == -1) return null;
            try {
                Uri uri;
                if (!Uri.TryCreate(url, UriKind.Absolute, out uri)) return null;
                return uri.AbsoluteUri;
            }
            catch {
                return null;
            }
        }

        public static string GetRelativeDirectoryPath(string dir, string baseDir) {
            if (dir.Length != 0 && Path.IsPathRooted(dir)) {
                Uri baseDirUri = new Uri(Path.Combine(baseDir, "dummy.txt"));
                Uri targetDirUri = new Uri(Path.Combine(dir, "dummy.txt"));
                try {
                    dir = Uri.UnescapeDataString(baseDirUri.MakeRelativeUri(targetDirUri).ToString());
                }
                catch (UriFormatException) {
                    // Workaround for Mono when determining the relative URI of directories
                    // on different drives in Windows.
                    return dir;
                }
                dir = (dir.Length == 0) ? "." : Path.GetDirectoryName(dir.Replace('/', Path.DirectorySeparatorChar));
            }
            return dir;
        }

        public static string GetAbsoluteDirectoryPath(string dir, string baseDir) {
            if (dir.Length != 0 && !Path.IsPathRooted(dir)) {
                dir = Path.GetFullPath(Path.Combine(baseDir, dir));
            }
            return dir;
        }

        public static string GetRelativeFilePath(string filePath, string baseDir) {
            if (filePath.Length != 0 && Path.IsPathRooted(filePath)) {
                string dir = Path.GetDirectoryName(filePath);
                string fileName = Path.GetFileName(filePath);
                dir = GetRelativeDirectoryPath(dir, baseDir);
                filePath = (dir == ".") ? fileName : Path.Combine(dir, fileName);
            }
            return filePath;
        }

        public static string GetAbsoluteFilePath(string filePath, string baseDir) {
            if (filePath.Length != 0 && !Path.IsPathRooted(filePath)) {
                filePath = Path.GetFullPath(Path.Combine(baseDir, filePath));
            }
            return filePath;
        }

        public static string GetLastDirectory(string dir) {
            char[] separators = new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
            dir = dir.TrimEnd(separators);
            int pos = dir.LastIndexOfAny(separators);
            return (pos == -1) ? String.Empty : dir.Substring(pos + 1);
        }

        public static string RemoveLastDirectory(string dir) {
            char[] separators = new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
            dir = dir.TrimEnd(separators);
            int pos = dir.LastIndexOfAny(separators);
            return (pos == -1) ? dir : dir.Substring(0, pos);
        }

        public static int GetMaximumFileNameLength(string dir) {
            // Kind of a binary search except we only know whether the middle
            // item is <= or > the target rather than <, =, or >.
            int min = 0;
            int max = 4096;
            while (max >= min + 2) {
                int n = (min + max) / 2;
                if (IsFileNameTooLong(dir, n)) {
                    max = n - 1;
                }
                else {
                    min = n;
                }
            }
            if (max > min) {
                return IsFileNameTooLong(dir, max) ? min : max;
            }
            else {
                return min;
            }
        }

        public static bool IsFileNameTooLong(string dir, int fileNameLength) {
            if (!Directory.Exists(dir)) throw new DirectoryNotFoundException();
            string path = null;
            bool foundFreeFileName = false;
            for (char c = 'a'; c <= 'z'; c++) {
                path = Path.Combine(dir, new string(c, fileNameLength));
                if (!File.Exists(path)) {
                    foundFreeFileName = true;
                    break;
                }
            }
            if (!foundFreeFileName) {
                throw new Exception("Unable to determine if filename is too long.");
            }
            try {
                using (File.Create(path)) { }
                try { File.Delete(path); }
                catch { }
                return false;
            }
            catch (PathTooLongException) {
                return true;
            }
            catch (DirectoryNotFoundException) {
                // Workaround for Mono
                return true;
            }
        }

        public static void EnsureThreadPoolMaxThreads(int minWorkerThreads, int minCompletionPortThreads) {
            int workerThreads;
            int completionPortThreads;
            ThreadPool.GetMaxThreads(out workerThreads, out completionPortThreads);
            if (workerThreads < minWorkerThreads || completionPortThreads < minCompletionPortThreads) {
                ThreadPool.SetMaxThreads(Math.Max(workerThreads, minWorkerThreads), Math.Max(completionPortThreads, minCompletionPortThreads));
            }
        }

        public static byte[] TryBase64Decode(string s) {
            try {
                return Convert.FromBase64String(s);
            }
            catch {
                return null;
            }
        }

        public static ulong Calculate64BitMD5(byte[] bytes) {
            using (MD5CryptoServiceProvider hashAlgo = new MD5CryptoServiceProvider()) {
                return BytesTo64BitXor(hashAlgo.ComputeHash(bytes));
            }
        }

        public static ulong BytesTo64BitXor(byte[] bytes) {
            ulong result = 0;
            for (int i = 0; i < bytes.Length; i++) {
                result ^= (ulong)bytes[i] << ((7 - (i % 8)) * 8);
            }
            return result;
        }

        public static void WriteReplacedString(string str, List<ReplaceInfo> replaceList, TextWriter outStream) {
            int offset = 0;
            replaceList.Sort((x, y) => x.Offset.CompareTo(y.Offset));
            for (int iReplace = 0; iReplace < replaceList.Count; iReplace++) {
                ReplaceInfo replace = replaceList[iReplace];
                if (replace.Offset < offset || replace.Length < 0) continue;
                if (replace.Offset + replace.Length > str.Length) break;
                if (replace.Offset > offset) {
                    outStream.Write(str.Substring(offset, replace.Offset - offset));
                }
                if (!String.IsNullOrEmpty(replace.Value)) {
                    outStream.Write(replace.Value);
                }
                offset = replace.Offset + replace.Length;
            }
            if (str.Length > offset) {
                outStream.Write(str.Substring(offset));
            }
        }

        public static void AddOtherReplaces(HTMLParser htmlParser, string pageURL, List<ReplaceInfo> replaceList) {
            HashSet<int> existingOffsets = new HashSet<int>();

            foreach (ReplaceInfo replace in replaceList) {
                existingOffsets.Add(replace.Offset);
            }

            if (Environment.NewLine != "\n") {
                int offset = 0;
                while ((offset = htmlParser.PreprocessedHTML.IndexOf('\n', offset)) != -1) {
                    replaceList.Add(new ReplaceInfo {
                        Offset = offset,
                        Length = 1,
                        Type = ReplaceType.Other,
                        Value = Environment.NewLine
                    });
                    offset += 1;
                }
            }

            foreach (HTMLTag tag in htmlParser.FindStartTags("base")) {
                replaceList.Add(
                    new ReplaceInfo {
                        Offset = tag.Offset,
                        Length = tag.Length,
                        Type = ReplaceType.Other,
                        Value = String.Empty
                    });
            }

            foreach (HTMLTag tag in htmlParser.FindStartTags("a", "img", "script", "link")) {
                bool isATag = tag.NameEquals("a");
                bool isImgTag = tag.NameEquals("img");
                bool isScriptTag = tag.NameEquals("script");
                bool isLinkTag = tag.NameEquals("link");
                bool usesHRefAttr = isATag || isLinkTag;
                bool usesSrcAttr = isImgTag || isScriptTag;
                if (usesHRefAttr || usesSrcAttr) {
                    HTMLAttribute attribute = tag.GetAttribute(usesHRefAttr ? "href" : usesSrcAttr ? "src" : null);
                    if (attribute != null && !existingOffsets.Contains(attribute.Offset)) {
                        // Make attribute's URL absolute
                        string newURL = GetAbsoluteURL(pageURL, HttpUtility.HtmlDecode(attribute.Value));
                        // For links to anchors on the current page, use just the fragment
                        if (isATag && newURL != null && newURL.Length > pageURL.Length &&
                            newURL.StartsWith(pageURL, StringComparison.Ordinal) && newURL[pageURL.Length] == '#')
                        {
                            newURL = newURL.Substring(pageURL.Length);
                        }
                        if (newURL != null) {
                            replaceList.Add(
                                new ReplaceInfo {
                                    Offset = attribute.Offset,
                                    Length = attribute.Length,
                                    Type = ReplaceType.Other,
                                    Value = attribute.Name + "=\"" + HttpUtility.HtmlAttributeEncode(newURL) + "\""
                                });
                        }
                    }
                }
            }
        }

        public static string URLFileName(string url) {
            int pos = url.LastIndexOf("/", StringComparison.Ordinal);
            return (pos == -1) ? String.Empty : url.Substring(pos + 1);
        }

        public static string CleanFileName(string src) {
            char[] dst = new char[src.Length];
            char[] inv = Path.GetInvalidFileNameChars();
            int iDst = 0;
            for (int iSrc = 0; iSrc < src.Length; iSrc++) {
                char c = src[iSrc];
                for (int j = 0; j < inv.Length; j++) {
                    if (c == inv[j]) {
                        c = (char)0;
                        break;
                    }
                }
                if (c != 0) {
                    dst[iDst++] = c;
                }
            }
            return new string(dst, 0, iDst);
        }

        public static string HtmlAttributeEncode(string s, bool allowHashParameter = true) {
            return HttpUtility.HtmlAttributeEncode(allowHashParameter ? s : s.Replace("#", "%23"));
        }

        public static int StrLen(byte[] bytes) {
            for (int i = 0; i < bytes.Length; i++) {
                if (bytes[i] == 0) return i;
            }
            return bytes.Length;
        }

        public static int StrLenW(byte[] bytes) {
            for (int i = 0; i < bytes.Length - 1; i += 2) {
                if (bytes[i] == 0 && bytes[i + 1] == 0) return i / 2;
            }
            return bytes.Length / 2;
        }

        public static void BackupThreadList(bool checkSize = false) {
            try {
                string path = Path.Combine(Settings.GetSettingsDirectory(), Settings.ThreadsFileName);
                if (!File.Exists(path)) return;
                var backupInfo = new FileInfo(path + ".bak");
                if (!checkSize || !backupInfo.Exists || new FileInfo(path).Length >= backupInfo.Length) {
                    string[] lines = File.ReadAllLines(path);
                    if (lines.Length < 1) return;
                    File.WriteAllLines(path + ".bak", lines);
                }
            }
            catch (Exception ex) {
                Logger.Log(ex.ToString());
            }
        }
    }
}