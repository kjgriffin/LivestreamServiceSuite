using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegratedPresenterAPIInterop
{

    public static class CommonAPINames
    {
        public static string HotReloadSyncFile { get => "IntegratedPresenter-HotReload-Sync"; }
        public static string HotReloadPresentationDescriptionFile { get => "SlideCreater-HotReload-PresentationDescription"; }
    }



    public class MirrorPresentationDescription
    {
        public List<MirrorSlide> Slides { get; set; } = new List<MirrorSlide>();
        public string HeavyResourcePath { get; set; } = "";


    }

    public class MirrorSlide
    {
        public SlideType SlideType { get; set; }


        public int Num { get; set; }

        public bool HasKey { get; set; }
        public bool HasPostset { get; set; }
        public bool HasPilot { get; set; }


        public bool IsFullAuto { get; set; }

        public bool HasOverridePrimary { get; set; }
        public bool HasOverrideKey { get; set; }


        public string PrimaryResource { get; set; }
        public string KeyResource { get; set; }

        public int PostsetInfo { get; set; }
        public string ActionInfo { get; set; }
        public string PilotInfo { get; set; }


        public bool AutomationEnabled { get; set; }
        public string PreAction { get; set; }
    }


}
