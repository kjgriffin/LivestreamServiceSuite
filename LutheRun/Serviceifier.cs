using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LutheRun
{
    class Serviceifier
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
            typeof(LSBElementCaption),
            typeof(LSBElementIntroit),
        };


        public static List<ILSBElement> Filter(List<ILSBElement> service, LSBImportOptions options)
        {
            return service.Where(x => options.Filter.FilteredTypes.Contains(x.GetType())).ToList();
        }

        public static List<ILSBElement> RemoveUnusedElement(List<ILSBElement> service, LSBImportOptions options)
        {
            List<ILSBElement> trimmed = new List<ILSBElement>();

            // rely on filter to remove extra junk
            // only use here is to remove unknown captions
            foreach (var element in service)
            {
                var caption = element as LSBElementCaption;
                if (caption != null)
                {
                    if (options.OnlyKnownCaptions)
                    {
                        if (new[] { "bells", "prelude", "postlude", "anthem", "sermon" }.Any(c => caption.Caption.ToLower().Contains(c)))
                        {
                            trimmed.Add(element);
                        }
                        else if (!options.OnlyKnownCaptions)
                        {
                            trimmed.Add(element);
                        }
                        continue;
                    }
                    else
                    {
                        trimmed.Add(element);
                    }
                }

                trimmed.Add(element);
            }

            return trimmed;
        }

        public static List<ILSBElement> AddAdditionalInferedElements(List<ILSBElement> service, LSBImportOptions options)
        {
            List<ILSBElement> servicetoparse = null;
            List<ILSBElement> newservice = new List<ILSBElement>();

            if (!options.UseCopyTitle)
            {
                // always start with titlepage insert
                newservice.Add(new InsertTitlepage());
                // always start with copyright
                // default to preset center after copyright (though bells would handle this...)
                // may want to be smart too-> if there's a prelude we could do soemthing else
                newservice.Add(new ExternalPrefab("#copyright", (int)Camera.Organ, options.InferPostset));

                servicetoparse = service;
            }
            else
            {

                LSBElementAcknowledments ack = service.FirstOrDefault(x => x.GetType() == typeof(LSBElementAcknowledments)) as LSBElementAcknowledments;
                string sack = "";
                if (ack != null)
                {
                    sack = ack.Text;
                }


                // eat the first 2 elements (expected to be either headding or caption)
                // and generate a combined title/copyright from it
                if (service.Take(2).All(x => x.GetType() == typeof(LSBElementCaption) || x.GetType() == typeof(LSBElementHeading)))
                {
                    // ignore them for the rest of the service
                    servicetoparse = service.Skip(2).ToList();

                    if (ack != null)
                    {
                        // remove from service- we've used it here
                        servicetoparse.Remove(ack);
                    }


                    newservice.Add(ExternalPrefabGenerator.GenerateCopyTitle(service.Take(2), sack, options));
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

            ILSBElement element = servicetoparse.First();
            ILSBElement prevelement = null;
            ILSBElement nextelement = null;
            for (int i = 0; i < servicetoparse.Count; i++)
            {
                element = servicetoparse[i];
                if (i + 1 < servicetoparse.Count)
                {
                    nextelement = servicetoparse[i + 1];
                }


                // Add postsets

                bool setlast = false;
                Camera lastselection = Camera.Unset;

                bool setfirst = false;
                Camera firstseelection = Camera.Unset;

                if ((nextelement as LSBElementCaption)?.Caption.ToLower().Contains("sermon") == true)
                {
                    setlast = true;
                    lastselection = Camera.Pulpit;
                }
                if (nextelement is LSBElementReading || nextelement is LSBElementReadingComplex)
                {
                    setlast = true;
                    lastselection = Camera.Lectern;
                }
                if (nextelement is LSBElementLiturgySung)
                {
                    setlast = true;
                    lastselection = Camera.Organ;
                }
                if (nextelement is LSBElementLiturgy || nextelement is LSBElementResponsiveLiturgy || nextelement is LSBElementIntroit)
                {
                    setlast = true;
                    lastselection = Camera.Center;
                }


                if ((element as LSBElementCaption)?.Caption.ToLower().Contains("sermon") == true)
                {
                    if (!setlast)
                    {
                        setlast = true;
                        lastselection = Camera.Center;
                    }
                }
                if (element is LSBElementLiturgySung)
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
                if (element is LSBElementLiturgy || element is LSBElementResponsiveLiturgy || element is LSBElementIntroit || element is LSBElementLiturgySung || element is LSBElementReading || element is LSBElementReadingComplex)
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
                    element.PostsetCmd = sb.ToString();
                }
                else
                {
                    element.PostsetCmd = "";
                }




                if (LiturgyElements.Contains(element.GetType()))
                {
                    inliturgy = true;
                }
                else
                {
                    if (inliturgy)
                    {
                        // don't add this one if we've infered bells should be prior
                        bool skip = false;
                        var caption = prevelement as LSBElementCaption;
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
                            newservice.Add(new ExternalPrefab("#liturgyoff"));
                        }
                        // we'll assume the bell's script turns it off
                        inliturgy = false;
                    }
                }

                if (element is LSBElementHymn)
                {
                    // we can use the new up-next tabs if we have a hymn #
                    newservice.Add(PrefabBuilder.BuildHymnIntroSlides(element as LSBElementHymn, options.UseUpNextForHymns));
                }

                newservice.Add(element);

                prevelement = element;
            }


            // add endservice slide
            newservice.Add(new ExternalPrefab("#viewservices"));

            return newservice;
        }

    }
}
