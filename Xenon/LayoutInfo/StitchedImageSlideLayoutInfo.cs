using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Xenon.LayoutInfo.BaseTypes;
using Xenon.LayoutInfo.Serialization;
using Xenon.Renderer;
using Xenon.SlideAssembly;

namespace Xenon.LayoutInfo
{
    internal class StitchedImageSlideLayoutInfo : ASlideLayoutInfo, ILayoutInfoResolver<StitchedImageSlideLayoutInfo>
    {
        public TextboxLayout TitleBox { get; set; }
        public TextboxLayout NameBox { get; set; }
        public TextboxLayout NumberBox { get; set; }
        public TextboxLayout CopyrightBox { get; set; }
        public bool AutoBoxSplitOnRefrain { get; set; } = false;
        [Obsolete]
        public DrawingBoxLayout MusicBox { get; set; } = null; // keep this so that we can parse old librarires. NOT for use
        /// <summary>
        /// DO NOT USE DIRECTLY! THIS IS TO SIMPLIFY JSON PARSING. USE <see cref=""/> for access instead
        /// </summary>
        public List<DrawingBoxLayout> MusicBoxes { get; set; }

        [JsonIgnore]
        public List<DrawingBoxLayout> AllMusicBoxes
        {
            get
            {
                if (MusicBoxes != null && MusicBoxes.Count > 0)
                {
                    return MusicBoxes;
                }
                return new List<DrawingBoxLayout> {
#pragma warning disable CS0612 // Type or member is obsolete
                    MusicBox 
#pragma warning restore CS0612 // Type or member is obsolete
                };
            }
        }


        public List<LWJPolygon> Shapes { get; set; }

        public override string GetDefaultJson()
        {
            return JsonSerializer.Serialize(_Internal_GetDefaultInfo(), new JsonSerializerOptions() { WriteIndented = true });
        }

        public StitchedImageSlideLayoutInfo GetLayoutInfo(Slide slide)
        {
            return ILayoutInfoResolver<StitchedImageSlideLayoutInfo>._InternalDefault_GetLayoutInfo(slide);
        }

        public StitchedImageSlideLayoutInfo _Internal_GetDefaultInfo(string overrideDefault = "")
        {
            return ILayoutInfoResolver<StitchedImageSlideLayoutInfo>.GetDefaultInfo(overrideDefault);
        }
    }
}
