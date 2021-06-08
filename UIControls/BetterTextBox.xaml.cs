using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UIControls
{

    public delegate void TextChanged(object sender, TextChangedEventArgs e);
    /// <summary>
    /// Interaction logic for BetterTextBox.xaml
    /// </summary>
    public partial class BetterTextBox : UserControl
    {
        public BetterTextBox()
        {
            InitializeComponent();
        }

        public event TextChanged TextChanged;


        public string GetAllText()
        {
            return new TextRange(tb.Document.ContentStart, tb.Document.ContentEnd).Text;
        }

        public void SetText(string text)
        {
            var lines = text.Split(Environment.NewLine);
            FlowDocument d = new FlowDocument();
            foreach (var line in lines)
            {
                Paragraph p = new Paragraph(new Run(line));
                d.Blocks.Add(p);
            }
            tb.Document = d;
            tb.CaretPosition = d.ContentEnd;
        }

        public void InsertTextAtCursor(string text)
        {
            tb.CaretPosition.InsertTextInRun(text);
        }

        public void InsertLinesAfterCursor(IEnumerable<string> lines, bool focus = false)
        {
            FrameworkContentElement obj = tb.CaretPosition.GetAdjacentElement(LogicalDirection.Forward) as FrameworkContentElement;
            while (obj != null)
            {
                if (obj as Block != null)
                {
                    obj = obj as Block;
                    break;
                }
                obj = obj.Parent as FrameworkContentElement;
            }

            Block block = obj as Block;
            foreach (var line in lines)
            {
                Paragraph p = new Paragraph(new Run(line));
                tb.Document.Blocks.InsertAfter(block, p);
                block = p;
            }
            tb.CaretPosition = block.ContentEnd;
            if (focus)
            {
                tb.Focus();
            }
        }

        private void PerformTextHighlighting(string overwritetext = "")
        {
            _LOCK_isalreadyhighlighting = true;

            foreach (var par in tb.Document.Blocks.Select(b => b as Paragraph))
            {
                // check if line comment
                if (par != null)
                {
                    // check if starts with comments
                    foreach (var run in par.Inlines.Select(i => i as Run))
                    {
                        if (run != null)
                        {
                            if (run.Text.StartsWith("///"))
                            {
                                run.Foreground = Brushes.LightGray;
                            }
                            else if (run.Text.StartsWith("//"))
                            {
                                run.Foreground = Brushes.Green;
                            }
                            else if (run.Text.StartsWith("#"))
                            {
                                run.Foreground = Brushes.Blue;
                            }
                            else
                            {
                                run.Foreground = Brushes.Black;
                            }
                        }
                    }
                }
            }

            _LOCK_isalreadyhighlighting = false;
        }

        bool _LOCK_isalreadyhighlighting = false;
        private void tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_LOCK_isalreadyhighlighting)
            {
                PerformTextHighlighting();
            }
            TextChanged?.Invoke(sender, e);
        }

        private void tb_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!_LOCK_isalreadyhighlighting)
            {
                //PerformTextHighlighting();
            }
        }
    }
}
