using System.Text.RegularExpressions;

namespace TiddlyConverter.Types
{
    /// <summary>
    /// JSON Representation of a Tiddler after Export
    /// </summary>
    public class Tiddler
    {
        /// <summary>
        /// Text content of Tiddler
        /// </summary>
        public string Text;
        /// <summary>
        /// Title of Tiddler
        /// </summary>
        public string Title;
        /// <summary>
        /// Tags of Tiddler; Will lost tag color and shape information (but available separately as Icon and Color); Space delimited
        /// </summary>
        public string Tags;
        /// <summary>
        /// Modified Date of Tiddler
        /// </summary>
        public string Modified;
        /// <summary>
        /// Created date of Tiddler
        /// </summary>
        public string Created;
        /// <summary>
        /// Type of Tiddler; Default empty; For markdown extension, it's: text/x-markdown
        /// </summary>
        public string Type;
        /// <summary>
        /// THis is the arbitrary metadata we can assign to Tiddlers; Empty for all of the tiddlers we have
        /// </summary>
        public string List;

        /// <summary>
        /// Color of Tiddler/Tag
        /// </summary>
        public string Icon;
        /// <summary>
        /// Color of Tiddler/Tag
        /// </summary>
        public string Color;

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
        /// Split tags
        /// </summary>
        public static string[] ParseTags(string tiddlyWikiTags)
        {
            if (string.IsNullOrWhiteSpace(tiddlyWikiTags))
                return Array.Empty<string>();
            string escaped = Regex.Replace(tiddlyWikiTags, @"\[\[(.*?)\]\]", "\"$1\"");
            return ParseCommandLineArguments(escaped);
        }
        /// <summary>
        /// Split string as CLI argument style
        /// </summary>
        public static string[] ParseCommandLineArguments(string commandLineString)
        {
            return Csv.CsvReader.ReadFromText($"{commandLineString}", new Csv.CsvOptions()
            {
                HeaderMode = Csv.HeaderMode.HeaderAbsent,
                Separator = ' ',
                SkipRow = (row, index) => false, // Disable skipping "comments" for some of the items might start with a `#` as tag name
            }).First().Values;
        }
        #endregion
    }
}
