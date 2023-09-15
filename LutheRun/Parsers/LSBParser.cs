using AngleSharp.Dom;
using AngleSharp.Html.Parser;

using LutheRun.Elements;
using LutheRun.Elements.Interface;
using LutheRun.Elements.LSB;
using LutheRun.Generators;
using LutheRun.Parsers.DataModel;
using LutheRun.Pilot;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Xenon.Helpers;

namespace LutheRun.Parsers
{


    public class LSBParser
    {

        private StringBuilder stringBuilder = new StringBuilder();

        public List<ParsedLSBElement> ServiceElements { get; private set; } = new List<ParsedLSBElement>();

        //public List<ParsedLSBElement> ParsedServiceElements { get; private set; } = new List<ParsedLSBElement>();


        public List<IElement> TopLevelServiceElements { get; private set; }

        public IElement HTMLHead { get; private set; }

        public string XenonText { get; private set; }

        private string ServiceFileName;

        private static int m_elementID = 0;
        public static int elementID { get => m_elementID++; }

        public LSBImportOptions LSBImportOptions { get; set; }


        internal void Clear()
        {
            stringBuilder.Clear();
            ServiceElements.Clear();
        }

        public void Serviceify(LSBImportOptions options)
        {
            int i = 0;
            ServiceElements = Serviceifier.NormalizeHeaddingsToCaptions(ServiceElements, options)
                                          .MarkCommunionHymns(options)
                                          .StripEarlyServiceOnly(options)
                                          .Filter(options)
                                          .ExpandifyElements(options)
                                          .AddAdditionalInferedElements(options)
                                          .Filter(options)
                                          .Select(x =>
                                          {
                                              x.ElementOrder = i++;
                                              return x;
                                          })
                                          .ToList()
                                          .PlanFlight(options);
        }

        public string XenonDebug()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var se in ServiceElements)
            {
                sb.AppendLine(se.LSBElement.DebugString());
            }
            return sb.ToString();
        }

        public async Task ParseHTML(string path)
        {
            m_elementID = 0;

            ServiceFileName = path;
            string html = "";
            try
            {
                using (TextReader reader = new StreamReader(path))
                {
                    html = await reader.ReadToEndAsync();
                    await ParseHTMLIntoDOM(html);
                }
            }
            catch (Exception)
            {
            }
        }

        private async Task ParseHTMLIntoDOM(string html)
        {
            var parser = new HtmlParser();
            var document = await parser.ParseDocumentAsync(html);

            var head = document.All.Where(e => e.LocalName == "head").FirstOrDefault();
            HTMLHead = head;

            var bulletin = document.All.Where(e => e.Id == "bulletin_surface").FirstOrDefault();
            if (bulletin != null)
            {
                TopLevelServiceElements = bulletin.Children.First().Children.First().Children.First().Children.Where(e => e.LocalName == "lsb-service-element").ToList();
                IElement acknowledgments = bulletin.Children.First().Children.First().Children.First().Children.Where(e => e.LocalName == "acknowledgments").FirstOrDefault();
                ParseDOMElements(TopLevelServiceElements, acknowledgments);
            }

        }

        private void ParseDOMElements(List<IElement> dom, IElement ack)
        {
            List<ILSBElement> elements = new List<ILSBElement>();
            ServiceElements.Clear();
            foreach (var e in dom)
            {
                _ParseLSBServiceElement(e, Guid.Empty);
            }
            ServiceElements.Add(new ParsedLSBElement
            {
                LSBElement = LSBElementAcknowledments.Parse(ack),
                FilterFromOutput = false,
                Generator = "AutoGen Ack",
                SourceElements = ack.ItemAsEnumerable(),
                ParentSourceElement = ack,
                Ancestory = Guid.NewGuid(),
            });
        }


        internal bool _ParseLSBServiceElement(IElement element, Guid parTrack, IElement parent = null, string parGen = "[Top LSB Element]")
        {
            // for a top-level element in the service generate a new id
            Guid p = parTrack == Guid.Empty ? Guid.NewGuid() : parTrack;
            var parsed = false;
            List<string> rejected = new List<string>();
            if (element.ClassList.Contains("caption"))
            {
                ServiceElements.Add(new ParsedLSBElement
                {
                    LSBElement = LSBElementCaption.Parse(element),
                    Generator = $"{parGen}; (E) => E.C:caption",
                    SourceElements = element.ItemAsEnumerable(),
                    ParentSourceElement = parent,
                    Ancestory = p,
                });
                return true;
            }
            else if (element.ClassList.Contains("heading"))
            {
                ServiceElements.Add(new ParsedLSBElement
                {
                    LSBElement = LSBElementHeading.Parse(element),
                    Generator = $"{parGen}; (E) => E.C:heading",
                    SourceElements = element.ItemAsEnumerable(),
                    ParentSourceElement = parent,
                    Ancestory = p,
                });
                return true;
            }
            else if (element.ClassList.Contains("hymn"))
            {
                ServiceElements.Add(new ParsedLSBElement
                {
                    LSBElement = LSBElementHymn.Parse(element),
                    Generator = $"{parGen}; (E) => E.C:hymn",
                    SourceElements = element.ItemAsEnumerable(),
                    ParentSourceElement = parent,
                    Ancestory = p,
                });
                return true;
            }
            else if (element.ClassList.Contains("reading"))
            {
                if (LSBImportOptions.UseComplexReading)
                {
                    ServiceElements.Add(new ParsedLSBElement
                    {
                        LSBElement = LSBElementReadingComplex.Parse(element, LSBImportOptions),
                        Generator = $"{parGen}; (E) => E.C:reading [Flag=UseComplexReading]",
                        SourceElements = element.ItemAsEnumerable(),
                        ParentSourceElement = parent,
                        Ancestory = p,
                    });
                }
                else
                {
                    ServiceElements.Add(new ParsedLSBElement
                    {
                        LSBElement = LSBElementReading.Parse(element),
                        Generator = $"{parGen}; (E) => E.C:reading",
                        SourceElements = element.ItemAsEnumerable(),
                        ParentSourceElement = parent,
                        Ancestory = p,
                    });
                }
                return true;
            }
            else if (element.ClassList.Contains("prayer"))
            {
                parsed = _ParseLSBPrayerElement(element, $"{parGen}; (E) => E.C:prayer", parent, p);
                if (!parsed)
                {
                    rejected.Add("(E) => E.C:prayer | REJECTED");
                }
            }
            else if (element.ClassList.Contains("proper"))
            {
                parsed = _ParseLSBProperElement(element, $"{parGen}; (E) => E.C:proper", parent, p);
                if (!parsed)
                {
                    rejected.Add("(E) => E.C:proper | REJECTED");
                }
            }
            else if (element.ClassList.Contains("static"))
            {
                parsed = _ParseLSBStaticElement(element, $"{parGen}; (E) => E.C:static", parent, p);
                if (!parsed)
                {
                    rejected.Add("(E) => E.C:static | REJECTED");
                }
            }
            else if (element.ClassList.Contains("shared-role"))
            {
                parsed = _ParseLSBStaticElement(element, $"{parGen}; (E) => E.C:shared-role", parent, p);
                if (!parsed)
                {
                    rejected.Add("(E) => E.C:shared-role | REJECTED");
                }
            }
            else if (element.ClassList.Contains("option-group"))
            {
                parsed = _ParseLSBOptionGroupElement(element, $"{parGen}; (E) => E.C:option-group", parent, p);
                if (!parsed)
                {
                    rejected.Add("(E) => E.C:option-group | REJECTED");
                }
            }
            else if (element.ClassList.Contains("group"))
            {
                parsed = _ParseLSBGroupElement(element, $"{parGen}; (E) => E.C:group", parent, p);
                if (!parsed)
                {
                    rejected.Add("(E) => E.C:group | REJECTED");
                }
            }
            else if (element.ClassList.Contains("rite-like-group"))
            {
                parsed = _ParseLSBRiteLikeGroupElement(element, $"{parGen}; (E) => E.C:rite-like-group", parent, p);
                if (!parsed)
                {
                    rejected.Add("(E) => E.C:rite-like-group | REJECTED");
                }
            }
            // capture it as unknown
            if (!parsed)
            {
                ServiceElements.Add(new ParsedLSBElement
                {
                    LSBElement = LSBElementUnknown.Parse(element),
                    Generator = $"(E) => E.C disptach fell through to default after :: {string.Join(',', rejected)}",
                    SourceElements = element.ItemAsEnumerable(),
                    ParentSourceElement = parent,
                    Ancestory = p,
                });
            }
            return parsed;
        }

        private bool _ParseLSBRiteLikeGroupElement(IElement element, string parGen, IElement parent, Guid p)
        {
            foreach (var subelement in element.Children.Where(c => c.ClassList.Contains("static")))
            {
                _ParseLSBServiceElement(subelement, p, parent, $"{parGen}; (E) => E.Children.where(c=>c.C:static) => c");
            }
            return true;
        }

        private bool _ParseLSBPrayerElement(IElement element, string parGen, IElement parent, Guid p)
        {
            // capture known prayers we have prefab slides for
            if (ParseAsPrefab(element, parGen, parent, p))
            {
                return true;
            }
            // otherwise just spit it out as liturgy
            return _ParseLSBElementIntoLiturgy(element, "(E) => E isnt prefab", parent, p);
        }

        private bool _ParseLSBProperElement(IElement element, string parGen, IElement parent, Guid p)
        {
            if (ParseAsPrefab(element, parGen, parent, p))
            {
                return true;
            }
            if (ParsePropperAsFullMusic(element, parGen, parent, p))
            {
                return true;
            }
            if (ParsePropperAsIntroit(element, parGen, parent, p))
            {
                return true;
            }
            return false;
        }

        private bool _ParseLSBStaticElement(IElement element, string parGen, IElement parent, Guid p)
        {
            if (ParseAsPrefab(element, parGen, parent, p))
            {
                return true;
            }
            if (ParsePropperAsFullMusic(element, parGen, parent, p))
            {
                return true;
            }
            return _ParseLSBElementIntoLiturgy(element, parGen, parent, p);
        }

        private bool _ParseLSBOptionGroupElement(IElement element, string parGen, IElement parent, Guid p)
        {
            // only parse selected element if in option group
            var selectedelement = element.Children.Where(c => c.LocalName == "lsb-service-element" && c.ClassList.Contains("selected"));
            foreach (var elem in selectedelement)
            {
                _ParseLSBServiceElement(elem, p, parent, $"{parGen}; (E) => E.Children.where(c=>c.LN=lsb-service-element AND c.C:selected) => c");
            }
            return true;
        }

        private bool _ParseLSBGroupElement(IElement element, string parGen, IElement parent, Guid p)
        {
            var subelements = element.Children.Where(c => c.LocalName == "lsb-service-element");
            foreach (var elem in subelements)
            {
                _ParseLSBServiceElement(elem, p, parent, $"{parGen}; (E) => E.Children.where(c=>c.LN=lsb-service-element) => c");
            }
            return true;
        }

        private bool _ParseLSBElementIntoLiturgy(IElement element, string parGen, IElement parent, Guid p)
        {
            var anySuccess = false;
            foreach (var content in element.Children.Where(x => x.LocalName == "lsb-content"))
            {
                var parsed = _ParseLSBContentIntoLiturgy(content, $"{parGen}; (E) => E.Children.Where(c=>c.LN=lsb-content) => c", parent, p);
                anySuccess |= parsed;
            }
            return anySuccess;
        }


        private bool _ParseLSBContentIntoLiturgy(IElement contentelement, string parGen, IElement parent, Guid p)
        {
            if (LSBImportOptions.UseResponsiveLiturgy)
            {
                return _PraseLSBContentIntoResponsiveLiturgy(contentelement, parGen, parent, p);
            }
            List<string> ltext = new List<string>();
            List<IElement> lchildren = new List<IElement>();
            foreach (var child in contentelement.Children.Where(c => c.ClassList.Contains("lsb-responsorial") || c.ClassList.Contains("lsb-responsorial-continued") || c.ClassList.Contains("image")))
            {
                if (child.ClassList.Contains("lsb-responsorial") || child.ClassList.Contains("lsb-responsorial-continued"))
                {
                    lchildren.Add(child);
                    var l = LSBElementLiturgy.Parse(child) as LSBElementLiturgy;
                    ltext.Add(l.LiturgyText);
                }
                else if (child.ClassList.Contains("image"))
                {
                    if (ltext.Any())
                    {
                        ServiceElements.Add(new ParsedLSBElement
                        {
                            LSBElement = LSBElementLiturgy.Create(ltext.StringTogether(Environment.NewLine), contentelement),
                            Generator = $"{parGen}; (E) => E.Children.where(c => c.C:lsb-responsorial,lsb-responsorial-continued) => c",
                            SourceElements = new List<IElement>(lchildren),
                            ParentSourceElement = parent,
                            Ancestory = p,
                        });
                        ltext.Clear();
                        lchildren.Clear();
                    }
                    ServiceElements.Add(new ParsedLSBElement
                    {
                        LSBElement = LSBElementLiturgySung.Parse(child),
                        Generator = $"{parGen}; (E) => E.Children.where(c => c.C:image) => c",
                        SourceElements = child.ItemAsEnumerable(),
                        ParentSourceElement = parent,
                        Ancestory = p,
                    });
                }
            }
            if (ltext.Any())
            {
                ServiceElements.Add(new ParsedLSBElement
                {
                    LSBElement = LSBElementLiturgy.Create(ltext.StringTogether(Environment.NewLine), contentelement),
                    Generator = $"{parGen}; (E) => E.Children.where(c => c.C:lsb-responsorial,lsb-responsorial-continued) => c",
                    SourceElements = new List<IElement>(lchildren),
                    ParentSourceElement = parent,
                    Ancestory = p,
                });
                ltext.Clear();
            }
            return true;
        }

        private bool _PraseLSBContentIntoResponsiveLiturgy(IElement contentelement, string parGen, IElement parent, Guid p)
        {
            // need to handle mixed liturgy
            // but collect all groups together

            List<IElement> liturgyelements = new List<IElement>();
            bool anySuccess = false;

            void AddChunkOfLiturgy()
            {
                if (liturgyelements.Any())
                {
                    var lit = LSBElementResponsiveLiturgy.Create(contentelement, liturgyelements) as LSBElementResponsiveLiturgy;
                    if (!lit.PredictsEmptyContent())
                    {
                        ServiceElements.Add(new ParsedLSBElement
                        {
                            LSBElement = lit,
                            Generator = $"{parGen}; (E) => E.Children.where(c=>c.C:lsb-responsorial,lsb-responsorial-continued) => c",
                            SourceElements = new List<IElement>(liturgyelements),
                            ParentSourceElement = parent,
                            Ancestory = p,
                        });
                        anySuccess |= liturgyelements.Any();
                    }
                }
                liturgyelements.Clear();
            }

            foreach (var child in contentelement.Children)
            {
                if (child.ClassList.Contains("lsb-responsorial") || child.ClassList.Contains("lsb-responsorial-continued"))
                {
                    liturgyelements.Add(child);
                }
                else if (child.ClassList.Contains("image"))
                {
                    AddChunkOfLiturgy();
                    ServiceElements.Add(new ParsedLSBElement
                    {
                        LSBElement = LSBElementLiturgySung.Parse(child),
                        Generator = $"{parGen}; (E) => E.Children.where(c=>c.C:image) => c",
                        SourceElements = child.ItemAsEnumerable(),
                        ParentSourceElement = parent,
                        Ancestory = p,
                    });
                    anySuccess |= liturgyelements.Any();
                }
                else if (LSBImportOptions.AggressivelyParseInsideLSBContent)
                {
                    // handle it with the unknown block
                    AddChunkOfLiturgy();
                    if (!string.IsNullOrWhiteSpace(child.TextContent))
                    {
                        ServiceElements.Add(new ParsedLSBElement
                        {
                            LSBElement = LSBElementUnknownFromContent.Create(child, child.TextContent),
                            Generator = $"{parGen}; (E) => E.Children.where(c=>c.C:UNKNOWN) [Flags=AggressivelyParseInsideLSBContent] c.C:'{child.ClassList}' => c",
                            SourceElements = child.ItemAsEnumerable(),
                            ParentSourceElement = parent,
                            ConsiderForServicification = false,
                            Ancestory = p,
                        });
                    }
                    // don't mark a successful parse by this item alone
                    //anySuccess |= liturgyelements.Any();
                }
            }
            AddChunkOfLiturgy();

            return anySuccess;
        }



        private bool ParsePropperAsIntroit(IElement element, string parGen, IElement parent, Guid p)
        {
            var caption = LSBElementCaption.Parse(element) as LSBElementCaption;
            if (caption == null || caption?.Caption == string.Empty)
            {
                return false;
            }

            if (!caption.Caption.ToLower().Contains("introit"))
            {
                return false;
            }

            ServiceElements.Add(new ParsedLSBElement
            {
                LSBElement = LSBElementIntroit.Parse(element),
                Generator = $"{parGen}; (E) => E has <Caption> containing 'introit'",
                SourceElements = element.ItemAsEnumerable(),
                ParentSourceElement = parent,
                Ancestory = p,
            });

            return true;
        }


        private bool ParsePropperAsFullMusic(IElement element, string parGen, IElement parent, Guid p)
        {
            var caption = LSBElementCaption.Parse(element) as LSBElementCaption;
            if (caption != null && caption?.Caption != string.Empty)
            {
                // reasonably sure this might have something interesting
                // we're expecting multiple images in the content
                foreach (var content in element.Children.Where(c => c.LocalName == "lsb-content"))
                {
                    var images = content.Children.Where(c => c.ClassList.Contains("image"));
                    if (images.Count() > 1)
                    {
                        LSBElementHymn hymn = LSBElementHymn.Parse(element);
                        // check that we really want this as a hymn
                        // biggest clue is separate text lines
                        if (!hymn.HasSeperateTextLines())
                        {
                            if (hymn.Lines <= 2)
                            {
                                // lets do it as litimage instead
                                return false;
                            }
                            int variance = hymn.LineWidthVariance(Path.GetDirectoryName(ServiceFileName));
                            if (variance > 10 * hymn.Lines)
                            {
                                // pretty sure it's not one thing, but lots of sung liturgy
                                return false;
                            }
                            // in the case of some Kyrie, we have more than 2 lines, which are all the same width...
                            // so we need to check for something at this point we'll check if there's a bunch of lsb-responsorial tags
                            if (content.Children.Any(c => c.ClassList.Contains("lsb-responsorial")))
                            {
                                return false;
                            }
                        }

                        ServiceElements.Add(new ParsedLSBElement
                        {
                            LSBElement = hymn,
                            Generator = $"{parGen}; (E) => E.Children.where(c=>c.LN=lsb-content) => c && c.Children.where(x=>x.C:image)",
                            SourceElements = element.ItemAsEnumerable(),
                            ParentSourceElement = parent,
                            Ancestory = p,
                        });
                        return true;
                    }
                }
            }
            return false;
        }

        public bool ParseAsPrefab(IElement element, string parGen, IElement parent, Guid p)
        {
            var caption = LSBElementCaption.Parse(element) as LSBElementCaption;
            if (caption != null && caption?.Caption != string.Empty)
            {
                string ctext = Regex.Replace(caption.Caption, @"[^\w ]", "");
                // first check if is known element (Creed/Prayer)
                Dictionary<string, string> prefabs = new Dictionary<string, string>()
                {
                    ["Apostles Creed"] = "apostlescreed",
                    ["Nicene Creed"] = "nicenecreed",
                    ["Lords Prayer"] = "lordsprayer",
                };
                Dictionary<string, string> prefabsMkII = new Dictionary<string, string>
                {
                    ["Apostles Creed"] = "ApostlesCreed",
                    ["Nicene Creed"] = "NiceneCreed",
                    ["Lords Prayer"] = "LordsPrayer",
                };
                if (!LSBImportOptions.UseThemedCreeds)
                {
                    if (prefabs.Keys.Contains(ctext))
                    {
                        // use a prefab instead
                        ServiceElements.Add(new ParsedLSBElement
                        {
                            LSBElement = new LSBElementIsPrefab(prefabs[ctext], element.StrippedText(), element, BlockType.CREED),
                            Generator = $"{parGen}; (E) => E has <Caption>",
                            ParentSourceElement = parent,
                            Ancestory = p,
                        });
                        return true;
                    }
                }
                else
                {
                    if (prefabsMkII.Keys.Contains(ctext))
                    {
                        string creedName = prefabsMkII[ctext] + (LSBImportOptions.ThemeCreedsWithHTML ? "HTML" : "") + ".txt";
                        string reason = "[Flags=UseThemedCreeds" + (LSBImportOptions.ThemeCreedsWithHTML ? ",ThemeCreedsWithHTML]" : "]");
                        // use a prefab instead
                        ServiceElements.Add(new ParsedLSBElement
                        {
                            LSBElement = ExternalPrefabGenerator.GenerateCreed(creedName, LSBImportOptions),
                            Generator = $"{parGen}; (E) => E has <Caption> {reason}",
                            SourceElements = element.ItemAsEnumerable(),
                            ParentSourceElement = parent,
                            Ancestory = p,
                        });
                        return true;
                    }
                }
            }
            return false;
        }


        public void CompileToXenon()
        {
            XenonText = XenonGenerator.CompileToXenon(ServiceFileName, LSBImportOptions, ServiceElements);
        }

        public Task LoadWebAssets(Action<Bitmap, string, string, string> addImageAsAsset)
        {
            IEnumerable<IDownloadWebResource> resources = ServiceElements.Select(s => s?.LSBElement as IDownloadWebResource).Where(s => s != null);
            IEnumerable<Task> tasks = resources.Select(async s =>
            {
                await s.GetResourcesFromLocalOrWeb(Path.GetDirectoryName(ServiceFileName));
                foreach (var image in s.Images)
                {
                    addImageAsAsset(image.Bitmap, image.RetinaScreenURL, image.InferedName, "LSB");
                }
            });
            return Task.WhenAll(tasks);
        }

        public Task LoadAssetsForElement(Action<Bitmap, string, string, string> addImageAsAsset, IEnumerable<ILSBElement> elements)
        {
            IEnumerable<IDownloadWebResource> resources = elements.Select(s => s as IDownloadWebResource).Where(s => s != null);
            IEnumerable<Task> tasks = resources.Select(async s =>
            {
                await s.GetResourcesFromLocalOrWeb(Path.GetDirectoryName(ServiceFileName));
                foreach (var image in s.Images)
                {
                    addImageAsAsset(image.Bitmap, image.RetinaScreenURL, image.InferedName, "LSB");
                }
            });
            return Task.WhenAll(tasks);
        }

    }
}
