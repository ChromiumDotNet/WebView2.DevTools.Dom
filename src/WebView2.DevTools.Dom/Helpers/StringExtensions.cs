using System;

namespace WebView2.DevTools.Dom.Helpers
{
    internal static class StringExtensions
    {
        /// <summary>
        /// Quotes the specified <see cref="string"/>.
        /// </summary>
        /// <param name="value">The string to quote.</param>
        /// <returns>A quoted string.</returns>
        public static string Quote(this string value)
        {
            if (!IsQuoted(value))
            {
                value = string.Concat("\"", value, "\"");
            }
            return value;
        }

        private static bool IsQuoted(this string value)
        {
            return value.StartsWith("\"", StringComparison.OrdinalIgnoreCase)
                   && value.EndsWith("\"", StringComparison.OrdinalIgnoreCase);
        }

        public static string ToLowerInvariant(this Enum e)
        {
#pragma warning disable CA1308 // Normalize strings to uppercase
            return e.ToString().ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase
        }
    }
}
