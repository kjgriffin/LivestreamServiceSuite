using System.Collections.Generic;

namespace LutheRun.Pilot
{
    public enum CameraID
    {
        PULPIT,
        CENTER,
        LECTERN,
        ORGAN,
        SLIDE,
        BACK,
    }

    public static class CameraIDHelpers
    {
        public static Dictionary<CameraID, string> DefaultMappings
        {
            get
            {
                Dictionary<CameraID, string> mappings = new Dictionary<CameraID, string>();

                foreach (var value in System.Enum.GetValues<CameraID>())
                {
                    mappings[value] = value.ToString().ToLower();
                }

                return mappings;
            }
        }
    }


}
