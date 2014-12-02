using System;

namespace Silverpop.Core.Tests.Extensions
{
    public static class StringExtensions
    {
        public static bool ContainsIgnoreWhitespaceAndNewLines(this string str, string value)
        {
            return str
                .Replace(Environment.NewLine, null)
                .Replace(" ", null)
                .Contains(value);
        }
    }
}