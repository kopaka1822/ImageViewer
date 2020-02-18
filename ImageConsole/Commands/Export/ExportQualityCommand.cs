using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model;
using ImageFramework.Model.Export;

namespace ImageConsole.Commands.Export
{
    public class ExportQualityCommand : Command
    {
        private readonly ExportCommand export;

        public ExportQualityCommand(ExportCommand export)
            : base("-exportquality", "quality", $"sets the quality level for jpg exports. Between {ExportDescription.QualityMin} and {ExportDescription.QualityMax}")
        {
            this.export = export;
        }

        public override void Execute(List<string> arguments, Models model)
        {
            var reader = new ParameterReader(arguments);
            var q = reader.ReadInt("quality");
            reader.ExpectNoMoreArgs();

            export.Quality = q;
        }
    }
}
