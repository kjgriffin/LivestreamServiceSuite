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
            //_trackTimer = new System.Windows.Forms.Timer();
            //_trackTimer.Interval = 50;
            //_trackTimer.Tick += _trackTimer_Tick;
            //_trackTimer.Enabled = true;

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



            //capture.FrameHeight = 720;
            //capture.FrameWidth = 1280;
            capture.Open(devINDEX, OpenCvSharp.VideoCaptureAPIs.DSHOW);
            capture.Set(OpenCvSharp.VideoCaptureProperties.FrameHeight, 720);
            capture.Set(OpenCvSharp.VideoCaptureProperties.FrameWidth, 1280);
            //capture.Set(OpenCvSharp.VideoCaptureProperties.FrameHeight, 1080);
            //capture.Set(OpenCvSharp.VideoCaptureProperties.FrameWidth, 1920);
            //capture.Set(OpenCvSharp.VideoCaptureProperties.FourCC, FourCC.H264);

            while (!cts.IsCancellationRequested)
            {


                var frameMat = capture.RetrieveMat();

                //frameMat = frameMat.Blur(new OpenCvSharp.Size(16, 16));

                var rects = classifier.DetectMultiScale(frameMat, 1.1, 7, OpenCvSharp.HaarDetectionTypes.ScaleImage | HaarDetectionTypes.DoCannyPruning | HaarDetectionTypes.DoRoughSearch | HaarDetectionTypes.FindBiggestObject);

                //var rects = classifier.DetectMultiScale(frameMat, 1.1, 7, OpenCvSharp.HaarDetectionTypes.ScaleImage);

                _tracker.UpdateTracks(rects);

                Bitmap bmp = frameMat.ToBitmap();
                Draw(bmp, _tracker.GetTracks());

                Task.Run(() => DoTrack());

                bmp.Dispose();
                frameMat.Dispose();

                Thread.Sleep(10);
            }
            capture.Release();
        }

        int fps = 0;
        int ticks = 0;
        int lastSec = 0;
        private void Draw(Bitmap bmp, List<CentroidTrack> tracks)
        {
            if (InvokeRequired)
            {
                Invoke(Draw, bmp, tracks);
                return;
            }

            var now = DateTime.Now.Second;
            if (lastSec != now)
            {
                fps = ticks;
                ticks = 0;
            }
            lastSec = now;
            ticks++;

            using (Graphics g = CreateGraphics())
            using (Bitmap b = new Bitmap(Size.Width, Size.Height))
            using (Graphics gfx = Graphics.FromImage(b))
            {
                gfx.Clear(Color.White);
                //gfx.DrawImage(bmp, new Rectangle(0, 0, 1280, 720), new Rectangle(0, 0, 1280, 720), GraphicsUnit.Pixel);
                gfx.DrawImage(bmp, 0, 0);


                foreach (var track in tracks)
                {
                    gfx.DrawRectangle(track.Stale ? Pens.Red : Pens.Green, new System.Drawing.Rectangle(track.Centroid.Bounds.X, track.Centroid.Bounds.Y, track.Centroid.Bounds.Width, track.Centroid.Bounds.Height));
                    gfx.FillRectangle(Brushes.Black, (int)track.Centroid.Center.X, (int)track.Centroid.Center.Y, 150, 25);
                    gfx.DrawString(track.Name, DefaultFont, Brushes.White, new System.Drawing.Point((int)track.Centroid.Center.X, (int)track.Centroid.Center.Y));

                    int scalar = 20;
                    int x1 = (int)(track.Centroid.Center.X + track.XVel * scalar);
                    int y1 = (int)(track.Centroid.Center.Y + track.YVel * scalar);
                    gfx.DrawLine(new Pen(Color.Green, 5), (int)track.Centroid.Center.X, (int)track.Centroid.Center.Y, x1, y1);

                }

                gfx.FillRectangle(Brushes.White, 10, 10, 150, 30);
                gfx.DrawString($"FPS {fps}", DefaultFont, Brushes.Black, new System.Drawing.Point(10, 10));

                for (int i = 0; i < tracks.Count; i++)
                {
                    gfx.FillRectangle(Brushes.White, 10, 40 + 20 * i, 150, 40);
                    gfx.DrawString(tracks[i].Name, DefaultFont, Brushes.Black, new System.Drawing.Point(10, 40 + i * 20));
                }

                // draw target location
                gfx.FillEllipse(Brushes.Yellow, _tgtX - 5, _tgtY - 5, 10, 10);
                gfx.DrawRectangle(Pens.Yellow, _tgtX - _mX, _tgtY - _mY, _mX * 2, _mY * 2);

                // draw tracking bounds
                gfx.DrawRectangle(new Pen(Color.Blue, 4), _validX - _rangeX, _validY - _rangeY, _rangeX * 2, _rangeY * 2);

                g.FillRectangle(Brushes.White, 100, 70, 200, 30);
                g.DrawString($"{_trackCMD} [{_cmdID}]", DefaultFont, Brushes.Blue, 100, 70);

                //g.DrawImage(b, new RectangleF(10, 80, 1280, 720), new RectangleF(0, 0, 640, 480), GraphicsUnit.Pixel);
                g.DrawImage(b, 10, 100);
            }
        }



        IRobustCamServer _camServer = IRobustCamServer.InstantiateRobust(null);

        private void button3_Click(object sender, EventArgs e)
        {
            _camServer.Start();
            _camServer.StartCamClient("test", new System.Net.IPEndPoint(IPAddress.Parse(tbCamIP.Text), int.Parse(tbCamPort.Text)));
        }

        private void button4_Click(object sender, EventArgs e)
        {
            _camServer.Cam_RunDriveProgram("test", DVIPProtocol.Protocol.Lib.Command.PTDrive.PanTiltDirection.STOP, 0, 0);
            _trackEnabled = false;
        }


        bool _trackEnabled = false;

        int _tgtX = 0;
        int _tgtY = 0;
        int _mX = 0;
        int _mY = 0;

        int _validX = 0;
        int _validY = 0;
        int _rangeX = 0;
        int _rangeY = 0;

        string _trackCMD = "NO TRACK";
        long _cmdID = 0;

        PanTiltDirection _last = PanTiltDirection.STOP;
        private void DoTrack()
        {
            if (_trackEnabled)
            {
                var track = _tracker.GetTracks().FirstOrDefault(t => Math.Abs(t.Centroid.Center.X - _validX) < _rangeX && Math.Abs(t.Centroid.Center.Y - _validY) < _rangeY);
                _cmdID++;

                if (track != null)
                {
                    // compute direction
                    if (!checkBox1.Checked && !track.Stale)
                    {

                    }
                    {

                        PanTiltDirection dir = PanTiltDirection.STOP;
                        string dcmd = "";

                        if (track.Centroid.Center.X < _tgtX - _mX)
                        {
                            dir = PanTiltDirection.LEFT;
                            dcmd = "LEFT";
                        }
                        else if (track.Centroid.Center.X > _tgtX + _mX)
                        {
                            dir = PanTiltDirection.RIGHT;
                            dcmd = "RIGHT";
                        }
                        else if (track.Centroid.Center.Y < _tgtY - _mY)
                        {
                            dir = PanTiltDirection.UP;
                            dcmd = "UP";
                        }
                        else if (track.Centroid.Center.Y > _tgtY + _mY)
                        {
                            dir = PanTiltDirection.DOWN;
                            dcmd = "DOWN";
                        }

                        if (_last != dir)
                        {
                            _camServer.Cam_RunDriveProgram("test", dir, 2, 0);
                            _trackCMD = dcmd;
                        }
                        _last = dir;
                    }
                }
                else
                {
                    _camServer.Cam_RunDriveProgram("test", DVIPProtocol.Protocol.Lib.Command.PTDrive.PanTiltDirection.STOP, 0, 100);
                    _trackCMD = "STABLE (stop)";
                    _last = PanTiltDirection.STOP;
                }
            }
            else
            {
                _trackCMD = "NO TRACK";
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            _trackEnabled = true;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            _trackEnabled = false;
            _camServer.Cam_RunDriveProgram("test", DVIPProtocol.Protocol.Lib.Command.PTDrive.PanTiltDirection.STOP, 0, 100);
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

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void button11_Click(object sender, EventArgs e)
        {
            _camServer.Cam_RunDriveProgram("test", DVIPProtocol.Protocol.Lib.Command.PTDrive.PanTiltDirection.LEFT, 5, 0);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            _camServer.Cam_RunDriveProgram("test", DVIPProtocol.Protocol.Lib.Command.PTDrive.PanTiltDirection.RIGHT, 5, 0);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            _camServer.Cam_RunDriveProgram("test", DVIPProtocol.Protocol.Lib.Command.PTDrive.PanTiltDirection.UP, 5, 0);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            _camServer.Cam_RunDriveProgram("test", DVIPProtocol.Protocol.Lib.Command.PTDrive.PanTiltDirection.DOWN, 5, 0);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            _validX = int.Parse(tbValidX.Text);
            _validY = int.Parse(tbValidY.Text);
            _rangeX = int.Parse(tbRangeX.Text);
            _rangeY = int.Parse(tbRangeY.Text);
        }
    }
}