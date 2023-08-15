using System;
using System.Collections.Generic;
using System.Text;

namespace Concord
{
    public interface IBibleInfoProvider
    {
        /// <summary>
        /// Returns the number of chapters in the book
        /// </summary>
        /// <param name="Book"></param>
        /// <returns></returns>
        int GetChapterCount(string Book);

        /// <summary>
        /// Gets the number of verses in a chapter of a book
        /// </summary>
        /// <param name="Book"></param>
        /// <param name="Chapter"></param>
        /// <returns></returns>
        int GetVerseCount(string Book, int Chapter);
    }

}
