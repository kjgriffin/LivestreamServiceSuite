using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Threading;

namespace SlideCreater
{
    static class TextBoxColoring
    {

        public static async void PerformTextHighlighting(TextBox tb, Dispatcher d)
        {
            string text = "";
            // grab text
            await d.BeginInvoke(() =>
            {
                text = tb.Text;
            });

            List<(string precomment, string comment)> plines = new List<(string, string)>();

            // process it
            // only filter for comments now
            await System.Threading.Tasks.Task.Run(() =>
            {
                var lines = text.Split(Environment.NewLine);
                bool inblockcomment = false;
                foreach (var item in lines)
                {
                    if (!inblockcomment)
                    {
                        var csep = item.Split("/*");
                        if (csep.Length > 1)
                        {
                            // have block comment
                            // this takes precedence
                            plines.Add((csep[0], "/*" + csep.Skip(1).ToString()));
                            inblockcomment = true;
                        }
                        // otherwise check for singleline comments
                        csep = item.Split("//");
                        if (csep.Length > 1)
                        {
                            plines.Add((csep[0], "//" + csep.Skip(1).ToString()));
                        }
                    }
                    else
                    {
                        var csep = item.Split("*/");
                        if (csep.Length > 1)
                        {
                            plines.Add(("", "*/"));
                            inblockcomment = false;
                        }
                        else
                        {
                            plines.Add(("", item));
                        }
                    }
                }
            });

            // write it out
            await d.BeginInvoke(() =>
            {
                tb.Text = "";
                foreach (var line in plines)
                {
                    
                }
            });
        }

    }
}
