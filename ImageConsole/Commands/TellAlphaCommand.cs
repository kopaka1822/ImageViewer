using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model;

namespace ImageConsole.Commands
{
    public class TellAlphaCommand : Command
    {
        public TellAlphaCommand()
            : base("-tellalpha", "", "prints true if any pixel has alpha that is not 1")
        {
        }

        public override void Execute(List<string> arguments, Models model)
        {
            var reader = new ParameterReader(arguments);
            reader.ExpectNoMoreArgs();

            model.Apply();
            var stats = model.GetStatistics(model.Pipelines[0].Image);
            Console.Out.WriteLine(stats.HasAlpha);
        }
    }
}
