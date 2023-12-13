using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using ImageFramework.Model;

namespace ImageConsole.Commands.Image
{
    public class OpenAsArrayCommand : Command
    {
        public OpenAsArrayCommand()
        :
        base("-openarray", "\"file1\" \"file2\" ...", "imports all filenames as a single image array")
        { }

        public override void Execute(List<string> arguments, Models model)
        {
            if(arguments.Count == 0)
                throw new Exception("no files specified");

            // first load all images
            var images = new List<TextureArray2D>();
            GliFormat format = GliFormat.UNDEFINED;
            foreach (var filename in arguments)
            {
                using (var img = IO.LoadImage(filename))
                {
                    if (format == GliFormat.UNDEFINED)
                    {
                        format = img.OriginalFormat;
                    }
                    images.Add(new TextureArray2D(img));
                }
            }

            // create array
            var combined = model.CombineToArray(images);
            
            model.Images.AddImage(
                combined,
                false,
                arguments.First(),
                format
            );
        }
    }
}
