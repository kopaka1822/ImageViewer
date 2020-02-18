using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model;

namespace ImageConsole.Commands.Export
{
    public class ExportMipmapCommand : Command
    {
        private readonly ExportCommand export;

        public ExportMipmapCommand(ExportCommand export) 
            : base("-exportmipmap", "mipmap", "sets the mipmap that should be exported. -1 means all mipmaps")
        {
            this.export = export;
        }

        public override void Execute(List<string> arguments, Models model)
        {
            var reader = new ParameterReader(arguments);
            var mip = reader.ReadInt("mipmap");
            reader.ExpectNoMoreArgs();

            export.Mipmap = mip;
        }
    }
}
