using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model.Equation.Markov;
using ImageFramework.Model.Equation.Token;
using CannotUnloadAppDomainException = System.CannotUnloadAppDomainException;

namespace ImageFramework.Model.Equation
{
    public class FloatEquation : Equation
    {
        private string formula;
        private float width = 0.0f;
        private float height = 0.0f;
        private float depth = 0.0f;

        public FloatEquation(string formula)
        {
            this.formula = formula;
            // resolve to token
            var tokens = GetToken(formula);
            if (tokens.Count == 0)
                throw new Exception("Cannot evaluate expression");

            // check if the expression is valid
            Compile(tokens).ToFloat();
        }

        public float GetFloatExpression(int width, int height, int depth)
        {
            this.width = width;
            this.height = height;
            this.depth = depth;

            // reevaluate tokens (should be valid since it was checked in the constructor)
            return Compile(GetToken(formula)).ToFloat();
        }

        protected override Token.Token HandleVariableString(string identifier)
        {
            switch (identifier)
            {
                case "width": return new NumberToken(width);
                case "height": return new NumberToken(height);
                case "depth": return new NumberToken(depth);
            }

            throw new Exception("unknown identifier: " + identifier);
        }

        protected override List<MarkovRule> GetRules()
        {
            List<MarkovRule> rules = new List<MarkovRule>();

            //rules.Add(new IntrinsicFunctionRule()); // nothing defined
            rules.Add(new UnaryFunctionRule());
            rules.Add(new BinaryFunctionRule());
            rules.Add(new TertiaryFunctionRule());
            rules.Add(new BracketRule());
            rules.Add(new RuleValueOperationValue(Token.Token.Type.Operation1));
            rules.Add(new RuleDoubleSign());
            rules.Add(new RuleSign());
            rules.Add(new RuleValueOperationValue(Token.Token.Type.Operation2));
            rules.Add(new RuleValueOperationValue(Token.Token.Type.Operation3));

            return rules;
        }
    }
}
