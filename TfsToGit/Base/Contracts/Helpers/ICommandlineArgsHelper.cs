using System;
using System.Collections.Generic;
using System.Text;

namespace TfsToGit.Base.Contracts.Helpers
{
    interface ICommandlineArgsHelper
    {
        TValue GetCommandlineArgumentValue<TValue>(string commandlineArgumentName, bool required = true);
        TValue GetCommandlineArgumentValue<TValue>(string commandlineArgumentName, bool required = true, params TValue[] options);

        string GetCommandlineUsageText();
    }
}
