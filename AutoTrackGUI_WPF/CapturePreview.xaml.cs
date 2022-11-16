using OpenCvSharp;
using OpenCvSharp.ImgHash;
using OpenCvSharp.WpfExtensions;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace AutoTrackGUI_WPF
{
    /// <summary>
    /// Interaction logic for CapturePreview.xaml
    /// </summary>
    public partial class CapturePreview : UserControl
    {
        DispatcherTimer _dtimer;
        VideoCapture capture;
        CascadeClassifier classifier;
        Mat cframeA = new Mat();
        Mat cframeB = new Mat();
        Mat cframe = new Mat();
        public CapturePreview()
        {
            InitializeComponent();
            //Task.Run(Work);
            _dtimer = new DispatcherTimer();
            _dtimer.Tick += _dtimer_Tick;
            _dtimer.Interval = new TimeSpan(0, 0, 1 / 30);
            Task.Run(() =>
            {
                capture = new VideoCapture(0, VideoCaptureAPIs.ANY);
                capture.FrameWidth = 1920;
                capture.FrameHeight = 1080;
                classifier = new CascadeClassifier("haarcascade_frontalface_default.xml");

                Dispatcher.Invoke(() => _dtimer.Start());
            });
        }

        private async void _dtimer_Tick(object? sender, EventArgs e)
        {

            BitmapImage img = null;
            capture.Read(cframe);

            OpenCvSharp.Rect[] objs = new OpenCvSharp.Rect[0];

            if (!cframe.Empty())
            {
                cframe.CopyTo(cframeB);
                //Cv2.CvtColor(cframe, cframeA, ColorConversionCodes.RGB2GRAY);

                objs = classifier.DetectMultiScale(cframe);

                foreach (var obj in objs)
                {
                    cframe.Rectangle(obj, Scalar.Red, 2, LineTypes.Link4, 0);
                }

                var stream = cframe.ToMemoryStream();
                img = ToBitmapFromPngMemoryStream(stream);

            }
            imgFrame.Source = img;
        }

        private async Task Work()
        {

            VideoCapture capture = new VideoCapture(0, VideoCaptureAPIs.ANY);
            //VideoCapture capture = new VideoCapture(0, VideoCaptureAPIs.DSHOW);
            capture.FrameWidth = 1920;
            capture.FrameHeight = 1080;

            capture.Fps = 30;
            //capture.AutoExposure = 0;
            //capture.Format = OpenCvSharp.FourCC.MPG4;
            //capture.FrameWidth = 1920;
            //capture.FrameHeight = 1080;

            //Dispatcher.Invoke(() =>
            //{
            //imgFrame.Width = 1280;
            //imgFrame.Height = 720;
            //});

            CascadeClassifier classifier = new CascadeClassifier("haarcascade_frontalface_default.xml");
            Mat cframeA = new Mat();
            Mat cframe = new Mat();


            int ticks = 0;
            int fps = 0;
            int second = DateTime.Now.Second;

            double rollingAverage = 0;

            Stopwatch swTimer = new Stopwatch();
            WriteableBitmap bmp = null;

            while (true)
            {
                var lastRun = swTimer.ElapsedMilliseconds;
                rollingAverage = (rollingAverage + lastRun) / 2;
                swTimer.Restart();

                capture.Read(cframe);

                //swTimer.Stop();

                OpenCvSharp.Rect[] objs = new OpenCvSharp.Rect[0];


                bool goodFrame = false;
                if (!cframe.Empty())
                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Task.Run(() =>
                    {
                        // frame to greyscale
                        //cframe.CopyTo(cframeA);

                        Mat cframeB = new Mat();
                        cframe.CopyTo(cframeB);
                        Cv2.CvtColor(cframe, cframeA, ColorConversionCodes.RGB2GRAY);

                        objs = classifier.DetectMultiScale(cframeA);

                        foreach (var obj in objs)
                        {
                            cframeA.Rectangle(obj, Scalar.Red, 2, LineTypes.Link4, 0);
                        }
                        //});
                        //Task.Run(() =>
                        //{
                        BitmapImage img = null;

                        var stream = cframeA.ToMemoryStream();
                        img = ToBitmapFromPngMemoryStream(stream);


                        //await Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, () =>
                        //Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, () =>
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, () =>
                        {
                            imgFrame.Source = img;
                        });
                    });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                    ticks++;
                    goodFrame = true;
                }

                swTimer.Stop();

                if (!goodFrame)
                {
                    int delay = (int)Math.Max((1000 / 15) - swTimer.ElapsedMilliseconds, 1);
                    await Task.Delay(delay);
                }

                var now = DateTime.Now.Second;
                if (second != now)
                {
                    fps = ticks;
                    ticks = 0;
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, () =>
                    {
                        lbFPS.Text = $"FPS: {fps}";
                        lbRTMS.Text = $"RTMS: {rollingAverage:0.00}";
                    });
                }
                second = now;
            }


        }

        public BitmapImage ToBitmapFromPngMemoryStream(MemoryStream ms)
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            image.Freeze(); // allow it to be used on a thread it wasn't created on
            return image;
        }



    }
}
