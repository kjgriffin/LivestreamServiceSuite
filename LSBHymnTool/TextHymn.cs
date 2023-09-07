using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSBHymnTool
{
    public class TextHymn
    {
        public string Name;
        public string Number;
        public List<TextVerse> Verses = new List<TextVerse>();
    }
    public class TextVerse
    {
        public string StanzaId;
        public string Words;
    }
}
