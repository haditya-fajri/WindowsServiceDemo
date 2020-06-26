using Demo.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Demo
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private FileSystemWatcher _folderWatcher;
        private readonly string _inputFolder;
        private readonly IServiceProvider _services;

        public Worker(ILogger<Worker> logger, IOptions<AppSettings> settings, IServiceProvider services)
        {
            _logger = logger;
            _inputFolder = settings.Value.InputFolder;
            _services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.CompletedTask;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Service Starting");
            if (!Directory.Exists(_inputFolder))
            {
                _logger.LogWarning($"Please make sure the Input Folder [{_inputFolder}] exist, then restart the service");

                return Task.CompletedTask;
            }

            _logger.LogInformation($"Binding Events from Input Folder: {_inputFolder}");
            _folderWatcher = new FileSystemWatcher(_inputFolder, "*.TXT")
            {
                NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName
            };

            _folderWatcher.Created += Input_OnChanged;
            _folderWatcher.Renamed += Input_OnChanged;
            _folderWatcher.EnableRaisingEvents = true;

            return base.StartAsync(cancellationToken);
        }

        private void Input_OnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                _logger.LogInformation($"Inbound Change Event Triggered by [{e.FullPath}]");

                //do some work
                _logger.LogInformation("File has been Created");
                using (var scope = _services.CreateScope())
                {
                    var serviceA = scope.ServiceProvider.GetRequiredService<IServiceA>();
                    serviceA.Run();
                }

                _logger.LogInformation("Done with Inbound Change Event");
            }

            if (e.ChangeType == WatcherChangeTypes.Renamed)
            {
                _logger.LogInformation($"Inbound Change Event Triggered by [{e.FullPath}]");

                //do some work
                _logger.LogInformation("File has been Renamed");

                _logger.LogInformation("Done with Inbound Change Event");
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Service");
            _folderWatcher.EnableRaisingEvents = false;

            return base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            _logger.LogInformation("Disposing Service");
            _folderWatcher.Dispose();
            base.Dispose();
        }
    }
}
