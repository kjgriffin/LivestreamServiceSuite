using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xenon.Engraver.Parser;
using Xenon.Engraver.Visual;
using Xenon.LayoutInfo;

namespace Xenon.Engraver.Layout
{
    internal class EngravingLayoutEngine
    {

        internal static List<IEngravingRenderable> TestLayout(List<MusicPart> musicParts, EngravingLayoutInfo layout)
        {
            List<IEngravingRenderable> vobjs = new List<IEngravingRenderable>();

            // for now dump out all parts to 1 slide

            float Yoff = 50; // leave some room above...
            float InterStaffHeight = 100;

            float InterPartHeight = 200;

            foreach (var part in musicParts)
            {
                foreach (var line in part.Lines)
                {
                    // try adding staves for each 'line'

                    EngravingCollection collection = new EngravingCollection
                    {
                        XOffset = 20,
                        YOffset = Yoff,
                        Objects = new List<IEngravingRenderable>
                        {
                            new VisualStaff
                            {
                                Width = layout.Engraving.Box.Size.Width,
                            }
                        }
                    };

                    // add bars/notes to staff
                    collection.Objects.AddRange(PlaceSimpleNotes(line));

                    vobjs.Add(collection);
                    Yoff += InterStaffHeight + 100; // staff height TODO: make it computed
                }

                Yoff += InterPartHeight;
            }






            return vobjs;
        }

        private static List<IEngravingRenderable> PlaceSimpleNotes(MusicLine line)
        {
            List<IEngravingRenderable> lobjs = new List<IEngravingRenderable>();

            float XLineOffset = 50;

            // TODO: clef/keysig/timesig

            foreach (var bar in line._ExtractNoteBars())
            {
                // add pre-bar
                VisualBarLine preBar = new VisualBarLine
                {
                    Type = bar.BeginBar,
                    Height = 80,
                    XOffset = XLineOffset,
                    YOffset = 0
                };

                XLineOffset += preBar.Width;
                if (preBar.Width > 0)
                {
                    lobjs.Add(preBar);
                }

                // add contents
                foreach (var note in bar.Notes)
                {
                    lobjs.Add(PlaceSimpleFigure(note, line.ParseClef(), ref XLineOffset));
                }

                // add end-bar
                VisualBarLine postBar = new VisualBarLine
                {
                    Type = bar.EndBar,
                    Height = 80,
                    XOffset = XLineOffset,
                    YOffset = 0
                };
                XLineOffset += postBar.Width + 40;
                lobjs.Add(postBar);
            }

            return lobjs;
        }

        private static IEngravingRenderable PlaceSimpleFigure(DataModel.Note note, DataModel.Clef clef, ref float Xoff)
        {
            VisualNoteFigure fig = new VisualNoteFigure
            {
                Clef = clef,
                NValue = note,
                XOffset = Xoff,
                YOffset = 0,
            };
            Xoff += fig.Width + 40;
            return fig;
        }

    }


}
