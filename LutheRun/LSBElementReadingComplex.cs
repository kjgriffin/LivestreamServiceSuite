using AngleSharp.Dom;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static LutheRun.LSBElementHymn;

namespace LutheRun
{

    class LSBElementReadingComplex : ILSBElement, IDownloadWebResource
    {
        class ReadingPartElementCollection
        {
            public List<IElement> Elements { get; private set; } = new List<IElement>();
            public ReadingResponsePart Type { get; private set; } = ReadingResponsePart.Unknown;

            public List<HymnImageLine> _imagelines = new List<HymnImageLine>();

            public string XenonCmdText(string postset, bool asfirst)
            {
                switch (Type)
                {
                    case ReadingResponsePart.SpokenResponse:
                        return "#liturgyresponsive"
                            + Environment.NewLine
                            + "{"
                            + Environment.NewLine
                            + LSBResponsorialExtractor.ExtractResponsiveLiturgy(Elements)
                            + Environment.NewLine
                            + "}"
                            + postset;
                    case ReadingResponsePart.SungResponse:
                        return AsLitImages(postset, asfirst);
                    default:
                        return "";
                }
            }
            private string AsLitImages(string postset, bool asfirst)
            {
                StringBuilder sb = new StringBuilder();
                int i = 0;
                foreach (var image in _imagelines)
                {
                    if ((asfirst && i == 0) || (!asfirst && i == _imagelines.Count - 1))
                    {
                        sb.AppendLine($"#litimage({image.InferedName}){postset}");
                    }
                    else
                    {
                        sb.AppendLine($"#litimage({image.InferedName})");
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

        public string XenonAutoGen(LSBImportOptions lSBImportOptions)
        {
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
                    sb.AppendLine($"{group.XenonCmdText(first, true)}");
                }
                else
                {
                    sb.AppendLine(group.XenonCmdText("", false));
                }
            }

            // do title
            sb.AppendLine($"#reading(\"{ReadingTitle}\", \"{ReadingReference}\"){onreadingpostset}");

            // skip text for now

            // do post-reading 'liturgy'
            int i = 0;
            foreach (var group in PostTitle)
            {
                if (i == PostTitle.Count - 1)
                {
                    sb.AppendLine($"{group.XenonCmdText(last, false)}");
                }
                else
                {
                    sb.AppendLine(group.XenonCmdText("", false));
                }
                i++;
            }





            return sb.ToString();
        }

        enum ReadingResponsePart
        {
            SpokenResponse,
            SungResponse,
            ReadingText,
            Unknown,
        }

        public static ILSBElement Parse(IElement element)
        {
            LSBElementReadingComplex reading = new LSBElementReadingComplex();
            var caption = LSBElementCaption.Parse(element.Children.First(e => e.ClassList.Contains("lsb-caption"))) as LSBElementCaption;

            // Assume only 1 content block
            // not sure if we can have multiple readings...??
            var content = element.Children.FirstOrDefault(e => e.LocalName == "lsb-content");
            if (content != null)
            {
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
