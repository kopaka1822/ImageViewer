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
    public class HlslEquation : Equation
    {
        private ValueToken finalToken;
        private int firstImageId = -1;
        private int maxImageId = -1;
        private int minImageId = Int32.MaxValue;
        private HashSet<int> imageIds = new HashSet<int>();
        private static readonly float EpsilonValue = float.Epsilon;
        private static readonly Dictionary<string, float> Constants = new Dictionary<string, float>
        {
            {"pi", (float)Math.PI },
            {"e", (float)Math.E },
            {"inf", float.PositiveInfinity },
            {"infinity", float.PositiveInfinity },
            {"float_max", float.MaxValue },
            {"fmax", float.MaxValue },
            {"eps", EpsilonValue },
            {"epsilon", EpsilonValue },
            {"nan", float.NaN },
        };

        public HlslEquation(string formula)
        {
            // resolve to token
            var tokens = GetToken(formula);
            if (tokens.Count == 0)
                throw new Exception("Please enter a formula");

            // determine first image id
            foreach (var token in tokens)
            {
                if (token is ImageToken itoken)
                {
                    if(firstImageId == -1) firstImageId = itoken.Id;
                    imageIds.Add(itoken.Id);
                }
            }

            // check for syntax
            finalToken = Compile(tokens);
        }

        public HlslEquation()
        {
            // does nothing
        }

        public string ReplaceImageInFormula(string formular, int oldImage, int newImage)
        {
            var tokens = GetToken(formular);
            string res = "";
            foreach (var token in tokens)
            {
                if (token is ImageToken itoken)
                {
                    if (itoken.Id == oldImage)
                        itoken.Id = newImage;
                }

                res += token.ToString();
            }

            return res;
        }

        public string GetHlslExpression()
        {
            return finalToken.ToHlsl();
        }

        /// <summary>
        /// id of the first image that occured in the formula
        /// </summary>
        public int FirstImageId => Math.Max(firstImageId, 0);

        /// <summary>
        /// highest image id that occured in the formula
        /// </summary>
        public int MaxImageId => Math.Max(maxImageId, 0);

        /// <summary>
        /// lowest image id that occured in the formula.
        /// </summary>
        public int MinImageId => minImageId == Int32.MaxValue ? 0 : minImageId;

        public bool HasImageId => firstImageId != -1;

        // hash set with all images that are used in the formula
        public HashSet<int> ImageIds => imageIds;

        protected override List<MarkovRule> GetRules()
        {
            List<MarkovRule> rules = new List<MarkovRule>();

            rules.Add(new IntrinsicFunctionRule());
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

        protected override Token.Token HandleVariableString(string identifier)
        {
            // handle variable
            var lower = identifier.ToLower(Models.Culture);
            if (lower.Length > 1 && lower[0] == 'i' && char.IsNumber(lower[1]))
            {
                // image identifier
                int number;
                if (!Int32.TryParse(identifier.Substring(1), NumberStyles.Integer, Models.Culture, out number))
                    throw new Exception($"Invalid Image Identifier: {identifier}");
                if (number < 0)
                    throw new Exception($"Invalid Image Range: {identifier}");
                // update highest image id
                maxImageId = Math.Max(maxImageId, number);
                minImageId = Math.Min(minImageId, number);
                if (firstImageId < 0) // first image that occured in the formula
                    firstImageId = maxImageId;

                return new ImageToken(number);
            }

            // constant?
            if (!Constants.TryGetValue(lower, out var value))
                throw new Exception($"Unknown Identifier: {identifier}");

            return new ConstantToken(lower, value);
        }
    }
}
