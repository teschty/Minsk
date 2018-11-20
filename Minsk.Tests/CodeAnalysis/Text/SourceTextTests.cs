using Minsk.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Minsk.Tests.CodeAnalysis.Text
{
    public class SourceTextTests
    {
        [Theory]
        [InlineData("a\r\nb", 2)]
        [InlineData("a\nb", 2)]
        [InlineData("one\ntwo\nthree", 3)]
        public void SourceText_From_HasCorrectNumberOfLines(string text, int expectedNumLines)
        {
            var sourceText = SourceText.From(text);
            Assert.Equal(expectedNumLines, sourceText.Lines.Length);
        }

        // Be sure to note that \r & \n are ONE character, not two
        [Theory]
        [InlineData("a\r\nb", 3, 1)]
        [InlineData("a\nb", 0, 0)]
        [InlineData("one\ntwo\nThree", 8, 2)]
        public void SourceText_From_CalculatesCorrectLineIndex(string text, int charPosition, int expectedLineIndex)
        {
            var sourceText = SourceText.From(text);
            Assert.Equal(expectedLineIndex, sourceText.GetLineIndex(charPosition));
        }
    }
}
