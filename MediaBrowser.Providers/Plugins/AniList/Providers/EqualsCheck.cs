using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
// ReSharper disable All

#pragma warning disable SA1614
#pragma warning disable SA1401
#pragma warning disable CA1307
#pragma warning disable CA1307
#pragma warning disable SA1122
#pragma warning disable CA2007
#pragma warning disable CA1851
#pragma warning disable SA1616
#pragma warning disable CA1829
#pragma warning disable CA1305
#pragma warning disable CA1311
#pragma warning disable CA1304
#pragma warning disable SA1505
#pragma warning disable CS1570

namespace MediaBrowser.Providers.Plugins.AniList.Providers
{
    internal class EqualsCheck
    {
        public readonly ILogger<EqualsCheck> _logger;

        public EqualsCheck(ILogger<EqualsCheck> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Clear name.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<string> ClearName(string a, CancellationToken cancellationToken)
        {
            try
            {
                a = a.Trim().Replace(await OneLineRegex(new Regex(@"(?s) \(.*?\)"), a.Trim(), cancellationToken, 0), "");
            }
            catch (Exception)
            {
            }

            a = a.Replace(".", " ");
            a = a.Replace("-", " ");
            a = a.Replace("`", "");
            a = a.Replace("'", "");
            a = a.Replace("&", "and");
            a = a.Replace("(", "");
            a = a.Replace(")", "");

            try
            {
                a = a.Replace(await OneLineRegex(new Regex(@"(?s)(S[0-9]+)"), a.Trim(), cancellationToken), await OneLineRegex(new Regex(@"(?s)S([0-9]+)"), a.Trim(), cancellationToken));
            }
            catch (Exception)
            {
            }

            return a;
        }

        /// <summary>
        /// Clear name heavy. Example: Text & Text to Text and Text.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<string> ClearNameStep2(string a, CancellationToken cancellationToken)
        {
            if (a.Contains("Gekijyouban"))
            {
                a = (a.Replace("Gekijyouban", "") + " Movie").Trim();
            }

            if (a.Contains("gekijyouban"))
            {
                a = (a.Replace("gekijyouban", "") + " Movie").Trim();
            }

            try
            {
                a = a.Trim().Replace(await OneLineRegex(new Regex(@"(?s) \(.*?\)"), a.Trim(), cancellationToken, 0), "");
            }
            catch (Exception)
            {
            }

            a = a.Replace(".", " ");
            a = a.Replace("-", " ");
            a = a.Replace("`", "");
            a = a.Replace("'", "");
            a = a.Replace("&", "and");
            a = a.Replace(":", "");
            a = a.Replace("␣", "");
            a = a.Replace("2wei", "zwei");
            a = a.Replace("3rei", "drei");
            a = a.Replace("4ier", "vier");

            return a;
        }

        /// <summary>
        /// If a and b match it return true.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<bool> CompareStrings(string a, string b, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(a) && !string.IsNullOrEmpty(b))
            {
                if (await SimpleCompare(a, b, cancellationToken))
                {
                    return true;
                }

                return false;
            }

            return false;
        }

        /// <summary>
        /// Cut p(%) away from the string.
        /// </summary>
        /// <param name="string_"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="min_lenght"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static async Task<string> HalfString(string string_, CancellationToken cancellationToken, int min_lenght = 0, int p = 50)
        {
            decimal length = 0;

            if (await Task.Run(() => ((int)((decimal)string_.Length - (((decimal)string_.Length / 100m) * (decimal)p)) > min_lenght), cancellationToken))
            {
                length = (decimal)string_.Length - (((decimal)string_.Length / 100m) * (decimal)p);
            }
            else
            {
                if (string_.Length < min_lenght)
                {
                    length = string_.Length;
                }
                else
                {
                    length = min_lenght;
                }
            }

            return string_.Substring(0, (int)length);
        }

        /// <summary>
        /// simple regex.
        /// </summary>
        /// <param name="regex"></param>
        /// <param name="match"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="group"></param>
        /// <param name="match_int"></param>
        /// <returns></returns>
        public static async Task<string> OneLineRegex(Regex regex, string match, CancellationToken cancellationToken, int group = 1, int match_int = 0)
        {
            int x = 0;

            foreach (Match m in regex.Matches(match))
            {
                if (x == match_int)
                {
                    return await Task.Run(() => m.Groups[group].Value.ToString(), cancellationToken);
                }

                x++;
            }

            return "";
        }

        /// <summary>
        /// Compare 2 Strings, and it just works
        /// SeriesA S2 == SeriesA Second Season | True.
        /// </summary>
        private static async Task<bool> SimpleCompare(string a, string b, CancellationToken cancellationToken, bool fastmode = false)
        {
            if (fastmode)
            {
                if (a[0] == b[0])
                {
                }
                else
                {
                    return false;
                }
            }

            if (await CoreCompare(a, b, cancellationToken))
            {
                return true;
            }

            if (await CoreCompare(b, a, cancellationToken))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Compare 2 Strings, and it just works.
        /// </summary>
        private static async Task<bool> CoreCompare(string a, string b, CancellationToken cancellationToken)
        {
            if (a == b)
            {
                return true;
            }

            a = a.ToLower().Replace(" ", "").Trim().Replace(".", "");
            b = b.ToLower().Replace(" ", "").Trim().Replace(".", "");

            if (await ClearName(a, cancellationToken) == await ClearName(b, cancellationToken))
            {
                return true;
            }

            if (await ClearNameStep2(a, cancellationToken) == await ClearNameStep2(b, cancellationToken))
            {
                return true;
            }

            if (a.Replace("-", " ") == b.Replace("-", " "))
            {
                return true;
            }

            if (a.Replace(" 2", ":secondseason") == b.Replace(" 2", ":secondseason"))
            {
                return true;
            }

            if (a.Replace("2", "secondseason") == b.Replace("2", "secondseason"))
            {
                return true;
            }

            if (await ConvertSymbolsToNumbers(a, "I", cancellationToken) == await ConvertSymbolsToNumbers(b, "I", cancellationToken))
            {
                return true;
            }

            if (await ConvertSymbolsToNumbers(a, "!", cancellationToken) == await ConvertSymbolsToNumbers(b, "!", cancellationToken))
            {
                return true;
            }

            if (a.Replace("ndseason", "") == b.Replace("ndseason", ""))
            {
                return true;
            }

            if (a.Replace("ndseason", "") == b)
            {
                return true;
            }

            if (await OneLineRegex(new Regex(@"((.*)s([0 - 9]))"), a, cancellationToken, 2) + await OneLineRegex(new Regex(@"((.*)s([0 - 9]))"), a, cancellationToken, 3) == await OneLineRegex(new Regex(@"((.*)s([0 - 9]))"), b, cancellationToken, 2) + await OneLineRegex(new Regex(@"((.*)s([0 - 9]))"), b, cancellationToken, 3))
            {
                if (!string.IsNullOrEmpty(await OneLineRegex(new Regex(@"((.*)s([0 - 9]))"), a, cancellationToken, 2) + await OneLineRegex(new Regex(@"((.*)s([0 - 9]))"), a, cancellationToken, 3)))
                {
                    return true;
                }
            }

            if (await OneLineRegex(new Regex(@"((.*)s([0 - 9]))"), a, cancellationToken, 2) + await OneLineRegex(new Regex(@"((.*)s([0 - 9]))"), a, cancellationToken, 3) == b)
            {
                if (!string.IsNullOrEmpty(await OneLineRegex(new Regex(@"((.*)s([0 - 9]))"), a, cancellationToken, 2) + await OneLineRegex(new Regex(@"((.*)s([0 - 9]))"), a, cancellationToken, 3)))
                {
                    return true;
                }
            }

            if (a.Replace("rdseason", "") == b.Replace("rdseason", ""))
            {
                return true;
            }

            if (a.Replace("rdseason", "") == b)
            {
                return true;
            }

            try
            {
                if (a.Replace("2", "secondseason").Replace(await OneLineRegex(new Regex(@"(?s)\(.*?\)"), a, cancellationToken, 0), "") == b.Replace("2", "secondseason").Replace(await OneLineRegex(new Regex(@"(?s)\(.*?\)"), b, cancellationToken, 0), ""))
                {
                    return true;
                }
            }
            catch (Exception)
            {
            }

            try
            {
                if (a.Replace("2", "secondseason").Replace(await OneLineRegex(new Regex(@"(?s)\(.*?\)"), a, cancellationToken, 0), "") == b)
                {
                    return true;
                }
            }
            catch (Exception)
            {
            }

            try
            {
                if (a.Replace(" 2", ":secondseason").Replace(await OneLineRegex(new Regex(@"(?s)\(.*?\)"), a, cancellationToken, 0), "") == b.Replace(" 2", ":secondseason").Replace(await OneLineRegex(new Regex(@"(?s)\(.*?\)"), b, cancellationToken, 0), ""))
                {

                    return true;
                }
            }
            catch (Exception)
            {
            }

            try
            {
                if (a.Replace(" 2", ":secondseason").Replace(await OneLineRegex(new Regex(@"(?s)\(.*?\)"), a, cancellationToken, 0), "") == b)
                {
                    return true;
                }
            }
            catch (Exception)
            {
            }

            try
            {
                if (a.Replace(await OneLineRegex(new Regex(@"(?s)\(.*?\)"), a, cancellationToken, 0), "") == b.Replace(await OneLineRegex(new Regex(@"(?s)\(.*?\)"), b, cancellationToken, 0), ""))
                {
                    return true;
                }
            }
            catch (Exception)
            {
            }

            try
            {
                if (a.Replace(await OneLineRegex(new Regex(@"(?s)\(.*?\)"), a, cancellationToken, 0), "") == b)
                {

                    return true;
                }
            }
            catch (Exception)
            {
            }

            try
            {
                if (b.Replace(await OneLineRegex(new Regex(@"(?s)\(.*?\)"), b, cancellationToken, 0), "").Replace("  2", ": second Season") == a)
                {
                    return true;
                }
            }
            catch (Exception)
            {
            }

            try
            {
                if (a.Replace(" 2ndseason", ":secondseason") + " vs " + b == a)
                {
                    return true;
                }
            }
            catch (Exception)
            {
            }

            try
            {
                if (a.Replace(await OneLineRegex(new Regex(@"(?s)\(.*?\)"), a, cancellationToken, 0), "").Replace("  2", ":secondseason") == b)
                {
                    return true;
                }
            }
            catch (Exception)
            {
            }

            return false;
        }

        /// <summary>
        /// Example: Convert II to 2.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="symbol"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private static async Task<string> ConvertSymbolsToNumbers(string input, string symbol, CancellationToken cancellationToken)
        {
            try
            {
                string regex_c = "_";
                int x = 0;
                int highest_number = 0;

                while (!string.IsNullOrEmpty(regex_c))
                {
                    regex_c = (await OneLineRegex(new Regex(@"(" + symbol + @"+)"), input.ToLower().Trim(), cancellationToken, 1, x)).Trim();

                    if (highest_number < regex_c.Count())
                    {
                        highest_number = regex_c.Count();
                    }

                    x++;
                }

                x = 0;
                string output = "";

                while (x != highest_number)
                {
                    output = output + symbol;
                    x++;
                }

                output = input.Replace(output, highest_number.ToString());
                if (string.IsNullOrEmpty(output))
                {
                    output = input;
                }

                return output;
            }
            catch (Exception)
            {
                return input;
            }
        }

        /// <summary>
        /// Simple Compare a XElement with a string.
        /// </summary>
        /// <param name="a_"></param>
        /// <param name="b"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>The comparison result.</returns>
        private static async Task<bool> SimpleCompare(IEnumerable<XElement> a_, string b, CancellationToken cancellationToken)
        {
            bool ignore_date = true;
            string a_date = string.Empty;
            string b_date = string.Empty;

            string b_date_ = await OneLineRegex(new Regex(@"([0-9][0-9][0-9][0-9])"), b, cancellationToken).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(b_date_))
            {
                b_date = b_date_;
            }

            if (!string.IsNullOrEmpty(b_date))
            {
                foreach (XElement a in a_)
                {
                    if (ignore_date)
                    {
                        string a_date_ = await OneLineRegex(new Regex(@"([0-9][0-9][0-9][0-9])"), a.Value, cancellationToken).ConfigureAwait(false);
                        if (!string.IsNullOrEmpty(a_date_))
                        {
                            a_date = a_date_;
                            ignore_date = false;
                        }
                    }
                }
            }

            if (!ignore_date)
            {
                if (a_date.Trim() == b_date.Trim())
                {
                    foreach (XElement a in a_)
                    {
                        if (await SimpleCompare(a.Value, b, cancellationToken, true))
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    return false;
                }

                return false;
            }
            else
            {
                foreach (XElement a in a_)
                {
                    if (ignore_date)
                    {
                        if (await SimpleCompare(a.Value, b, cancellationToken, true))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }
    }
}

#pragma warning restore SA1614
#pragma warning restore SA1401
#pragma warning restore CA1307
#pragma warning restore SA1122
#pragma warning restore CA2007
#pragma warning restore CA1851
#pragma warning restore SA1616
#pragma warning restore CA1829
#pragma warning restore CA1305
#pragma warning restore CA1311
#pragma warning restore CA1304
#pragma warning restore SA1505
#pragma warning restore CS1570
