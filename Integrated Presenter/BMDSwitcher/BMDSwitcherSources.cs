using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;

namespace Integrated_Presenter.BMDSwitcher
{
    public enum BMDSwitcherSources
    {
        Black = 0,
        Input1 = 1,
        Input2 = 2,
        Input3 = 3,
        Input4 = 4,
        Input5 = 5,
        Input6 = 6,
        Input7 = 7,
        Input8 = 8,
        // same until
        Input20 = 20,
        ColorBars = 1000,
        Color1 = 2001,
        Color2 = 2002,
        MediaPlayer1 = 3010,
        MediaPlayer1Key = 2011,
        MediaPlayer2 = 3020,
        MediaPlayer2Key = 3021,
        Key1Mask = 4010,
        Key2Mask = 4020,
        Key3Mask = 4030,
        Key4Mask = 4040,
        DSK1Mask = 5010,
        DSK2Mask = 5020,
        SuperSource = 6000,
        CleanFeed1 = 7001,
        CleenFeed2 = 7002,
        Auxillary1 = 8001,
        // Aux 800x
        Auxillary6 = 8006,
        ME1Prog = 10010,
        ME1Prev = 10011,
        ME2Prog = 10020,
        ME2Prev = 10021,
    }
}
