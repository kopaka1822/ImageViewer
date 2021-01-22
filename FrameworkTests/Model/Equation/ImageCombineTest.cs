using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model;
using ImageFramework.Model.Equation;
using ImageFramework.Model.Shader;
using ImageFramework.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FrameworkTests.Model.Equation
{
    [TestClass]
    public class ImageCombineTest
    {
        [TestMethod]
        public void DefaultImage()
        {
            TestFormula("I0");
        }

        [TestMethod]
        public void CombinedImage()
        {
            TestFormula("I0+I1");
        }

        [TestMethod]
        public void Numbers()
        {
            TestFormula("1");
            TestFormula("1 + 2");
            TestFormula("1 * 2");
            TestFormula("1 / 2");

            TestFormula("2 ^ 2");

            TestFormula("I0 ^ I0");
        }

        [TestMethod]
        public void IntrinsicFunctions()
        {
            TestFormula("pos()");
            TestFormula("cpos()");
            TestFormula("ipos()");
            TestFormula("size()");
            TestFormula("layer()");
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
            TestFormula("trunc(I0)");
        }

        [TestMethod]
        public void UnaryFunctionsExtended()
        {
            TestFormula("alpha(I0)");
            TestFormula("red(I0)");
            TestFormula("green(I0)");
            TestFormula("blue(I0)");

            TestFormula("a(I0)");
            TestFormula("r(I0)");
            TestFormula("g(I0)");
            TestFormula("b(I0)");

            TestFormula("x(I0)");
            TestFormula("y(I0)");
            TestFormula("z(I0)");
            TestFormula("w(I0)");

            TestFormula("tosrgb(I0)");
            TestFormula("fromsrgb(I0)");

            TestFormula("srgbAsUnorm(I0)");
            TestFormula("srgbAsSnorm(I0)");

            TestFormula("normalize(I0)");
            TestFormula("length(I0)");

            TestFormula("all(I0)");
            TestFormula("any(I0)");
            TestFormula("radians(I0)");
        }

        [TestMethod]
        public void BinaryFunctionsDefault()
        {
            TestFormula("min(I0, 1)");
            TestFormula("max(I0, 2)");
            TestFormula("atan2(I0, 2)");
            TestFormula("pow(I0, I0)");
            TestFormula("fmod(I0, I0)");
            TestFormula("step(I0, I0)");

            TestFormula("dot(I0, 2)");
            TestFormula("cross(I0, 2)");
            TestFormula("distance(I0, 2)");
        }

        [TestMethod]
        public void BinaryFunctionsExtended()
        {
            TestFormula("equal(I0, 2)");
            TestFormula("bigger(I0, 2)");
            TestFormula("smaller(I0, 2)");
            TestFormula("smallereq(I0, 2)");
            TestFormula("biggereq(I0, 2)");
        }

        [TestMethod]
        public void TertiaryFunctions()
        {
            TestFormula("rgb(I0, 2, 3)");
            TestFormula("lerp(1, 2, I0)");
            TestFormula("clamp(I0, 2, 3)");
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
            using (var shader = new ImageCombineShader(GetEq(formula), GetEq("1"), 2, new ShaderBuilder2D()))
            {
                
            }
        }

        [TestMethod]
        public void PowFunctionResults()
        {
            var models = new Models(1);
            models.AddImageFromFile(TestData.Directory + "pixel.png");
            Assert.AreEqual(1, models.Images.Size.X);
            Assert.AreEqual(1, models.Images.Size.Y);

            var eq = models.Pipelines[0].Alpha;

            // X > 0, Y > 0
            eq.Formula = "2^2";
            TestFor(models, 4.0f);

            // X > 0, Y < 0
            eq.Formula = "2^-2";
            TestFor(models, 0.25f);

            // X > 0, Y == 0
            eq.Formula = "12^0";
            TestFor(models, 1.0f);


            // X == 0, Y > 0
            eq.Formula = "0^1.2";
            TestFor(models, 0.0f);

            // X == 0, Y < 0
            eq.Formula = "0^-1.2";
            TestFor(models, float.PositiveInfinity);

            // X == 0, Y == 0
            eq.Formula = "0^0";
            TestFor(models, float.NaN);

            // X < 0, Y even
            eq.Formula = "(-2)^2";
            TestFor(models, 4.0f);

            // X < 0, Y odd
            eq.Formula = "(-2)^3";
            TestFor(models, -8.0f);

            // X < 0, Y fractional
            eq.Formula = "(-2)^1.00001";
            TestFor(models, float.NaN);
        }

        [TestMethod]
        public void LogFunctionResults()
        {
            var models = new Models(1);
            models.AddImageFromFile(TestData.Directory + "pixel.png");
            var eq = models.Pipelines[0].Alpha;
            

            eq.Formula = "log2(1)";
            TestFor(models, 0.0f);

            eq.Formula = "log(1)";
            TestFor(models, 0.0f);

            eq.Formula = "log10(1)";
            TestFor(models, 0.0f);

            eq.Formula = "log2(0)";
            TestFor(models, float.NegativeInfinity);

            eq.Formula = "log(0)";
            TestFor(models, float.NegativeInfinity);

            eq.Formula = "log10(0)";
            TestFor(models, float.NegativeInfinity);

            eq.Formula = "log2(-1)";
            TestFor(models, float.NaN);

            eq.Formula = "log(-1)";
            TestFor(models, float.NaN);

            eq.Formula = "log10(-1)";
            TestFor(models, float.NaN);
        }

        [TestMethod]
        public void SqrtFunctionResults()
        {
            var models = new Models(1);
            models.AddImageFromFile(TestData.Directory + "pixel.png");
            var eq = models.Pipelines[0].Alpha;


            eq.Formula = "sqrt(4)";
            TestFor(models, 2.0f);


            eq.Formula = "sqrt(0)";
            TestFor(models, 0.0f);


            eq.Formula = "sqrt(-1.0)";
            TestFor(models, float.NaN);
        }

        [TestMethod]
        public void NormalizeFunctionResults()
        {
            var models = new Models(1);
            models.AddImageFromFile(TestData.Directory + "pixel.png");
            var eq = models.Pipelines[0].Alpha;

            eq.Formula = "Red(normalize(RGB(1,0,0)))";
            TestFor(models, 1.0f);

            eq.Formula = "Red(normalize(RGB(-1,0,0)))";
            TestFor(models, -1.0f);

            eq.Formula = "Red(normalize(0))";
            TestFor(models, float.NaN);
        }

        // compares average alpha value with given value
        private static void TestFor(Models m, float value, float tolerance = 0.001f)
        {
            m.Apply();
            var val = m.Pipelines[0].Image.GetPixelColors(LayerMipmapSlice.Mip0)[0].Alpha;
            if (float.IsNaN(value))
                Assert.IsTrue(float.IsNaN(val));
            else
                Assert.AreEqual(value, val, tolerance);
        }
    }
}
