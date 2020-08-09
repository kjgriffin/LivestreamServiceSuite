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

        public string CurrentToken => _tokens[_tokenpos];

        public Lexer()
        {
            // initialize SplitWords

            SplitWords = new List<string>();
            SplitWords.AddRange(LanguageKeywords.Commands.Values);
            List<string> Seperators = new List<string>() {
                "\r\n",
                "//",
                "?",
                ";",
                ",",
                ".",
                " ",
                "(",
                ")",
                "{",
                "}",
                "#",
            };
            SplitWords.AddRange(Seperators);
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
            for (int splitwordindex = 0; splitwordindex < splitwords.Count; splitwordindex++)
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
            return textindex + 1;
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
        /// View the current token without consuming it
        /// </summary>
        /// <returns>The current token</returns>
        public string Peek()
        {
            return _tokens[_tokenpos];
        }

        /// <summary>
        /// View the next token without consuming it.
        /// </summary>
        /// <returns>The next token</returns>
        public string PeekNext()
        {
            if (_tokenpos + 1 < _tokens.Count)
            {
                return _tokens[_tokenpos + 1];
            }
            return "";
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
        /// Returns a compound string of all tokens until the current token matches the test
        /// </summary>
        /// <param name="test">A regex test pattern</param>
        /// <returns></returns>
        public string ConsumeUntil(string test)
        {
            StringBuilder sb = new StringBuilder();
            while(!Inspect(test))
            {
                sb.Append(Consume());
            }
            return sb.ToString();
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
