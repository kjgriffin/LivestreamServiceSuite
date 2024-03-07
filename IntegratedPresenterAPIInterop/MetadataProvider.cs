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
            [AutomationActions.OpsNote] = (0, AutomationActions.OpsNote, "note", null, ExpectedVariableContents.NONE),

            [AutomationActions.AutoTrans] = (0, AutomationActions.AutoTrans, "AutoTrans", null, ExpectedVariableContents.NONE),
            [AutomationActions.CutTrans] = (0, AutomationActions.CutTrans, "CutTrans", null, ExpectedVariableContents.NONE),
            [AutomationActions.AutoTakePresetIfOnSlide] = (0, AutomationActions.AutoTakePresetIfOnSlide, "AutoTakePresetIfOnSlide", null, ExpectedVariableContents.NONE),

            [AutomationActions.DSK1On] = (0, AutomationActions.DSK1On, "DSK1On", null, ExpectedVariableContents.NONE),
            [AutomationActions.DSK1Off] = (0, AutomationActions.DSK1Off, "DSK1Off", null, ExpectedVariableContents.NONE),
            [AutomationActions.DSK1FadeOn] = (0, AutomationActions.DSK1FadeOn, "DSK1FadeOn", null, ExpectedVariableContents.NONE),
            [AutomationActions.DSK1FadeOff] = (0, AutomationActions.DSK1FadeOff, "DSK1FadeOff", null, ExpectedVariableContents.NONE),
            [AutomationActions.DSK1TieOn] = (0, AutomationActions.DSK1TieOn, "DSK1TieOn", null, ExpectedVariableContents.NONE),
            [AutomationActions.DSK1TieOff] = (0, AutomationActions.DSK1TieOff, "DSK1TieOff", null, ExpectedVariableContents.NONE),

            [AutomationActions.DSK2On] = (0, AutomationActions.DSK2On, "DSK2On", null, ExpectedVariableContents.NONE),
            [AutomationActions.DSK2Off] = (0, AutomationActions.DSK2Off, "DSK2Off", null, ExpectedVariableContents.NONE),
            [AutomationActions.DSK2FadeOn] = (0, AutomationActions.DSK2FadeOn, "DSK2FadeOn", null, ExpectedVariableContents.NONE),
            [AutomationActions.DSK2FadeOff] = (0, AutomationActions.DSK2FadeOff, "DSK2FadeOff", null, ExpectedVariableContents.NONE),
            [AutomationActions.DSK2TieOn] = (0, AutomationActions.DSK2TieOn, "DSK2TieOn", null, ExpectedVariableContents.NONE),
            [AutomationActions.DSK2TieOff] = (0, AutomationActions.DSK2TieOff, "DSK2TieOff", null, ExpectedVariableContents.NONE),


            [AutomationActions.BKGDTieOn] = (0, AutomationActions.BKGDTieOn, "BKGDTieOn", null, ExpectedVariableContents.NONE),
            [AutomationActions.BKGDTieOff] = (0, AutomationActions.BKGDTieOff, "BKGDTieOff", null, ExpectedVariableContents.NONE),

            [AutomationActions.USK1On] = (0, AutomationActions.USK1On, "USK1On", null, ExpectedVariableContents.NONE),
            [AutomationActions.USK1Off] = (0, AutomationActions.USK1Off, "USK1Off", null, ExpectedVariableContents.NONE),
            [AutomationActions.USK1TieOn] = (0, AutomationActions.USK1TieOn, "USK1TieOn", null, ExpectedVariableContents.NONE),
            [AutomationActions.USK1TieOff] = (0, AutomationActions.USK1TieOff, "USK1TieOff", null, ExpectedVariableContents.NONE),
            [AutomationActions.USK1SetTypeChroma] = (0, AutomationActions.USK1SetTypeChroma, "USK1SetTypeChroma", null, ExpectedVariableContents.NONE),
            [AutomationActions.USK1SetTypeDVE] = (0, AutomationActions.USK1SetTypeDVE, "USK1SetTypeDVE", null, ExpectedVariableContents.NONE),
            [AutomationActions.USK1SetTypePATTERN] = (0, AutomationActions.USK1SetTypePATTERN, "USK1SetTypePATTERN", null, ExpectedVariableContents.NONE),

            [AutomationActions.RecordStart] = (0, AutomationActions.RecordStart, "RecordStart", null, ExpectedVariableContents.NONE),
            [AutomationActions.RecordStop] = (0, AutomationActions.RecordStop, "RecordStop", null, ExpectedVariableContents.NONE),

            [AutomationActions.OpenAudioPlayer] = (0, AutomationActions.OpenAudioPlayer, "OpenAudioPlayer", null, ExpectedVariableContents.NONE),
            [AutomationActions.PlayAuxAudio] = (0, AutomationActions.PlayAuxAudio, "PlayAuxAudio", null, ExpectedVariableContents.NONE),
            [AutomationActions.StopAuxAudio] = (0, AutomationActions.StopAuxAudio, "StopAuxAudio", null, ExpectedVariableContents.NONE),
            [AutomationActions.PauseAuxAudio] = (0, AutomationActions.PauseAuxAudio, "PauseAuxAudio", null, ExpectedVariableContents.NONE),
            [AutomationActions.ReplayAuxAudio] = (0, AutomationActions.ReplayAuxAudio, "ReplayAuxAudio", null, ExpectedVariableContents.NONE),

            [AutomationActions.PlayMedia] = (0, AutomationActions.PlayMedia, "PlayMedia", null, ExpectedVariableContents.NONE),
            [AutomationActions.PauseMedia] = (0, AutomationActions.PauseMedia, "PauseMedia", null, ExpectedVariableContents.NONE),
            [AutomationActions.StopMedia] = (0, AutomationActions.StopMedia, "StopMedia", null, ExpectedVariableContents.NONE),
            [AutomationActions.RestartMedia] = (0, AutomationActions.RestartMedia, "RestartMedia", null, ExpectedVariableContents.NONE),
            [AutomationActions.MuteMedia] = (0, AutomationActions.MuteMedia, "MuteMedia", null, ExpectedVariableContents.NONE),
            [AutomationActions.UnMuteMedia] = (0, AutomationActions.UnMuteMedia, "UnMuteMedia", null, ExpectedVariableContents.NONE),

            [AutomationActions.DriveNextSlide] = (0, AutomationActions.DriveNextSlide, "DriveNextSlide", null, ExpectedVariableContents.NONE),
            [AutomationActions.Timer1Restart] = (0, AutomationActions.Timer1Restart, "Timer1Restart", null, ExpectedVariableContents.NONE),


            [AutomationActions.Timer1Restart] = (0, AutomationActions.Timer1Restart, "Timer1Restart", null, ExpectedVariableContents.NONE),

            [AutomationActions.PresetSelect] = new AutomationActionMetadata(1, AutomationActions.PresetSelect, "PresetSelect", new List<(AutomationActionArgType, string)>
            {
                (AutomationActionArgType.Integer, "SourceID")
            }, ExpectedVariableContents.VIDEOSOURCE),
            [AutomationActions.ProgramSelect] = new AutomationActionMetadata(1, AutomationActions.ProgramSelect, "ProgramSelect", new List<(AutomationActionArgType, string)>
            {
                (AutomationActionArgType.Integer, "SourceID")
            }, ExpectedVariableContents.VIDEOSOURCE),
            [AutomationActions.AuxSelect] = new AutomationActionMetadata(1, AutomationActions.AuxSelect, "AuxSelect", new List<(AutomationActionArgType, string)>
            {
                (AutomationActionArgType.Integer, "SourceID")
            }, ExpectedVariableContents.VIDEOSOURCE),
            [AutomationActions.USK1Fill] = new AutomationActionMetadata(1, AutomationActions.USK1Fill, "USK1Fill", new List<(AutomationActionArgType, string)>
            {
                (AutomationActionArgType.Integer, "SourceID")
            }, ExpectedVariableContents.VIDEOSOURCE),


            [AutomationActions.LoadAudio] = (1, AutomationActions.LoadAudio, "LoadAudioFile", new List<(AutomationActionArgType, string)> { (AutomationActionArgType.String, "AudioFile") }, ExpectedVariableContents.PROJECTASSET),



            [AutomationActions.DelayMs] = (1, AutomationActions.DelayMs, "DelayMs", new List<(AutomationActionArgType, string)> { (AutomationActionArgType.Integer, "Delay") }, ExpectedVariableContents.NONE),
            [AutomationActions.DelayUntil] = (1, AutomationActions.DelayUntil, "DelayUntil", new List<(AutomationActionArgType, string)> { (AutomationActionArgType.String, "DateTime") }, ExpectedVariableContents.NONE),

            [AutomationActions.JumpToSlide] = (1, AutomationActions.JumpToSlide, "JumpToSlide", new List<(AutomationActionArgType, string)> { (AutomationActionArgType.Integer, "SlideNum") }, ExpectedVariableContents.NONE),

            // duplicate command for backwards compatibility
            [AutomationActions.WatchSwitcherStateBoolVal] = (3, AutomationActions.WatchSwitcherStateBoolVal, "WatchSwitcherStateBoolVal", new List<(AutomationActionArgType, string)>
            {
                (AutomationActionArgType.String,"WatchVar"),
                (AutomationActionArgType.Boolean,"ExpectedVal"),
                (AutomationActionArgType.String,"CondName"),
            },
            ExpectedVariableContents.EXPOSEDSTATE),
            [AutomationActions.WatchSwitcherStateIntVal] = (3, AutomationActions.WatchSwitcherStateIntVal, "WatchSwitcherStateIntVal", new List<(AutomationActionArgType, string)>
            {
                (AutomationActionArgType.String,"WatchVar"),
                (AutomationActionArgType.Integer,"ExpectedVal"),
                (AutomationActionArgType.String,"CondName"),
            },
            ExpectedVariableContents.EXPOSEDSTATE),


            [AutomationActions.WatchStateBoolVal] = (3, AutomationActions.WatchSwitcherStateBoolVal, "WatchStateBoolVal", new List<(AutomationActionArgType, string)>
            {
                (AutomationActionArgType.String,"WatchVar"),
                (AutomationActionArgType.Boolean,"ExpectedVal"),
                (AutomationActionArgType.String,"CondName"),
            },
            ExpectedVariableContents.EXPOSEDSTATE),
            [AutomationActions.WatchStateIntVal] = (3, AutomationActions.WatchSwitcherStateIntVal, "WatchStateIntVal", new List<(AutomationActionArgType, string)>
            {
                (AutomationActionArgType.String,"WatchVar"),
                (AutomationActionArgType.Integer,"ExpectedVal"),
                (AutomationActionArgType.String,"CondName"),
            },
            ExpectedVariableContents.EXPOSEDSTATE),


            [AutomationActions.InitComputedVal] = (3, AutomationActions.InitComputedVal, "InitComputedVal", new List<(AutomationActionArgType, string)>
            {
                (AutomationActionArgType.String,"VarName"),
                (AutomationActionArgType.String,"TypeStr"),
                (AutomationActionArgType.String,"VarDefaultVal"),
            },
            ExpectedVariableContents.EXPOSEDSTATE),
            [AutomationActions.WriteComputedVal] = (2, AutomationActions.WriteComputedVal, "WriteComputedVal", new List<(AutomationActionArgType, string)>
            {
                (AutomationActionArgType.String,"VarName"),
                (AutomationActionArgType.String,"VarVal"),
            },
            ExpectedVariableContents.NONE),
            [AutomationActions.SetupComputedTrack] = (2, AutomationActions.SetupComputedTrack, "SetupTrackVal", new List<(AutomationActionArgType, string)>
            {
                (AutomationActionArgType.String,"VarName"),
                (AutomationActionArgType.String,"TrackTarget"),
            },
            ExpectedVariableContents.EXPOSEDSTATE),
            [AutomationActions.ReleaseComputedTrack] = (1, AutomationActions.InitComputedVal, "ReleaseTackVal", new List<(AutomationActionArgType, string)>
            {
                (AutomationActionArgType.String,"VarName"),
            },
            ExpectedVariableContents.NONE),

            [AutomationActions.RedrawDynamicControls] = (0, AutomationActions.RedrawDynamicControls, "PaintCtrls", null, ExpectedVariableContents.NONE),




            [AutomationActions.PlacePIP] = (8, AutomationActions.PlacePIP, "PlacePIP", new List<(AutomationActionArgType, string)>
            {
                (AutomationActionArgType.Double, "PosX"),
                (AutomationActionArgType.Double, "PosY"),
                (AutomationActionArgType.Double, "ScaleX"),
                (AutomationActionArgType.Double, "ScaleY"),
                (AutomationActionArgType.Double, "MaskLeft"),
                (AutomationActionArgType.Double, "MaskRight"),
                (AutomationActionArgType.Double, "MaskTop"),
                (AutomationActionArgType.Double, "MaskBottom"),
            },
            ExpectedVariableContents.NONE),

            [AutomationActions.ConfigurePATTERN] = (7, AutomationActions.ConfigurePATTERN, "ApplyPATTERN", new List<(AutomationActionArgType, string)>
            {
                (AutomationActionArgType.String, "Type"),
                (AutomationActionArgType.Boolean, "Inverted"),
                (AutomationActionArgType.Double, "Size"),
                (AutomationActionArgType.Double, "Symmetry"),
                (AutomationActionArgType.Double, "Softness"),
                (AutomationActionArgType.Double, "XOffset"),
                (AutomationActionArgType.Double, "YOffset"),
            },
            ExpectedVariableContents.NONE),

            [AutomationActions.SetupButtons] = (3, AutomationActions.SetupButtons, "SetupButtons", new List<(AutomationActionArgType, string)>
            {
                (AutomationActionArgType.String, "File"),
                (AutomationActionArgType.String, "ResourcePath"),
                (AutomationActionArgType.Boolean, "Overwrite"),
            },
            ExpectedVariableContents.NONE),

            [AutomationActions.SetupExtras] = (4, AutomationActions.SetupExtras, "SetupExtras", new List<(AutomationActionArgType, string)>()
            {
                (AutomationActionArgType.String, "ExtraID"),
                (AutomationActionArgType.String, "File"),
                (AutomationActionArgType.String, "ResourcePath"),
                (AutomationActionArgType.Boolean, "Overwrite"),
            },
            ExpectedVariableContents.NONE),


            [AutomationActions.ForceRunPostSet] = (2, AutomationActions.ForceRunPostSet, "ForceRunPostset", new List<(AutomationActionArgType, string)>()
            {
                (AutomationActionArgType.Boolean, "Force"),
                (AutomationActionArgType.Integer, "SourceID"),
            },
            ExpectedVariableContents.NONE),

            [AutomationActions.FireActivePreset] = (1, AutomationActions.FireActivePreset, "FireActivePreset", new List<(AutomationActionArgType, string)>()
            {
                (AutomationActionArgType.String, "CamName"),
            },
            ExpectedVariableContents.NONE),
            [AutomationActions.FireCamPreset] = (4, AutomationActions.FireCamPreset, "FireCamPreset", new List<(AutomationActionArgType, string)>()
            {
                (AutomationActionArgType.String, "CamName"),
                (AutomationActionArgType.String, "PresetName"),
                (AutomationActionArgType.Integer, "Speed"),
                (AutomationActionArgType.String, "ZoomPST"),
            },
            ExpectedVariableContents.NONE),
            [AutomationActions.FireCamDrive] = (5, AutomationActions.FireCamDrive, "FireCamDrive", new List<(AutomationActionArgType, string)>()
            {
                (AutomationActionArgType.String, "CamName"),
                (AutomationActionArgType.Integer, "DirX"),
                (AutomationActionArgType.Integer, "DirY"),
                (AutomationActionArgType.Integer, "SpeedX"),
                (AutomationActionArgType.Integer, "SpeedY"),
            },
            ExpectedVariableContents.NONE),

        };

    }
}
