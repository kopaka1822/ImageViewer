using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model;
using ImageFramework.Model.Filter.Parameter;

namespace ImageConsole.Commands.Filter
{
    public class TellFilterParamsCommand : Command
    {
        public TellFilterParamsCommand() 
            : base("-tellfilterparams", "index", "prints the filter parameters of the filter at index")
        {
        }

        public override void Execute(List<string> arguments, Models model)
        {
            var reader = new ParameterReader(arguments);
            var idx = reader.ReadInt("index");
            reader.ExpectNoMoreArgs();

            var filter = model.Filter.Filter[idx];
            foreach (var fp in filter.Parameters)
            {
                Console.Out.Write($"{fp.GetBase().Name} ");
                switch (fp.GetParamterType())
                {
                    case ParameterType.Float:
                        Console.Out.WriteLine(fp.GetFloatModel().Value);
                        break;
                    case ParameterType.Int:
                        Console.Out.WriteLine(fp.GetIntModel().Value);
                        break;
                    case ParameterType.Bool:
                        Console.Out.WriteLine(fp.GetBoolModel().Value);
                        break;
                    case ParameterType.Enum:
                        Console.Out.Write(((EnumFilterParameterModel)fp).DisplayValue);
                        break;
                }
            }

            foreach (var tp in filter.TextureParameters)
            {
                Console.Out.WriteLine($"{tp.Name}: (image) {tp.Source}");
            }

            Console.Out.WriteLine(); // signal params end
        }
    }
}
