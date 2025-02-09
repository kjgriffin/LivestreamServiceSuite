﻿using System.Collections.Generic;

using Xenon.Compiler.AST;
using Xenon.LayoutInfo;
using Xenon.Renderer;

namespace Xenon.Compiler.LanguageDefinition
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

        public static Dictionary<LanguageKeywordCommand, (ILayoutInfoResolver<ALayoutInfo> layoutResolver, ISlideLayoutPrototypePreviewer<ALayoutInfo> prototypicalLayoutPreviewer, string defaultJsonFile, string type)> LayoutForType = new Dictionary<LanguageKeywordCommand, (ILayoutInfoResolver<ALayoutInfo>, ISlideLayoutPrototypePreviewer<ALayoutInfo>, string, string)>
        {
            //[LanguageKeywordCommand.TwoPartTitle] = (new _2TitleSlideLayoutInfo(), new TwoPartTitleSlideRenderer(), ""),
            [LanguageKeywordCommand.TwoPartTitle] = (new ShapeAndTextLayoutInfo(), new ShapeAndTextRenderer(), "_2TitleLayoutInfo_Default.json", "json"),
            [LanguageKeywordCommand.StitchedImage] = (new StitchedImageSlideLayoutInfo(), new StitchedImageRenderer(), "", "json"),
            [LanguageKeywordCommand.TitledLiturgyVerse] = (new TitledLiturgyVerseSlideLayoutInfo(), new TitledLiturgyVerseSlideRenderer(), "", "json"),
            [LanguageKeywordCommand.Liturgy2] = (new ResponsiveLiturgySlideLayoutInfo(), new ResponsiveLiturgyRenderer(), "", "json"),
            [LanguageKeywordCommand.TitledLiturgyVerse2] = (new TitledResponsiveLiturgySlideLayoutInfo(), new TitledResponsiveLiturgyRenderer(), "", "json"),
            [LanguageKeywordCommand.UpNext] = (new ShapeAndTextLayoutInfo(), new ShapeAndTextRenderer(), "UpNextLayoutInfo_Default.json", "json"),
            [LanguageKeywordCommand.AnthemTitle] = (new ShapeAndTextLayoutInfo(), new ShapeAndTextRenderer(), "AnthemTitleLayoutInfo_Default.json", "json"),
            [LanguageKeywordCommand.Reading] = (new ShapeAndTextLayoutInfo(), new ShapeAndTextRenderer(), "ReadingLayoutInfo_Default.json", "json"),
            [LanguageKeywordCommand.Sermon] = (new ShapeAndTextLayoutInfo(), new ShapeAndTextRenderer(), "SermonLayoutInfo_Default.json", "json"),
            [LanguageKeywordCommand.CustomText] = (new ShapeAndTextLayoutInfo(), new ShapeAndTextRenderer(), "", "json"),
            [LanguageKeywordCommand.CustomDraw] = (new ShapeImageAndTextLayoutInfo(), new ShapeImageAndTextRenderer(), "", "json"),
            [LanguageKeywordCommand.ComplexText] = (new ComplexShapeImageAndTextLayoutInfo(), new ComplexShapeImageAndTextRenderer(), "", "json"),
            [LanguageKeywordCommand.TextHymn] = (new TextHymnLayoutInfo(), new HymnTextVerseRenderer(), "", "json"),
            [LanguageKeywordCommand.LiturgyImage] = (new AdvancedImagesSlideLayoutInfo(), new AdvancedImageSlideRenderer(), "", "json"),
            [LanguageKeywordCommand.HTML] = (new HTMLLayoutInfo(), new HTMLSlideRenderer(), "", "html"),
            [LanguageKeywordCommand.HTML2] = (new HTMLLayoutInfo(), new HTMLSlideRenderer(), "", "html"),
        };

        public static List<string> WholeWords = new List<string>()
        {
            "solidcolorcanvas",
            "crop",
            "imgset",
            "centerassetfill",
            "assets",
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
            "parameter",
            "scriptname",
            "name",
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
            [LanguageKeywordCommand.ReStitchedHymn] = "restitchedhymn",
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
            [LanguageKeywordCommand.CalledScript] = "callscript",
            [LanguageKeywordCommand.NamedScript] = "namedscript",
            [LanguageKeywordCommand.Postset] = "postset",
            [LanguageKeywordCommand.VariableScope] = "scope",
            [LanguageKeywordCommand.ScopedVariable] = "var",
            [LanguageKeywordCommand.PostFilter] = "postfilter",
            [LanguageKeywordCommand.UpNext] = "upnext",
            [LanguageKeywordCommand.CustomText] = "customtext",
            [LanguageKeywordCommand.CustomDraw] = "customdraw",
            [LanguageKeywordCommand.ComplexText] = "complextext",
            [LanguageKeywordCommand.DynamicControllerDef] = "dynamiccontroller",
            [LanguageKeywordCommand.HTML] = "html",
            [LanguageKeywordCommand.HTML2] = "html2",
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
            [LanguageKeywordCommand.ReStitchedHymn] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, true, new XenonASTReStitchedHymn()),
            [LanguageKeywordCommand.LiturgyImage] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, true, false, new XenonASTLiturgyImage()),
            [LanguageKeywordCommand.Liturgy] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, false, new XenonASTLiturgy()),
            [LanguageKeywordCommand.Liturgy2] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, true, false, new XenonASTLiturgy2()),
            [LanguageKeywordCommand.LiturgyVerse] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, false, new XenonASTLiturgyVerse()),
            [LanguageKeywordCommand.TitledLiturgyVerse] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, true, false, new XenonASTTitledLiturgyVerse()),
            [LanguageKeywordCommand.TitledLiturgyVerse2] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, true, false, new XenonASTTitledLiturgy()),
            [LanguageKeywordCommand.Reading] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, true, false, new XenonASTReading()),
            [LanguageKeywordCommand.Sermon] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, true, false, new XenonASTSermon()),
            [LanguageKeywordCommand.AnthemTitle] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, true, false, new XenonASTAnthemTitle()),
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
            [LanguageKeywordCommand.CalledScript] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, false, new XenonASTCalledScript()),
            [LanguageKeywordCommand.NamedScript] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, false, new XenonASTNamedScript()),
            [LanguageKeywordCommand.Postset] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, false, new XenonASTExpression()),
            [LanguageKeywordCommand.ScopedVariable] = (true, LanguageKeywordCommand.VariableScope, false, false, new XenonASTScopedVariable()),
            [LanguageKeywordCommand.VariableScope] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, false, new XenonASTVariableScope()),
            [LanguageKeywordCommand.PostFilter] = (true, LanguageKeywordCommand.VariableScope, false, false, new XenonASTPostFilter()),
            [LanguageKeywordCommand.UpNext] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, true, false, new XenonASTUpNext()),
            [LanguageKeywordCommand.CustomText] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, true, false, new XenonASTShapesAndText()),
            [LanguageKeywordCommand.CustomDraw] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, true, false, new XenonASTShapesImagesAndText()),
            [LanguageKeywordCommand.ComplexText] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, true, true, new XenonASTShapesImagesAndTextComplex()),
            [LanguageKeywordCommand.DynamicControllerDef] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, false, false, new XenonASTDynamicController()),
            [LanguageKeywordCommand.HTML] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, true, true, new XenonASTHTML()),
            [LanguageKeywordCommand.HTML2] = (true, LanguageKeywordCommand.INVALIDUNKNOWN, true, true, new XenonASTHtml2()),
        };

    }

}
