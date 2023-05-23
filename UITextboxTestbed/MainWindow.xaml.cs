using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

using System.IO;
using System.Reflection;
using System.Windows;
using System.Xml;

namespace UITextboxTestbed
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(Assembly.GetExecutingAssembly().GetName().Name + ".xenonsyntax.xshd"))
            {
                using (XmlTextReader reader = new XmlTextReader(s))
                {
                    textEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
        }

        private void TestGetText(object sender, RoutedEventArgs e)
        {
            tbtest.Text = textEditor.Text;
        }

        private void TestInsert(object sender, RoutedEventArgs e)
        {
            textEditor.Document.Insert(textEditor.CaretOffset, "hello");
        }

    }
}
