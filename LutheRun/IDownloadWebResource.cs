using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

namespace LutheRun
{
    interface IDownloadWebResource
    {
        Task GetResourcesFromWeb();
        IEnumerable<LSBElementHymn.HymnImageLine> Images { get; }
    }
}
