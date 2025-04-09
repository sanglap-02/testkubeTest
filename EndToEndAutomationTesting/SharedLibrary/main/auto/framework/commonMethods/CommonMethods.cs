using SharedLibrary.Framework.Logger.Interface;
using SharedLibrary.main.auto.framework.constants;

namespace SharedLibrary.main.auto.framework.commonMethods;

public class CommonMethods
{
    private readonly ILogging _logging;
    public CommonMethods(ILogging logging) => _logging = logging; 
    
    public string GetRandomCountry()
    {
        _logging.LogInformation("CommonMethods - GetRandomCountry");
        
        var random = new Random();
        var randomNumber = random.Next(constants.Constants.CountryName.Count);
        return constants.Constants.CountryName[randomNumber];
    }
}