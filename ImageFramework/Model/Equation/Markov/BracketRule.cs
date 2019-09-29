using System.Collections.Generic;

namespace ImageFramework.Model.Equation.Markov
{
    internal class BracketRule : MarkovRule
    {
        public BracketRule()
        {
            Tokens = new List<Token.Token.Type>
            {
                Token.Token.Type.BracketOpen,
                Token.Token.Type.Value,
                Token.Token.Type.BracketClose
            };
        }

        public override List<Token.Token> Apply(List<Token.Token> match)
        {
            // brackets will be implicitly given through the token structure
            return new List<Token.Token> { match[1] };
        }
    }
}
