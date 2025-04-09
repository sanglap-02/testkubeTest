using NPOI.Util;
using SharedLibrary.Framework.Logger.Service;
using SharedLibrary.main.apiMethods.UserAndGroups;

namespace SharedLibrary.main.StepsMethods;

public static class AddUserGroupSteps
{
    public static async Task<Dictionary<string, string>> AddUserGroupsSteps(string database, string table, string testId, Dictionary<string, string> brokerDetails, Dictionary<string, string> envDetails)
    {
        var usersAndGroupsMethods = new UsersAndGroupsMethods(new Logging());
        
        try
        {
            var userDetails = usersAndGroupsMethods.SetUserAndGroupData(database, table, testId);
            
            var groupId = await usersAndGroupsMethods.CreateUserGroup(database, table, testId, userDetails, envDetails);
            var userId = await usersAndGroupsMethods.CreateUser(database, table, testId, userDetails, envDetails);
            await usersAndGroupsMethods.AddUserToGroup(userId, userDetails, envDetails);
            
            userDetails.Add("userId", userId);
            userDetails.Add("groupId", groupId);
            
            brokerDetails = brokerDetails.Concat(userDetails).GroupBy(pair => pair.Key)
                .ToDictionary(group => group.Key, group => group.Last().Value);

            return brokerDetails;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Add User step Failed.");
            Console.WriteLine(ex.StackTrace);
            throw new RuntimeException();
        }
    }
}