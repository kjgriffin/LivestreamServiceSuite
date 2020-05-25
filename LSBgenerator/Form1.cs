using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LSBgenerator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        TextRenderer renderer;
        public Font DrawingFont { get; set; }
        private void Form1_Load(object sender, EventArgs e)
        {
            DrawingFont = Font;
            try
            {
                DrawingFont = new Font("Arial", 36, FontStyle.Regular);
            }
            finally
            {

            }
            CreateRenderer();
        }

        private void CreateRenderer()
        {
            renderer = new TextRenderer(1920, 1080, (int)(1920 * nWidth.Value / 100), (int)(1080 * nHeight.Value / 100), (int)nPaddingLeft.Value, (int)nPaddingRight.Value, (int)nPaddingCol.Value, (int)nPaddingTop.Value, (int)nPaddingBottom.Value, DrawingFont);
            td = new TextData();
            UpdateLayoutPreview();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Preview pv = new Preview();
            UpdateLayoutPreview();
            pv.SetImage(renderer.bmp);
            pv.Show();
        }


        TextData td;
        private void button3_Click(object sender, EventArgs e)
        {
            td = new TextData();
            td.ParseText(tbinput.Text);

            //Bitmap bb = new Bitmap(renderer.DisplayWidth, renderer.DisplayHeight);
            //td.Render_PreviewText(Graphics.FromImage(bb), renderer, DrawingFont);

            //pbTypeset.Image = bb;
            //pbTypeset.SizeMode = PictureBoxSizeMode.StretchImage;

            renderer.Typeset_Text(td);

            lbSlides.Items.Clear();

            foreach (var s in renderer.Slides)
            {
                lbSlides.Items.Add($"{s.Order}");
            }

            renderer.RenderSlides();

            lbSlides.SelectedIndex = 0;

        }

        private void button5_Click(object sender, EventArgs e)
        {
            FontDialog fd = new FontDialog();
            fd.Font = DrawingFont;
            fd.ShowDialog();
            DrawingFont = fd.Font;
            CreateRenderer();
            lbSlides.Items.Clear();
        }

        private void button6_Click(object sender, EventArgs e)
        {

            ShowSlideRendered();

        }

        private void button7_Click(object sender, EventArgs e)
        {
            CreateRenderer();
        }

        private void UpdateLayoutPreview()
        {
            //CreateRenderer();
            renderer.Render_LayoutPreview(td, DrawingFont);
            pictureBox1.Image = renderer.bmp;
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ShowSlidePreview();
        }

        private void ShowSlidePreview()
        {
            int slidenum = 0;
            try
            {
                if (lbSlides.SelectedItem != null)
                {
                    slidenum = int.Parse(lbSlides.SelectedItem.ToString());
                }
                // render the slide
                //renderer.RenderSlides();
                pbTypeset.SizeMode = PictureBoxSizeMode.StretchImage;
                pbTypeset.Image = renderer.Slides[slidenum].rendering;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void ShowSlideRendered()
        {
            int slidenum = 0;
            try
            {
                Preview wnd = new Preview();
                if (lbSlides.SelectedItem != null)
                {
                    slidenum = int.Parse(lbSlides.SelectedItem.ToString());
                }
                // render the slide
                //renderer.RenderSlides();
                wnd.SetImage(renderer.Slides[slidenum].rendering);
                wnd.Show();
            }
            catch (Exception)
            {
                throw;
            }

        }

        private void lbSlides_SelectedValueChanged(object sender, EventArgs e)
        {
            ShowSlidePreview();
        }
    }
}
