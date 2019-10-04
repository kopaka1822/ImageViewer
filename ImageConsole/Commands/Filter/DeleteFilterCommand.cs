using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model;
using ImageFramework.Model.Filter;

namespace ImageConsole.Commands.Filter
{
    public class DeleteFilterCommand : Command
    {
        public DeleteFilterCommand() 
            : base("-deletefilter", "[index]", "deletes the filter with the given index or all filter if no index is given")
        {
        }

        public override void Execute(List<string> arguments, Models model)
        {
            var reader = new ParameterReader(arguments);
            if (reader.HasMoreArgs())
            {
                var idx = reader.ReadInt("index");
                reader.ExpectNoMoreArgs();

                model.Filter.DeleteFilter(idx);
            }
            else
            {
                // delete all
                model.Filter.SetFilter(new List<FilterModel>());
            }
        }
    }
}
