using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model;

namespace ImageConsole.Commands.Program
{
    public class CloseCommand : Command
    {
        private readonly ImageConsole.Program program;
        public CloseCommand(ImageConsole.Program program) 
            : base("-close", "", "stops reading from cin")
        {
            this.program = program;
        }

        public override void Execute(List<string> arguments, Models model)
        {
            var reader = new ParameterReader(arguments);
            reader.ExpectNoMoreArgs();

            program.Close = true;
        }
    }
}
