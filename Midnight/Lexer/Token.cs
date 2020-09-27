using System;
using System.Collections.Generic;
using System.Text;

namespace Midnight.Lexer
{
    struct Token
    {
        public string Value { get;}
        public bool IsEscaped { get; }
        public TokenType Type { get; }

        public Token(string val, bool escaped, TokenType type)
        {
            Value = val;
            IsEscaped = escaped;
            Type = type;
        }

        public static implicit operator Token((string val, bool escaped, TokenType type) value)
        {
            return new Token(value.val, value.escaped, value.type);
        }

        public override string ToString()
        {
            return $"'{Value}' Escaped: {IsEscaped} Type: {Type}";
        }

    }
}
