using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model;
using ImageFramework.Model.Equation;
using ImageFramework.Model.Equation.Token;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FrameworkTests.Model.Equation
{
    [TestClass]
    public class EquationTest
    {
        [TestMethod]
        public void BasicEquations()
        {
            AssertEquals("I0", "GetTexture0(coord)");

            AssertThrow("I-1");
            AssertThrow("");

            AssertSuccess("I0 + I1");
            AssertSuccess("I0+I1");

            AssertSuccess("I0/(sqrt(I1) + 1)");

            AssertSuccess("- - - -I1");
        }

        [TestMethod]
        public void MaxImageTest()
        {
            Assert.AreEqual(1, GetMaxImage("I1"));
            Assert.AreEqual(2, GetMaxImage("I1 + I2"));
            Assert.AreEqual(3, GetMaxImage("I3 + I2"));
            Assert.AreEqual(0, GetMaxImage("1"));
        }

        [TestMethod]
        public void ScientificNotation()
        {
            AssertSuccess("1e10");
            AssertSuccess("1e-10");
        }

        [TestMethod]
        public void OperatorPrecedence()
        {
            var eq = new ImageFramework.Model.Equation.HlslEquation("(3-1)*0.4+1");
            var hlsl = eq.GetHlslExpression();
            var expected =
                $"((({NumberToken.ToHlsl(3.0f)}-{NumberToken.ToHlsl(1.0f)})*{NumberToken.ToHlsl(0.4f)})+{NumberToken.ToHlsl(1.0f)})";

            Assert.AreEqual(expected, hlsl.Replace(" ", ""));

            eq = new ImageFramework.Model.Equation.HlslEquation("1+0.4*(3-1)");
            hlsl = eq.GetHlslExpression();

            expected =
                $"({NumberToken.ToHlsl(1.0f)}+({NumberToken.ToHlsl(0.4f)}*({NumberToken.ToHlsl(3.0f)}-{NumberToken.ToHlsl(1.0f)})))";

            Assert.AreEqual(expected, hlsl.Replace(" ", ""));
        }

        [TestMethod]
        public void SignOperatorAndBrackets()
        {
            var eq = new ImageFramework.Model.Equation.HlslEquation("1+0.4*-2");
            var hlsl = eq.GetHlslExpression();
            var expected =
                $"({NumberToken.ToHlsl(1.0f)}+({NumberToken.ToHlsl(0.4f)}*({NumberToken.ToHlsl(-1.0f)}*{NumberToken.ToHlsl(2.0f)})))";

            Assert.AreEqual(expected, hlsl.Replace(" ", ""));
        }

        [TestMethod]
        public void SignPrecedence()
        {
            var eq = new ImageFramework.Model.Equation.HlslEquation( "-1+2");
            var hlsl = eq.GetHlslExpression();
            var expected = $"(({NumberToken.ToHlsl(-1.0f)}*{NumberToken.ToHlsl(1.0f)})+{NumberToken.ToHlsl(2.0f)})";

            Assert.AreEqual(expected, hlsl.Replace(" ", ""));
        }

        [TestMethod]
        public void PowTranslation()
        {
            // this should translate to the same expression
            var eq = new ImageFramework.Model.Equation.HlslEquation("red(I0)^2*0.1");
            var eq2 = new ImageFramework.Model.Equation.HlslEquation("pow(red(I0),2)*0.1");
            var hlsl = eq.GetHlslExpression();
            var hlsl2 = eq2.GetHlslExpression();

            Assert.AreEqual(hlsl2, hlsl);
        }

        [TestMethod]
        public void Constants()
        {
            AssertEquals("PI", $"f4({((float)Math.PI).ToString(Models.Culture)})");
            AssertEquals("e", $"f4({((float)Math.E).ToString(Models.Culture)})");

            AssertSuccess("infinity");
            AssertSuccess("nan");
            AssertSuccess("float_max");
        }

        [TestMethod]
        public void FloatEquationTest()
        {
            // simple
            var eq = new FloatEquation("3 + 4 * 2");
            Assert.AreEqual(3.0f + 4.0f * 2.0f, eq.GetFloatExpression(0, 0, 0), 0.01f);
            
            // with parameters
            eq = new FloatEquation("max(width, height) + 1");
            Assert.AreEqual(3.0f + 1.0f, eq.GetFloatExpression(3, 1, 1), 0.01f);
            Assert.AreEqual(4.0f + 1.0f, eq.GetFloatExpression(3, 4, 1), 0.01f);

            // invalid syntax
            Assert.ThrowsException<Exception>(() => new FloatEquation("1 + w"));
            Assert.ThrowsException<Exception>(() => new FloatEquation("width**2"));
        }

        [TestMethod]
        public void ReplaceTest()
        {
            var eq = new ImageFramework.Model.Equation.HlslEquation("I0");
            var formla = eq.ReplaceImageInFormula("I0", 0, 1);
            // basic replace
            Assert.AreEqual("I1", formla);

            // nothing to replace
            formla = eq.ReplaceImageInFormula("I0", 1, 2); 
            Assert.AreEqual("I0", formla);

            // math operations replace
            formla = eq.ReplaceImageInFormula("(1/3+I2*3)^2-1", 2, 1);
            Assert.AreEqual("(1/3+I1*3)^2-1", formla);

            // functions replace
            formla = eq.ReplaceImageInFormula("max(I2, I3)", 2, 1);
            Assert.AreEqual("max(I1,I3)", formla);

            // multiple replace
            formla = eq.ReplaceImageInFormula("I2 + I2", 2, 1);
            Assert.AreEqual("I1+I1", formla);

            // multiple replace with multiple images
            formla = eq.ReplaceImageInFormula("I2+I3+I2+I1+I2", 2, 1);
            Assert.AreEqual("I1+I3+I1+I1+I1", formla);

            // replace with special constants
            formla = eq.ReplaceImageInFormula("e+I2", 2, 1);
            Assert.AreEqual("e+I1", formla);
        }

        // equation should fail
        private void AssertThrow(string formula)
        {
            try
            {
                var eq = new ImageFramework.Model.Equation.HlslEquation(formula);
            }
            catch (Exception)
            {
                return;
            }
            Assert.Fail("formula was invalid but passed");
        }

        // equation should succeed
        private void AssertSuccess(string formula)
        {
            try
            {
                var eq = new ImageFramework.Model.Equation.HlslEquation(formula);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        // equation should scceed with result
        private void AssertEquals(string formula, string result)
        {
            try
            {
                var eq = new ImageFramework.Model.Equation.HlslEquation(formula);
                var res = eq.GetHlslExpression();
                Assert.IsTrue(res.Equals(result));
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        private int GetMaxImage(string formula)
        {
            try
            {
                var eq = new ImageFramework.Model.Equation.HlslEquation(formula);
                return eq.MaxImageId;
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
                return -1;
            }
        }
    }
}
