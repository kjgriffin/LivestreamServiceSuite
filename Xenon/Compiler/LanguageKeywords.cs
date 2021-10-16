using System;
using System.Collections.Generic;
using System.Text;

namespace Xenon.Compiler
{
    static class LanguageKeywords
    {

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
        };

        public static Dictionary<LanguageKeywordCommand, LanguageKeywordInfo> LanguageKeywordMetadata = new Dictionary<LanguageKeywordCommand, LanguageKeywordInfo>()
        {
            [LanguageKeywordCommand.Script_LiturgyOff] = (true, LanguageKeywordCommand.INVALIDUNKNOWN),
            [LanguageKeywordCommand.Script_OrganIntro] = (true, LanguageKeywordCommand.INVALIDUNKNOWN),
            [LanguageKeywordCommand.SetVar] = (true, LanguageKeywordCommand.INVALIDUNKNOWN),
            [LanguageKeywordCommand.Break] = (false, LanguageKeywordCommand.Liturgy),
            [LanguageKeywordCommand.Video] = (true, LanguageKeywordCommand.INVALIDUNKNOWN),
            [LanguageKeywordCommand.FilterImage] = (true, LanguageKeywordCommand.INVALIDUNKNOWN),
            [LanguageKeywordCommand.FullImage] = (true, LanguageKeywordCommand.INVALIDUNKNOWN),
            [LanguageKeywordCommand.FitImage] = (true, LanguageKeywordCommand.INVALIDUNKNOWN),
            [LanguageKeywordCommand.AutoFitImage] = (true, LanguageKeywordCommand.INVALIDUNKNOWN),
            [LanguageKeywordCommand.StitchedImage] = (true, LanguageKeywordCommand.INVALIDUNKNOWN),
            [LanguageKeywordCommand.LiturgyImage] = (true, LanguageKeywordCommand.INVALIDUNKNOWN),
            [LanguageKeywordCommand.Liturgy] = (true, LanguageKeywordCommand.INVALIDUNKNOWN),
            [LanguageKeywordCommand.LiturgyVerse] = (true, LanguageKeywordCommand.INVALIDUNKNOWN),
            [LanguageKeywordCommand.TitledLiturgyVerse] = (true, LanguageKeywordCommand.INVALIDUNKNOWN),
            [LanguageKeywordCommand.Reading] = (true, LanguageKeywordCommand.INVALIDUNKNOWN),
            [LanguageKeywordCommand.Sermon] = (true, LanguageKeywordCommand.INVALIDUNKNOWN),
            [LanguageKeywordCommand.AnthemTitle] = (true, LanguageKeywordCommand.INVALIDUNKNOWN),
            [LanguageKeywordCommand.TwoPartTitle] = (true, LanguageKeywordCommand.INVALIDUNKNOWN),
            [LanguageKeywordCommand.TextHymn] = (true, LanguageKeywordCommand.INVALIDUNKNOWN),
            [LanguageKeywordCommand.Verse] = (false, LanguageKeywordCommand.TextHymn),
            [LanguageKeywordCommand.Copyright] = (true, LanguageKeywordCommand.INVALIDUNKNOWN),
            [LanguageKeywordCommand.ViewServices] = (true, LanguageKeywordCommand.INVALIDUNKNOWN),
            [LanguageKeywordCommand.ViewSeries] = (true, LanguageKeywordCommand.INVALIDUNKNOWN),
            [LanguageKeywordCommand.ApostlesCreed] = (true, LanguageKeywordCommand.INVALIDUNKNOWN),
            [LanguageKeywordCommand.NiceneCreed] = (true, LanguageKeywordCommand.INVALIDUNKNOWN),
            [LanguageKeywordCommand.LordsPrayer] = (true, LanguageKeywordCommand.INVALIDUNKNOWN),
            [LanguageKeywordCommand.Resource] = (true, LanguageKeywordCommand.INVALIDUNKNOWN),
            [LanguageKeywordCommand.Script] = (true, LanguageKeywordCommand.INVALIDUNKNOWN),
            [LanguageKeywordCommand.Postset] = (true, LanguageKeywordCommand.INVALIDUNKNOWN),
        };


    }

    internal struct LanguageKeywordInfo
    {
        internal bool toplevel;
        internal LanguageKeywordCommand parent;

        public LanguageKeywordInfo((bool istoplevel, LanguageKeywordCommand parentcmd) stuff)
        {
            toplevel = stuff.istoplevel;
            parent = stuff.parentcmd;
        }

        public static implicit operator LanguageKeywordInfo((bool, LanguageKeywordCommand INVALIDUNKNOWN) v)
        {
            return new LanguageKeywordInfo(v);
        }
    }


    enum LanguageKeywordCommand
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
    }

}
