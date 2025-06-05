namespace Nexx.Core.Logging.Interfaces;

public interface ILog<T>
{
    void LogInfo(string message);
    void LogWarning(string message);
    void LogError(string message, Exception? ex = null);
}
