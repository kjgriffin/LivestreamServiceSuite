using System;
using System.Text.RegularExpressions;

namespace Integrated_Presenter
{
    public class AutomationAction
    {
        public AutomationActionType Action { get; set; } = AutomationActionType.None;
        public string Message { get; set; } = "";
        public int DataI { get; set; } = 0;
        public string DataS { get; set; } = "";


        public static AutomationAction Parse(string command)
        {
            AutomationAction a = new AutomationAction();
            a.Action = AutomationActionType.None;
            a.DataI = 0;
            a.DataS = "";
            a.Message = "";

            if (command.StartsWith("arg0:"))
            {
                var res = Regex.Match(command, @"arg0:(?<commandname>.*?)(\[(?<msg>.*)\])?;");
                string cmd = res.Groups["commandname"].Value;
                string msg = res.Groups["msg"].Value;
                a.Message = msg;
                switch (cmd)
                {
                    case "AutoTrans":
                        a.Action = AutomationActionType.AutoTrans;
                        break;
                    case "CutTrans":
                        a.Action = AutomationActionType.CutTrans;
                        break;
                    case "AutoTakePresetIfOnSlide":
                        a.Action = AutomationActionType.AutoTakePresetIfOnSlide;
                        break;

                    case "DSK1On":
                        a.Action = AutomationActionType.DSK1On;
                        break;
                    case "DSK1Off":
                        a.Action = AutomationActionType.DSK1Off;
                        break;
                    case "DSK1FadeOn":
                        a.Action = AutomationActionType.DSK1FadeOn;
                        break;
                    case "DSK1FadeOff":
                        a.Action = AutomationActionType.DSK1FadeOff;
                        break;

                    case "DSK2On":
                        a.Action = AutomationActionType.DSK2On;
                        break;
                    case "DSK2Off":
                        a.Action = AutomationActionType.DSK2Off;
                        break;
                    case "DSK2FadeOn":
                        a.Action = AutomationActionType.DSK2FadeOn;
                        break;
                    case "DSK2FadeOff":
                        a.Action = AutomationActionType.DSK2FadeOff;
                        break;

                    case "USK1On":
                        a.Action = AutomationActionType.USK1On;
                        break;
                    case "USK1Off":
                        a.Action = AutomationActionType.USK1Off;
                        break;
                    case "USK1SetTypeChroma":
                        a.Action = AutomationActionType.USK1SetTypeChroma;
                        break;
                    case "USK1SetTypeDVE":
                        a.Action = AutomationActionType.USK1SetTypeDVE;
                        break;

                    case "RecordStart":
                        a.Action = AutomationActionType.RecordStart;
                        break;
                    case "RecordStop":
                        a.Action = AutomationActionType.RecordStop;
                        break;

                    case "OpenAudioPlayer":
                        a.Action = AutomationActionType.OpenAudioPlayer;
                        break;
                    case "PlayAuxAudio":
                        a.Action = AutomationActionType.PlayAuxAudio;
                        break;
                    case "StopAuxAudio":
                        a.Action = AutomationActionType.StopAuxAudio;
                        break;
                    case "PauseAuxAudio":
                        a.Action = AutomationActionType.PauseAuxAudio;
                        break;
                    case "ReplayAuxAudio":
                        a.Action = AutomationActionType.ReplayAuxAudio;
                        break;

                    case "PlayMedia":
                        a.Action = AutomationActionType.PlayMedia;
                        break;
                    case "PauseMedia":
                        a.Action = AutomationActionType.PauseMedia;
                        break;
                    case "StopMedia":
                        a.Action = AutomationActionType.StopMedia;
                        break;
                    case "RestartMedia":
                        a.Action = AutomationActionType.RestartMedia;
                        break;
                    case "MuteMedia":
                        a.Action = AutomationActionType.MuteMedia;
                        break;
                    case "UnMuteMedia":
                        a.Action = AutomationActionType.UnMuteMedia;
                        break;

                    case "DriveNextSlide":
                        a.Action = AutomationActionType.DriveNextSlide;
                        break;
                    case "Timer1Restart":
                        a.Action = AutomationActionType.Timer1Restart;
                        break;
                }
            }
            if (command.StartsWith("arg1:"))
            {
                var res = Regex.Match(command, @"arg1:(?<commandname>.*?)\((?<param>.*)\)(\[(?<msg>.*)\])?;");
                string cmd = res.Groups["commandname"].Value;
                string arg1 = res.Groups["param"].Value;
                string msg = res.Groups["msg"].Value;
                a.Message = msg;
                switch (cmd)
                {
                    case "PresetSelect":
                        a.Action = AutomationActionType.PresetSelect;
                        a.DataI = Convert.ToInt32(arg1);
                        break;
                    case "ProgramSelect":
                        a.Action = AutomationActionType.ProgramSelect;
                        a.DataI = Convert.ToInt32(arg1);
                        break;
                    case "AuxSelect":
                        a.Action = AutomationActionType.AuxSelect;
                        a.DataI = Convert.ToInt32(arg1);
                        break;
                    case "DelayMs":
                        a.Action = AutomationActionType.DelayMs;
                        a.DataI = Convert.ToInt32(arg1);
                        break;
                    case "LoadAudioFile":
                        a.Action = AutomationActionType.LoadAudio;
                        a.DataS = arg1;
                        break;
                    default:
                        break;
                }
            }


            return a;
        }

    }

}
