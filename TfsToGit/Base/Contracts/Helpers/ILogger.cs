using System;
using System.Collections.Generic;
using System.Text;

namespace TfsToGit.Base.Contracts.Helpers
{
    public interface ILogger
    {
        void Log(string logEntry, params string[] prefixes);
    }
}
