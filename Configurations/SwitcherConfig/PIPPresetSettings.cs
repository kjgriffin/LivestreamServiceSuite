using IntegratedPresenter.BMDSwitcher.Config;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Configurations.SwitcherConfig
{
    public class PIPPresetSettings
    {

        public Dictionary<int, PIPPreset> Presets = new Dictionary<int, PIPPreset>();

        public bool IsValid()
        {
            // for now we expect to have 5 presets
            for (int i = 1; i <= 5; i++)
            {
                if (Presets?.ContainsKey(i) != true)
                {
                    return false;
                }
            }
            return true;
        }

        public static PIPPresetSettings Default()
        {
            return new PIPPresetSettings
            {
                Presets = new Dictionary<int, PIPPreset>
                {
                    [1] = new PIPPreset
                    {
                        Name = "L. SPLIT",
                        Placement = new PIPPlaceSettings
                        {
                            MaskBottom = 0,
                            MaskLeft = 0,
                            MaskRight = 16,
                            MaskTop = 0,
                            PosX = 0,
                            PosY = 0,
                            ScaleX = 1,
                            ScaleY = 1,
                        }
                    },
                    [2] = new PIPPreset
                    {
                        Name = "R. SPLIT",
                        Placement = new PIPPlaceSettings
                        {
                            MaskBottom = 0,
                            MaskLeft = 16,
                            MaskRight = 0,
                            MaskTop = 0,
                            PosX = 0,
                            PosY = 0,
                            ScaleX = 1,
                            ScaleY = 1,
                        }
                    },
                    [3] = new PIPPreset
                    {
                        Name = "BCLASS",
                        Placement = new PIPPlaceSettings
                        {
                            MaskBottom = 0,
                            MaskLeft = 0,
                            MaskRight = 0,
                            MaskTop = 0,
                            PosX = -4.8,
                            PosY = 0,
                            ScaleX = 0.7,
                            ScaleY = 0.7,
                        }
                    },
                    [4] = new PIPPreset
                    {
                        Name = "HYMN",
                        Placement = new PIPPlaceSettings
                        {
                            MaskBottom = 0,
                            MaskLeft = 0,
                            MaskRight = 0,
                            MaskTop = 0,
                            PosX = -11.2,
                            PosY = -5.5,
                            ScaleX = 0.27,
                            ScaleY = 0.27,
                        }
                    },
                    [5] = new PIPPreset
                    {
                        Name = "R. BOX",
                        Placement = new PIPPlaceSettings
                        {
                            MaskBottom = 0,
                            MaskLeft = 0,
                            MaskRight = 0,
                            MaskTop = 0,
                            PosX = 9.6,
                            PosY = -5.4,
                            ScaleX = 0.4,
                            ScaleY = 0.4,
                        }
                    }
                }
            };
        }
    }

    public class PIPPreset
    {
        public string Name { get; set; }
        public PIPPlaceSettings Placement { get; set; }

        public override string ToString()
        {
            return $"Name: {Name}, Placement: {Placement.ToString()}";
        }
    }

}
