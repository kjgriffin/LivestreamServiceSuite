using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LSBgenerator
{
    public partial class MainWindow_1 : Form
    {
        public MainWindow_1()
        {
            InitializeComponent();
        }


        TextRenderer renderer;
        public Font DrawingFont { get; set; }
        public string DFName { get => DrawingFont.Name; }
        public string DFSize { get => DrawingFont.Size.ToString(); }

        ServiceProject proj = new ServiceProject();


        ImageList AssetImageList = new ImageList();
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
            tbFontName.Text = DrawingFont.Name;
            tbFontSize.Text = DrawingFont.Size.ToString();
            lvAssets.SmallImageList = AssetImageList;
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
            Typeset();
        }

        private void Typeset()
        {
            td = new TextData();
            //tbinput.Text = td.PreProcLine(tbinput.Text);
            td.ParseText(tbinput.Text, proj.Assets);

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
            tbFontName.Text = DrawingFont.Name;
            tbFontSize.Text = DrawingFont.Size.ToString();
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
                //throw;
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

        private void button4_Click_1(object sender, EventArgs e)
        {
            FontDialog fd = new FontDialog();
            fd.Font = tbinput.Font;
            fd.ShowDialog();
            tbinput.Font = fd.Font;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            SaveFileDialog savedialog = new SaveFileDialog();
            savedialog.Title = "Save Program Source";
            savedialog.Filter = "Text File|*.txt";
            savedialog.FileName = DateTime.Now.ToString("yyyyMMdd") + "_LSBService";
            savedialog.ShowDialog();
            if (savedialog.FileName != "")
            {
                using (StreamWriter tw = new StreamWriter(savedialog.OpenFile()))
                {
                    tw.Write(tbinput.Text);
                }
            }

        }

        private void button13_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Open Program Source";
            openFileDialog.Filter = "Text|*.txt";
            openFileDialog.ShowDialog();
            if (openFileDialog.FileName != "")
            {
                using (StreamReader rdr = new StreamReader(openFileDialog.OpenFile()))
                {
                    tbinput.Text = rdr.ReadToEnd();
                }
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Title = "Select Image Asset";
            openFileDialog.Filter = "Image Files(*.BMP;*.JPG;*.PNG)|*.BMP;*.JPG;*.PNG";
            openFileDialog.ShowDialog();

            foreach (var filename in openFileDialog.FileNames)
            {
                // based on filetype do different things

                string ext = Path.GetExtension(filename);

                ProjectAsset asset = new ProjectAsset();
                asset.Type = ext;
                asset.Name = Path.GetFileNameWithoutExtension(filename);
                asset.ResourcePath = filename;
                // get image
                asset.Image = (Bitmap)Image.FromFile(filename);

                proj.Assets.Add(asset);

                AddProjectAsset(asset);
            }

        }

        private void AddProjectAsset(ProjectAsset asset)
        {
            // add asset to assetbox
            lvAssets.SmallImageList.ImageSize = new Size(100, 100);
            lvAssets.SmallImageList.Images.Add(asset.guid.ToString(), asset.Image);
            ListViewItem i = new ListViewItem();
            i.ImageKey = asset.guid.ToString();
            i.Text = "";
            i.SubItems.Add(asset.Type);
            i.SubItems.Add(asset.Name);
            i.SubItems.Add(asset.guid.ToString());
            lvAssets.Items.Add(i);
        }

        private void button15_Click(object sender, EventArgs e)
        {
            if (lvAssets.SelectedItems.Count == 1)
            {
                Preview wnd = new Preview();
                wnd.SetImage(proj.Assets.First(a => a.Name == lvAssets.SelectedItems[0].SubItems[2].Text).Image);
                wnd.Show();
            }
        }

        private void button17_Click(object sender, EventArgs e)
        {
            // add slides to project
            proj.SourceText = tbinput.Text;

            // assume assets to have been loaded already
            // assume renderer to have been created
            proj.Layout = renderer.GetLayoutParams();


            // save project

            SaveFileDialog saveproj = new SaveFileDialog();
            saveproj.Title = "Save Project";
            saveproj.FileName = DateTime.Now.ToString("yyyyMMdd") + "_Proj.lsbproj";
            saveproj.Filter = "Project|*.lsbproj";
            saveproj.ShowDialog();

            if (saveproj.FileName != "")
            {
                proj.Serialize(saveproj.FileName);
            }


        }

        private void button11_Click(object sender, EventArgs e)
        {
            // save project assets
            SaveFileDialog saveassets = new SaveFileDialog();
            saveassets.Title = "Save Asset Group";
            saveassets.FileName = DateTime.Now.ToString("yyyyMMdd") + "_Assets.bal";
            saveassets.Filter = "Asset Set|*.bal";
            saveassets.ShowDialog();

            if (saveassets.FileName != "")
            {
                AssetListSerilizer.Serialize(saveassets.FileName, proj.Assets);
            }


        }

        private void button9_Click(object sender, EventArgs e)
        {
            OpenFileDialog openassets = new OpenFileDialog();
            openassets.Title = "Open Asset Group";
            openassets.Filter = "Asset Sets|*.bal";
            openassets.ShowDialog();

            if (openassets.FileName != "")
            {
                proj.Assets = AssetListSerilizer.Deserialize(openassets.FileName);
            }
            lvAssets.Items.Clear();
            foreach (var a in proj.Assets)
            {
                AddProjectAsset(a);
            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            OpenFileDialog openproj = new OpenFileDialog();
            openproj.Title = "Open Project";
            openproj.Filter = "Project|*.lsbproj";
            openproj.ShowDialog();

            if (openproj.FileName != "")
            {
                proj = ServiceProject.Deserialize(openproj.FileName);

                // create new renderer
                renderer = new TextRenderer(proj.Layout);

                nHeight.Value = (int)(renderer.DisplayHeight / 1080 * 100);
                nWidth.Value = (int)(renderer.DisplayWidth / 1920 * 100);
                nPaddingLeft.Value = renderer.PaddingLeft;
                nPaddingRight.Value = renderer.PaddingRight;
                nPaddingTop.Value = renderer.PaddingTop;
                nPaddingBottom.Value = renderer.PaddingBottom;
                nPaddingCol.Value = renderer.PaddingCol;
                // load assets
                lvAssets.Items.Clear();
                foreach (var a in proj.Assets)
                {
                    AddProjectAsset(a);
                }
                // set text
                tbinput.Text = proj.SourceText;
                // update layout
                UpdateLayoutPreview();

                // render slides
                Typeset();

            }
        }

        private void button18_Click(object sender, EventArgs e)
        {

            // select folder to save into
            //FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            //folderBrowser.ShowDialog();

            SaveFileDialog savefiles = new SaveFileDialog();
            savefiles.Title = "Save Stillframes";
            savefiles.Filter = "Bitmap|*.BMP|PNG|*.PNG|JPEG|*.JPG";
            savefiles.FilterIndex = 2;
            savefiles.FileName = "slide_#";
            savefiles.ShowDialog();

            if (savefiles.FileName != "")
            {
                ImageFormat format;
                switch (savefiles.FilterIndex)
                {
                    case 0:
                        format = ImageFormat.Bmp;
                        break;
                    case 1:
                        format = ImageFormat.Jpeg;
                        break;
                    case 2:
                        format = ImageFormat.Png;
                        break;
                    default:
                        format = ImageFormat.Png;
                        break;
                }
                StillFrameRenderer sfr = new StillFrameRenderer() { Format = format };

                List<Bitmap> slides = renderer.Slides.Select(p => p.rendering).ToList();

                sfr.ExportStillFrames(Path.GetDirectoryName(savefiles.FileName), slides);

            }

        }

        private void button12_Click(object sender, EventArgs e)
        {
            PowerPointRenderer powerPointRenderer = new PowerPointRenderer();
            SaveFileDialog saveppt = new SaveFileDialog();
            saveppt.Title = "Save to Powerpoint";
            saveppt.Filter = "Powerpoint|*.pptx";
            saveppt.ShowDialog();

            if (saveppt.FileName == "")
            {
                return;
            }

            OpenFileDialog openfiles = new OpenFileDialog();
            openfiles.Title = "Open Stills";
            openfiles.ShowDialog();

            if (openfiles.FileName == "")
            {
                return;
            }

            // get all the png files from openfile directory
            List<string> files = new List<string>();

            string path = Path.GetDirectoryName(openfiles.FileName);

            files = Directory.GetFiles(path, "*.png").ToList();

            powerPointRenderer.RenderStillsToPowerpoint(saveppt.FileName, files);
        }

        private void button19_Click(object sender, EventArgs e)
        {
            Display d = new Display();
            d.FormBorderStyle = FormBorderStyle.None;
            d.WindowState = FormWindowState.Maximized;
            d.Show();
        }

        private void button20_Click(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveFileDialog sf = new SaveFileDialog();
            sf.Title = "Save Luma Key";
            sf.Filter = "PNG|*.PNG";
            sf.FileName = "LumaKey";
            sf.ShowDialog();
            if (sf.FileName != "")
            {
                Bitmap bmp = renderer.Render_LayoutLumaKey();
                FileStream fs = new FileStream(sf.FileName, FileMode.Create);
                bmp.Save(fs, ImageFormat.Png);
                fs.Close();
            }
        }

        private void button19_Click_1(object sender, EventArgs e)
        {
            // copies renderer params
            renderer = new TextRenderer(renderer.GetLayoutParams());
            lvAssets.Items.Clear();
            proj.Assets.Clear();
            proj.Layout = renderer.GetLayoutParams();
            proj.SourceText = "";
            tbinput.Text = "";
            lbSlides.Items.Clear();
            pbTypeset.Image = renderer.bmp;
        }

        private void button20_Click_1(object sender, EventArgs e)
        {
            proj.Assets.RemoveAll(a => a.guid.ToString() == lvAssets.SelectedItems[0].SubItems[3].ToString());
        }

        private void button14_Click(object sender, EventArgs e)
        {
            //TextData.ShowCommands();
            CommandHelpList chl = new CommandHelpList(TextData.ListCommands());
            chl.Show();
        }

        private void button20_Click_2(object sender, EventArgs e)
        {

        }
    }
}
