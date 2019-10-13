using System;
using System.Collections.Generic;
using System.Linq;
using ImageFramework.Model;

namespace ImageConsole.Commands.Program
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

        private class CommandComparer : IComparer<KeyValuePair<string, Command>>
        {
            public int Compare(KeyValuePair<string, Command> x, KeyValuePair<string, Command> y)
            {
                return x.Key.CompareTo(y.Key);
            }
        }

        public override void Execute(List<string> arguments, Models model)
        {
            // examine padding
            var maxChars = 0;
            foreach (var command in commands)
            {
                maxChars = Math.Max(command.Key.Length + command.Value.Arguments.Length, maxChars);
            }

            maxChars += 3;

            // sort entries
            var entries = commands.ToList();
            entries.Sort(new CommandComparer());

            // print all commands
            Console.Error.WriteLine("Commands:");
            foreach (var command in entries)
            {
                Console.Out.Write(command.Key + " " + command.Value.Arguments);
                // write padding spaces
                var curChars = command.Key.Length + command.Value.Arguments.Length;
                while (curChars++ < maxChars)
                {
                    Console.Out.Write(" ");
                }

                Console.Out.WriteLine(command.Value.Description);
            }
        }
    }
}
