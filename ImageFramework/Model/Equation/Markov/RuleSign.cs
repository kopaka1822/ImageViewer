using System;
using System.Collections.Generic;
using System.Diagnostics;
using ImageFramework.Model.Equation.Token;

namespace ImageFramework.Model.Equation.Markov
{
    internal class RuleSign : MarkovRule
    {
        public RuleSign()
        {
            Tokens = new List<Token.Token.Type>
            {
                Token.Token.Type.Operation3, // + or -
                Token.Token.Type.Value
            };
        }

        public override List<Token.Token> Apply(List<Token.Token> match)
        {
            Debug.Assert(match.Count == 2);
            if (match[0] is SingleCharToken signToken)
            {
                if (signToken.Symbol == '+')
                    return new List<Token.Token> { match[1] }; // the value wont change

                if (signToken.Symbol == '-')
                {
                    return new List<Token.Token>
                    {
                        new CombinedValueToken(new NumberToken(-1.0f),
                            new SingleCharToken(Token.Token.Type.Operation2, '*'), match[1])
                    };
                }
            }
            throw new Exception("invalid token in Markov Rule RuleSign: " + match[0]);
        }
    }
}
