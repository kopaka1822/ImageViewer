using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model;

namespace ImageConsole.Commands.Export
{
    public class ExportLayerCommand : Command
    {
        private readonly ExportCommand export;

        public ExportLayerCommand(ExportCommand export) 
            : base("-exportlayer", "layer", "sets the layer that should be exported. -1 means all layers")
        {
            this.export = export;
        }

        public override void Execute(List<string> arguments, Models model)
        {
            var reader = new ParameterReader(arguments);
            var layer = reader.ReadInt("layer");
            reader.ExpectNoMoreArgs();

            export.Layer = layer;
        }
    }
}
