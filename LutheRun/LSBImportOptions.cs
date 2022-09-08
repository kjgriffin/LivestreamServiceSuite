using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LutheRun
{
    public class LSBImportOptions
    {
        [BoolSetting]
        public bool InferPostset { get; set; } = true;
        [BoolSetting]
        public bool UseUpNextForHymns { get; set; } = true;
        [BoolSetting]
        public bool UsePIPHymns { get; set; } = true;

        public bool OnlyKnownCaptions { get; set; } = true;
        [BoolSetting]
        public bool UseResponsiveLiturgy { get; set; } = true;
        [BoolSetting]
        public bool UseComplexIntroit { get; set; } = true;
        [BoolSetting]
        public bool UseComplexReading { get; set; } = true;
        /// <summary>
        /// Requires ComplexReadings. Used to bring the full text in, but does not force the whole package.
        /// </summary>
        [BoolSetting]
        public bool FullTextReadings { get; set; } = false;
        /// <summary>
        /// Requires ComplexReadings. Will include the full reading text, but packaged (to handle titles/scripts) accordingly.
        /// Will attempt to ignore introit/responsive psalms if InferResponsivePsalmReadingsAsTitledLiturgy is enabled
        /// </summary>
        [BoolSetting]
        public bool FullPackageReadings { get; set; } = true;
        [BoolSetting]
        public bool InferResponsivePslamReadingsAsTitledLiturgy { get; set; } = true;
        [BoolSetting]
        public bool ForcePsalmsAsTitledLiturgy { get; set; } = false;
        [BoolSetting]
        public bool PullAllReadingContentAsTitledLiturgy { get; set; } = false;
        [BoolSetting]
        public bool UseCopyTitle { get; set; } = true;
        [BoolSetting]
        public bool UseTitledEnd { get; set; } = true;
        [BoolSetting]
        public bool UseThemedCreeds { get; set; } = true;
        [BoolSetting]
        public bool UsePIPCreeds { get; set; } = true;
        [BoolSetting]
        public bool UseThemedHymns { get; set; } = true;

        public string ServiceThemeLib { get; set; } = "Xenon.CommonColored";

        public Dictionary<string, string> Macros { get; set; } = new Dictionary<string, string>();

        public LSBElementFilter Filter { get; set; } = new LSBElementFilter();
    }

    public class BoolSettingAttribute : Attribute
    {

    }

    public class LSBElementFilter
    {
        [BoolSetting]
        public bool Liturgy { get; set; } = true;
        [BoolSetting]
        public bool ResponsiveLiturgy { get; set; } = true;
        [BoolSetting]
        public bool SungLiturgy { get; set; } = true;
        [BoolSetting]
        public bool Reading { get; set; } = true;
        [BoolSetting]
        public bool ComplexReading { get; set; } = true;
        [BoolSetting]
        public bool Caption { get; set; } = true;
        [BoolSetting]
        public bool Heading { get; set; } = false;
        [BoolSetting]
        public bool Introit { get; set; } = true;
        [BoolSetting]
        public bool Hymn { get; set; } = true;
        [BoolSetting]
        public bool Prefab { get; set; } = true;
        [BoolSetting]
        public bool ExternalPrefab { get; set; } = true;
        [BoolSetting]
        public bool Unknown { get; set; } = false;
        [BoolSetting]
        public bool Acknowledgments { get; set; } = false;

        public List<Type> FilteredTypes
        {
            get
            {
                List<Type> elements = new List<Type>();
                if (Liturgy)
                    elements.Add(typeof(LSBElementLiturgy));
                if (ResponsiveLiturgy)
                    elements.Add(typeof(LSBElementResponsiveLiturgy));
                if (SungLiturgy)
                    elements.Add(typeof(LSBElementLiturgySung));
                if (Reading)
                    elements.Add(typeof(LSBElementReading));
                if (ComplexReading)
                    elements.Add(typeof(LSBElementReadingComplex));
                if (Caption)
                    elements.Add(typeof(LSBElementCaption));
                if (Heading)
                    elements.Add(typeof(LSBElementHeading));
                if (Introit)
                    elements.Add(typeof(LSBElementIntroit));
                if (Hymn)
                    elements.Add(typeof(LSBElementHymn));
                if (Prefab)
                    elements.Add(typeof(LSBElementIsPrefab));
                if (ExternalPrefab)
                    elements.Add(typeof(ExternalPrefab));
                if (Unknown)
                    elements.Add(typeof(LSBElementUnknown));
                if (Acknowledgments)
                    elements.Add(typeof(LSBElementAcknowledments));

                return elements;
            }
        }
    }

}
