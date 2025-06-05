using Nexx.Core.Logging.Interfaces;

namespace Nexx.Core.Logging.Adapters
{
    public class CompositeLoggerAdapter<T> : ILog<T>
    {
        private readonly ILog<T> _console;
        private readonly ILog<T> _serilog;

        public CompositeLoggerAdapter()
        {
            _console = new ConsoleLogAdapter<T>();
            _serilog = new SerilogAdapter<T>();
        }

        public void LogInfo(string message)
        {
            _console.LogInfo(message);
            _serilog.LogInfo(message);
        }

        public void LogWarning(string message)
        {
            _console.LogWarning(message);
            _serilog.LogWarning(message);
        }

        public void LogError(string message, Exception? ex = null)
        {
            _console.LogError(message, ex);
            _serilog.LogError(message, ex);
        }
    }
}
