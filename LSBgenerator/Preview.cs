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
    public partial class Preview : Form
    {
        public Preview()
        {
            InitializeComponent();
        }

        public void SetImage(Bitmap b)
        {
            Width = b.Width + 16;
            Height = b.Height + 40;
            pbPreview.Image = b;
            pbPreview.SizeMode = PictureBoxSizeMode.AutoSize;

        }

        public Graphics GetGraphics()
        {
            return pbPreview.CreateGraphics();
        }
    }
}
