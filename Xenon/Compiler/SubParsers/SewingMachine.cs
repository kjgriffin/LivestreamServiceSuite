using System;
using System.Collections.Generic;
using System.Linq;
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
            var match = Regex.Match(def, @"\((?<take>)\d+,1:(?<rel>\d+),(?<tag>\w+)\)");
            if (!match.Success)
            {
                throw new XenonCompilerException();
            }

            Tag = match.Groups["tag"].Value;
            SourceAssets = int.Parse(match.Groups["take"].Value);
            Mapping = int.Parse(match.Groups["rel"].Value);
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

            return null;

        }


    }


}
