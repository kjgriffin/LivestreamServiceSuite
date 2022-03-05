
using IntegratedPresenter.BMDSwitcher.Config;

using System;
using System.Text.RegularExpressions;

namespace IntegratedPresenterAPIInterop
{
    public class AutomationAction
    {
        public AutomationActions Action { get; set; } = AutomationActions.None;
        public string Message { get; set; } = "";
        public int DataI { get; set; } = 0;
        public string DataS { get; set; } = "";
        public object DataO { get; set; } = new object();

        public Dictionary<string, bool> Conditions { get; set; } = new Dictionary<string, bool>();

        public override string ToString()
        {
            return $"{Action}";
        }

        public bool MeetsConditionsToRun(Dictionary<string, bool> condValues)
        {
            foreach (var condReq in Conditions)
            {
                if (condValues?.TryGetValue(condReq.Key, out var condVal) == true)
                {
                    if (condVal != condReq.Value)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public static AutomationAction Parse(string cline)
        {
            AutomationAction a = new AutomationAction();
            a.Action = AutomationActions.None;
            a.DataI = 0;
            a.DataS = "";
            a.Message = "";
            a.Conditions = new Dictionary<string, bool>();

            string command = cline;

            if (cline.StartsWith("<"))
            {
                var res = Regex.Match(cline, @"<(?<conditions>.*)>(?<command>.*)");
                var conditions = res.Groups["conditions"]?.Value.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>(0);
                foreach (var cond in conditions)
                {
                    if (cond.StartsWith("!"))
                    {
                        a.Conditions[cond.Substring(1, cond.Length - 1)] = false;
                    }
                    else
                    {
                        a.Conditions[cond] = true;
                    }
                }
                command = res.Groups["command"].Value;
            }

            if (command.StartsWith("*note"))
            {
                var res = Regex.Match(command, @"\*note(\[(?<msg>.*)\])?;");
                a.Message = res.Groups["msg"].Value;
                a.Action = AutomationActions.OpsNote;
            }

            if (command.StartsWith("arg0:"))
            {
                var res = Regex.Match(command, @"arg0:(?<commandname>.*?)(\[(?<msg>.*)\])?;");
                string cmd = res.Groups["commandname"].Value;
                string msg = res.Groups["msg"].Value;
                a.Message = msg;

                var cmdmetadata = MetadataProvider.ScriptActionsMetadata.Values.FirstOrDefault(x => x.ActionName == cmd);
                a.Action = cmdmetadata.Action;
            }
            if (command.StartsWith("arg1:"))
            {
                var res = Regex.Match(command, @"arg1:(?<commandname>.*?)\((?<param>.*)\)(\[(?<msg>.*)\])?;");
                string cmd = res.Groups["commandname"].Value;
                string arg1 = res.Groups["param"].Value;
                string msg = res.Groups["msg"].Value;
                a.Message = msg;

                var cmdmetadata = MetadataProvider.ScriptActionsMetadata.Values.FirstOrDefault(x => x.ActionName == cmd);
                a.Action = cmdmetadata.Action;

                switch (cmdmetadata.OrderedArgTypes?.FirstOrDefault())
                {
                    case AutomationActionArgType.Integer:
                        a.DataI = Convert.ToInt32(arg1);
                        break;
                    case AutomationActionArgType.String:
                        a.DataS = arg1;
                        break;
                    default:
                        break;
                }
            }
            if (command.StartsWith("argd8:"))
            {
                var res = Regex.Match(command, @"argd8:(?<commandname>.*?)\((?<param1>.+),(?<param2>.+),(?<param3>.+),(?<param4>.+),(?<param5>.+),(?<param6>.+),(?<param7>.+),(?<param8>.+)\)(\[(?<msg>.*)\])?;");
                string cmd = res.Groups["commandname"].Value;
                double arg1 = Convert.ToDouble(res.Groups["param1"].Value);
                double arg2 = Convert.ToDouble(res.Groups["param2"].Value);
                double arg3 = Convert.ToDouble(res.Groups["param3"].Value);
                double arg4 = Convert.ToDouble(res.Groups["param4"].Value);
                double arg5 = Convert.ToDouble(res.Groups["param5"].Value);
                double arg6 = Convert.ToDouble(res.Groups["param6"].Value);
                double arg7 = Convert.ToDouble(res.Groups["param7"].Value);
                double arg8 = Convert.ToDouble(res.Groups["param8"].Value);
                string msg = res.Groups["msg"].Value;
                a.Message = msg;

                var cmdmetadata = MetadataProvider.ScriptActionsMetadata.Values.FirstOrDefault(x => x.ActionName == cmd);
                a.Action = cmdmetadata.Action;

                if (cmdmetadata.Action == AutomationActions.PlacePIP)
                {
                    PIPPlaceSettings placement = new PIPPlaceSettings()
                    {
                        PosX = arg1,
                        PosY = arg2,
                        ScaleX = arg3,
                        ScaleY = arg4,
                        MaskLeft = arg5,
                        MaskRight = arg6,
                        MaskTop = arg7,
                        MaskBottom = arg8,
                    };
                    a.DataO = placement;
                }

            }

            return a;
        }

    }

}
