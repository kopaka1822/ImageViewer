using System;
using System.Collections.Generic;
using ImageFramework.Model;
using ImageFramework.Model.Statistics;

namespace ImageConsole.Commands.Statistics
{
    public class StatisticsCommand : Command
    {
        enum StatMode
        {
            min,
            max,
            avg
        }

        enum StatType
        {
            luminance,
            luma,
            avg,
            lightness
        }

        public StatisticsCommand() 
            : base("-stats", "\"min/max/avg\" \"luminance/luma/avg/lightness\"", "prints the statistic")
        {
        }

        public override void Execute(List<string> arguments, Models model)
        {
            var reader = new ParameterReader(arguments);
            var mode = reader.ReadEnum<StatMode>("min/max/avg", StatMode.avg);
            var type = reader.ReadEnum<StatType>("luminance/luma/avg/lightness", StatType.avg);
            reader.ExpectNoMoreArgs();

            model.Apply();
            var stats = model.Stats.GetStatisticsFor(model.Pipelines[0].Image);
            switch (type)
            {
                case StatType.luminance:
                    Print(stats.Luminance, mode);
                    break;
                case StatType.luma:
                    Print(stats.Luma, mode);
                    break;
                case StatType.avg:
                    Print(stats.Average, mode);
                    break;
                case StatType.lightness:
                    Print(stats.Lightness, mode);
                    break;
                default: throw new Exception("unknown type " + type);
            }
        }

        private void Print(DefaultStatisticsType s, StatMode mode)
        {
            switch (mode)
            {
                case StatMode.min:
                    Console.WriteLine(s.Min);
                    break;
                case StatMode.max:
                    Console.WriteLine(s.Max);
                    break;
                case StatMode.avg:
                    Console.WriteLine(s.Avg);
                    break;
                default: throw new Exception("unknown mode " + mode);
            }
        }
    }
}
