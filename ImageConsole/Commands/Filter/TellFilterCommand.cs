using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model;

namespace ImageConsole.Commands.Filter
{
    public class TellFilterCommand : Command
    {
        public TellFilterCommand() : base("-tellfilter", "", "prints list of filters")
        {
        }

        public override void Execute(List<string> arguments, Models model)
        {
            var reader = new ParameterReader(arguments);
            reader.ExpectNoMoreArgs();

            for (var index = 0; index < model.Filter.Filter.Count; index++)
            {
                var filterModel = model.Filter.Filter[index];
                Console.Out.WriteLine($"{index}: {filterModel.Name}");
                Console.Out.WriteLine(filterModel.Description);
            }
        }
    }
}
