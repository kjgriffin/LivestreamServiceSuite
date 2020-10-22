using System;
using System.Collections.Generic;
using System.Text;

namespace Xenon.Compiler
{
    static class LanguageKeywords
    {

        public static Dictionary<LanguageKeywordCommand, string> Commands = new Dictionary<LanguageKeywordCommand, string>()
        {
            [LanguageKeywordCommand.Break] = "break", 
            [LanguageKeywordCommand.Image] = "image", 
            [LanguageKeywordCommand.Video] = "video", 
            [LanguageKeywordCommand.FullImage] = "fullimage", 
            [LanguageKeywordCommand.FitImage] = "fitimage", 
            [LanguageKeywordCommand.AutoFitImage] = "autofitimage", 
            [LanguageKeywordCommand.LiturgyImage] = "litimage", 
            [LanguageKeywordCommand.Liturgy] = "liturgy", 
            [LanguageKeywordCommand.LiturgyVerse] = "litverse", 
            [LanguageKeywordCommand.Reading] = "reading", 
            [LanguageKeywordCommand.Sermon] = "sermon", 
            [LanguageKeywordCommand.TextHymn] = "texthymn", 
            [LanguageKeywordCommand.Verse] = "verse", 
            [LanguageKeywordCommand.Copyright] = "copyright", 
            [LanguageKeywordCommand.ViewServices] = "viewservices", 
            [LanguageKeywordCommand.ViewSeries] = "viewseries", 
            [LanguageKeywordCommand.ApostlesCreed] = "apostlescreed", 
            [LanguageKeywordCommand.NiceneCreed] = "nicenecreed", 
            [LanguageKeywordCommand.LordsPrayer] = "lordsprayer", 
        };


    }

    enum LanguageKeywordCommand
    {
        Break,
        Image,
        Video,
        FullImage,
        FitImage,
        AutoFitImage,
        LiturgyImage,
        Liturgy,
        LiturgyVerse,
        Reading,
        Sermon,
        TextHymn,
        Verse,
        Copyright,
        ViewServices,
        ViewSeries,
        ApostlesCreed,
        NiceneCreed,
        LordsPrayer,
    }

}
