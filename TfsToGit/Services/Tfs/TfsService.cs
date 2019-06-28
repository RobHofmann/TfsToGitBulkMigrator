using CsvHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using TfsToGit.Base.Contracts.Helpers;
using TfsToGit.Base.Contracts.Services;
using TfsToGit.Base.Models.Configuration;
using TfsToGit.Base.Models.Csv;

namespace TfsToGit.Services.Tfs
{
    public class TfsService : ITfsService
    {
        private readonly ILogger _logger;
        private readonly IGitService _gitService;
        private readonly IDirectoryService _directoryService;
        private readonly SourceConnectionConfiguration _sourceConnectionConfiguration;
        private readonly TargetConnectionConfiguration _targetConnectionConfiguration;
        private readonly LocalConfiguration _localConfiguration;

        public TfsService(ILogger logger, IGitService gitService, SourceConnectionConfiguration sourceConnectionConfiguration,
            TargetConnectionConfiguration targetConnectionConfiguration, LocalConfiguration localConfiguration, IDirectoryService directoryService)
        {
            _logger = logger;
            _gitService = gitService;
            _sourceConnectionConfiguration = sourceConnectionConfiguration;
            _targetConnectionConfiguration = targetConnectionConfiguration;
            _localConfiguration = localConfiguration;
            _directoryService = directoryService;
        }

        public bool DownloadTfsRepository(string sourceAzureDevOpsUrl, string collectionName, string projectNameWithBranch, string tempWorkingDirectory)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git-tfs",
                    Arguments = $"clone --branches=all --all {sourceAzureDevOpsUrl}/{collectionName} \"{projectNameWithBranch}\" .",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WorkingDirectory = tempWorkingDirectory
                }
            };

            process.Start();
            while (!process.StandardOutput.EndOfStream)
                _logger.Log(process.StandardOutput.ReadLine());

            return process.ExitCode == 0;
        }

        public void RunTfsToGitMigration(ref CancellationTokenSource cts)
        {
            List<TfsTransferEntry> records;
            using (var reader = new StreamReader(_localConfiguration.TransferListCsvFullPath))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.Delimiter = ";";
                records = csv.GetRecords<TfsTransferEntry>().ToList();
                foreach (var record in records)
                {
                    if (cts.IsCancellationRequested) break;
                    try
                    {
                        string tempWorkingDirectory = $"{_localConfiguration.TempWorkingDirectory}\\{record.SourceProjectName}\\{record.SourceSubProjectName}";
                        _directoryService.ClearTempFolder(tempWorkingDirectory);
                        if (!DownloadTfsRepository(_sourceConnectionConfiguration.AzureDevOpsUrl, _sourceConnectionConfiguration.AzureDevOpsCollectionName, $"$/{record.SourceProjectName}/{record.SourceSubProjectName}/{record.SourceBranchName}", tempWorkingDirectory))
                            continue;
                        if (cts.IsCancellationRequested) break;
                        string destinationRepoUrl = _gitService.CreateGitRepository(_targetConnectionConfiguration.AzureDevOpsUrl, record.TargetProjectName, record.TargetRepoName, _targetConnectionConfiguration.PersonalAccessToken);
                        if (destinationRepoUrl.Length == 0)
                            continue;
                        if (cts.IsCancellationRequested) break;
                        if (!_gitService.AddOrigin(tempWorkingDirectory, destinationRepoUrl))
                            continue;
                        if (!_gitService.PushToRepository(tempWorkingDirectory))
                            continue;
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
