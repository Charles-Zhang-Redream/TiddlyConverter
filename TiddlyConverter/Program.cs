using Newtonsoft.Json;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using Console = Colorful.Console;

namespace TiddlyConverter
{
    public class Tiddler
    {
        public string Text;
        public string Title;
        public string Tags;
        public string Modified;
        public string Created;

        public string Icon;
        public string Color;

        public DateTime CreatedDate => DateTime.ParseExact(Created[..8], "yyyyMMdd", null);
        public DateTime ModifiedDate => DateTime.ParseExact(Modified[..8], "yyyyMMdd", null);
    }
    internal static class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine();
                return;
            }
            string jsonFile = args[0];
            string outputFile = args[1];

            Tiddler[] wiki = JsonConvert.DeserializeObject<Tiddler[]>(File.ReadAllText(jsonFile));
            // Filtering
            foreach (var item in wiki.Where(t => Regex.IsMatch(t.Title, "Draft of '.*?'")))
                Console.WriteLine($"Ignored {item.Title}: {(string.IsNullOrEmpty(item.Text) ? "(Empty)" : item.Text)}", Color.Goldenrod);
            var filtered = wiki.Where(t => !Regex.IsMatch(t.Title, "Draft of '.*?'")).ToArray();
            // Summary
            Dictionary<string, string[]> tiddlerTags = filtered.ToDictionary(f => f.Title, f => ParseTags(f.Tags));
            HashSet<string> tags = new HashSet<string>(tiddlerTags.SelectMany(ts => ts.Value));

            StringBuilder builder = new StringBuilder();
            // Statistics
            builder.AppendLine("Number of tiddlers: " + filtered.Length + "  ");
            builder.AppendLine("Number of tags: " + tags.Count + "  ");
            builder.AppendLine("Tags: " + string.Join(", ", tags.OrderBy(t => t).Select(t =>
            {
                int count = tiddlerTags.Where(ts => ts.Value.Contains(t)).Count();
                return $"{t} ({count})";
            })) + "  ");
            builder.AppendLine();
            // Enumerate tiddlers
            foreach (Tiddler tiddler in filtered.OrderBy(t => t.CreatedDate).ToArray())
            {
                builder.AppendLine($"# {tiddler.Title}");
                builder.AppendLine();
                if (!string.IsNullOrWhiteSpace(tiddler.Tags))
                    builder.AppendLine($"Tags: {FormatTiddlyWikiTags(tiddler.Tags)}");
                builder.AppendLine();
                builder.AppendLine(FormatTiddlyWikiToMD(tiddler.Text, filtered));
                builder.AppendLine();
            }
            File.WriteAllText(outputFile, builder.ToString());
        }

        static string[] ParseTags(string tiddlyWikiTags)
        {
            if (string.IsNullOrWhiteSpace(tiddlyWikiTags))
                return Array.Empty<string>();
            string escaped = Regex.Replace(tiddlyWikiTags, @"\[\[(.*?)\]\]", "\"$1\"");
            return ParseCommandLineArguments(escaped);
        }
        static string FormatTiddlyWikiTags(string tags)
        {
            string[] items = ParseTags(tags);
            return $"\"{string.Join(", ", items)}\"";
        }
        static string FormatTiddlyWikiToMD(string text, Tiddler[] catalog)
        {
            text = Regex.Replace(text, "''(.*?)''", "**$1**"); // Replace bolds
            text = Regex.Replace(text, "//(.*?)//", "*$1*"); // Replace italics
            text = Regex.Replace(text, "^(#) (.*)$", "1. $2", RegexOptions.Multiline); // Replace enumerations
            text = Regex.Replace(text, "<<<(.*?)<<<", m =>
            {
                string content = m.Groups[1].Value.Trim();
                if (content.StartsWith(".tc-big-quote"))
                    content = content.Substring(".tc-big-quote".Length).Trim();
                return Regex.Replace(content, $"^(.*)$", "> $1", RegexOptions.Multiline);
            }, RegexOptions.Singleline); // Replace quotes
            text = Regex.Replace(text, "^(!+) (.*)$", m =>
            {
                string formatter = m.Groups[1].Value.Replace("!", "#");
                string header = m.Groups[2].Value;
                return $"#{formatter} {header}\n";
            }, RegexOptions.Multiline); // Replace headers
            text = Regex.Replace(text, @"^(#+) \*\*(.*)\*\*$", "$1 $2", RegexOptions.Multiline); // Redundant header emphasis
            text = Regex.Replace(text, "<div class=\"tc-table-of-contents\">(.*?)</div>", m =>
            {
                string content = m.Groups[1].Value.Trim();
                string key = Regex.Match(content, "<<toc-selective-expandable '(.*?)'>>").Groups[1].Value;
                return $"TABLE OF CONTENTS: {key}";
            }, RegexOptions.Singleline); // Replace table of contents
            text = Regex.Replace(text, @"\[\[(.*?)\]\]", m =>
            {
                string trans = m.Groups[1].Value;
                Tiddler match = catalog.SingleOrDefault(c => c.Title == trans);
                if (match == null)
                    return $"{{{{{trans} (EMPTY REFERENCE)}}}}";
                else return $"{{{{{trans}}}}}";
            }); // Replace transclusions
            return text;
        }
        public static string[] ParseCommandLineArguments(string commandLineString)
        {
            return Csv.CsvReader.ReadFromText(commandLineString, new Csv.CsvOptions()
            {
                HeaderMode = Csv.HeaderMode.HeaderAbsent,
                Separator = ' '
            }).First().Values;
        }
    }
}