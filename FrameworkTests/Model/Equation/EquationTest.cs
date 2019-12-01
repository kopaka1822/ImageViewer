using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            AssertEquals("I0", "GetTexture0(coord.xy)");

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
        public void OperatorPrecedence()
        {
            var eq = new ImageFramework.Model.Equation.Equation("(3-1)*0.4+1");
            var hlsl = eq.GetHlslExpression();
            var expected =
                $"((({NumberToken.ToHlsl(3.0f)}-{NumberToken.ToHlsl(1.0f)})*{NumberToken.ToHlsl(0.4f)})+{NumberToken.ToHlsl(1.0f)})";

            Assert.AreEqual(expected, hlsl.Replace(" ", ""));

            eq = new ImageFramework.Model.Equation.Equation("1+0.4*(3-1)");
            hlsl = eq.GetHlslExpression();

            expected =
                $"({NumberToken.ToHlsl(1.0f)}+({NumberToken.ToHlsl(0.4f)}*({NumberToken.ToHlsl(3.0f)}-{NumberToken.ToHlsl(1.0f)})))";

            Assert.AreEqual(expected, hlsl.Replace(" ", ""));
        }

        [TestMethod]
        public void SignOperatorAndBrackets()
        {
            var eq = new ImageFramework.Model.Equation.Equation("1+0.4*-2");
            var hlsl = eq.GetHlslExpression();
            var expected =
                $"({NumberToken.ToHlsl(1.0f)}+({NumberToken.ToHlsl(0.4f)}*({NumberToken.ToHlsl(-1.0f)}*{NumberToken.ToHlsl(2.0f)})))";

            Assert.AreEqual(expected, hlsl.Replace(" ", ""));
        }

        [TestMethod]
        public void SignPrecedence()
        {
            var eq = new ImageFramework.Model.Equation.Equation( "-1+2");
            var hlsl = eq.GetHlslExpression();
            var expected = $"(({NumberToken.ToHlsl(-1.0f)}*{NumberToken.ToHlsl(1.0f)})+{NumberToken.ToHlsl(2.0f)})";

            Assert.AreEqual(expected, hlsl.Replace(" ", ""));
        }

        [TestMethod]
        public void PowTranslation()
        {
            // this should translate to the same expression
            var eq = new ImageFramework.Model.Equation.Equation("red(I0)^2*0.1");
            var eq2 = new ImageFramework.Model.Equation.Equation("pow(red(I0),2)*0.1");
            var hlsl = eq.GetHlslExpression();
            var hlsl2 = eq2.GetHlslExpression();

            Assert.AreEqual(hlsl2, hlsl);
        }

        // equation should fail
        private void AssertThrow(string formula)
        {
            try
            {
                var eq = new ImageFramework.Model.Equation.Equation(formula);
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
                var eq = new ImageFramework.Model.Equation.Equation(formula);
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
                var eq = new ImageFramework.Model.Equation.Equation(formula);
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
                var eq = new ImageFramework.Model.Equation.Equation(formula);
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
