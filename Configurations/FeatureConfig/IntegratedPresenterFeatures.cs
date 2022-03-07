using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Configurations.FeatureConfig
{
    public class IntegratedPresenterFeatures
    {
        public IntegratedPresentationFeatures_Automation AutomationSettings { get; set; }
        public IntegratedPresentationFeatures_Presentation PresentationSettings { get; set; }
        public IntegratedPresentationFeatures_View ViewSettings { get; set; }
        public IntegratedPresentationFeatures_Interface InterfaceSettings { get; set; }

        public static IntegratedPresenterFeatures Default()
        {
            return new IntegratedPresenterFeatures()
            {
                AutomationSettings = IntegratedPresentationFeatures_Automation.Default(),
                PresentationSettings = IntegratedPresentationFeatures_Presentation.Default(),
                ViewSettings = IntegratedPresentationFeatures_View.Default(),
                InterfaceSettings = IntegratedPresentationFeatures_Interface.Default(),
            };
        }

        public static IntegratedPresenterFeatures Load(string filename)
        {
            var src = "";
            try
            {
                using (var sr = new StreamReader(filename))
                {
                    src = sr.ReadToEnd();
                }
                return JsonSerializer.Deserialize<IntegratedPresenterFeatures>(src);
            }
            catch (Exception)
            {
                return IntegratedPresenterFeatures.Default();
            }

        }

        public void Save(string filename)
        {
            try
            {
                var obj = JsonSerializer.Serialize(this);
                using (var sw = new StreamWriter(filename))
                {
                    sw.Write(obj);

                }
            }
            catch (Exception)
            {
            }
        }
    }

    public class IntegratedPresentationFeatures_Interface
    {
        public bool EnterKeyOnlyAsAutoTrans { get; set; } = true;
        public bool SpaceKeyOnlyAsCutTrans { get; set; } = true;

        public static IntegratedPresentationFeatures_Interface Default()
        {
            return new IntegratedPresentationFeatures_Interface
            {
                EnterKeyOnlyAsAutoTrans = true,
                SpaceKeyOnlyAsCutTrans = true
            };
        }
    }

    public class IntegratedPresentationFeatures_Presentation
    {
        public bool StartPresentationMuted { get; set; }

        public static IntegratedPresentationFeatures_Presentation Default()
        {
            return new IntegratedPresentationFeatures_Presentation()
            {
                StartPresentationMuted = false,
            };
        }
    }

    public class IntegratedPresentationFeatures_Automation
    {
        public bool EnableSermonTimer { get; set; }
        public bool EnableDriveModeAutoTransGuard { get; set; }
        public bool EnableCutTransGuard { get; set; }
        public bool EnablePostset { get; set; }
        public bool EnableAutomationStepsPreview { get; set; }

        public static IntegratedPresentationFeatures_Automation Default()
        {
            return new IntegratedPresentationFeatures_Automation()
            {
                EnableCutTransGuard = true,
                EnableSermonTimer = true,
                EnableDriveModeAutoTransGuard = true,
                EnablePostset = true,
                EnableAutomationStepsPreview = true,
            };

        }
    }

    public class IntegratedPresentationFeatures_View
    {
        public bool View_PrevAfterPreviews { get; set; }
        public bool View_PreviewEffectiveCurrent { get; set; }
        public bool View_AdvancedPresentation { get; set; }
        public bool View_DefaultOpenAudioPlayer { get; set; }
        public bool View_AdvancedDVE { get; set; }
        public bool View_DefaultOpenAdvancedPIPLocation { get; set; }
        public bool View_AuxOutput { get; set; }

        public static IntegratedPresentationFeatures_View Default()
        {
            return new IntegratedPresentationFeatures_View()
            {
                View_PrevAfterPreviews = true,
                View_PreviewEffectiveCurrent = true,
                View_AdvancedDVE = false,
                View_AuxOutput = false,
                View_AdvancedPresentation = false,
                View_DefaultOpenAdvancedPIPLocation = false,
                View_DefaultOpenAudioPlayer = false,
            };
        }
    }


}
