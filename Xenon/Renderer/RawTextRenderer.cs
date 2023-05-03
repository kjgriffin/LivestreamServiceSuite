using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xenon.Compiler;
using Xenon.SlideAssembly;

namespace Xenon.Renderer
{
    internal class RawTextRenderer : ISlideRenderer
    {

        public static string DATAKEY_KEYNAME = "keyname";
        public static string DATAKEY_RAWTEXT = "rawtext";

        public void VisitSlideForRendering(Slide slide, IAssetResolver assetResolver, List<XenonCompilerMessage> Messages, ref RenderedSlide result)
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

                if (slide.Data.TryGetValue(DATAKEY_RAWTEXT, out var text))
                {
                    result.Text = (string)text;
                }
            }
        }
    }
}
