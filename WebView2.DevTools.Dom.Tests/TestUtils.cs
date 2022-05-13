using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace WebView2.DevTools.Dom.Tests
{
    public static class TestUtils
    {
        /// <summary>
        /// Removes as much whitespace as possible from a given string. Whitespace
        /// that separates letters and/or digits is collapsed to a space character.
        /// Other whitespace is fully removed.
        /// </summary>
        public static string CompressText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            var sb = new StringBuilder();
            var inWhitespace = false;
            foreach (var ch in text)
            {
                if (char.IsWhiteSpace(ch))
                {
                    if (ch != '\n' && ch != '\r')
                    {
                        inWhitespace = true;
                    }
                }
                else
                {
                    if (inWhitespace)
                    {
                        inWhitespace = false;
                        if (sb.Length > 0 && char.IsLetterOrDigit(sb[sb.Length - 1]) && char.IsLetterOrDigit(ch))
                        {
                            sb.Append(' ');
                        }
                    }
                    sb.Append(ch);
                }
            }

            return sb.ToString();
        }

        internal static async Task WaitForCookieInChromiumFileAsync(string path, string valueToCheck)
        {
            var attempts = 0;
            const int maxAttempts = 10;
            var cookiesFile = Path.Combine(path, "Default", "Cookies");

            while (true)
            {
                attempts++;

                try
                {
                    if (System.IO.File.Exists(cookiesFile) && System.IO.File.ReadAllText(cookiesFile).Contains(valueToCheck))
                    {
                        return;
                    }
                }
                catch (IOException)
                {
                    if (attempts == maxAttempts)
                    {
                        break;
                    }
                }
                await Task.Delay(100);
            }
        }
        internal static string CurateProtocol(string protocol)
            => protocol
                .ToLower()
                .Replace(" ", string.Empty)
                .Replace(".", string.Empty);
    }
}
