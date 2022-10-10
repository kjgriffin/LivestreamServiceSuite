using DirectShowLib;

using OpenCvSharp.Extensions;
using OpenCvSharp;

using System.Text.RegularExpressions;

using TestScreenshot;
using CameraDriver;
using System.Net;
using DVIPProtocol.Protocol.Lib.Command.PTDrive;

namespace AutoTrackerGUI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            _trackTimer = new System.Windows.Forms.Timer();
            _trackTimer.Interval = 800;
            _trackTimer.Tick += _trackTimer_Tick;
            _trackTimer.Enabled = true;

            this.DoubleBuffered = true;
            var inputs = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

            foreach (var input in inputs)
            {
                comboBox1.Items.Add(input.Name);
            }

            SetTrackParams();
        }

        private void _trackTimer_Tick(object? sender, EventArgs e)
        {
            if (_trackEnabled)
            {
                DoTrack();
            }
        }

        System.Windows.Forms.Timer _trackTimer;


        OpenCvSharp.VideoCapture capture;
        OpenCvSharp.CascadeClassifier classifier;


        CancellationTokenSource cts;
        Thread worker;

        CentroidTracker _tracker = new CentroidTracker();
        int devINDEX = 0;

        private void button1_Click(object sender, EventArgs e)
        {
            cts?.Dispose();
            cts = new CancellationTokenSource();
            worker = new Thread(DoWork);

            if (comboBox1.SelectedIndex >= 0)
            {
                devINDEX = comboBox1.SelectedIndex;
            }

            worker.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            cts.Cancel();
        }

        private void DoWork()
        {

            capture = new OpenCvSharp.VideoCapture();
            classifier = new OpenCvSharp.CascadeClassifier("haarcascade_frontalface_default.xml");



            capture.FrameHeight = 720;
            capture.FrameWidth = 1280;
            capture.Set(OpenCvSharp.VideoCaptureProperties.FrameHeight, 720);
            capture.Set(OpenCvSharp.VideoCaptureProperties.FrameWidth, 1280);
            capture.Open(devINDEX, OpenCvSharp.VideoCaptureAPIs.DSHOW);

            while (!cts.IsCancellationRequested)
            {


                var frameMat = capture.RetrieveMat();

                //frameMat = frameMat.Blur(new OpenCvSharp.Size(16, 16));

                var rects = classifier.DetectMultiScale(frameMat, 1.1, 7, OpenCvSharp.HaarDetectionTypes.ScaleImage | HaarDetectionTypes.DoCannyPruning | HaarDetectionTypes.DoRoughSearch | HaarDetectionTypes.FindBiggestObject);

                //var rects = classifier.DetectMultiScale(frameMat, 1.1, 7, OpenCvSharp.HaarDetectionTypes.ScaleImage);

                _tracker.UpdateTracks(rects);

                Bitmap bmp = frameMat.ToBitmap();
                Draw(bmp, _tracker.GetTracks());

                bmp.Dispose();
                frameMat.Dispose();

                Thread.Sleep(10);
            }
            capture.Release();
        }

        private void Draw(Bitmap bmp, List<CentroidTrack> tracks)
        {
            if (InvokeRequired)
            {
                Invoke(Draw, bmp, tracks);
                return;
            }
            using (Graphics g = CreateGraphics())
            using (Bitmap b = new Bitmap(Size.Width, Size.Height))
            using (Graphics gfx = Graphics.FromImage(b))
            {
                gfx.Clear(Color.White);
                gfx.DrawImage(bmp, new Rectangle(0, 0, 1280, 720), new Rectangle(0, 0, 1280, 720), GraphicsUnit.Pixel);

                foreach (var track in tracks)
                {
                    gfx.DrawRectangle(track.Stale ? Pens.Red : Pens.Green, new System.Drawing.Rectangle(track.Centroid.Bounds.X, track.Centroid.Bounds.Y, track.Centroid.Bounds.Width, track.Centroid.Bounds.Height));
                    gfx.FillRectangle(Brushes.Black, (int)track.Centroid.Center.X, (int)track.Centroid.Center.Y, 150, 25);
                    gfx.DrawString(track.Name, DefaultFont, Brushes.White, new System.Drawing.Point((int)track.Centroid.Center.X, (int)track.Centroid.Center.Y));
                }

                for (int i = 0; i < tracks.Count; i++)
                {
                    gfx.FillRectangle(Brushes.White, 10, 23 + 20 * i, 150, 20);
                    gfx.DrawString(tracks[i].Name, DefaultFont, Brushes.Black, new System.Drawing.Point(10, 20 + i * 20));
                }

                // draw target location
                gfx.FillEllipse(Brushes.Yellow, _tgtX - 5, _tgtY - 5, 10, 10);
                gfx.DrawRectangle(Pens.Yellow, _tgtX - _mX, _tgtY - _mY, _mX * 2, _mY * 2);


                //g.DrawImage(b, new RectangleF(10, 80, 1280, 720), new RectangleF(0, 0, 640, 480), GraphicsUnit.Pixel);
                g.DrawImage(b, 10, 80);
            }
        }



        IRobustCamServer _camServer = IRobustCamServer.InstantiateRobust(null);

        private void button3_Click(object sender, EventArgs e)
        {
            _camServer.Start();
            _camServer.StartCamClient("test", new System.Net.IPEndPoint(IPAddress.Parse("127.0.0.1"), 5002));
        }

        private void button4_Click(object sender, EventArgs e)
        {
            _camServer.Cam_RunDriveProgram("test", DVIPProtocol.Protocol.Lib.Command.PTDrive.PanTiltDirection.RIGHT, 10, 500);
        }


        bool _trackEnabled = false;

        int _tgtX = 0;
        int _tgtY = 0;
        int _mX = 0;
        int _mY = 0;

        private void DoTrack()
        {
            if (_trackEnabled)
            {
                var track = _tracker.GetTracks().FirstOrDefault();

                if (track != null && !track.Stale)
                {
                    // compute direction

                    PanTiltDirection dir = PanTiltDirection.STOP;

                    if (track.Centroid.Center.X < _tgtX - _mX)
                    {
                        dir = PanTiltDirection.RIGHT;
                    }
                    else if (track.Centroid.Center.X > _tgtX + _mX)
                    {
                        dir = PanTiltDirection.LEFT;
                    }
                    else if (track.Centroid.Center.Y < _tgtY - _mY)
                    {
                        dir = PanTiltDirection.DOWN;
                    }
                    else if (track.Centroid.Center.Y > _tgtY + _mY)
                    {
                        dir = PanTiltDirection.UP;
                    }


                    _camServer.Cam_RunDriveProgram("test", dir, 5, 500);
                }
                else
                {

                    _camServer.Cam_RunDriveProgram("test", DVIPProtocol.Protocol.Lib.Command.PTDrive.PanTiltDirection.STOP, 0, 500);
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            _trackEnabled = true;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            _trackEnabled = false;
            _camServer.Cam_RunDriveProgram("test", DVIPProtocol.Protocol.Lib.Command.PTDrive.PanTiltDirection.STOP, 0, 500);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            SetTrackParams();
        }

        private void SetTrackParams()
        {
            _tgtX = int.Parse(tb_tgtX.Text);
            _tgtY = int.Parse(tb_tgtY.Text);
            _mX = int.Parse(tb_mX.Text);
            _mY = int.Parse(tb_mY.Text);
        }
    }
}