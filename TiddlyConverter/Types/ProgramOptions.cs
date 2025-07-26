using System.Collections.Generic;

namespace TiddlyConverter.Types
{
    /// <summary>
    /// Overall parsing and output related options
    /// </summary>
    public sealed class ProgramOptions
    {
        #region Parsing
        /// <summary>
        /// Whether we should keep tiddlers that are "Draft 'Name'"
        /// </summary>
        public bool KeepDrafts { get; set; } = false;
        /// <summary>
        /// Whether we should ignore or highlight Tiddly Wiki-stype cross-reference links
        /// </summary>
        public bool HighlightLinks { get; set; } = false;
        #endregion

        #region Output
        /// <summary>
        /// Defines output categories which are defined as keys which will automatically be generated as folders or top level markdown header
        /// </summary>
        public List<string> OutputCategories { get; set; } = [];
        #endregion
    }
}
