using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model.Equation.Token;

namespace ImageFramework.Model.Equation.Markov
{
    class RuleDoubleSign : MarkovRule
    {
        public RuleDoubleSign()
        {
            Tokens = new List<Token.Token.Type>
            {
                Token.Token.Type.Operation3, // + or -
                Token.Token.Type.Operation3
            };
        }

        protected override List<Token.Token> Apply(List<Token.Token> match)
        {
            Debug.Assert(match.Count == 2);
            var left = (SingleCharToken) match[0];
            var right = (SingleCharToken) match[1];
            if(left.Symbol != right.Symbol) // different signs => negative
                return new List<Token.Token>{new SingleCharToken(Token.Token.Type.Operation3, '-')};

            // same signs => positive
            return new List<Token.Token>{new SingleCharToken(Token.Token.Type.Operation3, '+')};
        }
    }
}
