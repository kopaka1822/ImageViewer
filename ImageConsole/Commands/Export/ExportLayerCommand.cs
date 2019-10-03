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
        public ExportLayerCommand() 
            : base("-exportlayer", "layer", "sets the layer that should be exported. -1 means all layers")
        {
        }

        public override void Execute(List<string> arguments, Models model)
        {
            var reader = new ParameterReader(arguments);
            var layer = reader.ReadInt("layer");
            reader.ExpectNoMoreArgs();

            model.Export.Layer = layer;
        }
    }
}
