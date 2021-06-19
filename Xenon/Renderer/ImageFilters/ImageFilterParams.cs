using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Xenon.Renderer.ImageFilters
{
    public class ImageFilterParams
    {
    }

    public class CenterAssetFillFilterParams : ImageFilterParams
    {
        /// <summary>
        /// Path to image asset.
        /// </summary>
        public string AssetPath { get; set; }
    }

    public class SolidColorCanvasFilterParams : ImageFilterParams
    {
        /// <summary>
        /// Width of canvas.
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// Height of canvas.
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// Color to fill canvas.
        /// </summary>
        public Color Background { get; set; }
        /// <summary>
        /// Color to fill key canvas.
        /// </summary>
        public Color KBackground { get; set; }
    }

    public class CropFilterParams : ImageFilterParams
    {

        public enum CropBound
        {
            Top,
            Bottom,
            Left,
            Right,
        }

        /// <summary>
        /// Bound to crop. ("top", "left", "bottom", "right")
        /// </summary>
        public CropBound Bound { get; set; }
        /// <summary>
        /// The bound detection mode. If false detects bounds based on closest pixel matching Identifier (within tolerance). If true detects bounds based on first pixel that does not match Identifier (outside tolerance).
        /// </summary>
        public bool IsExcludeMatch { get; set; }
        /// <summary>
        /// Color that identifies image bound.
        /// </summary>
        public Color Identifier { get; set; }
        /// <summary>
        /// RGB-R value tolerance. Alloable tolerance between expected Identifier R value, and image's pixel R value for bounds detection.
        /// </summary>
        public int RTolerance { get; set; }
        /// <summary>
        /// RGB-G value tolerance. Alloable tolerance between expected Identifier G value, and image's pixel G value for bounds detection.
        /// </summary>
        public int GTolerance { get; set; }
        /// <summary>
        /// RGB-B value tolerance. Alloable tolerance between expected Identifier B value, and image's pixel B value for bounds detection.
        /// </summary>
        public int BTolerance { get; set; }
    }

    public class ColorEditFilterParams : ImageFilterParams
    {
        /// <summary>
        /// Color to match for applying color edit to.
        /// </summary>
        public Color Identifier { get; set; }
        /// <summary>
        /// Color to replace editied pixels with.
        /// </summary>
        public Color Replace { get; set; }
        /// <summary>
        /// If True will instead replace all pixels that don't match identifier within tolerance.
        /// </summary>
        public bool IsExcludeMatch { get; set; }
        /// <summary>
        /// True if should be applied on key instead of slide.
        /// </summary>
        public bool ForKey { get; set; }
        /// <summary>
        /// R tolerance.
        /// </summary>
        public int RTolerance { get; set; }
        /// <summary>
        /// G tolerance.
        /// </summary>
        public int GTolerance { get; set; }
        /// <summary>
        /// B tolerance.
        /// </summary>
        public int BTolerance { get; set; }
        /// <summary>
        /// Alpha tolerance.
        /// </summary>
        public int ATolerance { get; set; } = 0;
        /// <summary>
        /// If true alpha will also be compared.
        /// </summary>
        public bool CheckAlpha { get; set; } = false;

    }
        
    public class UniformStretchFilterParams : ImageFilterParams
    {
        /// <summary>
        /// Target Width.
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// Target Height.
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// Color to fill extra space.
        /// </summary>
        public Color Fill { get; set; }
        /// <summary>
        /// Color to fill key extra space.
        /// </summary>
        public Color KFill { get; set; }
    }

    public class CenterOnBackgroundFilterParams : ImageFilterParams
    {
        /// <summary>
        /// Background width.
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// Background height.
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// Background color.
        /// </summary>
        public Color Fill { get; set; }
        /// <summary>
        /// Background key color.
        /// </summary>
        public Color KFill { get; set; }
    }


}
