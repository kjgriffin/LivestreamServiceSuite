using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AngleSharp.Dom;

namespace LutheRun
{
    class LSBElementIntroit : ILSBElement
    {


        public string Caption { get; private set; }
        public List<(bool hasspeaker, string speaker, string text)> Lines { get; private set; } = new List<(bool hasspeaker, string speaker, string text)>();
        public string PostsetCmd { get; set; } = "";

        public static ILSBElement Parse(IElement element)
        {
            var lines = new List<(bool, string, string)>();
            foreach (var lsbcontent in element.Children.Where(c => c.LocalName == "lsb-content"))
            {
                lines.AddRange(lsbcontent.ExtractTextAsLiturgicalPoetry());
            }

            var caption = LSBElementCaption.Parse(element) as LSBElementCaption;

            return new LSBElementIntroit() { Lines = lines, Caption = caption?.Caption };

        }

        public string DebugString()
        {
            return $"/// XENON DEBUG::Parsed as LSB_ELEMENT_INTROIT'";
        }

        public string XenonAutoGen()
        {
            // for now we'll have to assume that a 2 line cadence will work
            // this may not at all be correct

            StringBuilder sb = new StringBuilder();

            bool showspeaker = false;
            // only show speakers if introit has multiple speakers
            var firstspeaker = Lines.FirstOrDefault().speaker;
            showspeaker = !Lines.All(x => x.speaker == firstspeaker);

            string lastspeaker = "$";

            int tcmdnum = 0;
            int totalcomands = (int)Math.Ceiling(Lines.Count / 2.0);
            var postset = PostsetCmd.ExtractPostsetValues();

            int lnum = 0;
            foreach (var line in Lines)
            {
                if (lnum > 1)
                {
                    lnum = 0;
                    tcmdnum += 1;
                }

                if (line.hasspeaker)
                {
                    lastspeaker = line.speaker;
                }

                if (lnum == 0)
                {
                    sb.Append("#tlverse(\"");
                    sb.Append(Caption);
                    sb.Append("\", ");
                    sb.Append("\"\", ");
                    sb.Append("\"");
                    sb.Append(showspeaker ? "true" : "false");
                    sb.AppendLine("\") {");

                    sb.Append(lastspeaker);
                    sb.Append(" ");
                    sb.AppendLine(line.text);
                }
                else
                {
                    sb.Append(lastspeaker);
                    sb.Append(" ");
                    sb.AppendLine(line.text);
                    sb.Append("}");
                    string fval = "";
                    string lval = "";
                    if (tcmdnum == 0)
                    {
                        if (postset.first != -1)
                        {
                            fval = $"first={postset.first}";
                        }
                    }
                    if (tcmdnum == totalcomands - 1)
                    {
                        if (postset.last != -1)
                        {
                            lval = $"last={postset.last}";
                        }
                    }
                    if (fval != string.Empty || lval != string.Empty)
                    {
                        sb.Append("::postset(");
                        if (fval != string.Empty)
                        {
                            sb.Append(fval);
                            if (lval != string.Empty)
                            {
                                sb.Append(", ");
                            }
                        }
                        if (lval != string.Empty)
                        {
                            sb.Append(lval);
                        }
                        sb.Append(")");
                    }
                    sb.AppendLine();
                }
                lnum += 1;
            }

            if (lnum == 1)
            {
                sb.Append("}");

                string fval = "";
                string lval = "";
                if (tcmdnum == 0)
                {
                    if (postset.first != -1)
                    {
                        fval = $"first={postset.first}";
                    }
                }
                if (tcmdnum == totalcomands - 1)
                {
                    if (postset.last != -1)
                    {
                        lval = $"last={postset.last}";
                    }
                }
                if (fval != string.Empty || lval != string.Empty)
                {
                    sb.Append("::postset(");
                    if (fval != string.Empty)
                    {
                        sb.Append(fval);
                        if (lval != string.Empty)
                        {
                            sb.Append(", ");
                        }
                    }
                    if (lval != string.Empty)
                    {
                        sb.Append(lval);
                    }
                    sb.Append(")");
                }

                sb.AppendLine();
            }

            return sb.ToString();

        }
    }
}
