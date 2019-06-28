using CsvHelper;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using TfsToGit.Base.Contracts.Helpers;
using TfsToGit.Base.Contracts.Services;
using TfsToGit.Base.Models.Configuration;
using TfsToGit.Base.Models.Csv;
using Process = System.Diagnostics.Process;

namespace TfsToGit.Services.Git
{
    public class GitService : IGitService
    {
        private readonly ILogger _logger;
        private readonly IDirectoryService _directoryService;
        private readonly SourceConnectionConfiguration _sourceConnectionConfiguration;
        private readonly TargetConnectionConfiguration _targetConnectionConfiguration;
        private readonly LocalConfiguration _localConfiguration;

        public GitService(ILogger logger, SourceConnectionConfiguration sourceConnectionConfiguration,
            TargetConnectionConfiguration targetConnectionConfiguration, LocalConfiguration localConfiguration, IDirectoryService directoryService)
        {
            _logger = logger;
            _sourceConnectionConfiguration = sourceConnectionConfiguration;
            _targetConnectionConfiguration = targetConnectionConfiguration;
            _localConfiguration = localConfiguration;
            _directoryService = directoryService;
        }

        public bool PushToRepository(string tempWorkingDirectory)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"push origin --all -u",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WorkingDirectory = tempWorkingDirectory
                }
            };

            process.Start();
            while (!process.StandardOutput.EndOfStream)
                _logger.Log(process.StandardOutput.ReadLine());

            _logger.Log($"Exit code (PushToRepository): {process.ExitCode}");

            return process.ExitCode == 0;
        }

        public bool AddOrigin(string tempWorkingDirectory, string destinationRepoUrl)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"remote add origin {destinationRepoUrl}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = tempWorkingDirectory
                }
            };

            process.Start();
            while (!process.StandardOutput.EndOfStream)
                _logger.Log(process.StandardOutput.ReadLine());

            _logger.Log($"Exit code (AddOrigin): {process.ExitCode}");

            return process.ExitCode == 0;
        }

        public bool RemoveOrigin(string tempWorkingDirectory)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"remote rm origin",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = tempWorkingDirectory
                }
            };

            process.Start();
            while (!process.StandardOutput.EndOfStream)
                _logger.Log(process.StandardOutput.ReadLine());

            _logger.Log($"Exit code (RemoveOrigin): {process.ExitCode}");

            return process.ExitCode == 0;
        }

        public string CreateGitRepository(string destinationAzureDevOpsUrl, string projectName, string repoName, string personalAccessToken)
        {
            VssCredentials creds = new VssBasicCredential(string.Empty, personalAccessToken);
            VssConnection connection = new VssConnection(new Uri(destinationAzureDevOpsUrl), creds);
            var projectClient = connection.GetClient<ProjectHttpClient>();
            var teamProject = projectClient.GetProject(projectName, true, true).Result;
            var gitClient = connection.GetClient<GitHttpClient>();

            var repo = gitClient.CreateRepositoryAsync(new GitRepository
            {
                DefaultBranch = "refs/heads/master",
                Name = repoName,
                ProjectReference = new TeamProjectReference
                {
                    Id = teamProject.Id
                }
            }).Result;

            return repo.RemoteUrl;
        }

        public bool CloneGitRepository(string gitRepoUrl, string branchName, string tempWorkingDirectory)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"clone --branch {branchName} {gitRepoUrl} .",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = tempWorkingDirectory
                }
            };

            process.Start();
            while (!process.StandardOutput.EndOfStream)
                _logger.Log(process.StandardOutput.ReadLine());

            _logger.Log($"Exit code (CloneGitRepository): {process.ExitCode}");

            return process.ExitCode == 0;
        }

        public void RunGitMigration(ref CancellationTokenSource cts)
        {
            List<GitTransferEntry> records;
            using (var reader = new StreamReader(_localConfiguration.TransferListCsvFullPath))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.Delimiter = ";";
                records = csv.GetRecords<GitTransferEntry>().ToList();
                foreach (var record in records)
                {
                    if (cts.IsCancellationRequested) break;
                    try
                    {
                        string tempWorkingDirectory = $"{_localConfiguration.TempWorkingDirectory}\\{record.SourceProjectName}\\{record.SourceSubProjectName}";
                        _directoryService.ClearTempFolder(tempWorkingDirectory);
                        if (!CloneGitRepository(string.Format(_sourceConnectionConfiguration.AzureDevOpsUrl, _sourceConnectionConfiguration.AzureDevOpsCollectionName, record.SourceProjectName, record.SourceSubProjectName), record.SourceBranchName, tempWorkingDirectory))
                        {
                            _logger.Log("ERROR: DownloadGitRepository went wrong");
                            continue;
                        }
                        if (cts.IsCancellationRequested) break;
                        if (!RemoveOrigin(tempWorkingDirectory))
                        {
                            _logger.Log("ERROR: RemoveOrigin went wrong");
                            continue;
                        }
                        string destinationRepoUrl = CreateGitRepository(_targetConnectionConfiguration.AzureDevOpsUrl, record.TargetProjectName, record.TargetRepoName, _targetConnectionConfiguration.PersonalAccessToken);
                        if (destinationRepoUrl.Length == 0)
                        {
                            _logger.Log("ERROR: destinationRepoUrl.Length == 0");
                            continue;
                        }
                        if (!AddOrigin(tempWorkingDirectory, destinationRepoUrl))
                        {
                            _logger.Log("ERROR: AddOrigin went wrong");
                            continue;
                        }
                        if (cts.IsCancellationRequested) break;
                        if (!PushToRepository(tempWorkingDirectory))
                        {
                            _logger.Log("ERROR: PushToRepository went wrong");
                            continue;
                        }
                        record.TransferedSuccessfully = true;
                    }
                    catch (Exception ex)
                    {
                        _logger.Log($"Uncaught Exception: {ex.Message}");
                        _logger.Log(ex.ToString());
                    }
                }
            }

            using (var writer = new StreamWriter(_localConfiguration.TransferListCsvFullPath))
            using (var csv = new CsvWriter(writer))
            {
                csv.Configuration.Delimiter = ";";
                csv.WriteRecords(records);
            }
        }
    }
}
