using System.Net;
using Newtonsoft.Json.Linq;
using NPOI.Util;
using SharedLibrary.Framework.Logger.Interface;
using SharedLibrary.Framework.Logger.Service;
using SharedLibrary.Framework.Reports;
using SharedLibrary.main.apiMethods.JsonMethods;
using SharedLibrary.main.auto.Constants;
using SharedLibrary.main.auto.framework.apicalls;
using SharedLibrary.main.auto.framework.commonMethods;
using SharedLibrary.main.auto.framework.constants;
using SharedLibrary.main.auto.framework.sqlDatabase;
using SharedLibrary.main.sqlQueries;

namespace SharedLibrary.main.apiMethods.UserAndGroups;

public class UsersAndGroupsMethods(ILogging logging)
{
    public Dictionary<string, string> SetGuestUsersData(string database, string table, string testId)
    {
        logging.LogInformation("UserGroupsMethods - SetGuestUsersData");

        var sqlTableUtilsQat = new SqlTableUtilsQat(new Logging());
        var sqlServerQueries = new SqlServerQueries(new Logging());
        
        try
        {
            var generateData = sqlTableUtilsQat.GetCellData(database, table, testId, "TEST_ID",
                "GenerateData");

            if (generateData.Equals("yes", StringComparison.OrdinalIgnoreCase))
            {
                return new Dictionary<string, string>();
            }

            var userCase = sqlTableUtilsQat.GetCellData(database, table, testId, "TEST_ID", "usercase");

            var random = new Random();
            var randomVal4 = $"{random.Next(10000):D4}";
            var randomVal2 = $"{random.Next(100):D2}";

            var userName = GetUsernameByCase(userCase, randomVal4, randomVal2);
            var hostName = $"HOST_{randomVal4}_{randomVal2}";
            
            var query = sqlServerQueries.UpdateQuerySql(table, "username", userName, "TEST_ID", testId);
            logging.LogInformation($"Query:: {query}");
            sqlTableUtilsQat.RunInsertUpdateQuerySql(query, database);

            query = sqlServerQueries.UpdateQuerySql(table, "userhost", hostName, "TEST_ID", testId);
            logging.LogInformation($"Query:: {query}");
            sqlTableUtilsQat.RunInsertUpdateQuerySql(query, database);

            return sqlTableUtilsQat.GetSqlTableMap(table, database, testId);
        }
        catch (Exception ex)
        {
            throw new RankException($"Error While Setting Users and Groups Data. \n {ex.StackTrace}");
        }
    }

    public Dictionary<string, string> SetUserAndGroupData(string database, string table, string testId)
    {
        logging.LogInformation("UsersAndGroupsMethods - SetUserAndGroupData");

        var sqlTableUtilsQat = new SqlTableUtilsQat(new Logging());
        var sqlServerQueries = new SqlServerQueries(new Logging());
        
        try
        {
            var generateUserData = sqlTableUtilsQat.GetCellData(database, table, testId, "TEST_ID", "GenerateData");

            if (generateUserData.Equals("no", StringComparison.OrdinalIgnoreCase))
            {
                return new Dictionary<string, string>();
            }

            var userData = new Dictionary<string, string>();
            var random = new Random();

            var userCase = sqlTableUtilsQat.GetCellData(database, table, testId, "TEST_ID", "usercase");
            PopulateUserDetails(userCase, random, ref userData);
            var query = sqlServerQueries.UpdateQuerySql(table, userData, testId);
            logging.LogInformation($"Query:: {query}");
            sqlTableUtilsQat.RunInsertUpdateQuerySql(query, database);

            var generateGroupData =
                sqlTableUtilsQat.GetCellData(database, table, testId, "TEST_ID", "GenerateGroupData");
            var groupName = MayBeGenerateGroupData(random, generateGroupData, userCase, userData);
            var groupQuery = sqlServerQueries.UpdateQuerySql(table, "groupName", groupName, "TEST_ID", testId);
            logging.LogInformation($"Query:: {groupQuery}");
            sqlTableUtilsQat.RunInsertUpdateQuerySql(groupQuery, database);

            groupQuery = sqlServerQueries.UpdateQuerySql(table, "groupID", "", "TEST_ID", testId);
            logging.LogInformation($"Query:: {groupQuery}");
            sqlTableUtilsQat.RunInsertUpdateQuerySql(groupQuery, database);

            userData.Add("groupName", groupName);
            userData.Add("groupID", "");

            return userData;
        }
        catch (Exception ex)
        {
            throw new RankException($"Error While Setting Users and Groups Data. \n {ex.StackTrace}");
        }
    }

    private void PopulateUserDetails(string userCase, Random random, ref Dictionary<string, string> userDetails)
    {
        logging.LogInformation("UsersAndGroupsMethods - PopulateUserDetails");

        userDetails = GenerateUserDetails(random, userCase);
    }

    private Dictionary<string, string> GenerateUserDetails(Random rand, string userCase)
    {
        logging.LogInformation("UsersAndGroupsMethods - GenerateUserDetails");

        var commonMethods = new CommonMethods(new Logging());

        var randomVal4 = rand.Next(10000).ToString("D4");
        var randomVal2 = rand.Next(100).ToString("D2");
        var dateNameSmall = DateTime.Now.ToString("ddHHmmss");

        var username = GetUsernameByCase(userCase, randomVal4, randomVal2);
        var userHost = $"HOST_{randomVal4}_{randomVal2}";
        var firstname = $"fn_{randomVal4}_{randomVal2}";
        var lastname = $"ln_{randomVal4}_{randomVal2}";
        var displayName = $"{firstname} {lastname}";
        var email = $"email_{randomVal4}_{randomVal2}@test.com";
        var userText = $"user_{randomVal4}{randomVal2}";
        var ip = $"10.{rand.Next(256)}.{rand.Next(256)}.{rand.Next(256)}";

        var country = commonMethods.GetRandomCountry();

        var details = new Dictionary<string, string>
        {
            { "username", username },
            { "firstname", firstname },
            { "lastname", lastname },
            { "displayName", displayName },
            { "email", email },
            { "phone", dateNameSmall + randomVal4 },
            { "mobilePhone", (dateNameSmall + randomVal2) + randomVal2 },
            { "department", $"dept_{randomVal4}_{randomVal2}" },
            { "title", new[] { "Mr.", "Ms.", "Mrs.", "Dr." }[rand.Next(4)] },
            { "country", country },
            { "office", $"ofc_{randomVal4}_{randomVal2}" },
            { "description", $"desc_{randomVal4}_{randomVal2}" },
            { "userhost", userHost },
            { "userText", userText },
            { "userhostip", ip }
        };

        return details;
    }

    private string GetUsernameByCase(string userCase, string randomVal4, string randomVal2)
    {
        return userCase switch
        {
            "lower" => $"user_{randomVal4}_{randomVal2}",
            "upper" => $"USER_{randomVal4}_{randomVal2}",
            "space" => $"us er_{randomVal4}_{randomVal2}",
            "load" => $"load_user{randomVal4}_{randomVal2}",
            "specialCharLower" => $"us@$er!_{randomVal4}_{randomVal2}",
            "specialCharUpper" => $"US@$Er!_{randomVal4}_{randomVal2}",
            _ => ""
        };
    }

    private string MayBeGenerateGroupData(Random random, string generateGroupData, string userCase,
        Dictionary<string, string> userData)
    {
        logging.LogInformation("UsersAndGroupsMethods - MayBeGenerateGroupData");
        var groupName = "";
        try
        {
            if (generateGroupData.Equals("yes", StringComparison.OrdinalIgnoreCase))
            {
                groupName = GetGroupName(userCase, random);
            }

            return groupName;
        }
        catch (Exception ex)
        {
            throw new RuntimeException("Not able to create the Group Data \n" + ex.Message);
        }
    }

    private string GetGroupName(string userCase, Random random)
    {
        logging.LogInformation("UsersAndGroupsMethods - GetGroupName");

        var randomVal4 = random.Next(10000).ToString("D4");
        var randomVal2 = random.Next(100).ToString("D2");

        return userCase.Equals("load") ? $"Load_GROUP_{randomVal4}_{randomVal2}" : $"GROUP_{randomVal4}_{randomVal2}";
    }

    public async Task<string> CreateUserGroup(string database, string table, string testId,
        Dictionary<string, string> userDetails, Dictionary<string, string> envDetails)
    {
        logging.LogInformation("UsersAndGroupsMethods - CreateUserGroup");
        await Task.Delay(1000);

        var sqlTableUtilsQat = new SqlTableUtilsQat(new Logging());
        var sqlServerQueries = new SqlServerQueries(new Logging());
        var urlMethods = new UrlMethods(new Logging());
        var usersAndGroupsJson = new UsersAndGroupsJson(new Logging());

        try
        {
            var generateGroupData =
                sqlTableUtilsQat.GetCellData(database, table, testId, "TEST_ID", "GenerateGroupData");

            var groupId = "";
            var ugsUrl = urlMethods.GetUrl(envDetails, "ugs");

            if (generateGroupData.Equals("yes", StringComparison.OrdinalIgnoreCase))
            {
                ugsUrl = ugsUrl + UrlConstants.UgsGroupsUrl;
                logging.LogInformation("API URL to be used for UGS:: " + ugsUrl);

                var json = usersAndGroupsJson.AddGroupNoUserJson(userDetails["groupName"]);
                logging.LogInformation($"Add Group Json :: {json}");

                groupId = await AddGroup(ugsUrl, json, null);

                var query = sqlServerQueries.UpdateQuerySql(table, "groupID", groupId, "TEST_ID", testId);
                logging.LogInformation($"Query :: {query}");
                sqlTableUtilsQat.RunInsertUpdateQuerySql(query, database);
            }

            return groupId;
        }
        catch (Exception ex)
        {
            Assert.Fail("Error: While creating Group.");
            throw new RuntimeException("Unable to create Group. \n" + ex.Message);
        }
    }

    public async Task<string> CreateUser(string database, string table, string testId,
        Dictionary<string, string> userDetails,
        Dictionary<string, string> envDetails)
    {
        logging.LogInformation("UsersAndGroupsMethods - CreateUser");

        var sqlTableUtilsQat = new SqlTableUtilsQat(new Logging());
        var sqlServerQueries = new SqlServerQueries(new Logging());
        var urlMethods = new UrlMethods(new Logging());
        var usersAndGroupsJson = new UsersAndGroupsJson(new Logging());

        try
        {
            await Task.Delay(1000);

            var userId = "";
            var addUser = sqlTableUtilsQat.GetCellData(database, table, testId, "TEST_ID", "AddUser");

            if (addUser.Equals("Yes", StringComparison.OrdinalIgnoreCase))
            {
                var ugsUrl = urlMethods.GetUrl(envDetails, "ugs");

                ugsUrl = ugsUrl + UrlConstants.UgsUsersUrl;
                logging.LogInformation($"API URL to be use :: {ugsUrl}");

                Thread.Sleep(1);

                var json = "";

                var addUserToGroup = sqlTableUtilsQat.GetCellData(database, table, testId, "TEST_ID", "AddUserToGroup");

                if (addUserToGroup.Equals("yes", StringComparison.OrdinalIgnoreCase))
                {
                    var groupName = userDetails["groupName"];
                    json = usersAndGroupsJson.AddUserJson(userDetails, groupName);
                }
                else
                {
                    json = usersAndGroupsJson.AddUserJson(userDetails);
                }

                logging.LogInformation($"Add User Json :: \n {json}");

                userId = await AddUser(ugsUrl, json, null);

                var query = sqlServerQueries.UpdateQuerySql(table, "userID", userId, "TEST_ID", testId);
                logging.LogInformation($"Query :: {query}");
                sqlTableUtilsQat.RunInsertUpdateQuerySql(query, database);
            }
            else
            {
                logging.LogInformation("Existing User to be used.");
                ExtentManager.LogInfo("Existing User to be used.");
            }

            return userId;
        }
        catch (Exception ex)
        {
            Assert.Fail("Error: While creating user.");
            logging.LogError("Error :: ", ex);
            throw new RuntimeException("Error While creating User. \n");
        }
    }

    public async Task AddUserToGroup(string userId, Dictionary<string, string> userDetails, Dictionary<string, string> envDetails)
    {
        logging.LogInformation("UsersAndGroupsMethods - AddUserToGroup");

        var urlMethods = new UrlMethods(new Logging());
        var usersAndGroupsJson = new UsersAndGroupsJson(new Logging());
        
        try
        {
            var ugsUrl = urlMethods.GetUrl(envDetails, "ugs");
            ugsUrl = ugsUrl + UrlConstants.UgsUsersUrl + $"/{userId}";
            logging.LogInformation("API URL to be used for adding a user to group :: " + ugsUrl);
            
            var json = usersAndGroupsJson.AddUserToGroupJson(userDetails["groupName"]);
            logging.LogInformation($"Add user to group Json :: {json}");

            await AddUserToGroup(ugsUrl, json, null);
        }
        catch (Exception ex)
        {
            Assert.Fail("Error: While adding user to group.");
            throw new RuntimeException("Unable to add user to group. \n" + ex.Message);
        }
    }
    
    private async Task<string> AddGroup(string apiUrl, string payload, string token)
    {
        logging.LogInformation("UsersAndGroupsMethods - AddGroup");

        var restApiCalls = new RestApiCalls(new Logging());

        try
        {
            var response = await restApiCalls.RestPostCall(apiUrl, payload, token);

            Assert.That(
                response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.Created, Is.True,
                $"Response Status is :: {response.StatusCode}");

            var responseContent = await response.Content.ReadAsStringAsync();

            var jObject = JObject.Parse(responseContent);

            var id = jObject["id"]?.ToString() ?? "";
            logging.LogInformation($"GroupId :: {id}");

            return id;
        }
        catch (Exception ex)
        {
            logging.LogError("Error while adding group in Ugs.", ex);
            throw new RuntimeException(ex.StackTrace);
        }
    }

    private async Task<string> AddUser(string apiUrl, string payload, string token)
    {
        logging.LogInformation("UsersAndGroupsMethods - AddUser");

        var restApiCalls = new RestApiCalls(new Logging());

        try
        {
            var response = await restApiCalls.RestPostCall(apiUrl, payload, token);

            Assert.That(
                response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.Created, Is.True,
                $"Response Status is :: {response.StatusCode}");

            var responseContent = await response.Content.ReadAsStringAsync();

            var jObject = JObject.Parse(responseContent);

            var id = jObject["id"]?.ToString() ?? "";
            logging.LogInformation($"UserId :: {id}");

            return id;
        }
        catch (Exception ex)
        {
            logging.LogError("Error while adding User in Ugs.", ex);
            throw new RuntimeException(ex.StackTrace);
        }
    }

    private async Task AddUserToGroup(string apiUrl, string payload, string token)
    {
        logging.LogInformation("UsersAndGroupsMethods - AddUserToGroup");

        var restApiCalls = new RestApiCalls(new Logging());

        try
        {
            var response = await restApiCalls.RestPatchCall(apiUrl, payload, token);
            
            Assert.That(
                response.StatusCode == HttpStatusCode.NoContent, Is.True, $"Response Status is :: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            logging.LogError("Error while adding a user to a group in Ugs.", ex);
            throw new RuntimeException(ex.StackTrace);
        }
    }
}