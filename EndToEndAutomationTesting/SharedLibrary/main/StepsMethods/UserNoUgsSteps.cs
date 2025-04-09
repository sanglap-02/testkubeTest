using NPOI.Util;
using SharedLibrary.Framework.Logger.Service;
using SharedLibrary.main.apiMethods.UserAndGroups;

namespace SharedLibrary.main.StepsMethods;

public static class UserNoUgsSteps
{
    public static Dictionary<string, string> AddUserNoUgs(string database, string table, string testId, Dictionary<string, string> brokerDetails)
    {
        var usersAndGroupsMethods = new UsersAndGroupsMethods(new Logging());
        
        try
        {
            var userDetails = usersAndGroupsMethods.SetGuestUsersData(database, table, testId);
            brokerDetails = brokerDetails.Concat(userDetails).GroupBy(pair => pair.Key)
                .ToDictionary(group => group.Key, group => group.Last().Value);

            return brokerDetails;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Create User step Failed.");
            Console.WriteLine(ex.StackTrace);
            throw new RuntimeException();
        }
    }
}