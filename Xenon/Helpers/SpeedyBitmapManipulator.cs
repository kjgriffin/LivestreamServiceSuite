using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Xenon.Helpers
{
    class SpeedyBitmapManipulator
    {

        Bitmap b;
        BitmapData bdata;
        int bbytesperpixel;
        byte[] bpixels;
        IntPtr ptrFirstbPixel;
        int bheightinpixels;
        int bwidthinpixels;

        Bitmap nb;
        BitmapData nbdata;
        int nbbyptesperpixel;
        byte[] nbpixels;
        IntPtr ptrFirstnbPixel;

        bool isinitialized = false;

        public void Initialize(Bitmap source)
        {
            b = source;
            bdata = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadOnly, b.PixelFormat);
            bbytesperpixel = Bitmap.GetPixelFormatSize(b.PixelFormat) / 8;
            bpixels = new byte[bdata.Stride * b.Height];
            ptrFirstbPixel = bdata.Scan0;
            bheightinpixels = bdata.Height;
            bwidthinpixels = bdata.Width * bbytesperpixel;

            Marshal.Copy(ptrFirstbPixel, bpixels, 0, bpixels.Length);


            nb = new Bitmap(source.Width, source.Height);
            nbdata = nb.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.WriteOnly, source.PixelFormat);
            nbbyptesperpixel = Bitmap.GetPixelFormatSize(nb.PixelFormat) / 8;
            nbpixels = new byte[nbdata.Stride * nb.Height];
            ptrFirstnbPixel = nbdata.Scan0;

            isinitialized = true;
        }

        public Bitmap Finialize()
        {
            checkinit();

            Marshal.Copy(nbpixels, 0, ptrFirstnbPixel, nbpixels.Length);

            nb.UnlockBits(nbdata);
            b.UnlockBits(bdata);
            isinitialized = false;

            return nb;
        }

        private void checkinit()
        {
            if (!isinitialized)
            {
                throw new Exception("Not initialized");
            }
        }

        public (int R, int G, int B, int A) GetPixelRGBA(int x, int y)
        {
            checkinit();

            int yoff = y * bdata.Stride;
            int xoff = x * bbytesperpixel;
            return (bpixels[yoff + xoff + 2], bpixels[yoff + xoff + 1], bpixels[yoff + xoff + 0], bpixels[yoff + xoff + 3]);
        }

        public Color GetPixel(int x, int y)
        {
            var c = GetPixelRGBA(x, y);
            return Color.FromArgb(c.A, c.R, c.G, c.B);
        }

        public void SetPixelRGBA(int x, int y, int R, int G, int B, int A)
        {
            checkinit();
            int yoff = y * nbdata.Stride;
            int xoff = x * nbbyptesperpixel;

            nbpixels[yoff + xoff + 0] = (byte)B;
            nbpixels[yoff + xoff + 1] = (byte)G;
            nbpixels[yoff + xoff + 2] = (byte)R;
            nbpixels[yoff + xoff + 3] = (byte)A;
        }

        public void SetPixel(int x, int y, Color c)
        {
            SetPixelRGBA(x, y, c.R, c.G, c.B, c.A);
        }

    }
}
