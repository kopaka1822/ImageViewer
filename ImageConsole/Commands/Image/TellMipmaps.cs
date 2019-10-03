using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model;

namespace ImageConsole.Commands.Image
{
    public class TellMipmaps : Command
    {
        public TellMipmaps() 
            : base("-tellmipmaps", "", "prints number of mipmaps")
        {
        }

        public override void Execute(List<string> arguments, Models model)
        {
            var reader = new ParameterReader(arguments);
            reader.ExpectNoMoreArgs();

            Console.Out.WriteLine(model.Images.NumMipmaps);
        }
    }
}
