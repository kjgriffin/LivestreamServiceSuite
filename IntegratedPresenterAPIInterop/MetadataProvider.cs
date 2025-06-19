using VariableMarkupAttributes;

namespace IntegratedPresenterAPIInterop
{
    public static class MetadataProvider
    {
        public static bool TryGetScriptActionMetadataByCommandName(string commandName, out AutomationActionMetadata metadata)
        {
            if (ScriptActionsMetadata.Values.Any(x => x.ActionName == commandName))
            {
                metadata = ScriptActionsMetadata.Values.FirstOrDefault(x => x.ActionName == commandName);
                return true;
            }
            metadata = default(AutomationActionMetadata);
            return false;
        }

        public static Dictionary<AutomationActions, AutomationActionMetadata> ScriptActionsMetadata = new Dictionary<AutomationActions, AutomationActionMetadata>()
        {
            [AutomationActions.OpsNote] = (AutomationActions.OpsNote, "note"),

            [AutomationActions.AutoTrans] = (AutomationActions.AutoTrans, "AutoTrans"),
            [AutomationActions.CutTrans] = (AutomationActions.CutTrans, "CutTrans"),
            [AutomationActions.AutoTakePresetIfOnSlide] = (AutomationActions.AutoTakePresetIfOnSlide, "AutoTakePresetIfOnSlide"),

            [AutomationActions.DSK1On] = (AutomationActions.DSK1On, "DSK1On"),
            [AutomationActions.DSK1Off] = (AutomationActions.DSK1Off, "DSK1Off"),
            [AutomationActions.DSK1FadeOn] = (AutomationActions.DSK1FadeOn, "DSK1FadeOn"),
            [AutomationActions.DSK1FadeOff] = (AutomationActions.DSK1FadeOff, "DSK1FadeOff"),
            [AutomationActions.DSK1TieOn] = (AutomationActions.DSK1TieOn, "DSK1TieOn"),
            [AutomationActions.DSK1TieOff] = (AutomationActions.DSK1TieOff, "DSK1TieOff"),

            [AutomationActions.DSK2On] = (AutomationActions.DSK2On, "DSK2On"),
            [AutomationActions.DSK2Off] = (AutomationActions.DSK2Off, "DSK2Off"),
            [AutomationActions.DSK2FadeOn] = (AutomationActions.DSK2FadeOn, "DSK2FadeOn"),
            [AutomationActions.DSK2FadeOff] = (AutomationActions.DSK2FadeOff, "DSK2FadeOff"),
            [AutomationActions.DSK2TieOn] = (AutomationActions.DSK2TieOn, "DSK2TieOn"),
            [AutomationActions.DSK2TieOff] = (AutomationActions.DSK2TieOff, "DSK2TieOff"),


            [AutomationActions.BKGDTieOn] = (AutomationActions.BKGDTieOn, "BKGDTieOn"),
            [AutomationActions.BKGDTieOff] = (AutomationActions.BKGDTieOff, "BKGDTieOff"),

            [AutomationActions.USK1On] = (AutomationActions.USK1On, "USK1On"),
            [AutomationActions.USK1Off] = (AutomationActions.USK1Off, "USK1Off"),
            [AutomationActions.USK1TieOn] = (AutomationActions.USK1TieOn, "USK1TieOn"),
            [AutomationActions.USK1TieOff] = (AutomationActions.USK1TieOff, "USK1TieOff"),
            [AutomationActions.USK1SetTypeChroma] = (AutomationActions.USK1SetTypeChroma, "USK1SetTypeChroma"),
            [AutomationActions.USK1SetTypeDVE] = (AutomationActions.USK1SetTypeDVE, "USK1SetTypeDVE"),
            [AutomationActions.USK1SetTypePATTERN] = (AutomationActions.USK1SetTypePATTERN, "USK1SetTypePATTERN"),

            [AutomationActions.RecordStart] = (AutomationActions.RecordStart, "RecordStart"),
            [AutomationActions.RecordStop] = (AutomationActions.RecordStop, "RecordStop"),

            [AutomationActions.OpenAudioPlayer] = (AutomationActions.OpenAudioPlayer, "OpenAudioPlayer"),
            [AutomationActions.PlayAuxAudio] = (AutomationActions.PlayAuxAudio, "PlayAuxAudio"),
            [AutomationActions.StopAuxAudio] = (AutomationActions.StopAuxAudio, "StopAuxAudio"),
            [AutomationActions.PauseAuxAudio] = (AutomationActions.PauseAuxAudio, "PauseAuxAudio"),
            [AutomationActions.ReplayAuxAudio] = (AutomationActions.ReplayAuxAudio, "ReplayAuxAudio"),

            [AutomationActions.PlayMedia] = (AutomationActions.PlayMedia, "PlayMedia"),
            [AutomationActions.PauseMedia] = (AutomationActions.PauseMedia, "PauseMedia"),
            [AutomationActions.StopMedia] = (AutomationActions.StopMedia, "StopMedia"),
            [AutomationActions.RestartMedia] = (AutomationActions.RestartMedia, "RestartMedia"),
            [AutomationActions.MuteMedia] = (AutomationActions.MuteMedia, "MuteMedia"),
            [AutomationActions.UnMuteMedia] = (AutomationActions.UnMuteMedia, "UnMuteMedia"),

            [AutomationActions.DriveNextSlide] = (AutomationActions.DriveNextSlide, "DriveNextSlide"),
            [AutomationActions.Timer1Restart] = (AutomationActions.Timer1Restart, "Timer1Restart"),


            [AutomationActions.Timer1Restart] = (AutomationActions.Timer1Restart, "Timer1Restart"),

            [AutomationActions.PresetSelect] = new AutomationActionMetadata(AutomationActions.PresetSelect, "PresetSelect", new List<AutomationActionParameterMetadata>
            {
                new AutomationActionParameterMetadata("SourceID", AutomationActionArgType.Integer, ExpectedVariableContents.VIDEOSOURCE)
            }),
            [AutomationActions.ProgramSelect] = new AutomationActionMetadata(AutomationActions.ProgramSelect, "ProgramSelect", new List<AutomationActionParameterMetadata>
            {
                new AutomationActionParameterMetadata("SourceID", AutomationActionArgType.Integer, ExpectedVariableContents.VIDEOSOURCE)
            }),
            [AutomationActions.AuxSelect] = new AutomationActionMetadata(AutomationActions.AuxSelect, "AuxSelect", new List<AutomationActionParameterMetadata>
            {
                new AutomationActionParameterMetadata("SourceID", AutomationActionArgType.Integer, ExpectedVariableContents.VIDEOSOURCE)
            }),
            [AutomationActions.USK1Fill] = new AutomationActionMetadata(AutomationActions.USK1Fill, "USK1Fill", new List<AutomationActionParameterMetadata>
            {
                new AutomationActionParameterMetadata("SourceID", AutomationActionArgType.Integer, ExpectedVariableContents.VIDEOSOURCE)
            }),


            [AutomationActions.LoadAudio] = new AutomationActionMetadata(AutomationActions.LoadAudio, "LoadAudioFile", new List<AutomationActionParameterMetadata>
            {
                new AutomationActionParameterMetadata("AudioFile", AutomationActionArgType.String, ExpectedVariableContents.PROJECTASSET),
            }),


            [AutomationActions.DelayMs] =
            new AutomationActionMetadata(AutomationActions.DelayMs, "DelayMs", new List<AutomationActionParameterMetadata>
            {
                new AutomationActionParameterMetadata("Delay", AutomationActionArgType.Integer)
            }),
            [AutomationActions.DelayUntil] = new AutomationActionMetadata(AutomationActions.DelayUntil, "DelayUntil", new List<AutomationActionParameterMetadata>
            {
                new AutomationActionParameterMetadata("DateTime", AutomationActionArgType.String)
            }),

            [AutomationActions.JumpToSlide] = new AutomationActionMetadata(AutomationActions.JumpToSlide, "JumpToSlide", new List<AutomationActionParameterMetadata>
            {
                new AutomationActionParameterMetadata("SlideNum", AutomationActionArgType.Integer, staticHints: new List<(string, string)>{("%slide.num.<LABEL>.0%", "[Use Label]")})
            }),

            [AutomationActions.SetTargetSlide] = new AutomationActionMetadata(AutomationActions.SetTargetSlide, "SetNextSlideTarget", new List<AutomationActionParameterMetadata>
            {
                new AutomationActionParameterMetadata("STargetNum", AutomationActionArgType.Integer, staticHints: new List<(string, string)>{("%slide.num.<LABEL>.0%", "[Use Label]")})
            }),

            // duplicate command for backwards compatibility
            [AutomationActions.WatchSwitcherStateBoolVal] = new AutomationActionMetadata(AutomationActions.WatchSwitcherStateBoolVal, "WatchSwitcherStateBoolVal", new List<AutomationActionParameterMetadata>
            {
                new AutomationActionParameterMetadata("WatchVar", AutomationActionArgType.String, ExpectedVariableContents.EXPOSEDSTATE),
                new AutomationActionParameterMetadata("ExpectedVal", AutomationActionArgType.Boolean, staticHints: new List<(string item, string description)>{("true", ""), ("false", "")}),
                new AutomationActionParameterMetadata("CondName", AutomationActionArgType.String),
            }),
            [AutomationActions.WatchSwitcherStateIntVal] = new AutomationActionMetadata(AutomationActions.WatchSwitcherStateIntVal, "WatchSwitcherStateIntVal", new List<AutomationActionParameterMetadata>
            {
                new AutomationActionParameterMetadata("WatchVar", AutomationActionArgType.String, ExpectedVariableContents.EXPOSEDSTATE),
                new AutomationActionParameterMetadata("ExpectedVal", AutomationActionArgType.Integer),
                new AutomationActionParameterMetadata("CondName", AutomationActionArgType.String),
            }),

            [AutomationActions.CaptureSwitcherState] = new AutomationActionMetadata(AutomationActions.CaptureSwitcherState, "CaptureSwitcherState", new List<AutomationActionParameterMetadata>
            {
                new AutomationActionParameterMetadata("VarName", AutomationActionArgType.String, ExpectedVariableContents.NONE),
            }),
            [AutomationActions.ApplySwitcherState] = new AutomationActionMetadata(AutomationActions.ApplySwitcherState, "ApplySwitcherState", new List<AutomationActionParameterMetadata>
            {
                new AutomationActionParameterMetadata("VarName", AutomationActionArgType.String, ExpectedVariableContents.NONE),
            }),


            [AutomationActions.WatchStateBoolVal] = new AutomationActionMetadata(AutomationActions.WatchSwitcherStateBoolVal, "WatchStateBoolVal", new List<AutomationActionParameterMetadata>
            {
                new AutomationActionParameterMetadata("WatchVar", AutomationActionArgType.String, ExpectedVariableContents.EXPOSEDSTATE),
                new AutomationActionParameterMetadata("ExpectedVal", AutomationActionArgType.Boolean, staticHints: new List<(string item, string description)>{("true", ""), ("false", "")}),
                new AutomationActionParameterMetadata("CondName", AutomationActionArgType.String),
            }),
            [AutomationActions.WatchStateIntVal] = new AutomationActionMetadata(AutomationActions.WatchSwitcherStateIntVal, "WatchStateIntVal", new List<AutomationActionParameterMetadata>
            {
                new AutomationActionParameterMetadata("WatchVar", AutomationActionArgType.String, ExpectedVariableContents.EXPOSEDSTATE),
                new AutomationActionParameterMetadata("ExpectedVal", AutomationActionArgType.Integer),
                new AutomationActionParameterMetadata("CondName", AutomationActionArgType.String),
            }),


            [AutomationActions.InitComputedVal] = new AutomationActionMetadata(AutomationActions.InitComputedVal, "InitComputedVal", new List<AutomationActionParameterMetadata>
            {
                new AutomationActionParameterMetadata("VarName", AutomationActionArgType.String),
                new AutomationActionParameterMetadata("TypeStr", AutomationActionArgType.String),
                new AutomationActionParameterMetadata("VarDefaultVal", AutomationActionArgType.String),
            }),
            [AutomationActions.WriteComputedVal] = new AutomationActionMetadata(AutomationActions.WriteComputedVal, "WriteComputedVal", new List<AutomationActionParameterMetadata>
            {
                new AutomationActionParameterMetadata("VarName", AutomationActionArgType.String),
                new AutomationActionParameterMetadata("VarVal", AutomationActionArgType.String),
            }),
            [AutomationActions.PurgeComputedVal] = new AutomationActionMetadata(AutomationActions.PurgeComputedVal, "PurgeComputedVal", new List<AutomationActionParameterMetadata>
            {
                new AutomationActionParameterMetadata("VarName", AutomationActionArgType.String),
            }),
            [AutomationActions.SetupComputedTrack] = new AutomationActionMetadata(AutomationActions.SetupComputedTrack, "SetupTrackVal", new List<AutomationActionParameterMetadata>
            {
                new AutomationActionParameterMetadata("VarName", AutomationActionArgType.String),
                new AutomationActionParameterMetadata("TrackTarget", AutomationActionArgType.String, ExpectedVariableContents.EXPOSEDSTATE),
            }),
            [AutomationActions.ReleaseComputedTrack] = new AutomationActionMetadata(AutomationActions.ReleaseComputedTrack, "ReleaseTackVal", new List<AutomationActionParameterMetadata>
            {
                new AutomationActionParameterMetadata("VarName", AutomationActionArgType.String),
            }),

            [AutomationActions.RedrawDynamicControls] = (AutomationActions.RedrawDynamicControls, "PaintCtrls"),




            [AutomationActions.PlacePIP] = new AutomationActionMetadata(AutomationActions.PlacePIP, "PlacePIP", new List<AutomationActionParameterMetadata>
            {
                new AutomationActionParameterMetadata("PosX", AutomationActionArgType.Double),
                new AutomationActionParameterMetadata("PosY", AutomationActionArgType.Double),
                new AutomationActionParameterMetadata("ScaleX", AutomationActionArgType.Double),
                new AutomationActionParameterMetadata("ScaleY", AutomationActionArgType.Double),
                new AutomationActionParameterMetadata("MaskLeft", AutomationActionArgType.Double),
                new AutomationActionParameterMetadata("MaskRight", AutomationActionArgType.Double),
                new AutomationActionParameterMetadata("MaskTop", AutomationActionArgType.Double),
                new AutomationActionParameterMetadata("MaskBottom", AutomationActionArgType.Double),
            }),

            [AutomationActions.ConfigurePATTERN] = new AutomationActionMetadata(AutomationActions.ConfigurePATTERN, "ApplyPATTERN", new List<AutomationActionParameterMetadata>
            {
                new AutomationActionParameterMetadata("Type", AutomationActionArgType.String),
                new AutomationActionParameterMetadata("Inverted", AutomationActionArgType.Boolean),
                new AutomationActionParameterMetadata("Size", AutomationActionArgType.Double),
                new AutomationActionParameterMetadata("Symmetry", AutomationActionArgType.Double),
                new AutomationActionParameterMetadata("Softness", AutomationActionArgType.Double),
                new AutomationActionParameterMetadata("XOffset", AutomationActionArgType.Double),
                new AutomationActionParameterMetadata("YOffset", AutomationActionArgType.Double),
            }),

            [AutomationActions.SetupButtons] = new AutomationActionMetadata(AutomationActions.SetupButtons, "SetupButtons", new List<AutomationActionParameterMetadata>
            {
                new AutomationActionParameterMetadata("File", AutomationActionArgType.String),
                new AutomationActionParameterMetadata("ResourcePath", AutomationActionArgType.String, staticHints: new List<(string item, string description)>{("%pres%", "Use the relative folder of the loaded presentation")}),
                new AutomationActionParameterMetadata("Overwrite", AutomationActionArgType.Boolean, staticHints : new List <(string item, string description) > {("true", ""),("false", "") }),
            }),

            [AutomationActions.SetupExtras] = new AutomationActionMetadata(AutomationActions.SetupExtras, "SetupExtras", new List<AutomationActionParameterMetadata>()
            {
                new AutomationActionParameterMetadata("ExtraID", AutomationActionArgType.String, staticHints: new List<(string item, string description)>{("spare1", "[API NOTE] Param does nothing curently")}),
                new AutomationActionParameterMetadata("File", AutomationActionArgType.String),
                new AutomationActionParameterMetadata("ResourcePath", AutomationActionArgType.String, staticHints: new List<(string item, string description)>{("%pres%", "Use the relative folder of the loaded presentation")}),
                new AutomationActionParameterMetadata("Overwrite", AutomationActionArgType.Boolean, staticHints : new List <(string item, string description) > {("true", ""),("false", "") }),
            }),


            [AutomationActions.ForceRunPostSet] = new AutomationActionMetadata(AutomationActions.ForceRunPostSet, "ForceRunPostset", new List<AutomationActionParameterMetadata>()
            {
                new AutomationActionParameterMetadata("Force", AutomationActionArgType.Boolean, staticHints : new List <(string item, string description) > {("true", ""),("false", "") }),
                new AutomationActionParameterMetadata("SourceID", AutomationActionArgType.Integer, ExpectedVariableContents.VIDEOSOURCE),
            }),

            [AutomationActions.FireActivePreset] = new AutomationActionMetadata(AutomationActions.FireActivePreset, "FireActivePreset", new List<AutomationActionParameterMetadata>()
            {
                new AutomationActionParameterMetadata("CamName", AutomationActionArgType.String),
            }),
            [AutomationActions.FireCamPreset] = new AutomationActionMetadata(AutomationActions.FireCamPreset, "FireCamPreset", new List<AutomationActionParameterMetadata>()
            {
                new AutomationActionParameterMetadata("CamName", AutomationActionArgType.String),
                new AutomationActionParameterMetadata("PresetName", AutomationActionArgType.String),
                new AutomationActionParameterMetadata("Speed", AutomationActionArgType.Integer),
                new AutomationActionParameterMetadata("ZoomPST", AutomationActionArgType.String),
            }),
            [AutomationActions.FireCamDrive] = new AutomationActionMetadata(AutomationActions.FireCamDrive, "FireCamDrive", new List<AutomationActionParameterMetadata>()
            {
                new AutomationActionParameterMetadata("CamName", AutomationActionArgType.String),
                new AutomationActionParameterMetadata("DirX", AutomationActionArgType.Integer),
                new AutomationActionParameterMetadata("DirY", AutomationActionArgType.Integer),
                new AutomationActionParameterMetadata("SpeedX", AutomationActionArgType.Integer),
                new AutomationActionParameterMetadata("SpeedY", AutomationActionArgType.Integer),
            }),

        };

    }
}
