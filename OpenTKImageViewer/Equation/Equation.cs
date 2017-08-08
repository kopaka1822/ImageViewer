using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKImageViewer.Equation
{
    public class Equation
    {
        private readonly int maxTextureUnits;

        public Equation(string formula, int maxTextureUnits)
        {
            this.maxTextureUnits = maxTextureUnits;
            // resolve to token
            var tokens = GetToken(formula);
            // check for syntax
        }

        private List<Token> GetToken(string formula)
        {
            List<Token> tokens = new List<Token>();

            string current = "";
            foreach (char c in formula)
            {
                Token nextToken = null;
                bool finishCurrent = true;
                switch (c)
                {
                    case ' ':
                        break;
                    case '^':
                        nextToken = new SingleCharToken(Token.Type.Operation1, '^'); 
                        break;
                    case '/':
                        nextToken = new SingleCharToken(Token.Type.Operation2, '/');
                        break;
                    case '*':
                        nextToken = new SingleCharToken(Token.Type.Operation2, '*');
                        break;
                    case '+':
                        nextToken = new SingleCharToken(Token.Type.Operation3, '+');
                        break;
                    case '-':
                        nextToken = new SingleCharToken(Token.Type.Operation3, '-');
                        break;
                    case '(':
                        nextToken = new SingleCharToken(Token.Type.BracketOpen, '-');
                        break;
                    case ')':
                        nextToken = new SingleCharToken(Token.Type.BracketClose, '-');
                        break;
                    default:
                        current += c;
                        finishCurrent = false;
                        break;
                }

                if (finishCurrent && current.Length > 0)
                {
                    // add value token
                    tokens.Add(MakeTokenFromString(current));
                    current = "";
                }
                if(nextToken != null)
                    tokens.Add(nextToken);
            }

            if(current.Length > 0)
                tokens.Add(MakeTokenFromString(current));

            return tokens;
        }

        private Token MakeTokenFromString(string identifier)
        {
            Debug.Assert(identifier.Length > 0);
            if (identifier.StartsWith("I"))
            {
                // image identifier
                int number;
                if(!Int32.TryParse(identifier.Substring(1), out number))
                    throw new Exception($"Invalid Image Identifier: {identifier}");
                if(number < 0 || number >= maxTextureUnits)
                    throw new Exception("Invalid Image Range: " + identifier);
                return new ImageToken(number);
            }

            double value;
            if(!Double.TryParse(identifier, out value))
                throw new Exception($"Invalid Number: {identifier}");
            return new NumberToken((float)value);
        }
    }
}
