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
                    Actions = new List<TrackedAutomationAction>(),// do this later
                    SetupActions = new List<TrackedAutomationAction>(), // do this later
                    Guid = Guid.NewGuid(),
                    AutoPilotActions = new List<IPilotAction>(), // do this later
                    EmergencyActions = new List<IPilotAction>(), // do this later
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
                        var text = reader.ReadToEnd();
                        if (ActionLoader.TryLoadActions(text, "", out var loaded, checkRealMedia: false))
                        {
                            slide.Title = loaded.Title;
                            slide.SetupActions = loaded.SetupActions;
                            slide.Actions = loaded.Actions;
                            slide.AutoOnly = loaded.AutoOnly;
                            // think we'll leave getting overrides from metadata
                        }
                    }
                }

                if (_s.HasPilot)
                {
                    using (var file = MemoryMappedFile.OpenExisting(_s.PilotInfo))
                    using (var stream = file.CreateViewStream())
                    using (var reader = new StreamReader(stream))
                    {
                        var text = reader.ReadToEnd();

                    }
                }

                pres.Slides.Add(slide);
            }

            // ignore bmd config
            // ignore user config
            // ignore ccu config






            return pres;
        }

    }
}
