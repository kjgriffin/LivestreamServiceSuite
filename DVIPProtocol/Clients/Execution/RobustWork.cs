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
        public bool IgnoreResponse { get; set; } = false;
    }

    public class RobustSequence : IRobustWork
    {
        /// <summary>
        /// Callback run when the execution of the entire sequence has failed after max attempts
        /// </summary>
        public RobustFail OnFail { get; set; }
        /// <summary>
        /// Callback run when the execution of the entire sequence has run to completion succesfully
        /// </summary>
        public Completed OnCompleted { get; set; }
        /// <summary>
        /// Max number of attempts to try execution the sequence
        /// </summary>
        public int RetryAttempts { get; set; }
        /// <summary>
        /// The primary command data to send
        /// </summary>
        public byte[] First { get; set; }
        /// <summary>
        /// The interval in ms between when the first command is sent and when the second command should be sent
        /// </summary>
        public int DelayMS { get; set; }
        /// <summary>
        /// The secondary command data to be sent
        /// </summary>
        public byte[] Second { get; set; }

        /// <summary>
        /// The command to send prior to the first command. Intened to be used to drive the system to a known state prior to execution primary command
        /// </summary>
        public byte[] Setup { get; set; }
        /// <summary>
        /// The interval of time required between setup and when the primary command should fire. Used to allow setup commands time to run to complete
        /// </summary>
        public int SetupDelayMS { get; set; }
        /// <summary>
        /// The command to send in the case the execution of the sequence fails partway. Intended to allow the system to come to rest prior to being reset.
        /// </summary>
        public byte[] Reset { get; set; }
        /// <summary>
        /// The interval of time required to gaurantee the reset command has run to complete.
        /// </summary>
        public int ResetDelayMS { get; set; }
    }
}
