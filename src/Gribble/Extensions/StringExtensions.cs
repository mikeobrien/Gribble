using System.Text.RegularExpressions;

namespace Gribble.Extensions
{
    public static class StringExtensions
    {
        public static string NormalizeWhitespace(this string value)
        {
            return Regex.Replace(value, @"\s+", " ");
        }
    }
}
