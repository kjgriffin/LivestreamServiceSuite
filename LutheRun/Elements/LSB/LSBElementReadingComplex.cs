using AngleSharp.Dom;

using LutheRun.Elements.Interface;
using LutheRun.Parsers;
using LutheRun.Pilot;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using static LutheRun.Elements.LSB.LSBElementHymn;

namespace LutheRun.Elements.LSB
{

    class LSBElementReadingComplex : ILSBElement, IDownloadWebResource
    {
        class ReadingPartElementCollection
        {
            public List<IElement> Elements { get; private set; } = new List<IElement>();
            public ReadingResponsePart Type { get; private set; } = ReadingResponsePart.Unknown;

            public List<HymnImageLine> _imagelines = new List<HymnImageLine>();

            public string XenonCmdText(string title, string reference, ref int indentDepth, int indentSpaces)
            {
                switch (Type)
                {
                    case ReadingResponsePart.SpokenResponse:
                        return AsPackageResponse(title, reference, ref indentDepth, indentSpaces);
                    case ReadingResponsePart.SungResponse:
                        return AsPackageLiturgy(title, reference, ref indentDepth, indentSpaces);
                    default:
                        return "";
                }
            }

            private string AsPackageResponse(string title, string reference, ref int indentDepth, int indentSpaces)
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("#tlit".Indent(indentDepth, indentSpaces));
                sb.AppendLine("{".Indent(indentDepth, indentSpaces));
                indentDepth++;
                sb.AppendLine($"title={{{title}}}".Indent(indentDepth, indentSpaces));
                sb.AppendLine($"title={{{reference}}}".Indent(indentDepth, indentSpaces));
                sb.AppendLine("content={".Indent(indentDepth, indentSpaces));
                indentDepth++;
                sb.AppendLine(LSBResponsorialExtractor.ExtractResponsiveLiturgy(Elements, ref indentDepth, indentSpaces));
                indentDepth--;
                sb.AppendLine("}".Indent(indentDepth, indentSpaces));
                indentDepth--;
                sb.AppendLine("}".Indent(indentDepth, indentSpaces));

                return sb.ToString();
            }
            private string AsPackageLiturgy(string title, string reference, ref int indentDepth, int indentSpaces)
            {
                StringBuilder sb = new StringBuilder();

                foreach (var img in _imagelines)
                {
                    sb.AppendLine("#scope(readingresponsesung)".Indent(indentDepth, indentSpaces));
                    sb.AppendLine("{".Indent(indentDepth, indentSpaces));
                    indentDepth++;

                    sb.AppendLine("#var(\"complextext.Layout\",\"Xenon.Readings::SideBarSungResponse2\")".Indent(indentDepth, indentSpaces));

                    sb.AppendLine("#complextext".Indent(indentDepth, indentSpaces));
                    sb.AppendLine("{".Indent(indentDepth, indentSpaces));
                    indentDepth++;
                    sb.AppendLine($"asset=(\"{img.InferedName}\", \"fg\")".Indent(indentDepth, indentSpaces));
                    sb.AppendLine($"text={{{title}}}".Indent(indentDepth, indentSpaces));
                    sb.AppendLine($"text={{{reference}}}".Indent(indentDepth, indentSpaces));
                    indentDepth--;
                    sb.AppendLine("}".Indent(indentDepth, indentSpaces));

                    indentDepth--;
                    sb.AppendLine("}".Indent(indentDepth, indentSpaces));
                }

                return sb.ToString();
            }

            public string XenonCmdText(string postset, bool asfirst, ref int indentDepth, int indentSpaces)
            {
                switch (Type)
                {
                    case ReadingResponsePart.SpokenResponse:
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("#liturgyresponsive".Indent(indentDepth, indentSpaces));
                        sb.AppendLine("{".Indent(indentDepth, indentSpaces));
                        indentDepth++;
                        sb.AppendLine(LSBResponsorialExtractor.ExtractResponsiveLiturgy(Elements, ref indentDepth, indentSpaces));
                        indentDepth--;
                        sb.Append("}".Indent(indentDepth, indentSpaces));
                        sb.Append(postset);
                        return sb.ToString();
                    case ReadingResponsePart.SungResponse:
                        return AsLitImages(postset, asfirst, ref indentDepth, indentSpaces);
                    default:
                        return "";
                }
            }
            private string AsLitImages(string postset, bool asfirst, ref int indentDepth, int indentSpaces)
            {
                StringBuilder sb = new StringBuilder();
                int i = 0;
                foreach (var image in _imagelines)
                {
                    if (asfirst && i == 0 || !asfirst && i == _imagelines.Count - 1)
                    {
                        sb.AppendLine($"#litimage({image.InferedName}){postset}".Indent(indentDepth, indentSpaces));
                    }
                    else
                    {
                        sb.AppendLine($"#litimage({image.InferedName})".Indent(indentDepth, indentSpaces));
                    }
                    i++;
                }
                return sb.ToString();
            }

            public static ReadingPartElementCollection Create(ReadingResponsePart type, List<IElement> elements)
            {
                ReadingPartElementCollection part = new ReadingPartElementCollection();
                part.Elements = elements;
                part.Type = type;
                if (type == ReadingResponsePart.SungResponse)
                {
                    foreach (var element in elements)
                    {
                        part._imagelines.AddRange(ParseResponseLitImage(element));
                    }
                }
                return part;
            }
        }

        public IEnumerable<HymnImageLine> Images
        {
            get
            {
                var preimages = PreTitle
                                    .Where(x => x.Type == ReadingResponsePart.SungResponse)
                                    .Select(x => x._imagelines)
                                    .ToList();
                var postimages = PostTitle
                                    .Where(x => x.Type == ReadingResponsePart.SungResponse)
                                    .Select(x => x._imagelines)
                                    .ToList();
                List<HymnImageLine> images = new List<HymnImageLine>();
                preimages.ForEach(x => images.AddRange(x));
                postimages.ForEach(x => images.AddRange(x));
                return images;
            }
        }
        public string PostsetCmd { get; set; }
        public IElement SourceHTML { get; private set; }

        public string ReadingTitle { get; private set; } = "";
        public string ReadingReference { get; private set; } = "";
        public string ReadingTextContent { get; private set; } = "";

        private List<ReadingPartElementCollection> PreTitle { get; set; } = new List<ReadingPartElementCollection>();
        private List<ReadingPartElementCollection> ReadingContent { get; set; } = new List<ReadingPartElementCollection>();
        private List<ReadingPartElementCollection> PostTitle { get; set; } = new List<ReadingPartElementCollection>();


        public BlockType BlockType()
        {
            return Pilot.BlockType.READING;
        }

        public string DebugString()
        {
            return $"/// XENON DEBUG::Parsed as LSB_ELEMENT_READING_COMPLEX.";
        }

        public Task GetResourcesFromLocalOrWeb(string path = "")
        {
            var tasks = Images.Select(async imageurls =>
                        {
                            if (!imageurls.Loaded)
                            {
                                await imageurls.GetResourcesFromLocalOrWeb(path);
                            }
                        });
            return Task.WhenAll(tasks);
        }

        public bool ShouldBePackaged(LSBImportOptions lSBImportOptions, out bool isResponsiveReading)
        {
            isResponsiveReading = false;
            bool fullpackage = false;
            if (lSBImportOptions.FullPackageReadings)
            {
                fullpackage = true;
            }

            // exceptions to when to full-package
            if (lSBImportOptions.InferResponsivePslamReadingsAsTitledLiturgy)
            {
                // check if we should instead fall-back to titled liturgy
                isResponsiveReading |= (ReadingTitle?.ToLower().Contains("psalm") == true);
                isResponsiveReading |= (ReadingReference?.ToLower().Contains("psalm") == true);
                isResponsiveReading |= (ReadingTitle?.ToLower().Contains("introit") == true);
                isResponsiveReading |= (ReadingReference?.ToLower().Contains("introit") == true);


                // optionally enforce strict responsive-ness
                if (lSBImportOptions.StrictlyEnforceResponsivenessForIntroits && isResponsiveReading)
                {
                    // dive inside the content and see what's going on
                    if (!ReadingContentParser.ItHasResponsoryStyledContent(ReadingContent.Where(x => x.Type == ReadingResponsePart.ReadingText || x.Type == ReadingResponsePart.Unknown)
                                                                                        .SelectMany(x => x.Elements)))
                    {
                        isResponsiveReading = false;
                    }
                }

                if (isResponsiveReading)
                {
                    fullpackage = false;
                }
            }


            // option for funeral psalm 23 exceptions
            if (lSBImportOptions.SoLikeImDoingAFuneralHereAndPsalm23sGonnaBeDoneResponsivelySoJustOverrideAnyOtherComplexReadingSettingsThatGetInTheWay)
            {
                string titlemash = (ReadingTitle + ReadingReference).ToLower();
                bool lookslikepsalm23 = titlemash.Contains("psalm") && titlemash.Contains("23");

                if (lookslikepsalm23)
                {
                    isResponsiveReading = true;
                    fullpackage = false;
                }
            }

            return fullpackage;
        }

        public string XenonAutoGen(LSBImportOptions lSBImportOptions, ref int indentDepth, int indentSpaces)
        {
            if (ShouldBePackaged(lSBImportOptions, out bool isResponsiveReading))
            {
                return FullPackageReading(lSBImportOptions, ref indentDepth, indentSpaces);
            }

            // get the postset onto the right command
            var postset = PostsetCmd.ExtractPostsetValues();

            string first = postset.first != -1 ? $"::postset(first={postset.first})" : "";
            string last = postset.last != -1 ? $"::postset(last={postset.last})" : "";

            bool prelit = PreTitle.Any();
            bool postlit = PostTitle.Any();

            string onreadingpostset = "";
            if (first != string.Empty && last != string.Empty && !prelit && !postlit)
            {
                // need to rewrite
                onreadingpostset = $"::postset(first={postset.first}, last={postset.last})";
            }
            else if (!prelit)
            {
                onreadingpostset = first;
            }
            else if (!postlit)
            {
                onreadingpostset = last;
            }



            StringBuilder sb = new StringBuilder();


            // do pre-reading 'liturgy'
            bool firstcmd = true;
            foreach (var group in PreTitle)
            {
                if (firstcmd)
                {
                    firstcmd = false;
                    sb.AppendLine($"{group.XenonCmdText(first, true, ref indentDepth, indentSpaces)}");
                }
                else
                {
                    sb.AppendLine(group.XenonCmdText("", false, ref indentDepth, indentSpaces));
                }
            }

            // do title
            //if (!(lSBImportOptions.InferResponsivePslamReadingsAsTitledLiturgy && ReadingTitle.ToLower().Contains("psalm")))
            if (!isResponsiveReading)
            {
                sb.AppendLine($"#reading(\"{ReadingTitle}\", \"{ReadingReference}\"){onreadingpostset}".Indent(indentDepth, indentSpaces));
            }
            else if (isResponsiveReading || lSBImportOptions.PullAllReadingContentAsTitledLiturgy)
            {
                sb.AppendLine("#tlit".Indent(indentDepth, indentSpaces));
                sb.AppendLine("{".Indent(indentDepth, indentSpaces));
                indentDepth++;
                sb.AppendLine();
                sb.AppendLine($"title={{{ReadingTitle}}}".Indent(indentDepth, indentSpaces));
                sb.AppendLine();
                sb.AppendLine($"title={{{ReadingReference}}}".Indent(indentDepth, indentSpaces));
                sb.AppendLine();
                sb.AppendLine("content={".Indent(indentDepth, indentSpaces));
                indentDepth++;
                sb.AppendLine(LSBResponsorialExtractor.ExtractResponsivePoetry(ReadingContent.FirstOrDefault()?.Elements ?? new List<IElement>(), ref indentDepth, indentSpaces));
                indentDepth--;
                sb.AppendLine("}".Indent(indentDepth, indentSpaces));
                indentDepth--;
                sb.AppendLine("}".Indent(indentDepth, indentSpaces));
            }

            // optional insert text
            if (lSBImportOptions.UseComplexReading && lSBImportOptions.FullTextReadings)
            {
                //... hmmmmmm
                // see if we get it at all
                sb.AppendLine("#complextext".Indent(indentDepth, indentSpaces));
                sb.AppendLine("{".Indent(indentDepth, indentSpaces));
                indentDepth++;

                sb.AppendLine($"text={{{ReadingTitle}}}".Indent(indentDepth, indentSpaces));
                sb.AppendLine($"text={{{ReadingReference}}}".Indent(indentDepth, indentSpaces));
                sb.AppendLine($"text={{ESV}}".Indent(indentDepth, indentSpaces)); // TODO: alert for non-ESV readings? or auto pick from service builder acknowledgements

                sb.AppendLine("ctext={".Indent(indentDepth, indentSpaces));
                indentDepth++;

                sb.AppendLine(LSBResponsorialExtractor.ExtractPoetryReading(ReadingContent.FirstOrDefault().Elements, ref indentDepth, indentSpaces));

                indentDepth--;
                sb.AppendLine("}".Indent(indentDepth, indentSpaces));
                indentDepth--;
                sb.AppendLine("}".Indent(indentDepth, indentSpaces));
            }

            // do post-reading 'liturgy'
            int i = 0;
            foreach (var group in PostTitle)
            {
                if (i == PostTitle.Count - 1)
                {
                    sb.AppendLine($"{group.XenonCmdText(last, false, ref indentDepth, indentSpaces)}");
                }
                else
                {
                    sb.AppendLine(group.XenonCmdText("", false, ref indentDepth, indentSpaces));
                }
                i++;
            }





            return sb.ToString();
        }

        private string FullPackageReading(LSBImportOptions lSBImportOptions, ref int _indentDepth, int indentSpaces)
        {
            StringBuilder sb = new StringBuilder();

            // NOTE: local depth here for local command, since it's placed in a prefab blob.
            int indentDepth = 0;

            // styling scripts
            sb.AppendLine("#scope(readingfull)".Indent(indentDepth, indentSpaces));
            sb.AppendLine("{".Indent(indentDepth, indentSpaces));
            indentDepth++;

            sb.AppendLine("#var(\"complextext.Layout\",\"Xenon.Readings::SideBarReading2\")".Indent(indentDepth, indentSpaces));
            sb.AppendLine("#var(\"tlit.Layout\",\"Xenon.Readings::SideBarResponse2\")".Indent(indentDepth, indentSpaces));

            // setup scripts -> handled by surrounding scripted block

            // do title first without text??
            if (!PreTitle.Any())
            {

            }
            else
            {
                foreach (var group in PreTitle)
                {
                    sb.AppendLine(group.XenonCmdText(ReadingTitle, ReadingReference, ref indentDepth, indentSpaces));
                }
            }
            // handle any pre-reading content??
            // tlit but what about lit images -> uses complex text

            // create reading text
            sb.AppendLine("#complextext".Indent(indentDepth, indentSpaces));
            sb.AppendLine("{".Indent(indentDepth, indentSpaces));
            indentDepth++;

            sb.AppendLine($"text={{{ReadingTitle}}}".Indent(indentDepth, indentSpaces));
            sb.AppendLine($"text={{{ReadingReference}}}".Indent(indentDepth, indentSpaces));
            sb.AppendLine($"text={{ESV}}".Indent(indentDepth, indentSpaces)); // TODO: alert for non-ESV readings? or auto pick from service builder acknowledgements

            sb.AppendLine("ctext={".Indent(indentDepth, indentSpaces));
            indentDepth++;

            sb.AppendLine(LSBResponsorialExtractor.ExtractPoetryReading(ReadingContent.FirstOrDefault()?.Elements ?? new List<IElement>(), ref indentDepth, indentSpaces));

            indentDepth--;
            sb.AppendLine("}".Indent(indentDepth, indentSpaces));
            indentDepth--;
            sb.AppendLine("}".Indent(indentDepth, indentSpaces));

            // do post-reading responses
            foreach (var group in PostTitle)
            {
                sb.AppendLine(group.XenonCmdText(ReadingTitle, ReadingReference, ref indentDepth, indentSpaces));
            }


            // teardowns -> handled by surrounding scripted block

            indentDepth--;
            sb.AppendLine("}".Indent(indentDepth, indentSpaces));

            if (!lSBImportOptions.WrapConsecuitivePackages)
            {
                // dump command into sucripted block
                var name = System.Reflection.Assembly.GetAssembly(typeof(ExternalPrefabGenerator))
                                                                         .GetManifestResourceNames()
                                                                         .FirstOrDefault(x => x.Contains("PIPReading"));

                var stream = System.Reflection.Assembly.GetAssembly(typeof(ExternalPrefabGenerator))
                    .GetManifestResourceStream(name);

                var prefabblob = "";
                using (StreamReader sr = new StreamReader(stream))
                {
                    prefabblob = sr.ReadToEnd();
                }

                int indent = Regex.Match(prefabblob, @"^(?<indent>\$>)\$READING", RegexOptions.Multiline).Groups["indent"].Value.Length / 2;
                string inject = sb.ToString().IndentBlock(indent, indentSpaces);


                prefabblob = Regex.Replace(prefabblob, "\\$>", "    ");

                prefabblob = Regex.Replace(prefabblob, @" +\$READING", inject);

                return prefabblob.IndentBlock(_indentDepth, indentSpaces);
            }

            return sb.ToString().IndentBlock(_indentDepth, indentSpaces);
        }

        enum ReadingResponsePart
        {
            SpokenResponse,
            SungResponse,
            ReadingText,
            Unknown,
        }

        public static ILSBElement Parse(IElement element, LSBImportOptions options)
        {
            LSBElementReadingComplex reading = new LSBElementReadingComplex();
            var caption = LSBElementCaption.Parse(element.Children.First(e => e.ClassList.Contains("lsb-caption"))) as LSBElementCaption;

            // Assume only 1 content block
            // not sure if we can have multiple readings...??
            var content = element.Children.FirstOrDefault(e => e.LocalName == "lsb-content");
            if (content != null)
            {
                if (content.Children.Where(c => c.LocalName == "p").All(c => c.ClassList.Any(x => x.StartsWith("lsb-responsorial"))))
                {
                    if (options.ForcePsalmsAsTitledLiturgy)
                    {
                        // parse content
                        return LSBElementIntroit.Parse(element);
                    }
                }

                bool pre = true;
                ReadingResponsePart lasttype = ReadingResponsePart.Unknown;
                List<IElement> egroup = null;

                foreach (var child_p_tag in content.Children.Where(c => c.LocalName == "p"))
                {
                    // figure out current type
                    ReadingResponsePart currenttype = ReadingResponsePart.Unknown;
                    if (child_p_tag.ClassList.Contains("image"))
                    {
                        currenttype = ReadingResponsePart.SungResponse;
                    }
                    else if (child_p_tag.ClassList.Any(x => new string[] { "lsb-responsorial", "lsb-responsorial-continued" }.Contains(x)))
                    {
                        currenttype = ReadingResponsePart.SpokenResponse;
                    }
                    else if (child_p_tag.ClassList.Contains("body") && string.IsNullOrWhiteSpace(child_p_tag.TextContent.Trim()))
                    {
                        continue;
                    }
                    else
                    {
                        currenttype = ReadingResponsePart.ReadingText;
                    }

                    // if matches last-type add to that, or create new group and add to that
                    if (lasttype != currenttype)
                    {
                        egroup = AddGroup(reading, pre, lasttype, egroup);
                    }
                    egroup.Add(child_p_tag);
                    lasttype = currenttype;

                    if (currenttype == ReadingResponsePart.ReadingText)
                    {
                        pre = false;
                    }
                }
                AddGroup(reading, pre, lasttype, egroup);
            }
            reading.SourceHTML = element;
            reading.ReadingTitle = caption?.Caption;
            reading.ReadingReference = caption?.SubCaption;
            return reading;
        }

        private static List<IElement> AddGroup(LSBElementReadingComplex reading, bool pre, ReadingResponsePart currenttype, List<IElement> egroup)
        {
            if (egroup?.Any() == true)
            {
                if (currenttype == ReadingResponsePart.ReadingText)
                {
                    reading.ReadingContent.Add(ReadingPartElementCollection.Create(currenttype, egroup));
                }
                else if (pre)
                {
                    reading.PreTitle.Add(ReadingPartElementCollection.Create(currenttype, egroup));
                }
                else
                {
                    reading.PostTitle.Add(ReadingPartElementCollection.Create(currenttype, egroup));
                }
            }
            egroup = new List<IElement>();
            return egroup;
        }

        private static List<HymnImageLine> ParseResponseLitImage(IElement element)
        {
            List<HymnImageLine> images = new List<HymnImageLine>();
            if (element.ClassList.Contains("image"))
            {
                var pictures = element?.Children.Where(c => c.LocalName == "picture");
                int localid = 0;
                HymnImageLine imageline = new HymnImageLine();
                foreach (var picture in pictures)
                {
                    var sources = picture?.Children.Where(c => c.LocalName == "source");
                    foreach (var source in sources)
                    {
                        if (source.Attributes["media"].Value == "print")
                        {
                            imageline.PrintURL = source.Attributes["srcset"].Value;
                        }
                        else if (source.Attributes["media"].Value == "screen")
                        {
                            string src = source.Attributes["srcset"].Value;
                            var urls = src.Split(", ");
                            foreach (var url in urls)
                            {
                                if (url.Contains("retina"))
                                {
                                    string s = url.Trim();
                                    if (s.EndsWith(" 2x"))
                                    {
                                        s = s.Substring(0, s.Length - 3);
                                    }
                                    imageline.RetinaScreenURL = s.Trim();
                                }
                                else
                                {
                                    imageline.ScreenURL = url.Trim();
                                }
                            }
                        }
                        var img = picture.Children.FirstOrDefault(c => c.LocalName == "img");
                        imageline.LocalPath = img?.Attributes["src"].Value;
                        // guid for uniqueness was a bad idea since we don't seem to be able to recreate them...
                        // for now use the parser's unique id generation (though not ideal code style)
                        imageline.InferedName = $"LiturgySung_{LSBParser.elementID}-{localid++}";
                    }
                    images.Add(imageline);
                }
            }
            return images;
        }




    }
}
