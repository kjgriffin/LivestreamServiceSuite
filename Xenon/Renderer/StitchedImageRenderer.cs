using OpenQA.Selenium.Internal;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using Xenon.Compiler;
using Xenon.Helpers;
using Xenon.LayoutEngine;
using Xenon.LayoutInfo;
using Xenon.Renderer.Helpers.ImageSharp;
using Xenon.SlideAssembly;

namespace Xenon.Renderer
{
    class StitchedImageRenderer : ISlideRenderer<StitchedImageSlideLayoutInfo>, ISlideRenderer, ISlideLayoutPrototypePreviewer<ASlideLayoutInfo>
    {

        public static string DATAKEY_IMAGES { get => "ordered-images"; }
        public static string DATAKEY_BOX_ASSIGNED_IMAGES { get => "box-assigned-ordered-images"; }
        public static string DATAKEY_TITLE { get => "title"; }
        public static string DATAKEY_NAME { get => "name"; }
        public static string DATAKEY_NUMBER { get => "number"; }
        public static string DATAKEY_COPYRIGHT { get => "copyright"; }
        public static string DATAKEY_COPYRIGHT_TEXT { get => "copyright-text"; }
        public static string DATAKEY_COPYRIGHT_TUNE { get => "copyright-tune"; }

        public RenderedSlide RenderSlide(Slide slide, List<Compiler.XenonCompilerMessage> messages, IAssetResolver projassets, StitchedImageSlideLayoutInfo layout)
        {
            RenderedSlide res = new RenderedSlide();
            res.MediaType = MediaType.Image;
            res.AssetPath = "";
            res.RenderedAs = string.IsNullOrWhiteSpace(layout?.SlideType) ? "Full" : layout.SlideType;

            CommonSlideRenderer.Render(out var ibmp, out var ikbmp, layout);

            if (layout.Shapes != null)
            {
                foreach (var shape in layout.Shapes)
                {
                    CommonPolygonRenderer.Render(ibmp, ikbmp, shape);
                }
            }

            //List<(LSBImageResource, int)> imageresources = (List<(LSBImageResource, int)>)slide.Data.GetOrDefault(DATAKEY_IMAGES, new List<(LSBImageResource, int)>());
            string title = (string)slide.Data.GetOrDefault(DATAKEY_TITLE, "");
            string hymnname = (string)slide.Data.GetOrDefault(DATAKEY_NAME, "");
            string number = (string)slide.Data.GetOrDefault(DATAKEY_NUMBER, "");


            List<(LSBImageResource img, int bid)> allimageresources = (List<(LSBImageResource, int)>)slide.Data.GetOrDefault(DATAKEY_BOX_ASSIGNED_IMAGES, new List<(LSBImageResource img, int bid)>());

            // handle each drawing box
            int b = 0;
            foreach (var musicbox in layout.AllMusicBoxes)
            {
                List<LSBImageResource> imageresources = allimageresources.Where(x => x.bid == b).Select(x => x.img).ToList();

                if (!imageresources.Any())
                {
                    continue;
                }

                int width = imageresources.Select(i => i.Size.Width).Max();
                int height = imageresources.Select(i => i.Size.Height).Aggregate((a, b) => a + b);

                Image<Bgra32> hibmp = new Image<Bgra32>(width, height);

                // draw all images
                int hoff = 0;
                foreach (var image in imageresources)
                {
                    // load image
                    try
                    {
                        Image<Bgra32> src = Image.Load<Bgra32>(projassets.GetProjectAssetByName(image.AssetRef).CurrentPath);
                        //Bitmap b = new Bitmap(projassets.GetProjectAssetByName(image.AssetRef).CurrentPath);
                        int x = (width - src.Width) / 2;
                        //g.DrawImage(b, x, hoff, b.Width, b.Height);
                        hibmp.Mutate(ctx =>
                        {
                            ctx.DrawImage(src, new Point(x, hoff), 1f);
                        });

                        hoff += src.Height;
                    }
                    catch (Exception ex)
                    {
                        messages.Add(new Compiler.XenonCompilerMessage() { ErrorMessage = "Error loading image", ErrorName = "Error Loading Asset", Generator = "StitchedImageRenderer", Inner = ex.Message, Level = Compiler.XenonCompilerMessageType.Error, Token = image.AssetRef });
                        return null;
                    }
                }

                /* we'll now let this logic be done generically by adv drawing box */

                /*
                // we'll manualy scale and center the hymn to its final size
                hibmp.Mutate(ctx => ctx.Resize(new ResizeOptions { Mode = ResizeMode.Max, Size = musicbox.Box.Size.Size }));
                // draw it back onto a full'sized blank
                Image<Bgra32> hibmpctr = new Image<Bgra32>(musicbox.Box.Size.Width, musicbox.Box.Size.Height);
                int xoff = Math.Clamp(Math.Abs((hibmp.Width - musicbox.Box.Size.Width)) / 2, 0, musicbox.Box.Size.Width);
                int yoff = Math.Clamp(Math.Abs((hibmp.Height - musicbox.Box.Size.Height)) / 2, 0, musicbox.Box.Size.Height);
                hibmpctr.Mutate(ctx => ctx.DrawImage(hibmp, new Point(xoff, yoff), 1f));
                // then let the render just position it inside the box
                */


                // invert??
                if (musicbox.InvertAll)
                {
                    //hibmpctr.Mutate(ctx => ctx.Invert());
                    hibmp.Mutate(ctx => ctx.Invert());
                }

                //CommonDrawingBoxRenderer.Render(ibmp, ikbmp, musicbox, hibmpctr);
                CommonDrawingBoxRenderer.Render(ibmp, ikbmp, musicbox, hibmp);

                b++;
            }

            // draw titles
            CommonTextBoxRenderer.Render(ibmp, ikbmp, layout.NameBox, hymnname);
            CommonTextBoxRenderer.Render(ibmp, ikbmp, layout.TitleBox, title);
            CommonTextBoxRenderer.Render(ibmp, ikbmp, layout.NumberBox, number);

            // draw copyright
            if (slide.Data.ContainsKey(DATAKEY_COPYRIGHT))
            {
                string copyrighttune = (string)slide.Data.GetOrDefault(DATAKEY_COPYRIGHT_TUNE, "");
                string copyrighttext = (string)slide.Data.GetOrDefault(DATAKEY_COPYRIGHT_TEXT, "");
                // change behaviour- let the textbox renderer just render out 2 lines
                string copyright = string.Join(Environment.NewLine, copyrighttune, copyrighttext);
                CommonTextBoxRenderer.Render(ibmp, ikbmp, layout.CopyrightBox, copyright);
            }
            else
            {
                string copyright = (string)slide.Data.GetOrDefault(DATAKEY_COPYRIGHT, "");
                CommonTextBoxRenderer.Render(ibmp, ikbmp, layout.CopyrightBox, copyright);
            }

            res.Bitmap = ibmp;
            res.KeyBitmap = ikbmp;
            return res;
        }

        public Task<RenderedSlide> VisitSlideForRendering(Slide slide, IAssetResolver assetResolver, ISlideRendertimeInfoProvider info, List<XenonCompilerMessage> Messages, RenderedSlide result)
        {
            if (slide.Format == SlideFormat.StitchedImage)
            {
                StitchedImageSlideLayoutInfo layout = (this as ISlideRenderer<StitchedImageSlideLayoutInfo>).LayoutResolver.GetLayoutInfo(slide);
                var render = RenderSlide(slide, Messages, assetResolver, layout);
                return Task.FromResult(render);
            }
            return Task.FromResult(result);
        }

        public Task<(Image<Bgra32> main, Image<Bgra32> key)> GetPreviewForLayout(string layoutInfo)
        {
            StitchedImageSlideLayoutInfo layout = JsonSerializer.Deserialize<StitchedImageSlideLayoutInfo>(layoutInfo);

            CommonSlideRenderer.Render(out var ibmp, out var ikbmp, layout);

            if (layout.Shapes != null)
            {
                foreach (var shape in layout.Shapes)
                {
                    CommonPolygonRenderer.Render(ibmp, ikbmp, shape);
                }
            }

            foreach (var musicbox in layout.AllMusicBoxes)
            {
                CommonDrawingBoxRenderer.RenderLayoutPreview(ibmp, ikbmp, musicbox);
            }

            CommonTextBoxRenderer.RenderLayoutPreview(ibmp, ikbmp, layout.TitleBox);
            CommonTextBoxRenderer.RenderLayoutPreview(ibmp, ikbmp, layout.NameBox);
            CommonTextBoxRenderer.RenderLayoutPreview(ibmp, ikbmp, layout.NumberBox);
            CommonTextBoxRenderer.RenderLayoutPreview(ibmp, ikbmp, layout.CopyrightBox);

            return Task.FromResult((ibmp, ikbmp));
        }

        public bool IsValidLayoutJson(string json)
        {
            return ISlideLayoutPrototypePreviewer<StitchedImageSlideLayoutInfo>._InternalDefaultIsValidLayoutJson(json);
        }

        ILayoutInfoResolver<StitchedImageSlideLayoutInfo> ISlideRenderer<StitchedImageSlideLayoutInfo>.LayoutResolver { get => new StitchedImageSlideLayoutInfo(); }
    }
}
