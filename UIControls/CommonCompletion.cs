using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

using System;
using System.Windows.Input;

namespace UIControls
{
    public class CommonCompletion : ICompletionData
    {

        public CommonCompletion(string text, string description = "", int captureIndex = -1, double priority = 0)
        {
            Text = text;
            Description = description;
            Priority = priority;
            CaptureIndex = captureIndex;
        }

        public System.Windows.Media.ImageSource Image { get; private set; } = null;

        public string Text { get; private set; } = "";

        public object Content => Text;

        private string description = "";

        public int CaptureIndex { get; private set; }
        public object Description
        {
            get
            {
                if (string.IsNullOrWhiteSpace(description))
                {
                    return $"Insert '{Text}'";
                }
                return description;
            }
            private set => description = (string)value;
        }

        public double Priority { get; private set; } = 0;

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            // only allow tab insert
            if (insertionRequestEventArgs is KeyEventArgs keyargs)
            {
                if (keyargs.Key == Key.Enter)
                {
                    textArea.Document.Insert(textArea.Caret.Offset, System.Environment.NewLine);
                }
                if (keyargs.Key != Key.Tab)
                {
                    return;
                }
            }
            if (completionSegment.EndOffset == completionSegment.Offset)
            {
                // try walking back and doing a greedy replacemen
                /*
                int index = completionSegment.EndOffset - 1;

                int newstart = completionSegment.EndOffset;

                int matchsize = completionSegment.EndOffset - index;

                while (index >= 0 && matchsize <= Text.Length && newstart >= 0)
                {
                    // try and match a bigger substring
                    if (Text.StartsWith(textArea.Document.Text.Substring(index, matchsize)))
                    {
                        newstart = index;
                    }
                    index--;
                    matchsize = completionSegment.EndOffset - index;
                }
                */
                //newstart = Math.Max(newstart, 0);
                int newstart = CaptureIndex != -1 ? CaptureIndex : completionSegment.EndOffset;
                textArea.Document.Replace(newstart, completionSegment.EndOffset - newstart, Text);

            }
            else
            {
                //textArea.Document.Replace(completionSegment, this.Text);
            }
        }
    }
}
