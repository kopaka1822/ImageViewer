using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model;

namespace ImageConsole.Commands
{
    public class HelpCommand : Command
    {
        private readonly Dictionary<string, Command> commands;

        public HelpCommand(Dictionary<string, Command> commands)
        :
        base("-help", "", "lists all commands")
        {
            this.commands = commands;
        }

        public override void Execute(List<string> arguments, Models model)
        {
            // print all commands
            Console.Error.WriteLine("Commands:");
            foreach (var command in commands)
            {
                Console.Error.WriteLine(command.Key + " " + command.Value.Arguments + "\t" + command.Value.Description);
            }
        }
    }
}
