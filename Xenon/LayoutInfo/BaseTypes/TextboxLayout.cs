using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.Json.Serialization;

using Xenon.LayoutInfo.Serialization;
using Xenon.Layouts;

namespace Xenon.LayoutInfo.BaseTypes
{

    internal class TextboxLayout
    {
        public LWJRect Textbox { get; set; }
        public LWJFont Font { get; set; }
        public LWJColor FColor { get; set; }
        public LWJColor BColor { get; set; }
        public LWJColor KColor { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LWJHAlign HorizontalAlignment { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LWJVAlign VerticalAlignment { get; set; }

        public StringFormat GetTextAlignment()
        {
            StringFormat f = new StringFormat();
            SetHAlign(f);
            SetValign(f);
            return f;
        }

        private void SetHAlign(StringFormat f)
        {
            switch (HorizontalAlignment)
            {
                case LWJHAlign.Left:
                    f.Alignment = StringAlignment.Near;
                    break;
                case LWJHAlign.Center:
                    f.Alignment = StringAlignment.Center;
                    break;
                case LWJHAlign.Right:
                    f.Alignment = StringAlignment.Far;
                    break;
                default:
                    f.Alignment = StringAlignment.Near;
                    break;
            }
        }

        private void SetValign(StringFormat f)
        {
            switch (VerticalAlignment)
            {
                case LWJVAlign.Top:
                    f.LineAlignment = StringAlignment.Near;
                    break;
                case LWJVAlign.Center:
                    f.LineAlignment = StringAlignment.Center;
                    break;
                case LWJVAlign.Bottom:
                    f.LineAlignment = StringAlignment.Far;
                    break;
                default:
                    break;
            }
        }

        public StringFormat GetHTextAlignment()
        {
            StringFormat f = new StringFormat();
            SetHAlign(f);
            return f;

        }

        public StringFormat GetVTextAlignment()
        {
            StringFormat f = new StringFormat();
            SetValign(f);
            return f;
        }

    }
}
