using System.Collections.Generic;
using System.Diagnostics;
using TextureViewer.Equation.Token;

namespace TextureViewer.Equation.Markov
{
    class RuleValueOperationValue : MarkovRule
    { 
        public RuleValueOperationValue(Token.Token.Type operationType)
        {
            Tokens = new List<Token.Token.Type>
            {
                Token.Token.Type.Value,
                operationType,
                Token.Token.Type.Value
            };
        }

        public override List<Token.Token> Apply(List<Token.Token> match)
        {
            Debug.Assert(match.Count == 3);
            return new List<Token.Token>{new CombinedValueToken(match[0], match[1], match[2])};
        }
    }
}
