using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextureViewer.Equation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextureViewer.Equation.Tests
{
    [TestClass()]
    public class EquationTests
    {
        [TestMethod()]
        public void EquationTest()
        {
            // test for some formulas
            AssertEquals("I0", 1, "GetTexture0()");

            AssertThrow("I0", 0);
            AssertThrow("", 1);

            AssertSuccess("I0 + I1", 2);
            AssertThrow("I0 + I1", 1);

            AssertSuccess("I0/(sqrt(I1)+1)", 2);
            AssertSuccess("- - - -I1", 2);
        }

        // equation should fail
        private void AssertThrow(string formula, int numImages)
        {
            try
            {
                var eq = new Equation(formula, numImages);
            }
            catch (Exception)
            {
                return;
            }
            Assert.Fail("formula was invalid but passed");
        }

        // equation should succeed
        private void AssertSuccess(string formula, int numImages)
        {
            try
            {
                var eq = new Equation(formula, numImages);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        // equation should scceed with result
        private void AssertEquals(string formula, int numImages, string result)
        {
            try
            {
                var eq = new Equation(formula, numImages);
                var res = eq.GetOpenGlExpression();
                Assert.IsTrue(res.Equals(result));
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }
    }
}