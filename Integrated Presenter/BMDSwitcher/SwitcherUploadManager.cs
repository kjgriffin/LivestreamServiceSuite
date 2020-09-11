using BMDSwitcherAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Integrated_Presenter.BMDSwitcher
{
    class SwitcherUploadManager
    {

        Dispatcher dispatcher;

        IBMDSwitcher m_switcher;
        IBMDSwitcherMediaPool m_mediapool;
        IBMDSwitcherStills m_stills;
        StillsMonitor m_stillsmonitor;
        SwitcherMediaPoolLockMonitor m_lockmontior;


        IBMDSwitcherFrame m_frame;
        uint m_slot;
        string m_imgname;


        public SwitcherUploadManager(Dispatcher dispatcher, IBMDSwitcher switcher)
        {
            this.dispatcher = dispatcher;
            m_switcher = switcher;
            m_mediapool = (IBMDSwitcherMediaPool)switcher;
            m_mediapool.GetStills(out m_stills);
            if (m_stills == null)
            {
                throw new Exception();
            }
        }

        private void OnTransferFailled()
        {
            dispatcher.Invoke(() =>
            {
                m_stills.Unlock(m_lockmontior);
            });
        }

        private void OnTransferCompleted()
        {
            dispatcher.Invoke(() =>
            {
                m_stills.Unlock(m_lockmontior);
            });
        }

        public void UploadImage(string filename, uint slot)
        {
            m_slot = slot;
            m_imgname = Path.GetFileNameWithoutExtension(filename);
            dispatcher.Invoke(() =>
            {
                m_frame = CreateFrame(filename);

                m_stillsmonitor = new StillsMonitor() { OnLockIdle = GetLock };

            });
        }

        public void GetLock()
        {
            dispatcher.Invoke(() =>
            {
                m_lockmontior = new SwitcherMediaPoolLockMonitor() { OnLockObtained = StartUploadWithLock };
                m_stills.Lock(m_lockmontior);
            });
        }

        private void StartUploadWithLock()
        {
            dispatcher.Invoke(() =>
            {
                if (m_stillsmonitor != null)
                    m_stills.RemoveCallback(m_stillsmonitor);
                m_stillsmonitor = new StillsMonitor() { OnTransferCompleted = OnTransferCompleted, OnTransferFailled = OnTransferFailled };
                m_stills.AddCallback(m_stillsmonitor);
                m_stills.Upload(m_slot, m_imgname, m_frame);
            });
        }

        private IBMDSwitcherFrame CreateFrame(string filename)
        {
            IBMDSwitcherFrame frame;
            m_mediapool.CreateFrame(_BMDSwitcherPixelFormat.bmdSwitcherPixelFormat8BitARGB, 1920, 1080, out frame);
            IntPtr buffer;
            frame.GetBytes(out buffer);
            byte[] source = this.ConvertImage(filename);
            Marshal.Copy(source, 0, buffer, source.Length);
            return frame;
        }

        protected byte[] ConvertImage(string filename)
        {
            try
            {
                Bitmap image = new Bitmap(filename);

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


    }
}
