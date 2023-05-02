using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace UIControls
{
    /// <summary>
    /// Interaction logic for SyntaxTextBox.xaml
    /// </summary>
    public partial class SyntaxTextBox : UserControl
    {
        public SyntaxTextBox()
        {
            InitializeComponent();
        }

        public event TextChanged TextChanged;

        public string GetAllText()
        {
            return new TextRange(m_rtb.Document.ContentStart, m_rtb.Document.ContentEnd).Text;
        }

        public void InsertLinesAfterCursor(IEnumerable<string> lines, bool focus = false)
        {
            FrameworkContentElement obj = m_rtb.CaretPosition.GetAdjacentElement(LogicalDirection.Forward) as FrameworkContentElement;
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
                m_rtb.Document.Blocks.InsertAfter(block, p);
                block = p;
            }
            m_rtb.CaretPosition = block.ContentEnd;
            if (focus)
            {
                m_rtb.Focus();
            }
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
            m_rtb.Document = d;
            m_rtb.CaretPosition = d.ContentEnd;
        }

        private void m_rtb_TextChanged(object sender, TextChangedEventArgs e)
        {
            m_rtb.TextChanged -= m_rtb_TextChanged;
            //PerformSyntaxHighlighting();
            m_rtb.TextChanged += m_rtb_TextChanged;
            TextChanged?.Invoke(sender, e);
        }

        private void PerformSyntaxHighlighting()
        {
            // see if we can edit the document now
            FlowDocument doc = new FlowDocument();

            string text = GetAllText();

            foreach (var line in text.Split(Environment.NewLine))
            {
                Paragraph p = new Paragraph(new Run(line));
                if (line.StartsWith("//"))
                {
                    p.Foreground = Brushes.Green;
                }
                doc.Blocks.Add(p);
            }

            m_rtb.Document = doc;

        }
    }
}
