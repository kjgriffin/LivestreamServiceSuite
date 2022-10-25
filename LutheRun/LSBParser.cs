using AngleSharp.Dom;
using AngleSharp.Html.Parser;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

using Xenon.Helpers;

namespace LutheRun
{
    public class ParsedLSBElement
    {
        public ILSBElement LSBElement { get; set; } = null;
        public string Generator { get; set; } = "";
        public string XenonCode { get; set; } = "";
        public IEnumerable<IElement> SourceElements { get; set; } = null;
        public IElement ParentSourceElement { get; set; } = null;
        public bool FilterFromOutput { get; set; } = false;
        public bool AddedByInference { get; set; } = false;
        internal BlockType BlockType { get; set; } = BlockType.UNKNOWN;
        internal CameraUsage CameraUse { get; set; } = new CameraUsage();
    }


    public class LSBParser
    {

        private StringBuilder stringBuilder = new StringBuilder();

        public List<ParsedLSBElement> ServiceElements { get; private set; } = new List<ParsedLSBElement>();

        //public List<ParsedLSBElement> ParsedServiceElements { get; private set; } = new List<ParsedLSBElement>();


        public List<IElement> TopLevelServiceElements { get; private set; }

        public IElement HTMLHead { get; private set; }

        public string XenonText { get => stringBuilder.ToString(); }

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
            ServiceElements = Serviceifier.NormalizeHeaddingsToCaptions(ServiceElements, options)
                                          .AddAdditionalInferedElements(options)
                                          .Filter(options)
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

        private void ParseDOMElements(List<AngleSharp.Dom.IElement> dom, IElement ack)
        {
            List<ILSBElement> elements = new List<ILSBElement>();
            ServiceElements.Clear();
            foreach (var e in dom)
            {
                _ParseLSBServiceElement(e);
            }
            ServiceElements.Add(new ParsedLSBElement
            {
                LSBElement = LSBElementAcknowledments.Parse(ack),
                FilterFromOutput = false,
                Generator = "AutoGen Ack",
                SourceElements = ack.ItemAsEnumerable(),
                ParentSourceElement = ack,
            });
        }

        private void ParseLSBServiceElement(IElement element, string previous = "[Is Top-Level LSB Element]", IElement parent = null)
        {
            /*
            bool checkforselection = false;
            if (element.ClassList.Contains("option-group"))
            {
                checkforselection = true;
            }
            if (element.Children.All(x => x.LocalName == "lsb-service-element"))
            {
                bool allseperate = true;
                if (checkforselection)
                {
                    allseperate = false;
                    if (element.Children.All(x => !x.ClassList.Contains("selected")))
                    {
                        allseperate = true;
                    }
                }
                if (allseperate)
                {
                    foreach (var child in element.Children)
                    {
                        ParseLSBServiceElement(child);
                    }
                }
                return;
            }
            */

            if (element.LocalName == "lsb-content")
            {
                if (element.Children.Any(c => c.ClassList.Contains("lsb-responsorial")))
                {
                    ServiceElements.Add(new ParsedLSBElement
                    {
                        LSBElement = LSBElementLiturgy.Parse(element),
                        Generator = $"{previous}; E=lsb-content with C:lsb-responsorial",
                        SourceElements = element.ItemAsEnumerable(),
                        ParentSourceElement = parent ?? element,
                    });
                }
                foreach (var imageline in element.Children.Where(c => c.ClassList.Contains("image")))
                {
                    ServiceElements.Add(new ParsedLSBElement
                    {
                        LSBElement = LSBElementLiturgySung.Parse(imageline),
                        Generator = $"{previous}; E<-lsb-content with C:image",
                        SourceElements = imageline.ItemAsEnumerable(),
                        ParentSourceElement = parent ?? element,
                    });
                }
            }
            else
            {

                if (element.ClassList.Contains("heading"))
                {
                    ServiceElements.Add(new ParsedLSBElement
                    {
                        LSBElement = LSBElementHeading.Parse(element),
                        Generator = $"{previous}; E!=lsb-content with C:heading",
                        ParentSourceElement = element,
                        SourceElements = parent.ItemAsEnumerable() ?? element.ItemAsEnumerable(),
                    });
                }
                else if (element.ClassList.Contains("caption"))
                {
                    // TODO: do something different for anthem-titles, sermon-titles, postlude/preludes. Ignore others???
                    ServiceElements.Add(new ParsedLSBElement
                    {
                        LSBElement = LSBElementCaption.Parse(element),
                        Generator = $"{previous} E!=lsb-content with C:caption",
                        ParentSourceElement = element,
                        SourceElements = parent.ItemAsEnumerable() ?? element.ItemAsEnumerable(),
                    });
                }
                else if (element.ClassList.Contains("static"))
                {
                    if (ParseAsPrefab(element, "TODO", parent))
                    {
                        return;
                    }

                    if (ParsePropperAsFullMusic(element, "TODO", parent))
                    {
                        return;
                    }

                    // is liturgy responsorial if it contains lsb-content elements
                    foreach (var child in element.Children.Where(c => c.LocalName == "lsb-content"))
                    {
                        ParseLSBServiceElement(child, "E!=lsb-content with E.Children.LN=lsb-content => E", element);
                    }
                }
                else if (element.ClassList.Contains("reading"))
                {
                    if (LSBImportOptions.UseComplexReading)
                    {
                        ServiceElements.Add(new ParsedLSBElement
                        {
                            LSBElement = LSBElementReadingComplex.Parse(element, LSBImportOptions),
                            Generator = "E!=lsb-content with C:reading [flag=UseComplexReading]",
                            SourceElements = element.ItemAsEnumerable(),
                            ParentSourceElement = parent ?? element,
                        });
                    }
                    else
                    {
                        ServiceElements.Add(new ParsedLSBElement
                        {
                            LSBElement = LSBElementReading.Parse(element),
                            Generator = "E!=lsb-content with C:reading [flag=!UseComplexReading]",
                            SourceElements = element.ItemAsEnumerable(),
                            ParentSourceElement = parent ?? element,
                        });
                    }
                }
                else if (element.ClassList.Contains("hymn"))
                {
                    ServiceElements.Add(new ParsedLSBElement
                    {
                        LSBElement = LSBElementHymn.Parse(element),
                        Generator = "E!=lsb-content with C:hymn",
                        SourceElements = element.ItemAsEnumerable(),
                        ParentSourceElement = parent ?? element,
                    });
                }
                else if (element.ClassList.Contains("option-group") || element.ClassList.Contains("group"))
                {
                    var singlechild = element.Children.Where(c => c.LocalName == "lsb-service-element");
                    if (singlechild.Count() == 1)
                    {
                        if (singlechild.First().ClassList.Contains("static"))
                        {
                            ParseLSBServiceElement(singlechild.First(), "E!=lsb-content with C:option-group|group && E.Children.Where=>E.LN=lsb-service-element && E.C:static", element);
                            return;
                        }
                    }

                    // select all sub lsb-elements what are group selected
                    var selectedgroup = element.Children.Where(c => c.LocalName == "lsb-service-element" && c.ClassList.Contains("selected"));
                    foreach (var selectedelement in selectedgroup)
                    {
                        if (selectedelement.ClassList.Contains("group"))
                        {
                            foreach (var lsbserviceelement in selectedelement.Children.Where(c => c.LocalName == "lsb-service-element"))
                            {
                                ParseLSBServiceElement(lsbserviceelement, "E!=lsb-content with C:option-group|group && E.Children.Where=>E.LN=lsb-service-element && E.C:group then E.Children.Where=>E.LN=lsb-service-element", element);
                            }
                        }
                        else if (selectedelement.ClassList.Contains("static"))
                        {
                            ParseLSBServiceElement(selectedelement, "E!=lsb-content with C:option-group|group && E.Children.Where=>E.LN=lsb-service-element && E.C:static", element);
                        }
                        else if (selectedelement.ClassList.Contains("prayer"))
                        {
                            foreach (var c in selectedelement.Children.Where(x => x.LocalName == "lsb-content"))
                            {
                                ParseLSBServiceElement(c, "E!=lsb-content with C:option-group|group && E.Children.Where=>E.LN=lsb-service-element && E.C:prayer then E.Children.Where=>E.LN=lsb-content", element);
                            }
                        }
                    }
                }
                else if (element.ClassList.Contains("proper"))
                {
                    if (ParseAsPrefab(element, "TODO", parent))
                    {
                        return;
                    }
                    if (ParsePropperAsFullMusic(element, "TODO", parent) == true)
                    {
                        return;
                    }
                }
                else if (element.ClassList.Contains("prayer"))
                {
                    if (ParseAsPrefab(element, "TODO", parent))
                    {
                        return;
                    }
                    foreach (var c in element.Children.Where(x => x.LocalName == "lsb-content"))
                    {
                        ParseLSBServiceElement(c, "E!=lsb-content with E.C:prayer then E.Children.Where=>E.LN=lsb-content", element);
                    }
                }
                else
                {
                    ServiceElements.Add(new ParsedLSBElement
                    {
                        LSBElement = LSBElementUnknown.Parse(element),
                        Generator = "E!=lsb-content default unknown",
                        ParentSourceElement = element,
                        SourceElements = parent.ItemAsEnumerable() ?? element.ItemAsEnumerable(),
                    });
                }
            }
        }





        internal bool _ParseLSBServiceElement(IElement element, IElement parent = null, string parGen = "[Top LSB Element]")
        {
            if (element.ClassList.Contains("caption"))
            {
                ServiceElements.Add(new ParsedLSBElement
                {
                    LSBElement = LSBElementCaption.Parse(element),
                    Generator = $"{parGen}; (E) => E.C:caption",
                    SourceElements = element.ItemAsEnumerable(),
                    ParentSourceElement = parent,
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
                    });
                }
                return true;
            }
            else if (element.ClassList.Contains("prayer"))
            {
                return _ParseLSBPrayerElement(element, $"{parGen}; (E) => E.C:prayer", parent);
            }
            else if (element.ClassList.Contains("proper"))
            {
                return _ParseLSBProperElement(element, $"{parGen}; (E) => E.C:proper", parent);
            }
            else if (element.ClassList.Contains("static"))
            {
                return _ParseLSBStaticElement(element, $"{parGen}; (E) => E.C:static", parent);
            }
            else if (element.ClassList.Contains("shared-role"))
            {
                return _ParseLSBStaticElement(element, $"{parGen}; (E) => E.C:shared-role", parent);
            }
            else if (element.ClassList.Contains("option-group"))
            {
                return _ParseLSBOptionGroupElement(element, $"{parGen}; (E) => E.C:option-group", parent);
            }
            else if (element.ClassList.Contains("group"))
            {
                return _ParseLSBGroupElement(element, $"{parGen}; (E) => E.C:group", parent);
            }
            else if (element.ClassList.Contains("rite-like-group"))
            {
                return _ParseLSBRiteLikeGroupElement(element, $"{parGen}; (E) => E.C:rite-like-group", parent);
            }
            ServiceElements.Add(new ParsedLSBElement
            {
                LSBElement = LSBElementUnknown.Parse(element),
                Generator = "(E) => E.C disptach fell through to default",
                SourceElements = element.ItemAsEnumerable(),
                ParentSourceElement = parent,
            });
            return false;
        }

        private bool _ParseLSBRiteLikeGroupElement(IElement element, string parGen, IElement parent)
        {
            foreach (var subelement in element.Children.Where(c => c.ClassList.Contains("static")))
            {
                _ParseLSBServiceElement(subelement, parent, $"{parGen}; (E) => E.Children.where(c=>c.C:static) => c");
            }
            return true;
        }

        private bool _ParseLSBPrayerElement(IElement element, string parGen, IElement parent)
        {
            // capture known prayers we have prefab slides for
            if (ParseAsPrefab(element, parGen, parent))
            {
                return true;
            }
            // otherwise just spit it out as liturgy
            return _ParseLSBElementIntoLiturgy(element, "(E) => E isnt prefab", parent);
        }

        private bool _ParseLSBProperElement(IElement element, string parGen, IElement parent)
        {
            if (ParseAsPrefab(element, parGen, parent))
            {
                return true;
            }
            if (ParsePropperAsFullMusic(element, parGen, parent))
            {
                return true;
            }
            if (ParsePropperAsIntroit(element, parGen, parent))
            {
                return true;
            }
            return false;
        }

        private bool _ParseLSBStaticElement(IElement element, string parGen, IElement parent)
        {
            if (ParseAsPrefab(element, parGen, parent))
            {
                return true;
            }
            if (ParsePropperAsFullMusic(element, parGen, parent))
            {
                return true;
            }
            return _ParseLSBElementIntoLiturgy(element, parGen, parent);
        }

        private bool _ParseLSBOptionGroupElement(IElement element, string parGen, IElement parent)
        {
            // only parse selected element if in option group
            var selectedelement = element.Children.Where(c => c.LocalName == "lsb-service-element" && c.ClassList.Contains("selected"));
            foreach (var elem in selectedelement)
            {
                _ParseLSBServiceElement(elem, parent, $"{parGen}; (E) => E.Children.where(c=>c.LN=lsb-service-element AND c.C:selected) => c");
            }
            return true;
        }

        private bool _ParseLSBGroupElement(IElement element, string parGen, IElement parent)
        {
            var subelements = element.Children.Where(c => c.LocalName == "lsb-service-element");
            foreach (var elem in subelements)
            {
                _ParseLSBServiceElement(elem, parent, $"{parGen}; (E) => E.Children.where(c=>c.LN=lsb-service-element) => c");
            }
            return true;
        }

        private bool _ParseLSBElementIntoLiturgy(IElement element, string parGen, IElement parent)
        {
            foreach (var content in element.Children.Where(x => x.LocalName == "lsb-content"))
            {
                _ParseLSBContentIntoLiturgy(content, $"{parGen}; (E) => E.Children.Where(c=>c.LN=lsb-content) => c", parent);
            }
            return true;
        }


        private bool _ParseLSBContentIntoLiturgy(IElement contentelement, string parGen, IElement parent)
        {
            if (LSBImportOptions.UseResponsiveLiturgy)
            {
                return _PraseLSBContentIntoResponsiveLiturgy(contentelement, parGen, parent);
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
                });
                ltext.Clear();
            }
            return true;
        }

        private bool _PraseLSBContentIntoResponsiveLiturgy(IElement contentelement, string parGen, IElement parent)
        {
            // need to handle mixed liturgy
            // but collect all groups together

            List<IElement> liturgyelements = new List<IElement>();

            void AddChunkOfLiturgy()
            {
                if (liturgyelements.Any())
                {
                    ServiceElements.Add(new ParsedLSBElement
                    {
                        LSBElement = LSBElementResponsiveLiturgy.Create(contentelement, liturgyelements),
                        Generator = $"{parGen}; (E) => E.Children.where(c=>c.C:lsb-responsorial,lsb-responsorial-continued) => c",
                        SourceElements = new List<IElement>(liturgyelements),
                        ParentSourceElement = parent,
                    });
                }
                liturgyelements.Clear();
            }

            foreach (var child in contentelement.Children.Where(c => c.ClassList.Contains("lsb-responsorial") || c.ClassList.Contains("lsb-responsorial-continued") || c.ClassList.Contains("image")))
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
                    });
                }
            }
            AddChunkOfLiturgy();

            return true;
        }



        private bool ParsePropperAsIntroit(IElement element, string parGen, IElement parent)
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
            });

            return true;
        }


        private bool ParsePropperAsFullMusic(IElement element, string parGen, IElement parent)
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
                        });
                        return true;
                    }
                }
            }
            return false;
        }

        public bool ParseAsPrefab(IElement element, string parGen, IElement parent)
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
                            Generator = $"{parGen}; (E) => E has <Caption> [Flags=UseThemedCreeds]",
                            SourceElements = element.ItemAsEnumerable(),
                            ParentSourceElement = parent,
                        });
                        return true;
                    }
                }
                else
                {
                    if (prefabsMkII.Keys.Contains(ctext))
                    {
                        // use a prefab instead
                        ServiceElements.Add(new ParsedLSBElement
                        {
                            LSBElement = ExternalPrefabGenerator.GenerateCreed(prefabsMkII[ctext], LSBImportOptions),
                            Generator = $"{parGen}; (E) => E has <Caption>",
                            SourceElements = element.ItemAsEnumerable(),
                            ParentSourceElement = parent,
                        });
                        return true;
                    }
                }
            }
            return false;
        }

        internal static Dictionary<string, string> ApplySeasonalMacroText(List<ParsedLSBElement> service, Dictionary<string, string> macros)
        {
            Dictionary<string, string> newmacros = new Dictionary<string, string>(macros);
            // determine service date

            // try and find a caption/heading the looks like a date (this may not be the strongest way to do it...)
            var candidateStrings = service.Select(x => (x.LSBElement as LSBElementCaption)?.Caption).Where(x => !string.IsNullOrEmpty(x)).ToList();

            var candidateDates = new List<DateTime>();

            foreach (var cdstring in candidateStrings)
            {
                if (DateTime.TryParse(cdstring, out var date))
                {
                    candidateDates.Add(date);
                }
            }

            var ldate = candidateDates.FirstOrDefault();
            if (ldate != null)
            {
                var keydates = GetKeySeasonDatesAsync();


                // try and find the 'most relevant' key date
                // this will be the earliest date that's equal to or later than the service
                var sdate = keydates.OrderByDescending(x => x.StartDate).FirstOrDefault(x => x.StartDate <= ldate);

                if (sdate != null)
                {
                    // add all macros
                    foreach (var macro in sdate.macros)
                    {
                        newmacros[macro.Key] = macro.Value;
                    }
                }

            }

            return newmacros;
        }

        class SeasonKeyDefinition
        {
            public string start { get; set; } = "";
            public string note { get; set; } = "";
            public Dictionary<string, string> macros { get; set; } = new Dictionary<string, string>();

            [JsonIgnore]
            public DateTime? StartDate
            {
                get
                {
                    if (DateTime.TryParse(start, out var date))
                    {
                        return date;
                    }
                    return null;
                }
            }
        }

        private static List<SeasonKeyDefinition> GetKeySeasonDatesAsync()
        {
            // try and find info for this date
            const string dateLookupURL = "https://raw.githubusercontent.com/kjgriffin/LivestreamServiceSuite/seasons/BlobData/seasons.json";

            List<SeasonKeyDefinition> res = new List<SeasonKeyDefinition>();

            try
            {
                System.Diagnostics.Debug.WriteLine($"Fetching image {dateLookupURL} from web.");
                var resp = WebHelpers.httpClient.GetAsync(dateLookupURL).Result?.Content;
                var json = resp.ReadAsStringAsync().Result;
                res = JsonSerializer.Deserialize<List<SeasonKeyDefinition>>(json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed trying to download: {dateLookupURL}\r\n{ex}");
            }

            return res;
        }


        public void CompileToXenon()
        {
            int indentDepth = 0;
            int indentSpace = 4;
            stringBuilder.Clear();
            stringBuilder.Append($"\r\n////////////////////////////////////\r\n// XENON AUTO GEN: From Service File '{System.IO.Path.GetFileName(ServiceFileName)}'\r\n////////////////////////////////////\r\n\r\n");

            if (LSBImportOptions.UseThemedHymns || LSBImportOptions.UseThemedCreeds)
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine("#scope(LSBService)".Indent(indentDepth, indentSpace));
                stringBuilder.AppendLine("{".Indent(indentDepth, indentSpace));
                indentDepth++;
                stringBuilder.AppendLine();
                stringBuilder.AppendLine($"#var(\"stitchedimage.Layout\", \"{LSBImportOptions.ServiceThemeLib}::SideBar\")".Indent(indentDepth, indentSpace));
                stringBuilder.AppendLine($"#var(\"texthymn.Layout\", \"{LSBImportOptions.ServiceThemeLib}::SideBar\")".Indent(indentDepth, indentSpace));
                stringBuilder.AppendLine();


                stringBuilder.AppendLine($"/// </MANUAL_UPDATE name='Theme Colors'>".Indent(indentDepth, indentSpace));
                stringBuilder.AppendLine($"// See: https://github.com/kjgriffin/LivestreamServiceSuite/wiki/Themes".Indent(indentDepth, indentSpace));

                Dictionary<string, string> macros = LSBImportOptions.Macros;

                if (LSBImportOptions.InferSeason)
                {
                    macros = LSBParser.ApplySeasonalMacroText(ServiceElements, macros);
                }

                // macros!
                foreach (var macro in macros)
                {
                    stringBuilder.AppendLine($"#var(\"{LSBImportOptions.ServiceThemeLib}@{macro.Key}\", ```{macro.Value}```)".Indent(indentDepth, indentSpace));
                }
                stringBuilder.AppendLine();

            }

            foreach (var se in ServiceElements)
            {
                if (!se.FilterFromOutput)
                {
                    stringBuilder.AppendLine(se.LSBElement.XenonAutoGen(LSBImportOptions, ref indentDepth, indentSpace));
                }
            }

            if (LSBImportOptions.UseThemedHymns)
            {
                indentDepth--;
                stringBuilder.AppendLine("}");
            }
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
