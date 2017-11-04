using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTKImageViewer.Equation.Token;

namespace OpenTKImageViewer.Equation.Markov
{
    class RuleSign : MarkovRule
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
            var signToken = match[0] as SingleCharToken;
            if (signToken != null)
            {
                if(signToken.Symbol == '+')
                    return new List<Token.Token>{match[1]}; // the value wont change

                if (signToken.Symbol == '-')
                    // multiply with -1
                    return new List<Token.Token>{new CombinedValueToken(new NumberToken(-1.0f),
                        new SingleCharToken(Token.Token.Type.Operation2, '*'), match[1])};
            }
            throw new Exception("invalid token in Markov Rule RuleSign: " + match[0]);
        }
    }
}
