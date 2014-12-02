using System;
using Xunit;

namespace Silverpop.Core.Tests.Extensions
{
    public class StringExtensionsTests
    {
        public class ContainsIgnoreWhitespaceAndNewLinesMethodTests
        {
            [Fact]
            public void ReturnsTrueWhenExpectedWithNoEnvironmentNewLineOrWhitespace()
            {
                Assert.True("test"
                    .ContainsIgnoreWhitespaceAndNewLines("test"));
            }

            [Fact]
            public void ReturnsFalseWhenExpectedWithNoEnvironmentNewLineOrWhitespace()
            {
                Assert.False("test"
                    .ContainsIgnoreWhitespaceAndNewLines("no-match"));
            }

            [Fact]
            public void ReturnsTrueRemovesEnvironmentNewLine()
            {
                Assert.True(("te" + Environment.NewLine + "st")
                    .ContainsIgnoreWhitespaceAndNewLines("test"));
            }

            [Fact]
            public void ReturnsTrueRemovesWhitespace()
            {
                Assert.True(("te  st")
                    .ContainsIgnoreWhitespaceAndNewLines("test"));
            }

            [Fact]
            public void ReturnsTrueRemovesEnvironmentNewLineAndWhitespace()
            {
                Assert.True(("te  " + Environment.NewLine + "  st")
                    .ContainsIgnoreWhitespaceAndNewLines("test"));
            }
        }
    }
}