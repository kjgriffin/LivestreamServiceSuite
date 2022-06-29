using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVIPProtocol.Clients.Execution
{

    public delegate void RobustFail(int attempts);
    public delegate void Completed(int attempts, byte[] data);

    public interface IRobustWork
    {
        public RobustFail OnFail { get; }
        public Completed OnCompleted { get; }
        public int RetryAttempts { get; }
    }

    public class RobustCommand : IRobustWork
    {
        public byte[] Data { get; set; }
        public RobustFail OnFail { get; set; }
        public Completed OnCompleted { get; set; }
        public int RetryAttempts { get; set; }
    }

    public class RobustSequence : IRobustWork
    {
        public RobustFail OnFail { get; set; }
        public Completed OnCompleted { get; set; }
        public int RetryAttempts { get; set; }
        public byte[] First { get; set; }
        public int DelayMS { get; set; }
        public byte[] Second { get; set; }
    }
}
