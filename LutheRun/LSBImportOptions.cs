using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LutheRun
{
    public class LSBImportOptions
    {
        public bool InferPostset { get; set; } = true;
        public bool UseUpNextForHymns { get; set; } = true;
        public bool OnlyKnownCaptions { get; set; } = true;
        public bool UseResponsiveLiturgy { get; set; } = true;

        public LSBElementFilter Filter { get; set; } = new LSBElementFilter();
    }

    public class LSBElementFilter
    {
        public bool Liturgy { get; set; } = true;
        public bool SungLiturgy { get; set; } = false;
        public bool Reading { get; set; } = false;
        public bool Caption { get; set; } = false;
        public bool Introit { get; set; } = false;
        public bool Hymn { get; set; } = false;
        public bool Prefab { get; set; } = false;
        public bool Unknown { get; set; } = false;

        public List<Type> FilteredTypes
        {
            get
            {
                List<Type> elements = new List<Type>();
                if (Liturgy)
                    elements.Add(typeof(LSBElementLiturgy));
                if (SungLiturgy)
                    elements.Add(typeof(LSBElementLiturgySung));
                if (Reading)
                    elements.Add(typeof(LSBElementReading));
                if (Caption)
                    elements.Add(typeof(LSBElementCaption));
                if (Introit)
                    elements.Add(typeof(LSBElementIntroit));
                if (Hymn)
                    elements.Add(typeof(LSBElementHymn));
                if (Prefab)
                    elements.Add(typeof(LSBElementIsPrefab));
                if (Unknown)
                    elements.Add(typeof(LSBElementUnknown));

                return elements;
            }
        }
    }

}
