using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Midnight.LanguageDef
{

    /// <summary>
    /// Defines the language keywords and seperators.
    /// </summary>
    static class LanguageDefs
    {

        /// <summary>
        /// Single line comment. Will be removed by preproc.
        /// </summary>
        public static string SingleLineComment { get; private set; } = "//";
        /// <summary>
        /// Start of block comment. Will be removed by preproc.
        /// </summary>
        public static string StartBlockComment { get; private set; } = "/*";
        /// <summary>
        /// End of block comment. Will be removed by preproc.
        /// </summary>
        public static string EndBlockComment { get; private set; } = "*/";


        /// <summary>
        /// Marks the next token as escaped.
        /// </summary>
        public static string EscapeSeq { get; private set; } = @"\";

        /// <summary>
        /// Reserved keywords in the language. These have the highest match precendence.
        /// </summary>
        public static List<string> ReservedWords { get; private set; } = Commands.Select(p => p.Value).ToList();


        /// <summary>
        /// All tokens that input should be split by. These have second highest match precendence.
        /// </summary>
        public static List<string> Seperators { get; private set; } = new List<string>()
        {
             "\r\n",
            System.Environment.NewLine,
            "\r",
            "\n",
            "(",
            ")",
            ",",
            ".",
            ";",
            " ",
            "{",
            "}",
            "#",
            "$",
            "\"",
            "\'",
        };


        public static Dictionary<LanguageKeywordCommand, string> Commands = new Dictionary<LanguageKeywordCommand, string>()
        {
            [LanguageKeywordCommand.Break] = "break",
            [LanguageKeywordCommand.Image] = "image",
            [LanguageKeywordCommand.Video] = "video",
            [LanguageKeywordCommand.FullImage] = "fullimage",
            [LanguageKeywordCommand.FitImage] = "fitimage",
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
