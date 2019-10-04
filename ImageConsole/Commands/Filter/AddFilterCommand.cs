using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model;

namespace ImageConsole.Commands.Filter
{
    public class AddFilterCommand : Command
    {
        public AddFilterCommand() 
            : base("-addfilter", "\"file1\" [\"file2\" ...]", "adds all filter to the pipeline")
        {
        }

        public override void Execute(List<string> arguments, Models model)
        {
            foreach (var argument in arguments)
            {
                var filter = model.CreateFilter(argument);
                model.Filter.AddFilter(filter);
            }
        }
    }
}
