using Nexx.Core.Logging.Interfaces;
using Serilog;

namespace Nexx.Core.Logging.Adapters
{
    public class SerilogAdapter<T> : ILog<T>
    {
        private readonly ILogger _logger;

        public SerilogAdapter()
        {
            _logger = Log.ForContext<T>();
        }

        public void LogInfo(string message) => _logger.Information(message);
        public void LogWarning(string message) => _logger.Warning(message);
        public void LogError(string message, Exception? ex = null) => _logger.Error(ex, message);
    }
}
