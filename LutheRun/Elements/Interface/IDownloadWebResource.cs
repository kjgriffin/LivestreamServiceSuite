using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using LutheRun.Elements.LSB;

namespace LutheRun.Elements.Interface
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
