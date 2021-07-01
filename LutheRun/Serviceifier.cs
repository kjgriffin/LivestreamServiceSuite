using System;
using System.Collections.Generic;
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

        public static List<Type> LiturgyElements = new List<Type>()
        {
            typeof(LSBElementLiturgy),
            typeof(LSBElementLiturgySung),
            typeof(LSBElementReading),
            typeof(LSBElementCaption),
        };


        public static List<ILSBElement> AddAdditionalInferedElements(List<ILSBElement> service)
        {
            List<ILSBElement> newservice = new List<ILSBElement>();

            // always start with titlepage insert
            newservice.Add(new InsertTitlepage());
            // always start with copyright
            newservice.Add(new ExternalPrefab("#copyright"));

            // warn abouth prelude? (if not present??)

            // warn about bells? (if not present??)

            // go through all elements, tracking when we have liturgy on, and insert liturgyoff commands as required...
            // go through all elements, insert organintro commands as required...

            bool inliturgy = false;

            for (int i = 0; i < service.Count; i++)
            {
                ILSBElement element = service[i];
                //ILSBElement prevelement = null;
                //ILSBElement nextelement = null;
                //if (i + 1 < service.Count)
                //{
                    //nextelement = service[i + 1];
                //}

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

                //prevelement = element;
            }


            // add endservice slide
            newservice.Add(new ExternalPrefab("#viewservices"));

            return newservice;
        }

    }
}
