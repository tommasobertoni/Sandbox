using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;

namespace Demo
{
    class Program
    {
        // Ahh static!
        static string text = null;
        static int count = 5;
        static bool bold = false;
        static bool autoQuit = false;

        /// <summary>
        /// Launching the app with
        /// -b -t "Hello World" -count 8
        /// (bold text=HelloWorld 8-times)
        /// will print HELLO WORLD (text bold) 8 times.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            ArgsParser argsParser = new ArgsParser();
            Setup(argsParser);

            bool helpCommandWasFound = argsParser.Parse(args);

            // If help command was found, don't continue the execution
            // (because the user got the parameters information displayed in the console).
            if (!helpCommandWasFound)
            {
                if (text == null)
                    throw new Exception($"{nameof(text)} not set.");

                for (int i = 0; i < count; i++)
                {
                    string line = bold ? text.ToUpperInvariant() : text;
                    Console.WriteLine(line);
                }
            }

            if (helpCommandWasFound || !autoQuit)
            {
                Console.Write("Press ENTER to terminate the program...");
                Console.ReadLine();
            }
        }

        private static void Setup(ArgsParser argsParser)
        {
            // The order by which the parameters are added doesn't matter.
            
            argsParser.Add(new ArgsParser.Parameter()
            {
                Commands = new string[] { "-text", "-t" },
                Description = "(string) Defines the text to print.",
                HasValue = true,
                Action = (val) =>
                {
                    text = val;
                    if (string.IsNullOrWhiteSpace(text))
                        throw new Exception($"{nameof(text)} should not be null or empty!");
                }
            });

            argsParser.Add(new ArgsParser.Parameter()
            {
                Commands = new string[] { "-count" },
                Description = $"(int) Defines how many times the text has to be printed (default {count}).",
                HasValue = true,
                Action = (val) =>
                {
                    if (int.TryParse(val, out count))
                        if (count < 0)
                            throw new Exception($"{nameof(count)} cannot be less than 0");
                }
            });
            
            argsParser.Add(new ArgsParser.Parameter()
            {
                Commands = new string[] { "-b" },
                Description = $"If passed as argument, tells the program that the text should be printed in bold style.",
                HasValue = false,
                Action = (val) => bold = true
            });
            
            argsParser.Add(new ArgsParser.Parameter()
            {
                Commands = new string[] { "-quit" },
                Description = $"If passed as argument, tells the program to quit automatically at the end of the execution without waiting for a user input.",
                HasValue = false,
                Action = (val) => autoQuit = true
            });
        }
    }
}
