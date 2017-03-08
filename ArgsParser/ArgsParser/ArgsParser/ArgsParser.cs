using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Utils.ArgsParser;

namespace Utils
{
    /// <summary>
    /// This class provides a set of basic features that can be used to
    /// easily parse the argument given to a console application.
    /// </summary>
    public class ArgsParser : IEnumerable<Parameter>
    {
        /// <summary>
        /// The Parameter type represents the single argument given to the application.
        /// </summary>
        public class Parameter
        {
            /// <summary>
            /// Set of commands that corresponds to the current parameter (e.g. -port, -count).
            /// The commands' matching is NOT case sensitive.
            /// </summary>
            public IEnumerable<string> Commands { get; set; }

            /// <summary>
            /// Indicates if this command has a value and that value is the argument immediate after this one
            /// (e.g. -port 8080).
            /// The default is true.
            /// </summary>
            public bool HasValue { get; set; } = true;

            /// <summary>
            /// The action to execute if this argument is found.
            /// The action's string parameter is the value of the argument, or null if it's without value.
            /// </summary>
            public Action<string> Action { get; set; }

            /// <summary>
            /// Description that is displayed when the user types the "Help" command.
            /// This should give the user enough information to understand how to use this parameter.
            /// </summary>
            public string Description { get; set; }
        }

        /// <summary>
        /// Determines if the user can type the help argument and receive a console-level report
        /// of all the available parameters and their description.
        /// </summary>
        public bool EnableHelpCommands { get; private set; } = true;

        /// <summary>
        /// Delegate for a custom Help command handler.
        /// </summary>
        /// <param name="helpCommand">The help command.</param>
        public delegate void HelpCommandDelegate(string helpCommand);

        /// <summary>
        /// Delegate used to handle the help command.
        /// If it's null, the help commands will be handled with a default behaviour.
        /// </summary>
        public HelpCommandDelegate HelpCommandHandler { get; set; }

        /// <summary>
        /// List of Help commands: it can be enhanced with more aliases or complitely rewritten.
        /// If this list is null, no help command will be detected.
        /// </summary>
        public List<string> HelpCommands { get; set; } = new List<string> { "-help", "-?" };

        private List<Parameter> _parameters = new List<Parameter>();

        /// <summary>
        /// Adds a new parameter to the available parameters.
        /// </summary>
        /// <param name="parameter">The parameter to add to the collection.</param>
        public void Add(Parameter parameter) => _parameters.Add(parameter);

        /// <summary>
        /// Return an enumerator that iterates through this collection.
        /// </summary>
        /// <returns>IEnumerator of Parameters</returns>
        public IEnumerator<Parameter> GetEnumerator() => _parameters.GetEnumerator();

        /// <summary>
        /// Return an enumerator that iterates through this collection.
        /// </summary>
        /// <returns>IEnumerator</returns>
        IEnumerator IEnumerable.GetEnumerator() => _parameters.GetEnumerator();

        /// <summary>
        /// Read the arguments and parse their value and giving those informations to the appropriate Parameter.
        /// If a help command is found, it stops the execution and returns.
        /// </summary>
        /// <param name="args">The application's arguments</param>
        /// <returns>True if the help command was found.</returns>
        public bool Parse(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    var command = args[i];

                    // If help commands are enabled and this is one of them
                    if (EnableHelpCommands && IsHelp(command))
                    {
                        HandleHelp(command);
                        return true;
                    }
                    else
                    {
                        // Find the first parameter that has a command matching the current one
                        var targetParameter = _parameters.FirstOrDefault(e => e.Commands?.Select(c => c.ToLowerInvariant())?.Contains(command.ToLowerInvariant()) ?? false);

                        if (targetParameter != null)
                        {
                            string paramValue = null;
                            if (targetParameter.HasValue)
                                if (i < args.Length - 1) // If there are more arguments next
                                    paramValue = args[++i]; // Take the next argument as value (and increment the index)

                            targetParameter.Action?.Invoke(paramValue);
                        }
                    }
                }
            }

            return false; // No help command has been found (or they are disabled)
        }

        private bool IsHelp(string command) => HelpCommands?.Select(h => h.ToLowerInvariant()).Contains(command.ToLowerInvariant()) ?? false;

        private void HandleHelp(string command)
        {
            if (HelpCommandHandler != null)
                HelpCommandHandler(command);
            else
            {
                // Default help command handling

                string separator = "-------------------------------------";
                StringBuilder sb = new StringBuilder();

                sb.AppendLine(separator);
                sb.AppendLine("Help");

                foreach (var ins in _parameters)
                {
                    if (ins.Commands != null)
                    {
                        sb.AppendLine();
                        sb.AppendLine($"  {string.Join(", ", ins.Commands)}");
                        if (!string.IsNullOrWhiteSpace(ins.Description))
                        {
                            sb.AppendLine($"    {ins.Description}");
                        }
                    }
                }

                sb.AppendLine(separator);
                Console.WriteLine(sb.ToString());
            }
        }
    }
}
