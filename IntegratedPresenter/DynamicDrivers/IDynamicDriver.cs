using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Integrated_Presenter.DynamicDrivers
{
    internal interface IDynamicDriver
    {
        static string ParseConfigID(string rawText)
        {
            var lines = rawText.Split(Environment.NewLine);
            if (lines.FirstOrDefault().StartsWith("dynamic"))
            {
                var match = Regex.Match(lines.FirstOrDefault(), "dynamic:(?<module>.*);");
                if (match.Success)
                {
                    return match.Groups["module"].Value;
                }
            }
            return string.Empty;
        }

        bool SupportsConfig(string configID);
        void ConfigureControls(string rawText, string resourceFolder, bool overwriteAll);
        void ClearControls();
    }

}
