using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserEngine
{
    public class Trie
    {
        private class TrieElem
        {
            public Dictionary<char, TrieElem> Choices { get; set; }
            public Token.TokenTypes TokenType { get; set; } = Token.TokenTypes.None;
        }

        public Trie(bool ignoreCase = false)
        {
            IgnoreCase = ignoreCase;
        }

        public bool IgnoreCase { get; }
        private TrieElem rootElement { get; } = new TrieElem();

        public void AddToken(string token, Token.TokenTypes tokenType)
        {
            if (token.Length == 0)
                return;
            TrieElem currElem = rootElement;
            for (int i = 0; i < token.Length; i++)
            {
                char ch = token[i];
                if (IgnoreCase)
                    ch = Char.ToLower(ch);
                TrieElem nextElem;
                if (currElem.Choices == null)
                {
                    currElem.Choices = new Dictionary<char, TrieElem>();
                    nextElem = null;
                }
                else
                {
                    if (!currElem.Choices.TryGetValue(ch, out nextElem))
                        nextElem = null;
                }
                if (nextElem == null)
                {
                    nextElem = new TrieElem();
                    currElem.Choices.Add(ch, nextElem);
                }
                currElem = nextElem;
            }
            currElem.TokenType = tokenType;
        }

        public void AddTokens(IEnumerable<string> tokens, Token.TokenTypes tokenType)
        {
            foreach (string token in tokens)
            {
                AddToken(token, tokenType);
            }
        }

        /// <summary>
        /// Match chars in s against tokens in trie.  charIndex is left at 1 beyond last char in matched token.
        /// If there is no match, charIndex is unchanged.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="charIndex"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Token.TokenTypes ParseToken(string s, ref int charIndex, out string token)
        {
            TrieElem currElem = rootElement;
            int startIndex = charIndex;
            while (charIndex < s.Length)
            {
                char ch = s[charIndex];
                if (IgnoreCase)
                    ch = Char.ToLower(ch);
                if (currElem.Choices != null && currElem.Choices.TryGetValue(ch, out TrieElem nextElem))
                {
                    currElem = nextElem;
                    charIndex++;
                }
                else
                {
                    break;
                }
            }
            Token.TokenTypes tokenType;
            if (charIndex == startIndex)
            {
                tokenType = Token.TokenTypes.None;
            }
            else
            {
                tokenType = currElem.TokenType;
            }
            token = tokenType == Token.TokenTypes.None ? null : s.Substring(startIndex, charIndex - startIndex);
            return tokenType;
        }
    }
}
