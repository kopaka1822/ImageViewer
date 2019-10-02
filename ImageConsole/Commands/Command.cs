using System.Collections.Generic;
using ImageFramework.Model;

namespace ImageConsole.Commands
{
    public abstract class Command
    {
        protected Command(string code, string arguments, string description)
        {
            Code = code;
            Arguments = arguments;
            Description = description;
        }

        /// <summary>
        /// command code: -open, -export (including the minus)
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// list of expected arguments
        /// </summary>
        public string Arguments { get; }

        /// <summary>
        /// what does the command do, what are the arguments
        /// </summary>
        public string Description { get; }

        public abstract void Execute(List<string> arguments, Models model);
    }
}
