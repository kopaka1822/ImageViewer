using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model;
using ImageFramework.Model.Shader;

namespace ImageConsole.Commands
{
    public class StatisticsCommand : Command
    {
        enum StatMode
        {
            min,
            max,
            avg
        }

        public StatisticsCommand() 
            : base("-stats", "[\"min/max/avg\"]", "prints the statistics. Default is avg")
        {
        }

        public override void Execute(List<string> arguments, Models model)
        {
            var reader = new ParameterReader(arguments);
            var mode = reader.ReadEnum<StatMode>("min/max/avg", StatMode.avg);
            reader.ExpectNoMoreArgs();

            model.Apply();
            var stats = model.GetStatistics(model.Pipelines[0].Image);
            switch (mode)
            {
                case StatMode.min:
                    Print(stats.Min);
                    break;
                case StatMode.max:
                    Print(stats.Max);
                    break;
                case StatMode.avg:
                    Print(stats.Avg);
                    break;
            }
        }

        private void Print(DefaultStatistics statsMin)
        {
            Console.Error.Write("luminance: ");
            Console.Out.WriteLine(statsMin.Luminance);

            Console.Error.Write("lightness: ");
            Console.Out.WriteLine(statsMin.Lightness);

            Console.Error.Write("luma: ");
            Console.Out.WriteLine(statsMin.Luma);
        }
    }
}
