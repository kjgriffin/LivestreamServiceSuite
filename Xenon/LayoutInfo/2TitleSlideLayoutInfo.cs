using System.Text.Json;
using System.Text.Json.Serialization;

using Xenon.LayoutInfo.BaseTypes;
using Xenon.Renderer;
using Xenon.SlideAssembly;

namespace Xenon.LayoutInfo
{
    internal class _2TitleSlideLayoutInfo : ASlideLayoutInfo, ILayoutInfoResolver<_2TitleSlideLayoutInfo>
    {
        public TextboxLayout MainText { get; set; }
        public TextboxLayout SubText { get; set; }
        public DrawingBoxLayout Banner { get; set; }

        // Don't really like this, but may be the way to keep legacy Xenon somewhat supported
        [JsonIgnore]
        internal TextboxLayout _LegacyHorizontalAlignment_MainText
        {
            get => new TextboxLayout()
            {
                BColor = new LWJColor(System.Drawing.Color.Transparent),
                KColor = new LWJColor(System.Drawing.Color.Transparent),
                FColor = new LWJColor(System.Drawing.Color.White),
                Font = new LWJFont() { Name = "Arial", Size = 36, Style = 1 },
                HorizontalAlignment = LWJHAlign.Left,
                VerticalAlignment = LWJVAlign.Center,
                Textbox = new LWJRect(new System.Drawing.Rectangle(80, 864, 1760, 216))
            };
        }

        [JsonIgnore]
        internal TextboxLayout _LegacyHorizontalAlignment_SubText
        {
            get => new TextboxLayout()
            {
                BColor = new LWJColor(System.Drawing.Color.Transparent),
                KColor = new LWJColor(System.Drawing.Color.Transparent),
                FColor = new LWJColor(System.Drawing.Color.White),
                Font = new LWJFont() { Name = "Arial", Size = 36, Style = 0 },
                HorizontalAlignment = LWJHAlign.Right,
                VerticalAlignment = LWJVAlign.Center,
                Textbox = new LWJRect(new System.Drawing.Rectangle(80, 864, 1760, 216))
            };
        }

        [JsonIgnore]
        internal TextboxLayout _LegacyVerticalAlignment_MainText
        {
            get => new TextboxLayout()
            {
                BColor = new LWJColor(System.Drawing.Color.Black),
                KColor = new LWJColor(System.Drawing.Color.Gray),
                FColor = new LWJColor(System.Drawing.Color.White),
                Font = new LWJFont() { Name = "Arial", Size = 36, Style = 0 },
                HorizontalAlignment = LWJHAlign.Left,
                VerticalAlignment = LWJVAlign.Center,
                Textbox = new LWJRect(new System.Drawing.Rectangle(0, 0, 1920, 1080))
            };
        }

        [JsonIgnore]
        internal TextboxLayout _LegacyVerticalAlignment_SubText
        {
            get => new TextboxLayout()
            {
                BColor = new LWJColor(System.Drawing.Color.Black),
                KColor = new LWJColor(System.Drawing.Color.Gray),
                FColor = new LWJColor(System.Drawing.Color.White),
                Font = new LWJFont() { Name = "Arial", Size = 36, Style = 0 },
                HorizontalAlignment = LWJHAlign.Left,
                VerticalAlignment = LWJVAlign.Center,
                Textbox = new LWJRect(new System.Drawing.Rectangle(0, 0, 1920, 1080))
            };
        }

        public override string GetDefaultJson()
        {
            return JsonSerializer.Serialize(_Internal_GetDefaultInfo(), new JsonSerializerOptions() { WriteIndented = true });
        }


        public _2TitleSlideLayoutInfo GetLayoutInfo(Slide slide)
        {
            return ILayoutInfoResolver<_2TitleSlideLayoutInfo>._InternalDefault_GetLayoutInfo(slide);
        }

        public _2TitleSlideLayoutInfo _Internal_GetDefaultInfo(string overrideDefault = "")
        {
            return ILayoutInfoResolver<_2TitleSlideLayoutInfo>.GetDefaultInfo(overrideDefault);
        }
    }
}
