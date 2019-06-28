using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace TfsToGit.Base.Contracts.Services
{
    public interface ITfsService
    {
        bool DownloadTfsRepository(string sourceAzureDevOpsUrl, string collectionName, string projectNameWithBranch, string tempWorkingDirectory);
        void RunTfsToGitMigration(ref CancellationTokenSource cts);
    }
}
