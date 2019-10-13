using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model;

namespace ImageConsole.Commands.Program
{
    public class SilentCommand : Command
    {
        private readonly ImageConsole.Program program;
        public SilentCommand(ImageConsole.Program program) :
            base("-silent", "", "disables progress output")
        {
            this.program = program;
        }

        public override void Execute(List<string> arguments, Models model)
        {
            var reader = new ParameterReader(arguments);
            reader.ExpectNoMoreArgs();

            program.ShowProgress = false;
        }
    }
}
