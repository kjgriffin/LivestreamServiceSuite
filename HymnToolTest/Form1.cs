using Emgu.CV;
using Emgu.CV.Cuda;
using Emgu.CV.Dnn;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;

using LSBHymnTool;
using LSBHymnTool.MeterSearch;

using System.Drawing;

namespace HymnToolTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        List<LSBMeterSearchAttributes> alts = new List<LSBMeterSearchAttributes>();
        private async void button1_Click(object sender, EventArgs e)
        {
            var hymnID = await LSBAPI.GetLSBHymnID(tbHymnNumber.Text);

            var info = await LSBAPI.GetLSBHymnMetaInfo(hymnID);
            // put in a list of all related hymns
            tbInfo.Clear();

            var display = $"TITLE: {info.name} LSB NUMBER:{info.number} TUNE: {info.tunes.FirstOrDefault().name} METER: {info.tunes.FirstOrDefault().meter} KEY: {info.tunes.FirstOrDefault().key}";
            tbInfo.Text = display;

            var others = await LSBAPI.GetLSBAltTunesByMeter(info.tunes.FirstOrDefault().meter);
            lbAltTunes.Items.Clear();
            alts.Clear();
            foreach (var item in others)
            {
                alts.Add(item);
                lbAltTunes.Items.Add($"{item.number} {item.name}");
            }

            var hymntext = await LSBAPI.GetLSBHymnText(hymnID);
            tbText.Text = string.Join(Environment.NewLine, hymntext.Verses.Select(x => $"{x.StanzaId} {x.Words}"));

            var urls = await LSBAPI.GetLSBHymnImageURLs(hymnID);

            pbSrc.Image = null;
            pbAlt.Image = null;

            List<Bitmap> images = new List<Bitmap>();
            foreach (var url in urls)
            {
                images.Add(await LSBAPI.GetLSBImageFromURL(url));
            }

            CompileImages(images, pbSrc);
        }

        private void CompileImages(List<Bitmap> images, PictureBox display)
        {
            // draw all images into pb
            Bitmap compiled = new Bitmap(images.Max(x => x.Width), images.Sum(x => x.Height));
            using (Graphics gfx = Graphics.FromImage(compiled))
            {
                int y = 0;
                foreach (var img in images)
                {
                    gfx.DrawImage(img, 0, y, img.Width, img.Height);
                    y += img.Height;
                }
            }
            display.SizeMode = PictureBoxSizeMode.Zoom;
            display.Image = compiled;
        }

        private Bitmap RedifyFirstFiveStaff(Bitmap input)
        {
            Bitmap copy = new Bitmap(input);
            using (Graphics gfx = Graphics.FromImage(copy))
            {
                int x = 10;
                int ysep = 0;
                int ydepth = 0;
                bool first = true;
                bool start = false;
                int lasty = 0;
                for (int y = 0; y < input.Height; y++)
                {
                    var pix = input.GetPixel(x, y);
                    if (pix.R == pix.G && pix.G == pix.B && pix.B == 0)
                    {
                        gfx.DrawLine(Pens.White, 0, y, copy.Width, y);
                        start = true;
                        if (first)
                        {
                            ydepth++;
                        }
                        if (ysep > 0)
                        {
                            first = false;
                        }
                        lasty = y;
                    }
                    else
                    {
                        if (first && start)
                        {
                            ysep++;
                        }
                    }
                }
                for (int i = 0; i < ydepth; i++)
                {
                    gfx.DrawLine(Pens.White, 0, lasty + ysep + i, copy.Width, lasty + ysep + i);
                }
            }
            SimpleBlobDetector detector = new SimpleBlobDetector(new SimpleBlobDetectorParams
            {
                FilterByCircularity = true,
                FilterByArea = true,
                MinCircularity = 0.5f,
                MaxCircularity = 2.2f,
                MinArea = 50,
                MaxArea = 2200,
                FilterByColor = true,
                blobColor = 0,
            });

            for (int y = 0; y < copy.Height; y++)
            {
                for (int x = 0; x < copy.Width; x++)
                {
                    var c = copy.GetPixel(x, y);
                    if (c.A == 0)
                    {
                        copy.SetPixel(x, y, Color.White);
                    }
                }
            }

            Image<Bgr, byte> myImage = copy.ToImage<Bgr, byte>();
            Image<Gray, Byte> grayImage = myImage.Convert<Gray, Byte>();
            var results = detector.Detect(myImage);
            var cresults = Emgu.CV.CvInvoke.HoughCircles(grayImage, Emgu.CV.CvEnum.HoughModes.Gradient, 1.5, 60, param1: 100, param2: 50, minRadius: 40, maxRadius: 80);

            //var gimg = grayImage.ToBitmap<Gray, byte>();
            var gimg = myImage.ToBitmap<Bgr, byte>();

            Bitmap bb = new Bitmap(gimg.Width, gimg.Height);

            using (Graphics gfx = Graphics.FromImage(bb))
            {
                gfx.DrawImage(gimg, 0, 0, gimg.Width, gimg.Height);
                foreach (var find in results)
                {
                    gfx.FillEllipse(Brushes.Red, find.Point.X - find.Size / 2, find.Point.Y - find.Size / 2, find.Size, find.Size);
                }
                foreach (var find in cresults)
                {
                    //gfx.FillEllipse(Brushes.LimeGreen, find.Center.X - find.Radius, find.Center.Y - find.Radius, find.Radius * 2, find.Radius * 2);
                }
            }

            return bb;
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            if (lbAltTunes.SelectedIndex != -1)
            {
                var selection = alts[lbAltTunes.SelectedIndex];

                var hymnID = await LSBAPI.GetLSBHymnID(selection.number);

                //var info = await LSBAPI.GetLSBHymnMetaInfo(hymnID);
                //var hymntext = await LSBAPI.GetLSBHymnText(hymnID);
                //tbText.Text = string.Join(Environment.NewLine, hymntext.Verses.Select(x => $"{x.StanzaId} {x.Words}"));

                var urls = await LSBAPI.GetLSBHymnImageURLs(hymnID);
                pbAlt.Image = null;
                List<Bitmap> images = new List<Bitmap>();
                foreach (var url in urls)
                {
                    images.Add(await LSBAPI.GetLSBImageFromURL(url));
                }
                CompileImages(images, pbAlt);


                List<Bitmap> test = new List<Bitmap>();
                foreach (var img in images)
                {
                    test.Add(RedifyFirstFiveStaff(img));
                }
                CompileImages(test, pbTest);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Bitmap b = new Bitmap(tbfile.Text.Replace("\"", ""));
            pbDefault.Image = b;
            pbDefault.SizeMode = PictureBoxSizeMode.StretchImage;
            pbCV.Image = RedifyFirstFiveStaff(b);
            pbCV.SizeMode = PictureBoxSizeMode.StretchImage;
        }
    }
}