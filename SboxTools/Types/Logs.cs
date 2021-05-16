using System;
using System.Collections.Generic;

namespace SboxTools.Types
{
    public class Logs
    {
        public List<string> Loggers { get; }
        public List<ConsoleOutput> Lines { get; }

        public Logs()
        {
            Loggers = new List<string>();
            Lines = new List<ConsoleOutput>();
        }

        internal void AddLine(ConsoleOutput consoleOutput)
        {
            // Add the logger if we dont have it listed
            if (!Loggers.Contains(consoleOutput.Logger))
            {
                Loggers.Add(consoleOutput.Logger);
            }

            Lines.Add(consoleOutput);
        }
    }
}
