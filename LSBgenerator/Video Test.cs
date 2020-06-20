using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using WpfControlLibrary1;
using System.Collections;
using System.IO;

namespace LSBgenerator
{
    public partial class Video_Test : Form
    {

        UserControl1 _videoPlayer;
        public Video_Test()
        {
            InitializeComponent();
            _videoPlayer = (UserControl1)elementHost1.Child;
        }

        public void play()
        {
            pictureBox1.Visible = false;
            _videoPlayer.Visibility = System.Windows.Visibility.Visible;
            _videoPlayer.SetVideo(new Uri(@"D:\\shared\\june21\\Majest Medley - Jack Kahl.mp4"));
            _videoPlayer.PlayVideo();
        }


        List<(Uri path, string type)> _slides;
        int slidenum;

        public void startPresentation(List<(Uri path, string type)> slides)
        {
            _slides = slides;
            slidenum = 0;
            showSlide();
        }

        public void nextSlide()
        {
            slidenum++;
            if (slidenum > _slides.Count - 1)
            {
                slidenum = _slides.Count - 1;
            }
            showSlide();
        }

        public void showSlide()
        {
            (Uri path, string type) slide = _slides[slidenum];
            if (slide.type != "video")
            {
                _videoPlayer.Visibility = System.Windows.Visibility.Hidden;
                pictureBox1.Image = Image.FromFile(slide.path.LocalPath);
                pictureBox1.Visible = true;
            }
            else
            {
                _videoPlayer.Visibility = System.Windows.Visibility.Visible;
                pictureBox1.Visible = false;
                _videoPlayer.SetVideo(slide.path);
                _videoPlayer.PlayVideo();
            }
        }

        public void prevSlide()
        {
            slidenum -= 1;
            if (slidenum < 0)
            {
                slidenum = 0;
            }
            showSlide();
        }



    }
}
