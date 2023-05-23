namespace IntegratedPresenter.BMDSwitcher.Config
{
    public class BMDUSKChromaSettings
    {
        public int FillSource { get; set; }
        public double Hue { get; set; }
        public double Gain { get; set; }
        public double YSuppress { get; set; }
        public double Lift { get; set; }
        public int Narrow { get; set; }

        public BMDUSKChromaSettings Copy()
        {
            return new BMDUSKChromaSettings()
            {
                FillSource = this.FillSource,
                Hue = this.Hue,
                Gain = this.Gain,
                YSuppress = this.YSuppress,
                Lift = this.Lift,
                Narrow = this.Narrow,
            };
        }
    }
}
