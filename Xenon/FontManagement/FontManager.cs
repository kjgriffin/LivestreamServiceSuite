using SixLabors.Fonts;

using System.Linq;

using Xenon.LayoutInfo;

namespace Xenon.FontManagement
{
    internal static class FontManager
    {


        static bool _isInit = false;
        static FontCollection FontCollection;

        private static void Initialize()
        {
            if (!_isInit)
            {
                FontCollection = new FontCollection();
                LoadSystemFonts();
                LoadDefaultFonts();
                _isInit = true;
            }
        }

        private static void LoadDefaultFonts()
        {

            // find all default font families
            var names = System.Reflection.Assembly.GetAssembly(typeof(FontManager))
                .GetManifestResourceNames()
                .Where(n => n.EndsWith(".ttf"));

            foreach (var name in names)
            {
                var stream = System.Reflection.Assembly.GetAssembly(typeof(FontManager)).GetManifestResourceStream(name);
                FontCollection.Add(stream);
            }

        }

        internal static Font GetFont(LWJFont font)
        {
            FontFamily family;
            if (!TryGetFont(font.Name, out family))
            {
                family = FontCollection.Families.FirstOrDefault();
            }
            return family.CreateFont(font.Size, (FontStyle)font.Style);
        }

        private static void LoadSystemFonts()
        {
            FontCollection.AddSystemFonts();
        }


        public static bool TryGetFont(string name, out FontFamily ffamily)
        {
            Initialize();

            return FontCollection.TryGet(name, out ffamily);
        }

        internal static bool HasFont(string altFont)
        {
            Initialize();
            return FontCollection.Families.Any(x => x.Name == altFont);
        }
    }
}
