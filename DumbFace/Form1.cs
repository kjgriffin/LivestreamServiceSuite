using DVIPProtocol.Servers.Mock;

using System.Net;

namespace DumbFace
{
    public partial class Form1 : Form
    {


        TCPMockDevice device = new TCPMockDevice();

        Bitmap face;

        System.Windows.Forms.Timer _uiTimer = new System.Windows.Forms.Timer();

        public Form1()
        {
            InitializeComponent();

            this.DoubleBuffered = true;
            face = new Bitmap(@"D:\Main\local-test\head.png");

            device.Start(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5002), true, false);
            device.OnStateChange += Device_OnStateChange;

            _uiTimer.Interval = 10;
            _uiTimer.Tick += _uiTimer_Tick;
            _uiTimer.Enabled = true;
        }

        private void _uiTimer_Tick(object? sender, EventArgs e)
        {
            if (left)
            {
                _x -= 1;
            }
            if (right)
            {
                _x += 1;
            }

            if (up)
            {
                _y -= 1;
            }
            if (down)
            {
                _y += 1;
            }

            this.Invalidate();
        }

        bool left = false;
        bool right = false;
        bool up = false;
        bool down = false;

        int _x = 500;
        int _y = 100;

        private void Device_OnStateChange(object? sender, DVIPEventArgs e)
        {
            var args = e as DVIPPanTiltDriveEventArgs;
            if (args != null)
            {
                left = args.Left;
                right = args.Right;
                up = args.Up;
                down = args.Down;


                // re-draw face
                this.Invalidate();
            }

        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.DrawImage(face, _x, _y);
        }


    }
}