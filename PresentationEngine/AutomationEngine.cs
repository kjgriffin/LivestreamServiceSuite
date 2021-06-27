using Integrated_Presenter;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Threading;

namespace PresentationEngine
{
    public class AutomationEngine
    {

        // Requirements
        /*
            Should abstract all the nasty presentation management stuff from whoever needs to use it
            Will have the correct dispatcher for threading
            manages the presentation
            has access to switcher manager to perform switcher commands
            handles the graphics display outputs
         */

        Dispatcher _dispatcher;
        public Dispatcher Dispatcher => _dispatcher;

        Presentation _presentation;
        public Presentation Presentation => _presentation;


        public AutomationEngine(Dispatcher d)
        {
            _dispatcher = d;
        }

        public void LoadPresentation()
        {

        }


    }
}
