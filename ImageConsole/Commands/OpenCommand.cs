using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model;

namespace ImageConsole.Commands
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
