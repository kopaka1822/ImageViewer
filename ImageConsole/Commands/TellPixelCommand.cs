using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model;
using ImageFramework.Utility;

namespace ImageConsole.Commands
{
    public class TellPixelCommand : Command
    {
        public TellPixelCommand() 
            : base("-tellpixel", "x y [layer mipmap radius]", "prints the pixel color in linear and in srgb")
        {
        }

        public override void Execute(List<string> arguments, Models model)
        {
            var reader = new ParameterReader(arguments);
            var x = reader.ReadInt("x");
            var y = reader.ReadInt("y");
            var layer = reader.ReadInt("layer", 0);
            var mipmap = reader.ReadInt("mipmap", 0);
            var radius = reader.ReadInt("radius", 0);
            reader.ExpectNoMoreArgs();

            model.Apply();
            var color = model.GetPixelValue(model.Pipelines[0].Image, new Size3(x, y, 0), new LayerMipmapSlice(layer, mipmap), radius);
            Console.Out.WriteLine(color.ToDecimalString(true, 5));
            
            color = color.ToSrgb();
            Console.Out.WriteLine(color.ToBitString(true));
        }
    }
}
