using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.Interfaces.Services.EventTrace;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.Models;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Infrastructure.Implementations.Services.EventTrace
{
    public sealed class FileEventTraceLogger : IEventTraceLogger
    {
        private static readonly SemaphoreSlim _lock = new(1, 1);
        private readonly string _filePath;

        public FileEventTraceLogger(IWebHostEnvironment env)
        {
            // Use wwwroot instead of project root
            var folder = Path.Combine(env.WebRootPath, "event-traces");
            Directory.CreateDirectory(folder);

            _filePath = Path.Combine(
                folder,
                $"trace-{DateTime.UtcNow:yyyy-MM-dd}.txt"
            );
        }

        public async Task TrackAsync(EventTraceEntry e)
        {
            var line = BuildLine(e);

            await _lock.WaitAsync();
            try
            {
                await File.AppendAllTextAsync(_filePath, line + Environment.NewLine);
            }
            finally
            {
                _lock.Release();
            }
        }

        private static string BuildLine(EventTraceEntry e)
        {
            return $"{DateTime.UtcNow:O}" +
                   $" | Correlation={e.CorrelationId}" +
                   $" | Tenant={e.TenantId?.ToString() ?? "N/A"}" +
                   $" | Service={e.Service}" +
                   $" | Stage={e.Stage}" +
                   $" | Topic={e.Topic ?? "N/A"}" +
                   $" | Partition={e.Partition?.ToString() ?? "N/A"}" +
                   $" | Offset={e.Offset?.ToString() ?? "N/A"}" +
                   $" | Status={e.Status}" +
                   $" | Message={e.Message ?? "N/A"}" +
                   (string.IsNullOrEmpty(e.Error) ? "" : $" | Error={e.Error}");
        }

    }
}
