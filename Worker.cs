using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WorkerService.ApplicationInsights.Sample
{
    public class Worker : BackgroundService
    {
        private static readonly HttpClient HttpClient = new HttpClient();
        
        private readonly ILogger<Worker> _logger;
        private readonly TelemetryClient _telemetryClient;

        public Worker(ILogger<Worker> logger, TelemetryClient telemetryClient)
        {
            _logger = logger;
            _telemetryClient = telemetryClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                using (_telemetryClient.StartOperation<RequestTelemetry>("OperationName"))
                {
                    _logger.LogWarning("A sample warning message. By default, logs with severity Warning or higher is captured by Application Insights");
                    _logger.LogInformation("Calling bing.com");
                    
                    var res = await HttpClient.GetAsync("https://bing.com", stoppingToken);
                    _logger.LogInformation("Calling bing completed with status:" + res.StatusCode);
                    
                    _telemetryClient.TrackEvent("Bing call event completed");
                }

                // Ensure that data is always sent, even if we are terminating the application.
                _telemetryClient.Flush();
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
