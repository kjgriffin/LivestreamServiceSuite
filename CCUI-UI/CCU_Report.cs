using System;

namespace CCUI_UI
{
    internal class CCU_Report
    {
        public string Timestamp { get; private set; }
        public string CMDName { get; private set; }
        public string CMDStatus { get; private set; }
        public Guid UID { get; private set; }
        public bool OK { get; private set; }

        public CCU_Report(string cmd, string status, bool OK)
        {
            this.OK = OK;
            this.Timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            this.CMDName = cmd;
            var sword = status.Split(Environment.NewLine);
            CMDStatus = sword[0];
            UID = Guid.TryParse(sword.Length > 1 ? sword[1] : "", out var uid) ? uid : Guid.Empty;
        }
    }

}
