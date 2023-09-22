using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model;
using ImageFramework.Model.Scaling;

namespace ImageConsole.Commands.Image
{
    class GenerateMipmapsCommand : Command
    {
        public GenerateMipmapsCommand() 
            : base("-genmipmaps", "[Box/Triangle/Lanczos/DetailPreserving/VeryDetailPreserving None/AlphaScale]", "(re)generates mipmaps. Default arguments are: Box None")
        {
        }

        public override void Execute(List<string> arguments, Models model)
        {
            var reader = new ParameterReader(arguments);

            if(reader.HasMoreArgs())
                model.Scaling.Minify = reader.ReadEnum<ScalingModel.MinifyFilters>("None/AlphaScale", ScalingModel.MinifyFilters.Box);

            if (reader.HasMoreArgs())
                model.Scaling.AlphaTestProcess =
                    reader.ReadEnum<ScalingModel.AlphaTestPostprocess>("None/AlphaScale", ScalingModel.AlphaTestPostprocess.None);
            
            reader.ExpectNoMoreArgs();

            if (model.Images.NumMipmaps > 1)
            {
                model.Images.DeleteMipmaps();
            }

            model.Images.GenerateMipmaps(model.Scaling);
        }
    }
}
