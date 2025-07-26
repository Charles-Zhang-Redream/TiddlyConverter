using System;
using System.Collections.Generic;
using System.Text;

namespace TiddlyConverter.Types
{
    /// <summary>
    /// JSON Representation of a Tiddler after Export
    /// </summary>
    public class Tiddler
    {
        #region Properties
        /// <summary>
        /// Text content of Tiddler
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// Title of Tiddler
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Tags of Tiddler; Will lost tag color and shape information (but available separately as Icon and Color); Space delimited
        /// </summary>
        public string Tags { get; set; }
        /// <summary>
        /// Modified Date of Tiddler
        /// </summary>
        public string Modified { get; set; }
        /// <summary>
        /// Created date of Tiddler
        /// </summary>
        public string Created { get; set; }
        /// <summary>
        /// Type of Tiddler; Default empty; For markdown extension, it's: text/x-markdown
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// THis is the arbitrary metadata we can assign to Tiddlers; Empty for all of the tiddlers we have
        /// </summary>
        public string List { get; set; }

        /// <summary>
        /// Color of Tiddler/Tag
        /// </summary>
        public string Icon { get; set; }
        /// <summary>
        /// Color of Tiddler/Tag
        /// </summary>
        public string Color { get; set; }
        #endregion

        #region Accessors
        /// <summary>
        /// Strongly typed creation date
        /// </summary>
        public DateTime CreatedDate
            => DateTime.ParseExact(Created[..8], "yyyyMMdd", null);
        /// <summary>
        /// Strongly typed modified date
        /// </summary>
        public DateTime ModifiedDate
            => DateTime.ParseExact(Modified[..8], "yyyyMMdd", null);
        /// <summary>
        /// Array formatted tags
        /// </summary>
        public string[] TagsArray
            => ParseTags(Tags);
        #endregion

        #region Helpers
        /// <summary>
        /// Split TiddlyWiki‑style tags into individual tag strings.
        /// [[…]] sequences become one tag (without the brackets), 
        /// everything else is split on whitespace.
        /// </summary>
        /// <remarks>
        /// In Tiddly Wiki export, the pattern is that if there is space then they use `[[]]`, otherwise tags are seperated by space.
        /// E.g. 
        /// [[Rank - Affection]] ~阿May 夢のノート
        /// [[Dream Observation]]
        /// ~原生人物 [[Rank - Weird]] 夢のノート Rank
        /// </remarks>
        public static string[] ParseTags(string tiddlyWikiTags)
        {
            if (string.IsNullOrWhiteSpace(tiddlyWikiTags))
                return [];

            List<string> tags = [];
            string s = tiddlyWikiTags;
            int len = s.Length;
            int i = 0;

            while (i < len)
            {
                // Skip any leading whitespace
                while (i < len && char.IsWhiteSpace(s[i]))
                    i++;
                if (i >= len)
                    break;

                if (i + 1 < len && s[i] == '[' && s[i + 1] == '[')
                {
                    // Found a [[…]] tag
                    i += 2;  // skip "[["
                    StringBuilder sb = new StringBuilder();
                    // read until closing "]]" or end of string
                    while (i + 1 < len && !(s[i] == ']' && s[i + 1] == ']'))
                    {
                        sb.Append(s[i]);
                        i++;
                    }
                    tags.Add(sb.ToString());
                    // skip the closing "]]" (if present)
                    if (i + 1 < len)
                        i += 2;
                    else
                        i = len;
                }
                else
                {
                    // ordinary tag: read until next whitespace
                    StringBuilder sb = new StringBuilder();
                    while (i < len && !char.IsWhiteSpace(s[i]))
                    {
                        sb.Append(s[i]);
                        i++;
                    }
                    tags.Add(sb.ToString());
                }
            }

            return tags.ToArray();
        }
        #endregion
    }
}
