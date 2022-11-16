using Accessibility;

using OpenCvSharp;
using OpenCvSharp.ImgHash;
using OpenCvSharp.WpfExtensions;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
        DispatcherTimer dtimer = new DispatcherTimer();


        const int DecodeThreads = 3;
        Thread[] decodeThreads = new Thread[DecodeThreads];

        public CapturePreview()
        {
            InitializeComponent();
            dtimer.Interval = TimeSpan.FromMilliseconds(15.69);
            dtimer.Tick += Dtimer_Tick;
            dtimer.Start();

            Task.Run(Work);

            for (int i = 0; i < DecodeThreads; i++)
            {
                decodeThreads[i] = new Thread(DecodeWork);
                decodeThreads[i].Start();
            }
        }

        int second = 0;
        int frames = 0;
        int fps = 0;
        private void Dtimer_Tick(object? sender, EventArgs e)
        {
            while (_doneFrames.TryDequeue(out var img))
            {
                frames++;
                imgFrame.Source = img;
            }
            var now = DateTime.Now.Second;
            if (now != second)
            {
                fps = frames;
                frames = 0;
            }
            second = now;
            lbFPS.Text = $"FPS: {fps}";
        }

        ConcurrentQueue<Mat> _toProcess = new ConcurrentQueue<Mat>();
        Semaphore _workSem = new Semaphore(0, int.MaxValue);
        ConcurrentQueue<BitmapImage> _doneFrames = new ConcurrentQueue<BitmapImage>();
        ManualResetEvent _doneMRE = new ManualResetEvent(false);
        long _inFlight = 0;

        private Task Work()
        {

            VideoCapture capture = new VideoCapture(0, VideoCaptureAPIs.ANY);
            //VideoCapture capture = new VideoCapture(0, VideoCaptureAPIs.DSHOW);
            capture.FrameWidth = 1920;
            capture.FrameHeight = 1080;

            //capture.Fps = 30;
            //capture.AutoExposure = 0;
            //capture.Format = OpenCvSharp.FourCC.MPG4;
            //capture.FrameWidth = 1920;
            //capture.FrameHeight = 1080;

            //Dispatcher.Invoke(() =>
            //{
            //imgFrame.Width = 1280;
            //imgFrame.Height = 720;
            //});


            Stopwatch stopwatch = Stopwatch.StartNew();

            while (true)
            {
                //var lastRun = swTimer.ElapsedMilliseconds;
                //rollingAverage = (rollingAverage + lastRun) / 2;

                if (Interlocked.Read(ref _inFlight) < DecodeThreads) // don't make more work than we can handle
                {

                    stopwatch.Restart();

                    Mat cframe = new Mat();
                    if (capture.Read(cframe))
                    {
                        // mark frame ready
                        Mat m = new Mat();
                        cframe.CopyTo(m);
                        _toProcess.Enqueue(m);
                        _workSem.Release(1);
                        Interlocked.Increment(ref _inFlight);
                    }
                }

                var ellapsed = stopwatch.ElapsedMilliseconds;
                stopwatch.Stop();

                var sleep = (int)Math.Max(33 - ellapsed, 0); // time for 1 frame 
                if (sleep > 0)
                {
                    Thread.Sleep(sleep);
                }

                //swTimer.Stop();

            }


        }

        private void DecodeWork()
        {
            CascadeClassifier classifier = new CascadeClassifier("haarcascade_frontalface_default.xml");


            Stopwatch swTimer = new Stopwatch();
            WriteableBitmap bmp = null;
            OpenCvSharp.Rect[] objs = new OpenCvSharp.Rect[0];


            while (true)
            {

                // wait for availbe frame to process
                _workSem.WaitOne();
                
                Interlocked.Decrement(ref _inFlight);

                Mat cframe;
                // get 'er out
                while (!_toProcess.TryDequeue(out cframe)) ;

                if (!cframe.Empty())
                {
                    Mat ccframe = new Mat();
                    cframe.CopyTo(ccframe);

                    objs = classifier.DetectMultiScale(ccframe);
                    foreach (var obj in objs)
                    {
                        ccframe.Rectangle(obj, Scalar.Red, 2, LineTypes.Link4, 0);
                    }

                    var stream = ccframe.ToMemoryStream();
                    var img = ToBitmapFromPngMemoryStream(stream);

                    // send it to done queue
                    _doneFrames.Enqueue(img);

                    //Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, () =>
                    //{
                    //    imgFrame.Source = img;
                    //});
                }

                //_workSem.Release();
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
