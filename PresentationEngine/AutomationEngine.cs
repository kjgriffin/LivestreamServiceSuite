using IntegratedPresenter.BMDSwitcher.Config;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace IntegratedPresenter.Presentation
{
    public class AutomationEngine
    {

        // Requirements
        /*
            Should abstract all the nasty presentation management stuff from whoever needs to use it
            Will have the correct dispatcher for threading
            manages the presentation
            has access to switcher manager to perform switcher commands
            handles the graphics display outputs
         */

        private Dispatcher _dispatcher;
        public Dispatcher Dispatcher => _dispatcher;

        private Presentation _presentation;
        public Presentation Presentation => _presentation;
        private bool isActivePresentation = false;
        public DriveMode CurrentSlideMode { get; set; }

        IBMDSwitcherManager _switcher;

        BMDSwitcherConfigSettings _config;

        GraphicsEngine _grahpicsEngine;

        public event PresentationStateUpdate OnPresentationSlideChanged;

        public AutomationEngine(Dispatcher d, ref IBMDSwitcherManager switcherManager, BMDSwitcherConfigSettings config)
        {
            _dispatcher = d;
            _grahpicsEngine = new GraphicsEngine();
            _switcher = switcherManager;
            _config = config;
        }


        /// <summary>
        /// Opens a FileBrowser to select a presentation. If selected, will open and load a new presentation, initializing as needed.
        /// </summary>
        public void LoadPresentation()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select Presentation";
            if (ofd.ShowDialog() == true)
            {
                string path = System.IO.Path.GetDirectoryName(ofd.FileName);
                // create new presentation

                // TODO: add better error handling here

                try
                {
                    Presentation pres = new Presentation();
                    pres.Create(path);
                    pres.StartPres();
                    _presentation = pres;
                    isActivePresentation = true;
                }
                catch (Exception ex)
                {
                    // something here
                    return;
                }

                _grahpicsEngine.Initialize(this);

                //DisableSlidePoolOverrides();

                //slidesUpdated();
                // OLD BEHAVIOUR:
                // update the grahpics displays
                // update all previews (if slide did change (prevents videos from stopping unnessecarily)
                // updates media controls
                // updates setups/main actions completion state (we probably want to improve upon this anyways...)

                OnPresentationSlideChanged?.Invoke(Presentation.EffectiveCurrent);
            }

        }


        /// <summary>
        /// Close windows and release all resources
        /// </summary>
        public void Shutdown()
        {
            _grahpicsEngine?.Shutdown();
        }



        /// <summary>
        /// Go the previous slide/skip backwards based on CurrentSlideMode
        /// </summary>
        public void PrevSlide()
        {
            if (!isActivePresentation)
            {
                return;
            }
            if (CurrentSlideMode == DriveMode.Skip)
            {
                SkipBack();
            }
            else
            {
                BackSlide();
            }
        }

        private void BackSlide()
        {
            Presentation.PrevSlide();
            OnPresentationSlideChanged?.Invoke(Presentation.EffectiveCurrent);
        }

        private void SkipBack()
        {
            Presentation.SkipPrevSlide();
            OnPresentationSlideChanged?.Invoke(Presentation.EffectiveCurrent);
        }

        /// <summary>
        /// Go to the next slide/skip forward based on the CurrentSlideMode
        /// </summary>
        public void NextSlide()
        {
            if (!isActivePresentation)
            {
                return;
            }
            switch (CurrentSlideMode)
            {
                case DriveMode.Undriven:
                    NoDriveNext();
                    break;
                case DriveMode.Drive:
                    DriveNext();
                    break;
                case DriveMode.DriveTied:
                    DriveNextTied();
                    break;
                case DriveMode.Skip:
                    SkipNext();
                    break;
                default:
                    break;
            }
        }

        private void SkipNext()
        {
            Presentation.SkipNextSlide();
            OnPresentationSlideChanged?.Invoke(Presentation.EffectiveCurrent);
        }

        private void NoDriveNext()
        {
            Presentation.NextSlide();
            OnPresentationSlideChanged?.Invoke(Presentation.EffectiveCurrent);
        }

        private void DriveNextTied()
        {
            DriveNext(true);
        }


        private async void DriveNext(bool Tied = false)
        {
            if (Presentation?.Next != null)
            {

                if (Presentation.Next.AutomationEnabled)
                {
                    if (Presentation.Next.Type == SlideType.Action)
                    {
                        //SetupActionsCompleted = false;
                        //ActionsCompleted = false;
                        // run stetup actions
                        await ExecuteSetupActions(Presentation.Next);

                        if (Presentation.Next.AutoOnly)
                        {
                            // for now we won't support running 2 back to back fullauto slides.
                            // There really shouldn't be any need.
                            // We also cant run a script's setup actions immediatley afterward.
                            // again it shouldn't be nessecary, since in both cases you can add it to the fullauto slide's setup actions
                            Presentation.NextSlide();
                        }
                        // Perform slide actions
                        Presentation.NextSlide();
                        //slidesUpdated();
                        OnPresentationSlideChanged?.Invoke(Presentation.EffectiveCurrent);
                        await ExecuteActionSlide(Presentation.EffectiveCurrent);
                    }
                    else if (Presentation.Next.Type == SlideType.Liturgy)
                    {
                        // turn of usk1 if chroma keyer
                        if (_switcher?.GetCurrentState().USK1OnAir == true && _switcher?.GetCurrentState().USK1KeyType == 2)
                        {
                            _switcher?.PerformOffAirUSK1();
                        }
                        // make sure slides aren't the program source
                        if (_switcher?.GetCurrentState().ProgramID == _config.Routing.Where(r => r.KeyName == "slide").First().PhysicalInputId)
                        {
                            //_switcher?.PerformAutoTransition();
                            PerformGuardedAutoTransition();
                            await Task.Delay((_config.MixEffectSettings.Rate / _config.VideoSettings.VideoFPS) * 1000);
                        }
                        Presentation.NextSlide();
                        //slidesUpdated();
                        OnPresentationSlideChanged?.Invoke(Presentation.EffectiveCurrent);
                        if (Presentation.OverridePres == true)
                        {
                            Presentation.OverridePres = false;
                            //slidesUpdated();
                            OnPresentationSlideChanged?.Invoke(Presentation.EffectiveCurrent);
                        }
                        _switcher?.PerformAutoOnAirDSK1();
                        // request auto transition if tied and slides aren't preset source
                        if (Tied && _switcher?.GetCurrentState().PresetID != _config.Routing.Where(r => r.KeyName == "slide").First().PhysicalInputId)
                        {
                            PerformGuardedAutoTransition();
                        }
                    }
                    else if (Presentation.Next.Type == SlideType.ChromaKeyStill || Presentation.Next.Type == SlideType.ChromaKeyVideo)
                    {
                        // turn of downstream keys
                        if (_switcher?.GetCurrentState().DSK1OnAir == true)
                        {
                            _switcher?.PerformAutoOffAirDSK1();
                            await Task.Delay((_config.DownstreamKey1Config.Rate / _config.VideoSettings.VideoFPS) * 1000);
                        }

                        // set usk1 to chroma (using curently loaded settings)
                        if (_switcher?.GetCurrentState().USK1OnAir == true)
                        {
                            _switcher?.PerformOffAirUSK1();
                        }
                        if (_switcher?.GetCurrentState().USK1KeyType != 2)
                        {
                            _switcher?.SetUSK1TypeChroma();
                        }
                        // select fill source to be slide, since slide is marked as key it must be the key source
                        _switcher?.PerformUSK1FillSourceSelect(_config.Routing.Where(r => r.KeyName == "slide").First().PhysicalInputId);

                        // pull slide off air (and then reset the preview to the old source)
                        long previewsource = _switcher?.GetCurrentState().PresetID ?? 0;
                        if (_switcher?.GetCurrentState().ProgramID == _config.Routing.Where(r => r.KeyName == "slide").First().PhysicalInputId)
                        {
                            //_switcher?.PerformAutoTransition();
                            PerformGuardedAutoTransition();
                            await Task.Delay((_config.MixEffectSettings.Rate / _config.VideoSettings.VideoFPS) * 1000);
                        }
                        _switcher?.PerformPresetSelect((int)previewsource);

                        // next slide
                        Presentation.NextSlide();
                        //slidesUpdated();
                        OnPresentationSlideChanged?.Invoke(Presentation.EffectiveCurrent);
                        if (Presentation.OverridePres == true)
                        {
                            Presentation.OverridePres = false;
                            //slidesUpdated();
                            OnPresentationSlideChanged?.Invoke(Presentation.EffectiveCurrent);
                        }

                        // start mediaplayout
                        if (Presentation.EffectiveCurrent.Type == SlideType.ChromaKeyVideo)
                        {
                            _grahpicsEngine.PlayMedia();
                            await Task.Delay(_config?.PrerollSettings.ChromaVideoPreRoll ?? 0);
                        }

                        // turn on chroma key once playout has started
                        _switcher?.PerformOnAirUSK1();

                    }
                    else
                    {
                        // turn off usk1 if chroma keyer
                        if (_switcher?.GetCurrentState().USK1OnAir == true && _switcher?.GetCurrentState().USK1KeyType == 2)
                        {
                            _switcher?.PerformOffAirUSK1();
                        }
                        if (_switcher?.GetCurrentState().DSK1OnAir == true)
                        {
                            _switcher?.PerformAutoOffAirDSK1();
                            await Task.Delay((_config.DownstreamKey1Config.Rate / _config.VideoSettings.VideoFPS) * 1000);
                        }
                        Presentation.NextSlide();
                        //slidesUpdated();
                        OnPresentationSlideChanged?.Invoke(Presentation.EffectiveCurrent);
                        if (Presentation.OverridePres == true)
                        {
                            Presentation.OverridePres = false;
                            //slidesUpdated();
                            OnPresentationSlideChanged?.Invoke(Presentation.EffectiveCurrent);
                        }
                        if (Presentation.EffectiveCurrent.Type == SlideType.Video)
                        {
                            _grahpicsEngine.PlayMedia();
                            await Task.Delay(_config?.PrerollSettings.VideoPreRoll ?? 0);
                        }
                        if (_switcher?.GetCurrentState().ProgramID != _config.Routing.Where(r => r.KeyName == "slide").First().PhysicalInputId)
                        {
                            _switcher?.PerformPresetSelect(_config.Routing.Where(r => r.KeyName == "slide").First().PhysicalInputId);
                            await Task.Delay(_config.PrerollSettings.PresetSelectDelay);
                            //_switcher?.PerformAutoTransition();
                            PerformGuardedAutoTransition();
                        }

                    }
                }
                else
                {
                    Presentation.NextSlide();
                    //slidesUpdated();
                    OnPresentationSlideChanged?.Invoke(Presentation.EffectiveCurrent);
                    if (Presentation.OverridePres == true)
                    {
                        Presentation.OverridePres = false;
                        //slidesUpdated();
                        OnPresentationSlideChanged?.Invoke(Presentation.EffectiveCurrent);
                    }

                }
                // At this point we've switched to the slide
                //SlideDriveVideo_Action(Presentation.EffectiveCurrent);
            }



        }


        public void PlayMedia()
        {
            _grahpicsEngine.PlayMedia();
        }
        public void PauseMedia()
        {
            _grahpicsEngine.PauseMedia();
        }
        public void StopMedia()
        {
            _grahpicsEngine.StopMedia();
        }
        public void RestartMedia()
        {
            _grahpicsEngine.RestartMedia();
        }

        private async Task ExecuteSetupActions(Slide s)
        {
            Dispatcher.Invoke(() =>
            {
                //SetupActionsCompleted = false;
                //CurrentPreview.SetupComplete(false);
            });
            await Task.Run(async () =>
            {
                foreach (var task in s.SetupActions)
                {
                    await PerformAutomationAction(task);
                }
            });
            Dispatcher.Invoke(() =>
            {
                //SetupActionsCompleted = true;
                //CurrentPreview.SetupComplete(true);
            });
        }

        private async Task ExecuteActionSlide(Slide s)
        {
            Dispatcher.Invoke(() =>
            {
                //ActionsCompleted = false;
                //CurrentPreview.ActionComplete(false);
            });
            await Task.Run(async () =>
            {
                foreach (var task in s.Actions)
                {
                    await PerformAutomationAction(task);
                }
            });
            Dispatcher.Invoke(() =>
            {
                //ActionsCompleted = true;
                //CurrentPreview.ActionComplete(true);
            });
        }

        private async Task PerformAutomationAction(AutomationAction task)
        {
            await Task.Run(async () =>
            {
                switch (task.Action)
                {
                    case AutomationActionType.PresetSelect:
                        Dispatcher.Invoke(() =>
                        {
                            _switcher?.PerformPresetSelect(task.DataI);
                        });
                        break;
                    case AutomationActionType.ProgramSelect:
                        Dispatcher.Invoke(() =>
                        {
                            _switcher?.PerformProgramSelect(task.DataI);
                        });
                        break;
                    case AutomationActionType.AuxSelect:
                        Dispatcher.Invoke(() =>
                        {
                            _switcher?.PerformAuxSelect(task.DataI);
                        });
                        break;
                    case AutomationActionType.AutoTrans:
                        Dispatcher.Invoke(() =>
                        {
                            //_switcher?.PerformAutoTransition();
                            PerformGuardedAutoTransition();
                        });
                        break;
                    case AutomationActionType.CutTrans:
                        Dispatcher.Invoke(() =>
                        {
                            _switcher?.PerformCutTransition();
                        });
                        break;
                    case AutomationActionType.AutoTakePresetIfOnSlide:
                        // Take Preset if program source is fed from slides
                        if (_switcher?.GetCurrentState().ProgramID == _config.Routing.Where(r => r.KeyName == "slide").First().PhysicalInputId)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                //_switcher?.PerformAutoTransition();
                                PerformGuardedAutoTransition();
                            });
                            await Task.Delay((_config.MixEffectSettings.Rate / _config.VideoSettings.VideoFPS) * 1000);
                        }
                        break;
                    case AutomationActionType.DSK1On:
                        if ((bool)!_switcher?.GetCurrentState().DSK1OnAir)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                _switcher?.PerformToggleDSK1();
                            });
                        }
                        break;
                    case AutomationActionType.DSK1Off:
                        if ((bool)(_switcher?.GetCurrentState().DSK1OnAir))
                        {
                            Dispatcher.Invoke(() =>
                            {
                                _switcher?.PerformToggleDSK1();
                            });
                        }
                        break;
                    case AutomationActionType.DSK1FadeOn:
                        if ((bool)!_switcher?.GetCurrentState().DSK1OnAir)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                _switcher?.PerformAutoOnAirDSK1();
                            });
                        }
                        break;
                    case AutomationActionType.DSK1FadeOff:
                        if ((bool)(_switcher?.GetCurrentState().DSK1OnAir))
                        {
                            Dispatcher.Invoke(() =>
                            {
                                _switcher?.PerformAutoOffAirDSK1();
                            });
                        }
                        break;
                    case AutomationActionType.DSK2On:
                        if ((bool)!_switcher?.GetCurrentState().DSK2OnAir)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                _switcher?.PerformToggleDSK2();
                            });
                        }
                        break;
                    case AutomationActionType.DSK2Off:
                        if ((bool)(_switcher?.GetCurrentState().DSK2OnAir))
                        {
                            Dispatcher.Invoke(() =>
                            {
                                _switcher?.PerformToggleDSK2();
                            });
                        }
                        break;
                    case AutomationActionType.DSK2FadeOn:
                        if ((bool)!_switcher?.GetCurrentState().DSK2OnAir)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                _switcher?.PerformAutoOnAirDSK2();
                            });
                        }
                        break;
                    case AutomationActionType.DSK2FadeOff:
                        if ((bool)(_switcher?.GetCurrentState().DSK2OnAir))
                        {
                            Dispatcher.Invoke(() =>
                            {
                                _switcher?.PerformAutoOffAirDSK2();
                            });
                        }
                        break;
                    case AutomationActionType.RecordStart:
                        Dispatcher.Invoke(() =>
                        {
                            //TryStartRecording();
                        });
                        break;
                    case AutomationActionType.RecordStop:
                        Dispatcher.Invoke(() =>
                        {
                            //TryStopRecording();
                        });
                        break;

                    /*
                case AutomationActionType.Timer1Restart:
                    if (automationtimer1enabled)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            timer1span = TimeSpan.Zero;
                        });
                    }
                    break;
                    */

                    case AutomationActionType.USK1On:
                        if ((bool)!_switcher?.GetCurrentState().USK1OnAir)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                _switcher?.PerformOnAirUSK1();
                            });
                        }
                        break;
                    case AutomationActionType.USK1Off:
                        if ((bool)_switcher?.GetCurrentState().USK1OnAir)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                _switcher?.PerformOffAirUSK1();
                            });
                        }
                        break;
                    case AutomationActionType.USK1SetTypeChroma:
                        Dispatcher.Invoke(() =>
                        {
                            _switcher?.SetUSK1TypeChroma();
                        });
                        break;
                    case AutomationActionType.USK1SetTypeDVE:
                        Dispatcher.Invoke(() =>
                        {
                            _switcher?.SetUSK1TypeDVE();
                        });
                        break;

                    /*
                case AutomationActionType.OpenAudioPlayer:
                    Dispatcher.Invoke(() =>
                    {
                        OpenAudioPlayer();
                        Focus();
                    });
                    break;
                case AutomationActionType.LoadAudio:
                    string filename = Path.Join(Presentation.Folder, task.DataS);
                    Dispatcher.Invoke(() =>
                    {
                        audioPlayer.OpenAudio(filename);
                    });
                    break;
                case AutomationActionType.PlayAuxAudio:
                    Dispatcher.Invoke(() =>
                    {
                        audioPlayer.PlayAudio();
                    });
                    break;
                case AutomationActionType.StopAuxAudio:
                    Dispatcher.Invoke(() =>
                    {
                        audioPlayer.StopAudio();
                    });
                    break;
                case AutomationActionType.PauseAuxAudio:
                    Dispatcher.Invoke(() =>
                    {
                        audioPlayer.PauseAudio();
                    });
                    break;
                case AutomationActionType.ReplayAuxAudio:
                    Dispatcher.Invoke(() =>
                    {
                        audioPlayer.RestartAudio();
                    });
                    break;
                    */

                    case AutomationActionType.PlayMedia:
                        Dispatcher.Invoke(() =>
                        {
                            _grahpicsEngine?.PlayMedia();
                        });
                        break;
                    case AutomationActionType.PauseMedia:
                        Dispatcher.Invoke(() =>
                        {
                            _grahpicsEngine?.PauseMedia();
                        });
                        break;
                    case AutomationActionType.StopMedia:
                        Dispatcher.Invoke(() =>
                        {
                            _grahpicsEngine?.StopMedia();
                        });
                        break;
                    case AutomationActionType.RestartMedia:
                        Dispatcher.Invoke(() =>
                        {
                            _grahpicsEngine?.RestartMedia();
                        });
                        break;
                    case AutomationActionType.MuteMedia:
                        Dispatcher.Invoke(() =>
                        {
                            _grahpicsEngine?.MuteMedia();
                        });
                        break;
                    case AutomationActionType.UnMuteMedia:
                        Dispatcher.Invoke(() =>
                        {
                            _grahpicsEngine?.UnMuteMedia();
                        });
                        break;


                    case AutomationActionType.DelayMs:
                        await Task.Delay(task.DataI);
                        break;
                    case AutomationActionType.None:
                        break;
                    default:
                        break;
                }
            });
        }


        private bool DriveMode_AutoTransitionGuard = true;
        private void PerformGuardedAutoTransition(bool force_guard = false)
        {
            if (DriveMode_AutoTransitionGuard || force_guard)
            {
                // if guarded, check if transition is already in progress
                if (_switcher?.GetCurrentState().InTransition == false)
                {
                    _switcher?.PerformAutoTransition();
                }
            }
            else
            {
                _switcher?.PerformAutoTransition();
            }
        }






    }
}
