using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Xenon.Compiler;
using Xenon.SlideAssembly;

namespace Xenon.Renderer
{
    internal class CommonTextContentSlideVariableReplacer
    {
        public static string ReplaceVariablesInText(string input, ISlideRendertimeInfoProvider info)
        {
            // think this can be limited to single pass replacement
            // i.e. don't need recursive replacements

            var matches = Regex.Matches(input, @"%slide\.num\.(.*)\.\d+%");
            foreach (Match match in matches)
            {
                int i = info.FindSlideNumber(match.Value);
                input = input.Replace(match.Value, i.ToString());
            }
            //while (matches.Any())
            //{
            //    // replace it
            //    int i = info.FindSlideNumber(matches[0].Value);
            //    input = Regex.Replace(input, @"%slide\.num\.(.*)\.\d+%", i.ToString());

            //    matches = Regex.Matches(input, @"%slide\.num\.(.*)\.\d+%");
            //}

            // should we also allow camera matches??
            matches = Regex.Matches(input, @"%cam\.(?<cam>\w+)%");
            foreach (Match match in matches)
            {
                int i = info.FindCameraID(match.Groups["cam"].Value.ToLower());
                input = input.Replace(match.Value, i.ToString());
            }


            return input;
        }
    }

    internal class RawTextRenderer : ISlideRenderer
    {

        public static string DATAKEY_KEYNAME = "keyname";
        public static string DATAKEY_RAWTEXT_TARGET = "rawtext";

        public void VisitSlideForRendering(Slide slide, IAssetResolver assetResolver, ISlideRendertimeInfoProvider info, List<XenonCompilerMessage> Messages, ref RenderedSlide result)
        {
            if (slide.Format == SlideFormat.RawTextFile)
            {
                result = new RenderedSlide();
                result.MediaType = MediaType.Empty;
                result.RenderedAs = "RawText";
                if (slide.Data.TryGetValue(DATAKEY_KEYNAME, out object name))
                {
                    result.Name = (string)name;
                }
                else
                {
                    Messages.Add(new Compiler.XenonCompilerMessage() { ErrorMessage = $"Resource name was not specified. Will use original file name {result.Name}, but this may not produce the expected result.", ErrorName = "Resource Name Mismatch", Generator = "CopySlideRenderer", Inner = "", Level = Compiler.XenonCompilerMessageType.Warning, Token = ("", int.MaxValue) });
                }
                result.CopyExtension = ".txt";

                if (slide.Data.TryGetValue(DATAKEY_RAWTEXT_TARGET, out var text))
                {
                    //result.Text = CommonTextContentSlideVariableReplacer.ReplaceVariablesInText((string)text, info);
                    result.Text = (string)text;
                }
            }
        }
    }
}
