using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model.Equation.Markov;
using ImageFramework.Model.Equation.Token;

namespace ImageFramework.Model.Equation
{
    // base class for equations
    public abstract class Equation
    {
        protected List<Token.Token> GetToken(string formula)
        {
            List<Token.Token> tokens = new List<Token.Token>();

            string current = "";
            foreach (char c in formula)
            {
                Token.Token nextToken = null;
                bool finishCurrent = true;
                switch (c)
                {
                    case '^':
                        nextToken = new SingleCharToken(Token.Token.Type.Operation1, '^');
                        break;
                    case '/':
                        nextToken = new SingleCharToken(Token.Token.Type.Operation2, '/');
                        break;
                    case '*':
                        nextToken = new SingleCharToken(Token.Token.Type.Operation2, '*');
                        break;
                    case '+':
                        nextToken = new SingleCharToken(Token.Token.Type.Operation3, '+');
                        break;
                    case '-':
                        if (current.Length >= 2)
                        {
                            // allow - for scientific notation (1e-10)
                            if (char.ToLower(current[current.Length - 1]) == 'e' &&
                                char.IsNumber(current[current.Length - 2]))
                            {
                                current += '-';
                                finishCurrent = false;
                                break;
                            }
                        }
                        nextToken = new SingleCharToken(Token.Token.Type.Operation3, '-');
                        break;
                    case ',':
                        nextToken = new SingleCharToken(Token.Token.Type.Seperator, ',');
                        break;
                    case '(':
                        if (current.Length > 0)
                        {
                            // this is a function
                            nextToken = new FunctionToken(current);
                            current = "";
                        }
                        else
                        {
                            nextToken = new SingleCharToken(Token.Token.Type.BracketOpen, '(');
                        }
                        break;
                    case ')':
                        nextToken = new SingleCharToken(Token.Token.Type.BracketClose, ')');
                        break;
                    default:
                        if (char.IsWhiteSpace(c)) break;

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
                if (nextToken != null)
                    tokens.Add(nextToken);
            }

            if (current.Length > 0)
                tokens.Add(MakeTokenFromString(current));

            return tokens;
        }

        /// function that converts an identifier to a token
        /// <param name="identifier">identifier string (starts with a letter and is not a number)</param>
        protected abstract Token.Token HandleVariableString(string identifier);

        private Token.Token MakeTokenFromString(string identifier)
        {
            Debug.Assert(identifier.Length > 0);
            if (char.IsLetter(identifier[0])) 
                return HandleVariableString(identifier);
            
            // handle digit
            double value;
            if (!Double.TryParse(identifier, NumberStyles.Float, Models.Culture, out value))
                throw new Exception($"Invalid Number: {identifier}");
            return new NumberToken((float)value);
        }

        protected ValueToken Compile(List<Token.Token> tokens)
        {
            // determine final token
            tokens = MarkovProcess.Run(GetRules(), tokens);

            if (tokens.Count != 1)
                throw new Exception("Could not resolve all tokens to an expression");

            if (tokens[0].TokenType != Token.Token.Type.Value)
                throw new Exception("Please enter a valid formula");

            return (ValueToken)tokens[0];
        }

        // set and order of markov rules
        protected abstract List<MarkovRule> GetRules();
    }
}
