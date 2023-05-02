namespace Xenon.Helpers
{
    internal static class BoolHelpers
    {
        public static bool OptionalTrue(this bool val, bool enabled)
        {
            if (enabled)
            {
                return val;
            }
            return true;
        }
        public static bool OptionalFalse(this bool val, bool enabled)
        {
            if (enabled)
            {
                return val;
            }
            return false;
        }
    }
}
