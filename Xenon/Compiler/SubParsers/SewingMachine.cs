using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Xenon.Compiler.SubParsers
{
    internal class SewingMachine
    {



    }

    internal class SewPattern
    {
        List<SewingBlock> Blocks = new List<SewingBlock>();
        List<List<StitchMark>> Stitching = new List<List<StitchMark>>();

        public SewPattern(string parseDef, string stitchDef, List<string> assets)
        {
            Blocks = SewingBlock.ParseBlocks(parseDef);
            Stitching = StitchMark.Parse(stitchDef);

            // divy up assets
            int skip = 0;
            foreach (var block in Blocks)
            {
                block.AssignAssets(assets.Skip(skip).Take(block.SourceAssets).ToList());
                skip += block.SourceAssets;
            }
        }

        public List<List<string>> GenerateStitchedSequence()
        {
            List<List<string>> slides = new List<List<string>>();
            foreach (var slide in Stitching)
            {
                List<string> lines = new List<string>();
                foreach (var srcGen in slide)
                {
                    // get assets if found
                    var b = Blocks.FirstOrDefault(b => b.Tag == srcGen.Tag);
                    if (b == null)
                    {
                        throw new Exception("Missing Tag!");
                    }
                    var lgen = b.GenerateSequence(srcGen.Indice);
                    lines.AddRange(lgen);
                }
                slides.Add(lines);
            }
            return slides;
        }

    }

    internal class StitchMark
    {
        public StitchMark(string tag, int indice, bool isIndexed)
        {
            Tag = tag;
            Indice = indice;
            IsIndexed = isIndexed;
        }

        public static List<List<StitchMark>> Parse(string input)
        {
            var stitches = input.Split(";", StringSplitOptions.RemoveEmptyEntries);
            List<List<StitchMark>> res = new List<List<StitchMark>>();
            foreach (var stitch in stitches)
            {
                List<StitchMark> slidethreads = new List<StitchMark>();
                var threads = stitch.Split(",", StringSplitOptions.RemoveEmptyEntries);
                foreach (var thread in threads)
                {
                    var match = Regex.Match(thread, @"(?<id>\w+)(\((?<arg>\d+)\))?");
                    if (match.Success)
                    {
                        int index = 1;
                        if (!string.IsNullOrEmpty(match.Groups["arg"].Value))
                        {
                            index = int.Parse(match.Groups["arg"].Value);
                        }
                        slidethreads.Add(new StitchMark(match.Groups["id"].Value, index, index > -1));
                    }
                }
                if (slidethreads.Any())
                {
                    res.Add(slidethreads);
                }
            }
            return res;
        }

        public string Tag { get; private set; }
        public int Indice { get; private set; }
        public bool IsIndexed { get; private set; }
    }

    internal class SewingBlock
    {
        public int SourceAssets { get; internal set; }
        public bool ManyToOneMapping { get => Mapping > 0; }
        public int Mapping { get; internal set; }
        public string Tag { get; internal set; }

        List<string> _AssetRefs;

        public SewingBlock(string def)
        {
            // parse the def
            var match = Regex.Match(def, @"\((?<take>\d+),1:(?<rel>\d+),(?<tag>\w+)\)");
            if (!match.Success)
            {
                throw new XenonCompilerException();
            }

            Tag = match.Groups["tag"].Value;
            SourceAssets = int.Parse(match.Groups["take"].Value);
            Mapping = int.Parse(match.Groups["rel"].Value);
        }

        public static List<SewingBlock> ParseBlocks(string def)
        {
            var blocks = def.Split(";", StringSplitOptions.RemoveEmptyEntries);
            List<SewingBlock> res = new List<SewingBlock>();
            foreach (var block in blocks)
            {
                res.Add(new SewingBlock(block));
            }
            return res;
        }

        public void AssignAssets(List<string> assets)
        {
            _AssetRefs = assets;
        }

        public List<string> GenerateSequence(int verse = 1)
        {
            if (!ManyToOneMapping || Mapping == 1)
            {
                return _AssetRefs;
            }

            if (verse > Mapping)
            {
                throw new XenonCompilerException(); // requesting verse that was defined to not exist!
            }

            // assume without bounds check?
            List<string> result = new List<string>();

            // iterate 'verses' (nominally)
            var systems = (int)Math.Floor((double)_AssetRefs.Count / (Mapping + 1));
            var lnum = Mapping + 1;

            for (int l = 0; l < systems; l++)
            {
                result.Add(_AssetRefs[(l * lnum)]);
                result.Add(_AssetRefs[(l * lnum) + verse]);
            }

            return result;

        }


    }


}
