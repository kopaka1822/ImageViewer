using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageFramework.Model.Equation.Markov
{
    public static class MarkovProcess
    {
        public static List<Token.Token> Run(List<MarkovRule> rules, List<Token.Token> tokens)
        {
            VerifyBrackets(tokens);

            while (tokens.Count > 1)
            {
                // find a range of tokens to verify first (tokens inside brackets)
                int startId = 0;
                int endId = tokens.Count;
                for (var i = 0; i < tokens.Count; i++)
                {
                    if (tokens[i].TokenType == Token.Token.Type.BracketOpen || tokens[i].TokenType == Token.Token.Type.Function)
                    {
                        startId = endId = i;
                    }

                    if (tokens[i].TokenType == Token.Token.Type.BracketClose && startId == endId)
                        endId = i + 1;
                }

                // replace start id/ end id range
                var range = Resolve(rules, tokens.GetRange(startId, endId - startId));
                tokens = Replace(tokens, startId, endId - startId, range);
            } 

            return tokens;
        }

        public static List<Token.Token> Replace(List<Token.Token> list, int startIdx, int count, List<Token.Token> replacement)
        {
            var firstPart = list.GetRange(0, startIdx);
            var lastPart = list.GetRange(startIdx + count, list.Count - startIdx - count);

            firstPart.AddRange(replacement);
            firstPart.AddRange(lastPart);

            return firstPart;
        }

        public static List<Token.Token> Resolve(List<MarkovRule> rules, List<Token.Token> tokens)
        {
            var foundRules = true;
            markovloop:
            while (foundRules)
            {
                foreach (var markovRule in rules)
                {
                    // find matching sequence
                    for (int i = 0; i < tokens.Count; ++i)
                    {
                        if (TokensMatch(markovRule, tokens, i))
                        {
                            // apply the rule
                            var middle = tokens.GetRange(i, markovRule.Tokens.Count);
                            Token.Token left = null;
                            if (i > 0) left = tokens[i - 1];

                            middle = markovRule.ApplyEx(middle, left);
                            if(middle == null) continue; // no match

                            tokens = Replace(tokens, i, markovRule.Tokens.Count, middle);
                            
                            // and again
                            goto markovloop;
                        }
                    }
                }

                foundRules = false;
            }

            if(tokens.Count > 1)
                throw new Exception("Could not resolve all tokens to an expression");

            return tokens;
        }

        private static bool TokensMatch(MarkovRule rule, IReadOnlyList<Token.Token> tokens, int startIndex)
        {
            for (var i = 0; i < rule.Tokens.Count; ++i)
            {
                if (startIndex + i >= tokens.Count || rule.Tokens[i] != tokens[startIndex + i].TokenType)
                {
                    return false;
                }
            }
            return true;
        }

        private static void VerifyBrackets(IReadOnlyList<Token.Token> tokens)
        {
            int openBrackets = 0;
            foreach (var token in tokens)
            {
                if (token.TokenType == Token.Token.Type.BracketOpen || token.TokenType == Token.Token.Type.Function)
                    ++openBrackets;
                if (token.TokenType == Token.Token.Type.BracketClose)
                {
                    --openBrackets;
                    if (openBrackets < 0)
                        throw new Exception("too many closing brackets");
                }
            }

            if(openBrackets != 0)
                throw new Exception("not all brackets were closed");
        }
    }
}
