using BMDSwitcherAPI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Configurations.SwitcherConfig
{
    public class BMDUSKPATTERNSettings
    {

        public string PatternType { get; set; }
        public bool Inverted { get; set; }
        public double Softness { get; set; }
        public double Symmetry { get; set; }
        public double Size { get; set; }
        public double XOffset { get; set; }
        public double YOffset { get; set; }
        public int DefaultFillSource { get; set; }

        public bool Equivalent(BMDUSKPATTERNSettings pattern)
        {
            return CloseEnough(Softness, pattern?.Softness) &&
                CloseEnough(Symmetry, pattern?.Symmetry) &&
                CloseEnough(Size, pattern?.Size) &&
                CloseEnough(XOffset, pattern?.XOffset) &&
                CloseEnough(YOffset, pattern?.YOffset) &&
                Inverted == pattern?.Inverted &&
                PatternType == pattern?.PatternType;
        }

        private bool CloseEnough(double a, double? b)
        {
            return Math.Abs(a - b ?? 0) <= 0.0001;
        }

        public BMDUSKPATTERNSettings Copy()
        {
            return new BMDUSKPATTERNSettings
            {
                PatternType = PatternType,
                Inverted = Inverted,
                Softness = Softness,
                Symmetry = Symmetry,
                Size = Size,
                XOffset = XOffset,
                YOffset = YOffset,
            };
        }



        public static _BMDSwitcherPatternStyle DEFAULTPATTERNTYPE { get => _BMDSwitcherPatternStyle.bmdSwitcherPatternStyleCircleIris; }

        public static Dictionary<string, _BMDSwitcherPatternStyle> Patterns = new Dictionary<string, _BMDSwitcherPatternStyle>
        {
            ["v-bar"] = _BMDSwitcherPatternStyle.bmdSwitcherPatternStyleTopToBottomBar,
            ["h-bar"] = _BMDSwitcherPatternStyle.bmdSwitcherPatternStyleLeftToRightBar,

            ["v-barn"] = _BMDSwitcherPatternStyle.bmdSwitcherPatternStyleVerticalBarnDoor,
            ["h-barn"] = _BMDSwitcherPatternStyle.bmdSwitcherPatternStyleHorizontalBarnDoor,

            ["rect-iris"] = _BMDSwitcherPatternStyle.bmdSwitcherPatternStyleRectangleIris,
            ["diamond-iris"] = _BMDSwitcherPatternStyle.bmdSwitcherPatternStyleDiamondIris,
            ["circle-iris"] = _BMDSwitcherPatternStyle.bmdSwitcherPatternStyleCircleIris,

            ["t-box"] = _BMDSwitcherPatternStyle.bmdSwitcherPatternStyleCornersInFourBox,

            ["tl-box"] = _BMDSwitcherPatternStyle.bmdSwitcherPatternStyleTopLeftBox,
            ["tr-box"] = _BMDSwitcherPatternStyle.bmdSwitcherPatternStyleTopRightBox,
            ["bl-box"] = _BMDSwitcherPatternStyle.bmdSwitcherPatternStyleBottomLeftBox,
            ["br-box"] = _BMDSwitcherPatternStyle.bmdSwitcherPatternStyleBottomRightBox,

            ["lc-box"] = _BMDSwitcherPatternStyle.bmdSwitcherPatternStyleLeftCentreBox,
            ["rc-box"] = _BMDSwitcherPatternStyle.bmdSwitcherPatternStyleRightCentreBox,
            ["tc-box"] = _BMDSwitcherPatternStyle.bmdSwitcherPatternStyleTopCentreBox,
            ["bc-box"] = _BMDSwitcherPatternStyle.bmdSwitcherPatternStyleBottomCentreBox,

            ["l-diag"] = _BMDSwitcherPatternStyle.bmdSwitcherPatternStyleTopLeftDiagonal,
            ["r-diag"] = _BMDSwitcherPatternStyle.bmdSwitcherPatternStyleTopRightDiagonal,
        };


    }
}
