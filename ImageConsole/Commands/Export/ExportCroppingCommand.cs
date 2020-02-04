using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model;
using ImageFramework.Utility;

namespace ImageConsole.Commands.Export
{
    class ExportCroppingCommand : Command
    {
        public ExportCroppingCommand() 
            : base("-exportcrop", "true/false [xStart yStart xEnd yEnd]", "enables/disables export cropping and sets cropping boundaries")
        {
        }

        public override void Execute(List<string> arguments, Models model)
        {
            var reader = new ParameterReader(arguments);
            var enable = reader.ReadBool("enabled");
            if (!reader.HasMoreArgs())
            {
                model.Export.UseCropping = enable;
            }
            else
            {
                var xStart = reader.ReadInt("xStart");
                var yStart = reader.ReadInt("yStart");
                var xEnd = reader.ReadInt("xEnd");
                var yEnd = reader.ReadInt("yEnd");
                reader.ExpectNoMoreArgs();

                model.Export.UseCropping = enable;
                model.Export.CropStart = new Size3(xStart, yStart, 0);
                model.Export.CropEnd = new Size3(xEnd, yEnd, 0);
            }
        }
    }
}
