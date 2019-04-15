using System;
using System.Text.RegularExpressions;

namespace Gribble.Extensions
{
    public static class StringExtensions
    {
        public static string NormalizeWhitespace(this string value)
        {
            return Regex.Replace(value, @"\s+", " ");
        }

        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static bool EqualsIgnoreCase(this string source, string compare)
        {
            return source.Equals(compare, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
