using Concord;

using System;
using System.Linq;

namespace LutheRun.Parsers
{
    public class ConcordReadingVerifyier
    {
        public static bool ValidateReadingIsGeneratable(BibleTranslations translation, string reference)
        {
            try
            {
                LSBReferenceUnpacker refdecoder = new LSBReferenceUnpacker();
                var sections = refdecoder.ParseSections(reference);

                if (!sections.Any())
                {
                    // ref is empty
                    return false;
                }

                IBibleAPI bible = BibleBuilder.BuildAPI(translation);

                int chars = 0;

                foreach (var section in sections)
                {
                    foreach (var vlist in refdecoder.EnumerateVerses(section, bible))
                    {
                        var x = bible.GetVerse(vlist.Book, vlist.Chapter, vlist.Verse);
                        chars += x.Text.Length;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
    }
}
