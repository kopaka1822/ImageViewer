using System.Collections.Generic;
using ImageFramework.Model;

namespace ImageConsole.Commands.Image
{
    public class OpenCommand : Command
    {
        public OpenCommand()
            :
            base("-open", "\"file1\" \"file2\" ...","imports all filenames as images")
        {}

        public override void Execute(List<string> arguments, Models model)
        {
            foreach (var filename in arguments)
            {
                model.AddImageFromFile(filename);
            }
        }
    }
}
