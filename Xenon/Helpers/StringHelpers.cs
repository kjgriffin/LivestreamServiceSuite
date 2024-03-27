using System.Collections.Generic;
using System.Linq;

namespace Xenon.Helpers
{
    public static class StringHelpers
    {

        public static string SubstringToEnd(this string s, int index)
        {
            return s.Substring(index, s.End() - index);
        }

        public static int End(this string s)
        {
            return s.Length - 1;
        }

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

        public static IEnumerable<(string, T1, T2)> OrderByClosestMatch<T1, T2>(this IEnumerable<(string str, T1 itemA, T2 itemB)> data, string comparison)
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

        public static IEnumerable<(string, T1)> OrderByClosestStrictMatch<T1>(this IEnumerable<(string str, T1 itemA)> data, string comparison)
        {
            return data
                .Select(d =>
                {
                    if (d.str.StartsWith(comparison, System.StringComparison.InvariantCultureIgnoreCase))
                    {
                        return (str: d, rank: d.str.Length - comparison.Length);
                    }
                    else return (str: d, rank: int.MaxValue);
                })
                .Where(d => d.rank < int.MaxValue)
                .OrderBy(x => x.rank)
                .Select(x => x.str);
        }

        public static IEnumerable<(string, T1, T2)> OrderByClosestStrictMatch<T1, T2>(this IEnumerable<(string str, T1 itemA, T2 itemB)> data, string comparison)
        {
            return data
                .Select(d =>
                {
                    if (d.str.StartsWith(comparison, System.StringComparison.InvariantCultureIgnoreCase))
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
