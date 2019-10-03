using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model;

namespace ImageConsole.Commands.Image
{
    public class TellSizeCommand : Command
    {
        public TellSizeCommand() : 
            base("-tellsize", "[mipmapIndex]", "prints the width and height of the mipmap")
        {
        }

        public override void Execute(List<string> arguments, Models model)
        {
            var reader = new ParameterReader(arguments);
            var mip = reader.ReadInt("mipmapIndex", 0);
            reader.ExpectNoMoreArgs();

            Console.Out.WriteLine(model.Images.GetWidth(mip));
            Console.Out.WriteLine(model.Images.GetHeight(mip));
        }
    }
}
