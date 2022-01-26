using Xenon.LayoutInfo.BaseTypes;

namespace Xenon.LayoutEngine.L2
{
    static class ResponsiveLiturgyLayoutInfoPrototyptes
    {
        public static TextboxLayout TEXTBOX = new TextboxLayout
        {
            Font = new Layouts.LWJFont
            {
                Name = "Arial",
                Size = 36,
                Style = 0
            },
            Textbox = new Layouts.LWJRect
            {
                Origin = new Layouts.LWJPoint { X = 0, Y = 0 },
                Size = new Layouts.LWJSize { Width = 1920, Height = 216 }
            }
        };
    }
}
