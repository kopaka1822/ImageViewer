using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model;
using ImageFramework.Model.Filter.Parameter;

namespace ImageConsole.Commands.Filter
{
    public class FilterParameterCommand : Command
    {
        public FilterParameterCommand() 
            : base("-filterparam", "index \"param name\" value", "sets the parameter of the filter at index")
        {
        }

        public override void Execute(List<string> arguments, Models model)
        {
            var reader = new ParameterReader(arguments);
            var idx = reader.ReadInt("index");
            var name = reader.ReadString("param name");

            foreach (var fp in model.Filter.Filter[idx].Parameters)
            {
                if (fp.GetBase().Name == name)
                {
                    switch (fp.GetParamterType())
                    {
                        case ParameterType.Float:
                            fp.GetFloatModel().Value = reader.ReadInt("value");
                            break;
                        case ParameterType.Int:
                            fp.GetIntModel().Value = reader.ReadInt("value");
                            break;
                        case ParameterType.Bool:
                            fp.GetBoolModel().Value = reader.ReadBool("value");
                            break;
                    }
                    reader.ExpectNoMoreArgs();
                    return;
                }
            }

            foreach (var tp in model.Filter.Filter[idx].TextureParameters)
            {
                if (tp.Name == name)
                {
                    tp.Source = reader.ReadInt("value");
                    reader.ExpectNoMoreArgs();
                    return;
                }
            }

            throw new Exception($"parameter {name} not found");
        }
    }
}
