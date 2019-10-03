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
        public ExportMipmapCommand() 
            : base("-exportmipmap", "mipmap", "sets the mipmap that should be exported. -1 means all mipmaps")
        {
        }

        public override void Execute(List<string> arguments, Models model)
        {
            var reader = new ParameterReader(arguments);
            var mip = reader.ReadInt("mipmap");
            reader.ExpectNoMoreArgs();

            model.Export.Mipmap = mip;
        }
    }
}
