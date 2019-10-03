using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model;

namespace ImageConsole.Commands.Image
{
    public class MoveCommand : Command
    {
        public MoveCommand() 
            : base("-move", "oldIndex newIndex", "moves the image to the given image index")
        {
        }

        public override void Execute(List<string> arguments, Models model)
        {
            var reader = new ParameterReader(arguments);
            var idx1 = reader.ReadInt("oldIndex");
            var idx2 = reader.ReadInt("newIndex");
            reader.ExpectNoMoreArgs();

            model.Images.MoveImage(idx1, idx2);
        }
    }
}
