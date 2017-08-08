using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKImageViewer.Equation.Markov
{
    class RuleSign : MarkovRule
    {
        public RuleSign()
        {
            Tokens = new List<Token.Type>
            {
                Token.Type.Operation3, // + or -
                Token.Type.Value
            };
        }

        public override List<Token> Apply(List<Token> match)
        {
            Debug.Assert(match.Count == 3);
            var signToken = match[0] as SingleCharToken;
            if (signToken != null)
            {
                if(signToken.Symbol == '+')
                    return new List<Token>{match[1]}; // the value wont change

                if (signToken.Symbol == '-')
                    // multiply with -1
                    return new List<Token>{new CombinedValueToken(new NumberToken(-1.0f),
                        new SingleCharToken(Token.Type.Operation2, '*'), match[1])};
            }
            throw new Exception("invalid token in Markov Rule RuleSign: " + match[0]);
        }
    }
}
