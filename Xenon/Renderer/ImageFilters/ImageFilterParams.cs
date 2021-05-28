using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Xenon.Renderer.ImageFilters
{
    class ImageFilterParams
    {
    }

    class SolidColorCanvasFilterParams: ImageFilterParams
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

    class CropFilterParams : ImageFilterParams
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

    class UniformStretchFilterParams : ImageFilterParams
    {
        /// <summary>
        /// Target Width.
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// Target Height.
        /// </summary>
        public int Height { get; set; }
    }

}
