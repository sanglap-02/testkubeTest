using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using NPOI.Util;
using SharedLibrary.Framework.Logger.Interface;

namespace SharedLibrary.main.apiMethods.JsonMethods;

public class UsersAndGroupsJson
{
    private readonly ILogging _logging;
    public UsersAndGroupsJson(ILogging logging) => _logging = logging;

    public string AddGroupJson(Dictionary<string, string> userDetails)
    {
        _logging.LogInformation("UsersAndGroupsJson - AddGroupJson");

        try
        {
            var body = new Dictionary<string, object>();

            body.Add("name", userDetails["groupName"]);
            body.Add("status", "Enabled");

            var parentGroups = new List<Dictionary<string, object>>();
            body.Add("parentGroups", parentGroups);

            var users = new List<string>();
            users.Add(userDetails["username"]);
            body.Add("users", users);

            var json = JsonConvert.SerializeObject(body);
            return json;
        }
        catch (Exception ex)
        {
            _logging.LogError("Error :: ", ex);
            throw new RuntimeException("Error While Creating Group Json. \n" + ex);
        }
    }

    public string AddGroupNoUserJson(string groupName)
    {
        _logging.LogInformation("UsersAndGroupsJson - AddGroupNoUserJson");

        try
        {
            var body = new Dictionary<string, object>
            {
                ["name"] = groupName,
                ["status"] = "Enabled"
            };

            var parentGroups = new List<Dictionary<string, object>>();
            body["parentGroups"] = parentGroups;

            var users = new List<string>();
            body["users"] = users;

            return JsonConvert.SerializeObject(body);
        }
        catch (Exception ex)
        {
            _logging.LogError("Error :: ", ex);
            throw new RuntimeException("Error While Creating Group Json. \n");
        }
 
    }

    public string AddUserToGroupJson(string groupName)
    {
        _logging.LogInformation("UsersAndGroupsJson - AddUserToGroupJson");

        try
        {
            var body = new
            {
                parentGroupsNames = new List<string> { $"{groupName}" }
            };
            
            return JsonConvert.SerializeObject(body, Formatting.Indented);
        }
        catch (Exception ex)
        {
            _logging.LogError("Error :: ", ex);
            throw new RuntimeException("Error while creating Json for adding user to a group. \n");
        }
    }

    public string AddUserJson(Dictionary<string, string> userDetails)
    {
        _logging.LogInformation("UsersAndGroupsJson - AddUserJson");
        return AddUserJson(userDetails, null);
    }
    
    public string AddUserJson(Dictionary<string, string> userDetails, string groupName)
    {
        _logging.LogInformation("UsersAndGroupsJson - AddUserJson");

        try
        {
            var body = new Dictionary<string, object>
            {
                ["index"] = 1,
                ["firstName"] = userDetails["firstname"],
                ["lastName"] = userDetails["lastname"],
                ["title"] = userDetails["title"],
                ["email"] = userDetails["email"],
                ["username"] = userDetails["username"],
                ["department"] = userDetails["department"],
                ["displayName"] = userDetails["displayName"],
                ["phone"] = userDetails["phone"],
                ["mobilePhone"] = userDetails["mobilePhone"],
                ["country"] = userDetails["country"],
                ["office"] = userDetails["office"],
                ["description"] = userDetails["description"],
                ["source"] = "ManualEditing", 
                ["status"] = "Enabled"
            };

            var parentGroups = new List<string>();
            if (!groupName.IsNullOrEmpty())
            {
                parentGroups.Add(groupName);
                body["defaultGroup"] = groupName;
            }
            else
            {
                body["defaultGroup"] = "";
            }
            body["parentGroups"] = parentGroups;

            return JsonConvert.SerializeObject(body);
        }
        catch (Exception ex)
        {
            _logging.LogError("Error While Creating User Json. \n", ex);
            throw new RuntimeException(ex.StackTrace);
        }
    }
}