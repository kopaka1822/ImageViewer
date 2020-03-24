using System;
using System.Collections.Generic;
using ImageFramework.Model;
using ImageFramework.Utility;

namespace ImageConsole.Commands.Statistics
{
    class SSIMCommand : Command
    {
        public SSIMCommand() : base("-ssim", "imageId1 imageId2", "prints ssim of the two input images")
        {
        }

        public override void Execute(List<string> arguments, Models model)
        {
            var reader = new ParameterReader(arguments);
            var id1 = reader.ReadInt("imageId1");
            var id2 = reader.ReadInt("imageId2");
            reader.ExpectNoMoreArgs();

            if(id1 < 0 || id1 >= model.Images.NumImages)
                throw new Exception("imageId1 out of range");

            if (id2 < 0 || id2 >= model.Images.NumImages)
                throw new Exception("imageId2 out of range");

            var ssim = model.SSIM.GetStats(model.Images.Images[id1].Image, model.Images.Images[id2].Image,
                LayerMipmapRange.MostDetailed);

            Console.Error.Write("luminance:\t");
            Console.Out.WriteLine(ssim.Luminance);
            Console.Error.Write("contrast:\t");
            Console.Out.WriteLine(ssim.Contrast);
            Console.Error.Write("structure:\t");
            Console.Out.WriteLine(ssim.Structure);
            Console.Error.Write("ssim:\t\t");
            Console.Out.WriteLine(ssim.SSIM);
            Console.Error.Write("dssim:\t\t");
            Console.Out.WriteLine(ssim.DSSIM);
        }
    }
}
