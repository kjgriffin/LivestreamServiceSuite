using IntegratedPresenterAPIInterop;

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

        public static Dictionary<LanguageKeywordCommand, (ILayoutInfoResolver<ALayoutInfo> layoutResolver, ISlideLayoutPrototypePreviewer<ALayoutInfo> prototypicalLayoutPreviewer, string defaultJsonFile)> LayoutForType = new Dictionary<LanguageKeywordCommand, (ILayoutInfoResolver<ALayoutInfo>, ISlideLayoutPrototypePreviewer<ALayoutInfo>, string)>
        {
            [LanguageKeywordCommand.TwoPartTitle] = (new _2TitleSlideLayoutInfo(), new TwoPartTitleSlideRenderer(), ""),
            [LanguageKeywordCommand.StitchedImage] = (new StitchedImageSlideLayoutInfo(), new StitchedImageRenderer(), ""),
            [LanguageKeywordCommand.TitledLiturgyVerse] = (new TitledLiturgyVerseSlideLayoutInfo(), new TitledLiturgyVerseSlideRenderer(), ""),
            [LanguageKeywordCommand.Liturgy2] = (new ResponsiveLiturgySlideLayoutInfo(), new ResponsiveLiturgyRenderer(), ""),
            [LanguageKeywordCommand.TitledLiturgyVerse2] = (new TitledResponsiveLiturgySlideLayoutInfo(), new TitledResponsiveLiturgyRenderer(), ""),
            [LanguageKeywordCommand.UpNext] = (new ShapeAndTextLayoutInfo(), new ShapeAndTextRenderer(), "UpNextLayoutInfo_Default.json"),
            [LanguageKeywordCommand.CustomText] = (new ShapeAndTextLayoutInfo(), new ShapeAndTextRenderer(), ""),
            [LanguageKeywordCommand.CustomDraw] = (new ShapeImageAndTextLayoutInfo(), new ShapeImageAndTextRenderer(), ""),
            [LanguageKeywordCommand.TextHymn] = (new TextHymnLayoutInfo(), new HymnTextVerseRenderer(), ""),
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
            "coloredithsv",
            "colorshifthsv",
            "colortint",
            "coloruntint",
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
            [LanguageKeywordCommand.Liturgy2] = "liturgyresponsive",
            [LanguageKeywordCommand.LiturgyVerse] = "litverse",
            [LanguageKeywordCommand.TitledLiturgyVerse] = "tlverse",
            [LanguageKeywordCommand.TitledLiturgyVerse2] = "tlit",
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
            [LanguageKeywordCommand.Scripted] = "scripted",
            [LanguageKeywordCommand.Postset] = "postset",
            [LanguageKeywordCommand.VariableScope] = "scope",
            [LanguageKeywordCommand.ScopedVariable] = "var",
            [LanguageKeywordCommand.PostFilter] = "postfilter",
            [LanguageKeywordCommand.UpNext] = "upnext",
            [LanguageKeywordCommand.CustomText] = "customtext",
            [LanguageKeywordCommand.CustomDraw] = "customdraw",
        };

        public static Dictionary<LanguageKeywordCommand, LanguageKeywordMetadata> LanguageKeywordMetadata = new Dictionary<LanguageKeywordCommand, LanguageKeywordMetadata>()
        {
            [LanguageKeywordCommand.Script_LiturgyOff] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, false, new XenonASTPrefabScriptLiturgyOff()),
            [LanguageKeywordCommand.Script_OrganIntro] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, false, new XenonASTPrefabScriptOrganIntro()),
            [LanguageKeywordCommand.SetVar] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, false, new XenonASTSetVariable()),
            [LanguageKeywordCommand.Break] = (false, LanguageKeywordCommand.Liturgy, false, false, new XenonASTSlideBreak()),
            [LanguageKeywordCommand.Video] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, false, new XenonASTVideo()),
            [LanguageKeywordCommand.FilterImage] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, false, new XenonASTFilterImage()),
            [LanguageKeywordCommand.FullImage] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, false, new XenonASTFullImage()),
            [LanguageKeywordCommand.FitImage] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, false, new XenonASTFitImage()),
            [LanguageKeywordCommand.AutoFitImage] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, false, new XenonASTAutoFitImage()),
            [LanguageKeywordCommand.StitchedImage] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, true, true, new XenonASTStitchedHymn()),
            [LanguageKeywordCommand.LiturgyImage] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, false, new XenonASTLiturgyImage()),
            [LanguageKeywordCommand.Liturgy] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, false, new XenonASTLiturgy()),
            [LanguageKeywordCommand.Liturgy2] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, true, false, new XenonASTLiturgy2()),
            [LanguageKeywordCommand.LiturgyVerse] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, false, new XenonASTLiturgyVerse()),
            [LanguageKeywordCommand.TitledLiturgyVerse] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, true, false, new XenonASTTitledLiturgyVerse()),
            [LanguageKeywordCommand.TitledLiturgyVerse2] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, true, false, new XenonASTTitledLiturgy()),
            [LanguageKeywordCommand.Reading] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, false, new XenonASTReading()),
            [LanguageKeywordCommand.Sermon] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, false, new XenonASTSermon()),
            [LanguageKeywordCommand.AnthemTitle] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, false, new XenonASTAnthemTitle()),
            [LanguageKeywordCommand.TwoPartTitle] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, true, false, new XenonAST2PartTitle()),
            [LanguageKeywordCommand.TextHymn] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, true, false, new XenonASTTextHymn()),
            [LanguageKeywordCommand.Verse] = (false, LanguageKeywordCommand.TextHymn, false, false, new XenonASTHymnVerse()),
            [LanguageKeywordCommand.Copyright] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, false, new XenonASTPrefabCopyright()),
            [LanguageKeywordCommand.ViewServices] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, false, new XenonASTPrefabViewServices()),
            [LanguageKeywordCommand.ViewSeries] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, false, new XenonASTPrefabViewSeries()),
            [LanguageKeywordCommand.ApostlesCreed] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, false, new XenonASTPrefabApostlesCreed()),
            [LanguageKeywordCommand.NiceneCreed] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, false, new XenonASTPrefabNiceneCreed()),
            [LanguageKeywordCommand.LordsPrayer] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, false, new XenonASTPrefabLordsPrayer()),
            [LanguageKeywordCommand.Resource] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, false, new XenonASTResource()),
            [LanguageKeywordCommand.Script] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, false, new XenonASTScript()),
            [LanguageKeywordCommand.Scripted] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, false, new XenonASTAsScripted()),
            [LanguageKeywordCommand.Postset] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, false, new XenonASTExpression()),
            [LanguageKeywordCommand.ScopedVariable] = (true, LanguageKeywordCommand.VariableScope, false, false, new XenonASTScopedVariable()),
            [LanguageKeywordCommand.VariableScope] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, false, new XenonASTVariableScope()),
            [LanguageKeywordCommand.PostFilter] = (true, LanguageKeywordCommand.VariableScope, false, false, new XenonASTPostFilter()),
            [LanguageKeywordCommand.UpNext] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, true, false, new XenonASTUpNext()),
            [LanguageKeywordCommand.CustomText] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, true, false, new XenonASTShapesAndText()),
            [LanguageKeywordCommand.CustomDraw] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, true, false, new XenonASTShapesImagesAndText()),
        };

    }

    internal struct LanguageKeywordMetadata
    {
        internal bool toplevel;
        internal LanguageKeywordCommand parent;
        internal bool hasLayoutInfo;
        internal bool acceptsOverrideExportMode;
        internal IXenonASTElement implementation;

        public LanguageKeywordMetadata((bool istoplevel, LanguageKeywordCommand parentcmd, bool hasLayout, bool acceptOverrideExportMode, IXenonASTElement impl) stuff)
        {
            toplevel = stuff.istoplevel;
            parent = stuff.parentcmd;
            hasLayoutInfo = stuff.hasLayout;
            implementation = stuff.impl;
            this.acceptsOverrideExportMode = stuff.acceptOverrideExportMode;
        }

        public static implicit operator LanguageKeywordMetadata((bool, LanguageKeywordCommand INVALIDUNKNOWN, bool layoutinfo, bool overrideexport, IXenonASTElement impl) v)
        {
            return new LanguageKeywordMetadata(v);
        }
    }



    public enum LanguageKeywordCommand
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
        PostFilter,
        Liturgy2,
        UpNext,
        CustomText,
        Scripted,
        CustomDraw,
        TitledLiturgyVerse2,
    }

}
