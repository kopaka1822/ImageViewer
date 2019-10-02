using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageConsole.Commands;
using ImageFramework.Model;

namespace ImageConsole
{
    class Program
    {
        private readonly Dictionary<string, Command> commands = new Dictionary<string, Command>();

        private Program()
        {
            // setup commands
            AddCommand(new OpenCommand());
            AddCommand(new HelpCommand(commands));
        }

        private void AddCommand(Command c)
        {
            Debug.Assert(!commands.ContainsKey(c.Code));
            commands[c.Code] = c;
        }

        private void Run(string[] args)
        {
            using (var model = new Models(1))
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

                if (args.Length == 0)
                {
                    Console.Error.WriteLine("Use -help to view all commands");
                }
            }
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
