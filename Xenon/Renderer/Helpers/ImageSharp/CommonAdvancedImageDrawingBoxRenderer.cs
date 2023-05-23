using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using Xenon.Helpers;
using Xenon.LayoutInfo.BaseTypes;

namespace Xenon.Renderer.Helpers.ImageSharp
{
    internal static class CommonAdvancedImageDrawingBoxRenderer
    {
        internal static void RenderLayoutPreview(Image<Bgra32> ibmp, Image<Bgra32> ikbmp, AdvancedDrawingBoxLayout layout)
        {
            CommonDrawingBoxRenderer.RenderLayoutPreview(ibmp, ikbmp, layout);
        }

        internal static void Render(Image<Bgra32> ibmp, Image<Bgra32> ikbmp, AdvancedDrawingBoxLayout image, Image<Bgra32> src)
        {

            if (image.ForceBasic)
            {
                CommonDrawingBoxRenderer.Render(ibmp, ikbmp, image, src);
                return;
            }

            // we're going to destroy/mutilate the pixel values if we color-replace/crop
            // work on a copy since we'll still need the original to start again on the key

            // put down the background
            //CommonDrawingBoxRenderer.Render(ibmp, ikbmp, image);

            // Ok- we'll draw it on a white background to fix alpha issues
            // then this might work
            Image<Bgra32> srcpy = new Image<Bgra32>(src.Width, src.Height, new Bgra32(255, 255, 255, 255));
            srcpy.Mutate(ctx => ctx.DrawImage(src, 1));

            if (image.AutoCrop)
            {
                srcpy = srcpy.Crop((byte)image.CropExclude.Red, (byte)image.CropExclude.Green, (byte)image.CropExclude.Blue, (byte)image.CropExclude.Alpha, image.CropExcludeAlpha);
            }
            else
            {
                srcpy = src.Clone();
            }

            if (image.InvertColors)
            {
                srcpy.Mutate(ctx => ctx.Invert());
            }

            // do alpha replacement
            for (int y = 0; y < srcpy.Height; y++)
            {
                for (int x = 0; x < srcpy.Width; x++)
                {
                    var pix = srcpy[x, y];
                    if (image.AlphaReplace)
                    {
                        if (pix.R == 0 && pix.G == 0 && pix.B == 0)
                        {
                            srcpy[x, y] = new Bgra32(pix.R, pix.G, pix.B, 0);
                        }
                    }
                    if (image.AlphaToGrey)
                    {
                        if (pix.A < 255)
                        {
                            srcpy[x, y] = new Bgra32(pix.A, pix.A, pix.A, 255);
                        }
                    }
                }
            }

            // manually draw everything

            // background
            CommonDrawingBoxRenderer.Render(ibmp, ikbmp, image);

            // draw on image directly
            ResizeOptions ropts = new ResizeOptions()
            {
                Mode = ResizeMode.Max,
                Size = image.Box.Rectangle.Size,

            };
            // I don't think we want modify the source image- we're not sure we actually own it here
            srcpy.Mutate(ctx => ctx.Resize(ropts));

            Point coffset = new Point((image.Box.Size.Width - srcpy.Width) / 2 + image.Box.Origin.Point.X, (image.Box.Size.Height - srcpy.Height) / 2 + image.Box.Origin.Point.Y);

            ibmp.Mutate_OverlayImage(srcpy, coffset);

            // draw key on directly
            if (!image.ForceSolidKey)
            {
                ikbmp.Mutate_OverlayImage(srcpy, coffset);
            }
        }



    }
}
