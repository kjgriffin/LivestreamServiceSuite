using System;
using System.Collections.Generic;
using System.Text;

namespace Xenon.Compiler
{
    static class LanguageKeywords
    {

        public static Dictionary<LanguageKeywordCommand, string> Commands = new Dictionary<LanguageKeywordCommand, string>()
        {
            [LanguageKeywordCommand.SetVar] = "set", 
            [LanguageKeywordCommand.Break] = "break", 
            [LanguageKeywordCommand.Video] = "video", 
            [LanguageKeywordCommand.FullImage] = "fullimage", 
            [LanguageKeywordCommand.FitImage] = "fitimage", 
            [LanguageKeywordCommand.AutoFitImage] = "autofitimage", 
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
        };


    }

    enum LanguageKeywordCommand
    {
        SetVar,
        Break,
        Video,
        FullImage,
        FitImage,
        AutoFitImage,
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
    }

}
