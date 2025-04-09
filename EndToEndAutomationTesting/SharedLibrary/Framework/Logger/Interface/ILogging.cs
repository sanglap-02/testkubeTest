namespace SharedLibrary.Framework.Logger.Interface;

public interface ILogging
{
    void LogInformation(string message);
    void LogError(string message, Exception ex);
    void LogError(string message);
}