using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

using System.IO;
using System.Reflection;
using System.Xml;

namespace UIControls
{
    public static class LanguageLoader
    {

        public static void LoadLanguage_XENON(this ICSharpCode.AvalonEdit.TextEditor editor)
        {
            using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(Assembly.GetExecutingAssembly().GetName().Name + ".xenonsyntax.xshd"))
            {
                using (XmlTextReader reader = new XmlTextReader(s))
                {
                    editor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
        }
        public static void LoadLanguage_JSON(this ICSharpCode.AvalonEdit.TextEditor editor)
        {
            using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(Assembly.GetExecutingAssembly().GetName().Name + ".jsonsyntax.xshd"))
            {
                using (XmlTextReader reader = new XmlTextReader(s))
                {
                    editor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
        }
        public static void LoadLanguage_HTML(this ICSharpCode.AvalonEdit.TextEditor editor)
        {
            using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(Assembly.GetExecutingAssembly().GetName().Name + ".htmlsyntax.xshd"))
            {
                using (XmlTextReader reader = new XmlTextReader(s))
                {
                    editor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
        }
        public static void LoadLanguage_CSS(this ICSharpCode.AvalonEdit.TextEditor editor)
        {
            using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(Assembly.GetExecutingAssembly().GetName().Name + ".csssyntax.xshd"))
            {
                using (XmlTextReader reader = new XmlTextReader(s))
                {
                    editor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
        }



    }
}
