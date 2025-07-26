using TiddlyConverter.Types;

namespace TiddlyConverter.UnitTests
{
    public class TiddlyTagParserTests
    {
        [Fact]
        public void ParseTags_NullOrWhiteSpace_ReturnsEmptyArray()
        {
            // null, empty or all‑whitespace inputs
            Assert.Empty(Tiddler.ParseTags(null));
            Assert.Empty(Tiddler.ParseTags(string.Empty));
            Assert.Empty(Tiddler.ParseTags("   "));
        }

        [Fact]
        public void ParseTags_SimpleTags_SplittedBySpace()
        {
            var result = Tiddler.ParseTags("tag1 tag2 tag3");
            Assert.Equal(new[] { "tag1", "tag2", "tag3" }, result);
        }

        [Fact]
        public void ParseTags_SingleBracketTag_ReturnsInnerContent()
        {
            var result = Tiddler.ParseTags("[[Rank - Affection]]");
            Assert.Equal(new[] { "Rank - Affection" }, result);
        }

        [Fact]
        public void ParseTags_MixedBracketAndSimpleTags()
        {
            var input = "tag1 [[a b c]] tag2";
            var expected = new[] { "tag1", "a b c", "tag2" };
            Assert.Equal(expected, Tiddler.ParseTags(input));
        }

        [Fact]
        public void ParseTags_MultipleBracketTags()
        {
            var input = "[[a b]] [[c d]]";
            var expected = new[] { "a b", "c d" };
            Assert.Equal(expected, Tiddler.ParseTags(input));
        }

        [Fact]
        public void ParseTags_UnicodeAndSpecialChars()
        {
            var input = "~阿May 夢のノート";
            var expected = new[] { "~阿May", "夢のノート" };
            Assert.Equal(expected, Tiddler.ParseTags(input));
        }

        [Fact]
        public void ParseTags_LeadingTrailingWhitespace()
        {
            var input = "   tag1   tag2   ";
            var expected = new[] { "tag1", "tag2" };
            Assert.Equal(expected, Tiddler.ParseTags(input));
        }

        [Fact]
        public void ParseTags_ConsecutiveSpaces_Ignored()
        {
            var input = "tag1    tag2";
            var expected = new[] { "tag1", "tag2" };
            Assert.Equal(expected, Tiddler.ParseTags(input));
        }
    }
}