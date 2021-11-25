using System;
using System.Collections.Generic;
using System.Text;

using Xenon.Compiler.AST;
using Xenon.LayoutInfo;
using Xenon.Renderer;

namespace Xenon.Compiler
{
    static class LanguageKeywords
    {

        const string LAYOUTVARNAME = "Layout";
        const string LAYOUTJSONVARNAME = "Layout.JSON";
        public static string LayoutVarName(LanguageKeywordCommand cmd)
        {
            return $"{Commands[cmd]}.{LAYOUTVARNAME}";
        }
        public static string LayoutJsonVarName(LanguageKeywordCommand cmd)
        {
            return $"{Commands[cmd]}.{LAYOUTJSONVARNAME}";
        }

        public static Dictionary<LanguageKeywordCommand, (ILayoutInfoResolver<ALayoutInfo> layoutResolver, ISlideLayoutPrototypePreviewer<ALayoutInfo> prototypicalLayoutPreviewer)> LayoutForType = new Dictionary<LanguageKeywordCommand, (ILayoutInfoResolver<ALayoutInfo>, ISlideLayoutPrototypePreviewer<ALayoutInfo>)>
        {
            [LanguageKeywordCommand.TwoPartTitle] = (new _2TitleSlideLayoutInfo(), new TwoPartTitleSlideRenderer()),
            //[LanguageKeywordCommand.StitchedImage] = (new StitchedImageSlideLayoutInfo(), new StitchedImageRenderer()),
        };

        public static List<string> WholeWords = new List<string>()
        {
            "solidcolorcanvas",
            "crop",
            "centerassetfill",
            "asset",
            "width",
            "height",
            "color",
            "kcolor",
            "bound",
            "Top",
            "Left",
            "Bottom",
            "Right",
            "True",
            "False",
            "icolor",
            "rtol",
            "gtol",
            "btol",
            "uniformstretch",
            "fill",
            "kfill",
            "centeronbackground",
            "coloredit",
            "forkey",
            LAYOUTVARNAME,
            LAYOUTJSONVARNAME,
        };

        public static Dictionary<LanguageKeywordCommand, string> Commands = new Dictionary<LanguageKeywordCommand, string>()
        {
            [LanguageKeywordCommand.Script_LiturgyOff] = "liturgyoff",
            [LanguageKeywordCommand.Script_OrganIntro] = "organintro",
            [LanguageKeywordCommand.SetVar] = "set",
            [LanguageKeywordCommand.Break] = "break",
            [LanguageKeywordCommand.Video] = "video",
            [LanguageKeywordCommand.FilterImage] = "filterimage",
            [LanguageKeywordCommand.FullImage] = "fullimage",
            [LanguageKeywordCommand.FitImage] = "fitimage",
            [LanguageKeywordCommand.AutoFitImage] = "autofitimage",
            [LanguageKeywordCommand.StitchedImage] = "stitchedimage",
            [LanguageKeywordCommand.LiturgyImage] = "litimage",
            [LanguageKeywordCommand.Liturgy] = "liturgy",
            [LanguageKeywordCommand.LiturgyVerse] = "litverse",
            [LanguageKeywordCommand.TitledLiturgyVerse] = "tlverse",
            [LanguageKeywordCommand.Reading] = "reading",
            [LanguageKeywordCommand.Sermon] = "sermon",
            [LanguageKeywordCommand.AnthemTitle] = "anthemtitle",
            [LanguageKeywordCommand.TwoPartTitle] = "2title",
            [LanguageKeywordCommand.TextHymn] = "texthymn",
            [LanguageKeywordCommand.Verse] = "verse",
            [LanguageKeywordCommand.Copyright] = "copyright",
            [LanguageKeywordCommand.ViewServices] = "viewservices",
            [LanguageKeywordCommand.ViewSeries] = "viewseries",
            [LanguageKeywordCommand.ApostlesCreed] = "apostlescreed",
            [LanguageKeywordCommand.NiceneCreed] = "nicenecreed",
            [LanguageKeywordCommand.LordsPrayer] = "lordsprayer",
            [LanguageKeywordCommand.Resource] = "resource",
            [LanguageKeywordCommand.Script] = "script",
            [LanguageKeywordCommand.Postset] = "postset",
            [LanguageKeywordCommand.VariableScope] = "scope",
            [LanguageKeywordCommand.ScopedVariable] = "var",
        };

        public static Dictionary<LanguageKeywordCommand, LanguageKeywordMetadata> LanguageKeywordMetadata = new Dictionary<LanguageKeywordCommand, LanguageKeywordMetadata>()
        {
            [LanguageKeywordCommand.Script_LiturgyOff] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, new XenonASTPrefabScriptLiturgyOff()),
            [LanguageKeywordCommand.Script_OrganIntro] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, new XenonASTPrefabScriptOrganIntro()),
            [LanguageKeywordCommand.SetVar] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, new XenonASTSetVariable()),
            [LanguageKeywordCommand.Break] = (false, LanguageKeywordCommand.Liturgy, false, new XenonASTSlideBreak()),
            [LanguageKeywordCommand.Video] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, new XenonASTVideo()),
            [LanguageKeywordCommand.FilterImage] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, new XenonASTFilterImage()),
            [LanguageKeywordCommand.FullImage] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, new XenonASTFullImage()),
            [LanguageKeywordCommand.FitImage] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, new XenonASTFitImage()),
            [LanguageKeywordCommand.AutoFitImage] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, new XenonASTAutoFitImage()),
            [LanguageKeywordCommand.StitchedImage] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, new XenonASTStitchedHymn()),
            [LanguageKeywordCommand.LiturgyImage] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, new XenonASTLiturgyImage()),
            [LanguageKeywordCommand.Liturgy] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, new XenonASTLiturgy()),
            [LanguageKeywordCommand.LiturgyVerse] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, new XenonASTLiturgyVerse()),
            [LanguageKeywordCommand.TitledLiturgyVerse] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, new XenonASTTitledLiturgyVerse()),
            [LanguageKeywordCommand.Reading] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, new XenonASTReading()),
            [LanguageKeywordCommand.Sermon] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, new XenonASTSermon()),
            [LanguageKeywordCommand.AnthemTitle] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, new XenonASTAnthemTitle()),
            [LanguageKeywordCommand.TwoPartTitle] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, true, new XenonAST2PartTitle()),
            [LanguageKeywordCommand.TextHymn] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, new XenonASTTextHymn()),
            [LanguageKeywordCommand.Verse] = (false, LanguageKeywordCommand.TextHymn, false, new XenonASTHymnVerse()),
            [LanguageKeywordCommand.Copyright] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, new XenonASTPrefabCopyright()),
            [LanguageKeywordCommand.ViewServices] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, new XenonASTPrefabViewServices()),
            [LanguageKeywordCommand.ViewSeries] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, new XenonASTPrefabViewSeries()),
            [LanguageKeywordCommand.ApostlesCreed] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, new XenonASTPrefabApostlesCreed()),
            [LanguageKeywordCommand.NiceneCreed] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, new XenonASTPrefabNiceneCreed()),
            [LanguageKeywordCommand.LordsPrayer] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, new XenonASTPrefabLordsPrayer()),
            [LanguageKeywordCommand.Resource] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, new XenonASTResource()),
            [LanguageKeywordCommand.Script] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, new XenonASTScript()),
            [LanguageKeywordCommand.Postset] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, new XenonASTExpression()),
            [LanguageKeywordCommand.ScopedVariable] = (false, LanguageKeywordCommand.VariableScope, false, new XenonASTScopedVariable()),
            [LanguageKeywordCommand.VariableScope] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, new XenonASTVariableScope()),
        };

        public static Dictionary<AutomationActions, AutomationActionMetadata> ScriptActionsMetadata = new Dictionary<AutomationActions, AutomationActionMetadata>()
        {
            [AutomationActions.AutoTrans] = (0, AutomationActions.AutoTrans, "AutoTrans", null),
            [AutomationActions.CutTrans] = (0, AutomationActions.CutTrans, "CutTrans", null),
            [AutomationActions.AutoTakePresetIfOnSlide] = (0, AutomationActions.AutoTakePresetIfOnSlide, "AutoTakePresetIfOnSlide", null),

            [AutomationActions.DSK1On] = (0, AutomationActions.DSK1On, "DKS1On", null),
            [AutomationActions.DSK1Off] = (0, AutomationActions.DSK1Off, "DSK1Off", null),
            [AutomationActions.DSK1FadeOn] = (0, AutomationActions.DSK1FadeOn, "DSK1FadeOn", null),
            [AutomationActions.DSK1FadeOff] = (0, AutomationActions.DSK1FadeOff, "DSK1FadeOff", null),

            [AutomationActions.DSK2On] = (0, AutomationActions.DSK2On, "DSK2On", null),
            [AutomationActions.DSK2Off] = (0, AutomationActions.DSK2Off, "DSK2Off", null),
            [AutomationActions.DSK2FadeOn] = (0, AutomationActions.DSK2FadeOn, "DSK2FadeOn", null),
            [AutomationActions.DSK2FadeOff] = (0, AutomationActions.DSK2FadeOff, "DSK2FadeOff", null),

            [AutomationActions.USK1On] = (0, AutomationActions.USK1On, "USK1On", null),
            [AutomationActions.USK1Off] = (0, AutomationActions.USK1Off, "USK1OFf", null),
            [AutomationActions.USK1SetTypeChroma] = (0, AutomationActions.USK1SetTypeChroma, "USK1SetTypeChroma", null),
            [AutomationActions.USK1SetTypeDVE] = (0, AutomationActions.USK1SetTypeDVE, "USK1SetTypeDVE", null),

            [AutomationActions.RecordStart] = (0, AutomationActions.RecordStart, "RecordStart", null),
            [AutomationActions.RecordStop] = (0, AutomationActions.RecordStop, "RecordStop", null),

            [AutomationActions.OpenAudioPlayer] = (0, AutomationActions.OpenAudioPlayer, "OpenAudioPlayer", null),
            [AutomationActions.PlayAuxAudio] = (0, AutomationActions.PlayAuxAudio, "PlayAuxAudio", null),
            [AutomationActions.StopAuxAudio] = (0, AutomationActions.StopAuxAudio, "StopAuxAudio", null),
            [AutomationActions.PauseAuxAudio] = (0, AutomationActions.PauseAuxAudio, "PauseAuxAudio", null),
            [AutomationActions.ReplayAuxAudio] = (0, AutomationActions.ReplayAuxAudio, "ReplayAuxAudio", null),

            [AutomationActions.PlayMedia] = (0, AutomationActions.PlayMedia, "PlayMedia", null),
            [AutomationActions.PauseMedia] = (0, AutomationActions.PauseMedia, "PauseMedia", null),
            [AutomationActions.StopMedia] = (0, AutomationActions.StopMedia, "StopMedia", null),
            [AutomationActions.RestartMedia] = (0, AutomationActions.RestartMedia, "RestartMedia", null),
            [AutomationActions.MuteMedia] = (0, AutomationActions.MuteMedia, "MuteMedia", null),
            [AutomationActions.UnMuteMedia] = (0, AutomationActions.UnMuteMedia, "UnMuteMedia", null),

            [AutomationActions.DriveNextSlide] = (0, AutomationActions.DriveNextSlide, "DriveNextSlide", null),
            [AutomationActions.Timer1Restart] = (0, AutomationActions.Timer1Restart, "Timer1Restart", null),



            [AutomationActions.PresetSelect] = (1, AutomationActions.PresetSelect, "PresetSelect", new List<AutomationActionArgType> { AutomationActionArgType.Integer }),
            [AutomationActions.ProgramSelect] = (1, AutomationActions.ProgramSelect, "ProgramSelect", new List<AutomationActionArgType> { AutomationActionArgType.Integer }),
            [AutomationActions.AuxSelect] = (1, AutomationActions.AuxSelect, "AuxSelect", new List<AutomationActionArgType> { AutomationActionArgType.Integer }),
            [AutomationActions.DelayMs] = (1, AutomationActions.DelayMs, "DelayMs", new List<AutomationActionArgType> { AutomationActionArgType.Integer }),
            [AutomationActions.LoadAudio] = (1, AutomationActions.LoadAudio, "LoadAudioFile", new List<AutomationActionArgType> { AutomationActionArgType.String }),
        };


    }

    internal struct LanguageKeywordMetadata
    {
        internal bool toplevel;
        internal LanguageKeywordCommand parent;
        internal bool hasLayoutInfo;
        internal IXenonASTElement implementation;

        public LanguageKeywordMetadata((bool istoplevel, LanguageKeywordCommand parentcmd, bool hasLayout, IXenonASTElement impl) stuff)
        {
            toplevel = stuff.istoplevel;
            parent = stuff.parentcmd;
            hasLayoutInfo = stuff.hasLayout;
            implementation = stuff.impl;
        }

        public static implicit operator LanguageKeywordMetadata((bool, LanguageKeywordCommand INVALIDUNKNOWN, bool layoutinfo, IXenonASTElement impl) v)
        {
            return new LanguageKeywordMetadata(v);
        }
    }

    internal struct AutomationActionMetadata
    {
        internal int NumArgs;
        internal AutomationActions Action;
        internal string ActionName;
        internal List<AutomationActionArgType> OrderedArgTypes;

        public AutomationActionMetadata((int nargs, AutomationActions action, string name, List<AutomationActionArgType> argtypes) stuff)
        {
            NumArgs = stuff.nargs;
            Action = stuff.action;
            ActionName = stuff.name;
            OrderedArgTypes = stuff.argtypes;
        }

        public static implicit operator AutomationActionMetadata((int nargs, AutomationActions action, string name, List<AutomationActionArgType> argtypes) stuff)
        {
            return new AutomationActionMetadata(stuff);
        }
    }



    internal enum LanguageKeywordCommand
    {
        INVALIDUNKNOWN,
        SetVar,
        Break,
        Video,
        FilterImage,
        FullImage,
        FitImage,
        AutoFitImage,
        StitchedImage,
        LiturgyImage,
        Liturgy,
        LiturgyVerse,
        TitledLiturgyVerse,
        Reading,
        Sermon,
        AnthemTitle,
        TwoPartTitle,
        TextHymn,
        Verse,
        Copyright,
        ViewServices,
        ViewSeries,
        ApostlesCreed,
        NiceneCreed,
        LordsPrayer,
        Resource,
        Script,
        Script_LiturgyOff,
        Script_OrganIntro,
        Postset, // Doesn't translate into a true AST command, but is of same priority so we'd better add it here
        ScopedVariable,
        VariableScope,
    }

    public enum AutomationActionArgType
    {
        Integer,
        String,
    }


    public enum AutomationActions
    {
        PresetSelect,
        ProgramSelect,

        AutoTrans,
        CutTrans,

        DSK1On,
        DSK1Off,
        DSK1FadeOn,
        DSK1FadeOff,

        DSK2On,
        DSK2Off,
        DSK2FadeOn,
        DSK2FadeOff,

        USK1Off,
        USK1On,

        USK1SetTypeChroma,
        USK1SetTypeDVE,

        RecordStart,
        RecordStop,

        AutoTakePresetIfOnSlide,


        OpenAudioPlayer,
        LoadAudio,
        PlayAuxAudio,
        StopAuxAudio,
        PauseAuxAudio,
        ReplayAuxAudio,


        DelayMs,

        None,
        DriveNextSlide,
        Timer1Restart,
        PlayMedia,
        PauseMedia,
        StopMedia,
        RestartMedia,
        MuteMedia,
        UnMuteMedia,
        AuxSelect,
    }


}
