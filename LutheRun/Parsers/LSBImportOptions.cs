using LutheRun.Elements;
using LutheRun.Elements.LSB;
using LutheRun.Pilot;

using System;
using System.Collections.Generic;

namespace LutheRun.Parsers
{
    public class LSBImportOptions
    {
        [BoolSetting]
        public bool InferPostset { get; set; } = true;
        [BoolSetting]
        public bool UseUpNextForHymns { get; set; } = true;
        [BoolSetting]
        public bool YeetThyselfFromLiturgyToUpNextWithAsLittleAplombAsPossible { get; set; } = true;
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
        public bool WrapConsecuitivePackages { get; set; } = true;
        [BoolSetting]
        public bool InferResponsivePslamReadingsAsTitledLiturgy { get; set; } = true;
        [BoolSetting]
        public bool StrictlyEnforceResponsivenessForIntroits { get; set; } = true;
        [BoolSetting]
        public bool SoLikeImDoingAFuneralHereAndPsalm23sGonnaBeDoneResponsivelySoJustOverrideAnyOtherComplexReadingSettingsThatGetInTheWay { get; set; } = false;
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
        [BoolSetting]
        public bool InferSeason { get; set; } = true;
        [BoolSetting]
        public bool AggressivelyParseInsideLSBContent { get; set; } = true;
        [BoolSetting]
        public bool FlightPlanning { get; set; } = true;
        [BoolSetting]
        public bool RemoveEarlyServiceSpecificElements { get; internal set; } = true;


        public string ServiceThemeLib { get; set; } = "Xenon.CommonColored";

        public Dictionary<string, string> Macros { get; set; } = new Dictionary<string, string>();

        public LSBElementFilter Filter { get; set; } = new LSBElementFilter();

        public FlightPlanCameras PlannedCameras { get; set; } = new FlightPlanCameras();
        public Dictionary<CameraID, string> PilotCamIdMap { get; set; } = CameraIDHelpers.DefaultMappings;
        public Dictionary<string, string> PilotPresetMap { get; set; } = new Dictionary<string, string>
        {
            ["center:front"] = "FRONT",
            ["center:sermon"] = "SERMON",
            ["center:anthem"] = "ANTHEM",

            ["pulpit:sermon"] = "JESUS",
            ["pulpit:anthem"] = "ANTHEM",
            ["pulpit:opening"] = "OPENING",

            ["lectern:reading"] = "READING",
            ["lectern:sermon"] = "SERMON",
            ["lectern:anthem"] = "ANTHEM",
            ["lectern:opening"] = "OPENING",

            ["organ:organ"] = "ORGAN",
            ["organ:piano?"] = "PIANO",
            ["organ:prelude"] = "ORGAN",
            ["organ:postlude"] = "ORGAN",
            ["organ:anthem"] = "ANTHEM",

            ["back:wide"] = "WIDE",
        };

        public Dictionary<string, string> PilotZoomMap { get; set; } = new Dictionary<string, string>
        {
            ["center:front"] = "FRONT",
            ["center:sermon"] = "SERMON",
            ["center:anthem"] = "ANTHEM",

            ["pulpit:sermon"] = "JESUS",
            ["pulpit:anthem"] = "ANTHEM",
            ["pulpit:opening"] = "OPENING",

            ["lectern:reading"] = "READING",
            ["lectern:sermon"] = "SERMON",
            ["lectern:anthem"] = "ANTHEM",
            ["lectern:opening"] = "OPENING",

            ["organ:organ"] = "ORGAN",
            ["organ:piano?"] = "PIANO",
            ["organ:prelude"] = "ORGAN",
            ["organ:postlude"] = "ORGAN",
            ["organ:anthem"] = "ANTHEM",

            ["back:wide"] = "WIDE",
        };
    }

    public class BoolSettingAttribute : Attribute
    {

    }

    public class PilotEnumMap : Attribute
    {
        internal CameraID ID { get; set; }
        public PilotEnumMap(CameraID cam)
        {
            ID = cam;
        }
    }

    public class FlightPlanCameras
    {
        [BoolSetting]
        [PilotEnumMap(CameraID.PULPIT)]
        public bool Pulpit { get; set; } = true;
        [BoolSetting]
        [PilotEnumMap(CameraID.CENTER)]
        public bool Center { get; set; } = true;
        [BoolSetting]
        [PilotEnumMap(CameraID.LECTERN)]
        public bool Lectern { get; set; } = true;
        [BoolSetting]
        [PilotEnumMap(CameraID.ORGAN)]
        public bool Organ { get; set; } = false;
        [BoolSetting]
        [PilotEnumMap(CameraID.BACK)]
        public bool Back { get; set; } = false;
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
        public bool ScriptWrapper { get; set; } = true;
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
        public bool ContentFromUnknown { get; set; } = true;
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
                if (ScriptWrapper)
                    elements.Add(typeof(ScriptedWrapper));
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
                if (ContentFromUnknown)
                    elements.Add(typeof(LSBElementUnknownFromContent));
                if (Acknowledgments)
                    elements.Add(typeof(LSBElementAcknowledments));

                return elements;
            }
        }
    }

}
