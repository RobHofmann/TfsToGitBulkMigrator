using CsvHelper;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TfsToGit.Base.Contracts.Helpers;
using TfsToGit.Base.Contracts.Services;
using TfsToGit.Base.Models.Configuration;
using TfsToGit.Base.Models.Csv;
using ILogger = TfsToGit.Base.Contracts.Helpers.ILogger;

namespace TfsToGit.Services.HostedServices
{
    internal class MigrateVersionControlHostedService : IHostedService
    {
        private readonly ILogger _logger;
        private readonly ITfsService _tfsService;
        private readonly IGitService _gitService;
        private readonly IDirectoryService _directoryService;
        private readonly IApplicationLifetime _appLifetime;
        private readonly ICommandlineArgsHelper _commandlineArgsHelper;
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly SourceConnectionConfiguration _sourceConnectionConfiguration;
        private readonly TargetConnectionConfiguration _targetConnectionConfiguration;
        private readonly LocalConfiguration _localConfiguration;

        public MigrateVersionControlHostedService(ILogger logger, IGitService gitService, ITfsService tfsService, IDirectoryService directoryService,
            IApplicationLifetime appLifetime, ICommandlineArgsHelper commandlineArgsHelper, SourceConnectionConfiguration sourceConnectionConfiguration,
            TargetConnectionConfiguration targetConnectionConfiguration, LocalConfiguration localConfiguration)
        {
            _logger = logger;
            _gitService = gitService;
            _tfsService = tfsService;
            _directoryService = directoryService;
            _appLifetime = appLifetime;
            _commandlineArgsHelper = commandlineArgsHelper;
            _sourceConnectionConfiguration = sourceConnectionConfiguration;
            _targetConnectionConfiguration = targetConnectionConfiguration;
            _localConfiguration = localConfiguration;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _appLifetime.ApplicationStarted.Register(OnStarted);
            _appLifetime.ApplicationStopping.Register(OnStopping);
            _appLifetime.ApplicationStopped.Register(OnStopped);
        }

        private void OnStarted()
        {
            Run().GetAwaiter().GetResult();
        }

        private async Task Run()
        {
            try
            {
                _logger.Log("Hello World! Welcome to the Bulk TFS to GIT & Bulk GIT to GIT Migrator");
                _logger.Log("TRANSFER ALL THE THINGS!!!!");

                switch(_sourceConnectionConfiguration.Type.ToLower())
                {
                    case "tfs":
                        {
                            _tfsService.RunTfsToGitMigration(ref _cts);
                        }
                        break;
                    case "git":
                        {
                            _gitService.RunGitMigration(ref _cts);
                        }
                        break;
                }

                if (_cts.IsCancellationRequested) return;
                _logger.Log("Done!");
            }
            catch (Exception ex)
            {
                if (ex is ArgumentOutOfRangeException || ex is ArgumentNullException)
                {
                    _logger.Log(ex.Message);
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine();
                    _logger.Log(_commandlineArgsHelper.GetCommandlineUsageText());
                    return;
                }
                throw;
            }
        }

        private void OnStopping()
        {
            _logger.Log("Exiting...");
            _cts.Cancel();
            // Perform on-stopping activities here
        }

        private void OnStopped()
        {
            _logger.Log("Exited...");
            // Perform post-stopped activities here
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {

        }
    }
}
