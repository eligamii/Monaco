using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Monaco.Helpers
{
    public static class JavascriptHelpers
    {
        public static string ToCSharpString(this string str) 
        {
            // convert \x things
            str = str.Replace(@"\\", "\\")
                     .Replace(@"\n", "\n")
                     .Replace(@"\r", "\r")
                     .Replace(@"\t", "\t")
                     .Replace(@"\b", "\b")
                     .Replace(@"\f", "\f")
                     .Replace("\\\"", "\"");

            // convert unicode
            var matches = Regex.Matches(str, @"\\u[A-Z0-9]{4}");
            if (matches.Count >= 0)
            {
                foreach (Match match in matches)
                {
                    int unicode = int.Parse(match.Value.Replace("\\u", ""), System.Globalization.NumberStyles.HexNumber);
                    char c = (char)unicode;
                    str = str.Replace(match.Value, c.ToString());
                }
            }


            str = str.Substring(1);
            str = str.Remove(str.Length - 1);
            return str;
        }
    }
}
