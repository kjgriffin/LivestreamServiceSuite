using System;
using System.Collections.Generic;
using System.Text;

namespace Slide_Presentation
{
    public interface ISlide
    {
        string Filename { get; set; }
        Uri SourceUri { get; }
    }
}
