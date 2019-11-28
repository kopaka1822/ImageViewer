using System.Collections.Generic;

namespace ImageFramework.Model.Equation.Markov
{
    public abstract class MarkovRule
    {
        public List<Token.Token.Type> Tokens { get; protected set; }

        /// <summary>
        /// this will be called if a sequence of tokens matches the Tokens signature of this class
        /// </summary>
        /// <param name="match">Tokens with the same signature as Tokens</param>
        /// <returns></returns>
        protected virtual List<Token.Token> Apply(List<Token.Token> match)
        {
            return null;
        }

        public virtual List<Token.Token> ApplyEx(List<Token.Token> match, Token.Token left)
        {
            return Apply(match);
        }
    }
}
