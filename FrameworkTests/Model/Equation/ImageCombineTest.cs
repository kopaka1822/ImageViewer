using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model.Equation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FrameworkTests.Model.Equation
{
    [TestClass]
    public class ImageCombineTest
    {
        [TestMethod]
        public void DefaultImage()
        {
            var shader = new ImageCombineShader(GetEq("I0"), GetEq("I0"), 1);
            
        }

        [TestMethod]
        public void CombinedImage()
        {
            var shader = new ImageCombineShader(GetEq("I0 + I1"), GetEq("1"), 2);
        }

        [TestMethod]
        public void Numbers()
        {
            TestFormula("1");
            TestFormula("1 + 2");
            TestFormula("1 * 2");
            TestFormula("1 / 2");

            TestFormula("2 ^ 2");
        }

        [TestMethod]
        public void UnaryFunctionsDefault()
        {
            TestFormula("abs(I0)");
            TestFormula("sin(I0)");
            TestFormula("cos(I0)");
            TestFormula("tan(I0)");
            TestFormula("asin(I0)");
            TestFormula("acos(I0)");
            TestFormula("atan(I0)");
            TestFormula("exp(I0)");
            TestFormula("log(I0)");
            TestFormula("exp2(I0)");
            TestFormula("log2(I0)");
            TestFormula("sqrt(I0)");
            TestFormula("sign(I0)");
            TestFormula("floor(I0)");
            TestFormula("ceil(I0)");
            TestFormula("frac(I0)");
        }

        [TestMethod]
        public void UnaryFunctionsExtended()
        {
            TestFormula("alpha(I0)");
            TestFormula("tosrgb(I0)");
            TestFormula("fromsrgb(I0)");
            TestFormula("red(I0)");
            TestFormula("green(I0)");
            TestFormula("blue(I0)");

            TestFormula("normalize(I0)");
            TestFormula("length(I0)");
        }

        [TestMethod]
        public void BinaryFunctionsDefault()
        {
            TestFormula("min(1, 2)");
            TestFormula("max(1, 2)");
            TestFormula("atan2(1, 2)");
            TestFormula("pow(1, 2)");
            TestFormula("fmod(1, 2)");
            TestFormula("step(1, 2)");

            TestFormula("dot(1, 2)");
            TestFormula("cross(1, 2)");
        }

        [TestMethod]
        public void BinaryFunctionsExtended()
        {
            TestFormula("equal(1, 2)");
            TestFormula("bigger(1, 2)");
            TestFormula("smaller(1, 2)");
            TestFormula("smallereq(1, 2)");
            TestFormula("biggereq(1, 2)");
        }

        [TestMethod]
        public void TertiaryFunctions()
        {
            TestFormula("rgb(1, 2, 3)");
        }

        public string GetEq(string formula)
        {
            var eq = new ImageFramework.Model.Equation.Equation(formula);
            return eq.GetHlslExpression();
        }

        /// <summary>
        /// tests if the shader compiles with the given formula
        /// </summary>
        /// <param name="formula"></param>
        public void TestFormula(string formula)
        {
            using (var shader = new ImageCombineShader(GetEq(formula), GetEq("1"), 1))
            {
                
            }
        }
    }
}
