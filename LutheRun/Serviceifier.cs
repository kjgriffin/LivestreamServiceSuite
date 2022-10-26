using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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
            foreach (var element in service)
            {
                var caption = element.LSBElement as LSBElementCaption;
                if (caption != null)
                {
                    if (options.OnlyKnownCaptions)
                    {
                        if (new[] { "bells", "prelude", "postlude", "anthem", "sermon" }.Any(c => caption.Caption.ToLower().Contains(c)))
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
                });
                // always start with copyright
                // default to preset center after copyright (though bells would handle this...)
                // may want to be smart too-> if there's a prelude we could do soemthing else
                newservice.Add(new ParsedLSBElement
                {
                    LSBElement = new ExternalPrefab("#copyright", (int)Camera.Organ, options.InferPostset, "copyright", BlockType.TITLEPAGE),
                    Generator = "Start of Service [Flag=UseCopyTitle]",
                    AddedByInference = true,
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
                    });

                    if (options.UseTitledEnd)
                    {
                        end = new ParsedLSBElement
                        {
                            LSBElement = ExternalPrefabGenerator.GenerateEndPage(acks, options),
                            Generator = "Generate Copy Title [Flags=UseCopyTitle]",
                            AddedByInference = true,
                            SourceElements = acks.Select(x => x.SourceHTML),
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




                if (element.ConsiderForServicification && LiturgyElements.Contains(element.LSBElement?.GetType()))
                {
                    inliturgy = true;
                    // also skip if its a full-package reading, since they're considered responsible for their own teardown
                    var reading = element.LSBElement as LSBElementReadingComplex;
                    if (reading != null && options.FullPackageReadings)
                    {
                        inliturgy = false;
                    }
                }
                else if (element.ConsiderForServicification)
                {
                    if (inliturgy)
                    {
                        // don't add this one if we've infered bells should be prior
                        bool skip = false;
                        var caption = prevelement.LSBElement as LSBElementCaption;
                        if (caption != null)
                        {
                            if (caption.Caption.ToLower().Contains("bells"))
                            {
                                skip = true;
                            }
                        }

                        // get rid of liturgy
                        if (!skip)
                        {
                            newservice.Add(new ParsedLSBElement
                            {
                                LSBElement = new ExternalPrefab("#liturgyoff", "liturgyoff", BlockType.MISC_CORPERATE),
                                Generator = "Next element NOT [liturgy]. Previous element was [liturgy]",
                                AddedByInference = true,
                            });
                        }
                        // we'll assume the bell's script turns it off
                        inliturgy = false;
                    }
                }

                if (element.LSBElement is LSBElementHymn)
                {
                    // we can use the new up-next tabs if we have a hymn #
                    newservice.Add(new ParsedLSBElement
                    {
                        LSBElement = ExternalPrefabGenerator.BuildHymnIntroSlides(element.LSBElement as LSBElementHymn, options.UseUpNextForHymns),
                        AddedByInference = true,
                        Generator = "Next element is [hymn]",
                        ParentSourceElement = element.ParentSourceElement,
                    });

                }

                newservice.Add(element);

                prevelement = element;
            }


            // add endservice slide
            if (end == null)
            {
                newservice.Add(new ParsedLSBElement
                {
                    LSBElement = new ExternalPrefab("#viewservices", "viewservices", BlockType.UNKNOWN),
                    Generator = "End Service",
                    AddedByInference = true,
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
