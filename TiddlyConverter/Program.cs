using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TiddlyConverter.Types;
using Console = Colorful.Console;

namespace TiddlyConverter
{
    [JsonSerializable(typeof(Tiddler[]))]
    public partial class TiddlerJsonContext : JsonSerializerContext
    {
    }

    internal static class Program
    {
        #region Main
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine($"""
                    TiddlyConverter <Source JSON File> <Output File/Folder> [<Additional Toggles>]
                    If <Output File/Folder> ends in .md, then it outputs a single MD file, otherwise it creates loose files in the folder.
                    Keyword Arguments:
                    {string.Join(Environment.NewLine, GetNonToggles().Keys.OrderBy(t => t).Select(t => $"  --{t}"))}
                    Additional toggles: 
                    {string.Join(Environment.NewLine, GetToggles().Keys.OrderBy(t => t).Select(t => $"  --{t}"))}
                    """);
                return;
            }
            string jsonFile = args[0];
            string outputFileOrFolderPath = args[1];
            ProgramOptions options = ParseAdditionalOptions(args.Skip(2));

            Tiddler[] wiki = JsonSerializer.Deserialize<Tiddler[]>(File.ReadAllText(jsonFile), TiddlerJsonContext.Default.TiddlerArray);
            TiddlerToMDConverter converter = new(wiki);
            MarkdownDocument[] mds = converter.Convert(options);
            // Statistics summary
            StringBuilder builder = new();
            builder.AppendLine("Number of tiddlers: " + converter.UsefulTiddlers);
            builder.AppendLine("Number of tags: " + converter.UniqueTags.Count);
            builder.AppendLine("Tags: " + string.Join(", ", converter.UniqueTags
                .OrderBy(t => t)
                .Select(t =>
                {
                    int count = wiki.Where(w => w.TagsArray.Contains(t)).Count();
                    return $"{t} ({count})";
                }))
            );
            if (options.OutputCategories.Count > 0)
                builder.AppendLine("Categories: " + string.Join(", ", options.OutputCategories.Where(converter.UniqueTags.Contains)));
            string summary = builder.ToString().Trim();
            Console.WriteLine(summary);

            // Enumerate outputs
            if (outputFileOrFolderPath.EndsWith(".md"))
                OutputToSingleFile(summary, outputFileOrFolderPath, mds, options);
            else
                OutputToFolders(summary, outputFileOrFolderPath, mds, options);
        }
        #endregion

        #region Helpers
        private static Dictionary<string, PropertyInfo> GetToggles()
        {
            return typeof(ProgramOptions)
                .GetProperties()
                .Where(p => p.PropertyType == typeof(bool))
                .ToDictionary(p => p.Name, p => p);
        }
        private static Dictionary<string, PropertyInfo> GetNonToggles()
        {
            return typeof(ProgramOptions)
                .GetProperties()
                .Where(p => p.PropertyType != typeof(bool))
                .ToDictionary(p => p.Name, p => p);
        }
        #endregion

        #region Routines
        private static ProgramOptions ParseAdditionalOptions(IEnumerable<string> arguments)
        {
            Dictionary<string, PropertyInfo> toggles = GetToggles();

            string currentState = null;
            ProgramOptions options = new();
            foreach (var argument in arguments)
            {
                if (argument.StartsWith("--"))
                {
                    string name = argument.TrimStart('-');
                    // Toggle toggles on/off
                    if (toggles.ContainsKey(name))
                    {
                        toggles[name].SetValue(options, !(bool)toggles[name].GetValue(options));
                        currentState = null;
                    }
                    else
                        currentState = argument;
                }
                else
                {
                    switch (currentState)
                    {
                        case "--OutputCategories":
                            options.OutputCategories.AddRange(argument.Split(',').Select(a => a.Trim()));
                            break;
                        default:
                            throw new ArgumentException($"Invalid argument: {argument}");
                    }
                }
            }
            return options;
        }
        private static void OutputToFolders(string summary, string folderPath, MarkdownDocument[] mds, ProgramOptions options)
        {
            HashSet<MarkdownDocument> saved = [];
            Directory.CreateDirectory(folderPath);
            File.WriteAllText(Path.Combine(folderPath, "Summary.md"), summary);

            foreach (var item in mds
                .Where(t => t.Tags.Contains(MarkdownDocument.SpecialDatedTagName))
                .OrderBy(t => t.CreateDate))
            {
                item.Save(Path.Combine(folderPath, "Dated.md"), true);
                saved.Add(item);
            }

            foreach (string category in options.OutputCategories)
            {
                Directory.CreateDirectory(Path.Combine(folderPath, category));
                Console.WriteLine($"Output category: {category}...");
                foreach (MarkdownDocument item in mds
                    .Where(r => r.Tags.Contains(category) && !saved.Contains(r)))
                {
                    item.Save(Path.Combine(folderPath, category, $"{item.Title}.md"), false);
                    saved.Add(item);
                }
            }

            int failedCounter = 0; // Use this counter to guarantee alternative filename uniqueness
            foreach (MarkdownDocument item in mds.Where(r => !saved.Contains(r)))
            {
                Directory.CreateDirectory(Path.Combine(folderPath, "Default"));
                try
                {
                    item.Save(Path.Combine(folderPath, "Default", $"{item.Title}.md"), false);
                }
                catch (Exception e)
                {
                    failedCounter++;
                    string alternativeName = $"Item {failedCounter} ({item.CreateDate:yyyyMMdd}).md";
                    item.Save(Path.Combine(folderPath, "Default", alternativeName), false);
                    Console.WriteLine($"Issue saving {item.Title}.md: {e.Message}; Save as {alternativeName} instead.");
                }
            }
        }

        private static void OutputToSingleFile(string summary, string filePath, MarkdownDocument[] mds, ProgramOptions options)
        {
            File.WriteAllText(filePath, $"""
                    <!-- Generated using Tiddly Converter.
                    {summary.Trim()}
                    -->
                    """ + Environment.NewLine + Environment.NewLine);

            IOrderedEnumerable<MarkdownDocument> sorted = mds
                .OrderByDescending(t => t.Tags.Contains(MarkdownDocument.SpecialDatedTagName));
            foreach (string category in options.OutputCategories.OrderBy(c => c))
                sorted = sorted.ThenByDescending(s => s.Tags.Contains(category));
            foreach (MarkdownDocument item in sorted.ThenBy(t => t.CreateDate))
                item.Save(filePath, true);
        }
        #endregion
    }
}