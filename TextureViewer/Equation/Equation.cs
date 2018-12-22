using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using TextureViewer.Equation.Markov;
using TextureViewer.Equation.Token;

namespace TextureViewer.Equation
{
    public class Equation
    {
        private readonly int maxTextureUnits;
        private ValueToken finalToken;
        private int firstImageId;

        public Equation(string formula, int maxTextureUnits)
        {
            this.maxTextureUnits = maxTextureUnits;
            // resolve to token
            var tokens = GetToken(formula);
            if(tokens.Count == 0)
                throw new Exception("Please enter a formula");
            // check for syntax
            Compile(tokens);
        }

        public string GetOpenGlExpression()
        {
            return finalToken.ToOpenGl();
        }

        /// <summary>
        /// returns the id of the first image token that was used in the formula
        /// </summary>
        /// <returns>id of the first image token that was used in the formula</returns>
        public int GetFirstImageId()
        {
            return firstImageId;
        }

        private List<Token.Token> GetToken(string formula)
        {
            List<Token.Token> tokens = new List<Token.Token>();

            string current = "";
            foreach (char c in formula)
            {
                Token.Token nextToken = null;
                bool finishCurrent = true;
                switch (c)
                {
                    case ' ':
                        break;
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

        private Token.Token MakeTokenFromString(string identifier)
        {
            Debug.Assert(identifier.Length > 0);
            if (identifier.StartsWith("I"))
            {
                // image identifier
                int number;
                if(!Int32.TryParse(identifier.Substring(1), NumberStyles.Integer, App.GetCulture(), out number))
                    throw new Exception($"Invalid Image Identifier: {identifier}");
                if(number < 0 || number >= maxTextureUnits)
                    throw new Exception("Invalid Image Range: " + identifier);
                return new ImageToken(number);
            }

            double value;
            if(!Double.TryParse(identifier, NumberStyles.Float, App.GetCulture(), out value))
                throw new Exception($"Invalid Number: {identifier}");
            return new NumberToken((float)value);
        }

        private void Compile(List<Token.Token> tokens)
        {
            // determine first image id
            foreach(var token in tokens)
            {
                if (token is ImageToken itoken)
                {
                    firstImageId = itoken.Id;
                    break;
                }
            }

            // determine final token
            tokens = MarkovProcess.Run(GetRules(), tokens);

            if(tokens.Count != 1)
                throw new Exception("Could not resolve all tokens to an expression");

            if(tokens[0].TokenType != Token.Token.Type.Value)
                throw new Exception("Please enter a valid formula");

            finalToken = (ValueToken)tokens[0];
        }

        private List<MarkovRule> GetRules()
        {
            List<MarkovRule> rules = new List<MarkovRule>();

            rules.Add(new RuleValueOperationValue(Token.Token.Type.Operation1));
            rules.Add(new RuleValueOperationValue(Token.Token.Type.Operation2));
            rules.Add(new RuleValueOperationValue(Token.Token.Type.Operation3));
            rules.Add(new BracketRule());
            rules.Add(new UnaryFunctionRule());
            rules.Add(new BinaryFunctionRule());
            rules.Add(new TertiaryFunctionRule());
            rules.Add(new RuleSign());

            return rules;
        }
    }
}
