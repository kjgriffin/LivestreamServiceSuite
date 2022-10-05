using IntegratedPresenterAPIInterop;

using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xenon.Renderer;
using Xenon.SlideAssembly;

namespace SlideCreater
{
    internal class HotReloadMonitor
    {
        CancellationTokenSource m_cancel;
        public event EventHandler<EventArgs> OnHotReloadConsumed;

        public bool PublishReady { get; set; } = false;

        public TimeSpan RefreshDelay { get; set; } = TimeSpan.FromSeconds(1);

        public void StartListening()
        {
            m_cancel = new CancellationTokenSource();
            m_hotreloadSyncFile = MemoryMappedFile.CreateOrOpen(CommonAPINames.HotReloadSyncFile, 1024, MemoryMappedFileAccess.ReadWrite);

            Task.Run(async () =>
            {
                while (!m_cancel.IsCancellationRequested)
                {
                    if (PublishReady)
                    {
                        bool stillThere = true;
                        using (var view = m_hotreloadSyncFile.CreateViewAccessor())
                        {
                            stillThere = view.ReadBoolean(0);
                        }
                        if (!stillThere)
                        {
                            OnHotReloadConsumed?.Invoke(this, new EventArgs());
                            PublishReady = false;
                        }
                    }

                    await Task.Delay((int)RefreshDelay.TotalMilliseconds, m_cancel.Token);
                }

                m_cancel?.Dispose();
                m_hotreloadSyncFile?.Dispose();

            }, m_cancel.Token);
        }

        public void StopListening()
        {
            m_cancel?.Cancel();
        }

        public void Release()
        {
            foreach (var file in mpres?.MFiles ?? new List<MemoryMappedFile>())
            {
                file.Dispose();
            }
            m_hotreloadSyncFile?.Dispose();
        }


        SharedMemoryRenderer.SharedMemoryPresentation mpres;
        MemoryMappedFile m_hotreloadSyncFile;

        public bool PublishReload(Project proj, List<RenderedSlide> slides)
        {

            foreach (var file in mpres?.MFiles ?? new List<MemoryMappedFile>())
            {
                file.Dispose();
            }

            mpres = SharedMemoryRenderer.ExportSlides(proj, slides);

            if (m_hotreloadSyncFile == null)
            {
                m_hotreloadSyncFile = MemoryMappedFile.CreateOrOpen(CommonAPINames.HotReloadSyncFile, 1024, MemoryMappedFileAccess.ReadWrite);
            }

            if (mpres.Info.Slides.Count > 0)
            {
                PublishReady = true;
                using (var view = m_hotreloadSyncFile.CreateViewAccessor())
                {
                    view.Write(0, true);
                }
                return true;
            }
            return false;
        }

    }
}
