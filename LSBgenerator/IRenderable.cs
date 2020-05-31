namespace LSBgenerator
{
    public interface IRenderable
    {
        int RenderX { get; set; }
        int RenderY { get; set; }
        int Height { get; set; }
        int Width { get; set; }
        LayoutMode RenderLayoutMode { get; set; }
        void Render(RenderSlide slide, TextRenderer r);
    }
}
