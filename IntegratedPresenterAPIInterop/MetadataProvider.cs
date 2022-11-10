using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegratedPresenterAPIInterop
{
    public static class MetadataProvider
    {
        public static Dictionary<AutomationActions, AutomationActionMetadata> ScriptActionsMetadata = new Dictionary<AutomationActions, AutomationActionMetadata>()
        {
            [AutomationActions.OpsNote] = (0, AutomationActions.OpsNote, "note", null),

            [AutomationActions.AutoTrans] = (0, AutomationActions.AutoTrans, "AutoTrans", null),
            [AutomationActions.CutTrans] = (0, AutomationActions.CutTrans, "CutTrans", null),
            [AutomationActions.AutoTakePresetIfOnSlide] = (0, AutomationActions.AutoTakePresetIfOnSlide, "AutoTakePresetIfOnSlide", null),

            [AutomationActions.DSK1On] = (0, AutomationActions.DSK1On, "DSK1On", null),
            [AutomationActions.DSK1Off] = (0, AutomationActions.DSK1Off, "DSK1Off", null),
            [AutomationActions.DSK1FadeOn] = (0, AutomationActions.DSK1FadeOn, "DSK1FadeOn", null),
            [AutomationActions.DSK1FadeOff] = (0, AutomationActions.DSK1FadeOff, "DSK1FadeOff", null),
            [AutomationActions.DSK1TieOn] = (0, AutomationActions.DSK1TieOn, "DSK1TieOn", null),
            [AutomationActions.DSK1TieOff] = (0, AutomationActions.DSK1TieOff, "DSK1TieOff", null),

            [AutomationActions.DSK2On] = (0, AutomationActions.DSK2On, "DSK2On", null),
            [AutomationActions.DSK2Off] = (0, AutomationActions.DSK2Off, "DSK2Off", null),
            [AutomationActions.DSK2FadeOn] = (0, AutomationActions.DSK2FadeOn, "DSK2FadeOn", null),
            [AutomationActions.DSK2FadeOff] = (0, AutomationActions.DSK2FadeOff, "DSK2FadeOff", null),
            [AutomationActions.DSK2TieOn] = (0, AutomationActions.DSK2TieOn, "DSK2TieOn", null),
            [AutomationActions.DSK2TieOff] = (0, AutomationActions.DSK2TieOff, "DSK2TieOff", null),

            [AutomationActions.USK1On] = (0, AutomationActions.USK1On, "USK1On", null),
            [AutomationActions.USK1Off] = (0, AutomationActions.USK1Off, "USK1Off", null),
            [AutomationActions.USK1TieOn] = (0, AutomationActions.USK1TieOn, "USK1TieOn", null),
            [AutomationActions.USK1TieOff] = (0, AutomationActions.USK1TieOff, "USK1TieOff", null),
            [AutomationActions.USK1SetTypeChroma] = (0, AutomationActions.USK1SetTypeChroma, "USK1SetTypeChroma", null),
            [AutomationActions.USK1SetTypeDVE] = (0, AutomationActions.USK1SetTypeDVE, "USK1SetTypeDVE", null),

            [AutomationActions.RecordStart] = (0, AutomationActions.RecordStart, "RecordStart", null),
            [AutomationActions.RecordStop] = (0, AutomationActions.RecordStop, "RecordStop", null),

            [AutomationActions.OpenAudioPlayer] = (0, AutomationActions.OpenAudioPlayer, "OpenAudioPlayer", null),
            [AutomationActions.PlayAuxAudio] = (0, AutomationActions.PlayAuxAudio, "PlayAuxAudio", null),
            [AutomationActions.StopAuxAudio] = (0, AutomationActions.StopAuxAudio, "StopAuxAudio", null),
            [AutomationActions.PauseAuxAudio] = (0, AutomationActions.PauseAuxAudio, "PauseAuxAudio", null),
            [AutomationActions.ReplayAuxAudio] = (0, AutomationActions.ReplayAuxAudio, "ReplayAuxAudio", null),

            [AutomationActions.PlayMedia] = (0, AutomationActions.PlayMedia, "PlayMedia", null),
            [AutomationActions.PauseMedia] = (0, AutomationActions.PauseMedia, "PauseMedia", null),
            [AutomationActions.StopMedia] = (0, AutomationActions.StopMedia, "StopMedia", null),
            [AutomationActions.RestartMedia] = (0, AutomationActions.RestartMedia, "RestartMedia", null),
            [AutomationActions.MuteMedia] = (0, AutomationActions.MuteMedia, "MuteMedia", null),
            [AutomationActions.UnMuteMedia] = (0, AutomationActions.UnMuteMedia, "UnMuteMedia", null),

            [AutomationActions.DriveNextSlide] = (0, AutomationActions.DriveNextSlide, "DriveNextSlide", null),
            [AutomationActions.Timer1Restart] = (0, AutomationActions.Timer1Restart, "Timer1Restart", null),



            [AutomationActions.PresetSelect] = (1, AutomationActions.PresetSelect, "PresetSelect", new List<AutomationActionArgType> { AutomationActionArgType.Integer }),
            [AutomationActions.ProgramSelect] = (1, AutomationActions.ProgramSelect, "ProgramSelect", new List<AutomationActionArgType> { AutomationActionArgType.Integer }),
            [AutomationActions.AuxSelect] = (1, AutomationActions.AuxSelect, "AuxSelect", new List<AutomationActionArgType> { AutomationActionArgType.Integer }),
            [AutomationActions.USK1Fill] = (1, AutomationActions.USK1Fill, "USK1Fill", new List<AutomationActionArgType> { AutomationActionArgType.Integer }),
            [AutomationActions.DelayMs] = (1, AutomationActions.DelayMs, "DelayMs", new List<AutomationActionArgType> { AutomationActionArgType.Integer }),
            [AutomationActions.LoadAudio] = (1, AutomationActions.LoadAudio, "LoadAudioFile", new List<AutomationActionArgType> { AutomationActionArgType.String }),


            [AutomationActions.JumpToSlide] = (1, AutomationActions.JumpToSlide, "JumpToSlide", new List<AutomationActionArgType> { AutomationActionArgType.Integer }),
            [AutomationActions.WatchSwitcherStateBoolVal] = (3, AutomationActions.WatchSwitcherStateBoolVal, "WatchSwitcherStateBoolVal", new List<AutomationActionArgType>
            {
                AutomationActionArgType.String,
                AutomationActionArgType.Boolean,
                AutomationActionArgType.String,
            }),
            [AutomationActions.WatchSwitcherStateIntVal] = (3, AutomationActions.WatchSwitcherStateIntVal, "WatchSwitcherStateIntVal", new List<AutomationActionArgType>
            {
                AutomationActionArgType.String,
                AutomationActionArgType.Integer,
                AutomationActionArgType.String,
            }),

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
            }),
        };

    }
}
