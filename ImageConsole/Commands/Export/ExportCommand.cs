using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.ImageLoader;
using ImageFramework.Model;
using ImageFramework.Model.Export;
using ImageFramework.Utility;

namespace ImageConsole.Commands.Export
{
    public class ExportCommand : Command
    {
        public bool UseCropping { get; set; } = false;
        public Float3 CropStart { get; set; } = Float3.Zero;
        public Float3 CropEnd { get; set; } = Float3.One;

        public int Quality { get; set; } = 90;

        public int Layer { get; set; } = -1;
        public int Mipmap { get; set; } = -1;

        public ExportCommand() 
            : base("-export", "filename (gliFormat)", "saves the current image with the filename") {
        }

        public override void Execute(List<string> arguments, Models model)
        {
            var reader = new ParameterReader(arguments);
            var filename = reader.ReadString("filename");
            var format = reader.ReadEnum<GliFormat>("gliFormat", GliFormat.UNDEFINED);
            reader.ExpectNoMoreArgs();

            // split filename
            var splitIdx = filename.LastIndexOf('.');
            if(splitIdx < 0)
                throw new Exception($"{filename} missing file extension");

            var leftFile = filename.Substring(0, splitIdx);
            var extension = filename.Substring(splitIdx + 1);

            if (!model.Pipelines[0].IsValid)
                throw new Exception("image formula is invalid.");

            // apply changes before exporting
            model.Apply();
            var image = model.Pipelines[0].Image;

            // handle automatic detection of export format
            if (format == GliFormat.UNDEFINED)
            {
                // use original format of first image used in the equation
                var firstId = model.Pipelines[0].Color.FirstImageId;
                if(firstId >= 0 && firstId < model.Images.NumImages)
                    format = model.Images.Images[firstId].OriginalFormat;

                // test if this format is supported
                var exportFormats = ExportDescription.GetExportFormat(extension);
                if (exportFormats != null)
                {
                    if (!exportFormats.Formats.Contains(format))
                    {
                        // try to find a supported format:
                        // find the closest matching format
                        var bestScore = -float.MaxValue;
                        var stats = model.Stats.GetStatisticsFor(image);
                        
                        foreach (var exformat in exportFormats.Formats)
                        {
                            if (image.Is3D && exformat.IsExcludedFrom3DExport()) continue;

                            var rating = stats.GetFormatRating(exformat, GliFormat.UNDEFINED, false);
                            if (rating > bestScore)
                            {
                                bestScore = rating;
                                format = exformat;
                            }
                        }
                    }
                }
            }


            var desc = new ExportDescription(image, leftFile, extension)
            {
                FileFormat = format,
                UseCropping = UseCropping,
                CropStart = CropStart,
                CropEnd = CropEnd,
                Quality = Quality,
                Layer = Layer,
                Mipmap = Mipmap
            };
            model.Export.Export(desc);
        }
    }
}
