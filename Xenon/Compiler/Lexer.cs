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

        public static readonly Token EOFText = ("$EOF$", int.MaxValue);

        public string SingleLineComment { get; } = "//";

        public string EscapedSingleLineComment { get; } = @"\//";

        public string BeginBlockComment { get; } = "/*";
        public string EndBlockComment { get; } = "*/";

        public string Text { get; private set; } = string.Empty;

        public List<string> SplitWords { get; private set; }


        private List<(string tvalue, int sourcelinenum)> _tokens;

        private int _tokenpos = 0;

        public Token CurrentToken
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
            SplitWords.AddRange(LanguageKeywords.Commands.Values.OrderByDescending(p => p));
            SplitWords.AddRange(LanguageKeywords.WholeWords);
            List<string> Seperators = new List<string>() {
                "```",
                "\r\n",
                //"/*",
                //"*/",
                "//",
                "::",
                ">>",
                "?",
                ":", // hopefully this doesn't break anything
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
                "<",
                ">",
                "#",
                "\\",
                "$",
                "\"",
                "=",
            };
            SplitWords.AddRange(Seperators);
            SplitWords = SplitWords.OrderByDescending(s => s.Length).ToList();
        }

        private List<(string tvalue, int lnum)> SplitAndKeep(string text, List<string> splitwords, int startline = 0)
        {
            //_tokens = new List<string>();

            List<(string tval, int lnum)> tokens = new List<(string, int)>();

            int lnum = startline;

            for (int textindex = 0; textindex < text.Length;)
            {
                textindex = InnerCheck(ref text, splitwords, textindex, tokens, ref lnum);
            }


            // add the rest
            //_tokens.Add(text);
            tokens.Add((text, lnum));

            // remove empty tokens
            //_tokens.RemoveAll(p => p == string.Empty);
            tokens.RemoveAll(p => p.tval == string.Empty);


            // debug
            //_tokens.ForEach((string t) => { Debug.WriteLine(t); });
            return tokens;

        }

        private int InnerCheck(ref string text, List<string> splitwords, int textindex, List<(string tval, int lnum)> tokens, ref int clnum)
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

                        tokens.Add((before, clnum));
                        tokens.Add((word, clnum));

                        if (s == System.Environment.NewLine)
                        {
                            clnum++;
                        }

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
        public List<(string blocktext, int blockstart)> StripComments(string input)
        {
            List<string> seperators = new List<string>()
            {
                System.Environment.NewLine,
                //"\r\n",
                //"\n",
                //"\r",
                BeginBlockComment,
                EndBlockComment,
                EscapedSingleLineComment,
                SingleLineComment,
            };

            var parts = SplitAndKeep(input, seperators);

            List<(string, int)> noblocks = new List<(string, int)>();
            List<(string, int)> nocomments = new List<(string, int)>();
            // remove all block comments

            int lnum = 0;
            bool inblock = false;
            foreach (var p in parts)
            {
                if (p.tvalue == System.Environment.NewLine)
                {
                    lnum++;
                }
                if (p.tvalue == BeginBlockComment)
                {
                    inblock = true;
                }

                if (!inblock)
                {
                    noblocks.Add(p);
                }

                if (p.tvalue == EndBlockComment)
                {
                    inblock = false;
                }
            }

            // remove single line comments
            bool iscomment = false;
            foreach (var b in noblocks)
            {
                if (b.Item1 == SingleLineComment)
                {
                    iscomment = true;
                }

                if (!iscomment)
                {
                    if (b.Item1 == EscapedSingleLineComment)
                    {
                        nocomments.Add((SingleLineComment, b.Item2));
                    }
                    else
                    {
                        nocomments.Add(b);
                    }
                }

                if (b.Item1 == System.Environment.NewLine)
                {
                    iscomment = false;
                }
            }

            /*
            StringBuilder sb = new StringBuilder();
            foreach (var w in nocomments)
            {
                sb.Append(w);
            }
            return sb.ToString();
            */

            return nocomments;
        }

        public void Tokenize(List<(string inputblock, int startline)> input)
        {
            //Text = input;
            // split based on split words
            if (_tokens == null)
            {
                _tokens = new List<(string tvalue, int sourcelinenum)>();
            }
            _tokens?.Clear();
            foreach (var ib in input)
            {
                _tokens.AddRange(SplitAndKeep(ib.inputblock, SplitWords, ib.startline));
            }

            //_tokens = SplitAndKeep(Text, SplitWords);
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
            var m = Regex.Match(_tokens[_tokenpos].tvalue, pattern);

            inspections++;
            return m.Success;
        }


        /// <summary>
        /// View the current token without consuming it
        /// </summary>
        /// <returns>The current token</returns>
        public Token Peek()
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
                return _tokens[_tokenpos + 1].tvalue;
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
            if (_tokens[_tokenpos].tvalue == text)
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
        public Token Consume()
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
        public Token ConsumeUntil(string test, string errormessage = "", bool escapewithbackslash = false)
        {
            bool escapenext = false;
            StringBuilder sb = new StringBuilder();

            List<Token> capture = new List<Token>();

            try
            {
                while (!(Inspect(test) && (!escapenext || !escapewithbackslash)))
                {
                    if (escapewithbackslash)
                    {
                        if (Inspect(@"\"))
                        {
                            escapenext = true;
                            Consume();
                            continue;
                        }
                    }
                    capture.Add(Consume());
                    escapenext = false;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(new XenonCompilerMessage() { ErrorName = "Expecting missing token", ErrorMessage = $"Expecting token <'{test}'>.\r\nAdditional Info: {errormessage}", Generator = "Lexer.ConsumeUntil", Inner = ex.Message, Level = XenonCompilerMessageType.Error, Token = CurrentToken });
                throw;
            }

            //return sb.ToString();
            foreach (var t in capture)
            {
                sb.Append(t.tvalue);
            }
            return (sb.ToString(), capture.Any() ? capture.First().linenum : CurrentToken.linenum);
        }


        public string ConsumeUntil(IEnumerable<string> tests, string errormessage = "")
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                while (!tests.Contains(Peek().tvalue))
                {
                    sb.Append(Consume());
                }
            }
            catch (Exception ex)
            {
                Logger.Log(new XenonCompilerMessage()
                {
                    ErrorName = "Expecting missing token",
                    ErrorMessage = $"Expecting tokens <'{tests.Aggregate((acc, v) => acc + "," + v)}'>.\r\nAdditional Info: {errormessage}",
                    Generator = "Lexer.ConsumeUntil",
                    Inner = ex.Message,
                    Level = XenonCompilerMessageType.Error,
                    Token = CurrentToken
                });
                throw;
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
                if (Regex.Match(_tokens[_tokenpos].tvalue, "\\s").Success)
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
                    res[p] = ConsumeUntil(paramendseq, escapewithbackslash: areenclosed);
                }
                catch (Exception ex)
                {
                    Logger.Log(new XenonCompilerMessage() { ErrorName = "Bad parameter", ErrorMessage = $"Mal-formed parameter {helpmsg}.", Generator = "Lexer.ConsumeArgList", Inner = ex.Message, Level = XenonCompilerMessageType.Error, Token = CurrentToken });
                    throw;
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


        public Dictionary<string, Token> ConsumeOptionalNamedArgsUnenclosed(params string[] paramnames)
        {
            Dictionary<string, Token> res = new Dictionary<string, Token>();
            GobbleandLog(ConsumeArgList_StartSeq, $"Expected opening {ConsumeArgList_StartSeq} to begin parameter list.");

            int found = 0;
            // inspect for each optional parameter until ending sequence
            while (!Inspect(ConsumeArgList_EndSeq))
            {
                GobbleWhitespace();
                if (found == paramnames.Length)
                {
                    // too many parameters, we should be done
                    Logger.Log(new XenonCompilerMessage() { ErrorName = "Bad Arguments", ErrorMessage = $"Found too many parameters for command. Expected only {paramnames}", Generator = "Lexer.ConsumeOptionalNamedArgsUnenclosed", Inner = "", Token = CurrentToken, Level = XenonCompilerMessageType.Error });
                    return res;
                }
                // try a parameter
                var name = Peek();
                if (paramnames.Contains(name.tvalue))
                {
                    // eat it
                    found += 1;
                    Consume();
                    GobbleWhitespace();
                    GobbleandLog("=", $"Expecting '=' before value of parameter {name}");
                    var endtokens = new[] { ConsumeArgList_EndSeq, ConsumeArgList_SepSeq };
                    string val = ConsumeUntil(endtokens, $"Expecting {endtokens} after value for parameter {name}.");
                    GobbleWhitespace();
                    Gobble(ConsumeArgList_SepSeq);
                    res.Add(name.tvalue, val);
                    GobbleWhitespace();
                }
            }
            Consume();
            return res;
        }


        public (Dictionary<string, Token> args, List<string> flags) ConsumeOptionalNamedArgsUnenclosed_WithFlags(params string[] paramnames)
        {
            Dictionary<string, Token> res = new Dictionary<string, Token>();
            List<string> flags = new List<string>();
            GobbleandLog(ConsumeArgList_StartSeq, $"Expected opening {ConsumeArgList_StartSeq} to begin parameter list.");

            // inspect for each optional parameter until ending sequence
            while (!Inspect(ConsumeArgList_EndSeq))
            {
                GobbleWhitespace();

                // try a parameter
                var name = Peek();
                if (paramnames.Contains(name.tvalue))
                {
                    // eat it
                    Consume();
                    GobbleWhitespace();
                    GobbleandLog("=", $"Expecting '=' before value of parameter {name}");
                    var endtokens = new[] { ConsumeArgList_EndSeq, ConsumeArgList_SepSeq };
                    string val = ConsumeUntil(endtokens, $"Expecting {endtokens} after value for parameter {name}.");
                    GobbleWhitespace();
                    Gobble(ConsumeArgList_SepSeq);
                    res.Add(name.tvalue, val);
                    GobbleWhitespace();
                }
                else // could be a flag
                {
                    Consume();
                    GobbleWhitespace();
                    Gobble(ConsumeArgList_SepSeq);
                    flags.Add(name.tvalue);
                }
            }
            Consume();
            return (res, flags);
        }

        /// <summary>
        /// Consumes tokens, while keeping track of nesting. Reassembles all tokens to a raw string.
        /// </summary>
        /// <param name="nest_inc">Token that identifies new nested block</param>
        /// <param name="nest_dec">Token that identifies end of nested block</param>
        /// <param name="escape_inc">Token that identifes next token should be escaped for the purpose of nesting</param>
        /// <param name="escape_dec">Token that identifes next token should be scaped for the purpose of nesting</param>
        /// <returns></returns>
        public string ConsumeNestedBlockAsRaw(string nest_inc = "{", string nest_dec = "}", string escape_inc = @"\", string escape_dec = @"\")
        {
            StringBuilder sb = new StringBuilder();

            int depth = 0;

            bool escapeinc = false;
            bool escapedec = false;

            do
            {
                var next = Consume();
                if (next == escape_inc && PeekNext() == nest_inc)
                {
                    escapeinc = true;
                }

                if (escapeinc && next == nest_inc)
                {
                    escapeinc = false;
                    continue;
                }
                else if (next == nest_inc)
                {
                    depth++;
                }

                if (next == escape_dec && PeekNext() == nest_dec)
                {
                    escapedec = true;
                }

                if (escapedec && next == nest_dec)
                {
                    escapeinc = false;
                    continue;
                }
                else if (next == nest_dec)
                {
                    depth--;
                }

                sb.Append(next);
            }
            while (!InspectEOF() && depth > 0);


            return sb.ToString();
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

    public struct Token
    {
        public string tvalue;
        public int linenum;

        public Token(string tvalue, int linenum)
        {
            this.tvalue = tvalue;
            this.linenum = linenum;
        }

        public override bool Equals(object obj)
        {
            return obj is Token other &&
                   tvalue == other.tvalue &&
                   linenum == other.linenum;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(tvalue, linenum);
        }

        public void Deconstruct(out string tvalue, out int linenum)
        {
            tvalue = this.tvalue;
            linenum = this.linenum;
        }

        public static implicit operator (string tvalue, int linenum)(Token value)
        {
            return (value.tvalue, value.linenum);
        }

        public static implicit operator Token((string tvalue, int linenum) value)
        {
            return new Token(value.tvalue, value.linenum);
        }

        public override string ToString()
        {
            return $"Value: {{{tvalue}}}, Source Line: {linenum + 1}";
        }

        public static implicit operator string(Token input)
        {
            return input.tvalue;
        }

        public static implicit operator Token(string value)
        {
            return (value, int.MaxValue);
        }
    }
}
