using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model;

namespace ImageConsole.Commands.Image
{
    class RecomputeMipmapsCommand : Command
    {
        public RecomputeMipmapsCommand() : base("-recomputemips", "true/false", "if enabled, mipmaps will be recomputed as a last step")
        {
        }

        public override void Execute(List<string> arguments, Models model)
        {
            var reader = new ParameterReader(arguments);
            model.Pipelines[0].RecomputeMipmaps = reader.ReadBool("true/false");
            reader.ExpectNoMoreArgs();
        }
    }
}
