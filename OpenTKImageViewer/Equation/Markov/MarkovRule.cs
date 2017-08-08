using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKImageViewer.Equation
{
    public abstract class MarkovRule
    {
        public List<Token.Type> Tokens { get; protected set; }

        /// <summary>
        /// this will be called if a sequence of tokens matches the Tokens signature of this class
        /// </summary>
        /// <param name="match">Tokens with the same signature as Tokens</param>
        /// <returns></returns>
        public abstract List<Token> Apply(List<Token> match);
    }
}
