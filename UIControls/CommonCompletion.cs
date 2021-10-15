using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;

namespace UIControls
{
    public class CommonCompletion : ICompletionData
    {

        public CommonCompletion(string text, string description = "", double priority = 0)
        {
            Text = text;
            Description = description;
            Priority = priority;
        }

        public ImageSource Image { get; private set; } = null;

        public string Text { get; private set; } = "";

        public object Content => Text;

        private string description = "";
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
            if (completionSegment.EndOffset == completionSegment.Offset)
            {
                // try walking back and doing a greedy replacemen
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
                newstart = Math.Max(newstart, 0);
                textArea.Document.Replace(newstart, completionSegment.EndOffset - newstart, Text);

            }
            else
            {
                //textArea.Document.Replace(completionSegment, this.Text);
            }
        }
    }
}
