using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model;
using ImageFramework.Utility;

namespace ImageConsole.Commands.Export
{
    public class ThumbnailCommand : Command
    {
        public ThumbnailCommand() 
            : base("-thumbnail", "size", "creates a thumbnail with the specified size and returns byte stream")
        {
        }

        public override void Execute(List<string> arguments, Models model)
        {
            var reader = new ParameterReader(arguments);
            var size = reader.ReadInt("size");
            reader.ExpectNoMoreArgs();
            
            model.Apply();

            using (var tex = model.CreateThumbnail(size, model.Pipelines[0].Image))
            {
                var width = tex.Size.Width;
                var heigth = tex.Size.Height;
                var bytes = tex.GetBytes(LayerMipmapSlice.Mip0, (uint)(width * heigth * 4));

                Console.Out.WriteLine(width);
                Console.Out.WriteLine(heigth);

                using (var stream = Console.OpenStandardOutput())
                {
                    stream.Write(bytes, 0, bytes.Length);
                }
            }
        }
    }
}
