using Microsoft.Extensions.Configuration;
using Serilog;
using SharedLibrary.Framework.Logger.Interface;
using SharedLibrary.main.auto.Constants;

namespace SharedLibrary.Framework.Logger.Service;

public class Logging : ILogging
{
    public Logging()
    {
        ConfigureLogger();
    }

    private static void ConfigureLogger()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Constants.BasePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();
    }

    public void LogInformation(string message)
    {
        Log.Information(message);
    }

    public void LogError(string message, Exception ex)
    {
        Log.Error(ex, message);
    }
    
    public void LogError(string message)
    {
        Log.Error( message);
    }
}