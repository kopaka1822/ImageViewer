using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageViewer.Models.Settings
{
    public class EquationConfig
    {
        public class Equation
        {
            public bool IsEnabled { get; set; }
            public string ColorFormula { get; set; }
            public string AlphaFormula { get; set; }
            public bool GenMipmaps { get; set; }
            public bool UseFilter { get; set; }
        }

        public List<Equation> Equations { get; } = new List<Equation>();

        public static EquationConfig LoadFromModels(ModelsEx models)
        {
            var res = new EquationConfig();
            foreach (var pipe in models.Pipelines)
            {
                res.Equations.Add(new Equation
                {
                    IsEnabled = pipe.IsEnabled,
                    ColorFormula = pipe.Color.Formula,
                    AlphaFormula = pipe.Alpha.Formula,
                    GenMipmaps = pipe.RecomputeMipmaps,
                    UseFilter = pipe.UseFilter
                });
            }

            return res;
        }

        public void ApplyToModels(ModelsEx models)
        {
            if(models.Pipelines.Count != Equations.Count)
                throw new Exception("equation count mismatch");

            for (var i = 0; i < Equations.Count; i++)
            {
                var equation = Equations[i];
                var pipe = models.Pipelines[i];
                pipe.IsEnabled = equation.IsEnabled;
                pipe.Color.Formula = equation.ColorFormula;
                pipe.Alpha.Formula = equation.AlphaFormula;
                pipe.RecomputeMipmaps = equation.GenMipmaps;
                pipe.UseFilter = equation.UseFilter;
            }
        }
    }
}
