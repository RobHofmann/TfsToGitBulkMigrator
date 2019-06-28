using System;
using System.Collections.Generic;
using System.Text;

namespace TfsToGit.Base.Contracts.Helpers
{
    public interface IDirectoryService
    {
        void ClearTempFolder(string tempWorkingDirectory);
    }
}
