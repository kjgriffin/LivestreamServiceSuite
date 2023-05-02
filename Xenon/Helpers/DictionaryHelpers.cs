using System.Collections.Generic;

namespace Xenon.Helpers
{
    public static class DictionaryHelpers
    {
        public static TValue GetOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue defaultval)
        {
            if (dict.ContainsKey(key))
            {
                return dict[key];
            }
            return defaultval;
        }
    }
}
