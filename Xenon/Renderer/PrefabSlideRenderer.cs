using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

using Xenon.Compiler;
using Xenon.Compiler.AST;
using Xenon.Helpers;
using Xenon.SlideAssembly;

namespace Xenon.Renderer
{
    class PrefabSlideRenderer : ISlideRenderer
    {
        public SlideLayout Layouts { get; set; }

        public RenderedSlide RenderSlide(Slide slide, List<XenonCompilerMessage> messages, ISlideRendertimeInfoProvider info)
        {
            RenderedSlide res = new RenderedSlide();
            res.AssetPath = "";
            res.MediaType = MediaType.Image;
            res.RenderedAs = "Full"; // I think we can leave this just fine...

            Bitmap bmp = new Bitmap(Layouts?.PrefabLayout.Size.Width ?? 1920, Layouts?.PrefabLayout.Size.Height ?? 1080);
            Bitmap kbmp = new Bitmap(Layouts?.PrefabLayout.Size.Width ?? 1920, Layouts?.PrefabLayout.Size.Height ?? 1080);
            Graphics gfx = Graphics.FromImage(bmp);
            Graphics kgfx = Graphics.FromImage(kbmp);

            gfx.Clear(Color.White);
            kgfx.Clear(Color.White);

            res.KeyBitmap = kbmp.ToImageSharpImage<SixLabors.ImageSharp.PixelFormats.Bgra32>();


            Bitmap src = null;

            try
            {

                switch (slide.Data["prefabtype"])
                {
                    case PrefabSlides.Copyright:
                        res.RenderedAs = "Full-startrecord";
                        src = ProjectResources.PrefabSlides.CopyrightLicense;
                        break;
                    case PrefabSlides.ViewServices:
                        src = ProjectResources.PrefabSlides.ViewServices;
                        break;
                    case PrefabSlides.ViewSeries:
                        src = ProjectResources.PrefabSlides.ViewSessions;
                        break;
                    case PrefabSlides.ApostlesCreed:
                        switch (slide.Data["layoutnum"])
                        {
                            case 1:
                                src = ProjectResources.PrefabSlides.ApostlesCreed1;
                                break;
                            case 2:
                                src = ProjectResources.PrefabSlides.ApostlesCreed2;
                                break;
                            case 3:
                                src = ProjectResources.PrefabSlides.ApostlesCreed3;
                                break;
                        }
                        break;
                    case PrefabSlides.NiceneCreed:
                        switch (slide.Data["layoutnum"])
                        {
                            case 1:
                                src = ProjectResources.PrefabSlides.NiceneCreed1;
                                break;
                            case 2:
                                src = ProjectResources.PrefabSlides.NiceneCreed2;
                                break;
                            case 3:
                                src = ProjectResources.PrefabSlides.NiceneCreed3;
                                break;
                            case 4:
                                src = ProjectResources.PrefabSlides.NiceneCreed4;
                                break;
                            case 5:
                                src = ProjectResources.PrefabSlides.NiceneCreed5;
                                break;
                        }
                        break;
                    case PrefabSlides.LordsPrayer:
                        src = ProjectResources.PrefabSlides.LordsPrayer;
                        break;
                    // special case for prefab scripts
                    case PrefabSlides.Script_LiturgyOff:
                    case PrefabSlides.Script_OrganIntro:
                        ScriptRenderer sr = new ScriptRenderer();
                        return sr.RenderSlide(slide, messages, info);
                }

            }
            catch (Exception)
            {
                res.Bitmap = bmp.ToImageSharpImage<SixLabors.ImageSharp.PixelFormats.Bgra32>();
                string tmp = "KEY ERROR on data attribute";
                object a;
                slide.Data.TryGetValue("prefabtype", out a);
                if (a is PrefabSlides && a != null)
                {
                    tmp = ((PrefabSlides)a).Convert();
                }
                messages.Add(new XenonCompilerMessage() { Level = XenonCompilerMessageType.Error, ErrorMessage = $"Requested prefab image not loaded. While rendering slide {slide.Number}", ErrorName = "Prefab not found", Token = tmp });
                throw;
            }

            if (src != null)
            {
                gfx.DrawImage(src, new Rectangle(new Point(0, 0), Layouts.PrefabLayout.Size));
            }

            res.Bitmap = bmp.ToImageSharpImage<SixLabors.ImageSharp.PixelFormats.Bgra32>();
            return res;
        }

        public Task<RenderedSlide> VisitSlideForRendering(Slide slide, IAssetResolver assetResolver, ISlideRendertimeInfoProvider info, List<XenonCompilerMessage> Messages, RenderedSlide operand)
        {
            if (slide.Format == SlideFormat.Prefab)
            {
                var render = RenderSlide(slide, Messages, info);
                return Task.FromResult(render);
            }
            return Task.FromResult(operand);
        }
    }
}
