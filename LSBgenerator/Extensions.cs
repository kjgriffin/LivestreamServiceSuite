using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LSBgenerator
{
    static class Extensions
    {


        public static TVal TryGetVal<TKey, TVal>(this Dictionary<TKey, TVal> dict, TKey key, TVal defaultValue)
        {
            if (dict.ContainsKey(key))
            {
                return dict[key];
            }
            return defaultValue;
        }


    }
}
