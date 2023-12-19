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

            [AutomationActions.PresetSelect] = (1, AutomationActions.PresetSelect, "PresetSelect", new List<AutomationActionArgType> { AutomationActionArgType.Integer }, ExpectedVariableContents.VIDEOSOURCE),
            [AutomationActions.ProgramSelect] = (1, AutomationActions.ProgramSelect, "ProgramSelect", new List<AutomationActionArgType> { AutomationActionArgType.Integer }, ExpectedVariableContents.VIDEOSOURCE),
            [AutomationActions.AuxSelect] = (1, AutomationActions.AuxSelect, "AuxSelect", new List<AutomationActionArgType> { AutomationActionArgType.Integer }, ExpectedVariableContents.VIDEOSOURCE),
            [AutomationActions.USK1Fill] = (1, AutomationActions.USK1Fill, "USK1Fill", new List<AutomationActionArgType> { AutomationActionArgType.Integer }, ExpectedVariableContents.VIDEOSOURCE),
            [AutomationActions.LoadAudio] = (1, AutomationActions.LoadAudio, "LoadAudioFile", new List<AutomationActionArgType> { AutomationActionArgType.String }, ExpectedVariableContents.PROJECTASSET),

            [AutomationActions.DelayMs] = (1, AutomationActions.DelayMs, "DelayMs", new List<AutomationActionArgType> { AutomationActionArgType.Integer }, ExpectedVariableContents.NONE),
            [AutomationActions.DelayUntil] = (1, AutomationActions.DelayUntil, "DelayUntil", new List<AutomationActionArgType> { AutomationActionArgType.String }, ExpectedVariableContents.NONE),

            [AutomationActions.JumpToSlide] = (1, AutomationActions.JumpToSlide, "JumpToSlide", new List<AutomationActionArgType> { AutomationActionArgType.Integer }, ExpectedVariableContents.NONE),
            [AutomationActions.WatchSwitcherStateBoolVal] = (3, AutomationActions.WatchSwitcherStateBoolVal, "WatchSwitcherStateBoolVal", new List<AutomationActionArgType>
            {
                AutomationActionArgType.String,
                AutomationActionArgType.Boolean,
                AutomationActionArgType.String,
            },
            ExpectedVariableContents.EXPOSEDSTATE),
            [AutomationActions.WatchSwitcherStateIntVal] = (3, AutomationActions.WatchSwitcherStateIntVal, "WatchSwitcherStateIntVal", new List<AutomationActionArgType>
            {
                AutomationActionArgType.String,
                AutomationActionArgType.Integer,
                AutomationActionArgType.String,
            },
            ExpectedVariableContents.EXPOSEDSTATE),
            // duplicate command for backwards compatibility
            [AutomationActions.WatchStateBoolVal] = (3, AutomationActions.WatchSwitcherStateBoolVal, "WatchStateBoolVal", new List<AutomationActionArgType>
            {
                AutomationActionArgType.String,
                AutomationActionArgType.Boolean,
                AutomationActionArgType.String,
            },
            ExpectedVariableContents.EXPOSEDSTATE),
            [AutomationActions.WatchStateIntVal] = (3, AutomationActions.WatchSwitcherStateIntVal, "WatchStateIntVal", new List<AutomationActionArgType>
            {
                AutomationActionArgType.String,
                AutomationActionArgType.Integer,
                AutomationActionArgType.String,
            },
            ExpectedVariableContents.EXPOSEDSTATE),



            [AutomationActions.PlacePIP] = (8, AutomationActions.PlacePIP, "PlacePIP", new List<AutomationActionArgType>
            {
                AutomationActionArgType.Double,
                AutomationActionArgType.Double,
                AutomationActionArgType.Double,
                AutomationActionArgType.Double,
                AutomationActionArgType.Double,
                AutomationActionArgType.Double,
                AutomationActionArgType.Double,
                AutomationActionArgType.Double,
            },
            ExpectedVariableContents.NONE),

            [AutomationActions.ConfigurePATTERN] = (7, AutomationActions.ConfigurePATTERN, "ApplyPATTERN", new List<AutomationActionArgType>
            {
                AutomationActionArgType.String,
                AutomationActionArgType.Boolean,
                AutomationActionArgType.Double,
                AutomationActionArgType.Double,
                AutomationActionArgType.Double,
                AutomationActionArgType.Double,
                AutomationActionArgType.Double,
            },
            ExpectedVariableContents.NONE),

            [AutomationActions.SetupButtons] = (3, AutomationActions.SetupButtons, "SetupButtons", new List<AutomationActionArgType>
            {
                AutomationActionArgType.String,
                AutomationActionArgType.String,
                AutomationActionArgType.Boolean,
            },
            ExpectedVariableContents.NONE),

            [AutomationActions.SetupExtras] = (4, AutomationActions.SetupExtras, "SetupExtras", new List<AutomationActionArgType>()
            {
                AutomationActionArgType.String,
                AutomationActionArgType.String,
                AutomationActionArgType.String,
                AutomationActionArgType.Boolean,
            },
            ExpectedVariableContents.NONE),


            [AutomationActions.ForceRunPostSet] = (2, AutomationActions.ForceRunPostSet, "ForceRunPostset", new List<AutomationActionArgType>()
            {
                AutomationActionArgType.Boolean,
                AutomationActionArgType.Integer,
            },
            ExpectedVariableContents.NONE),

            [AutomationActions.FireActivePreset] = (1, AutomationActions.FireActivePreset, "FireActivePreset", new List<AutomationActionArgType>()
            {
                AutomationActionArgType.String,
            },
            ExpectedVariableContents.NONE),
            [AutomationActions.FireCamPreset] = (4, AutomationActions.FireCamPreset, "FireCamPreset", new List<AutomationActionArgType>()
            {
                AutomationActionArgType.String,
                AutomationActionArgType.String,
                AutomationActionArgType.Integer,
                AutomationActionArgType.String,
            },
            ExpectedVariableContents.NONE),
            [AutomationActions.FireCamDrive] = (5, AutomationActions.FireCamDrive, "FireCamDrive", new List<AutomationActionArgType>()
            {
                AutomationActionArgType.String,
                AutomationActionArgType.Integer,
                AutomationActionArgType.Integer,
                AutomationActionArgType.Integer,
                AutomationActionArgType.Integer,
            },
            ExpectedVariableContents.NONE),

        };

    }
}
