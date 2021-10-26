using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xenon.SlideAssembly
{
    public class TitledLiturgyVerseLayout
    {
        public Size Size { get; set; }
        public Rectangle Key { get; set; }
        public Rectangle Textbox { get; set; }
        public Rectangle TitleLine { get; set; }
        [JsonIgnore]
        public Font Font { get; set; }

        public TitledLiturgyVerseLayout()
        {
            Font = new Font("Arial", 36, FontStyle.Regular);
        }

        public static TitledLiturgyVerseLayout FromJSON(string json)
        {
            LWJTLVerseLayout layout = JsonSerializer.Deserialize<LWJTLVerseLayout>(json);

            TitledLiturgyVerseLayout res = new TitledLiturgyVerseLayout();
            res.Size = new Size(layout.Size.Width, layout.Size.Height);
            res.Key = new Rectangle(layout.Key.Origin.X, layout.Key.Origin.Y, layout.Key.Size.Width, layout.Key.Size.Height);
            res.Textbox = new Rectangle(layout.Textbox.Origin.X, layout.Textbox.Origin.Y, layout.Textbox.Size.Width, layout.Textbox.Size.Height);
            res.TitleLine = new Rectangle(layout.TitleLine.Origin.X, layout.TitleLine.Origin.Y, layout.TitleLine.Size.Width, layout.TitleLine.Size.Height);

            try
            {
                res.Font = new Font(layout.Font.Name, layout.Font.Size, (FontStyle)layout.Font.Style);
            }
            catch
            {
                res.Font = new Font("Arial", 36, FontStyle.Regular);
            }

            return res;
        }

    }

    class LWJPoint
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    class LWJSize
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }

    class LWJRect
    {
        public LWJPoint Origin { get; set; }
        public LWJSize Size { get; set; }
    }

    class LWJFont
    {
        public string Name { get; set; }
        public int Size { get; set; }
        public int Style { get; set; }
    }

    class LWJTLVerseLayout
    {
        public LWJSize Size { get; set; }
        public LWJRect Key { get; set; }
        public LWJRect Textbox { get; set; }
        public LWJRect TitleLine { get; set; }
        public LWJFont Font { get; set; }
    }


}
