﻿using BMDSwitcherAPI;

using System;
using System.Collections.Generic;
using System.Linq;

using VariableMarkupAttributes.Attributes;

namespace Configurations.SwitcherConfig
{
    public class BMDUSKPATTERNSettings
    {

        [ExposedAsVariable(nameof(PatternType))]
        public string PatternType { get; set; }

        [ExposedAsVariable(nameof(Inverted))]
        public bool Inverted { get; set; }

        [ExposedAsVariable(nameof(Softness))]
        public double Softness { get; set; }

        [ExposedAsVariable(nameof(Symmetry))]
        public double Symmetry { get; set; }

        [ExposedAsVariable(nameof(Size))]
        public double Size { get; set; }

        [ExposedAsVariable(nameof(XOffset))]
        public double XOffset { get; set; }

        [ExposedAsVariable(nameof(YOffset))]
        public double YOffset { get; set; }

        public override string ToString()
        {
            return $"X{XOffset};Y{YOffset};Sz{Size};Sym{Symmetry};Soft{Softness};I{Inverted};P{PatternType};";
        }


        [ExposedAsVariable(nameof(DefaultFillSource))]
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
                DefaultFillSource = DefaultFillSource,
                PatternType = PatternType,
                Inverted = Inverted,
                Softness = Softness,
                Symmetry = Symmetry,
                Size = Size,
                XOffset = XOffset,
                YOffset = YOffset,
            };
        }

        public static string FindPattern(_BMDSwitcherPatternStyle pattern)
        {
            var match = Patterns.FirstOrDefault(x => x.Value == pattern).Key;
            return match;
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
