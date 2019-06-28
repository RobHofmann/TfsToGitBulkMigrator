using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TfsToGit.Base.Contracts.Helpers;

namespace TfsToGit.Utils.Helpers
{
    public class DirectoryService : IDirectoryService
    {
        public void ClearTempFolder(string tempWorkingDirectory)
        {
            var di = new DirectoryInfo(tempWorkingDirectory);
            if (di.Exists)
                di.Delete(true);
            di.Create();
        }
    }
}
