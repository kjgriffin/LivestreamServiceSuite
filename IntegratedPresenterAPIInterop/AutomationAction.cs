
using Configurations.SwitcherConfig;

using IntegratedPresenter.BMDSwitcher.Config;

using System.Text.RegularExpressions;

using VariableMarkupAttributes;

namespace IntegratedPresenterAPIInterop
{
    public class SumOfProductExpression
    {
        public List<Dictionary<string, bool>> Products { get; set; } = new List<Dictionary<string, bool>>();

        public bool HasConditions
        {
            get
            {
                return Products.Any(x => x.Any());
            }
        }

        public static SumOfProductExpression Parse(string input)
        {
            List<Dictionary<string, bool>> expectedProducts = new List<Dictionary<string, bool>>();

            var productterms = Regex.Split(input, @"\+");

            foreach (var pterm in productterms)
            {
                Dictionary<string, bool> expectedTerms = new Dictionary<string, bool>();

                var factors = Regex.Split(pterm, "[*,]");

                foreach (var factor in factors)
                {
                    // compute expectation of the factor
                    string fstr = factor.Trim();
                    if (fstr.StartsWith("!"))
                    {
                        expectedTerms[fstr.Substring(1, fstr.Length - 1)] = false;
                    }
                    else
                    {
                        expectedTerms[fstr] = true;
                    }
                }

                expectedProducts.Add(expectedTerms);
            }

            return new SumOfProductExpression
            {
                Products = expectedProducts,
            };
        }


        public static bool EvaluateProductTerm(Dictionary<string, bool> product, Dictionary<string, bool> condValues)
        {
            foreach (var cterm in product)
            {
                if (condValues?.TryGetValue(cterm.Key, out var cval) == true)
                {
                    if (cval != cterm.Value)
                    {
                        // product term fails
                        return false;
                    }
                }
                else if (cterm.Value)
                {
                    // expected true- undefined conditions are always false
                    // product term fails
                    return false;
                }

            }
            return true;
        }

        public static bool EvaluateExpression(SumOfProductExpression expr, Dictionary<string, bool> condValues)
        {
            if (!expr.HasConditions)
            {
                return true;
            }
            foreach (var productExpr in expr?.Products)
            {
                if (EvaluateProductTerm(productExpr, condValues))
                {
                    return true;
                }
            }
            return false;
        }

    }

    public class AutomationAction
    {
        public AutomationActions Action { get; set; } = AutomationActions.None;
        public string Message { get; set; } = "";
        public int DataI { get; set; } = 0;
        public string DataS { get; set; } = "";
        public object DataO { get; set; } = new object();
        public List<object> RawParams { get; set; } = new List<object>();

        public SumOfProductExpression ExpectedConditions { get; set; } = new SumOfProductExpression();

        public override string ToString()
        {
            return $"{Action}";
        }

        public bool MeetsConditionsToRun(Dictionary<string, bool> condValues)
        {
            return SumOfProductExpression.EvaluateExpression(ExpectedConditions, condValues);
        }

        public static AutomationAction Parse(string cline)
        {
            AutomationAction a = new AutomationAction();
            a.Action = AutomationActions.None;
            a.DataI = 0;
            a.DataS = "";
            a.Message = "";
            a.ExpectedConditions = new SumOfProductExpression();

            string command = cline;

            if (cline.StartsWith("<"))
            {
                var res = Regex.Match(cline, @"<(?<conditions>.*)>(?<command>.*)");

                a.ExpectedConditions = SumOfProductExpression.Parse(res.Groups["conditions"].Value);

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
            if (command.StartsWith("cmd:")) // dynamically parse it
            {
                var cmatch = Regex.Match(command, @"cmd:(?<commandname>.*?)(\((?<params>.*?)\))?(\[(?<msg>.*)\])?;");

                string cmdName = cmatch.Groups["commandname"].Value;
                string pargs = cmatch.Groups["params"].Value;
                string msg = cmatch.Groups["msg"].Value;

                a.Message = msg;
                // find command
                if (MetadataProvider.ScriptActionsMetadata.Any(x => x.Value.ActionName == cmdName))
                {
                    var cmdMetadata = MetadataProvider.ScriptActionsMetadata.FirstOrDefault(x => x.Value.ActionName == cmdName);

                    // with the metadata availabe now we can dynamicaly parse the args based on what's expected
                    var parsedParams = ParseDynamicParams(pargs, cmdMetadata.Value);

                    if (parsedParams.success && parsedParams.data.Count == cmdMetadata.Value.NumArgs || cmdMetadata.Value.NumArgs == 0)
                    {
                        // legacy support for arg0, arg1, argd8 commands
                        if (cmdMetadata.Value.NumArgs == 1)
                        {
                            switch (cmdMetadata.Value.OrderedArgTypes?.FirstOrDefault())
                            {
                                case AutomationActionArgType.Integer:
                                    a.DataI = Convert.ToInt32((long)parsedParams.data.First());
                                    break;
                                case AutomationActionArgType.String:
                                    a.DataS = (string)parsedParams.data.First();
                                    break;
                            }
                        }
                        else if (cmdMetadata.Value.NumArgs == 8 && cmdMetadata.Value.Action == AutomationActions.PlacePIP)
                        {
                            PIPPlaceSettings placement = new PIPPlaceSettings()
                            {
                                PosX = (double)parsedParams.data[0],
                                PosY = (double)parsedParams.data[1],
                                ScaleX = (double)parsedParams.data[2],
                                ScaleY = (double)parsedParams.data[3],
                                MaskLeft = (double)parsedParams.data[4],
                                MaskRight = (double)parsedParams.data[5],
                                MaskTop = (double)parsedParams.data[6],
                                MaskBottom = (double)parsedParams.data[7],
                            };
                            a.DataO = placement;
                        }
                        else if (cmdMetadata.Value.NumArgs == 7 && cmdMetadata.Value.Action == AutomationActions.ConfigurePATTERN)
                        {
                            BMDUSKPATTERNSettings pattern = new BMDUSKPATTERNSettings()
                            {
                                PatternType = (string)parsedParams.data[0],
                                Inverted = (bool)parsedParams.data[1],
                                Size = (double)parsedParams.data[2],
                                Symmetry = (double)parsedParams.data[3],
                                Softness = (double)parsedParams.data[4],
                                XOffset = (double)parsedParams.data[5],
                                YOffset = (double)parsedParams.data[6],
                                DefaultFillSource = 0,
                            };
                        }

                        // otherwise just stuff the args directly into the args
                        // new commands will look there for thier values as required
                        a.RawParams = parsedParams.data;
                        a.Action = cmdMetadata.Value.Action;
                    }
                }
            }

            return a;
        }

        static (bool success, List<object> data) ParseDynamicParams(string pstr, AutomationActionMetadata cmdMetadata)
        {
            var res = new List<object>();

            // expect all args to be un-enclosed (even strings!)
            // all args are comma seperated

            var pargs = pstr.Split(",");

            if (pargs.Length != cmdMetadata.NumArgs)
            {
                return (false, res);
            }

            for (int i = 0; i < pargs.Length; i++)
            {
                // try parsing the value as requested
                switch (cmdMetadata.OrderedArgTypes[i])
                {
                    case AutomationActionArgType.Integer:
                        // make life easy here and use more memory for gauranteed type safety
                        // 64 bits is certianly better than 32 bits
                        if (long.TryParse(pargs[i].Trim(), out var ival))
                        {
                            res.Add(ival);
                        }
                        break;
                    case AutomationActionArgType.String:
                        res.Add(pargs[i].Trim());
                        break;
                    case AutomationActionArgType.Double:
                        if (double.TryParse(pargs[i].Trim(), out var dval))
                        {
                            res.Add(dval);
                        }
                        break;
                    case AutomationActionArgType.Boolean:
                        if (bool.TryParse(pargs[i].Trim(), out var bval))
                        {
                            res.Add(bval);
                        }
                        break;
                    default:
                        return (false, res);
                }
            }

            return (true, res);
        }

    }





}
