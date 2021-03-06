﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using BMDSwitcherAPI;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace Integrated_Presenter
{
    public class Upload
    {
        private enum Status
        {
            NotStarted,
            Started,
            Completed,
        }

        private Upload.Status currentStatus;
        private String filename;
        private int uploadSlot;
        private IBMDSwitcher switcher;
        private String name;
        private IBMDSwitcherFrame frame;
        private IBMDSwitcherStills stills;
        private IBMDSwitcherLockCallback lockCallback;
        private Dispatcher d;

        public Upload(Dispatcher d, IBMDSwitcher switcher, string filename, int uploadSlot)
        {
            this.switcher = switcher;
            this.filename = filename;
            this.uploadSlot = uploadSlot;
            this.d = d;

            if (!File.Exists(filename))
            {
                throw new FileNotFoundException(String.Format("{0} does not exist", filename));
            }

            d.Invoke(() =>
            {
                this.stills = this.GetStills();
            });
        }

        public bool InProgress()
        {
            return this.currentStatus == Upload.Status.Started;
        }

        public void SetName(String name)
        {
            this.name = name;
        }

        public int GetProgress()
        {
            if (this.currentStatus == Upload.Status.NotStarted)
            {
                return 0;
            }
            if (this.currentStatus == Upload.Status.Completed)
            {
                return 100;
            }

            double progress;
            progress = 0;
            d.Invoke(() =>
            {
                this.stills.GetProgress(out progress);
            });
            return (int)Math.Round(progress * 100.0);
        }

        public void Start()
        {
            d.Invoke(() =>
            {
                this.currentStatus = Upload.Status.Started;
                this.frame = this.GetFrame();
                this.lockCallback = (IBMDSwitcherLockCallback)new UploadLock(this);
                this.stills.Lock(this.lockCallback);
            });
        }

        protected IBMDSwitcherFrame GetFrame()
        {
            IBMDSwitcherMediaPool switcherMediaPool = (IBMDSwitcherMediaPool)this.switcher;
            IBMDSwitcherFrame frame;
            switcherMediaPool.CreateFrame(_BMDSwitcherPixelFormat.bmdSwitcherPixelFormat8BitARGB, 1920, 1080, out frame);
            IntPtr buffer;
            frame.GetBytes(out buffer);
            byte[] source = this.ConvertImage();
            Marshal.Copy(source, 0, buffer, source.Length);
            return frame;
        }

        protected byte[] ConvertImage()
        {
            try
            {
                Bitmap image = new Bitmap(this.filename);

                if (image.Width != 1920 || image.Height != 1080)
                {
                    throw new Exception(String.Format("Image is {0}x{1} it needs to be the same resolution as the switcher", image.Width.ToString(), image.Height.ToString()));
                }

                byte[] numArray = new byte[image.Width * image.Height * 4];
                for (int index1 = 0; index1 < image.Width * image.Height; index1++)
                {
                    Color pixel = this.GetPixel(image, index1);
                    int index2 = index1 * 4;
                    numArray[index2] = pixel.B;
                    numArray[index2 + 1] = pixel.G;
                    numArray[index2 + 2] = pixel.R;
                    numArray[index2 + 3] = pixel.A;
                }
                return numArray;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        protected Color GetPixel(Bitmap image, int index)
        {
            int x = index % image.Width;
            int y = (index - x) / image.Width;
            return image.GetPixel(x, y);
        }

        protected IBMDSwitcherStills GetStills()
        {
            IBMDSwitcherMediaPool switcherMediaPool = (IBMDSwitcherMediaPool)this.switcher;
            IBMDSwitcherStills stills;
            switcherMediaPool.GetStills(out stills);
            return stills;
        }

        public void UnlockCallback()
        {
            this.currentStatus = Upload.Status.Completed;
        }

        public void LockCallback()
        {
            d.Invoke(() =>
            {
                IBMDSwitcherStillsCallback callback = (IBMDSwitcherStillsCallback)new Stills(this);
                this.stills.AddCallback(callback);
                this.stills.Upload((uint)this.uploadSlot, this.GetName(), this.frame);
            });
        }

        public void TransferCompleted()
        {
            //Log.Debug("Completed upload");
            d.Invoke(() =>
            {
                this.stills.Unlock(this.lockCallback);
                this.currentStatus = Upload.Status.Completed;
            });
        }

        public String GetName()
        {
            if (this.name != null)
            {
                return this.name;
            }
            return Path.GetFileNameWithoutExtension(this.filename);
        }
    }
}
