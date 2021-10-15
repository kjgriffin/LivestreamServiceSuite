using System;
using System.Collections.Generic;
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

    }
}
