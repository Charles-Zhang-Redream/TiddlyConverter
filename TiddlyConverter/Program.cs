using Newtonsoft.Json;
using System.Text;
using TiddlyConverter.Types;
using Console = Colorful.Console;

namespace TiddlyConverter
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("""
                    TiddlyConverter <Source JSON File> <Output File/Folder> [<Additional Toggles>]
                    If <Output File/Folder> ends in .md, then it outputs a single MD file, otherwise it creates loose files in the folder.
                    Additional toggles: 
                      --KeepDrafts
                      --HighlightLinks
                    """);
                return;
            }
            string jsonFile = args[0];
            string outputFileOrFolderPath = args[1];
            ProgramOptions options = ParseAdditionalOptions(args.Skip(2));

            Tiddler[] wiki = JsonConvert.DeserializeObject<Tiddler[]>(File.ReadAllText(jsonFile));
            TiddlerToMDConverter converter = new TiddlerToMDConverter(wiki);
            MarkdownDocument[] mds = converter.Convert(options);
            // Statistics summary
            StringBuilder summary = new();
            summary.AppendLine("Number of tiddlers: " + converter.UsefulTiddlers);
            summary.AppendLine("Number of tags: " + converter.UniqueTags.Count);
            summary.AppendLine("Tags: " + string.Join(", ", converter.UniqueTags
                .OrderBy(t => t)
                .Select(t =>
                {
                    int count = wiki.Where(w => w.TagsArray.Contains(t)).Count();
                    return $"{t} ({count})";
                }))
            );
            Console.WriteLine(summary.ToString());

            // Enumerate outputs
            if (outputFileOrFolderPath.EndsWith(".md"))
                File.WriteAllText(outputFileOrFolderPath, $"""
                    <!-- Generated using Tiddly Converter.
                    {summary.ToString().Trim()}
                    -->


                    """);
            foreach (var item in mds.OrderBy(t => t.CreateDate))
            {
                if (outputFileOrFolderPath.EndsWith(".md"))
                    item.Save(outputFileOrFolderPath, true);
                else
                    item.Save(Path.Combine(outputFileOrFolderPath, item.Title), false);
            }
        }

        private static ProgramOptions ParseAdditionalOptions(IEnumerable<string> arguments)
        {
            ProgramOptions options = new();
            foreach (var argument in arguments)
            {
                if (!argument.StartsWith("--"))
                    throw new ArgumentException($"Invalid argument: {argument}");
                switch (argument)
                {
                    case "--KeepDrafts":
                        options.KeepDrafts = true;
                        break;
                    case "--HighlightLinks":
                        options.KeepDrafts = true;
                        break;
                    default:
                        break;
                }
            }
            return options;
        }
    }
}