using System;
using System.Collections.Generic;
using System.Text;

namespace Slide_Presentation
{
    public interface ISlideDeck
    {
        List<ISlide> Slides { get; set; }
    }
}
