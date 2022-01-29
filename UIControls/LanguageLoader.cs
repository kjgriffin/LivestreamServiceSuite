using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
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


    }
}
