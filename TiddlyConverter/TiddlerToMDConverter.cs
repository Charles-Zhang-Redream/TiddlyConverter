using System.Drawing;
using System.Text.RegularExpressions;
using System.Text;
using TiddlyConverter.Types;
using Console = Colorful.Console;
using System.Collections.Generic;
using System.Linq;

namespace TiddlyConverter
{
    /// <summary>
    /// Provides routines for converting tiddlers to MD
    /// </summary>
    public sealed partial class TiddlerToMDConverter
    {
        #region Properties
        /// <summary>
        /// Original tiddlers
        /// </summary>
        public Tiddler[] Tiddlers { get; }
        #endregion

        #region Statistics
        /// <summary>
        /// Unique tags
        /// </summary>
        public HashSet<string> UniqueTags { get; private set; }
        /// <summary>
        /// Number of tiddlers that will be converted
        /// </summary>
        public int UsefulTiddlers { get; private set; }
        #endregion

        #region Construction
        /// <summary>
        /// Initialize converter with a set of tiddlers
        /// </summary>
        public TiddlerToMDConverter(Tiddler[] tiddlers)
        {
            Tiddlers = tiddlers;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Convert existing tiddlers to correponding markdown documents
        /// </summary>
        public MarkdownDocument[] Convert(ProgramOptions options)
        {
            // Filtering
            Tiddler[] filtered = Tiddlers;
            if (!options.KeepDrafts)
            {
                foreach (Tiddler item in Tiddlers.Where(t => DraftTiddlerTitleRegex().IsMatch(t.Title)))
                    Console.WriteLine($"Ignored {item.Title}: {(string.IsNullOrEmpty(item.Text) ? "(Empty)" : item.Text)}", Color.Goldenrod);
                filtered = [.. Tiddlers.Where(t => !DraftTiddlerTitleRegex().IsMatch(t.Title))];
            }

            // Statistics
            UsefulTiddlers = filtered.Length;
            UniqueTags = [.. filtered.SelectMany(t => t.TagsArray)];

            return [.. filtered.Select(f => new MarkdownDocument()
            {
                Title = f.Title,
                Content = FormatTiddlyWikiTextToMD(f.Type, f.Text, filtered, options),
                CreateDate = f.CreatedDate,
                ModificationDate = f.ModifiedDate,
                Tags = f.TagsArray
            })];
        }
        #endregion

        #region Routines
        /// <summary>
        /// Converts Tiddly Wiki formatted content to proper MD content
        /// </summary>
        public static string FormatTiddlyWikiTextToMD(string type, string text, Tiddler[] catalog, ProgramOptions options)
        {
            string markdownExtensionType = "text/x-markdown";
            if (type == markdownExtensionType)
                return text;

            // Deal with Tiddly Wiki syntax
            
            // Replace bolds
            text = Regex.Replace(text, "''(.*?)''", "**$1**");
            
            // Replace italics
            text = Regex.Replace(text, "//(.*?)//", "*$1*");
            
            // Replace enumerations
            text = Regex.Replace(text, "^(#) (.*)$", "1. $2", RegexOptions.Multiline);
            text = Regex.Replace(text, @"^(#)(\*+) (.*)$", m =>
            {
                string levels = m.Groups[2].Value;
                string item = m.Groups[3].Value;
                return $"{new string('\t', levels.Length)}* {item}";
            }, RegexOptions.Multiline);
            text = Regex.Replace(text, @"^(\*)(\*+) (.*)$", m =>
            {
                string levels = m.Groups[2].Value;
                string item = m.Groups[3].Value;
                return $"{new string('\t', levels.Length)}* {item}";
            }, RegexOptions.Multiline);

            // Replcae troublesome texts
            text = Regex.Replace(text, "~~~~", "~~ ~~");

            // Replace quotes
            text = Regex.Replace(text, "<<<(.*?)<<<", m =>
            {
                string content = m.Groups[1].Value.Trim();
                if (content.StartsWith(".tc-big-quote"))
                    content = content[".tc-big-quote".Length..].Trim();
                return Regex.Replace(content, $"^(.*)$", "> $1", RegexOptions.Multiline);
            }, RegexOptions.Singleline);
            
            // Replace headers
            text = Regex.Replace(text, "^(!+) (.*)$", m =>
            {
                string formatter = m.Groups[1].Value.Replace("!", "#");
                string header = m.Groups[2].Value;
                return $"#{formatter} {header}\n";
            }, RegexOptions.Multiline);
            
            // Redundant header emphasis
            text = Regex.Replace(text, @"^(#+) \*\*(.*)\*\*$", "$1 $2", RegexOptions.Multiline);
            
            // Replace table of contents
            text = Regex.Replace(text, "<div class=\"tc-table-of-contents\">(.*?)</div>", m =>
            {
                string content = m.Groups[1].Value.Trim();
                string key = Regex.Match(content, "<<toc-selective-expandable '(.*?)'>>").Groups[1].Value;
                IEnumerable<Tiddler> items = catalog.Where(c => c.TagsArray.Contains(key));
                StringBuilder toc = new();
                foreach (var item in items)
                    toc.AppendLine($"* {item.Title}");
                return $"""
                    TABLE OF CONTENTS ({key}):

                    {toc}
                    """;
            }, RegexOptions.Singleline);
            
            // Replace links
            text = Regex.Replace(text, @"\[\[(.*?)\]\]", m =>
            {
                string linkedTitle = m.Groups[1].Value;
                Tiddler match = catalog.SingleOrDefault(c => c.Title == linkedTitle);
                if (options.HighlightLinks)
                {
                    if (match == null)
                        return $"[{linkedTitle}](EMPTY REFERENCE)";
                    else return $"[{linkedTitle}](./{linkedTitle})";
                }
                else return linkedTitle;
            });

            // Replace transclusions
            text = Regex.Replace(text, @"{{(.*?)}}", m =>
            {
                string trans = m.Groups[1].Value;
                Tiddler match = catalog.SingleOrDefault(c => c.Title == trans);
                if (match == null)
                    return $"{{{{{trans} (EMPTY REFERENCE)}}}}";
                else return $"{{{{{trans}}}}}";
            });
            
            return text.Trim();
        }
        #endregion

        #region Regular Expressions
        [GeneratedRegex(@"Draft \d* ?of '.*?'")]
        private static partial Regex DraftTiddlerTitleRegex();
        #endregion
    }
}
