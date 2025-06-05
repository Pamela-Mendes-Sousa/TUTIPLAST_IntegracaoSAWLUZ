using Nexx.Core.Logging.Interfaces;

namespace Nexx.Core.Logging.Adapters;

public class ConsoleLogAdapter<T> : ILog<T>
{
    public void LogInfo(string message) =>
        Console.WriteLine($"[INFO] [{typeof(T).Name}] {message}");

    public void LogWarning(string message) =>
        Console.WriteLine($"[WARN] [{typeof(T).Name}] {message}");

    public void LogError(string message, Exception? ex = null) =>
        Console.WriteLine($"[ERROR] [{typeof(T).Name}] {message} - {ex?.Message}");
}
