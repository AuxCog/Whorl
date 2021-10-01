using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserEngine
{
    public class Tokenizer
    {
        private enum CharTypes
        {
            None,
            Punctuation,
            Alpha,
            Numeric,
            WhiteSpace,
            Unknown
        }

        private const char CommentChar = '#';
        //public string Expression { get; private set; }
        public Trie TokenTrie { get; }
        public bool CheckAlphaOperators { get; set; }
        public string CloseCommentText { get; private set; }
        public HashSet<string> AlphaOperatorsHashSet { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public Tokenizer(Trie tokenTrie = null, bool forCSharp = true, bool addOperators = false)
        {
            TokenTrie = tokenTrie ?? new Trie();
            if (forCSharp)
            {
                InitializeForCSharp(addOperators);
            }
            else
            {
                TokenTrie.AddToken(CommentChar.ToString(), Token.TokenTypes.LineComment);
                TokenTrie.AddToken("\"", Token.TokenTypes.StringDelimiter);
                CheckAlphaOperators = true;
            }
        }

        private void InitializeForCSharp(bool addOperators)
        {
            TokenTrie.AddToken("//", Token.TokenTypes.LineComment);
            TokenTrie.AddToken("/*", Token.TokenTypes.OpenComment);
            CloseCommentText = "*/";
            string[] delims = { "\"", "'", "@\"", "$\"", "$@\"" };
            TokenTrie.AddTokens(delims, Token.TokenTypes.StringDelimiter);
            if (addOperators)
            {
                string[] operators = { "+", "-", "/", "*", "=", "==", "!=", "<=", ">=", ".", 
                                       "++", "--", "+=", "-=", "*=", "/=",
                                       "&&", "||", "&", "|", "&=", "|="};
                TokenTrie.AddTokens(operators, Token.TokenTypes.Operator);
            }
        }

        public List<Token> TokenizeExpression(string expression)
        {
            List<Token> tokens = new List<Token>();
            if (string.IsNullOrEmpty(expression))
                return tokens;
            int index = 0;
            expression += " ";
            int expLen = expression.Length;
            while (index < expLen)
            {
                Token token = ParseToken(expression, ref index);
                if (token == null)
                    break;
                else
                    tokens.Add(token);
            }
            return tokens;
        }

        public Token ParseToken(string expression, ref int index)
        {
            Token token = null;
            Token.TokenTypes tokenType;
            string tokenText;
            int expLen = expression.Length;
            while (index < expLen)
            {
                char ch;
                do
                {
                    ch = expression[index++];
                } while (Char.IsWhiteSpace(ch) && index < expLen);
                if (index == expLen)
                    break;
                int startIndex = index - 1;
                CharTypes charType = GetCharType(ch);
                switch (charType)
                {
                    case CharTypes.Punctuation:
                        index--;
                        tokenType = TokenTrie.ParseToken(expression, ref index, out tokenText);
                        if (tokenType != Token.TokenTypes.None)
                        {
                            if (tokenType == Token.TokenTypes.LineComment ||
                                (tokenType == Token.TokenTypes.OpenComment && CloseCommentText != null))
                            {
                                string pattern = tokenType == Token.TokenTypes.LineComment ? "\n" : CloseCommentText;
                                index = expression.IndexOf(pattern, index);
                                if (index == -1)
                                    index = expLen;
                                else
                                    index += pattern.Length;
                            }
                            else if (tokenType == Token.TokenTypes.StringDelimiter)
                            {
                                token = ParseStringToken(expression, ref index, delimiter: tokenText);
                            }
                            else
                            {
                                token = CreateToken(tokenType, tokenText, index);
                            }
                        }
                        else
                        {
                            index++;
                            token = CreateToken(Token.TokenTypes.Other, ch.ToString(), index);
                        }
                        break;
                    case CharTypes.Alpha:
                        while (index < expLen)
                        {
                            charType = GetCharType(expression[index]);
                            if (charType == CharTypes.Alpha || charType == CharTypes.Numeric)
                                index++;
                            else
                                break;
                        }
                        tokenText = expression.Substring(startIndex, index - startIndex);
                        token = CreateToken(Token.TokenTypes.Identifier, tokenText, index);
                        break;
                    case CharTypes.Numeric:
                        tokenType = Token.TokenTypes.Number;
                        bool foundDecPoint = false;
                        while (index < expLen)
                        {
                            ch = expression[index];
                            if (ch == '.' && !foundDecPoint)
                            {
                                index++;
                                foundDecPoint = true;
                            }
                            else
                            {
                                charType = GetCharType(ch);
                                if (charType == CharTypes.Numeric)
                                    index++;
                                else
                                {
                                    if (charType == CharTypes.Alpha)
                                        tokenType = Token.TokenTypes.Invalid;
                                    break;
                                }
                            }
                        }
                        tokenText = expression.Substring(startIndex, index - startIndex);
                        token = CreateToken(tokenType, tokenText, index);
                        break;
                    default:
                        token = CreateToken(Token.TokenTypes.Invalid, ch.ToString(), index);
                        break;
                }
                if (token != null)
                    break;
            }
            return token;
        }

        private Token ParseStringToken(string expression, ref int index, string delimiter)
        {
            bool hasAtDelim = delimiter.Contains('@');
            bool allowEmbedded = delimiter.Length != 0 && delimiter[0] == '$';
            bool isChar = delimiter == "'";
            int expLen = expression.Length;
            var sbStringValue = new StringBuilder();
            int startIndex = index - 1;
            int nesting = -1;
            bool isValid = true;
            while (true)
            {
                if (index >= expLen)
                {
                    isValid = false;
                    break;
                }
                char ch = expression[index++];
                if (isChar)
                {
                    if (ch == '\'')
                    {
                        isValid = sbStringValue.Length == 1;
                        break;
                    }
                    else if (sbStringValue.Length > 0)
                    {
                        isValid = false;
                        break;
                    }
                }
                else
                {
                    if (ch == '"')
                        break;
                }
                if (ch == '\\' && index < expLen)
                {
                    char ch2 = expression[index];
                    if (ch2 == '"' || !hasAtDelim)
                    {
                        if (char.IsControl(ch2))
                            isValid = false;
                        index++;
                        ch = ch2;
                    }
                }
                else if (ch == '\n' && !hasAtDelim)
                {
                    isValid = false;
                }
                else if (allowEmbedded && ch == '{')
                {
                    if (index < expLen && expression[index] == '{')
                    {
                        index++;
                    }
                    else
                    {
                        nesting = 1;
                        sbStringValue = null;
                        while (true)
                        {
                            Token subToken = ParseToken(expression, ref index);
                            if (subToken == null || subToken.TokenType == Token.TokenTypes.Invalid)
                            {
                                isValid = false;
                                break;
                            }
                            if (subToken.Text == "{")
                                nesting++;
                            else if (subToken.Text == "}")
                            {
                                if (--nesting == 0)
                                    break;
                            }
                        }
                    }
                }
                if (sbStringValue != null && isValid)
                    sbStringValue.Append(ch);
            }
            string tokenText = expression.Substring(startIndex, index - startIndex);
            Token token;
            if (isValid)
            {
                if (isChar)
                    token = new CharToken(tokenText, sbStringValue.ToString()[0], startIndex);
                else
                    token = new StringToken(tokenText, sbStringValue?.ToString(), startIndex);
            }
            else
            {
                token = new Token(tokenText, Token.TokenTypes.Invalid, startIndex);
            }
            return token;
        }

        private Token CreateToken(Token.TokenTypes tokenType, string text, int charIndex)
        {
            if (CheckAlphaOperators && tokenType == Token.TokenTypes.Identifier)
            {
                if (AlphaOperatorsHashSet.Contains(text))
                    tokenType = Token.TokenTypes.Operator;
            }
            return new Token(text, tokenType, charIndex - text.Length);
        }

        private static CharTypes GetCharType(char ch)
        {
            CharTypes charType;
            if (char.IsLetter(ch) || ch == '_')
                charType = CharTypes.Alpha;
            else if (char.IsNumber(ch))
                charType = CharTypes.Numeric;
            else if (char.IsPunctuation(ch) || char.IsSymbol(ch))
                charType = CharTypes.Punctuation;
            else if (char.IsWhiteSpace(ch))
                charType = CharTypes.WhiteSpace;
            else
                charType = CharTypes.Unknown;
            return charType;
        }

    }
}
