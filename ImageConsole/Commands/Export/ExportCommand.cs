using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.ImageLoader;
using ImageFramework.Model;

namespace ImageConsole.Commands.Export
{
    public class ExportCommand : Command
    {
        public ExportCommand() 
            : base("-export", "filename gliFormat", "saves the current image with the filename")
        {
        }

        public override void Execute(List<string> arguments, Models model)
        {
            var reader = new ParameterReader(arguments);
            var filename = reader.ReadString("filename");
            var format = reader.ReadEnum<GliFormat>("gliFormat");
            reader.ExpectNoMoreArgs();

            // split filename
            var splitIdx = filename.LastIndexOf('.');
            if(splitIdx < 0)
                throw new Exception($"{filename} missing file extension");

            var leftFile = filename.Substring(0, splitIdx);
            var rightFile = filename.Substring(splitIdx + 1);

            model.ExportPipelineImage(leftFile, rightFile, format);
        }
    }
}
