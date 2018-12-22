using System.Collections.Generic;

namespace TextureViewer.Equation.Markov
{
    public abstract class MarkovRule
    {
        public List<Token.Token.Type> Tokens { get; protected set; }

        /// <summary>
        /// this will be called if a sequence of tokens matches the Tokens signature of this class
        /// </summary>
        /// <param name="match">Tokens with the same signature as Tokens</param>
        /// <returns></returns>
        public abstract List<Token.Token> Apply(List<Token.Token> match);
    }
}
