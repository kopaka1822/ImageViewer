using System.Collections.Generic;
using ImageFramework.Model;

namespace ImageConsole.Commands.Program
{
    class CinCommand : Command
    {
        private readonly ImageConsole.Program program;

        public CinCommand(ImageConsole.Program program) 
            : base("-cin", "", "keeps the console open to retrieve commands via cin")
        {
            this.program = program;
        }

        public override void Execute(List<string> arguments, Models model)
        {
            var reader = new ParameterReader(arguments);
            reader.ExpectNoMoreArgs();

            this.program.ReadCin = true;
        }
    }
}
