using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xenon.Helpers
{
    public static class StringHelpers
    {

        public static string TrySubstring(this string s, int startIndex, int length)
        {
            if (s.Length >= startIndex + length)
            {
                return s.Substring(startIndex, length);
            }
            return "";
        }

        public static IEnumerable<(string, T)> OrderByClosestMatch<T>(this IEnumerable<(string str, T other)> data, string comparison)
        {
            return data.Select(d =>
            {
                if (d.str.StartsWith(comparison))
                {
                    return (d, d.str.Length - comparison.Length);
                }
                else return (d, int.MaxValue);
            }).OrderBy(x => x.Item2).Select(x => x.d);
        }

        public static IEnumerable<(string, T)> OrderByClosestStrictMatch<T>(this IEnumerable<(string str, T other)> data, string comparison)
        {
            return data
                .Select(d =>
                {
                    if (d.str.StartsWith(comparison))
                    {
                        return (str: d, rank: d.str.Length - comparison.Length);
                    }
                    else return (str: d, rank: int.MaxValue);
                })
                .Where(d => d.rank < int.MaxValue)
                .OrderBy(x => x.rank)
                .Select(x => x.str);
        }

    }
}
