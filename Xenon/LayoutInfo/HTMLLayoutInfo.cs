using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Xenon.Renderer;
using Xenon.SlideAssembly;

namespace Xenon.LayoutInfo
{
    internal class HTMLLayoutInfo : ALayoutInfo, ILayoutInfoResolver<HTMLLayoutInfo>
    {
        public string HTMLSrc { get; set; } = "";
        public string CSSSrc { get; set; } = "";

        public HTMLLayoutInfo GetLayoutInfo(Slide slide)
        {
            return ILayoutInfoResolver<HTMLLayoutInfo>._InternalDefault_GetLayoutInfo(slide);
        }

        public HTMLLayoutInfo _Internal_GetDefaultInfo(string overrideDefault = "")
        {
            return ILayoutInfoResolver<HTMLLayoutInfo>.GetDefaultInfo(overrideDefault);
        }

        public override string GetDefaultJson()
        {
            return JsonSerializer.Serialize(_Internal_GetDefaultInfo(), new JsonSerializerOptions() { WriteIndented = true });
        }
    }
}
