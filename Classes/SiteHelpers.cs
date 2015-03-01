using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace JDP {
    public class SiteHelper {
        protected string _url = String.Empty;
        protected HTMLParser _htmlParser;

        public static SiteHelper GetInstance(string host) {
            Type type = null;
            try {
                string ns = (typeof(SiteHelper)).Namespace;
                string[] hostSplit = host.ToLower(CultureInfo.InvariantCulture).Split('.');
                for (int i = 0; i < hostSplit.Length - 1; i++) {
                    type = Assembly.GetExecutingAssembly().GetType(ns + ".SiteHelper_" +
                                                                   String.Join("_", hostSplit, i, hostSplit.Length - i));
                    if (type != null) break;
                }
            }
            catch { }
            if (type == null) type = typeof(SiteHelper);
            return (SiteHelper)Activator.CreateInstance(type);
        }

        public void SetURL(string url) {
            _url = url;
        }

        public void SetHTMLParser(HTMLParser htmlParser) {
            _htmlParser = htmlParser;
        }

        public HTMLParser GetHTMLParser() {
            return _htmlParser;
        }

        protected string[] SplitURL() {
            return SplitURL(_url);
        }

        protected string[] SplitURL(string url) {
            int pos = url.IndexOf("://", StringComparison.Ordinal);
            if (pos == -1) return new string[0];
            return url.Substring(pos + 3).Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public virtual string GetSiteName() {
            string[] hostSplit = (new Uri(_url)).Host.Split('.');
            return (hostSplit.Length >= 2) ? hostSplit[hostSplit.Length - 2] : String.Empty;
        }

        public virtual string GetBoardName() {
            string[] urlSplit = SplitURL();
            return (urlSplit.Length >= 3) ? urlSplit[1] : String.Empty;
        }

        public virtual string GetThreadName() {
            string[] urlSplit = SplitURL();
            if (urlSplit.Length >= 3) {
                string page = urlSplit[urlSplit.Length - 1];
                int pos = page.IndexOf('?');
                if (pos != -1) page = page.Substring(0, pos);
                pos = page.LastIndexOf('.');
                if (pos != -1) page = page.Substring(0, pos);
                return page;
            }
            return String.Empty;
        }

        protected virtual string GetThreadName(SlugType slugType) {
            return GetThreadName(_url, slugType);
        }

        protected virtual string GetThreadName(string url, SlugType slugType) {
            return String.Empty;
        }

        public virtual string GetThreadID() {
            return GetThreadName();
        }

        public virtual string GetPageID() {
            return String.Join("/", new[] { GetSiteName(), GetBoardName(), GetThreadID() });
        }

        public virtual bool HasSlug() {
            return HasSlug(_url);
        }

        protected virtual bool HasSlug(string url) {
            return false;
        }

        public virtual bool IsBoardHighTurnover() {
            return false;
        }

        protected virtual string ImageURLKeyword {
            get { return "/src/"; }
        }

        protected virtual bool IsImage(HTMLTag linkTag) {
            string url = General.GetAbsoluteURL(_url, HttpUtility.HtmlDecode(linkTag.GetAttributeValueOrEmpty("href")));
            return url != null && url.IndexOf(ImageURLKeyword, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public virtual List<ImageInfo> GetImages(List<ReplaceInfo> replaceList, List<ThumbnailInfo> thumbnailList, bool local = false) {
            HashSet<string> imageFileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> thumbnailFileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<ImageInfo> imageList = new List<ImageInfo>();
            HTMLAttribute attribute;
            string url;
            int pos;

            foreach (HTMLTag linkTag in _htmlParser.FindStartTags("a")) {
                attribute = linkTag.GetAttribute("href");
                if (attribute == null) continue;
                url = General.GetAbsoluteURL(_url, HttpUtility.HtmlDecode(attribute.Value));
                if (url == null || !IsImage(linkTag)) continue;

                HTMLTag linkEndTag = _htmlParser.FindCorrespondingEndTag(linkTag);
                if (linkEndTag == null) continue;

                ImageInfo image = new ImageInfo { Poster = String.Empty };
                ThumbnailInfo thumb = null;

                image.URL = url;
                if (image.URL == null || image.FileName.Length == 0) continue;
                pos = Math.Max(
                    image.URL.LastIndexOf("http://", StringComparison.OrdinalIgnoreCase),
                    image.URL.LastIndexOf("https://", StringComparison.OrdinalIgnoreCase));
                if (pos == -1) {
                    image.Referer = _url;
                }
                else {
                    image.Referer = image.URL;
                    image.URL = image.URL.Substring(pos);
                }
                if (replaceList != null) {
                    replaceList.Add(
                        new ReplaceInfo {
                            Offset = attribute.Offset,
                            Length = attribute.Length,
                            Type = ReplaceType.ImageLinkHref,
                            Tag = image.FileName
                        });
                }

                HTMLTag imageTag = _htmlParser.FindStartTag(linkTag, linkEndTag, "img");
                if (imageTag != null) {
                    attribute = imageTag.GetAttribute("src");
                    if (attribute != null) {
                        url = General.GetAbsoluteURL(_url, HttpUtility.HtmlDecode(attribute.Value));
                        if (url != null) {
                            thumb = new ThumbnailInfo();
                            thumb.URL = url;
                            thumb.Referer = _url;
                            if (replaceList != null) {
                                replaceList.Add(
                                    new ReplaceInfo {
                                        Offset = attribute.Offset,
                                        Length = attribute.Length,
                                        Type = ReplaceType.ImageSrc,
                                        Tag = thumb.FileName
                                    });
                            }
                        }
                    }
                }

                if (!imageFileNames.Contains(image.FileName)) {
                    imageList.Add(image);
                    imageFileNames.Add(image.FileName);
                }
                if (thumb != null && !thumbnailFileNames.Contains(thumb.FileName)) {
                    thumbnailList.Add(thumb);
                    thumbnailFileNames.Add(thumb.FileName);
                }
            }

            return imageList;
        }

        public virtual HashSet<string> GetCrossLinks(List<ReplaceInfo> replaceList, bool interBoardAutoFollow) {
            return new HashSet<string>();
        }

        public virtual void ResurrectDeadPosts(HTMLParser previousParser, List<ReplaceInfo> replaceList) {
        }

        public virtual string GetNextPageURL() {
            return null;
        }
    }

    public class SiteHelper_4chan_org : SiteHelper {
        public override string GetThreadName() {
            if (HasSlug()) {
                return GetThreadName(Settings.SlugType);
            }
            if (Settings.UseSlug == true) {
                try {
                    HTMLParser parser = new HTMLParser(General.DownloadPageToString(_url));
                    HTMLTag canonicalLinkTag = Enumerable.FirstOrDefault(Enumerable.Where(parser.FindStartTags(parser.CreateTagRange(parser.FindStartTag("head")), "link"), t => t.GetAttributeValueOrEmpty("rel").Equals("canonical")));
                    return GetThreadName(canonicalLinkTag.GetAttributeValueOrEmpty("href"), Settings.SlugType);
                }
                catch {
                    return GetThreadID();
                }
            }
            return GetThreadID();
        }

        protected override string GetThreadName(string url, SlugType slugType) {
            if (Settings.UseSlug != true || !HasSlug(url)) return GetThreadID();
            string[] urlSplit = SplitURL(url);
            switch (slugType) {
                case SlugType.First:
                    return urlSplit[urlSplit.Length - 1] + "_" + urlSplit[urlSplit.Length - 2];
                case SlugType.Last:
                    return urlSplit[urlSplit.Length - 2] + "_" + urlSplit[urlSplit.Length - 1];
                case SlugType.Only:
                    return urlSplit[urlSplit.Length - 1];
                default:
                    return urlSplit[urlSplit.Length - 2];
            }
        }

        public override string GetThreadID() {
            string[] urlSplit = SplitURL();
            return HasSlug() ? urlSplit[urlSplit.Length - 2] : urlSplit[urlSplit.Length - 1];
        }

        protected override bool HasSlug(string url) {
            return url.Contains("/thread/") && SplitURL(url).Length == 5;
        }

        protected override string ImageURLKeyword {
            get { return "//i.4cdn.org/"; }
        }

        public override List<ImageInfo> GetImages(List<ReplaceInfo> replaceList, List<ThumbnailInfo> thumbnailList, bool local = false) {
            List<ImageInfo> imageList = new List<ImageInfo>();
            bool seenSpoiler = false;

            foreach (HTMLTagRange postTagRange in Enumerable.Where(Enumerable.Select(Enumerable.Where(_htmlParser.FindStartTags("div"),
                t => HTMLParser.ClassAttributeValueHas(t, "post")), t => _htmlParser.CreateTagRange(t)), r => r != null))
            {
                HTMLTagRange fileTextDivTagRange = _htmlParser.CreateTagRange(Enumerable.FirstOrDefault(Enumerable.Where(
                    _htmlParser.FindStartTags(postTagRange, "div"), t => HTMLParser.ClassAttributeValueHas(t, "fileText"))));
                if (fileTextDivTagRange == null) continue;

                HTMLTagRange fileThumbLinkTagRange = _htmlParser.CreateTagRange(Enumerable.FirstOrDefault(Enumerable.Where(
                    _htmlParser.FindStartTags(postTagRange, "a"), t => HTMLParser.ClassAttributeValueHas(t, "fileThumb"))));
                if (fileThumbLinkTagRange == null) continue;

                HTMLTag fileTextLinkStartTag = _htmlParser.FindStartTag(fileTextDivTagRange, "a");
                if (fileTextLinkStartTag == null) continue;

                HTMLTag fileThumbImageTag = _htmlParser.FindStartTag(fileThumbLinkTagRange, "img");
                if (fileThumbImageTag == null) continue;

                string imageURL = fileTextLinkStartTag.GetAttributeValue("href");
                if (imageURL == null || (!local && imageURL.IndexOf(ImageURLKeyword, StringComparison.OrdinalIgnoreCase) == -1)) continue;

                string thumbURL = fileThumbImageTag.GetAttributeValue("src");
                if (thumbURL == null) continue;

                bool isSpoiler = HTMLParser.ClassAttributeValueHas(fileThumbLinkTagRange.StartTag, "imgspoiler");

                string originalFileName;
                if (isSpoiler) {
                    originalFileName = fileTextDivTagRange.StartTag.GetAttributeValue("title");
                }
                else {
                    originalFileName = fileTextLinkStartTag.GetAttributeValue("title") ?? _htmlParser.GetInnerHTML(_htmlParser.CreateTagRange(fileTextLinkStartTag));
                }

                string imageMD5 = fileThumbImageTag.GetAttributeValue("data-md5");
                if (imageMD5 == null) continue;

                string poster = String.Empty;
                HTMLTagRange nameBlockSpanTagRange = _htmlParser.CreateTagRange(Enumerable.FirstOrDefault(Enumerable.Where(
                    _htmlParser.FindStartTags(postTagRange, "span"), t => HTMLParser.ClassAttributeValueHas(t, "nameBlock"))));

                if (nameBlockSpanTagRange != null) {
                    HTMLTagRange nameSpanTagRange = _htmlParser.CreateTagRange(Enumerable.FirstOrDefault(Enumerable.Where(
                        _htmlParser.FindStartTags(nameBlockSpanTagRange, "span"), t => HTMLParser.ClassAttributeValueHas(t, "name"))));

                    HTMLTagRange posterTripSpanTagRange = _htmlParser.CreateTagRange(Enumerable.FirstOrDefault(Enumerable.Where(
                        _htmlParser.FindStartTags(nameBlockSpanTagRange, "span"), t => HTMLParser.ClassAttributeValueHas(t, "postertrip"))));

                    HTMLTagRange idSpanTagRange = _htmlParser.CreateTagRange(Enumerable.FirstOrDefault(Enumerable.Where(
                        _htmlParser.FindStartTags(nameBlockSpanTagRange, "span"), t => HTMLParser.ClassAttributeValueHas(t, "hand"))));
                    
                    if (idSpanTagRange != null) {
                        poster = _htmlParser.GetInnerHTML(idSpanTagRange);
                    }
                    else if (nameSpanTagRange != null) {
                        string name = _htmlParser.GetInnerHTML(nameSpanTagRange);
                        if (posterTripSpanTagRange != null) {
                            poster = name + _htmlParser.GetInnerHTML(posterTripSpanTagRange);
                        }
                        else if (name != "Anonymous") {
                            poster = name;
                        }
                    }
                }
                
                ImageInfo image = new ImageInfo {
                    URL = General.GetAbsoluteURL(_url, HttpUtility.HtmlDecode(imageURL)),
                    Referer = _url,
                    OriginalFileName = General.CleanFileName(HttpUtility.HtmlDecode(originalFileName) ?? ""),
                    HashType = HashType.MD5,
                    Hash = General.TryBase64Decode(imageMD5),
                    Poster = General.CleanFileName(poster)
                };
                if (image.URL.Length == 0 || image.FileName.Length == 0 || image.Hash == null) continue;

                ThumbnailInfo thumb = new ThumbnailInfo {
                    URL = General.GetAbsoluteURL(_url, HttpUtility.HtmlDecode(thumbURL)),
                    Referer = _url
                };
                if (thumb.URL == null || thumb.FileName.Length == 0) continue;

                if (replaceList != null) {
                    HTMLAttribute attribute;

                    attribute = fileTextLinkStartTag.GetAttribute("href");
                    if (attribute != null) {
                        replaceList.Add(
                            new ReplaceInfo {
                                Offset = attribute.Offset,
                                Length = attribute.Length,
                                Type = ReplaceType.ImageLinkHref,
                                Tag = image.FileName
                            });
                    }

                    attribute = fileThumbLinkTagRange.StartTag.GetAttribute("href");
                    if (attribute != null) {
                        replaceList.Add(
                            new ReplaceInfo {
                                Offset = attribute.Offset,
                                Length = attribute.Length,
                                Type = ReplaceType.ImageLinkHref,
                                Tag = image.FileName
                            });
                    }

                    attribute = fileThumbImageTag.GetAttribute("src");
                    if (attribute != null) {
                        replaceList.Add(
                            new ReplaceInfo {
                                Offset = attribute.Offset,
                                Length = attribute.Length,
                                Type = ReplaceType.ImageSrc,
                                Tag = thumb.FileName
                            });
                    }
                }

                imageList.Add(image);

                if (!isSpoiler || !seenSpoiler) {
                    thumbnailList.Add(thumb);
                    if (isSpoiler) seenSpoiler = true;
                }
            }

            return imageList;
        }

        public override HashSet<string> GetCrossLinks(List<ReplaceInfo> replaceList, bool interBoardAutoFollow) {
            HashSet<string> crossLinks = new HashSet<string>();

            foreach (HTMLTagRange postMessageTagRange in Enumerable.Where(Enumerable.Select(Enumerable.Where(_htmlParser.FindStartTags("blockquote"),
                t => HTMLParser.ClassAttributeValueHas(t, "postMessage")), t => _htmlParser.CreateTagRange(t)), r => r != null))
            {
                foreach (HTMLTag quoteLinkTag in Enumerable.Where(_htmlParser.FindStartTags(postMessageTagRange, "a"),
                    t => HTMLParser.ClassAttributeValueHas(t, "quotelink")))
                {
                    HTMLAttribute attribute = quoteLinkTag.GetAttribute("href");
                    string href = attribute.Value.Substring(0, attribute.Value.Contains("#") ? attribute.Value.IndexOf('#') : attribute.Value.Length);
                    if (!href.Contains("/thread/") || (!interBoardAutoFollow && GetBoardName() != href.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)[0])) continue;
                    crossLinks.Add(General.GetAbsoluteURL(_url, href));
                    if (replaceList != null) {
                        replaceList.Add(
                            new ReplaceInfo {
                                Offset = attribute.Offset,
                                Length = attribute.Length,
                                Type = ReplaceType.QuoteLinkHref,
                                Tag = href.Replace("/thread", "").Insert(0, GetSiteName()),
                                Value = attribute.Value
                            });
                    }
                }

                foreach (HTMLTagRange deadLinkTagRange in Enumerable.Where(Enumerable.Select(Enumerable.Where(_htmlParser.FindStartTags(postMessageTagRange, "span"),
                    t => HTMLParser.ClassAttributeValueHas(t, "deadlink")), t => _htmlParser.CreateTagRange(t)), r => r != null))
                {
                    string boardName;
                    string pageID;
                    string deadLinkInnerHTML = HttpUtility.HtmlDecode(_htmlParser.GetInnerHTML(deadLinkTagRange));
                    if (deadLinkInnerHTML.Contains(">>>")) {
                        boardName = deadLinkInnerHTML.Split('/')[1];
                        pageID = deadLinkInnerHTML.Split('/')[2];
                    }
                    else {
                        boardName = GetBoardName();
                        pageID = deadLinkInnerHTML.Substring(2);
                    }

                    if (replaceList != null) {
                        replaceList.Add(
                            new ReplaceInfo {
                                Offset = deadLinkTagRange.Offset,
                                Length = deadLinkTagRange.Length,
                                Type = ReplaceType.DeadLink,
                                Tag = String.Join("/", new[] { GetSiteName(), boardName, pageID }),
                                Value = _htmlParser.GetHTML(deadLinkTagRange)
                            });
                    }
                }
            }
            return crossLinks;
        }

        public override void ResurrectDeadPosts(HTMLParser previousParser, List<ReplaceInfo> replaceList) {
            if (previousParser == null) return;
            List<ReplaceInfo> tempReplaceList = new List<ReplaceInfo>();
            Dictionary<string, HTMLTagRange> newPostContainers = new Dictionary<string, HTMLTagRange>();
            Dictionary<string, HTMLTagRange> resurrectedPostContainers = new Dictionary<string, HTMLTagRange>();
            foreach (HTMLTagRange postContainerTagRange in Enumerable.Where(Enumerable.Select(Enumerable.Where(_htmlParser.FindStartTags("div"),
                t => HTMLParser.ClassAttributeValueHas(t, "postContainer")), t => _htmlParser.CreateTagRange(t)), r => r != null))
            {
                newPostContainers.Add(postContainerTagRange.StartTag.GetAttributeValue("id"), postContainerTagRange);
            }

            HTMLTagRange lastExistingPostContainerTagRange = null;
            foreach (HTMLTagRange previousPostContainerTagRange in Enumerable.Where(Enumerable.Select(Enumerable.Where(previousParser.FindStartTags("div"),
                t => HTMLParser.ClassAttributeValueHas(t, "postContainer")), t => previousParser.CreateTagRange(t)), r => r != null))
            {
                HTMLTagRange tempTagRange;
                if (!newPostContainers.TryGetValue(previousPostContainerTagRange.StartTag.GetAttributeValue("id"), out tempTagRange)) {
                    int offset = lastExistingPostContainerTagRange != null ? lastExistingPostContainerTagRange.EndOffset :
                        Enumerable.FirstOrDefault(Enumerable.Where(_htmlParser.FindStartTags("div"), t => HTMLParser.ClassAttributeValueHas(t, "thread"))).EndOffset;
                    HTMLTag inputTag = previousParser.FindTag(false, previousPostContainerTagRange, "input");
                    string value = previousParser.GetHTML(previousPostContainerTagRange);
                    if (!value.Contains("<strong style=\"color: #FF0000\">[Deleted]</strong>")) {
                        value = value.Insert(inputTag.EndOffset - previousPostContainerTagRange.Offset, "<strong style=\"color: #FF0000\">[Deleted]</strong>");
                    }
                    tempReplaceList.Add(
                            new ReplaceInfo {
                                Offset = offset,
                                Length = 0,
                                Type = ReplaceType.DeadPost,
                                Tag = previousPostContainerTagRange.StartTag.GetAttributeValue("id"),
                                Value = value
                            });
                    resurrectedPostContainers.Add(previousPostContainerTagRange.StartTag.GetAttributeValue("id"), previousPostContainerTagRange);
                }
                else {
                    lastExistingPostContainerTagRange = tempTagRange;
                }
            }

            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb)) {
                General.WriteReplacedString(_htmlParser.PreprocessedHTML, tempReplaceList, sw);
            }
            _htmlParser = new HTMLParser(sb.ToString());

            tempReplaceList.Clear();
            foreach (HTMLTagRange deadLinkTagRange in Enumerable.Where(Enumerable.Select(Enumerable.Where(_htmlParser.FindStartTags("span"),
                    t => HTMLParser.ClassAttributeValueHas(t, "deadlink")), t => _htmlParser.CreateTagRange(t)), r => r != null))
            {
                string deadLinkInnerHTML = HttpUtility.HtmlDecode(_htmlParser.GetInnerHTML(deadLinkTagRange));
                if (String.IsNullOrEmpty(deadLinkInnerHTML) || deadLinkInnerHTML.Contains(">>>")) continue;
                string deadLinkID = deadLinkInnerHTML.Substring(2);
                if (resurrectedPostContainers.ContainsKey("pc" + deadLinkID)) {
                    tempReplaceList.Add(
                        new ReplaceInfo {
                            Offset = deadLinkTagRange.Offset,
                            Length = deadLinkTagRange.Length,
                            Type = ReplaceType.DeadLink,
                            Tag = "pc" + deadLinkID,
                            Value = "<a class=\"quotelink\" href=\"#p" + deadLinkID + "\">>>" + deadLinkID + "</a>"
                        });
                }
            }

            sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb)) {
                General.WriteReplacedString(_htmlParser.PreprocessedHTML, tempReplaceList, sw);
            }
            _htmlParser = new HTMLParser(sb.ToString());

            if (replaceList == null) return;
            foreach (HTMLTagRange postContainerTagRange in Enumerable.Where(Enumerable.Select(Enumerable.Where(_htmlParser.FindStartTags("div"),
                t => HTMLParser.ClassAttributeValueHas(t, "postContainer")), t => _htmlParser.CreateTagRange(t)), r => r != null))
            {
                if (!resurrectedPostContainers.ContainsKey(postContainerTagRange.StartTag.GetAttributeValue("id"))) continue;

                HTMLTagRange fileTextDivTagRange = _htmlParser.CreateTagRange(Enumerable.FirstOrDefault(Enumerable.Where(
                    _htmlParser.FindStartTags(postContainerTagRange, "div"), t => HTMLParser.ClassAttributeValueHas(t, "fileText"))));
                if (fileTextDivTagRange == null) continue;

                HTMLTagRange fileThumbLinkTagRange = _htmlParser.CreateTagRange(Enumerable.FirstOrDefault(Enumerable.Where(
                    _htmlParser.FindStartTags(postContainerTagRange, "a"), t => HTMLParser.ClassAttributeValueHas(t, "fileThumb"))));
                if (fileThumbLinkTagRange == null) continue;

                HTMLTag fileTextLinkStartTag = _htmlParser.FindStartTag(fileTextDivTagRange, "a");
                if (fileTextLinkStartTag == null) continue;

                HTMLTag fileThumbImageTag = _htmlParser.FindStartTag(fileThumbLinkTagRange, "img");
                if (fileThumbImageTag == null) continue;

                HTMLAttribute attribute = fileTextLinkStartTag.GetAttribute("href");
                if (attribute != null) {
                    replaceList.Add(
                        new ReplaceInfo {
                            Offset = attribute.Offset,
                            Length = attribute.Length,
                            Type = ReplaceType.ImageLinkHref,
                            Tag = String.Empty,
                            Value = "href=\"" + General.HtmlAttributeEncode(attribute.Value, false) + "\""
                        });
                }

                attribute = fileThumbLinkTagRange.StartTag.GetAttribute("href");
                if (attribute != null) {
                    replaceList.Add(
                        new ReplaceInfo {
                            Offset = attribute.Offset,
                            Length = attribute.Length,
                            Type = ReplaceType.ImageLinkHref,
                            Tag = String.Empty,
                            Value = "href=\"" + General.HtmlAttributeEncode(attribute.Value, false) + "\""
                        });
                }

                attribute = fileThumbImageTag.GetAttribute("src");
                if (attribute != null) {
                    replaceList.Add(
                        new ReplaceInfo {
                            Offset = attribute.Offset,
                            Length = attribute.Length,
                            Type = ReplaceType.ImageSrc,
                            Tag = String.Empty,
                            Value = "src=\"" + General.HtmlAttributeEncode(attribute.Value, false) + "\""
                        });
                }
            }
        }

        public override bool IsBoardHighTurnover() {
            return String.Equals(GetBoardName(), "b", StringComparison.OrdinalIgnoreCase);
        }
    }

    public class SiteHelper_krautchan_net : SiteHelper {
        public override string GetThreadName() {
            string threadName = base.GetThreadName();
            if (threadName.StartsWith("thread-", StringComparison.OrdinalIgnoreCase)) {
                threadName = threadName.Substring(7);
            }
            return threadName;
        }

        protected override string ImageURLKeyword {
            get { return "/files/"; }
        }
    }

    public abstract class FuukaSiteHelper : SiteHelper {
        protected override bool IsImage(HTMLTag linkTag) {
            return Enumerable.FirstOrDefault(Enumerable.Where(_htmlParser.FindStartTags(_htmlParser.CreateTagRange(linkTag), "img"), t => HTMLParser.ClassAttributeValueHas(t, "thumb"))) != null;
        }

        public override List<ImageInfo> GetImages(List<ReplaceInfo> replaceList, List<ThumbnailInfo> thumbnailList, bool local = false) {
            List<ImageInfo> imageList = new List<ImageInfo>();

            foreach (HTMLTagRange postTagRange in Enumerable.Where(Enumerable.Select(Enumerable.Where(_htmlParser.FindStartTags("td", "div"),
                t => new Regex("^p\\d+$").IsMatch(t.GetAttributeValueOrEmpty("id"))), t => _htmlParser.CreateTagRange(t)), r => r != null))
            {
                HTMLTagRange labelTagRange = _htmlParser.CreateTagRange(_htmlParser.FindStartTag(postTagRange, "label"));
                if (labelTagRange == null) continue;

                HTMLTagRange imageLinkTagRange = _htmlParser.CreateTagRange(Enumerable.FirstOrDefault(Enumerable.Where(_htmlParser.FindStartTags(postTagRange, "a"), IsImage)));
                if (imageLinkTagRange == null) continue;

                HTMLTag thumbImageTag = _htmlParser.FindStartTag(imageLinkTagRange, "img");
                if (thumbImageTag == null) continue;

                string imageURL = imageLinkTagRange.StartTag.GetAttributeValue("href");
                if (imageURL == null) continue;

                string thumbURL = thumbImageTag.GetAttributeValue("src");
                if (thumbURL == null) continue;

                HTMLTagRange fileInfoTagRange = _htmlParser.CreateTagRange(_htmlParser.FindStartTag(labelTagRange.EndTag, null, "span"));
                if (fileInfoTagRange == null) continue;

                string[] fileInfoSplit = _htmlParser.GetInnerHTML(fileInfoTagRange).Split(new[] { ',' }, 3);
                if (fileInfoSplit.Length < 3) continue;
                
                string originalFileName;
                string imageMD5 = null;
                string fileInfo = fileInfoSplit[2].Trim();
                if (fileInfo.EndsWith("-->")) {
                    int hashIndex = fileInfo.LastIndexOf("<!--", StringComparison.Ordinal);
                    originalFileName = fileInfo.Remove(hashIndex).Trim();
                    imageMD5 = fileInfo.Substring(hashIndex).Replace("<!--", "").Replace("-->", "").Trim();
                }
                else {
                    originalFileName = fileInfo;
                    HTMLTag similarImageLinkStartTag = Enumerable.FirstOrDefault(Enumerable.Where(
                        _htmlParser.FindStartTags(fileInfoTagRange.EndTag, imageLinkTagRange.StartTag, "a"), t => t.GetAttributeValueOrEmpty("href").Contains("/image/")));
                    if (similarImageLinkStartTag != null) {
                        string[] hrefSplit = similarImageLinkStartTag.GetAttributeValueOrEmpty("href").Split('/');
                        imageMD5 = hrefSplit[hrefSplit.Length - 1].Replace('-', '+').Replace('_', '/');
                        imageMD5 = imageMD5.PadRight(imageMD5.Length + (4 - imageMD5.Length % 4) % 4, '=');
                    }
                }

                HTMLTagRange posterNameSpanTagRange = _htmlParser.CreateTagRange(Enumerable.FirstOrDefault(Enumerable.Where(
                    _htmlParser.FindStartTags(labelTagRange, "span"), t => HTMLParser.ClassAttributeValueHas(t, "postername"))));

                HTMLTagRange posterTripSpanTagRange = _htmlParser.CreateTagRange(Enumerable.FirstOrDefault(Enumerable.Where(
                    _htmlParser.FindStartTags(labelTagRange, "span"), t => HTMLParser.ClassAttributeValueHas(t, "postertrip"))));
                    
                string poster = String.Empty;
                if (posterNameSpanTagRange != null) {
                    string name = _htmlParser.GetInnerHTML(_htmlParser.CreateTagRange(_htmlParser.FindStartTag(posterNameSpanTagRange, "a")) ?? posterNameSpanTagRange).Replace("\r", "").Replace("\n", "").Replace("&nbsp;", "").Trim();
                    if (posterTripSpanTagRange != null) {
                        poster = name + _htmlParser.GetInnerHTML(posterTripSpanTagRange).Replace("&nbsp;", "").Trim();
                    }
                    else if (name != "Anonymous") {
                        poster = name;
                    }
                }

                ImageInfo image = new ImageInfo {
                    URL = General.GetAbsoluteURL(_url, HttpUtility.HtmlDecode(imageURL)),
                    Referer = _url,
                    OriginalFileName = General.CleanFileName(HttpUtility.HtmlDecode(originalFileName)),
                    HashType = imageMD5 != null ? HashType.MD5 : HashType.None,
                    Hash = General.TryBase64Decode(imageMD5),
                    Poster = General.CleanFileName(HttpUtility.HtmlDecode(poster))
                };
                if (image.URL.Length == 0 || image.FileName.Length == 0) continue;

                ThumbnailInfo thumb = new ThumbnailInfo {
                    URL = General.GetAbsoluteURL(_url, HttpUtility.HtmlDecode(thumbURL)),
                    Referer = _url
                };
                if (thumb.URL == null || thumb.FileName.Length == 0) continue;

                if (replaceList != null) {
                    HTMLAttribute attribute;

                    attribute = imageLinkTagRange.StartTag.GetAttribute("href");
                    if (attribute != null) {
                        replaceList.Add(
                            new ReplaceInfo {
                                Offset = attribute.Offset,
                                Length = attribute.Length,
                                Type = ReplaceType.ImageLinkHref,
                                Tag = image.FileName
                            });
                    }

                    attribute = thumbImageTag.GetAttribute("src");
                    if (attribute != null) {
                        replaceList.Add(
                            new ReplaceInfo {
                                Offset = attribute.Offset,
                                Length = attribute.Length,
                                Type = ReplaceType.ImageSrc,
                                Tag = thumb.FileName
                            });
                    }
                }

                imageList.Add(image);
                thumbnailList.Add(thumb);
            }

            return imageList;
        }
    }

    public class SiteHelper_archive_rebeccablacktech_com : FuukaSiteHelper { }

    public class SiteHelper_rbt_asia : FuukaSiteHelper { }

    public class SiteHelper_warosu_org : FuukaSiteHelper { }
    
    public abstract class FoolFuukaSiteHelper : SiteHelper {
        public override List<ImageInfo> GetImages(List<ReplaceInfo> replaceList, List<ThumbnailInfo> thumbnailList, bool local = false) {
            List<ImageInfo> imageList = new List<ImageInfo>();

            foreach (HTMLTagRange postTagRange in Enumerable.Where(Enumerable.Select(Enumerable.Where(_htmlParser.FindStartTags("article"),
                t => HTMLParser.ClassAttributeValueHas(t, "has_image") || (HTMLParser.ClassAttributeValueHas(t, "thread") && t.GetAttribute("id") != null)), t => _htmlParser.CreateTagRange(t)), r => r != null))
            {
                HTMLTagRange imageLinkTagRange = _htmlParser.CreateTagRange(Enumerable.FirstOrDefault(Enumerable.Where(
                    _htmlParser.FindStartTags(postTagRange, "a"), t => HTMLParser.ClassAttributeValueHas(t, "thread_image_link"))));
                if (imageLinkTagRange == null) continue;
                
                HTMLTag thumbImageTag = _htmlParser.FindStartTag(imageLinkTagRange, "img");
                if (thumbImageTag == null) continue;

                string imageURL = imageLinkTagRange.StartTag.GetAttributeValue("href");
                if (imageURL == null) continue;

                string thumbURL = thumbImageTag.GetAttributeValue("src");
                if (thumbURL == null) continue;
                
                HTMLTagRange postFileTagRange = _htmlParser.CreateTagRange(Enumerable.FirstOrDefault(Enumerable.Where(
                    _htmlParser.FindStartTags(postTagRange, "div"), t => HTMLParser.ClassAttributeValueHas(t, "post_file"))));
                if (postFileTagRange == null) continue;

                string originalFileName = String.Empty;
                HTMLTagRange fileNameLinkTagRange = _htmlParser.CreateTagRange(Enumerable.FirstOrDefault(Enumerable.Where(
                    _htmlParser.FindStartTags(postFileTagRange, "a"), t => HTMLParser.ClassAttributeValueHas(t, "post_file_filename"))));
                if (fileNameLinkTagRange == null) {
                    HTMLTagRange fileNameSpanTagRange = _htmlParser.CreateTagRange(Enumerable.FirstOrDefault(Enumerable.Where(
                        _htmlParser.FindStartTags(postFileTagRange, "span"), t => HTMLParser.ClassAttributeValueHas(t, "post_file_filename"))));
                    if (fileNameSpanTagRange == null) {
                        HTMLTag postFileControlsEndTag = _htmlParser.FindCorrespondingEndTag(Enumerable.FirstOrDefault(Enumerable.Where(
                            _htmlParser.FindStartTags(postFileTagRange, "span"), t => HTMLParser.ClassAttributeValueHas(t, "post_file_controls"))));
                        HTMLTag postFileMetadataStartTag = Enumerable.FirstOrDefault(Enumerable.Where(
                            _htmlParser.FindStartTags(postFileTagRange, "span"), t => HTMLParser.ClassAttributeValueHas(t, "post_file_metadata")));
                        if (postFileControlsEndTag != null && postFileMetadataStartTag != null) {
                            originalFileName = _htmlParser.GetInnerHTML(postFileControlsEndTag, postFileMetadataStartTag).Trim().TrimEnd(',');
                        }
                        else if (postFileControlsEndTag == null && postFileMetadataStartTag == null) {
                            string[] postFileSplit = _htmlParser.GetInnerHTML(postFileTagRange).Split(new[] { ',' }, 3);
                            if (postFileSplit.Length == 3) {
                                originalFileName = postFileSplit[2].Trim();
                            }
                        }
                    }
                    else {
                        originalFileName = fileNameSpanTagRange.StartTag.GetAttributeValue("title") ?? _htmlParser.GetInnerHTML(fileNameSpanTagRange);
                    }
                }
                else {
                    originalFileName = fileNameLinkTagRange.StartTag.GetAttributeValue("title") ?? _htmlParser.GetInnerHTML(fileNameLinkTagRange);
                }

                string imageMD5 = thumbImageTag.GetAttributeValue("data-md5");
                if (imageMD5 == null) continue;

                HTMLTagRange posterDataSpanTagRange = _htmlParser.CreateTagRange(Enumerable.FirstOrDefault(Enumerable.Where(
                    _htmlParser.FindStartTags(postTagRange, "span"), t => HTMLParser.ClassAttributeValueHas(t, "post_poster_data"))));
                if (posterDataSpanTagRange == null) continue;
                
                HTMLTagRange authorSpanTagRange = _htmlParser.CreateTagRange(Enumerable.FirstOrDefault(Enumerable.Where(
                    _htmlParser.FindStartTags(posterDataSpanTagRange, "span"), t => HTMLParser.ClassAttributeValueHas(t, "post_author"))));

                HTMLTagRange tripcodeSpanTagRange = _htmlParser.CreateTagRange(Enumerable.FirstOrDefault(Enumerable.Where(
                    _htmlParser.FindStartTags(posterDataSpanTagRange, "span"), t => HTMLParser.ClassAttributeValueHas(t, "post_tripcode"))));
                    
                HTMLTagRange idSpanTagRange = _htmlParser.CreateTagRange(Enumerable.FirstOrDefault(Enumerable.Where(
                    _htmlParser.FindStartTags(posterDataSpanTagRange, "span"), t => HTMLParser.ClassAttributeValueHas(t, "poster_hash"))));
                    
                string poster = String.Empty;
                if (idSpanTagRange != null) {
                    poster = _htmlParser.GetInnerHTML(idSpanTagRange).Replace("ID:", "");
                }
                else if (authorSpanTagRange != null) {
                    string name = _htmlParser.GetInnerHTML(authorSpanTagRange);
                    if (tripcodeSpanTagRange != null && !String.IsNullOrEmpty(_htmlParser.GetInnerHTML(tripcodeSpanTagRange))) {
                        poster = name + _htmlParser.GetInnerHTML(tripcodeSpanTagRange);
                    }
                    else if (name != "Anonymous") {
                        poster = name;
                    }
                }
                
                ImageInfo image = new ImageInfo {
                    URL = General.GetAbsoluteURL(_url, HttpUtility.HtmlDecode(imageURL)),
                    Referer = _url,
                    OriginalFileName = General.CleanFileName(HttpUtility.HtmlDecode(originalFileName)),
                    HashType = HashType.MD5,
                    Hash = General.TryBase64Decode(imageMD5),
                    Poster = General.CleanFileName(HttpUtility.HtmlDecode(poster))
                };
                if (image.URL.Length == 0 || image.FileName.Length == 0 || image.Hash == null) continue;

                ThumbnailInfo thumb = new ThumbnailInfo {
                    URL = General.GetAbsoluteURL(_url, HttpUtility.HtmlDecode(thumbURL)),
                    Referer = _url
                };
                if (thumb.URL == null || thumb.FileName.Length == 0) continue;

                if (replaceList != null) {
                    HTMLAttribute attribute;

                    attribute = imageLinkTagRange.StartTag.GetAttribute("href");
                    if (attribute != null) {
                        replaceList.Add(
                            new ReplaceInfo {
                                Offset = attribute.Offset,
                                Length = attribute.Length,
                                Type = ReplaceType.ImageLinkHref,
                                Tag = image.FileName
                            });
                    }

                    if (fileNameLinkTagRange != null) {
                        attribute = fileNameLinkTagRange.StartTag.GetAttribute("href");
                        if (attribute != null) {
                            replaceList.Add(
                                new ReplaceInfo {
                                    Offset = attribute.Offset,
                                    Length = attribute.Length,
                                    Type = ReplaceType.ImageLinkHref,
                                    Tag = image.FileName
                                });
                        }
                    }

                    attribute = thumbImageTag.GetAttribute("src");
                    if (attribute != null) {
                        replaceList.Add(
                            new ReplaceInfo {
                                Offset = attribute.Offset,
                                Length = attribute.Length,
                                Type = ReplaceType.ImageSrc,
                                Tag = thumb.FileName
                            });
                    }
                }

                imageList.Add(image);
                thumbnailList.Add(thumb);
            }

            return imageList;
        }
    }

    public class SiteHelper_archive_moe : FoolFuukaSiteHelper { }
    
    public class SiteHelper_4plebs_org : FoolFuukaSiteHelper { }
    
    public class SiteHelper_archive_nyafuu_org : FoolFuukaSiteHelper { }

    public class SiteHelper_archive_loveisover_me : FoolFuukaSiteHelper { }

    public class SiteHelper_fgts_jp : FoolFuukaSiteHelper { }

    public class SiteHelper_imcute_yt : FoolFuukaSiteHelper { }
}