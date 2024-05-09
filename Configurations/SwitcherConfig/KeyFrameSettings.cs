using VariableMarkupAttributes.Attributes;

namespace IntegratedPresenter.BMDSwitcher.Config
{
    public class KeyFrameSettings
    {

        [ExposedAsVariable(nameof(PositionX))]
        public double PositionX { get; set; }

        [ExposedAsVariable(nameof(PositionY))]
        public double PositionY { get; set; }
        
        [ExposedAsVariable(nameof(SizeX))]
        public double SizeX { get; set; }

        [ExposedAsVariable(nameof(SizeY))]
        public double SizeY { get; set; }

        public KeyFrameSettings Copy()
        {
            return new KeyFrameSettings()
            {
                PositionX = this.PositionX,
                PositionY = this.PositionY,
                SizeX = this.SizeX,
                SizeY = this.SizeX,
            };
        }
    }
}
