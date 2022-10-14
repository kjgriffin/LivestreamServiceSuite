using CCU.Config;

using CCUI_UI;

using Configurations.FeatureConfig;

using IntegratedPresenter.BMDSwitcher.Config;
using IntegratedPresenter.Main;

using IntegratedPresenterAPIInterop;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Integrated_Presenter.Presentation
{

    internal class PresentationHotReloadService
    {

        CancellationTokenSource m_cancel;
        public event EventHandler<EventArgs> OnHotReloadReady;

        public TimeSpan RefreshDelay { get; set; } = TimeSpan.FromSeconds(1);

        MemoryMappedFile m_file;

        public void StartListening()
        {
            m_cancel = new CancellationTokenSource();
            m_file = MemoryMappedFile.CreateOrOpen(CommonAPINames.HotReloadSyncFile, 1024, MemoryMappedFileAccess.ReadWrite);

            Task.Run(async () =>
            {
                while (!m_cancel.IsCancellationRequested)
                {
                    bool available = false;
                    using (var view = m_file.CreateViewAccessor())
                    {
                        available = view.ReadBoolean(0);
                        if (available)
                        {
                            view.Write(0, false);
                        }
                    }

                    if (available)
                    {
                        OnHotReloadReady?.Invoke(this, new EventArgs());
                    }

                    await Task.Delay((int)RefreshDelay.TotalMilliseconds, m_cancel.Token);
                }

                m_cancel.Dispose();
                m_file.Dispose();

            }, m_cancel.Token);
        }

        public void StopListening()
        {
            m_cancel?.Cancel();
        }



    }



    internal class MirroredPresentationBuilder
    {

        public static IPresentation Create()
        {
            IntegratedPresenter.Main.Presentation pres = new IntegratedPresenter.Main.Presentation();

            string dfiletext = "";
            using (var file = MemoryMappedFile.OpenExisting(CommonAPINames.HotReloadPresentationDescriptionFile))
            using (var dstream = file.CreateViewStream())
            using (var reader = new StreamReader(dstream))
            {
                dfiletext = reader.ReadToEnd().Trim().TrimEnd('\0');
            }

            var presPackage = JsonSerializer.Deserialize<MirrorPresentationDescription>(dfiletext);

            // load all slides according to the package labels

            foreach (var _s in presPackage.Slides.OrderBy(x => x.Num))
            {
                MSlide slide = new MSlide
                {
                    Title = "",
                    Type = _s.SlideType,
                    AutomationEnabled = _s.AutomationEnabled,
                    Actions = new List<TrackedAutomationAction>(),
                    SetupActions = new List<TrackedAutomationAction>(),
                    Guid = Guid.NewGuid(),
                    AutoPilotActions = new List<IPilotAction>(),
                    EmergencyActions = new List<IPilotAction>(),
                    PostsetEnabled = _s.HasPostset,
                    PostsetId = _s.PostsetInfo,
                    AutoOnly = _s.IsFullAuto,
                    PreAction = _s.PreAction,
                    Source = _s.PrimaryResource,
                    KeySource = _s.KeyResource,
                    AltSources = _s.HasOverrideKey || _s.HasOverridePrimary,
                    AltSource = _s.HasOverridePrimary ? _s.PrimaryResource : "",
                    AltKeySource = _s.HasOverrideKey ? _s.KeyResource : "",
                };

                if (_s.SlideType == SlideType.Action)
                {
                    using (var file = MemoryMappedFile.OpenExisting(_s.ActionInfo))
                    using (var stream = file.CreateViewStream())
                    using (var reader = new StreamReader(stream))
                    {
                        var text = reader.ReadToEnd().Trim().TrimEnd('\0');
                        if (ActionLoader.TryLoadActions(text, "", out var loaded, checkRealMedia: false))
                        {
                            slide.Title = loaded.Title;
                            slide.SetupActions = loaded.SetupActions;
                            slide.Actions = loaded.Actions;
                            slide.AutoOnly = loaded.AutoOnly;
                        }
                    }
                }

                if (_s.HasPilot)
                {
                    using (var file = MemoryMappedFile.OpenExisting(_s.PilotInfo))
                    using (var stream = file.CreateViewStream())
                    using (var reader = new StreamReader(stream))
                    {
                        var text = reader.ReadToEnd().Trim().TrimEnd('\0');
                        PilotActionBuilder.BuildPilotActions(text, out var act, out var emg);
                        slide.AutoPilotActions = act;
                        slide.EmergencyActions = emg;
                    }
                }

                pres.Slides.Add(slide);
            }


            if (!string.IsNullOrEmpty(presPackage.BMDCfgFile))
            {
                // load 'er up
                using (var file = MemoryMappedFile.OpenExisting(presPackage.BMDCfgFile))
                using (var stream = file.CreateViewStream())
                using (var reader = new StreamReader(stream))
                {
                    var text = reader.ReadToEnd().Trim().TrimEnd('\0');
                    var cfg = JsonSerializer.Deserialize<BMDSwitcherConfigSettings>(text);
                    pres.HasSwitcherConfig = true;
                    pres.SwitcherConfig = cfg;
                }
            }

            if (!string.IsNullOrEmpty(presPackage.CCUCfgFile))
            {
                // load 'er up
                using (var file = MemoryMappedFile.OpenExisting(presPackage.CCUCfgFile))
                using (var stream = file.CreateViewStream())
                using (var reader = new StreamReader(stream))
                {
                    var text = reader.ReadToEnd().Trim().TrimEnd('\0');
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        var cfg = JsonSerializer.Deserialize<CCPUConfig_Extended>(text);
                        pres.HasCCUConfig = true;
                        pres.CCPUConfig = cfg;
                    }
                }
            }

            // ignore user config



            return pres;
        }

    }
}
