using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

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

        public void SetFontSize(int size)
        {
            tb.FontSize = size;
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
            _LOCK_allow_hightlight_to_rebuild_document = true;
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
            _LOCK_allow_hightlight_to_rebuild_document = false;
            PerformTextHighlighting();
        }


        FlowDocument setdoc = new FlowDocument();

        private void PerformTextHighlighting(string overwritetext = "")
        {
            _LOCK_allow_hightlight_to_rebuild_document = true;

            TextPointer currentposition = tb.CaretPosition;
            TextPointer newpos = null;

            FlowDocument newdoc = new FlowDocument();

            Run newrunpos = null;
            int offset = 0;

            foreach (var par in tb.Document.Blocks.Select(b => b as Paragraph))
            {
                // check if line comment
                Paragraph newpar = new Paragraph();
                if (par != null)
                {
                    // check if starts with comments
                    foreach (var run in par.Inlines.Select(i => i as Run))
                    {
                        if (run != null)
                        {

                            if (run.Text.StartsWith("/// "))
                            {
                                Run nr = new Run(run.Text) { Foreground = Brushes.Gray };
                                newpar.Inlines.Add(nr);
                                //run.Foreground = Brushes.Gray;
                                if (currentposition.Parent == run)
                                {
                                    offset = currentposition.GetOffsetToPosition(run.ContentStart) * -1;
                                    newrunpos = nr;
                                }

                            }
                            else if (run.Text.StartsWith("//>"))
                            {
                                Run nr = new Run(run.Text) { Foreground = Brushes.Orange };
                                newpar.Inlines.Add(nr);
                                //run.Foreground = Brushes.Orange;
                                if (currentposition.Parent == run)
                                {
                                    offset = currentposition.GetOffsetToPosition(run.ContentStart) * -1;
                                    newrunpos = nr;
                                }
                            }
                            else if (run.Text.StartsWith("//"))
                            {
                                Run nr = new Run(run.Text) { Foreground = Brushes.Green };
                                newpar.Inlines.Add(nr);
                                //run.Foreground = Brushes.Green;

                                if (currentposition.Parent == run)
                                {
                                    offset = currentposition.GetOffsetToPosition(run.ContentStart) * -1;
                                    newrunpos = nr;
                                }

                            }
                            else if (run.Text.StartsWith("#"))
                            {

                                if (currentposition.Parent == run)
                                {
                                    offset = currentposition.GetOffsetToPosition(run.ContentStart) * -1;
                                }

                                // split run into command/ rest
                                var sindex = run.Text.IndexOfAny(new[] { ' ', '{', '(', ':' });
                                var cmd = "";
                                var other = "";
                                if (sindex != -1)
                                {
                                    cmd = run.Text.Substring(0, sindex);
                                    Run cmdrun = new Run(cmd) { Foreground = Brushes.Blue };
                                    newpar.Inlines.Add(cmdrun);
                                    Run otherrun = null;
                                    if (run.Text.Length > sindex)
                                    {
                                        //other = run.Text.Substring(sindex -1, run.Text.Length -1);
                                        other = run.Text.Remove(0, cmd.Length);
                                        otherrun = new Run(other) { Foreground = Brushes.Black };
                                        newpar.Inlines.Add(otherrun);
                                    }

                                    if (currentposition.Parent == run)
                                    {
                                        if (offset > sindex)
                                        {
                                            offset -= sindex;
                                            newrunpos = otherrun;
                                        }
                                        else
                                        {
                                            newrunpos = cmdrun;
                                        }
                                    }

                                }
                                else
                                {
                                    Run nr = new Run(run.Text) { Foreground = Brushes.Blue };
                                    newpar.Inlines.Add(nr);
                                    if (currentposition.Parent == run)
                                    {
                                        newrunpos = nr;
                                    }
                                }
                            }
                            else
                            {
                                //run.Foreground = Brushes.Black;
                                Run nr = new Run(run.Text) { Foreground = Brushes.Black };
                                newpar.Inlines.Add(nr);

                                if (currentposition.Parent == run)
                                {
                                    offset = currentposition.GetOffsetToPosition(run.ContentStart) * -1;
                                    newrunpos = nr;
                                }

                            }
                        }
                    }
                }
                newdoc.Blocks.Add(newpar);
            }

            setdoc = newdoc;
            tb.Document.Blocks.Clear();
            tb.Document.Blocks.AddRange(newdoc.Blocks.ToList());

            if (newrunpos != null)
            {
                var localpointer = newrunpos.ContentStart;
                newpos = localpointer.GetPositionAtOffset(offset, LogicalDirection.Forward);
            }

            if (newpos != null)
            {
                tb.CaretPosition = newpos;
            }

            _LOCK_allow_hightlight_to_rebuild_document = false;
        }

        bool _LOCK_allow_hightlight_to_rebuild_document = false;
        private void tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextChanged?.Invoke(sender, e);
            if (!_LOCK_allow_hightlight_to_rebuild_document)
            {
                PerformTextHighlighting();
            }
        }

        private void tb_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!_LOCK_allow_hightlight_to_rebuild_document)
            {
                //PerformTextHighlighting();
            }
        }
    }
}
