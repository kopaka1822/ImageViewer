using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKImageViewer.Equation.Markov
{
    class RuleValueOperationValue : MarkovRule
    { 
        public RuleValueOperationValue(Token.Type operationType)
        {
            Tokens = new List<Token.Type>
            {
                Token.Type.Value,
                operationType,
                Token.Type.Value
            };
        }

        public override List<Token> Apply(List<Token> match)
        {
            Debug.Assert(match.Count == 3);
            return new List<Token>{new CombinedValueToken(match[0], match[1], match[2])};
        }
    }
}
