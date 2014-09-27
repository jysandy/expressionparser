using System;
using System.Collections.Generic;

namespace Sandy.Expression
{
    enum TokenType
    {
        Literal,
        Operator,
        LeftParenthesis,
        RightParenthesis,
        Function,
        Comma
    }

    class Token
    {
        public string Data { get; set; }
        public TokenType Type { get; set; }

        public Token(string data, TokenType type)
        {
            this.Data = data;
            this.Type = type;
        }
    }
}
