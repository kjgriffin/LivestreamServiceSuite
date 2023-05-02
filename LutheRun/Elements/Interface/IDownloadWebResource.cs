using LutheRun.Elements.LSB;

using System.Collections.Generic;
using System.Threading.Tasks;

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
