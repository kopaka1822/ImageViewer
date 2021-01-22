using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model.Equation.Token;

namespace ImageFramework.Model.Equation.Markov
{
    public class IntrinsicFunctionRule : MarkovRule
    {
        public IntrinsicFunctionRule()
        {
            Tokens = new List<Token.Token.Type>
            {
                Token.Token.Type.Function,
                Token.Token.Type.BracketClose
            };
        }

        protected override List<Token.Token> Apply(List<Token.Token> match)
        {
            var function = (FunctionToken)match[0];
            return new List<Token.Token> { new IntrinsicToken(function.FuncName) };
        }
    }
}
