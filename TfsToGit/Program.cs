using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.IO;
using System.Threading.Tasks;
using TfsToGit.Base.Contracts.Helpers;
using TfsToGit.Base.Contracts.Services;
using TfsToGit.Base.Models.Configuration;
using TfsToGit.Services.Git;
using TfsToGit.Services.HostedServices;
using TfsToGit.Services.Tfs;
using TfsToGit.Utils.Helpers;

namespace TfsToGit
{
    class Program
    {
        public static Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureHostConfiguration(configHost =>
                {
                    configHost.SetBasePath(Directory.GetCurrentDirectory());
                    configHost.AddCommandLine(args);
                })
                .ConfigureAppConfiguration((hostContext, configApp) =>
                {
                    configApp.AddJsonFile("appsettings.json", false);
                    configApp.AddCommandLine(args);
                    
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<ILogger, ConsoleLogger>();
                    services.AddSingleton<IGitService, GitService>();
                    services.AddSingleton<ITfsService, TfsService>();
                    services.AddSingleton<IDirectoryService, DirectoryService>();
                    services.AddSingleton<ICommandlineArgsHelper, CommandlineArgsHelper>();
                    services.AddHostedService<MigrateVersionControlHostedService>();

                    services.Configure<SourceConnectionConfiguration>(hostContext.Configuration.GetSection("Source"));
                    services.AddSingleton(resolver =>
                        resolver.GetRequiredService<IOptions<SourceConnectionConfiguration>>().Value);

                    services.Configure<TargetConnectionConfiguration>(hostContext.Configuration.GetSection("Target"));
                    services.AddSingleton(resolver =>
                        resolver.GetRequiredService<IOptions<TargetConnectionConfiguration>>().Value);

                    services.Configure<LocalConfiguration>(hostContext.Configuration.GetSection("Local"));
                    services.AddSingleton(resolver =>
                        resolver.GetRequiredService<IOptions<LocalConfiguration>>().Value);
                })
                .UseConsoleLifetime();
            return host.RunConsoleAsync();
        }
    }
}
