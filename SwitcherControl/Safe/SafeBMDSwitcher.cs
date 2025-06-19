using ATEMSharedState.SwitcherState;

using BMDSwitcherAPI;

using Configurations.SwitcherConfig;

using IntegratedPresenter.BMDSwitcher;
using IntegratedPresenter.BMDSwitcher.Config;

using log4net;

using SwitcherControl.BMDSwitcher;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace SwitcherControl.Safe
{

    public class SafeBMDSwitcher : IBMDSwitcherManager
    {

        private IBMDSwitcherDiscovery _BMDSwitcherDiscovery;
        private IBMDSwitcher _BMDSwitcher;

        private IBMDSwitcherMixEffectBlock _BMDSwitcherMixEffectBlock1;
        private IBMDSwitcherInputAux _BMDSwitcherAuxInput;
        private IBMDSwitcherKey _BMDSwitcherUpstreamKey1;
        private IBMDSwitcherDownstreamKey _BMDSwitcherDownstreamKey1;
        private IBMDSwitcherDownstreamKey _BMDSwitcherDownstreamKey2;
        private List<IBMDSwitcherInput> _BMDSwitcherInputs;
        private IBMDSwitcherMultiView _BMDSwitcherMultiView;
        private IBMDSwitcherMediaPlayer _BMDSwitcherMediaPlayer1;
        private IBMDSwitcherMediaPlayer _BMDSwitcherMediaPlayer2;
        private IBMDSwitcherMediaPool _BMDSwitcherMediaPool;
        private IBMDSwitcherKeyFlyParameters _BMDSwitcherFlyKeyParamters;
        private IBMDSwitcherTransitionParameters _BMDSwitcherTransitionParameters;

        private SwitcherMonitor _switcherMonitor;
        private MixEffectBlockMonitor _mixEffectBlockMonitor;
        private BMDSwitcherAuxMonitor _auxMonitor;
        private UpstreamKeyMonitor _upstreamKey1Monitor;
        private BMDSwitcherPatternKeyMonitor _patternKeyMonitor;
        private DownstreamKeyMonitor _dsk1Monitor;
        private DownstreamKeyMonitor _dsk2Monitor;
        private List<InputMonitor> _inputMonitors;
        private SwitcherFlyKeyMonitor _flykeyMonitor;
        private SwitcherTransitionMonitor _transitionMontitor;
        // perhaps don't need to monitor multiviewer
        // for now won't have mediaplayer monitors

        private BMDSwitcherState _state;


        public event SwitcherStateChange SwitcherStateChanged;
        public event EventHandler<bool> OnSwitcherConnectionChanged;

        public bool GoodConnection { get; set; } = false;

        private bool IsInitialized = false;

        private BMDSwitcherConfigSettings _config;

        private ILog _logger;

        ManualResetEvent _autoTransMRE;

        SingleThreadedExecutor _executor;

        public SafeBMDSwitcher(ManualResetEvent autoTransMRE, ILog log, string versionTitle = "unknown version")
        {
            _autoTransMRE = autoTransMRE;
            _executor = new SingleThreadedExecutor();
            _logger = log;

            // make sure that any initialization happens on the dedicated thread
            _executor.EnqueueWork(() => Initialize(versionTitle));
        }

        #region Internal API Implementation

        private void Initialize(string versionTitle)
        {
            if (_autoTransMRE == null)
            {
                _autoTransMRE = new ManualResetEvent(false);
            }
            // initialize logger
            // enable logging
            using (Stream cstream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Integrated_Presenter.Log4Net_switcher.config"))
            {
                log4net.Config.XmlConfigurator.Configure(cstream);
            }

            _logger = LogManager.GetLogger("SwitcherLogger");
            _logger?.Info($"[BMD HW] Switcher Logger for {versionTitle} Started.");


            _inputMonitors = new List<InputMonitor>();
            _BMDSwitcherInputs = new List<IBMDSwitcherInput>();

            _switcherMonitor = new SwitcherMonitor();
            _switcherMonitor.SwitcherDisconnected += _switcherMonitor_SwitcherDisconnected;


            _mixEffectBlockMonitor = new MixEffectBlockMonitor();
            _mixEffectBlockMonitor.PreviewInputChanged += _mixEffectBlockMonitor_PreviewInputChanged;
            _mixEffectBlockMonitor.ProgramInputChanged += _mixEffectBlockMonitor_ProgramInputChanged;
            _mixEffectBlockMonitor.FateToBlackFullyChanged += _mixEffectBlockMonitor_FateToBlackFullyChanged;
            _mixEffectBlockMonitor.InTransitionChanged += _mixEffectBlockMonitor_InTransitionChanged;

            _auxMonitor = new BMDSwitcherAuxMonitor();
            _auxMonitor.OnAuxInputChanged += _auxMonitor_OnAuxInputChanged;

            _upstreamKey1Monitor = new UpstreamKeyMonitor();
            _upstreamKey1Monitor.UpstreamKeyOnAirChanged += _upstreamKey1Monitor_UpstreamKeyOnAirChanged;
            _upstreamKey1Monitor.UpstreamKeyFillChanged += _upstreamKey1Monitor_UpstreamKeyFillChanged;
            _upstreamKey1Monitor.UpstreamKeyTypeChanged += _upstreamKey1Monitor_UpstreamKeyTypeChanged;
            _upstreamKey1Monitor.UpstreamKeyCutChanged += _upstreamKey1Monitor_UpstreamKeyCutChanged;
            _upstreamKey1Monitor.UpstreamKeyMaskChanged += _upstreamKey1Monitor_UpstreamKeyMaskChanged;

            _patternKeyMonitor = new BMDSwitcherPatternKeyMonitor();
            _patternKeyMonitor.OnAnyChange += _patternKeyMonitor_OnAnyChange;

            _flykeyMonitor = new SwitcherFlyKeyMonitor();
            _flykeyMonitor.KeyFrameChanged += _flykeyMonitor_KeyFrameChanged;
            _flykeyMonitor.KeyFrameStateChange += _flykeyMonitor_KeyFrameStateChange;

            _transitionMontitor = new SwitcherTransitionMonitor();
            _transitionMontitor.OnTransitionSelectionChanged += _transitionMontitor_OnNextTransitionSelectionChanged;

            _dsk1Monitor = new DownstreamKeyMonitor();
            _dsk1Monitor.OnAirChanged += _dsk1Manager_OnAirChanged;
            _dsk1Monitor.TieChanged += _dsk1Manager_TieChanged;
            _dsk2Monitor = new DownstreamKeyMonitor();
            _dsk2Monitor.OnAirChanged += _dsk2Manager_OnAirChanged;
            _dsk2Monitor.TieChanged += _dsk2Manager_TieChanged;

            _BMDSwitcherDiscovery = new CBMDSwitcherDiscovery();
            if (_BMDSwitcherDiscovery == null)
            {
                _logger?.Error("Could not create Switcher Discovery Instance.\nATEM Switcher Software not found/installed");
                MessageBox.Show("Could not create Switcher Discovery Instance.\nATEM Switcher Software not found/installed", "Error");
            }
            _state = new BMDSwitcherState();
            SwitcherDisconnected();
            _logger?.Info("Initialized Switcher with disconnected state. Setup monitor callbacks.");
            IsInitialized = true;
        }

        private void _patternKeyMonitor_OnAnyChange(object sender, EventArgs e)
        {
            _logger?.Debug($"[BMD HW] EVENT ON {System.Reflection.MethodBase.GetCurrentMethod()}");
            _executor.EnqueueWork(() =>
            {
                ForceStateUpdate_PatternSettings();
                SwitcherStateChanged?.Invoke(_state);
            });
        }

        private void _mixEffectBlockMonitor_InTransitionChanged(object sender, object args)
        {
            _logger?.Debug($"[BMD HW] EVENT ON {System.Reflection.MethodBase.GetCurrentMethod()}");
            _executor.EnqueueWork(() =>
            {
                ForceStateUpdate_TransitionPosition();
                SwitcherStateChanged?.Invoke(_state);
                if (_state.InTransition == false)
                {
                    _autoTransMRE.Set();
                }
            });
        }

        private void _upstreamKey1Monitor_UpstreamKeyMaskChanged(object sender, object args)
        {
            _logger?.Debug($"[BMD HW] EVENT ON {System.Reflection.MethodBase.GetCurrentMethod()}");
            _executor.EnqueueWork(() =>
            {
                ForceStateUpdate_USK1();
                SwitcherStateChanged?.Invoke(_state);
            });
        }

        private void _upstreamKey1Monitor_UpstreamKeyCutChanged(object sender, object args)
        {
            _logger?.Debug($"[BMD HW] EVENT ON {System.Reflection.MethodBase.GetCurrentMethod()}");
            _executor.EnqueueWork(() =>
            {
                ForceStateUpdate_USK1();
                SwitcherStateChanged?.Invoke(_state);
            });
        }

        private void _flykeyMonitor_KeyFrameStateChange(object sender, object args)
        {
            _logger?.Debug($"[BMD HW] EVENT ON {System.Reflection.MethodBase.GetCurrentMethod()}");
            _executor.EnqueueWork(() =>
            {
                ForceStateUpdate_USK1();
                ForceStateUpdate_PIPSettings();
                SwitcherStateChanged?.Invoke(_state);
            });
        }

        private void _auxMonitor_OnAuxInputChanged(object sender, object args)
        {
            _logger?.Debug($"[BMD HW] EVENT ON {System.Reflection.MethodBase.GetCurrentMethod()}");
            _executor.EnqueueWork(() =>
            {
                ForceStateUpdate_AuxInput();
                SwitcherStateChanged?.Invoke(_state);
            });
        }

        private void _upstreamKey1Monitor_UpstreamKeyTypeChanged(object sender, object args)
        {
            _logger?.Debug($"[BMD HW] EVENT ON {System.Reflection.MethodBase.GetCurrentMethod()}");
            _executor.EnqueueWork(() =>
            {
                ForceStateUpdate_USK1();
                ForceStateUpdate_ChromaSettings();
                ForceStateUpdate_PIPSettings();
                SwitcherStateChanged?.Invoke(_state);

            });
        }

        private void _upstreamKey1Monitor_UpstreamKeyFillChanged(object sender, object args)
        {
            _logger?.Debug($"[BMD HW] EVENT ON {System.Reflection.MethodBase.GetCurrentMethod()}");
            _executor.EnqueueWork(() =>
            {
                ForceStateUpdate_USK1();
                SwitcherStateChanged?.Invoke(_state);
            });
        }

        private void _transitionMontitor_OnNextTransitionSelectionChanged(object sender)
        {
            _logger?.Debug($"[BMD HW] EVENT ON {System.Reflection.MethodBase.GetCurrentMethod()}");
            _executor.EnqueueWork(() =>
            {
                ForceStateUpdate_Transition();
                SwitcherStateChanged?.Invoke(_state);
            });
        }

        private void _flykeyMonitor_KeyFrameChanged(object sender, int keyframe)
        {
            _logger?.Debug($"[BMD HW] EVENT ON {System.Reflection.MethodBase.GetCurrentMethod()}");
            _executor.EnqueueWork(() =>
            {
                ForceStateUpdate_USK1();
                SwitcherStateChanged?.Invoke(_state);
            });
        }

        private void InputMonitor_ShortNameChanged(object sender, object args)
        {
            //throw new NotImplementedException();
            _logger?.Debug($"[BMD HW] EVENT ON {System.Reflection.MethodBase.GetCurrentMethod()}");
        }

        private void InputMonitor_LongNameChanged(object sender, object args)
        {
            //throw new NotImplementedException();
            _logger?.Debug($"[BMD HW] EVENT ON {System.Reflection.MethodBase.GetCurrentMethod()}");
        }


        private void _upstreamKey1Monitor_UpstreamKeyOnAirChanged(object sender, object args)
        {
            _logger?.Debug($"[BMD HW] EVENT ON {System.Reflection.MethodBase.GetCurrentMethod()}");
            _executor.EnqueueWork(() =>
            {
                ForceStateUpdate_USK1();
                SwitcherStateChanged?.Invoke(_state);
            });
        }

        private void _mixEffectBlockMonitor_FateToBlackFullyChanged(object sender, object args)
        {
            _logger?.Debug($"[BMD HW] EVENT ON {System.Reflection.MethodBase.GetCurrentMethod()}");
            _executor.EnqueueWork(() =>
            {
                ForceStateUpdate_FTB();
                SwitcherStateChanged?.Invoke(_state);
            });
        }

        private void _dsk2Manager_TieChanged(object sender, object args)
        {
            _logger?.Debug($"[BMD HW] EVENT ON {System.Reflection.MethodBase.GetCurrentMethod()}");
            _executor.EnqueueWork(() =>
            {
                ForceStateUpdate_DSK2();
                SwitcherStateChanged?.Invoke(_state);
            });
        }

        private void _dsk2Manager_OnAirChanged(object sender, object args)
        {
            _logger?.Debug($"[BMD HW] EVENT ON {System.Reflection.MethodBase.GetCurrentMethod()}");
            _executor.EnqueueWork(() =>
            {
                ForceStateUpdate_DSK2();
                SwitcherStateChanged?.Invoke(_state);
            });
        }

        private void _dsk1Manager_TieChanged(object sender, object args)
        {
            _logger?.Debug($"[BMD HW] EVENT ON {System.Reflection.MethodBase.GetCurrentMethod()}");
            _executor.EnqueueWork(() =>
            {
                ForceStateUpdate_DSK1();
                SwitcherStateChanged?.Invoke(_state);
            });
        }

        private void _dsk1Manager_OnAirChanged(object sender, object args)
        {
            _logger?.Debug($"[BMD HW] EVENT ON {System.Reflection.MethodBase.GetCurrentMethod()}");
            _executor.EnqueueWork(() =>
            {
                ForceStateUpdate_DSK1();
                SwitcherStateChanged?.Invoke(_state);
            });
        }

        private void _mixEffectBlockMonitor_ProgramInputChanged(object sender, object args)
        {
            _logger?.Debug($"[BMD HW] EVENT ON {System.Reflection.MethodBase.GetCurrentMethod()}");
            _executor.EnqueueWork(() =>
            {
                ForceStateUpdate_ProgramInput();
                SwitcherStateChanged?.Invoke(_state);
            });
        }

        private void _mixEffectBlockMonitor_PreviewInputChanged(object sender, object args)
        {
            _logger?.Debug($"[BMD HW] EVENT ON {System.Reflection.MethodBase.GetCurrentMethod()}");
            _executor.EnqueueWork(() =>
            {
                ForceStateUpdate_PreviewInput();
                SwitcherStateChanged?.Invoke(_state);
            });
        }

        private void _switcherMonitor_SwitcherDisconnected(object sender, object args)
        {
            _logger?.Debug($"[BMD HW] EVENT ON {System.Reflection.MethodBase.GetCurrentMethod()}");
            _executor.EnqueueWork(() =>
            {
                SwitcherDisconnected();
            });
        }


        private void TryConnect(string address)
        {
            while (!IsInitialized) ;

            _BMDSwitcherConnectToFailure failReason = 0;
            try
            {
                _logger?.Info($"Attempting synchronous connection to switcher on {address}");
                _BMDSwitcherDiscovery.ConnectTo(address, out _BMDSwitcher, out failReason);
                SwitcherConnected();
                OnSwitcherConnectionChanged?.Invoke(this, true);
            }
            catch (COMException)
            {
                switch (failReason)
                {
                    case _BMDSwitcherConnectToFailure.bmdSwitcherConnectToFailureNoResponse:
                        _logger?.Error($"Failed to connect to bmd switcher. No Response.");
                        MessageBox.Show("No response from Switcher", "Error");
                        break;
                    case _BMDSwitcherConnectToFailure.bmdSwitcherConnectToFailureIncompatibleFirmware:
                        _logger?.Error($"Failed to connect to bmd switcher. Incompatible firmware.");
                        MessageBox.Show("Switcher has incompatible firmware", "Error");
                        break;
                    default:
                        _logger?.Error($"Failed to connect to bmd switcher. Unknown reason.");
                        MessageBox.Show("Switcher failed to connect for unknown reason", "Error");
                        break;
                }
                OnSwitcherConnectionChanged?.Invoke(this, false);
            }

        }

        private void Disconnect()
        {
            _logger?.Debug($"[BMD HW] Runnning {System.Reflection.MethodBase.GetCurrentMethod()}");
            OnSwitcherConnectionChanged?.Invoke(this, false);
            SwitcherDisconnected();
        }

        private bool InitializeInputSources()
        {
            _logger?.Info("[BMD HW] Initializing Input Sources");
            // get all input sources
            IBMDSwitcherInputIterator inputIterator = null;
            IntPtr inputIteratorPtr;
            Guid inputIteratorIID = typeof(IBMDSwitcherInputIterator).GUID;
            _BMDSwitcher.CreateIterator(ref inputIteratorIID, out inputIteratorPtr);
#pragma warning disable CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'
            if (inputIteratorPtr != null)
#pragma warning restore CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'
            {
                inputIterator = (IBMDSwitcherInputIterator)Marshal.GetObjectForIUnknown(inputIteratorPtr);
            }
            else
            {
                _logger?.Error($"[BMD HW] input iterator null");
                return false;
            }
            if (inputIterator != null)
            {
                IBMDSwitcherInput input;
                inputIterator.Next(out input);
                while (input != null)
                {
                    _BMDSwitcherInputs.Add(input);
                    InputMonitor inputMonitor = new InputMonitor(input);
                    input.AddCallback(inputMonitor);
                    inputMonitor.LongNameChanged += InputMonitor_LongNameChanged;
                    inputMonitor.ShortNameChanged += InputMonitor_ShortNameChanged;
                    _inputMonitors.Add(inputMonitor);
                    inputIterator.Next(out input);
                }
            }
            else
            {
                _logger?.Error($"[BMD HW] failed to iterate inputs");
                return false;
            }

            return true;
        }

        private bool InitializeAuxInput()
        {
            _logger?.Info("[BMD HW] Initializing Aux Sources");
            // get all input sources
            IBMDSwitcherInputIterator inputIterator = null;
            IntPtr inputIteratorPtr;
            Guid inputIteratorIID = typeof(IBMDSwitcherInputIterator).GUID;
            _BMDSwitcher.CreateIterator(ref inputIteratorIID, out inputIteratorPtr);
#pragma warning disable CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'
            if (inputIteratorPtr != null)
#pragma warning restore CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'
            {
                inputIterator = (IBMDSwitcherInputIterator)Marshal.GetObjectForIUnknown(inputIteratorPtr);
            }
            else
            {
                _logger?.Error($"[BMD HW] aux iterator null");
                return false;
            }
            if (inputIterator != null)
            {
                IBMDSwitcherInput aux;
                inputIterator.GetById((long)BMDSwitcherVideoSources.Auxillary1, out aux);
                _BMDSwitcherAuxInput = (IBMDSwitcherInputAux)aux;
                _BMDSwitcherAuxInput.AddCallback(_auxMonitor);
                return true;
            }

            _logger?.Error($"[BMD HW] failed to iterate aux sources");
            return false;
        }

        private bool InitializeMultiView()
        {
            _logger?.Info("[BMD HW] Initializing Multiview");
            IntPtr multiViewPtr;
            Guid multiViewIID = typeof(IBMDSwitcherMultiViewIterator).GUID;
            _BMDSwitcher.CreateIterator(ref multiViewIID, out multiViewPtr);
#pragma warning disable CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'
            if (multiViewPtr == null)
#pragma warning restore CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'
            {
                return false;
            }
            IBMDSwitcherMultiViewIterator multiViewIterator = (IBMDSwitcherMultiViewIterator)Marshal.GetObjectForIUnknown(multiViewPtr);
            if (multiViewIterator == null)
            {
                _logger?.Error($"[BMD HW] failed to initialize multiview");
                return false;
            }

            multiViewIterator.Next(out _BMDSwitcherMultiView);

            return true;
        }

        private bool InitializeMixEffectBlock()
        {
            _logger?.Info($"[BMD HW] initialize mix effect blocks");
            // get mixeffectblock1
            IBMDSwitcherMixEffectBlockIterator meIterator = null;
            IntPtr meIteratorPtr;
            Guid meIteratorIID = typeof(IBMDSwitcherMixEffectBlockIterator).GUID;
            _BMDSwitcher.CreateIterator(ref meIteratorIID, out meIteratorPtr);
#pragma warning disable CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'
            if (meIteratorPtr != null)
#pragma warning restore CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'
            {
                meIterator = (IBMDSwitcherMixEffectBlockIterator)Marshal.GetObjectForIUnknown(meIteratorPtr);
            }
            if (meIterator == null)
            {
                _logger?.Error($"[BMD HW] mix effect block iterator null");
                return false;
            }


            meIterator.Next(out _BMDSwitcherMixEffectBlock1);

            if (_BMDSwitcherMixEffectBlock1 == null)
            {
                _logger?.Error($"[BMD HW] failed to initialize mix effect block");
                MessageBox.Show("Unexpected: Could not get first mix effect block", "Error");
                return false;
            }

            // add callbacks
            _BMDSwitcherMixEffectBlock1.AddCallback(_mixEffectBlockMonitor);

            // get transitions
            _BMDSwitcherTransitionParameters = (IBMDSwitcherTransitionParameters)_BMDSwitcherMixEffectBlock1;
            _BMDSwitcherTransitionParameters.AddCallback(_transitionMontitor);

            return true;
        }

        private bool InitializeUpstreamKeyers()
        {
            _logger?.Info($"[BMD HW] initialize upstream keyers");
            IBMDSwitcherKeyIterator keyIterator;
            IntPtr keyItrPtr;
            Guid keyItrIID = typeof(IBMDSwitcherKeyIterator).GUID;
            _BMDSwitcherMixEffectBlock1.CreateIterator(ref keyItrIID, out keyItrPtr);

            keyIterator = (IBMDSwitcherKeyIterator)Marshal.GetObjectForIUnknown(keyItrPtr);
            keyIterator.Next(out _BMDSwitcherUpstreamKey1);

            _BMDSwitcherUpstreamKey1.AddCallback(_upstreamKey1Monitor);

            _BMDSwitcherFlyKeyParamters = (IBMDSwitcherKeyFlyParameters)_BMDSwitcherUpstreamKey1;
            _BMDSwitcherFlyKeyParamters.AddCallback(_flykeyMonitor);

            IBMDSwitcherKeyPatternParameters pattern = (IBMDSwitcherKeyPatternParameters)_BMDSwitcherUpstreamKey1;
            pattern.AddCallback(_patternKeyMonitor);

            return true;
        }

        private bool InitializeDownstreamKeyers()
        {
            _logger?.Info($"[BMD HW] initialize downstream keyers");
            // get downstream keyers
            IBMDSwitcherDownstreamKeyIterator dskIterator = null;
            IntPtr dskIteratorPtr;
            Guid dskIteratorIID = typeof(IBMDSwitcherDownstreamKeyIterator).GUID;
            _BMDSwitcher.CreateIterator(ref dskIteratorIID, out dskIteratorPtr);
#pragma warning disable CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'
            if (dskIteratorPtr != null)
#pragma warning restore CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'
            {
                dskIterator = (IBMDSwitcherDownstreamKeyIterator)Marshal.GetObjectForIUnknown(dskIteratorPtr);
            }
            if (dskIterator == null)
            {
                _logger?.Error($"[BMD HW] downstream key iterator null");
                return false;
            }

            if (dskIterator != null)
            {
                // get first dsk
                dskIterator.Next(out _BMDSwitcherDownstreamKey1);
            }
            if (dskIterator != null)
            {
                // get second dsk
                dskIterator.Next(out _BMDSwitcherDownstreamKey2);
            }

            if (_BMDSwitcherDownstreamKey1 == null || _BMDSwitcherDownstreamKey2 == null)
            {
                _logger?.Error($"[BMD HW] failed to iterate downstream keyers");
                MessageBox.Show("Unexpected: Could not get one of the downstream keyers", "Error");
                return false;
            }

            // add callbacks
            _BMDSwitcherDownstreamKey1.AddCallback(_dsk1Monitor);
            _BMDSwitcherDownstreamKey2.AddCallback(_dsk2Monitor);

            return true;

        }

        private bool InitializeMediaPool()
        {
            _logger?.Info($"[BMD HW] initialize media pool");
            _BMDSwitcherMediaPool = (IBMDSwitcherMediaPool)_BMDSwitcher;
            return true;
        }

        private bool InitializeMediaPlayers()
        {
            _logger?.Info($"[BMD HW] initialize media players");
            IBMDSwitcherMediaPlayerIterator mediaPlayerIterator = null;
            IntPtr mediaPlayerPtr;
            Guid mediaPlayerIID = typeof(IBMDSwitcherMediaPlayerIterator).GUID;
            _BMDSwitcher.CreateIterator(ref mediaPlayerIID, out mediaPlayerPtr);
#pragma warning disable CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'
            if (mediaPlayerPtr == null)
#pragma warning restore CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'
            {
                _logger?.Error($"[BMD HW] media player iterator null");
                return false;
            }
            mediaPlayerIterator = (IBMDSwitcherMediaPlayerIterator)Marshal.GetObjectForIUnknown(mediaPlayerPtr);
            if (mediaPlayerIterator == null)
            {
                _logger?.Error($"[BMD HW] failed to iterate media players");
                return false;
            }

            mediaPlayerIterator.Next(out _BMDSwitcherMediaPlayer1);
            mediaPlayerIterator.Next(out _BMDSwitcherMediaPlayer2);

            return true;
        }

        private void SwitcherConnected()
        {
            _logger?.Debug($"[BMD HW] Called SwitcherConnected()");
            // add callbacks (monitors switcher connection)
            _BMDSwitcher.AddCallback(_switcherMonitor);


            bool mixeffects = InitializeMixEffectBlock();
            bool upstreamkeyers = InitializeUpstreamKeyers();
            bool downstreamkeyers = InitializeDownstreamKeyers();

            bool inputsources = InitializeInputSources();
            bool auxsource = InitializeAuxInput();
            bool multiviewer = InitializeMultiView();

            bool mediapool = InitializeMediaPool();
            bool mediaplayers = InitializeMediaPlayers();

            GoodConnection = mixeffects && auxsource && downstreamkeyers && upstreamkeyers && inputsources && multiviewer && mediaplayers && mediapool;

            // update state
            ForceStateUpdate();
            SwitcherStateChanged?.Invoke(_state);
        }

        private void SwitcherDisconnected()
        {
            _logger?.Debug($"[BMD HW] Called SwitcherDisconnected(). Removing Monitor Callbacks");
            GoodConnection = false;

            // remove callbacks
            if (_BMDSwitcherMixEffectBlock1 != null)
            {
                _BMDSwitcherMixEffectBlock1.RemoveCallback(_mixEffectBlockMonitor);
                _BMDSwitcherMixEffectBlock1 = null;
            }

            if (_BMDSwitcherUpstreamKey1 != null)
            {
                _BMDSwitcherUpstreamKey1.RemoveCallback(_upstreamKey1Monitor);
                _BMDSwitcherUpstreamKey1 = null;
            }

            if (_BMDSwitcherDownstreamKey1 != null)
            {
                _BMDSwitcherDownstreamKey1.RemoveCallback(_dsk1Monitor);
                _BMDSwitcherDownstreamKey1 = null;
            }
            if (_BMDSwitcherDownstreamKey2 != null)
            {
                _BMDSwitcherDownstreamKey2.RemoveCallback(_dsk2Monitor);
                _BMDSwitcherDownstreamKey2 = null;
            }

            if (_BMDSwitcherAuxInput != null)
            {
                _BMDSwitcherAuxInput.RemoveCallback(_auxMonitor);
                _BMDSwitcherAuxInput = null;
            }

            if (_BMDSwitcher != null)
            {
                _BMDSwitcher.RemoveCallback(_switcherMonitor);
                _switcherMonitor = null;
            }

            int i = 0;
            foreach (var input in _BMDSwitcherInputs)
            {
                input.RemoveCallback(_inputMonitors[i++]);
            }
            _inputMonitors.Clear();
            _BMDSwitcherInputs.Clear();

            if (_BMDSwitcherMultiView != null)
            {
                // no callback yet
                _BMDSwitcherMultiView = null;
            }

        }



        private BMDSwitcherState ForceStateUpdate()
        {
            _logger?.Debug($"[BMD HW] Called ForceStateUpdate()");
            if (GoodConnection)
            {
                // update state
                ForceStateUpdate_ProgramInput();
                ForceStateUpdate_PreviewInput();
                ForceStateUpdate_AuxInput();
                ForceStateUpdate_Transition();
                ForceStateUpdate_USK1();
                ForceStateUpdate_ChromaSettings();
                ForceStateUpdate_PIPSettings();
                ForceStateUpdate_PatternSettings();
                ForceStateUpdate_DSK1();
                ForceStateUpdate_DSK2();
                ForceStateUpdate_FTB();
            }
            return _state;
        }



        private void ForceStateUpdate_AuxInput()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            long source;
            _BMDSwitcherAuxInput.GetInputSource(out source);
            _state.AuxID = source;
        }

        private void ForceStateUpdate_USK1()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            int onair;
            _BMDSwitcherUpstreamKey1.GetOnAir(out onair);
            _state.USK1OnAir = onair != 0;

            _BMDSwitcherKeyType keytype;
            _BMDSwitcherUpstreamKey1.GetType(out keytype);

            switch (keytype)
            {
                case _BMDSwitcherKeyType.bmdSwitcherKeyTypeLuma:
                    _state.USK1KeyType = 4;
                    break;
                case _BMDSwitcherKeyType.bmdSwitcherKeyTypeChroma:
                    _state.USK1KeyType = 2;
                    break;
                case _BMDSwitcherKeyType.bmdSwitcherKeyTypePattern:
                    _state.USK1KeyType = 3;
                    break;
                case _BMDSwitcherKeyType.bmdSwitcherKeyTypeDVE:
                    _state.USK1KeyType = 1;
                    break;
                default:
                    _state.USK1KeyType = 0;
                    break;
            }

            _BMDSwitcherFlyKeyFrame frame;
            _BMDSwitcherFlyKeyParamters.IsAtKeyFrames(out frame);

            switch (frame)
            {
                case _BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameFull:
                    _state.USK1KeyFrame = 0;
                    break;
                case _BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameA:
                    _state.USK1KeyFrame = 1;
                    break;
                case _BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameB:
                    _state.USK1KeyFrame = 2;
                    break;
                default:
                    _state.USK1KeyFrame = -1;
                    break;
            }

            long usk1fill;
            _BMDSwitcherUpstreamKey1.GetInputFill(out usk1fill);
            _state.USK1FillSource = usk1fill;

        }
        private void ForceStateUpdate_DSK1()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            int dsk1onair;
            int dsk1tie;
            _BMDSwitcherDownstreamKey1.GetOnAir(out dsk1onair);
            _BMDSwitcherDownstreamKey1.GetTie(out dsk1tie);

            _state.DSK1OnAir = dsk1onair != 0;
            _state.DSK1Tie = dsk1tie != 0;
        }
        private void ForceStateUpdate_DSK2()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            int dsk2onair;
            int dsk2tie;
            _BMDSwitcherDownstreamKey2.GetOnAir(out dsk2onair);
            _BMDSwitcherDownstreamKey2.GetTie(out dsk2tie);

            _state.DSK2OnAir = dsk2onair != 0;
            _state.DSK2Tie = dsk2tie != 0;
        }
        private void ForceStateUpdate_ProgramInput()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            // get current program source
            long programid;
            _BMDSwitcherMixEffectBlock1.GetProgramInput(out programid);
            _state.ProgramID = programid;
        }
        private void ForceStateUpdate_PreviewInput()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            // get current preset source
            long presetid;
            _BMDSwitcherMixEffectBlock1.GetPreviewInput(out presetid);
            _state.PresetID = presetid;
        }
        private void ForceStateUpdate_TransitionPosition()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            double trans_position;
            uint trans_frames_remaining;
            int intrans;

            _BMDSwitcherMixEffectBlock1.GetTransitionPosition(out trans_position);
            _BMDSwitcherMixEffectBlock1.GetTransitionFramesRemaining(out trans_frames_remaining);
            _BMDSwitcherMixEffectBlock1.GetInTransition(out intrans);

            _state.InTransition = intrans != 0;
            _state.TransitionFramesRemaining = (int)trans_frames_remaining;
            _state.TransitionPosition = trans_position;
        }
        private void ForceStateUpdate_Transition()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _BMDSwitcherTransitionSelection sel;
            _BMDSwitcherTransitionParameters.GetNextTransitionSelection(out sel);
            int selection = (int)sel;

            _state.TransNextBackground = (selection & (int)_BMDSwitcherTransitionSelection.bmdSwitcherTransitionSelectionBackground) == (int)_BMDSwitcherTransitionSelection.bmdSwitcherTransitionSelectionBackground;
            _state.TransNextKey1 = (selection & (int)_BMDSwitcherTransitionSelection.bmdSwitcherTransitionSelectionKey1) == (int)_BMDSwitcherTransitionSelection.bmdSwitcherTransitionSelectionKey1;

        }
        private void ForceStateUpdate_FTB()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            int ftb;
            _BMDSwitcherMixEffectBlock1.GetFadeToBlackFullyBlack(out ftb);
            _state.FTB = ftb != 0;
        }




        private BMDSwitcherState GetCurrentState()
        {
            return _state;
        }



        private void ConfigureSwitcher(BMDSwitcherConfigSettings config, bool hardUpdate = true)
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _config = config;
            ConfigureMixEffectBlock();

            if (hardUpdate)
            {
                ConfigureAux();
            }

            ConfigureCameraSources();
            ConfigureDownstreamKeys();
            ConfigureMultiviewer();

            if (hardUpdate)
            {
                ConfigureUpstreamKey();
            }
            // disable for now - doesn't work
            //ConfigureMediaPool();
            ConfigureAudioLevels();

            ForceStateUpdate();
        }

        private void ConfigureAux()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _BMDSwitcherAuxInput.SetInputSource(_config.DefaultAuxSource);
        }
        private void ConfigureMixEffectBlock()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _BMDSwitcherMixEffectBlock1.SetFadeToBlackRate((uint)_config.MixEffectSettings.FTBRate);

            IBMDSwitcherTransitionMixParameters mixParameters = (IBMDSwitcherTransitionMixParameters)_BMDSwitcherMixEffectBlock1;
            mixParameters.SetRate((uint)_config.MixEffectSettings.Rate);
        }
        private void ConfigureCameraSources()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");

            // set input source names
            foreach (var inputsource in _BMDSwitcherInputs)
            {
                long sourceid;
                inputsource.GetInputId(out sourceid);

                var map = _config.Routing.Where(r => r.PhysicalInputId == sourceid);
                if (map != null && map.Count() > 0)
                {
                    var source = map.First();
                    inputsource.SetLongName(source.LongName);
                    inputsource.SetShortName(source.ShortName);
                }

            }
        }
        private void ConfigureDownstreamKeys()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            ConfigureDownstreamKey1();
            ConfigureDownstreamKey2();
        }
        private void ConfigureDownstreamKey1()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _BMDSwitcherDownstreamKey1.SetInputFill(_config.DownstreamKey1Config.InputFill);
            _BMDSwitcherDownstreamKey1.SetInputCut(_config.DownstreamKey1Config.InputCut);
            _BMDSwitcherDownstreamKey1.SetRate((uint)_config.DownstreamKey1Config.Rate);

            _BMDSwitcherDownstreamKey1.SetPreMultiplied(_config.DownstreamKey1Config.IsPremultipled);
            _BMDSwitcherDownstreamKey1.SetClip(_config.DownstreamKey1Config.Clip);
            _BMDSwitcherDownstreamKey1.SetGain(_config.DownstreamKey1Config.Gain);
            _BMDSwitcherDownstreamKey1.SetInverse(_config.DownstreamKey1Config.Invert);
            _BMDSwitcherDownstreamKey1.SetMasked(_config.DownstreamKey1Config.IsMasked);
            _BMDSwitcherDownstreamKey1.SetMaskTop(_config.DownstreamKey1Config.MaskTop);
            _BMDSwitcherDownstreamKey1.SetMaskBottom(_config.DownstreamKey1Config.MaskBottom);
            _BMDSwitcherDownstreamKey1.SetMaskLeft(_config.DownstreamKey1Config.MaskLeft);
            _BMDSwitcherDownstreamKey1.SetMaskRight(_config.DownstreamKey1Config.MaskRight);
        }
        private void ConfigureDownstreamKey2()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _BMDSwitcherDownstreamKey2.SetInputFill(_config.DownstreamKey2Config.InputFill);
            _BMDSwitcherDownstreamKey2.SetInputCut(_config.DownstreamKey2Config.InputCut);
            _BMDSwitcherDownstreamKey2.SetRate((uint)_config.DownstreamKey2Config.Rate);

            _BMDSwitcherDownstreamKey2.SetPreMultiplied(_config.DownstreamKey2Config.IsPremultipled);
            _BMDSwitcherDownstreamKey2.SetClip(_config.DownstreamKey2Config.Clip);
            _BMDSwitcherDownstreamKey2.SetGain(_config.DownstreamKey2Config.Gain);
            _BMDSwitcherDownstreamKey2.SetInverse(_config.DownstreamKey2Config.Invert);
            _BMDSwitcherDownstreamKey2.SetMasked(_config.DownstreamKey2Config.IsMasked);
            _BMDSwitcherDownstreamKey2.SetMaskTop(_config.DownstreamKey2Config.MaskTop);
            _BMDSwitcherDownstreamKey2.SetMaskBottom(_config.DownstreamKey2Config.MaskBottom);
            _BMDSwitcherDownstreamKey2.SetMaskLeft(_config.DownstreamKey2Config.MaskLeft);
            _BMDSwitcherDownstreamKey2.SetMaskRight(_config.DownstreamKey2Config.MaskRight);

        }
        private void ConfigureMultiviewer()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _BMDSwitcherMultiView.SetLayout((_BMDSwitcherMultiViewLayout)_config.MultiviewerConfig.Layout);
            _BMDSwitcherMultiView.SetWindowInput(2, _config.MultiviewerConfig.Window2);
            _BMDSwitcherMultiView.SetWindowInput(3, _config.MultiviewerConfig.Window3);
            _BMDSwitcherMultiView.SetWindowInput(4, _config.MultiviewerConfig.Window4);
            _BMDSwitcherMultiView.SetWindowInput(5, _config.MultiviewerConfig.Window5);
            _BMDSwitcherMultiView.SetWindowInput(6, _config.MultiviewerConfig.Window6);
            _BMDSwitcherMultiView.SetWindowInput(7, _config.MultiviewerConfig.Window7);
            _BMDSwitcherMultiView.SetWindowInput(8, _config.MultiviewerConfig.Window8);
            _BMDSwitcherMultiView.SetWindowInput(9, _config.MultiviewerConfig.Window9);

            // disable all vu meters
            for (int i = 0; i < 10; i++)
            {
                _BMDSwitcherMultiView.SetVuMeterEnabled((uint)i, _config.MultiviewerConfig.ShowVUMetersOnWindows.Contains(i) ? 1 : 0);
            }
        }
        private void ConfigureUpstreamKey()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");

            ConfigureUSK1ChromaSettings();
            ConfigureDVEPIPSettings();
            ConfigureUSK1PATTERNSettings(_config.USKSettings.PATTERNSettings);

            if (_config.USKSettings.DefaultKeyType == 1)
            {
                ConfigureUSKforPictureInPicture();
            }
            else if (_config.USKSettings.DefaultKeyType == 2)
            {
                ConfigureUSKforChroma();
            }
            else if (_config.USKSettings.DefaultKeyType == 3)
            {
                ConfigureUSKForPATTERN(_config.USKSettings.PATTERNSettings);
            }
        }

        private void ConfigureUSKForPATTERN(BMDUSKPATTERNSettings settings)
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _BMDSwitcherUpstreamKey1.SetType(_BMDSwitcherKeyType.bmdSwitcherKeyTypePattern);
            _config.USKSettings.DefaultKeyType = 3;

            ConfigureUSK1PATTERNSettings(settings);

            ForceStateUpdate();
        }

        private void ConfigureUSKforChroma()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _BMDSwitcherUpstreamKey1.SetType(_BMDSwitcherKeyType.bmdSwitcherKeyTypeChroma);
            _config.USKSettings.DefaultKeyType = 2;

            ConfigureUSK1ChromaSettings();

            ForceStateUpdate();
        }
        private void ConfigureUSK1ChromaSettings()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            // set initial fill to slide
            _BMDSwitcherUpstreamKey1.SetInputFill(_config.USKSettings.ChromaSettings.FillSource);

            IBMDSwitcherKeyChromaParameters chromaParameters = (IBMDSwitcherKeyChromaParameters)_BMDSwitcherUpstreamKey1;

            chromaParameters.SetHue(_config.USKSettings.ChromaSettings.Hue);
            chromaParameters.SetGain(_config.USKSettings.ChromaSettings.Gain);
            chromaParameters.SetYSuppress(_config.USKSettings.ChromaSettings.YSuppress);
            chromaParameters.SetLift(_config.USKSettings.ChromaSettings.Lift);
            chromaParameters.SetNarrow(_config.USKSettings.ChromaSettings.Narrow);

        }
        private void ApplyUSK1ChromaSettings(BMDUSKChromaSettings settings)
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");

            IBMDSwitcherKeyChromaParameters chromaParameters = (IBMDSwitcherKeyChromaParameters)_BMDSwitcherUpstreamKey1;

            chromaParameters.SetHue(settings.Hue);
            chromaParameters.SetGain(settings.Gain);
            chromaParameters.SetYSuppress(settings.YSuppress);
            chromaParameters.SetLift(settings.Lift);
            chromaParameters.SetNarrow(settings.Narrow);
        }


        private void ApplyStateToSwitcher(BMDSwitcherState state)
        {
            PerformPresetSelect((int)state.PresetID);
            PerformProgramSelect((int)state.ProgramID);
            PerformAuxSelect((int)state.AuxID);
            PerformSetDSK1(state.DSK1OnAir);
            PerformSetDSK2(state.DSK1OnAir);
            PerformSetTieDSK1(state.DSK1Tie);
            PerformSetTieDSK2(state.DSK2Tie);
            if (!_state.FTB && state.FTB)
            {
                PerformToggleFTB();
            }
            PerformUSK1FillSourceSelect((int)state.USK1FillSource);
            PerformSetNextTransition(state.TransNextBackground, state.TransNextKey1);
            ApplyUSK1ChromaSettings(state.ChromaSettings);
            ApplyUSK1PATTERNSettings(state.PATTERNSettings);
            SetPIPPosition(state.DVESettings);
            SetUSK1State(state.USK1OnAir);
            switch ((_BMDSwitcherKeyType)state.USK1KeyType)
            {
                case _BMDSwitcherKeyType.bmdSwitcherKeyTypeLuma:
                    break;
                case _BMDSwitcherKeyType.bmdSwitcherKeyTypeChroma:
                    SetUSK1TypeChroma();
                    break;
                case _BMDSwitcherKeyType.bmdSwitcherKeyTypePattern:
                    SetUSK1TypePATTERN();
                    break;
                case _BMDSwitcherKeyType.bmdSwitcherKeyTypeDVE:
                    SetUSK1TypeDVE();
                    break;
                default:
                    break;
            }
        }

        private void ForceStateUpdate_ChromaSettings()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            IBMDSwitcherKeyChromaParameters chromaParameters = (IBMDSwitcherKeyChromaParameters)_BMDSwitcherUpstreamKey1;
            double hue, gain, lift, ysuppress;
            int narrow;
            chromaParameters.GetHue(out hue);
            chromaParameters.GetGain(out gain);
            chromaParameters.GetLift(out lift);
            chromaParameters.GetYSuppress(out ysuppress);
            chromaParameters.GetNarrow(out narrow);
            _state.ChromaSettings = new BMDUSKChromaSettings()
            {
                FillSource = (int)_state.USK1FillSource,
                Hue = hue,
                Gain = gain,
                Lift = lift,
                YSuppress = ysuppress,
                Narrow = narrow
            };
        }
        private void ConfigureUSKforPictureInPicture()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _BMDSwitcherUpstreamKey1.SetType(_BMDSwitcherKeyType.bmdSwitcherKeyTypeDVE);
            _config.USKSettings.DefaultKeyType = 1;

            ConfigureDVEPIPSettings();

            ForceStateUpdate();
        }
        private void ConfigureDVEPIPSettings()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            // set initial fill source to center
            _BMDSwitcherUpstreamKey1.SetInputFill(_config.USKSettings.PIPSettings.DefaultFillSource);

            IBMDSwitcherKeyDVEParameters dveparams = (IBMDSwitcherKeyDVEParameters)_BMDSwitcherUpstreamKey1;

            // set border with dveparams
            dveparams.SetMasked(_config.USKSettings.PIPSettings.IsMasked);
            dveparams.SetMaskTop(_config.USKSettings.PIPSettings.MaskTop);
            dveparams.SetMaskBottom(_config.USKSettings.PIPSettings.MaskBottom);
            dveparams.SetMaskLeft(_config.USKSettings.PIPSettings.MaskLeft);
            dveparams.SetMaskRight(_config.USKSettings.PIPSettings.MaskRight);


            dveparams.SetBorderEnabled(_config.USKSettings.PIPSettings.IsBordered);


            // config size & base position
            _BMDSwitcherFlyKeyParamters.SetPositionX(_config.USKSettings.PIPSettings.Current.PositionX);
            _BMDSwitcherFlyKeyParamters.SetPositionY(_config.USKSettings.PIPSettings.Current.PositionY);
            _BMDSwitcherFlyKeyParamters.SetSizeX(_config.USKSettings.PIPSettings.Current.SizeX);
            _BMDSwitcherFlyKeyParamters.SetSizeY(_config.USKSettings.PIPSettings.Current.SizeY);

            // setup keyframes
            IBMDSwitcherKeyFlyKeyFrameParameters keyframeparams;
            _BMDSwitcherFlyKeyParamters.GetKeyFrameParameters(_BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameA, out keyframeparams);

            keyframeparams.SetPositionX(_config.USKSettings.PIPSettings.KeyFrameA.PositionX);
            keyframeparams.SetPositionY(_config.USKSettings.PIPSettings.KeyFrameA.PositionY);
            keyframeparams.SetSizeX(_config.USKSettings.PIPSettings.KeyFrameA.SizeX);
            keyframeparams.SetSizeY(_config.USKSettings.PIPSettings.KeyFrameA.SizeY);

            _BMDSwitcherFlyKeyParamters.GetKeyFrameParameters(_BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameB, out keyframeparams);

            keyframeparams.SetPositionX(_config.USKSettings.PIPSettings.KeyFrameB.PositionX);
            keyframeparams.SetPositionY(_config.USKSettings.PIPSettings.KeyFrameB.PositionY);
            keyframeparams.SetSizeX(_config.USKSettings.PIPSettings.KeyFrameB.SizeX);
            keyframeparams.SetSizeY(_config.USKSettings.PIPSettings.KeyFrameB.SizeY);
        }
        private void ForceStateUpdate_PIPSettings()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            IBMDSwitcherKeyDVEParameters dveparams = (IBMDSwitcherKeyDVEParameters)_BMDSwitcherUpstreamKey1;

            int isbordered;
            int ismasked;
            double masktop, maskbot, maskleft, maskright;
            dveparams.GetMasked(out ismasked);
            dveparams.GetMaskBottom(out maskbot);
            dveparams.GetMaskTop(out masktop);
            dveparams.GetMaskLeft(out maskleft);
            dveparams.GetMaskRight(out maskright);
            dveparams.GetBorderEnabled(out isbordered);

            double cposx, cposy, csizex, csizey;
            _BMDSwitcherFlyKeyParamters.GetPositionX(out cposx);
            _BMDSwitcherFlyKeyParamters.GetPositionY(out cposy);
            _BMDSwitcherFlyKeyParamters.GetSizeX(out csizex);
            _BMDSwitcherFlyKeyParamters.GetSizeY(out csizey);

            double aposx, aposy, asizex, asizey;
            IBMDSwitcherKeyFlyKeyFrameParameters keyframeparamsa;
            _BMDSwitcherFlyKeyParamters.GetKeyFrameParameters(_BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameA, out keyframeparamsa);
            keyframeparamsa.GetPositionX(out aposx);
            keyframeparamsa.GetPositionY(out aposy);
            keyframeparamsa.GetSizeX(out asizex);
            keyframeparamsa.GetSizeY(out asizey);

            double bposx, bposy, bsizex, bsizey;
            IBMDSwitcherKeyFlyKeyFrameParameters keyframeparamsb;
            _BMDSwitcherFlyKeyParamters.GetKeyFrameParameters(_BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameB, out keyframeparamsb);
            keyframeparamsb.GetPositionX(out bposx);
            keyframeparamsb.GetPositionY(out bposy);
            keyframeparamsb.GetSizeX(out bsizex);
            keyframeparamsb.GetSizeY(out bsizey);


            _state.DVESettings = new BMDUSKDVESettings()
            {
                Current = new KeyFrameSettings()
                {
                    PositionX = cposx,
                    PositionY = cposy,
                    SizeX = csizex,
                    SizeY = csizey
                },
                DefaultFillSource = _config?.USKSettings.PIPSettings.DefaultFillSource ?? 0,
                IsBordered = isbordered,
                IsMasked = ismasked,
                MaskBottom = (float)maskbot,
                MaskTop = (float)masktop,
                MaskLeft = (float)maskleft,
                MaskRight = (float)maskright,
                KeyFrameA = new KeyFrameSettings()
                {
                    PositionX = aposx,
                    PositionY = aposy,
                    SizeX = asizex,
                    SizeY = asizey
                },
                KeyFrameB = new KeyFrameSettings()
                {
                    PositionX = bposx,
                    PositionY = bposy,
                    SizeX = bsizex,
                    SizeY = bsizey
                }
            };
        }

        private void ForceStateUpdate_PatternSettings()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            IBMDSwitcherKeyPatternParameters patternparams = (IBMDSwitcherKeyPatternParameters)_BMDSwitcherUpstreamKey1;

            patternparams.GetPattern(out var pattern);
            patternparams.GetInverse(out var inverse);
            patternparams.GetSize(out var size);
            patternparams.GetSoftness(out var softness);
            patternparams.GetSymmetry(out var symmetry);
            patternparams.GetHorizontalOffset(out var xoff);
            patternparams.GetVerticalOffset(out var yoff);

            double cposx, cposy, csizex, csizey;
            _BMDSwitcherFlyKeyParamters.GetPositionX(out cposx);
            _BMDSwitcherFlyKeyParamters.GetPositionY(out cposy);
            _BMDSwitcherFlyKeyParamters.GetSizeX(out csizex);
            _BMDSwitcherFlyKeyParamters.GetSizeY(out csizey);

            string patternName = BMDUSKPATTERNSettings.FindPattern(pattern);

            _state.PATTERNSettings = new BMDUSKPATTERNSettings()
            {
                DefaultFillSource = _config?.USKSettings.PATTERNSettings.DefaultFillSource ?? 0,
                PatternType = patternName,
                Inverted = inverse == 1,
                Size = size,
                Softness = softness,
                Symmetry = symmetry,
                XOffset = xoff,
                YOffset = yoff,
            };
        }


        private void ConfigureAudioLevels()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            IBMDSwitcherAudioMixer mixer = (IBMDSwitcherAudioMixer)_BMDSwitcher;

            mixer.SetProgramOutGain(_config.AudioSettings.ProgramOutGain);

            IBMDSwitcherAudioInputIterator audioIterator = null;
            IntPtr audioPtr;
            Guid audioInputIID = typeof(IBMDSwitcherAudioInputIterator).GUID;
            mixer.CreateIterator(ref audioInputIID, out audioPtr);
#pragma warning disable CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'
            if (audioPtr != null)
#pragma warning restore CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'
            {
                audioIterator = (IBMDSwitcherAudioInputIterator)Marshal.GetObjectForIUnknown(audioPtr);

                IBMDSwitcherAudioInput xlr;
                audioIterator.GetById((long)BMDSwitcherAudioSources.XLR, out xlr);
                xlr.SetGain(_config.AudioSettings.XLRInputGain);

                // for now disable all other inputs
                IBMDSwitcherAudioInput audioinput;
                audioIterator.GetById((long)BMDSwitcherAudioSources.Input_1, out audioinput);
                audioinput.SetMixOption(_BMDSwitcherAudioMixOption.bmdSwitcherAudioMixOptionOff);
                audioIterator.GetById((long)BMDSwitcherAudioSources.Input_2, out audioinput);
                audioinput.SetMixOption(_BMDSwitcherAudioMixOption.bmdSwitcherAudioMixOptionOff);
                audioIterator.GetById((long)BMDSwitcherAudioSources.Input_3, out audioinput);
                audioinput.SetMixOption(_BMDSwitcherAudioMixOption.bmdSwitcherAudioMixOptionOff);
                audioIterator.GetById((long)BMDSwitcherAudioSources.Input_4, out audioinput);
                audioinput.SetMixOption(_BMDSwitcherAudioMixOption.bmdSwitcherAudioMixOptionOff);
                audioIterator.GetById((long)BMDSwitcherAudioSources.Input_5, out audioinput);
                audioinput.SetMixOption(_BMDSwitcherAudioMixOption.bmdSwitcherAudioMixOptionOff);
                audioIterator.GetById((long)BMDSwitcherAudioSources.Input_6, out audioinput);
                audioinput.SetMixOption(_BMDSwitcherAudioMixOption.bmdSwitcherAudioMixOptionOff);
                audioIterator.GetById((long)BMDSwitcherAudioSources.Input_7, out audioinput);
                audioinput.SetMixOption(_BMDSwitcherAudioMixOption.bmdSwitcherAudioMixOptionOff);
                audioIterator.GetById((long)BMDSwitcherAudioSources.Input_8, out audioinput);
                audioinput.SetMixOption(_BMDSwitcherAudioMixOption.bmdSwitcherAudioMixOptionOff);

            }

        }

        private void PerformPresetSelect(int sourceID)
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()} {sourceID}");
            if (GoodConnection)
            {
                _BMDSwitcherMixEffectBlock1.SetPreviewInput(sourceID);
            }
        }

        private void PerformProgramSelect(int sourceID)
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()} {sourceID}");
            if (GoodConnection)
            {
                _BMDSwitcherMixEffectBlock1.SetProgramInput(sourceID);
            }
        }

        private void PerformCutTransition()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            if (GoodConnection)
            {
                _BMDSwitcherMixEffectBlock1.PerformCut();
            }
        }

        private void PerformAutoTransition()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            if (GoodConnection)
            {
                _BMDSwitcherMixEffectBlock1.PerformAutoTransition();
            }
        }

        private void PerformToggleUSK1()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()} {(_state.USK1OnAir ? 0 : 1)}");
            _BMDSwitcherUpstreamKey1.SetOnAir(_state.USK1OnAir ? 0 : 1);
        }

        private void PerformTieUSK1()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            //_BMDSwitcherUpstreamKey1.set
        }

        private void PerformSetDSK1(bool on)
        {
            var val = on ? 1 : 0;
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()} {on}");
            _BMDSwitcherDownstreamKey1.SetOnAir(val);
        }
        private void PerformToggleDSK1()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()} {(_state.DSK1OnAir ? 0 : 1)}");
            _BMDSwitcherDownstreamKey1.SetOnAir(_state.DSK1OnAir ? 0 : 1);
        }
        private void PerformTieDSK1()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()} {(_state.DSK1Tie ? 0 : 1)}");
            _BMDSwitcherDownstreamKey1.SetTie(_state.DSK1Tie ? 0 : 1);
        }
        private void PerformSetTieDSK1(bool set)
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()} {set}");
            _BMDSwitcherDownstreamKey1.SetTie(set ? 1 : 0);
        }
        private void PerformTakeAutoDSK1()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _BMDSwitcherDownstreamKey1.PerformAutoTransition();
        }
        private void PerformAutoOnAirDSK1()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()} 1");
            _BMDSwitcherDownstreamKey1.PerformAutoTransitionInDirection(1);
        }
        private void PerformAutoOffAirDSK1()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()} 0");
            _BMDSwitcherDownstreamKey1.PerformAutoTransitionInDirection(0);
        }
        private void PerformSetDSK2(bool on)
        {
            var val = on ? 1 : 0;
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()} {on}");
            _BMDSwitcherDownstreamKey1.SetOnAir(val);
        }
        private void PerformToggleDSK2()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()} {(_state.DSK2OnAir ? 0 : 1)}");
            _BMDSwitcherDownstreamKey2.SetOnAir(_state.DSK2OnAir ? 0 : 1);
        }
        private void PerformTieDSK2()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()} {(_state.DSK2Tie ? 0 : 1)}");
            _BMDSwitcherDownstreamKey2.SetTie(_state.DSK2Tie ? 0 : 1);
        }
        private void PerformSetTieDSK2(bool set)
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()} {set}");
            _BMDSwitcherDownstreamKey2.SetTie(set ? 1 : 0);
        }

        private void PerformTakeAutoDSK2()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _BMDSwitcherDownstreamKey2.PerformAutoTransition();
        }
        private void PerformAutoOnAirDSK2()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()} 1");
            _BMDSwitcherDownstreamKey2.PerformAutoTransitionInDirection(1);
        }
        private void PerformAutoOffAirDSK2()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()} 0");
            _BMDSwitcherDownstreamKey2.PerformAutoTransitionInDirection(0);
        }

        private void PerformToggleFTB()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _BMDSwitcherMixEffectBlock1.PerformFadeToBlack();
        }

        private void Close()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            SwitcherDisconnected();
        }

        private void PerformUSK1RunToKeyFrameA()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _BMDSwitcherFlyKeyParamters.RunToKeyFrame(_BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameA);
        }

        private void PerformUSK1RunToKeyFrameB()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _BMDSwitcherFlyKeyParamters.RunToKeyFrame(_BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameB);
        }

        private void PerformUSK1RunToKeyFrameFull()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _BMDSwitcherFlyKeyParamters.RunToKeyFrame(_BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameFull);
        }

        private void PerformUSK1FillSourceSelect(int sourceID)
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _BMDSwitcherUpstreamKey1.SetInputFill(sourceID);
        }

        private void PerformToggleBackgroundForNextTrans()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            // only allow deselection if at least one layer is selected
            if (_state.TransNextBackground)
            {
                // to disable background key1 needs to be selected
                if (!_state.TransNextKey1)
                {
                    // dont' do anything
                    return;
                }
            }


            int val = 0;
            if (!_state.TransNextBackground)
            {
                val |= (int)_BMDSwitcherTransitionSelection.bmdSwitcherTransitionSelectionBackground;
            }
            if (_state.TransNextKey1)
            {
                val |= (int)_BMDSwitcherTransitionSelection.bmdSwitcherTransitionSelectionKey1;
            }
            _BMDSwitcherTransitionParameters.SetNextTransitionSelection((_BMDSwitcherTransitionSelection)val);
        }
        private void PerformSetBKDGOnForNextTrans()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");

            // need at least 1 layer on
            // so this will always work
            var activelayers = _BMDSwitcherTransitionSelection.bmdSwitcherTransitionSelectionBackground;
            if (_state.TransNextKey1)
            {
                activelayers |= _BMDSwitcherTransitionSelection.bmdSwitcherTransitionSelectionKey1;
            }

            _BMDSwitcherTransitionParameters.SetNextTransitionSelection(activelayers);
        }

        private void PerformSetBKDGOffForNextTrans()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");

            // need at least 1 layer on
            // only works if we have key1 on
            if (_state.TransNextKey1)
            {
                _BMDSwitcherTransitionParameters.SetNextTransitionSelection(_BMDSwitcherTransitionSelection.bmdSwitcherTransitionSelectionKey1);
            }
        }


        private void PerformToggleKey1ForNextTrans()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");

            // only allow deselection if at least one layer is selected
            if (_state.TransNextKey1)
            {
                // to disable key1 background needs to be selected
                if (!_state.TransNextBackground)
                {
                    // don't do anything
                    return;
                }
            }


            int val = 0;
            if (_state.TransNextBackground)
            {
                val |= (int)_BMDSwitcherTransitionSelection.bmdSwitcherTransitionSelectionBackground;
            }
            if (!_state.TransNextKey1)
            {
                val |= (int)_BMDSwitcherTransitionSelection.bmdSwitcherTransitionSelectionKey1;
            }
            _BMDSwitcherTransitionParameters.SetNextTransitionSelection((_BMDSwitcherTransitionSelection)val);
        }

        private void PerformSetNextTransition(bool nextBKDG, bool nextUSK1)
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _BMDSwitcherTransitionSelection val = 0;
            if (nextBKDG)
            {
                val |= _BMDSwitcherTransitionSelection.bmdSwitcherTransitionSelectionBackground;
            }
            if (nextUSK1)
            {
                val |= _BMDSwitcherTransitionSelection.bmdSwitcherTransitionSelectionKey1;
            }

            _BMDSwitcherTransitionParameters.SetNextTransitionSelection(val);
        }

        private void ConfigureUSK1PATTERNSettings(BMDUSKPATTERNSettings pattern)
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            IBMDSwitcherKeyPatternParameters patternParameters = (IBMDSwitcherKeyPatternParameters)_BMDSwitcherUpstreamKey1;

            _BMDSwitcherUpstreamKey1.SetInputFill(pattern.DefaultFillSource);

            if (!BMDUSKPATTERNSettings.Patterns.TryGetValue(pattern.PatternType, out var ptype))
            {
                ptype = BMDUSKPATTERNSettings.DEFAULTPATTERNTYPE;
            }

            patternParameters.SetPattern(ptype);

            patternParameters.SetInverse(pattern.Inverted ? 1 : 0);
            patternParameters.SetVerticalOffset(pattern.YOffset);
            patternParameters.SetHorizontalOffset(pattern.XOffset);
            patternParameters.SetSoftness(pattern.Softness);
            patternParameters.SetSize(pattern.Size);
            patternParameters.SetSymmetry(pattern.Symmetry);

            _BMDSwitcherFlyKeyParamters.SetFly(1);
        }

        private void ApplyUSK1PATTERNSettings(BMDUSKPATTERNSettings pattern)
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            IBMDSwitcherKeyPatternParameters patternParameters = (IBMDSwitcherKeyPatternParameters)_BMDSwitcherUpstreamKey1;

            if (!BMDUSKPATTERNSettings.Patterns.TryGetValue(pattern.PatternType, out var ptype))
            {
                ptype = BMDUSKPATTERNSettings.DEFAULTPATTERNTYPE;
            }

            patternParameters.SetPattern(ptype);

            patternParameters.SetInverse(pattern.Inverted ? 1 : 0);
            patternParameters.SetVerticalOffset(pattern.YOffset);
            patternParameters.SetHorizontalOffset(pattern.XOffset);
            patternParameters.SetSoftness(pattern.Softness);
            patternParameters.SetSize(pattern.Size);
            patternParameters.SetSymmetry(pattern.Symmetry);

            _BMDSwitcherFlyKeyParamters.SetFly(1);
        }


        private void SetPIPPosition(BMDUSKDVESettings settings)
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            IBMDSwitcherKeyDVEParameters dveparams = (IBMDSwitcherKeyDVEParameters)_BMDSwitcherUpstreamKey1;

            // set border with dveparams
            dveparams.SetMasked(settings.IsMasked);
            dveparams.SetMaskTop(settings.MaskTop);
            dveparams.SetMaskBottom(settings.MaskBottom);
            dveparams.SetMaskLeft(settings.MaskLeft);
            dveparams.SetMaskRight(settings.MaskRight);


            dveparams.SetBorderEnabled(settings.IsBordered);


            // config size & base position
            _BMDSwitcherFlyKeyParamters.SetPositionX(settings.Current.PositionX);
            _BMDSwitcherFlyKeyParamters.SetPositionY(settings.Current.PositionY);
            _BMDSwitcherFlyKeyParamters.SetSizeX(settings.Current.SizeX);
            _BMDSwitcherFlyKeyParamters.SetSizeY(settings.Current.SizeY);

            ForceStateUpdate();
            SwitcherStateChanged?.Invoke(_state);
        }

        private void SetPIPKeyFrameA(BMDUSKDVESettings settings)
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            IBMDSwitcherKeyDVEParameters dveparams = (IBMDSwitcherKeyDVEParameters)_BMDSwitcherUpstreamKey1;

            // set border with dveparams
            dveparams.SetMasked(settings.IsMasked);
            dveparams.SetMaskTop(settings.MaskTop);
            dveparams.SetMaskBottom(settings.MaskBottom);
            dveparams.SetMaskLeft(settings.MaskLeft);
            dveparams.SetMaskRight(settings.MaskRight);


            dveparams.SetBorderEnabled(settings.IsBordered);

            // setup keyframes
            IBMDSwitcherKeyFlyKeyFrameParameters keyframeparams;
            _BMDSwitcherFlyKeyParamters.GetKeyFrameParameters(_BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameA, out keyframeparams);

            keyframeparams.SetPositionX(settings.KeyFrameA.PositionX);
            keyframeparams.SetPositionY(settings.KeyFrameA.PositionY);
            keyframeparams.SetSizeX(settings.KeyFrameA.SizeX);
            keyframeparams.SetSizeY(settings.KeyFrameA.SizeY);


            ForceStateUpdate();
            SwitcherStateChanged?.Invoke(_state);
        }

        private void SetPIPKeyFrameB(BMDUSKDVESettings settings)
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            IBMDSwitcherKeyDVEParameters dveparams = (IBMDSwitcherKeyDVEParameters)_BMDSwitcherUpstreamKey1;

            // set border with dveparams
            dveparams.SetMasked(settings.IsMasked);
            dveparams.SetMaskTop(settings.MaskTop);
            dveparams.SetMaskBottom(settings.MaskBottom);
            dveparams.SetMaskLeft(settings.MaskLeft);
            dveparams.SetMaskRight(settings.MaskRight);


            dveparams.SetBorderEnabled(settings.IsBordered);

            // setup keyframes
            IBMDSwitcherKeyFlyKeyFrameParameters keyframeparams;
            _BMDSwitcherFlyKeyParamters.GetKeyFrameParameters(_BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameB, out keyframeparams);

            keyframeparams.SetPositionX(settings.KeyFrameB.PositionX);
            keyframeparams.SetPositionY(settings.KeyFrameB.PositionY);
            keyframeparams.SetSizeX(settings.KeyFrameB.SizeX);
            keyframeparams.SetSizeY(settings.KeyFrameB.SizeY);


            ForceStateUpdate();
            SwitcherStateChanged?.Invoke(_state);
        }


        private void ConfigureUSK1PIP(BMDUSKDVESettings settings)
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            //_config.USKSettings.IsDVE = 1;
            //_config.USKSettings.IsChroma = 0;
            _config.USKSettings.DefaultKeyType = 1;
            _config.USKSettings.PIPSettings = settings;
            ConfigureUSKforPictureInPicture();

            ForceStateUpdate();
        }

        private void ConfigureUSK1PATTERN(BMDUSKPATTERNSettings settings)
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            //_config.USKSettings.IsDVE = 0;
            //_config.USKSettings.IsChroma = 1;
            _config.USKSettings.DefaultKeyType = 3;


            ConfigureUSKForPATTERN(settings);

            ForceStateUpdate();
        }


        private void ConfigureUSK1Chroma(BMDUSKChromaSettings settings)
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            //_config.USKSettings.IsDVE = 0;
            //_config.USKSettings.IsChroma = 1;
            _config.USKSettings.DefaultKeyType = 2;
            _config.USKSettings.ChromaSettings = settings;
            ConfigureUSKforChroma();

            ForceStateUpdate();
        }

        private void SetUSK1State(bool OnAir)
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _BMDSwitcherUpstreamKey1.SetOnAir(OnAir ? 1 : 0);
        }

        private void PerformOnAirUSK1()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _BMDSwitcherUpstreamKey1.SetOnAir(1);
        }

        private void PerformOffAirUSK1()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _BMDSwitcherUpstreamKey1.SetOnAir(0);
        }

        private void SetUSK1TypeDVE()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _BMDSwitcherUpstreamKey1.SetType(_BMDSwitcherKeyType.bmdSwitcherKeyTypeDVE);
            ForceStateUpdate();
        }
        private void SetUSK1TypePATTERN()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _BMDSwitcherUpstreamKey1.SetType(_BMDSwitcherKeyType.bmdSwitcherKeyTypePattern);
            ForceStateUpdate();
        }

        private void SetUSK1TypeChroma()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _BMDSwitcherUpstreamKey1.SetType(_BMDSwitcherKeyType.bmdSwitcherKeyTypeChroma);
            ForceStateUpdate();
        }

        /// <summary>
        /// Enables USK1 as part of next transition. Does not change the state of BKGD selection for the next transition.
        /// </summary>
        private void PerformSetKey1OnForNextTrans()
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");


            int val = 0;
            if (_state.TransNextBackground)
            {
                val |= (int)_BMDSwitcherTransitionSelection.bmdSwitcherTransitionSelectionBackground;
            }
            val |= (int)_BMDSwitcherTransitionSelection.bmdSwitcherTransitionSelectionKey1;
            _BMDSwitcherTransitionParameters.SetNextTransitionSelection((_BMDSwitcherTransitionSelection)val);
        }

        /// <summary>
        /// Assumes only 1 M/E and 1 USK
        /// Thus this will force the BKGD layer selected, since 1 must always be selected.
        /// </summary>
        private void PerformSetKey1OffForNextTrans()
        {

            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");

            _BMDSwitcherTransitionParameters.SetNextTransitionSelection(_BMDSwitcherTransitionSelection.bmdSwitcherTransitionSelectionBackground);

        }

        private void PerformAuxSelect(int sourceID)
        {
            _logger?.Debug($"[BMD HW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _BMDSwitcherAuxInput?.SetInputSource(sourceID);
            ForceStateUpdate();
        }


        #endregion

        #region External API Implementation

        void IBMDSwitcherManager.TryConnect(string address)
        {
            _executor.EnqueueWork(() => TryConnect(address));
        }

        void IBMDSwitcherManager.Disconnect()
        {
            _executor.EnqueueWork(Disconnect);
        }

        void IBMDSwitcherManager.ConfigureSwitcher(IntegratedPresenter.BMDSwitcher.Config.BMDSwitcherConfigSettings config, bool hardUpdate)
        {
            _executor.EnqueueWork(() => ConfigureSwitcher(config, hardUpdate));
        }

        BMDSwitcherState IBMDSwitcherManager.ForceStateUpdate()
        {
            BMDSwitcherState result = _state; // not sure it this is correct...
            var handle = _executor.EnqueueWorkWithCallback(() => result = ForceStateUpdate(), null);
            // block caller until complete
            // this isn't performante rally, but the point here is to basically switch threads
            // to make sure that any downstream COM calls onto the switcher API occur on the correct thread
            handle.wait.WaitOne();
            //handle.wait.WaitOne();
            // at this point we call has been completed on the correct thread
            // clean up expensive resources
            _executor.RemoveTrackedWork(handle.id);
            return result;
        }

        BMDSwitcherState IBMDSwitcherManager.GetCurrentState()
        {
            return GetCurrentState();
        }

        void IBMDSwitcherManager.PerformAutoOffAirDSK2()
        {
            _executor.EnqueueWork(PerformAutoOffAirDSK2);
        }

        void IBMDSwitcherManager.PerformAutoOnAirDSK2()
        {
            _executor.EnqueueWork(PerformAutoOnAirDSK2);
        }

        void IBMDSwitcherManager.PerformAutoOffAirDSK1()
        {
            _executor.EnqueueWork(PerformAutoOffAirDSK1);
        }

        void IBMDSwitcherManager.PerformAutoOnAirDSK1()
        {
            _executor.EnqueueWork(PerformAutoOnAirDSK1);
        }

        void IBMDSwitcherManager.PerformAutoTransition()
        {
            _executor.EnqueueWork(PerformAutoTransition);
        }

        void IBMDSwitcherManager.PerformCutTransition()
        {
            _executor.EnqueueWork(PerformCutTransition);
        }

        void IBMDSwitcherManager.PerformPresetSelect(int sourceID)
        {
            _executor.EnqueueWork(() => PerformPresetSelect(sourceID));
        }

        void IBMDSwitcherManager.PerformProgramSelect(int sourceID)
        {
            _executor.EnqueueWork(() => PerformProgramSelect(sourceID));
        }

        void IBMDSwitcherManager.PerformAuxSelect(int sourceID)
        {
            _executor.EnqueueWork(() => PerformAuxSelect(sourceID));
        }

        void IBMDSwitcherManager.PerformTakeAutoDSK1()
        {
            _executor.EnqueueWork(PerformTakeAutoDSK1);
        }

        void IBMDSwitcherManager.PerformTakeAutoDSK2()
        {
            _executor.EnqueueWork(PerformTakeAutoDSK2);
        }

        void IBMDSwitcherManager.PerformTieDSK1()
        {
            _executor.EnqueueWork(PerformTieDSK1);
        }

        void IBMDSwitcherManager.PerformTieDSK2()
        {
            _executor.EnqueueWork(PerformTieDSK2);
        }

        void IBMDSwitcherManager.PerformSetTieDSK1(bool set)
        {
            _executor.EnqueueWork(() => PerformSetTieDSK1(set));
        }

        void IBMDSwitcherManager.PerformSetTieDSK2(bool set)
        {
            _executor.EnqueueWork(() => PerformSetTieDSK2(set));
        }

        void IBMDSwitcherManager.PerformToggleDSK1()
        {
            _executor.EnqueueWork(PerformToggleDSK1);
        }

        void IBMDSwitcherManager.PerformToggleDSK2()
        {
            _executor.EnqueueWork(PerformToggleDSK2);
        }

        void IBMDSwitcherManager.PerformToggleFTB()
        {
            _executor.EnqueueWork(PerformToggleFTB);
        }

        void IBMDSwitcherManager.PerformToggleUSK1()
        {
            _executor.EnqueueWork(PerformToggleUSK1);
        }

        void IBMDSwitcherManager.PerformOnAirUSK1()
        {
            _executor.EnqueueWork(PerformOnAirUSK1);
        }

        void IBMDSwitcherManager.PerformOffAirUSK1()
        {
            _executor.EnqueueWork(PerformOffAirUSK1);
        }

        void IBMDSwitcherManager.PerformUSK1RunToKeyFrameA()
        {
            _executor.EnqueueWork(PerformUSK1RunToKeyFrameA);
        }

        void IBMDSwitcherManager.PerformUSK1RunToKeyFrameB()
        {
            _executor.EnqueueWork(PerformUSK1RunToKeyFrameB);
        }

        void IBMDSwitcherManager.PerformUSK1RunToKeyFrameFull()
        {
            _executor.EnqueueWork(PerformUSK1RunToKeyFrameFull);
        }

        void IBMDSwitcherManager.PerformUSK1FillSourceSelect(int sourceID)
        {
            _executor.EnqueueWork(() => PerformUSK1FillSourceSelect(sourceID));
        }

        void IBMDSwitcherManager.PerformToggleBackgroundForNextTrans()
        {
            _executor.EnqueueWork(PerformToggleBackgroundForNextTrans);
        }

        void IBMDSwitcherManager.PerformSetBKDGOnForNextTrans()
        {
            _executor.EnqueueWork(PerformSetBKDGOnForNextTrans);
        }

        void IBMDSwitcherManager.PerformSetBKDGOffForNextTrans()
        {
            _executor.EnqueueWork(PerformSetBKDGOffForNextTrans);
        }


        void IBMDSwitcherManager.PerformToggleKey1ForNextTrans()
        {
            _executor.EnqueueWork(PerformToggleKey1ForNextTrans);
        }

        void IBMDSwitcherManager.PerformSetKey1OnForNextTrans()
        {
            _executor.EnqueueWork(PerformSetKey1OnForNextTrans);
        }

        void IBMDSwitcherManager.PerformSetKey1OffForNextTrans()
        {
            _executor.EnqueueWork(PerformSetKey1OffForNextTrans);
        }

        void IBMDSwitcherManager.SetUSK1TypeDVE()
        {
            _executor.EnqueueWork(SetUSK1TypeDVE);
        }
        void IBMDSwitcherManager.SetUSK1TypePATTERN()
        {
            _executor.EnqueueWork(SetUSK1TypePATTERN);
        }

        void IBMDSwitcherManager.SetUSK1TypeChroma()
        {
            _executor.EnqueueWork(SetUSK1TypeChroma);
        }

        void IBMDSwitcherManager.SetPIPPosition(BMDUSKDVESettings settings)
        {
            _executor.EnqueueWork(() => SetPIPPosition(settings));
        }

        void IBMDSwitcherManager.SetPIPKeyFrameA(BMDUSKDVESettings settings)
        {
            _executor.EnqueueWork(() => SetPIPKeyFrameA(settings));
        }

        void IBMDSwitcherManager.SetPIPKeyFrameB(BMDUSKDVESettings settings)
        {
            _executor.EnqueueWork(() => SetPIPKeyFrameB(settings));
        }

        void IBMDSwitcherManager.ConfigureUSK1PIP(BMDUSKDVESettings settings)
        {
            _executor.EnqueueWork(() => ConfigureUSK1PIP(settings));
        }

        void IBMDSwitcherManager.ConfigureUSK1Chroma(BMDUSKChromaSettings settings)
        {
            _executor.EnqueueWork(() => ConfigureUSK1Chroma(settings));
        }
        void IBMDSwitcherManager.ConfigureUSK1PATTERN(BMDUSKPATTERNSettings settings)
        {
            _executor.EnqueueWork(() => ConfigureUSK1PATTERN(settings));
        }

        void IBMDSwitcherManager.Close()
        {
            _executor.EnqueueWork(Close);

            // TODO: is this right??
            _executor.Stop();
        }

        void IBMDSwitcherManager.ApplyState(BMDSwitcherState state)
        {
            _executor.EnqueueWork(() => ApplyStateToSwitcher(state));
        }





        #endregion

    }
}
