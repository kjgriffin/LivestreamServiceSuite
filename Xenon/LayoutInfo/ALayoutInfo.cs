using System;
using System.Collections.Generic;
using System.Text;

namespace Xenon.LayoutInfo
{
    public abstract class ALayoutInfo
    {
        virtual public string GetDefaultJson()
        {
            return "";
        }
    }
}
