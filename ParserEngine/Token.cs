using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserEngine
{
    public class Token : IDisposable
    {
        public enum TokenTypes
        {
            None,
            Number,
            Identifier,
            Operator,
            LeftParen,
            RightParen,
            Semicolon,
            Comma,
            LeftBrace,
            RightBrace,
            LeftBracket,
            RightBracket,
            String,
            Char,
            Other,
            Custom,
            LineComment,
            OpenComment,
            CloseComment,
            StringDelimiter,
            Invalid,
            EndOfStream
        }

        public TokenTypes TokenType { get; internal set; }

        public string Text { get; internal set; }

        public int CharIndex { get; }

        public Token(string text, TokenTypes tokenType, int charIndex)
        {
            Text = text;
            TokenType = tokenType;
            CharIndex = charIndex;
        }

        public Token(Token token, TokenTypes tokenType)
        {
            TokenType = tokenType;
            Text = token.Text;
            CharIndex = token.CharIndex;
        }

        public bool IsLeftParen
        {
            get { return Text == "("; }
        }

        public bool EqualsToken(Token tok)
        {
            return TokenType == tok.TokenType && Text == tok.Text;
        }

        public override string ToString()
        {
            return TokenType.ToString() + ":'" + Text.ToString() + "'";
        }

        public bool Disposed { get; protected set; }

        public virtual void Dispose()
        {
        }
    }

    public class StringToken : Token
    {
        public String Value { get; }

        public StringToken(string text, string value, int charIndex) :
               base(text, TokenTypes.String, charIndex)
        {
            Value = value;
        }
    }

    public class CharToken : Token
    {
        public char Value { get; }

        public CharToken(string text, char value, int charIndex) :
               base(text, TokenTypes.Char, charIndex)
        {
            Value = value;
        }
    }

    public class TypedToken : Token
    {
        public Type ReturnType { get; set; }
        public OperandModifiers OperandModifier { get; internal set; } = OperandModifiers.None;

        public TypedToken(Token token, TokenTypes tokenType, Type returnType) :
                          base(token, tokenType)
        {
            this.ReturnType = returnType;
        }
    }

}
