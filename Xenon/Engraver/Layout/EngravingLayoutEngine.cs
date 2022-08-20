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
                    var staff = new VisualStaff
                    {
                        Width = layout.Engraving.Box.Size.Width,
                    };
                    EngravingCollection collection = new EngravingCollection
                    {
                        XOffset = 20,
                        YOffset = Yoff,
                        Objects = new List<IEngravingRenderable>
                        {
                            staff,
                        }
                    };

                    // add bars/notes to staff
                    collection.Objects.AddRange(PlaceSimpleNotes(line, staff));

                    vobjs.Add(collection);
                    Yoff += InterStaffHeight + 100; // staff height TODO: make it computed
                }

                Yoff += InterPartHeight;
            }






            return vobjs;
        }

        private static List<IEngravingRenderable> PlaceSimpleNotes(MusicLine line, VisualStaff staff)
        {
            List<IEngravingRenderable> lobjs = new List<IEngravingRenderable>();

            float XLineOffset = 0;

            // TODO: clef/keysig/timesig

            bool firstbar = true;

            foreach (var bar in line._ExtractNoteBars())
            {
                // add pre-bar
                VisualBarLine preBar = new VisualBarLine
                {
                    Type = bar.BeginBar,
                    Height = 80,
                    XOffset = firstbar ? 0 : XLineOffset,
                    YOffset = 0
                };

                if (preBar.Width > 0)
                {
                    lobjs.Add(preBar);
                    XLineOffset += preBar.Width;
                }

                firstbar = false;

                // add contents

                // add metadata  (clef, tsig, ksig)
                if (bar.ShowClef)
                {
                    VisualClef vclef = new VisualClef
                    {
                        XOffset = XLineOffset,
                        ClefType = bar.Clef,
                        YOffset = (float)Math.Round(staff.Lines / 2f) * staff.LineSpace,
                    };
                    var cbounds = vclef.CalculateBounds(0, 0);
                    XLineOffset += cbounds.MaxBounds.Width;
                    lobjs.Add(vclef);
                }
                if (bar.ShowKeySig)
                {
                    VisualKeySignature key = new VisualKeySignature
                    {
                        XOffset = XLineOffset,
                        YOffset = 0,
                        Key = bar.KeySig,
                        Clef = bar.Clef,
                    };
                    var kbounds = key.CalculateBounds(0, 0);
                    XLineOffset += kbounds.MaxBounds.Width;
                    lobjs.Add(key);
                }

                // add notes
                foreach (var ngroup in bar.Notes)
                {
                    lobjs.Add(PlaceNoteGroups(ngroup, bar.Clef, ref XLineOffset));
                }

                // add end-bar
                VisualBarLine postBar = new VisualBarLine
                {
                    Type = bar.EndBar,
                    Height = 80,
                    XOffset = XLineOffset,
                    YOffset = 0
                };
                XLineOffset += postBar.Width;
                lobjs.Add(postBar);
            }

            return lobjs;
        }

        private static IEngravingRenderable PlaceNoteGroups(DataModel.NoteGroup ngroup, DataModel.Clef clef, ref float Xoff)
        {
            if (ngroup.Notes.Count == 1)
            {
                return PlaceSimpleFigure(ngroup.Notes.First(), clef, ref Xoff);
            }

            return PlaceSimpleBeamGroup(ngroup, clef, ref Xoff);
        }

        private static IEngravingRenderable PlaceSimpleBeamGroup(DataModel.NoteGroup ngroup, DataModel.Clef clef, ref float Xoff)
        {
            VisualNoteFigureBeamGroup fig = new VisualNoteFigureBeamGroup
            {
                Clef = clef,
                ChildNotes = ngroup.Notes,
                XOffset = Xoff,
                YOffset = 0,
            };

            var bounds = fig.CalculateBounds(0, 0);

            // adjust figure position based on size
            //fig.XOffset = fig.XOffset + (bounds.BodyOrigin.X - bounds.FigureBounds.Left);

            Xoff += bounds.MaxBounds.Width;

            return fig;
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

            var bounds = fig.CalculateBounds(0, 0);

            // adjust figure position based on size
            fig.XOffset = fig.XOffset + (bounds.BodyOrigin.X - bounds.FigureBounds.Left);

            Xoff += bounds.MaxBounds.Width;

            return fig;
        }

    }


}
