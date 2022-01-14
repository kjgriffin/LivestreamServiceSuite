using BMDSwitcherAPI;

using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SwitcherControl.Mk2
{
    /// <summary>
    /// Provides thread-safe access to a BMD swithcer's API.     
    /// Additionally will provide mechanism to know when commands have been completed by the switcher.
    /// </summary>
    internal class SwitcherCommandDispatcher
    {

        IPAddress switcherAddress;

        Thread switcherThread;
        CancellationTokenSource stopLoopCTS;
        CancellationToken stopLoopToken;

        BlockingCollection<QueuedSwitcherWorkItem> commandQueue;

        BMDSwitcherAPIInterface switcherAPI;



        private void OpenConnect(IPAddress address)
        {
            this.switcherAddress = address;
            // TODO: command dispatcher should only be talking to one at a time... so we should probably forcibly exit/cancel/stop if we're already doing stuff

            // create new thread to do everything on
            switcherThread = new Thread(Setup);
            // initialize all fields
            stopLoopCTS?.Dispose();
            stopLoopCTS = new CancellationTokenSource();
            stopLoopToken = stopLoopCTS.Token;
            commandQueue = new BlockingCollection<QueuedSwitcherWorkItem>();

            switcherThread.Start();
        }

        private void Setup()
        {
            // use new thread to get the swticher COM API
            if (BMDSwitcherAPIInterface.TryDiscoverSwitcher(switcherAddress, out IBMDSwitcher switcher, out string connectionMessage)) // TODO: real IP address that would work
            {
                // initialize all the switcher stuff we support
                switcherAPI = new BMDSwitcherAPIInterface(switcher);
            }
            else
            {
                // figure out what to do if everything fails
            }

            // enter the runloop
            InternalRunLoop();
        }

        private void InternalRunLoop()
        {

            // check if should exit
            if (stopLoopToken.IsCancellationRequested)
            {
                // exit or something....
            }

            // get the next thing to do.
            // blocks until something appears or we're cancelled
            QueuedSwitcherWorkItem cmd = commandQueue.Take(stopLoopToken);
            cmd.APIInvokeAction?.Invoke(switcherAPI);

            // TODO: figure out how to track that we've fired off a command over the network- and should be waiting now for something from the switcher so we'll know if it works

        }





    }
}
