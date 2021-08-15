using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        enum Camera
        {
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
            typeof(LSBElementLiturgy),
            typeof(LSBElementLiturgySung),
            typeof(LSBElementReading),
            typeof(LSBElementCaption),
            typeof(LSBElementIntroit),
        };


        public static List<ILSBElement> RemoveUnusedElement(List<ILSBElement> service)
        {
            // for now just remove: headdings, captions that don't match keys and any unknown
            List<ILSBElement> trimmed = new List<ILSBElement>();

            foreach (var element in service)
            {
                var caption = element as LSBElementCaption;
                if (caption != null)
                {
                    if (new[] { "bells", "prelude", "postlude", "anthem", "sermon" }.Any(c => caption.Caption.ToLower().Contains(c)))
                    {
                        trimmed.Add(element);
                    }
                    continue;
                }

                if (element is LSBElementHeading || element is LSBElementUnknown)
                {
                    continue;
                }

                trimmed.Add(element);
            }

            return trimmed;
        }

        public static List<ILSBElement> AddAdditionalInferedElements(List<ILSBElement> service)
        {
            List<ILSBElement> newservice = new List<ILSBElement>();

            // always start with titlepage insert
            newservice.Add(new InsertTitlepage());
            // always start with copyright
            // default to preset center after copyright (though bells would handle this...)
            // may want to be smart too-> if there's a prelude we could do soemthing else
            newservice.Add(new ExternalPrefab("#copyright", (int)Camera.Organ));

            // warn abouth prelude? (if not present??)

            // warn about bells? (if not present??)

            // go through all elements, tracking when we have liturgy on, and insert liturgyoff commands as required...
            // go through all elements, insert organintro commands as required...

            bool inliturgy = false;

            for (int i = 0; i < service.Count; i++)
            {
                ILSBElement element = service[i];
                ILSBElement prevelement = null;
                ILSBElement nextelement = null;
                if (i + 1 < service.Count)
                {
                    nextelement = service[i + 1];
                }


                // Add postsets

                bool setlast = false;
                Camera lastselection = 0;

                bool setfirst = false;
                Camera firstseelection = 0;

                if ((nextelement as LSBElementCaption)?.Caption.ToLower().Contains("sermon") == true)
                {
                    setlast = true;
                    lastselection = Camera.Pulpit;
                }
                if (nextelement is LSBElementReading)
                {
                    setlast = true;
                    lastselection = Camera.Lectern;
                }
                if (nextelement is LSBElementLiturgySung)
                {
                    setlast = true;
                    lastselection = Camera.Organ;
                }
                if (nextelement is LSBElementLiturgy || nextelement is LSBElementIntroit)
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
                if (element is LSBElementLiturgy || element is LSBElementIntroit || element is LSBElementLiturgySung || element is LSBElementReading)
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
                StringBuilder sb = new StringBuilder();
                if (setfirst || setlast)
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




                if (LiturgyElements.Contains(element.GetType()))
                {
                    inliturgy = true;
                }
                else
                {
                    if (inliturgy)
                    {
                        // get rid of liturgy
                        newservice.Add(new ExternalPrefab("#liturgyoff"));
                        inliturgy = false;
                    }
                }

                if (element is LSBElementHymn)
                {
                    newservice.Add(new ExternalPrefab("#organintro"));
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
