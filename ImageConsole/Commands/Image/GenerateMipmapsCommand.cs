using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model;

namespace ImageConsole.Commands.Image
{
    class GenerateMipmapsCommand : Command
    {
        public GenerateMipmapsCommand() 
            : base("-genmipmaps", "", "(re)generates mipmaps")
        {
        }

        public override void Execute(List<string> arguments, Models model)
        {
            var reader = new ParameterReader(arguments);
            reader.ExpectNoMoreArgs();

            if (model.Images.NumMipmaps > 1)
            {
                model.Images.DeleteMipmaps();
            }

            model.Images.GenerateMipmaps(model.Scaling);
        }
    }
}
