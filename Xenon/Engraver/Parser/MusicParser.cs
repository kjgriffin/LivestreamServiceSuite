using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Xenon.Engraver.DataModel;

namespace Xenon.Engraver.Parser
{

    internal static class BaseParser
    {
        internal static List<string> SplitBraceBlocks(string input)
        {
            var split = Regex.Split(input, @"((?<!\\){|(?<!\\)})");

            return split.ToList();
        }

        internal static string ExtractSquareTag(string input, string prefix)
        {
            var tin = input.Trim();
            if (tin.StartsWith(prefix))
            {
                return Regex.Match(tin, @"\[(?<name>.*)\]").Groups["name"].Value;
            }
            return "";
        }

        internal static Dictionary<string, string> ExtractNamedArgs(string input)
        {
            Dictionary<string, string> args = new Dictionary<string, string>();

            var tin = Regex.Match(input, @"\((?<args>.*)\)").Groups["args"].Value;
            var parts = Regex.Split(tin, ",");

            foreach (var part in parts)
            {
                var match = Regex.Match(part, @"(?<key>.*)=(?<val>.*)");
                args[match.Groups["key"].Value] = match.Groups["val"].Value;
            }

            return args;
        }

    }

    internal class MusicParser
    {

        internal static List<MusicPart> ExtractMusicParts(string input)
        {
            var bsplit = BaseParser.SplitBraceBlocks(input);

            List<MusicPart> Parts = new List<MusicPart>();

            StringBuilder sb = new StringBuilder();

            // go hunting part blocks
            MusicPart part = null;
            int depth = 0;
            bool open = false;
            foreach (var item in bsplit)
            {
                if (item == "{")
                {
                    open = true;
                    depth++;
                    continue;
                }
                else if (item == "}")
                {
                    open = false;
                    depth--;
                    continue;
                }

                if (depth == 1 && open) // expect part
                {
                    open = false;
                    part = new MusicPart();
                    part.Name = BaseParser.ExtractSquareTag(item.Trim(), "part");
                    Parts.Add(part);
                }
                else if (depth == 2) // expect lines
                {
                    var val = item.Trim();
                    if (!string.IsNullOrWhiteSpace(val))
                    {
                        part?.Lines.Add(new MusicLine()
                        {
                            Args = BaseParser.ExtractNamedArgs(val)
                        });
                    }
                }
                else if (depth == 3) // content of line
                {
                    var val = item.Trim();
                    if (!string.IsNullOrWhiteSpace(val))
                    {
                        part?.Lines?.LastOrDefault()?.SetBody(val);
                    }
                }

            }



            return Parts;
        }


    }

    internal class MusicPart
    {
        public string Name { get; set; }
        public List<MusicLine> Lines { get; set; } = new List<MusicLine>();
    }

    internal class MusicLine
    {
        public Dictionary<string, string> Args { get; set; } = new Dictionary<string, string>();
        public string Body { get; set; }

        internal void SetBody(string val)
        {
            Body = val;
        }

        internal List<MusicBar> _ExtractNoteBars()
        {
            string nseq = Regex.Match(Body, @"note=(?<seq>.*);").Groups["seq"].Value;
            string rseq = Regex.Match(Body, @"rhythm=(?<seq>.*);").Groups["seq"].Value;

            List<MusicBar> bars = new List<MusicBar>();

            var lbars = Regex.Split(nseq, @"(\|\||\|)").Where(x => !string.IsNullOrEmpty(x)).ToArray();
            var rbars = Regex.Split(rseq, @"(\|\||\|)").Where(x => !string.IsNullOrEmpty(x)).ToArray();

            if (lbars.Length != rbars.Length)
            {
                // ERROR!
                return bars;
            }


            var clefs = ParseClefs();
            var keys = ParseKeySignatures();

            var curentLineclef = clefs.First();
            int clefid = 0;

            var curentKeySig = keys.First();
            int keyid = 0;

            for (int i = 0; i + 1 < lbars.Length; i += 2)
            {
                MusicBar bar = new MusicBar();
                // handle pre-bar
                if (i + 2 < lbars.Length)
                {
                    var type = ParseBarType(lbars[i]);
                    if (type != BarType.None)
                    {
                        bar.BeginBar = type;
                        i++;
                    }
                }

                var bardata = Regex.Match(lbars[i], @"(?<clef>C)?(?<tsig>T)?(?<ksig>K)?,?(?<nv>.*)");

                if (bardata.Groups["clef"].Success)
                {
                    if (clefs.Count > clefid)
                    {
                        curentLineclef = clefs[clefid++];
                    }
                    else
                    {
                        curentLineclef = clefs.Last();
                    }
                    bar.ShowClef = true;
                }

                if (bardata.Groups["ksig"].Success)
                {
                    if (keys.Count > keyid)
                    {
                        curentKeySig = keys[keyid++];
                    }
                    else
                    {
                        curentKeySig = keys.Last();
                    }
                    bar.ShowKeySig = true;
                }


                // get bar metadata
                bar.Clef = curentLineclef;
                bar.KeySig = curentKeySig;

                // get contents
                bar.Notes = ParseNoteNames(bardata.Groups["nv"].Value, curentLineclef);

                // attach rhythm values to notes
                EvaluateRhythm_Mutate(rbars[i], bar.Notes);

                // look for end
                bar.EndBar = ParseBarType(lbars[i + 1]);


                bars.Add(bar);
            }

            return bars;
        }

        private BarType ParseBarType(string input)
        {
            switch (input)
            {
                case "|":
                    return BarType.Single;
                case "||":
                    return BarType.Double;
                default:
                    return BarType.None;
            }
        }

        private List<Note> ParseNoteNames(string input, Clef clef)
        {
            List<Note> notes = new List<Note>();

            int baseReg = Theory.TheoryRules.NominalRegister(clef);

            var nstrings = input.Split(",");

            foreach (var nstring in nstrings)
            {
                // make it a note
                // we'll ignore rhythm for now
                notes.Add(ParseNoteVal(nstring, baseReg));
            }


            return notes;
        }

        private void EvaluateRhythm_Mutate(string rinput, List<Note> notes)
        {
            var rstrings = rinput.Split(",");

            if (notes.Count != rstrings.Length)
            {
                // ERROR!
                return;
            }

            int i = 0;
            foreach (var nstring in rstrings)
            {
                // add the rhythm to the note
                var nl = ParseNoteLength(nstring);
                notes[i].Length = nl.len;
                notes[i].LengthDots = nl.dots;
                i++;
            }

        }

        private (NoteLength len, int dots) ParseNoteLength(string val)
        {
            // 1 = quarter
            // %8 = eighth
            // %16 = sixteenth etc.
            // 1. = quarter 1 dot
            // %8. = eigth dotted 
            var match = Regex.Match(val, @"(?<sub>%)?(?<val>\d+)(?<dot>\.*)");

            if (int.TryParse(match.Groups["val"].Value, out int nval))
            {
                int dots = 0;
                var ds = match.Groups["dot"]?.Value ?? "";
                dots = ds.Length;

                if (match.Groups["sub"].Value == "%")
                {
                    switch (nval)
                    {
                        case 8:
                            return (NoteLength.EIGHTH, dots);
                        case 16:
                            return (NoteLength.SIXTEENTH, dots);
                    }
                }
                else
                {
                    switch (nval)
                    {
                        case 1:
                            return (NoteLength.QUARTER, dots);
                        case 2:
                            return (NoteLength.HALF, dots);
                        case 4:
                            return (NoteLength.WHOLE, dots);
                    }
                }

            }

            return (NoteLength.QUARTER, 0);
        }


        internal List<Clef> ParseClefs()
        {

            if (Args.TryGetValue("clef", out var sclef))
            {

                var clefs = sclef.Split(";", StringSplitOptions.RemoveEmptyEntries);

                List<Clef> result = new List<Clef>();
                foreach (var clef in clefs)
                {
                    switch (clef)
                    {
                        case "trebble":
                            result.Add(Clef.Trebble);
                            break;
                        case "base":
                            result.Add(Clef.Base);
                            break;
                    }

                }
                return result;
            }
            return new List<Clef> { Clef.Unkown };
        }

        internal List<KeySignature> ParseKeySignatures()
        {

            if (Args.TryGetValue("key", out var skeys))
            {

                var keys = skeys.Split(";", StringSplitOptions.RemoveEmptyEntries);

                List<KeySignature> result = new List<KeySignature>();
                foreach (var key in keys)
                {

                    var kparts = Regex.Match(key, "(?<k>[cdefgabCDEFGAB])(?<a>[#b])?(?<m>[mM])");

                    var kname = kparts.Groups["k"].Value.ToUpper();
                    var acc = kparts.Groups["a"]?.Value ?? "";
                    var mode = kparts.Groups["m"].Value;

                    var kstr = kname + acc + mode;

                    switch (kstr)
                    {
                        case "CM":
                            result.Add(KeySignature.C_MAJOR);
                            break;
                        case "GM":
                            result.Add(KeySignature.G_MAJOR);
                            break;
                        case "DM":
                            result.Add(KeySignature.D_MAJOR);
                            break;
                        case "AM":
                            result.Add(KeySignature.A_MAJOR);
                            break;
                        case "EM":
                            result.Add(KeySignature.E_MAJOR);
                            break;
                        case "BM":
                            result.Add(KeySignature.B_MAJOR);
                            break;
                        case "F#M":
                            result.Add(KeySignature.F_SHARP_MAJOR);
                            break;
                        case "C#M":
                            result.Add(KeySignature.C_SHARP_MAJOR);
                            break;

                        case "FM":
                            result.Add(KeySignature.F_MAJOR);
                            break;
                        case "BbM":
                            result.Add(KeySignature.B_FLAT_MAJOR);
                            break;
                        case "EbM":
                            result.Add(KeySignature.E_FLAT_MAJOR);
                            break;
                        case "AbM":
                            result.Add(KeySignature.A_FLAT_MAJOR);
                            break;
                        case "DbM":
                            result.Add(KeySignature.D_FLAT_MAJOR);
                            break;
                        case "GbM":
                            result.Add(KeySignature.G_FLAT_MAJOR);
                            break;
                        case "CbM":
                            result.Add(KeySignature.C_FLAT_MAJOR);
                            break;
                            
                    }

                }
                return result;
            }
            return new List<KeySignature> { KeySignature.NONE };
        }


        private Note ParseNoteVal(string input, int basereg)
        {
            Note note = new Note();

            // note syntax:
            // <nval><accidental(optional)><register(optional)>
            // eg.
            // b = note 'B' at cleft nominal register
            // b_b = note 'B flat' at clef nominal register
            // b_5 = note 'B' at register 5 
            // b_#_2 = note 'B sharp' at register 2

            // notenames: a,b,c,d,e,f,g
            // accidentals: b,#,%, flat, sharp, natural
            // registers: 1,2,3,4,5,6,7 etc.

            var nmatch = Regex.Match(input, @"(?<nval>[abcdefg])(?<acc>[b#%])?(?<reg>\d)?");

            note.Name = ParseNoteName(nmatch.Groups["nval"].Value);

            if (nmatch.Groups.TryGetValue("acc", out var acc))
            {
                note.Accidental = ParseAccidental(acc.Value);
            }
            else
            {
                note.Accidental = Accidental.None;
            }

            if (nmatch.Groups.TryGetValue("reg", out var reg) && !string.IsNullOrEmpty(reg.Value))
            {
                note.Register = int.Parse(reg.Value);
            }
            else
            {
                note.Register = basereg;
            }

            note.TieToNext = false;
            // for now just ignore
            note.Length = NoteLength.QUARTER;

            return note;
        }

        private Accidental ParseAccidental(string value)
        {
            switch (value)
            {
                case "b":
                    return Accidental.Flat;
                case "%":
                    return Accidental.Natural;
                case "#":
                    return Accidental.Sharp;
                default:
                    return Accidental.None;
            }
        }

        private NoteName ParseNoteName(string input)
        {
            switch (input)
            {
                case "a":
                    return NoteName.A;
                case "b":
                    return NoteName.B;
                case "c":
                    return NoteName.C;
                case "d":
                    return NoteName.D;
                case "e":
                    return NoteName.E;
                case "f":
                    return NoteName.F;
                case "g":
                    return NoteName.G;
                default:
                    return NoteName.A;
            }
        }


    }

}
