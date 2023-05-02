using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using System;

using Xenon.Helpers;
using Xenon.LayoutInfo.BaseTypes;

namespace Xenon.Renderer.Helpers.ImageSharp
{
    internal static class CommonDrawingBoxRenderer
    {

        public static void Render(Image<Bgra32> ibmp, Image<Bgra32> ikbmp, DrawingBoxLayout layout)
        {
            RenderBackground(ibmp, ikbmp, layout);
        }

        public static void Render(Image<Bgra32> ibmp, Image<Bgra32> ikbmp, DrawingBoxLayout layout, Image<Bgra32> src, ResizeMode resizeMode = ResizeMode.Max, bool inferKey = true)
        {
            RenderBackground(ibmp, ikbmp, layout);

            // draw src
            ResizeOptions ropts = new ResizeOptions()
            {
                Mode = resizeMode,
                Size = layout.Box.Rectangle.Size
            };
            // I don't think we want modify the source image- we're not sure we actually own it here

            // save original size for figuring out what to do later ??
            SizeF rscalefactor = new SizeF(layout.Box.Size.Width - src.Width, layout.Box.Size.Height - src.Height);

            var scpy = src.Clone();
            scpy.Mutate(ctx => ctx.Resize(ropts));

            // apply image processing here


            //ibmp.Mutate(ctx => ctx.DrawImage(scpy, layout.Box.Origin.Point, PixelColorBlendingMode.Normal, PixelAlphaCompositionMode.Clear, 1f));

            // compute location based on v/h align
            Point imgorig = layout.Box.Origin.Point;
            switch (layout.HorizontalAlignment)
            {
                case LayoutInfo.LWJHAlign.Center:
                case LayoutInfo.LWJHAlign.Centered:
                    imgorig.X += (layout.Box.Size.Width - scpy.Width) / 2;
                    break;
                case LayoutInfo.LWJHAlign.Right:
                    imgorig.X += (layout.Box.Size.Width - scpy.Width);
                    break;
            }
            switch (layout.VerticalAlignment)
            {
                case LayoutInfo.LWJVAlign.Center:
                    imgorig.Y += (layout.Box.Size.Height - scpy.Height) / 2;
                    break;
                case LayoutInfo.LWJVAlign.Bottom:
                    imgorig.Y += (layout.Box.Size.Height - scpy.Height);
                    break;
            }

            if (layout?.OverrideKeyImg == true)
            {
                ibmp.Mutate(ctx => ctx.DrawImage(scpy, imgorig, 1f));
            }
            else
            {
                ibmp.Mutate_OverlayImage(scpy, imgorig);
            }

            // if transparent then create key from image based on alpha channel
            if (inferKey && layout.KeyColor.Alpha == 0)
            {
                // generate key image
                // we're going to mutilate the image here. probably good we did make a full copy
                // there's probably a nifty way to just pull the alpha channel and merge that somehow, but for now:
                // we'll do this
                int XBOUND = scpy.Width;
                int YBOUND = scpy.Height;
                for (int x = 0; x < XBOUND; x++)
                {
                    for (int y = 0; y < YBOUND; y++)
                    {
                        // using the alpha channel, turn it into a greyscale fully opaque
                        byte alpha = scpy[x, y].A;
                        scpy[x, y] = new Bgra32(alpha, alpha, alpha, 255);
                    }
                }
                ikbmp.Mutate(ctx => ctx.DrawImage(scpy, imgorig, 1f));
            }
            if (layout.OverrideKeyImg)
            {

                ropts = new ResizeOptions()
                {
                    Mode = resizeMode,
                    Size = layout.KeyBoxOverride.Rectangle.Size
                };
                // I don't think we want modify the source image- we're not sure we actually own it here

                // save original size for figuring out what to do later ??
                rscalefactor = new SizeF(layout.KeyBoxOverride.Size.Width - src.Width, layout.KeyBoxOverride.Size.Height - src.Height);

                var kscpy = src.Clone();
                kscpy.Mutate(ctx => ctx.Resize(ropts));

                // apply image processing here


                //ibmp.Mutate(ctx => ctx.DrawImage(scpy, layout.Box.Origin.Point, PixelColorBlendingMode.Normal, PixelAlphaCompositionMode.Clear, 1f));

                // compute location based on v/h align
                Point kimgorig = layout.KeyBoxOverride.Origin.Point;
                switch (layout.KeyHorizontalAlignmentOverride)
                {
                    case LayoutInfo.LWJHAlign.Center:
                    case LayoutInfo.LWJHAlign.Centered:
                        kimgorig.X += (layout.KeyBoxOverride.Size.Width - kscpy.Width) / 2;
                        break;
                    case LayoutInfo.LWJHAlign.Right:
                        kimgorig.X += (layout.KeyBoxOverride.Size.Width - kscpy.Width);
                        break;
                }
                switch (layout.KeyVerticalAlignmentOverride)
                {
                    case LayoutInfo.LWJVAlign.Center:
                        kimgorig.Y += (layout.KeyBoxOverride.Size.Height - kscpy.Height) / 2;
                        break;
                    case LayoutInfo.LWJVAlign.Bottom:
                        kimgorig.Y += (layout.KeyBoxOverride.Size.Height - kscpy.Height);
                        break;
                }

                ikbmp.Mutate(ctx => ctx.DrawImage(kscpy, kimgorig, 1f));
            }

        }

        internal static void Mutate_OverlayImage(this Image<Bgra32> ibmp, Image<Bgra32> ioverlay, Point location)
        {
            int XBOUND = ioverlay.Width;
            int YBOUND = ioverlay.Height;
            for (int x = 0; x < XBOUND; x++)
            {
                for (int y = 0; y < YBOUND; y++)
                {
                    int xx = Math.Clamp(x + location.X, 0, ibmp.Width - 1);
                    int yy = Math.Clamp(y + location.Y, 0, ibmp.Height - 1);
                    // for now won't handle semi-transparent...
                    // this may not work...

                    // I think we can ignore the alpha channel
                    // its' expected this is used where premultiplication with an explicit key image will overwrite that

                    // need to blend RGB channels based on overlay's alpha value...

                    // - alpha = 0, keep pixel from source
                    // - alpha = 1, pixel totaly from overlay


                    // for now we'll be naieve and use the alpha as a linear blend factor
                    // (may need some sort of non-linear blend factor) TODO: check out gamma blending

                    byte Blend(byte dest, byte src, float alpha)
                    {
                        return (byte)(((1 - alpha) * dest) + (src * alpha));
                    }

                    var dpix = ibmp[xx, yy];
                    var spix = ioverlay[x, y];

                    float ascale = spix.A / 255f;

                    Bgra32 pix = new Bgra32(Blend(dpix.R, spix.R, ascale), Blend(dpix.G, spix.G, ascale), Blend(dpix.B, spix.B, ascale), dpix.A);
                    ibmp[xx, yy] = pix;
                }
            }
        }

        private static void RenderBackground(Image<Bgra32> ibmp, Image<Bgra32> ikbmp, DrawingBoxLayout layout)
        {
            DrawingOptions dopts = new DrawingOptions()
            {
                GraphicsOptions = new GraphicsOptions()
                {
                    ColorBlendingMode = PixelColorBlendingMode.Normal,
                    //AlphaCompositionMode = PixelAlphaCompositionMode.DestAtop,
                },
            };
            ibmp.Mutate(ctx => ctx.Fill(dopts, layout.FillColor.ToColor(), layout.Box.RectangleF));
            //ibmp.Mutate(ctx => ctx.Fill(layout.FillColor.ToColor(), layout.Box.RectangleF));
            //ikbmp.Mutate(ctx => ctx.Fill(layout.FillColor.ToColor(), layout.Box.RectangleF));
            ikbmp.Mutate(ctx => ctx.Fill(dopts, layout.KeyColor.ToColor(), layout.Box.RectangleF));
        }

        public static void RenderLayoutPreview(Image<Bgra32> ibmp, Image<Bgra32> ikbmp, DrawingBoxLayout layout)
        {
            Image<Bgra32> previewIcon = ProjectResources.Icons.ImageIcon.ToImageSharpImage<Bgra32>();

            Render(ibmp, ikbmp, layout, previewIcon, ResizeMode.Max, inferKey: true);
        }
    }
}
