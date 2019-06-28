using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace TfsToGit.Base.Contracts.Services
{
    public interface IGitService
    {
        bool PushToRepository(string tempWorkingDirectory);
        bool AddOrigin(string tempWorkingDirectory, string destinationRepoUrl);
        bool RemoveOrigin(string tempWorkingDirectory);
        string CreateGitRepository(string destinationAzureDevOpsUrl, string projectName, string repoName, string personalAccessToken);
        bool CloneGitRepository(string gitRepoUrl, string branchName, string tempWorkingDirectory);
        void RunGitMigration(ref CancellationTokenSource cts);
    }
}
