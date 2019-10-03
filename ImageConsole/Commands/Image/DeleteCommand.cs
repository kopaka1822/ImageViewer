using System.Collections.Generic;
using ImageFramework.Model;

namespace ImageConsole.Commands.Image
{
    class DeleteCommand : Command
    {
        public DeleteCommand() 
            : 
        base("-delete", "[index]", "deletes the image with the specified index or all images if no index was specified")
        {
        }

        public override void Execute(List<string> arguments, Models model)
        {
            var reader = new ParameterReader(arguments);
            if (reader.HasMoreArgs())
            {
                var idx = reader.ReadInt("image index");
                reader.ExpectNoMoreArgs();

                model.Images.DeleteImage(idx);
            }
            else
            {
                // TODO add clear method to images
                while (model.Images.NumImages != 0)
                {
                    model.Images.DeleteImage(0);
                }
            }
        }
    }
}
