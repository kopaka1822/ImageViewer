using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKImageViewer.Equation.Markov
{
    public static class MarkovProcess
    {
        public static List<Token.Token> Run(List<MarkovRule> rules, List<Token.Token> tokens)
        {
            bool foundRules = true;
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
                            List<Token.Token> firstPart = tokens.GetRange(0, i);
                            List<Token.Token> middle = tokens.GetRange(i, markovRule.Tokens.Count);
                            List<Token.Token> lastPart = tokens.GetRange(i + markovRule.Tokens.Count,
                                tokens.Count - i - markovRule.Tokens.Count);

                            middle = markovRule.Apply(middle);
                            tokens.Clear();
                            tokens.AddRange(firstPart);
                            tokens.AddRange(middle);
                            tokens.AddRange(lastPart);

                            // and again
                            goto markovloop;
                        }
                    }
                }

                foundRules = false;
            }
            return tokens;
        }

        private static bool TokensMatch(MarkovRule rule, List<Token.Token> tokens, int startIndex)
        {
            for(var i = 0; i < rule.Tokens.Count; ++i)
            {
                if (startIndex + i >= tokens.Count || rule.Tokens[i] != tokens[startIndex + i].TokenType)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
