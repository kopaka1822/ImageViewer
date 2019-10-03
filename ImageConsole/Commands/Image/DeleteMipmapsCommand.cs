using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model;

namespace ImageConsole.Commands.Image
{
    public class DeleteMipmapsCommand : Command
    {
        public DeleteMipmapsCommand() 
            : base("-deletemipmaps", "", "keeps only the most detailed mipmap")
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
        }
    }
}
