using System;
using System.Collections.Generic;
using System.Text;

namespace IntegratedPresenter.Presentation
{
    public enum DriveMode
    {
        /// <summary>
        /// Next slide just advances the presentation.
        /// </summary>
        Undriven,
        /// <summary>
        /// Next slide advances presentation and applies automation on slides
        /// </summary>
        Drive,
        /// <summary>
        /// Next slide advances presentation and applies automation on slides, also ties a transition request
        /// </summary>
        DriveTied,
        /// <summary>
        /// Next slide sets the 'next' slide to be the 'after' slide. Does not change current slide.
        /// </summary>
        Skip,
    }
}
