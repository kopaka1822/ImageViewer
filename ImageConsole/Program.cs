using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageConsole.Commands;
using ImageConsole.Commands.Equation;
using ImageConsole.Commands.Export;
using ImageConsole.Commands.Filter;
using ImageConsole.Commands.Image;
using ImageConsole.Commands.Program;
using ImageConsole.Commands.Statistics;
using ImageFramework.Model;
using ImageFramework.Model.Progress;

namespace ImageConsole
{
    public class Program
    {
        private readonly Dictionary<string, Command> commands = new Dictionary<string, Command>();
        public bool ReadCin { get; set; } = false;
        public bool Close { get; set; } = false;

        public bool ShowProgress { get; set; } = true;

        private Program()
        {
            // setup commands
            AddCommand(new OpenCommand());
            AddCommand(new OpenAsArrayCommand());
            AddCommand(new DeleteCommand());
            AddCommand(new MoveCommand());
            AddCommand(new GenerateMipmapsCommand());
            AddCommand(new DeleteMipmapsCommand());
            AddCommand(new RecomputeMipmapsCommand());
            AddCommand(new TellLayersCommand());
            AddCommand(new TellMipmapsCommand());
            AddCommand(new TellSizeCommand());

            AddCommand(new CinCommand(this));
            AddCommand(new CloseCommand(this));
            AddCommand(new SilentCommand(this));

            AddCommand(new EquationCommand());

            AddCommand(new AddFilterCommand());
            AddCommand(new DeleteFilterCommand());
            AddCommand(new FilterParameterCommand());
            AddCommand(new TellFilterCommand());
            AddCommand(new TellFilterParamsCommand());

            var export = new ExportCommand();
            AddCommand(export);
            AddCommand(new TellFormatsCommand());
            AddCommand(new ExportMipmapCommand(export));
            AddCommand(new ExportLayerCommand(export));
            AddCommand(new ExportCroppingCommand(export));
            AddCommand(new ExportQualityCommand(export));

            AddCommand(new TellPixelCommand());
            AddCommand(new StatisticsCommand());
            AddCommand(new SSIMCommand());
            AddCommand(new TellAlphaCommand());

            AddCommand(new HelpCommand(commands));

            AddCommand(new ThumbnailCommand());
        }

        private void AddCommand(Command c)
        {
            Debug.Assert(!commands.ContainsKey(c.Code));
            Debug.Assert(c.Code.StartsWith("-"));
            commands[c.Code] = c;
        }

        private void Run(string[] args)
        {
            using (var model = new Models(1))
            {
                // overwrite culture for Console.WriteLine / ReadLine
                CultureInfo.DefaultThreadCurrentCulture = Models.Culture;
                
                model.Progress.PropertyChanged += ProgressOnPropertyChanged;

                // handle startup arguments
               InterpretArgs(args, model);

                if (args.Length == 0)
                {
                    Console.Error.WriteLine("Use -help to view all commands");
                }

                // handle cin input
                while (ReadCin && !Close)
                {
                    try
                    {
                        var line = Console.ReadLine();
                        var moreArgs = SplitToArgs(line);
                        InterpretArgs(moreArgs, model);
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine(e.Message);
                    }
                }
            }
        }

        private void ProgressOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!ShowProgress) return;

            var pm = (ProgressModel) sender;
            switch (e.PropertyName)
            {
                case nameof(ProgressModel.What):
                    Console.Error.WriteLine($"> {pm.What} {(int)(pm.Progress * 100.0f)}%");
                    break;
                case nameof(ProgressModel.IsProcessing):
                    if (pm.IsProcessing)
                    {
                        Console.Error.WriteLine("started processing");
                    }
                    else
                    {
                        Console.Error.WriteLine("finished processing");
                    }
                    break;
            }
        }

        private void InterpretArgs(string[] args, Models model)
        {
            int idx = 0;
            while (idx < args.Length)
            {
                var command = args[idx++];
                var arguments = ReadArgs(args, ref idx);

                if (!commands.TryGetValue(command, out var commandObject))
                {
                    throw new Exception("could not find command " + command + ". Use -help to view all commands");
                }

                commandObject.Execute(arguments, model);
            }
        }

        public static string[] SplitToArgs(string argString)
        {
            string curValue = "";
            var res = new List<string>();
            bool isString = false;

            // for each character
            foreach (var c in argString)
            {
                if (c == ' ' && !isString)
                {
                    if(curValue.Length != 0)
                        res.Add(curValue);
                    curValue = "";
                }
                else if(c == '\"')
                {
                    isString = !isString;
                    if (!isString && curValue.Length != 0) // string was closed
                    {
                        res.Add(curValue);
                        curValue = "";
                    }
                }
                else // add character
                {
                    curValue += c;
                }
            }

            // add last argument
            if(curValue.Length != 0)
                res.Add(curValue);

            return res.ToArray();
        }

        public static List<string> ReadArgs(string[] args, ref int startIdx)
        {
            var res = new List<string>();
            while (startIdx < args.Length)
            {
                if (args[startIdx].StartsWith("-")) break;

                res.Add(args[startIdx++]);
            }

            return res;
        }

        static void Main(string[] args)
        {
            try
            {
                var p = new Program();
                p.Run(args);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
            }

            return;
        }
    }
}
