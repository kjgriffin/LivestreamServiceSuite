using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LutheRun
{
    class LSBElementReading : ILSBElement
    {

        public string ReadingTitle { get; private set; } = "";
        public string ReadingReference { get; private set; } = "";
        public string PreLiturgy { get; private set; } = "";
        public string PostLiturgy { get; private set; } = "";

        public static ILSBElement Parse(IElement element)
        {
            var caption = LSBElementCaption.Parse(element.Children.First(e => e.ClassList.Contains("lsb-caption"))) as LSBElementCaption;

            string preliturgy = "";
            string postliturgy = "";

            foreach (var child in element.Children.Where(e => e.LocalName == "lsb-content"))
            {
                bool pre = true;
                foreach (var p in child.Children)
                {
                    if (p.ClassList.Contains("lsb-responsorial"))
                    {
                        if (pre)
                        {
                            preliturgy += " " + p.StrippedText();
                        }
                        else
                        {
                            postliturgy += " " + p.StrippedText();
                        }
                    }
                    else
                    {
                        pre = false;
                    }
                }
            }

            LSBElementReading reading = new LSBElementReading();
            reading.ReadingTitle = caption.Caption;
            reading.ReadingReference = caption.SubCaption;
            reading.PreLiturgy = preliturgy;
            reading.PostLiturgy = postliturgy;

            return reading;
        }

        public string DebugString()
        {
            return $"/// XENON DEBUG::Parsed as LSB_ELEMENT_READING. Title: {ReadingTitle} Reference: {ReadingReference} PreLiturgy: {PreLiturgy} PostLiturgy: {PostLiturgy}";
        }

        public string XenonAutoGen()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("/// <XENON_AUTO_GEN>");
            if (PreLiturgy.Trim() != string.Empty)
            {
                sb.AppendLine($"#liturgy{{\r\n{PreLiturgy}\r\n}}");
            }
            sb.AppendLine($"#reading(\"{ReadingTitle}\", \"{ReadingReference}\")");
            if (PostLiturgy.Trim() != string.Empty)
            {
                sb.AppendLine($"#liturgy{{\r\n{PostLiturgy}\r\n}}");
            }
            sb.AppendLine("/// </XENON_AUTO_GEN>");
            return sb.ToString();
        }
    }
}
