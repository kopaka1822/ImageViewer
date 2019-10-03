using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model;

namespace ImageConsole.Commands.Equation
{
    public class EquationCommand : Command
    {
        public EquationCommand() : 
            base("-equation", "\"color equation\" [\"alpha equation\"]", "sets image combine equations")
        {
        }

        public override void Execute(List<string> arguments, Models model)
        {
            var reader = new ParameterReader(arguments);
            var color = reader.ReadString("image equation");
            var hasAlpha = reader.HasMoreArgs();
            var alpha = reader.ReadString("alpha equation", "");
            reader.ExpectNoMoreArgs();

            model.Pipelines[0].Color.Formula = color;
            if (hasAlpha)
                model.Pipelines[0].Alpha.Formula = alpha;
        }
    }
}
