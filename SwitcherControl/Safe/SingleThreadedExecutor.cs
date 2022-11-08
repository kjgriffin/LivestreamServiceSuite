using SwitcherControl.BMDSwitcher;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SwitcherControl.Safe
{
    internal class WorkItem
    {
        public Guid ID { get; internal set; }
        public Action Work { get; set; }
        public Action Callback { get; set; }
    }

    public class SingleThreadedExecutor
    {

        Thread workerThread;
        CancellationTokenSource m_cancel;

        ManualResetEvent m_workAvailable;
        ConcurrentQueue<WorkItem> m_workQueue;

        ConcurrentDictionary<Guid, ManualResetEvent> m_workCallbackSync;

        public SingleThreadedExecutor()
        {
            m_workQueue = new ConcurrentQueue<WorkItem>();
            m_workAvailable = new ManualResetEvent(false);
            m_workCallbackSync = new ConcurrentDictionary<Guid, ManualResetEvent>();

            workerThread = new Thread(RunLoop);
            workerThread.Name = "SingleThreadedExecutorWorkerThread";
            workerThread.SetApartmentState(ApartmentState.STA);
            workerThread.Start();
        }

        private void RunLoop(object obj)
        {
            while (!m_cancel.IsCancellationRequested)
            {
                // execute all work
                while (m_workQueue.TryDequeue(out WorkItem? workitem))
                {
                    if (!m_cancel.IsCancellationRequested)
                    {
                        workitem?.Work?.Invoke();
                        workitem?.Callback?.Invoke();
                        // mark work complete
                        if (m_workCallbackSync.TryGetValue(workitem.ID, out var sync))
                        {
                            sync.Set();
                        }
                    }
                }
                m_workAvailable.Reset();

                // wait for work
                if (!m_cancel.IsCancellationRequested)
                {
                    WaitHandle.WaitAny(new WaitHandle[] { m_cancel.Token.WaitHandle, m_workAvailable });
                }
            }

            // cleanup
            m_cancel.Dispose();
            m_workAvailable.Dispose();
        }

        public void EnqueueWork(Action work)
        {
            m_workQueue.Enqueue(new WorkItem
            {
                ID = Guid.NewGuid(),
                Work = work,
                Callback = null,
            });
            m_workAvailable.Set();
        }

        public (Guid id, ManualResetEvent wait) EnqueueWorkWithCallback(Action work, Action callback)
        {
            ManualResetEvent workDone = new ManualResetEvent(false);
            var item = new WorkItem
            {
                ID = Guid.NewGuid(),
                Work = work,
                Callback = callback,
            };
            m_workCallbackSync[item.ID] = workDone;
            m_workQueue.Enqueue(item);
            m_workAvailable.Set();

            return (item.ID, workDone);
        }

        public void RemoveTrackedWork(Guid ID)
        {
            if (m_workCallbackSync.TryRemove(ID, out var workDone))
            {
                workDone.Dispose();
            }
        }

        public void Stop()
        {
            m_cancel.Cancel();
        }



    }
}
