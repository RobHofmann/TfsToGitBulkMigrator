using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TfsToGit.Base.Contracts.Helpers;

namespace TfsToGit.Utils.Helpers
{
    public class ConsoleLogger : ILogger
    {
        public void Log(string logEntry, params string[] prefixes)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")}]{string.Join("", prefixes.Select(p => $"[{p}]"))} {logEntry}");
        }
    }
}
