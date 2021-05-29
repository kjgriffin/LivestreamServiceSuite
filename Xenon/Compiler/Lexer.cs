using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Xenon.Compiler
{
    class Lexer
    {

        public static readonly string EOFText = "$EOF$";

        public string SingleLineComment { get; } = "//";
        public string BeginBlockComment { get; } = "/*";
        public string EndBlockComment { get; } = "*/";

        public string Text { get; private set; } = string.Empty;

        public List<string> SplitWords { get; private set; }


        private List<string> _tokens;

        private int _tokenpos = 0;

        public string CurrentToken
        {
            get
            {
                if (_tokenpos >= _tokens.Count)
                {
                    return EOFText;
                }
                return _tokens[_tokenpos];
            }
        }

        public int MaxChecks { get; private set; }

        private int peeks = 0;
        private int peeknexts = 0;
        private int inspections = 0;
        public void ClearInspectionState()
        {
            MaxChecks = new List<int>() { peeks, peeknexts, inspections, MaxChecks }.OrderByDescending(p => p).First();
            peeks = 0;
            peeknexts = 0;
            inspections = 0;
        }

        /// <summary>
        /// Restores all the state of the lexer
        /// </summary>
        public void Reset()
        {
            ClearInspectionState();
            _tokenpos = 0;
        }

        /// <summary>
        /// Max number of times a token can be inspected before an error is thrown. Usually indicative of unexpected token.
        /// </summary>
        public int MaxInspections { get; set; } = 10000;

        private ILexerLogger Logger;

        public Lexer(ILexerLogger Logger)
        {

            this.Logger = Logger;
            // initialize SplitWords

            SplitWords = new List<string>();
            SplitWords.AddRange(LanguageKeywords.Commands.Values);
            SplitWords.AddRange(LanguageKeywords.WholeWords);
            List<string> Seperators = new List<string>() {
                "\r\n",
                "//",
                "::",
                "?",
                ";",
                ",",
                ".",
                " ",
                "(",
                ")",
                "{",
                "}",
                "[",
                "]",
                "#",
                "\\",
                "$",
                "\"",
                "=",
            };
            SplitWords.AddRange(Seperators);
            SplitWords = SplitWords.OrderByDescending(s => s.Length).ToList();
        }

        private List<string> SplitAndKeep(string text, List<string> splitwords)
        {
            //_tokens = new List<string>();

            List<string> tokens = new List<string>();

            for (int textindex = 0; textindex < text.Length;)
            {
                textindex = InnerCheck(ref text, splitwords, textindex, tokens);
            }


            // add the rest
            //_tokens.Add(text);
            tokens.Add(text);

            // remove empty tokens
            //_tokens.RemoveAll(p => p == string.Empty);
            tokens.RemoveAll(p => p == string.Empty);


            // debug
            //_tokens.ForEach((string t) => { Debug.WriteLine(t); });
            return tokens;

        }

        private int InnerCheck(ref string text, List<string> splitwords, int textindex, List<string> tokens)
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

                        tokens.Add(before);
                        tokens.Add(word);

                        // advance to check against next char starting from start of split list
                        return 0;
                    }
                }
            }
            return textindex + 1;
        }


        /// <summary>
        /// Removes all comment lines from source input.
        /// </summary>
        /// <param name="input">Text</param>
        /// <returns>Source input without comments.</returns>
        public string StripComments(string input)
        {
            List<string> seperators = new List<string>()
            {
                System.Environment.NewLine,
                //"\r\n",
                //"\n",
                //"\r",
                BeginBlockComment,
                EndBlockComment,
                SingleLineComment,
            };

            List<string> parts = SplitAndKeep(input, seperators);

            List<string> noblocks = new List<string>();
            List<string> nocomments = new List<string>();
            // remove all block comments

            bool inblock = false;
            foreach (var p in parts)
            {
                if (p == BeginBlockComment)
                {
                    inblock = true;
                }

                if (!inblock)
                {
                    noblocks.Add(p);
                }

                if (p == EndBlockComment)
                {
                    inblock = false;
                }
            }

            // remove single line comments
            bool iscomment = false;
            foreach (var b in noblocks)
            {
                if (b == SingleLineComment)
                {
                    iscomment = true;
                }

                if (!iscomment)
                {
                    nocomments.Add(b);
                }

                if (b == System.Environment.NewLine)
                {
                    iscomment = false;
                }
            }

            StringBuilder sb = new StringBuilder();
            foreach (var w in nocomments)
            {
                sb.Append(w);
            }

            return sb.ToString();

        }

        public void Tokenize(string input)
        {
            Text = input;
            // split based on split words
            _tokens = SplitAndKeep(Text, SplitWords);
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
            if (inspections > MaxInspections)
            {
                Logger.Log(new XenonCompilerMessage() { ErrorMessage = $"Possible stuck compiler. {inspections} concurrent inspections on token.", ErrorName = "Lexer Overflow", Generator = "Lexer", Level = XenonCompilerMessageType.Error, Token = CurrentToken });
                throw new Exception($"Too many inspections of token '{_tokens[_tokenpos]}'");
            }
            if (InspectEOF())
            {
                Logger.Log(new XenonCompilerMessage() { ErrorMessage = $"Lexer asked to inspect for token <'{Regex.Escape(test)}'>. Got EOF instead.", ErrorName = "Unexpected EOF", Generator = "Lexer", Level = XenonCompilerMessageType.Debug, Token = EOFText });
                throw new ArgumentOutOfRangeException($"Unexpected token {test}. EOF");
            }
            string pattern = test;
            if (strict)
            {
                pattern = $"^{Regex.Escape(test)}$";
            }
            var m = Regex.Match(_tokens[_tokenpos], pattern);

            inspections++;
            return m.Success;
        }


        /// <summary>
        /// View the current token without consuming it
        /// </summary>
        /// <returns>The current token</returns>
        public string Peek()
        {
            if (peeks > MaxInspections)
            {
                throw new Exception($"Too many peeks of token '{_tokens[_tokenpos]}'");
            }
            peeks++;
            return _tokens[_tokenpos];
        }

        /// <summary>
        /// View the next token without consuming it.
        /// </summary>
        /// <returns>The next token</returns>
        public string PeekNext()
        {
            if (peeknexts > MaxInspections)
            {
                throw new Exception($"Too many peeks of token '{_tokens[_tokenpos]}'");
            }
            peeknexts++;
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
            if (InspectEOF())
            {
                Logger.Log(new XenonCompilerMessage() { ErrorMessage = $"Lexer expecting token: '{text}'", ErrorName = "Unexpected eof", Generator = "Lexer.Gobble", Inner = "", Level = XenonCompilerMessageType.Error, Token = EOFText });
                throw new Exception();
            }
            if (_tokens[_tokenpos] == text)
            {
                ClearInspectionState();
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
            ClearInspectionState();
            return _tokens[_tokenpos++];
        }

        /// <summary>
        /// Returns a compound string of all tokens until the current token matches the test
        /// </summary>
        /// <param name="test">A regex test pattern</param>
        /// <param name="errormessage">Additional error message if until token not found</param>
        /// <returns></returns>
        public string ConsumeUntil(string test, string errormessage = "")
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                while (!Inspect(test))
                {
                    sb.Append(Consume());
                }
            }
            catch (Exception ex)
            {
                Logger.Log(new XenonCompilerMessage() { ErrorName = "Expecting missing token", ErrorMessage = $"Expecting token <'{test}'>.\r\nAdditional Info: {errormessage}", Generator = "Lexer.ConsumeUnti", Inner = ex.Message, Level = XenonCompilerMessageType.Error, Token = CurrentToken });
                throw ex;
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
                    ClearInspectionState();
                    _tokenpos += 1;
                }
                else
                {
                    return;
                }
            }
        }


        public string ConsumeArgList_StartSeq { get; set; } = "(";
        public string ConsumeArgList_EndSeq { get; set; } = ")";
        public string ConsumeArgList_SepSeq { get; set; } = ",";
        public string ConsumeArgList_EncSeq { get; set; } = "\"";

        /// <summary>
        /// Will consume all tokens starting with '(' and ending with ')'. Will assign values to paramaters.
        /// </summary>
        /// <returns>Values captured for each parameter</returns>
        public Dictionary<string, string> ConsumeArgList(bool areenclosed = false, params string[] paramnames)
        {
            Dictionary<string, string> res = new Dictionary<string, string>();

            GobbleandLog(ConsumeArgList_StartSeq);

            int pnum = 0;
            foreach (var p in paramnames)
            {
                string helpmsg = $"while trying to parse parameter: {p}";
                GobbleWhitespace();

                if (areenclosed)
                {
                    GobbleandLog(ConsumeArgList_EncSeq, helpmsg);
                }


                string paramendseq = areenclosed ? ConsumeArgList_EncSeq : pnum < paramnames.Length - 1 ? ConsumeArgList_SepSeq : ConsumeArgList_EndSeq;
                try
                {
                    res[p] = ConsumeUntil(paramendseq);
                }
                catch (Exception ex)
                {
                    Logger.Log(new XenonCompilerMessage() { ErrorName = "Bad parameter", ErrorMessage = $"Mal-formed parameter {helpmsg}.", Generator = "Lexer.ConsumeArgList", Inner = ex.Message, Level = XenonCompilerMessageType.Error, Token = CurrentToken });
                    throw ex;
                }


                if (areenclosed)
                {
                    GobbleandLog(ConsumeArgList_EncSeq, helpmsg);
                    GobbleWhitespace();
                }

                if (pnum < paramnames.Length - 1)
                {
                    GobbleandLog(ConsumeArgList_SepSeq, helpmsg);
                }

                pnum++;

            }

            GobbleWhitespace();
            GobbleandLog(ConsumeArgList_EndSeq);

            return res;
        }

        /// <summary>
        /// Checks the current token matches 'text' and if so advances to next token. Logs messages on failure.
        /// </summary>
        /// <param name="text">Test to check token against</param>
        /// <param name="messages">List of messages to log to</param>
        /// <returns>True if matched. False if no match</returns>
        public bool GobbleandLog(string text, string additionalinfo = "")
        {
            bool val = Gobble(text);
            if (!val)
            {
                Logger.Log(new XenonCompilerMessage() { ErrorName = "Syntax: Unexpected Token", ErrorMessage = $"Expecting '{text}', got '{Peek()}'", Level = XenonCompilerMessageType.Error, Token = Peek(), Generator = "Lexer", Inner = additionalinfo });
            }
            return val;
        }


        public override string ToString()
        {
            return base.ToString() + $" At Token: {CurrentToken}";
        }

    }
}
