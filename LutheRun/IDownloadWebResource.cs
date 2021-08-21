using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

namespace LutheRun
{

    public interface ILoadResourceAsync
    {
        Task GetResourcesFromLocalOrWeb(string path = "");
    }

    interface IDownloadWebResource : ILoadResourceAsync
    {
        IEnumerable<LSBElementHymn.HymnImageLine> Images { get; }
    }
}
