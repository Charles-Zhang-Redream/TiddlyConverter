using System;
using System.IO;
using System.Linq;
using System.Text;

namespace TiddlyConverter.Types
{
    /// <summary>
    /// Provides type for Markdown document representation
    /// </summary>
    public sealed class MarkdownDocument
    {
        #region Constants
        public static readonly string SpecialDatedTagName = "#Dated";
        #endregion

        #region Properties
        /// <summary>
        /// Title of Markdown document
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Content of Markdown document
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// Tags of Markdown document
        /// </summary>
        public string[] Tags { get; set; }
        /// <summary>
        /// Date of creation
        /// </summary>
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// Date of modification
        /// </summary>
        public DateTime ModificationDate { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Save the markdown document either as a new file or append to existing file
        /// </summary>
        public void Save(string path, bool append = false)
        {
            StringBuilder builder = new();
            
            // Title, deal with #Dated
            if (Tags.Contains(SpecialDatedTagName))
                builder.AppendLine($"# {CreateDate:yyyyMMdd} {Title}");
            else 
                builder.AppendLine($"# {Title}");
            builder.AppendLine();

            // Tags, deal with #Dated
            if (Tags.Length != 0)
            {
                builder.AppendLine($"Tags: {string.Join(", ", Tags.Except([SpecialDatedTagName]).OrderBy(t => t))}");
                builder.AppendLine();
            }

            // Meta-data
            builder.AppendLine($"""
                Creation: {CreateDate:yyyy-MM-dd}  
                Last Modification: {ModificationDate:yyyy-MM-dd}
                """);
            builder.AppendLine();

            // Content
            if (!string.IsNullOrEmpty(Content))
            {
                builder.AppendLine(Content);
                builder.AppendLine();
            }

            // Append or write to new file
            if (append)
                File.AppendAllText(path, builder.ToString());
            else 
                File.WriteAllText(path, builder.ToString());
        }
        #endregion
    }
}
