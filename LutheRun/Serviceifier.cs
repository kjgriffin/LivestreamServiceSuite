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
                    if (new[] { "bells", "prelude", "postlude", "anthem", "sermon" }.Contains(caption.Caption.ToLower()))
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
            newservice.Add(new ExternalPrefab("#copyright", (int)Camera.Center));

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


                // add postsets

                // for hymns, if liturgy follows -> set postset center for first&last on hymn
                var h = element as LSBElementHymn;
                if (h != null)
                {
                    if (nextelement != null)
                    {
                        if (LiturgyElements.Contains(nextelement.GetType()))
                        {
                            // preset center for everything expect liturgy or sermon
                            if (nextelement is LSBElementLiturgy)
                            {
                                element.PostsetCmd = $"::postset(last={(int)Camera.Center})";
                            }
                            else if (nextelement is LSBElementLiturgySung)
                            {
                                element.PostsetCmd = $"::postset(last={(int)Camera.Organ})";
                            }
                            else if (nextelement is LSBElementCaption)
                            {
                                var c = nextelement as LSBElementCaption;
                                if (c.Caption.ToLower().Contains("sermon"))
                                {
                                    element.PostsetCmd = $"::postset(last={(int)Camera.Pulpit})";
                                }
                            }
                            else if (nextelement is LSBElementReading)
                            {
                                element.PostsetCmd = $"::postset(last={(int)Camera.Lectern})";
                            }
                        }
                    }
                }

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
