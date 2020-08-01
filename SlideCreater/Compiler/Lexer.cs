using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Printing;
using System.Text;
using System.Text.RegularExpressions;

namespace SlideCreater.Compiler
{
    class Lexer
    {

        public string Text { get; private set; } = string.Empty;

        public List<string> SplitWords { get; private set; }

        private List<string> _tokens;

        private int _tokenpos = 0;

        public Lexer()
        {
            // initialize SplitWords
            SplitWords = new List<string>() { "\r\n", ",", ".", " ", "(", ")", "#break", "#image", "#video" };
        }

        private void SplitAndKeep(string text, List<string> splitwords)
        {
            _tokens = new List<string>();


            for (int textindex = 0; textindex < text.Length;)
            {
                textindex = InnerCheck(ref text, splitwords, textindex);
            }


            // add the rest
            _tokens.Add(text);

            // remove empty tokens
            _tokens.RemoveAll(p => p == string.Empty);


            // debug
            _tokens.ForEach((string t) => { Debug.WriteLine(t); });

        }

        private int InnerCheck(ref string text, List<string> splitwords, int textindex)
        {
            for (int splitwordindex = 0; splitwordindex < splitwords.Count - 1; splitwordindex++)
            {
                if (textindex + splitwords[splitwordindex].Length <= text.Length)
                {
                    var s = text.Substring(textindex, splitwords[splitwordindex].Length);
                    if (s == splitwords[splitwordindex])
                    {
                        // match - remove
                        string before = text.Substring(0, textindex);
                        string word = splitwords[splitwordindex];
                        text = text.Substring(textindex + word.Length);

                        _tokens.Add(before);
                        _tokens.Add(word);

                        // advance to check against next char starting from start of split list
                        return 0;
                    }
                }
            }
            return textindex+1;
        }

        public void Tokenize(string input)
        {
            Text = input;
            // split based on split words
            SplitAndKeep(Text, SplitWords);
            _tokenpos = 0;
        }


        /// <summary>
        /// Check if next token is end of file
        /// </summary>
        /// <returns></returns>
        public bool InspectEOF()
        {
            if (_tokenpos > _tokens.Count - 1)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check if next token matches test (regex)
        /// </summary>
        /// <param name="test"></param>
        public bool Inspect(string test, bool strict = true)
        {
            if (InspectEOF())
            {
                throw new ArgumentOutOfRangeException($"Unexpected token {test}. EOF");
            }
            string pattern = test;
            if (strict)
            {
                pattern = $"^{test}$";
            }
            var m = Regex.Match(_tokens[_tokenpos], pattern);

            return m.Success;
        }

        /// <summary>
        /// Tries to advance lexer position to end of text. Returns false if no match found. 
        /// </summary>
        /// <param name="text"></param>
        public bool Gobble(string text)
        {
            if (_tokens[_tokenpos] == text)
            {
                _tokenpos++;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns current token, advances to next token.
        /// </summary>
        /// <returns></returns>
        public string Consume()
        {
            return _tokens[_tokenpos++];
        }

        /// <summary>
        /// Advance to the next non-whitespace token
        /// </summary>
        public void GobbleWhitespace()
        {
            while (_tokenpos < _tokens.Count)
            {
                if (Regex.Match(_tokens[_tokenpos], "\\s").Success)
                {
                    _tokenpos += 1;
                }
                else
                {
                    return;
                }
            }
        }
    }
}
