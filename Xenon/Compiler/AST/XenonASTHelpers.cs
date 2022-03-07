using System;
using System.Collections.Generic;
using System.Text;

using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    static class XenonASTHelpers
    {
        public static string DATAKEY_POSTSET { get => "postset"; }
        /// <summary>
        /// If the _parent is a XenonASTExpression will add postset info to slide.
        /// </summary>
        /// <param name="slide">Slide to add postset info for</param>
        /// <param name="_parent">Parent of command generating slide. Should be of type XenonASTExpression.</param>
        /// <param name="isfirst">Is first slide of command.</param>
        /// <param name="islast">Is last slide of command.</param>
        public static void AddPostset(this Slide slide, IXenonASTElement _parent, bool isfirst, bool islast)
        {
            var parent = _parent as XenonASTExpression;
            if (parent == null || parent?.Postset == false)
            {
                // can't do it.
                return;
            }
            // check for first/last
            if (isfirst && parent.Postset_forFirst)
            {
                slide.Data[DATAKEY_POSTSET] = parent.Postset_First;
            }
            else if (islast && parent.Postset_forLast)
            {
                // last will overwrite first request if only one slide.
                slide.Data[DATAKEY_POSTSET] = parent.Postset_Last;
            }
            else
            {
                // use the all if it exists
                if (parent.Postset_forAll)
                {
                    slide.Data[DATAKEY_POSTSET] = parent.Postset_All;
                }
            }
        }

        public static bool TryGetPostset(this Slide slide, out int postset)
        {
            if (slide.Data.TryGetValue(DATAKEY_POSTSET, out object val))
            {
                postset = (int)val;
                return true;
            }
            postset = -1;
            return false;
        }
    }
}
