using LutheRun.Elements;
using LutheRun.Elements.LSB;
using LutheRun.Parsers;
using LutheRun.Parsers.DataModel;
using LutheRun.Pilot;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LutheRun
{
    static class Serviceifier
    {

        /*
            Main Goals:

            1. auto detect other components to services that would otherwise be needed...
            
            - add titlepage
            - add copyright
            - at end of liturgy type segements insert liturgyoff commands
            - before hymns add organintro commands
         */

        // TODO: make this a bit less hardcoded
        internal enum Camera
        {
            Unset = -1,
            Pulpit = 8,
            Center = 7,
            Lectern = 6,
            Organ = 5,
            Slide = 4,
            Key = 3,
            Proj = 2,
            Cam1 = 1,
        }

        public static List<Type> LiturgyElements = new List<Type>()
        {
            typeof(LSBElementResponsiveLiturgy),
            typeof(LSBElementLiturgy),
            typeof(LSBElementLiturgySung),
            typeof(LSBElementReading),
            typeof(LSBElementReadingComplex),
            typeof(LSBElementCaption),
            typeof(LSBElementIntroit),
        };


        public static List<ParsedLSBElement> Filter(this List<ParsedLSBElement> service, LSBImportOptions options)
        {
            return service.Select(x =>
            {
                if (options.Filter.FilteredTypes.Contains(x.LSBElement.GetType()))
                {
                    return x;
                }
                x.FilterFromOutput = true;
                x.Generator += "; ===> REMOVED [filtered exclusion]";
                return x;
            }).ToList();
        }

        public static List<ParsedLSBElement> NormalizeHeaddingsToCaptions(this List<ParsedLSBElement> service, LSBImportOptions options)
        {
            return service.Select(x =>
            {
                var y = x.LSBElement as LSBElementHeading;
                if (y != null)
                {
                    return new ParsedLSBElement
                    {
                        FilterFromOutput = x.FilterFromOutput,
                        Generator = $"{x.Generator}; ==> Normalized from Heading to Caption",
                        ParentSourceElement = x.ParentSourceElement,
                        SourceElements = x.SourceElements,
                        XenonCode = x.XenonCode,
                        AddedByInference = x.AddedByInference,
                        LSBElement = LSBElementCaption.FromHeading(y),
                    };
                }
                else
                {
                    return x;
                }
            }).ToList();
        }

        public static List<ParsedLSBElement> RemoveUnusedElement(this List<ParsedLSBElement> service, LSBImportOptions options)
        {
            List<ParsedLSBElement> trimmed = new List<ParsedLSBElement>();

            // rely on filter to remove extra junk
            // only use here is to remove unknown captions
            // allow earlier filtering to rule
            foreach (var element in service.Where(x => !x.FilterFromOutput))
            {
                var caption = element.LSBElement as LSBElementCaption;
                if (caption != null)
                {
                    if (options.OnlyKnownCaptions)
                    {
                        // removed 'postlude' since it's now handled via end title
                        if (new[] { "bells", "prelude", "anthem", "sermon" }.Any(c => caption.Caption.ToLower().Contains(c)))
                        {
                            //trimmed.Add(element);
                            element.FilterFromOutput = false;
                            element.Generator += "; ===> INCLUDED [servicifier known caption]";
                        }
                        else if (!options.OnlyKnownCaptions)
                        {
                            //trimmed.Add(element);
                            element.FilterFromOutput = false;
                            element.Generator += "; ===> INCLUDED [servicifier known caption]";
                        }
                        //continue;
                        else
                        {
                            element.FilterFromOutput = true;
                            element.Generator += "; ===> REMOVED [unused element]";
                        }
                    }
                    else
                    {
                        //trimmed.Add(element);
                    }
                }

                //trimmed.Add(element);
            }

            //return trimmed;
            return service;
        }


        public static List<ParsedLSBElement> MarkCommunionHymns(this List<ParsedLSBElement> service, LSBImportOptions options)
        {
            // go through service
            // if we see a caption about distribution we know communion hymns follow
            bool communion = false;
            bool foundCHymns = false;
            foreach (var elem in service)
            {
                var cap = elem.LSBElement as LSBElementCaption;
                if (cap != null && cap.Caption.ToLower().Contains("distribution"))
                {
                    communion = true;
                }

                if (communion)
                {
                    var hymn = elem.LSBElement as LSBElementHymn;

                    if (hymn != null)
                    {
                        foundCHymns = true;
                        hymn.IsCommunionHymn = true;
                        elem.Generator += "; marked for distribution-> follows 'distribution' caption, before suspected end of communion";
                    }

                    if (foundCHymns) // allow elements other than hymns between indication of distribution and the first hymn
                    {
                        // once we find a hymn then we will enforce strict checks
                        if (elem.LSBElement is not LSBElementHymn && (elem.LSBElement as ExternalPrefab)?.TypeIdentifier != "upnext")
                        {
                            // probably no-longer in distribution segment
                            communion = false;
                        }
                    }
                }

            }


            return service;
        }

        public static List<ParsedLSBElement> StripEarlyServiceOnly(this List<ParsedLSBElement> service, LSBImportOptions options)
        {
            // go through service
            // if we see a caption about distribution we know communion hymns follow
            if (options.RemoveEarlyServiceSpecificElements)
            {
                HashSet<Guid> removedRoots = new HashSet<Guid>();

                foreach (var elem in service)
                {
                    var hymn = elem.LSBElement as LSBElementHymn;
                    if (hymn != null)
                    {
                        if (hymn.Caption.ToLower().Contains("8:30"))
                        {
                            // mark it for removal
                            elem.FilterFromOutput = true;
                            elem.Generator += " ====> [REMOVED early service only];";
                            removedRoots.Add(elem.Ancestory);
                        }
                    }
                    var caption = elem.LSBElement as LSBElementCaption;
                    if (caption != null)
                    {
                        if (caption.Caption.ToLower().Contains("8:30"))
                        {
                            // mark it for removal
                            elem.FilterFromOutput = true;
                            removedRoots.Add(elem.Ancestory);
                        }
                    }
                }

                // remove everything with a parent listed as being removed
                foreach (var elem in service)
                {
                    if (removedRoots.Contains(elem.Ancestory))
                    {
                        elem.FilterFromOutput = true;
                        elem.Generator += " ====> [REMOVED early service only];";
                    }
                }
            }

            return service;
        }


        public static List<ParsedLSBElement> AddAdditionalInferedElements(this List<ParsedLSBElement> service, LSBImportOptions options)
        {
            List<ParsedLSBElement> servicetoparse = null;
            List<ParsedLSBElement> newservice = new List<ParsedLSBElement>();

            ParsedLSBElement end = null;

            if (!options.UseCopyTitle)
            {
                // always start with titlepage insert
                newservice.Add(new ParsedLSBElement
                {
                    LSBElement = new InsertTitlepage(),
                    AddedByInference = true,
                    Generator = "Start of Service",
                    Ancestory = Guid.NewGuid(),
                });
                // always start with copyright
                // default to preset center after copyright (though bells would handle this...)
                // may want to be smart too-> if there's a prelude we could do soemthing else
                newservice.Add(new ParsedLSBElement
                {
                    LSBElement = new ExternalPrefab("#copyright", (int)Camera.Organ, options.InferPostset, "copyright", BlockType.TITLEPAGE),
                    Generator = "Start of Service [Flag=UseCopyTitle]",
                    AddedByInference = true,
                    Ancestory = Guid.NewGuid(),
                });

                servicetoparse = service;
            }
            else
            {

                ParsedLSBElement ackelem = service.FirstOrDefault(x => x.LSBElement?.GetType() == typeof(LSBElementAcknowledments));
                LSBElementAcknowledments ack = ackelem?.LSBElement as LSBElementAcknowledments;
                string sack = "";
                if (ackelem != null)
                {
                    sack = ack.Text;
                }


                // eat the first 2 elements (expected to be either headding or caption)
                // and generate a combined title/copyright from it
                if (service.Take(2).All(x => x.LSBElement?.GetType() == typeof(LSBElementCaption) || x.LSBElement?.GetType() == typeof(LSBElementHeading)))
                {
                    // ignore them for the rest of the service
                    //servicetoparse = service.Skip(2).ToList();
                    service.First().FilterFromOutput = true;
                    service.First().Generator += "; ===> REMOVED. Converted to CopyTitle [Flags=UseCopyTitle]";
                    service.Skip(1).First().FilterFromOutput = true;
                    service.Skip(1).First().Generator += "; ===> REMOVED. Converted to CopyTitle [Flags=UseCopyTitle]";

                    servicetoparse = service;

                    if (ack != null)
                    {
                        // remove from service- we've used it here
                        //servicetoparse.Remove();
                        ackelem.FilterFromOutput = true;
                        ackelem.Generator += "; ===> REMOVED. Converted to CopyTitle at start [Flags=UseCopyTitle]";
                    }

                    var acks = service.Take(2).Select(x => x.LSBElement).ToList();

                    newservice.Add(new ParsedLSBElement
                    {
                        LSBElement = ExternalPrefabGenerator.GenerateCopyTitle(acks, sack, options),
                        AddedByInference = true,
                        Generator = "Converted into CopyTilte [Flags=UseCopyTitle]",
                        SourceElements = acks.Select(x => x.SourceHTML).Concat(ack.SourceHTML.ItemAsEnumerable()),
                        Ancestory = Guid.NewGuid(),
                    });

                    if (options.UseTitledEnd)
                    {
                        end = new ParsedLSBElement
                        {
                            LSBElement = ExternalPrefabGenerator.GenerateEndPage(acks, options),
                            Generator = "Generate Copy Title [Flags=UseCopyTitle]",
                            AddedByInference = true,
                            SourceElements = acks.Select(x => x.SourceHTML),
                            Ancestory = Guid.NewGuid(),
                        };
                    }
                }
                else
                {
                    servicetoparse = service;
                }

            }

            servicetoparse = RemoveUnusedElement(servicetoparse, options);

            // warn abouth prelude? (if not present??)

            // warn about bells? (if not present??)

            // go through all elements, tracking when we have liturgy on, and insert liturgyoff commands as required...
            // go through all elements, insert organintro commands as required...

            bool inliturgy = false;

            ParsedLSBElement element = servicetoparse.First();
            ParsedLSBElement prevelement = null;
            ParsedLSBElement nextelement = null;
            for (int i = 0; i < servicetoparse.Count; i++)
            {
                element = servicetoparse[i];
                if (i + 1 < servicetoparse.Count)
                {
                    // ignore unknown elements while parsing next element...
                    //nextelement = servicetoparse[i + 1];
                    nextelement = servicetoparse.Skip(i + 1).SkipWhile(x => x.LSBElement is LSBElementUnknown || x.FilterFromOutput || !x.ConsiderForServicification).FirstOrDefault();
                }


                // Add postsets

                bool setlast = false;
                Camera lastselection = Camera.Unset;

                bool setfirst = false;
                Camera firstseelection = Camera.Unset;

                if ((nextelement?.LSBElement as LSBElementCaption)?.Caption.ToLower().Contains("sermon") == true)
                {
                    setlast = true;
                    lastselection = Camera.Pulpit;
                }
                if (nextelement?.LSBElement is LSBElementReading || nextelement?.LSBElement is LSBElementReadingComplex)
                {
                    setlast = true;
                    lastselection = Camera.Lectern;
                }
                if (nextelement?.LSBElement is LSBElementLiturgySung)
                {
                    setlast = true;
                    lastselection = Camera.Organ;
                }
                if (nextelement?.LSBElement is LSBElementLiturgy || nextelement?.LSBElement is LSBElementResponsiveLiturgy || nextelement?.LSBElement is LSBElementIntroit || new List<string> { "upnext", "creed" }.Contains((nextelement?.LSBElement as ExternalPrefab)?.TypeIdentifier))
                {
                    setlast = true;
                    lastselection = Camera.Center;
                }


                if ((element.LSBElement as LSBElementCaption)?.Caption.ToLower().Contains("sermon") == true)
                {
                    if (!setlast)
                    {
                        setlast = true;
                        lastselection = Camera.Center;
                    }
                }
                if (element.LSBElement is LSBElementLiturgySung)
                {
                    if (!setlast)
                    {
                        setlast = true;
                        lastselection = Camera.Center;
                    }
                    else
                    {
                        // not sure what we should do here.
                        // next element is also requesting to set last...
                        // for now let it override, since the center postset is more just a handy help- less a nessecary
                        // probably handled by rule for any liturgy-type to set first to liturgy (maybe)
                    }
                }
                if (element.LSBElement is LSBElementLiturgy || element.LSBElement is LSBElementResponsiveLiturgy || element.LSBElement is LSBElementIntroit || element.LSBElement is LSBElementLiturgySung || element.LSBElement is LSBElementReading || element.LSBElement is LSBElementReadingComplex)
                {
                    // since we're setting the first here, if a last was previously set it will overwrite so we can be a bit more aggressive with
                    // selecting elements to set a first for
                    if (!setfirst)
                    {
                        setfirst = true;
                        firstseelection = Camera.Center;
                    }
                }

                // Create Postset command
                if (options.InferPostset)
                {
                    StringBuilder sb = new StringBuilder();
                    if ((setfirst && firstseelection != Camera.Unset) || (setlast && lastselection != Camera.Unset))
                    {
                        sb.Append("::postset(");
                        if (setfirst)
                        {
                            sb.Append("first=");
                            sb.Append((int)firstseelection);
                            if (setlast)
                            {
                                sb.Append(", ");
                            }
                        }
                        if (setlast)
                        {
                            sb.Append("last=");
                            sb.Append((int)lastselection);
                        }
                        sb.Append(")");
                    }
                    element.LSBElement.PostsetCmd = sb.ToString();
                }
                else
                {
                    element.LSBElement.PostsetCmd = "";
                }




                bool shutdownliturgy = false;
                if (element.ConsiderForServicification && !element.FilterFromOutput && LiturgyElements.Contains(element.LSBElement?.GetType()))
                {
                    inliturgy = true;
                    // also skip if its a full-package reading, since they're considered responsible for their own teardown
                    var reading = element.LSBElement as LSBElementReadingComplex;
                    if (reading != null && options.FullPackageReadings)
                    {
                        inliturgy = false;

                        // anthem generating captions ought be excluded
                        if ((prevelement?.LSBElement as LSBElementCaption)?.Caption?.ToLower().Contains("anthem") == true)
                        {
                            shutdownliturgy = true;
                        }
                    }

                    var caption = element.LSBElement as LSBElementCaption;
                    if (caption != null)
                    {
                        if (caption.Caption.ToLower().Contains("bells"))
                        {
                            inliturgy = false;
                        }
                    }

                }
                else if (element.ConsiderForServicification && !element.FilterFromOutput)
                {
                    if (inliturgy)
                    {
                        // assumption here is that we'll get it off somehow
                        // either explicitly, or via an allowable exception that will put us in a state where we don't expect to need to do this expliclity prior to a new set of liturgy being identified
                        inliturgy = false;
                        shutdownliturgy = true;

                        // get rid of liturgy

                        if (options.YeetThyselfFromLiturgyToUpNextWithAsLittleAplombAsPossible)
                        {
                            var btype = element?.LSBElement?.BlockType() ?? BlockType.UNKNOWN;
                            if (btype == BlockType.HYMN_INTRO || btype == BlockType.HYMN)
                            {
                                shutdownliturgy = false;
                            }
                        }

                    }
                }

                if (shutdownliturgy)
                {
                    newservice.Add(new ParsedLSBElement
                    {
                        LSBElement = new ExternalPrefab("#liturgyoff", "liturgyoff", BlockType.MISC_CORPERATE),
                        Generator = $"Next element NOT [liturgy] is [{element?.BlockType}]. Previous element was [liturgy]",
                        AddedByInference = true,
                        Ancestory = Guid.NewGuid(),
                    });
                }


                if (element.LSBElement is LSBElementHymn && !element.FilterFromOutput)
                {
                    bool dointro = true;

                    if (options.WrapConsecuitivePackages)
                    {
                        // we're assumed to be wrapping consecutive hymns,
                        // so check if last hymn is the same, because in that case we don't re-introduce it
                        if (prevelement?.LSBElement is LSBElementHymn)
                        {
                            if ((prevelement.LSBElement as LSBElementHymn).Caption == (element.LSBElement as LSBElementHymn).Caption)
                            {
                                dointro = false;
                            }
                        }
                    }

                    if (dointro)
                    {
                        // we can use the new up-next tabs if we have a hymn #
                        newservice.Add(new ParsedLSBElement
                        {
                            LSBElement = ExternalPrefabGenerator.BuildHymnIntroSlides(element.LSBElement as LSBElementHymn, options.UseUpNextForHymns),
                            AddedByInference = true,
                            Generator = "Next element is [hymn]",
                            ParentSourceElement = element.ParentSourceElement,
                            Ancestory = Guid.NewGuid(),
                        });
                    }
                }

                // try having pre/post script blocks
                // allow them added here so we can coallesce blocks that should be scripted together

                if (options.WrapConsecuitivePackages)
                {
                    if (element.LSBElement is LSBElementReadingComplex && options.FullPackageReadings && !element.FilterFromOutput && (element.LSBElement as LSBElementReadingComplex)?.ShouldBePackaged(options, out _) == true)
                    {
                        // assume the first consecutive reading element setups the block 
                        if (prevelement.LSBElement is not LSBElementReadingComplex || (prevelement.LSBElement as LSBElementReadingComplex)?.ShouldBePackaged(options, out _) == false)
                        {
                            // add the reading prefab intro block
                            newservice.Add(new ParsedLSBElement
                            {
                                LSBElement = ScriptedWrapper.FromBlob(BlockType.READING, "PIPReadingScriptIntroBlock"),
                                AddedByInference = true,
                                Generator = "Next element is [reading]",
                                ParentSourceElement = element.ParentSourceElement,
                                Ancestory = Guid.NewGuid(),
                                HasWingsForFlighPlanning = false, // not a valid location to dump pilot-crap
                            });
                        }
                    }
                    if (element.LSBElement is LSBElementHymn && options.UsePIPHymns && !element.FilterFromOutput)
                    {
                        // assume the first consecutive hymn element setups the block 
                        if (prevelement?.LSBElement is not LSBElementHymn)
                        {
                            // add the reading prefab intro block
                            Dictionary<string, string> pipReplace = new Dictionary<string, string>
                            {
                                ["$PIPFILL"] = (element.LSBElement as LSBElementHymn).IsCommunionHymn ? "5" : "1",
                                ["$PIPCAM"] = (element.LSBElement as LSBElementHymn).IsCommunionHymn ? "ORGAN" : "BACK",
                            };

                            newservice.Add(new ParsedLSBElement
                            {
                                LSBElement = ScriptedWrapper.FromBlob(BlockType.HYMN, "PrePIPScriptIntroBlock_Hymn", blobReplace: pipReplace),
                                AddedByInference = true,
                                Generator = "Next element is [hymn]",
                                ParentSourceElement = element.ParentSourceElement,
                                Ancestory = Guid.NewGuid(),
                                HasWingsForFlighPlanning = false, // not a valid location to dump pilot-crap
                            });
                        }
                    }
                }

                newservice.Add(element);
                prevelement = element;


                // handle post scripting blocks here
                if (options.WrapConsecuitivePackages)
                {
                    if (element.LSBElement is LSBElementReadingComplex && options.FullPackageReadings && !element.FilterFromOutput && (element.LSBElement as LSBElementReadingComplex)?.ShouldBePackaged(options, out _) == true)
                    {
                        if (nextelement?.LSBElement is not LSBElementReadingComplex || (nextelement?.LSBElement as LSBElementReadingComplex)?.ShouldBePackaged(options, out _) == false)
                        {
                            // add the script teardown block here
                            var scriptclose = new ParsedLSBElement
                            {
                                LSBElement = ScriptedWrapper.ClosingWrapper(BlockType.READING),
                                AddedByInference = true,
                                Generator = "Last element was [reading]",
                                ParentSourceElement = element.ParentSourceElement,
                                Ancestory = Guid.NewGuid(),
                            };
                            newservice.Add(scriptclose);
                            prevelement = scriptclose;
                        }
                    }
                    if (element.LSBElement is LSBElementHymn && options.FullPackageReadings && !element.FilterFromOutput)
                    {
                        bool doend = true;
                        if (nextelement?.LSBElement is LSBElementHymn)
                        {
                            if ((nextelement.LSBElement as LSBElementHymn).Caption == (element.LSBElement as LSBElementHymn).Caption)
                            {
                                doend = false;
                            }
                        }

                        if (doend)
                        {
                            // add the script teardown block here
                            var scriptclose = new ParsedLSBElement
                            {
                                LSBElement = ScriptedWrapper.ClosingWrapper(BlockType.HYMN),
                                AddedByInference = true,
                                Generator = "Last element was [hymn]",
                                ParentSourceElement = element.ParentSourceElement,
                                Ancestory = Guid.NewGuid(),
                            };
                            newservice.Add(scriptclose);
                            prevelement = scriptclose;
                        }
                    }
                }

            }


            // add endservice slide
            if (end == null)
            {
                newservice.Add(new ParsedLSBElement
                {
                    LSBElement = new ExternalPrefab("#viewservices", "viewservices", BlockType.UNKNOWN),
                    Generator = "End Service",
                    AddedByInference = true,
                    Ancestory = Guid.NewGuid(),
                });
            }
            else
            {
                newservice.Add(end);
            }

            return newservice;
        }

    }
}
