using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVIPProtocol.Clients.Execution
{
    internal class WorkList
    {

    }

    internal class Work
    {

    }

    public delegate void RobustFail(int attempts);
    public delegate void Completed(byte[] data);

    internal interface IRobustWork
    {
        public RobustFail OnFail { get; }
        public Completed OnCompleted { get; }
        public int RetryAttempts { get; }
    }

    internal class RobustCommand : IRobustWork
    {
        public byte[] Data { get; set; }
        public RobustFail OnFail { get; set; }
        public Completed OnCompleted { get; set; }
        public int RetryAttempts { get; set; }
    }

    internal class RobustSequence : IRobustWork
    {
        public RobustFail OnFail { get; set; }
        public Completed OnCompleted { get; set; }
        public int RetryAttempts { get; set; }
        public byte[] First { get; set; }
        public int DelayMS { get; set; }
        public byte[] Second { get; set; }
    }
}
