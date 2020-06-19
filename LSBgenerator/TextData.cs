using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace LSBgenerator
{


    [Serializable]
    public class TextData
    {

        public List<ITypesettable> LineData = new List<ITypesettable>();


        public string PreProcLine(string line)
        {
            // replace non-frivalous newlines with forced newlines
            return Regex.Replace(line, @"(?<delimiter>[!?.,:;])(?<newline>[\n\r]+)(?<start>[A-Z])", @"${delimiter}\\${newline}${start}", RegexOptions.Multiline);
        }


        public static void ShowCommands()
        {
            MessageBox.Show(@"
\wrap
\break

\apostlescreed
\lordsprayer

\copyright
\viewseries
\viewservices

\reading(a, b)
\sermon(a, b)

\image(a)
\fullimage(a)
\fitimage(a)
", "Supported Commands", MessageBoxButtons.OK);
        }

        public static List<string> ListCommands()
        {
            return new List<string>()
            {
                @"\wrap",
                @"\break",
                @"\apostlescreed",
                @"\lordsprayer",
                @"\copyright",
                @"\viewservices",
                @"\viewseries",
                @"\reading(a, b)",
                @"\sermon(a, b)",
                @"\image(a)",
                @"\fullimage(a)",
                @"\fitimage(a)",
            };
        }

        public void ParseText(string text, List<ProjectAsset> assets)
        {

            text = PreProcLine(text);

            // preprocessor to make linewraping more convenient
            text = text.Replace(@"\wrap", @"\\\wrap\\")
                .Replace(@"\break", @"\\\break\\")
                //.Replace(@"\reading", @"\\\reading")
                //.Replace(@"\sermon", @"\\\sermon")
                //.Replace(@"\image", @"\\\image")
                //.Replace(@"\fullimage", @"\\\fullimage")
                //.Replace(@"\fitimage", @"\\\fitimage")
                .Replace(@"\apostlescreed", @"\\\apostlescreed\\")
                .Replace(@"\nicenecreed", @"\\\nicenecreed")
                .Replace(@"\lordsprayer", @"\\\lordsprayer\\")
                .Replace(@"\morningprayer", @"\\\morningprayer")
                .Replace(@"\copyright", @"\\\copyright\\")
                .Replace(@"\viewseries", @"\\\viewseries\\")
                .Replace(@"\viewservices", @"\\\viewservices\\")
                .Replace(@"\hymn", @"\\\hymn");

            // replace functions
            text = Regex.Replace(text, @"(?<funct>\\\w+\(.*\))", @"\\\\${funct}\\\\", RegexOptions.Multiline);

            var lines = text.Split(new[] { @"\\" }, StringSplitOptions.RemoveEmptyEntries).ToList();

            // try to intelligently split lines by newlines after punctuation

            // remove 'empty' lines (new lines)
            //lines = lines.Where(s => s != "\n")
                //.Where(s => s != "\r\n")
                //.Where(s => s != Environment.NewLine).ToList();

            lines = lines.Where(s => !Regex.Match(s, @"^\s$", RegexOptions.None).Success).ToList();
            lines = lines.Where(s => s.Trim() != string.Empty).Select(s => s.Trim()).ToList();




            foreach (var line in lines)
            {
                string tl = line.Trim().Replace(Environment.NewLine, " ").Replace("\n", " ").Replace("\r", " ").Replace("\r\n", " ");

                if (tl.StartsWith(@"\break"))
                {
                    TypesetCommand cmd = new TypesetCommand() { Command = Command.NewSlide };
                    LineData.Add(cmd);
                    continue;
                }

                if (tl.StartsWith(@"\wrap"))
                {
                    TypesetCommand cmd = new TypesetCommand() { Command = Command.WrapSpeakerText };
                    LineData.Add(cmd);
                    continue;
                }

                // typeset the reading on a new slide
                // expected params are comma delimited
                // eg. \reading(<Title>,<Reference>)
                if (tl.StartsWith(@"\reading"))
                {
                    //var contents = tl.Split(new char[] { '(', ',', ')' }, 4, StringSplitOptions.RemoveEmptyEntries);
                    var a = tl.Split('(')[1];
                    var b = a.Split(')')[0];
                    var contents = b.Split(new char[] { ',' }, 2, StringSplitOptions.RemoveEmptyEntries);


                    ReadingLine rl = new ReadingLine() { Title = contents[0].Trim(), Reference = contents[1].Trim() };
                    LineData.Add(rl);
                    continue;
                }

                if (tl.StartsWith(@"\sermon"))
                {
                    var matches = Regex.Match(tl, @"\\sermon\((?<title>.*),(?<source>.*)\)");
                    SermonTitle sermonTitle = new SermonTitle() { Title = "Sermon", SermonName = matches.Groups["title"].Value, SermonText = matches.Groups["source"].Value };
                    LineData.Add(sermonTitle);
                    continue;
                }

                if (tl.StartsWith(@"\image"))
                {
                    var args = tl.Split('(', ',', ')');
                    InlineImage inlineImage = new InlineImage();
                    inlineImage.ImageAsset = assets.Find(a => a.Name == args[1]);
                    inlineImage.AutoScale = args[2] == "fill";
                    LineData.Add(inlineImage);
                    continue;
                }

                if (tl.StartsWith(@"\fullimage"))
                {
                    var args = tl.Split('(', ')');
                    Fullimage img = new Fullimage();
                    img.ImageAsset = assets.Find(a => a.Name == args[1]);
                    LineData.Add(img);
                    continue;
                }

                if (tl.StartsWith(@"\fitimage"))
                {
                    var args = tl.Split('(', ')');
                    Fullimage img = new Fullimage();
                    img.ImageAsset = assets.Find(a => a.Name == args[1]);
                    img.Streach = false;
                    LineData.Add(img);
                    continue;
                }

                if (tl.StartsWith(@"\apostlescreed"))
                {
                    Fullimage ac = new Fullimage();
                    ac.ImageAsset = new ProjectAsset();
                    ac.ImageAsset.Image = Properties.Resources.apostlescreed;
                    ac.ImageAsset.Name = "Apostle Creed - Prerendered";
                    ac.Streach = true;
                    LineData.Add(ac);
                    continue;
                }
                if (tl.StartsWith(@"\nicenecreed"))
                {

                    TypesetCommand cmd = new TypesetCommand() { Command = Command.NewSlide };
                    LineData.Add(cmd);
                    continue;
                }
                if (tl.StartsWith(@"\lordsprayer"))
                {
                    Fullimage ac = new Fullimage();
                    ac.ImageAsset = new ProjectAsset();
                    ac.ImageAsset.Image = Properties.Resources.lordsprayer;
                    ac.ImageAsset.Name = "Lords Prayer - Prerendered";
                    ac.Streach = true;
                    LineData.Add(ac);
                    continue;
                }
                if (tl.StartsWith(@"\viewseries"))
                {
                    Fullimage ac = new Fullimage();
                    ac.ImageAsset = new ProjectAsset();
                    ac.ImageAsset.Image = Properties.Resources.sessions;
                    ac.ImageAsset.Name = "View Series - Prerendered";
                    ac.Streach = true;
                    LineData.Add(ac);
                    continue;
                }
                if (tl.StartsWith(@"\viewservices"))
                {
                    Fullimage ac = new Fullimage();
                    ac.ImageAsset = new ProjectAsset();
                    ac.ImageAsset.Image = Properties.Resources.services;
                    ac.ImageAsset.Name = "View Services - Prerendered";
                    ac.Streach = true;
                    LineData.Add(ac);
                    continue;
                }
                if (tl.StartsWith(@"\copyright"))
                {
                    Fullimage ac = new Fullimage();
                    ac.ImageAsset = new ProjectAsset();
                    ac.ImageAsset.Image = Properties.Resources.copyright;
                    ac.ImageAsset.Name = "Copyright1 - Prerendered";
                    ac.Streach = true;
                    LineData.Add(ac);
                    continue;
                }
                if (tl.StartsWith(@"\morningprayer"))
                {

                    TypesetCommand cmd = new TypesetCommand() { Command = Command.NewSlide };
                    LineData.Add(cmd);
                    continue;
                }
                if (tl.StartsWith(@"\hymn"))
                {

                    TypesetCommand cmd = new TypesetCommand() { Command = Command.NewSlide };
                    LineData.Add(cmd);
                    continue;
                }

                LiturgyLine l = new LiturgyLine();
                if (tl.StartsWith("P"))
                {
                    l.Text = tl.Replace(" T ", " + ").Substring(1).Trim();
                    l.Speaker = Speaker.Pastor;
                }
                else if (tl.StartsWith("L"))
                {
                    l.Text = tl.Replace(" T ", " + ").Substring(1).Trim();
                    l.Speaker = Speaker.Leader;
                }
                else if (tl.StartsWith("C"))
                {
                    l.Text = tl.Replace(" T ", " + ").Substring(1).Trim();
                    l.Speaker = Speaker.Congregation;
                }
                else if (tl.StartsWith("A"))
                {
                    l.Text = tl.Replace(" T ", " + ").Substring(1).Trim();
                    l.Speaker = Speaker.Assistant;
                }
                else
                {
                    l.Text = tl.Trim();
                    l.Speaker = Speaker.None;
                }
                l.IsSubsplit = false;
                LineData.Add(l);
            }


        }



        StringFormat format = new StringFormat() { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near, Trimming = StringTrimming.Word };

        public void Render_PreviewText(Graphics gfx, TextRenderer r, Font f, bool clipLines = true)
        {


            int linestartX = 10 + (r.DisplayWidth - r.TextboxWidth) / 2;
            int linestartY = r.DisplayHeight - r.TextboxHeight;

            int speakerblocksize = (int)(f.Size * gfx.DpiX / 72);

            gfx.Clear(Color.Black);


            gfx.FillRectangle(Brushes.White, (r.DisplayWidth - r.TextboxWidth) / 2, r.DisplayHeight - r.TextboxHeight, r.TextboxWidth, r.TextboxHeight);



            // render in centered mode
            foreach (var l in LineData)
            {
                LiturgyLine ll;
                if (l is LiturgyLine)
                {
                    ll = l as LiturgyLine;
                }
                else
                {
                    return;
                }

                RectangleF displayaera = new RectangleF(3 + linestartX + speakerblocksize, linestartY + 3, r.TextboxWidth - speakerblocksize, r.TextboxHeight - 3);
                Font df;
                if (ll.Speaker == Speaker.Congregation)
                {
                    df = new Font(f, FontStyle.Bold);
                }
                else
                {
                    df = new Font(f, FontStyle.Regular);
                }

                if (clipLines)
                {
                    if (linestartY + (int)gfx.MeasureString(ll.Text, df, displayaera.Size).Height >= r.DisplayHeight)
                    {
                        return;
                    }
                }

                // draw speaker

                // compute lineheight of single character
                int lineheight = (int)Math.Ceiling(gfx.MeasureString("C", df).Height);
                int speakeroffsety = (int)Math.Floor((lineheight - speakerblocksize) / 2d) - 1;

                //gfx.DrawRectangle(Pens.Black, new Rectangle(linestartX, linestartY + speakeroffsety, speakerblocksize, speakerblocksize));
                Rectangle speakerblock = new Rectangle(linestartX, linestartY + speakeroffsety, speakerblocksize, speakerblocksize);


                if (ll.Speaker == Speaker.Congregation)
                {
                    Font cf = new Font(f, FontStyle.Bold);
                    gfx.FillPath(Brushes.Black, DrawRoundedRect(speakerblock, 2));
                    gfx.DrawString("C", cf, Brushes.White, speakerblock, format);
                }

                if (ll.Speaker == Speaker.Pastor)
                {
                    Font cf = new Font(f, FontStyle.Bold);
                    gfx.FillPath(Brushes.White, DrawRoundedRect(speakerblock, 2));
                    gfx.DrawPath(Pens.Black, DrawRoundedRect(speakerblock, 2));
                    gfx.DrawString("P", cf, Brushes.Black, speakerblock, format);
                }

                // account for offset of speaker graphic

                gfx.DrawString(ll.Text, df, Brushes.Black, displayaera, format);
                int yoffset = (int)gfx.MeasureString(ll.Text, df, new Size(r.TextboxWidth, r.TextboxHeight)).Height;
                linestartY += yoffset;

            }


        }

        private GraphicsPath DrawRoundedRect(Rectangle bounds, int borderradius)
        {

            int diameter = borderradius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (borderradius == 0)
            {
                path.AddRectangle(arc);
                return path;
            }

            path.AddArc(arc, 180, 90);

            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;

        }







    }
}
