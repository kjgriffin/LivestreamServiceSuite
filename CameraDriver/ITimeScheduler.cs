using System.Diagnostics;

namespace CameraDriver
{

    public delegate void WorkItemDelegate(object data);

    public interface ITimeScheduler
    {
        Guid ScheduleForNow(WorkItemDelegate work);
        Guid ScheduleForRelativeFuture(int ms, WorkItemDelegate work);
        Guid ScheduleForRelativeFutureToItem(Guid item, int ms, WorkItemDelegate work);
        Guid CancelItem(Guid item);
    }

    public class FairlyAccurateTimeScheduler : ITimeScheduler
    {

        Thread _workThread;
        ManualResetEvent _wake = new ManualResetEvent(false);

        Stopwatch timer = new Stopwatch();

        object _lock = new object();
        HashSet<Guid> _enquedItems = new HashSet<Guid>();
        Dictionary<Guid, WorkItemDelegate> _workItems = new Dictionary<Guid, WorkItemDelegate>();
        Dictionary<Guid, long> _times = new Dictionary<Guid, long>();


        public FairlyAccurateTimeScheduler()
        {
            _workThread = new Thread(Run_Internal);
            _workThread.IsBackground = true;
            _workThread.Name = "FairlyAccurateTimeScheduler_WorkerThread";
            _workThread.Start();
        }

        private void Run_Internal()
        {
            // setup all the timers etc.

            while (true)
            {

                // wait for something interesting


            }

        }

        public Guid CancelItem(Guid item)
        {
            throw new NotImplementedException();
        }

        public Guid ScheduleForNow(WorkItemDelegate work)
        {
            throw new NotImplementedException();
        }

        public Guid ScheduleForRelativeFuture(int ms, WorkItemDelegate work)
        {
            throw new NotImplementedException();
        }

        public Guid ScheduleForRelativeFutureToItem(Guid item, int ms, WorkItemDelegate work)
        {
            throw new NotImplementedException();
        }
    }
}