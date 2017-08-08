using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKImageViewer.Equation.Markov
{
    class BracketRule : MarkovRule
    {
        public BracketRule()
        {
            Tokens = new List<Token.Type>
            {
                Token.Type.BracketOpen,
                Token.Type.Value,
                Token.Type.BracketClose
            };
        }

        public override List<Token> Apply(List<Token> match)
        {
            // brackets will be implicitly given through the token structure
            return new List<Token> {match[1]};
        }
    }
}
