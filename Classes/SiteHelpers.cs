using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
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
            int pos = _url.IndexOf("://", StringComparison.Ordinal);
            if (pos == -1) return new string[0];
            return _url.Substring(pos + 3).Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
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

        public virtual string GetThreadID() {
            return GetThreadName();
        }

        public virtual string GetPageID() {
            return String.Join("/", new[] { GetSiteName(), GetBoardName(), GetThreadID() });
        }

        public virtual bool HasSlug() {
            return false;
        }

        public virtual bool IsBoardHighTurnover() {
            return false;
        }

        protected virtual string ImageURLKeyword {
            get { return "/src/"; }
        }

        public virtual List<ImageInfo> GetImages(List<ReplaceInfo> replaceList, List<ThumbnailInfo> thumbnailList) {
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
                if (url == null || url.IndexOf(ImageURLKeyword, StringComparison.OrdinalIgnoreCase) == -1) continue;

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

        public virtual void ResurrectDeadPosts(HTMLParser previousParser) {
        }

        public virtual string GetNextPageURL() {
            return null;
        }
    }

    public class SiteHelper_4chan_org : SiteHelper {
        public override string GetThreadName() {
            string[] urlSplit = SplitURL();
            if (HasSlug()) {
                if (Settings.UseSlug == true) {
                    switch (Settings.SlugType) {
                        case SlugType.First:
                            return urlSplit[urlSplit.Length - 1] + "_" + urlSplit[urlSplit.Length - 2];
                        case SlugType.Last:
                            return urlSplit[urlSplit.Length - 2] + "_" + urlSplit[urlSplit.Length - 1];
                        case SlugType.Only:
                            return urlSplit[urlSplit.Length - 1];
                    }
                }
                return urlSplit[urlSplit.Length - 2];
            }
            return base.GetThreadName();
        }

        public override string GetThreadID() {
            string[] urlSplit = SplitURL();
            return HasSlug() ? urlSplit[urlSplit.Length - 2] : base.GetThreadID();
        }

        public override bool HasSlug() {
            return _url.IndexOf(GetBoardName() + "/thread/", StringComparison.Ordinal) > -1 && SplitURL().Length == 5;
        }

        public override List<ImageInfo> GetImages(List<ReplaceInfo> replaceList, List<ThumbnailInfo> thumbnailList) {
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
                if (imageURL == null || !imageURL.StartsWith("//i.4cdn.org/")) continue;

                string thumbURL = fileThumbImageTag.GetAttributeValue("src");
                if (thumbURL == null) continue;

                bool isSpoiler = HTMLParser.ClassAttributeValueHas(fileThumbLinkTagRange.StartTag, "imgspoiler");

                string originalFileName = null;
                if (!isSpoiler) {
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
                    URL = "http:" + HttpUtility.HtmlDecode(imageURL),
                    Referer = _url,
                    OriginalFileName = General.CleanFileName(HttpUtility.HtmlDecode(originalFileName) ?? ""),
                    HashType = HashType.MD5,
                    Hash = General.TryBase64Decode(imageMD5),
                    Poster = General.CleanFileName(poster)
                };
                if (image.URL.Length == 0 || image.FileName.Length == 0 || image.Hash == null) continue;

                ThumbnailInfo thumb = new ThumbnailInfo {
                    URL = "http:" + HttpUtility.HtmlDecode(thumbURL),
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
                    if (!href.StartsWith("/") || !href.Contains("/thread/") || (!interBoardAutoFollow && GetBoardName() != href.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)[0])) continue;
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

        public override void ResurrectDeadPosts(HTMLParser previousParser) {
            if (previousParser == null) return;
            List<ReplaceInfo> replaceList = new List<ReplaceInfo>();
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
                    replaceList.Add(
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
                General.WriteReplacedString(_htmlParser.PreprocessedHTML, replaceList, sw);
            }
            _htmlParser = new HTMLParser(sb.ToString());

            replaceList.Clear();
            foreach (HTMLTagRange deadLinkTagRange in Enumerable.Where(Enumerable.Select(Enumerable.Where(_htmlParser.FindStartTags("span"),
                    t => HTMLParser.ClassAttributeValueHas(t, "deadlink")), t => _htmlParser.CreateTagRange(t)), r => r != null))
            {
                string deadLinkInnerHTML = HttpUtility.HtmlDecode(_htmlParser.GetInnerHTML(deadLinkTagRange));
                if (deadLinkInnerHTML.Contains(">>>")) continue;
                string[] deadLinkSplit = deadLinkInnerHTML.Split('/');
                if (deadLinkSplit.Length < 1) continue;
                string deadLinkID = deadLinkSplit[deadLinkSplit.Length - 1];
                if (resurrectedPostContainers.ContainsKey("pc" + deadLinkID)) {
                    replaceList.Add(
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
                General.WriteReplacedString(_htmlParser.PreprocessedHTML, replaceList, sw);
            }
            _htmlParser = new HTMLParser(sb.ToString());
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
}