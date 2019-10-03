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
        public ExportQualityCommand()
            : base("-exportquality", "quality", $"sets the quality level for jpg exports. Between {ExportModel.QualityMin} and {ExportModel.QualityMax}")
        {
        }

        public override void Execute(List<string> arguments, Models model)
        {
            var reader = new ParameterReader(arguments);
            var q = reader.ReadInt("quality");
            reader.ExpectNoMoreArgs();

            model.Export.Quality = q;
        }
    }
}
