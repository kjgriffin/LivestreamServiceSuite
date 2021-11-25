using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

using Xenon.Compiler;
using Xenon.Renderer;

namespace Xenon.SlideAssembly
{
    public class ProjectLayouts
    {

        internal Dictionary<LanguageKeywordCommand, Dictionary<string, string>> AllLayouts { get; set; } = new Dictionary<LanguageKeywordCommand, Dictionary<string, string>>();

        public List<(string group, Dictionary<string, string> layouts)> GetGroupedLayouts()
        {
            List<(string group, Dictionary<string, string> layouts)> res = new List<(string group, Dictionary<string, string> layouts)>();
            foreach (var lgroup in AllLayouts)
            {
                res.Add((LanguageKeywords.Commands[lgroup.Key], lgroup.Value));
            }
            return res;
        }

        public static (Bitmap main, Bitmap key) GetLayoutPreview(string layoutname, string layoutjson)
        {

            if (LanguageKeywords.Commands.ContainsValue(layoutname))
            {
                LanguageKeywordCommand cmd = LanguageKeywords.Commands.FirstOrDefault(x => x.Value == layoutname).Key;
                if (LanguageKeywords.LayoutForType.TryGetValue(cmd, out var proto))
                {
                    return proto.prototypicalLayoutPreviewer.GetPreviewForLayout(layoutjson);
                }
            }

            return (new Bitmap(1920, 1080), new Bitmap(1920, 1080));
        }

        public void LoadDefaults()
        {
            foreach (var cmd in LanguageKeywords.Commands.Keys)
            {
                var cmdmeta = LanguageKeywords.LanguageKeywordMetadata[cmd];
                if (cmdmeta.hasLayoutInfo)
                {
                    //var name = cmdmeta.implementation.GetType().Name;
                    var name = "Default";

                    var d = LanguageKeywords.LayoutForType[cmd].layoutResolver._Internal_GetDefaultInfo();
                    string json = d.GetDefaultJson();

                    AllLayouts.TryGetValue(cmd, out var dict);
                    if (dict == null)
                    {
                        dict = new Dictionary<string, string>();
                    }
                    dict[name] = json;
                    AllLayouts[cmd] = dict;
                }
            }
        }

    }
}
