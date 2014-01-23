using System;
using System.Collections.Generic;

namespace JDP {
	public class HTMLParser {
		private string _preprocessedHTML;
		private List<HTMLTag> _tags;
		private Dictionary<int, int> _offsetToIndex = new Dictionary<int, int>();

		public HTMLParser(string html) {
			_preprocessedHTML = Preprocess(html);
			_tags = new List<HTMLTag>(ParseTags(_preprocessedHTML, 0, _preprocessedHTML.Length));
			for (int i = 0; i < _tags.Count; i++) {
				_offsetToIndex.Add(_tags[i].Offset, i);
			}
		}

		public string PreprocessedHTML {
			get { return _preprocessedHTML; }
		}

		public IList<HTMLTag> Tags {
			get { return _tags.AsReadOnly(); }
		}

		public string GetInnerHTML(HTMLTag startTag, HTMLTag endTag) {
			return startTag.IsSelfClosing ? String.Empty : GetSection(_preprocessedHTML, startTag.EndOffset, endTag.Offset);
		}

		public string GetInnerHTML(HTMLTagRange tagRange) {
			return GetInnerHTML(tagRange.StartTag, tagRange.EndTag);
		}

		public IEnumerable<HTMLTag> EnumerateTags(HTMLTag startAfterTag, HTMLTag stopBeforeTag) {
			int startIndex = startAfterTag != null ? (GetTagIndex(startAfterTag) + 1) : 0;
			int stopIndex = stopBeforeTag != null ? (GetTagIndex(stopBeforeTag) - 1) : (_tags.Count - 1);
			for (int i = startIndex; i <= stopIndex; i++) {
				yield return _tags[i];
			}
		}

		public IEnumerable<HTMLTag> EnumerateTags(HTMLTagRange containingTagRange) {
			return EnumerateTags(containingTagRange.StartTag, containingTagRange.EndTag);
		}

		public IEnumerable<HTMLTag> FindTags(bool isEndTag, HTMLTag startAfterTag, HTMLTag stopBeforeTag, params string[] names) {
			foreach (HTMLTag tag in EnumerateTags(startAfterTag, stopBeforeTag)) {
				if (tag.IsEnd == isEndTag && tag.NameEqualsAny(names)) {
					yield return tag;
				}
			}
		}

		public IEnumerable<HTMLTag> FindTags(bool isEndTag, HTMLTagRange containingTagRange, params string[] names) {
			return FindTags(isEndTag, containingTagRange.StartTag, containingTagRange.EndTag, names);
		}

		public HTMLTag FindTag(bool isEndTag, HTMLTag startAfterTag, HTMLTag stopBeforeTag, params string[] names) {
			foreach (HTMLTag tag in FindTags(isEndTag, startAfterTag, stopBeforeTag, names)) {
				return tag;
			}
			return null;
		}

		public HTMLTag FindTag(bool isEndTag, HTMLTagRange containingTagRange, params string[] names) {
			return FindTag(isEndTag, containingTagRange.StartTag, containingTagRange.EndTag, names);
		}

		public IEnumerable<HTMLTag> FindStartTags(HTMLTag startAfterTag, HTMLTag stopBeforeTag, params string[] names) {
			return FindTags(false, startAfterTag, stopBeforeTag, names);
		}

		public IEnumerable<HTMLTag> FindStartTags(HTMLTagRange containingTagRange, params string[] names) {
			return FindStartTags(containingTagRange.StartTag, containingTagRange.EndTag, names);
		}

		public IEnumerable<HTMLTag> FindStartTags(params string[] names) {
			return FindTags(false, null, null, names);
		}

		public HTMLTag FindStartTag(HTMLTag startAfterTag, HTMLTag stopBeforeTag, params string[] names) {
			return FindTag(false, startAfterTag, stopBeforeTag, names);
		}

		public HTMLTag FindStartTag(HTMLTagRange containingTagRange, params string[] names) {
			return FindStartTag(containingTagRange.StartTag, containingTagRange.EndTag, names);
		}

		public HTMLTag FindStartTag(params string[] names) {
			return FindTag(false, null, null, names);
		}

		public IEnumerable<HTMLTag> FindEndTags(HTMLTag startAfterTag, HTMLTag stopBeforeTag, params string[] names) {
			return FindTags(true, startAfterTag, stopBeforeTag, names);
		}

		public IEnumerable<HTMLTag> FindEndTags(HTMLTagRange containingTagRange, params string[] names) {
			return FindEndTags(containingTagRange.StartTag, containingTagRange.EndTag, names);
		}

		public IEnumerable<HTMLTag> FindEndTags(params string[] names) {
			return FindTags(true, null, null, names);
		}

		public HTMLTag FindEndTag(HTMLTag startAfterTag, HTMLTag stopBeforeTag, params string[] names) {
			return FindTag(true, startAfterTag, stopBeforeTag, names);
		}

		public HTMLTag FindEndTag(HTMLTagRange containingTagRange, params string[] names) {
			return FindEndTag(containingTagRange.StartTag, containingTagRange.EndTag, names);
		}

		public HTMLTag FindEndTag(params string[] names) {
			return FindTag(true, null, null, names);
		}

		public HTMLTag FindCorrespondingEndTag(HTMLTag tag) {
			return FindCorrespondingEndTag(tag, null);
		}

		public HTMLTag FindCorrespondingEndTag(HTMLTag tag, HTMLTag stopBeforeTag) {
			if (tag == null) {
				return null;
			}
			if (tag.IsEnd) {
				throw new ArgumentException("Tag must be a start tag.");
			}
			if (tag.IsSelfClosing) {
				return tag;
			}
			int startIndex = GetTagIndex(tag) + 1;
			int stopIndex = stopBeforeTag != null ? (GetTagIndex(stopBeforeTag) - 1) : (_tags.Count - 1);
			int depth = 1;
			for (int i = startIndex; i <= stopIndex; i++) {
				HTMLTag tag2 = _tags[i];
				if (!tag2.IsSelfClosing && tag2.NameEquals(tag.Name)) {
					depth += tag2.IsEnd ? -1 : 1;
					if (depth == 0) {
						return tag2;
					}
				}
			}
			return null;
		}

		public HTMLTagRange CreateTagRange(HTMLTag tag) {
			return CreateTagRange(tag, null);
		}

		public HTMLTagRange CreateTagRange(HTMLTag tag, HTMLTag stopBeforeTag) {
			HTMLTag endTag = FindCorrespondingEndTag(tag, stopBeforeTag);
			return (tag != null && endTag != null) ? new HTMLTagRange(tag, endTag) : null;
		}

		private int GetTagIndex(HTMLTag tag) {
			int i;
			if (!_offsetToIndex.TryGetValue(tag.Offset, out i)) {
				throw new Exception("Unable to locate the specified tag.");
			}
			return i;
		}

		private static string Preprocess(string html) {
			if (html.IndexOf('\r') == -1) {
				// No preprocessing needed
				return html;
			}
			char[] dst = new char[html.Length];
			int iDst = 0;
			for (int iSrc = 0; iSrc < html.Length; iSrc++) {
				char c = html[iSrc];
				if (c == '\n' && iSrc >= 1 && html[iSrc - 1] == '\r') {
					// Skip line feed following carriage return
					continue;
				}
				if (c == '\r') {
					// Convert carriage return to line feed
					c = '\n';
				}
				dst[iDst++] = c;
			}
			return new string(dst, 0, iDst);
		}

		private static IEnumerable<HTMLTag> ParseTags(string html, int htmlStart, int htmlEnd) {
			while (htmlStart < htmlEnd) {
				int pos = IndexOf(html, htmlStart, htmlEnd, '<');
				if (pos == -1) yield break;

				HTMLTag tag = new HTMLTag();
				tag.Offset = pos;
				htmlStart = pos + 1;
				tag.IsEnd = StartsWith(html, htmlStart, htmlEnd, '/');
				if (StartsWithLetter(html, tag.IsEnd ? (htmlStart + 1) : htmlStart, htmlEnd)) {
					// Parse tag name
					if (tag.IsEnd) htmlStart += 1;
					pos = IndexOfAny(html, htmlStart, htmlEnd, true, '/', '>');
					if (pos == -1) yield break;
					tag.Name = GetSectionLower(html, htmlStart, pos);
					htmlStart = pos;

					// Parse attributes
					bool isTagComplete = false;
					do {
						while (StartsWithWhiteSpace(html, htmlStart, htmlEnd)) htmlStart++;
						tag.IsSelfClosing = StartsWith(html, htmlStart, htmlEnd, '/');
						if (tag.IsSelfClosing) htmlStart += 1;
						if (StartsWith(html, htmlStart, htmlEnd, '>')) {
							htmlStart += 1;
							isTagComplete = true;
						}
						else if (tag.IsSelfClosing) { }
						else {
							HTMLAttribute attribute = new HTMLAttribute();
							attribute.Offset = htmlStart;

							// Parse attribute name
							pos = IndexOfAny(html, htmlStart + 1, htmlEnd, true, '=', '/', '>');
							if (pos == -1) yield break;
							attribute.Name = GetSectionLower(html, htmlStart, pos);
							htmlStart = pos;

							while (StartsWithWhiteSpace(html, htmlStart, htmlEnd)) htmlStart++;
							if (StartsWith(html, htmlStart, htmlEnd, '=')) {
								// Parse attribute value
								htmlStart += 1;
								while (StartsWithWhiteSpace(html, htmlStart, htmlEnd)) htmlStart++;
								if (StartsWithAny(html, htmlStart, htmlEnd, '"', '\'')) {
									char quoteChar = html[htmlStart];
									htmlStart += 1;
									pos = IndexOf(html, htmlStart, htmlEnd, quoteChar);
									if (pos == -1) yield break;
									attribute.Value = GetSection(html, htmlStart, pos);
									htmlStart = pos + 1;
								}
								else {
									pos = IndexOfAny(html, htmlStart, htmlEnd, true, '>');
									if (pos == -1) yield break;
									attribute.Value = GetSection(html, htmlStart, pos);
									htmlStart = pos;
								}
							}
							else {
								attribute.Value = String.Empty;
							}

							attribute.Length = htmlStart - attribute.Offset;
							if (tag.GetAttribute(attribute.Name) == null) {
								tag.Attributes.Add(attribute);
							}
						}
					}
					while (!isTagComplete);
					tag.Length = htmlStart - tag.Offset;

					// Yield result
					yield return tag;

					// Skip contents of special tags whose contents are to be treated as raw text
					if (!tag.IsEnd && !tag.IsSelfClosing && tag.NameEqualsAny("script", "style", "title", "textarea")) {
						bool foundEndTag = false;
						do {
							pos = IndexOf(html, htmlStart, htmlEnd, '<');
							if (pos == -1) yield break;
							htmlStart = pos + 1;
							string endTagText = "/" + tag.Name;
							if (StartsWith(html, htmlStart, htmlEnd, endTagText, true) &&
								(StartsWithWhiteSpace(html, htmlStart + endTagText.Length, htmlEnd) ||
								 StartsWithAny(html, htmlStart + endTagText.Length, htmlEnd, '/', '>')))
							{
								htmlStart -= 1;
								foundEndTag = true;
							}
						}
						while (!foundEndTag);
					}
				}
				else if (StartsWith(html, htmlStart, htmlEnd, "!--", false) && !StartsWith(html, htmlStart + 3, htmlEnd, '>')) {
					// Skip comment
					htmlStart += 3;
					bool foundEnd = false;
					do {
						pos = IndexOf(html, htmlStart, htmlEnd, '-');
						if (pos == -1) yield break;
						htmlStart = pos + 1;
						if (StartsWith(html, htmlStart, htmlEnd, "->", false)) {
							htmlStart += 2;
							foundEnd = true;
						}
						else if (StartsWith(html, htmlStart, htmlEnd, "-!>", false)) {
							htmlStart += 3;
							foundEnd = true;
						}
					}
					while (!foundEnd);
				}
				else if (StartsWithAny(html, htmlStart, htmlEnd, '?', '/', '!')) {
					// Skip bogus comment or DOCTYPE
					htmlStart += 1;
					pos = IndexOf(html, htmlStart, htmlEnd, '>');
					if (pos == -1) yield break;
					htmlStart = pos + 1;
				}
			}
		}

		private static int IndexOf(string html, int htmlStart, int htmlEnd, char value) {
			while (htmlStart < htmlEnd) {
				if (html[htmlStart] == value) {
					return htmlStart;
				}
				htmlStart++;
			}
			return -1;
		}

		private static int IndexOfAny(string html, int htmlStart, int htmlEnd, bool findWhiteSpace, params char[] values) {
			while (htmlStart < htmlEnd) {
				char c = html[htmlStart];
				if (findWhiteSpace && CharIsWhiteSpace(c)) {
					return htmlStart;
				}
				foreach (char v in values) {
					if (c == v) {
						return htmlStart;
					}
				}
				htmlStart++;
			}
			return -1;
		}

		private static bool StartsWith(string html, int htmlStart, int htmlEnd, char value) {
			if (htmlStart >= htmlEnd) return false;
			return html[htmlStart] == value;
		}

		private static bool StartsWith(string html, int htmlStart, int htmlEnd, string value, bool ignoreCase) {
			if (htmlStart + (value.Length - 1) >= htmlEnd) return false;
			for (int i = 0; i < value.Length; i++) {
				char c = html[htmlStart + i];
				char v = value[i];
				if (ignoreCase) {
					c = CharToLower(c);
					v = CharToLower(v);
				}
				if (c != v) return false;
			}
			return true;
		}

		private static bool StartsWithAny(string html, int htmlStart, int htmlEnd, params char[] values) {
			if (htmlStart >= htmlEnd) return false;
			char c = html[htmlStart];
			foreach (char v in values) {
				if (c == v) return true;
			}
			return false;
		}

		private static bool StartsWithWhiteSpace(string html, int htmlStart, int htmlEnd) {
			if (htmlStart >= htmlEnd) return false;
			char c = html[htmlStart];
			return CharIsWhiteSpace(c);
		}

		private static bool StartsWithLetter(string html, int htmlStart, int htmlEnd) {
			if (htmlStart >= htmlEnd) return false;
			return CharIsLetter(html[htmlStart]);
		}

		private static string GetSectionLower(string html, int htmlStart, int htmlEnd) {
			char[] dst = new char[htmlEnd - htmlStart];
			for (int i = 0; i < dst.Length; i++) {
				dst[i] = CharToLower(html[htmlStart + i]);
			}
			return new string(dst);
		}

		private static string GetSection(string html, int htmlStart, int htmlEnd) {
			return html.Substring(htmlStart, htmlEnd - htmlStart);
		}

		private static bool CharIsLetter(char c) {
			return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
		}

		private static bool CharIsWhiteSpace(char c) {
			return c == ' ' || c == '\t' || c == '\f' || c == '\n';
		}

		private static char CharToLower(char c) {
			return (c >= 'A' && c <= 'Z') ? (char)(c + ('a' - 'A')) : c;
		}

		public static char[] GetWhiteSpaceChars() {
			return new char[] { ' ', '\t', '\f', '\n' };
		}

		public static bool ClassAttributeValueHas(string attributeValue, string targetClassName) {
			string[] assignedClassNames = attributeValue.Split(GetWhiteSpaceChars(), StringSplitOptions.RemoveEmptyEntries);
			return Array.Exists(assignedClassNames, n => n.Equals(targetClassName, StringComparison.Ordinal));
		}

		public static bool ClassAttributeValueHas(HTMLTag tag, string targetClassName) {
			string attributeValue = tag.GetAttributeValue("class");
			return attributeValue != null && ClassAttributeValueHas(attributeValue, targetClassName);
		}
	}

	public class HTMLTag {
		public string Name { get; set; }
		public bool IsEnd { get; set; }
		public bool IsSelfClosing { get; set; }
		public List<HTMLAttribute> Attributes { get; set; }
		public int Offset { get; set; }
		public int Length { get; set; }

		public HTMLTag() {
			Attributes  = new List<HTMLAttribute>();
		}

		public int EndOffset {
			get {
				return Offset + Length;
			}
		}

		public bool NameEquals(string name) {
			return Name.Equals(name, StringComparison.OrdinalIgnoreCase);
		}

		public bool NameEqualsAny(params string[] names) {
			foreach (string name in names) {
				if (Name.Equals(name, StringComparison.OrdinalIgnoreCase)) {
					return true;
				}
			}
			return false;
		}

		public HTMLAttribute GetAttribute(string name) {
			foreach (HTMLAttribute attribute in Attributes) {
				if (attribute.NameEquals(name)) {
					return attribute;
				}
			}
			return null;
		}

		public string GetAttributeValue(string attributeName) {
			HTMLAttribute attribute = GetAttribute(attributeName);
			return attribute != null ? attribute.Value : null;
		}

		public string GetAttributeValueOrEmpty(string attributeName) {
			return GetAttributeValue(attributeName) ?? String.Empty;
		}
	}

	public class HTMLAttribute {
		public string Name { get; set; }
		public string Value { get; set; }
		public int Offset { get; set; }
		public int Length { get; set; }

		public bool NameEquals(string name) {
			return Name.Equals(name, StringComparison.OrdinalIgnoreCase);
		}
	}

	public class HTMLTagRange {
		public HTMLTag StartTag { get; set; }
		public HTMLTag EndTag { get; set; }

		public HTMLTagRange(HTMLTag startTag, HTMLTag endTag) {
			StartTag = startTag;
			EndTag = endTag;
		}
	}
}
