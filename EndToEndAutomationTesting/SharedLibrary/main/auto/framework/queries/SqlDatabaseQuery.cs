using System.Text;
using NPOI.Util;
using SharedLibrary.Framework.Logger.Interface;

namespace SharedLibrary.main.auto.framework.queries
{
    public class SqlDatabaseQuery
    {
        private readonly ILogging _logging;

        public SqlDatabaseQuery(ILogging logging) => _logging = logging;

        public string CreateTableDssLoadTest(string tableName)
        {
            _logging.LogInformation("SqlDatabaseQuery - CreateTableDssLoadTest");

            var query = "CREATE TABLE " + tableName + " ( " +
                        "TEST_ID VARCHAR(100), " +
                        "UserName VARCHAR(50), " +
                        "GroupName VARCHAR(50), " +
                        "UserId VARCHAR(255), " +
                        "GroupId VARCHAR(255), " +
                        "dsscanresulttopic_TypeZero VARCHAR(20), " +
                        "dsscanresulttopic_TypeTwo VARCHAR(20), " +
                        "dsusersgroupstopic_TypeZero VARCHAR(20), " +
                        "dsusersgroupstopic_TypeThree VARCHAR(20), " +
                        "MongoDB VARCHAR(20)" +
                        ");";

            _logging.LogInformation(query);
            return query;
        }

        public string CreateInsertQuery(string tableName, Dictionary<string, string> map)
        {
            _logging.LogInformation("SqlDatabaseQuery - CreateInsertQuery");
            //string query = "INSERT INTO " + tableName + " VALUES " +
            //        "(" + map["test_id"] + ", " + map["username"] + ", " + map["groupName"] + ", " + map["dsscanresulttopic_TypeZero_verify"] + ", " + map["dsscanresulttopic_TypeTwo_verify"] + ", " + map["dsusersgroupstopic_TypeZero_verify"] + ", " + map["dsusersgroupstopic_TypeThree_verify"] + ", " + map["dsusersgroupstopic_TypeThree_verify"] + ", " + map["Mongo_verify"] + ");";

            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentException("Table name cannot be null or whitespace.", nameof(tableName));
            }

            if (map == null || map.Count == 0)
            {
                throw new ArgumentException("Map cannot be null or empty.", nameof(map));
            }

            var columns = new StringBuilder();
            var values = new StringBuilder();

            foreach (var kvp in map)
            {
                if (columns.Length > 0)
                {
                    columns.Append(", ");
                    values.Append(", ");
                }

                columns.Append(kvp.Key);
                values.Append($"'{kvp.Value}'");
            }

            var query = $"INSERT INTO {tableName} ({columns}) VALUES ({values});";
            _logging.LogInformation(query);
            return query;
        }

        public string UpdateQuery(string tableName, string columnName, string value, string testId)
        {
            _logging.LogInformation("SqlDatabaseQuery - UpdateQuery");
            try
            {
                return $@"UPDATE {tableName} SET {columnName}= '{value}' WHERE TEST_ID = '{testId}';";
            }
            catch (Exception e)
            {
                _logging.LogError("Error to create update Query.", e);
                throw new RuntimeException(e.Message);
            }
        }

        public string UpdateQuery(string tableName, string columnName, int value, string testId)
        {
            _logging.LogInformation("SqlDatabaseQuery - UpdateQuery");
            try
            {
                return $@"UPDATE {tableName} SET {columnName}= {value} WHERE TEST_ID = '{testId}';";
            }
            catch (Exception e)
            {
                _logging.LogError("Error to create update Query.", e);
                throw new RuntimeException(e.Message);
            }
        }

        public string UpdateQuery(string tableName, string columnName, string value, string searchColumn, string testId)
        {
            _logging.LogInformation("SqlDatabaseQuery - UpdateQuery");
            try
            {
                return $@"UPDATE {tableName} SET {columnName}= '{value}' WHERE {searchColumn} = '{testId}';";
            }
            catch (Exception e)
            {
                _logging.LogError("Error to create update Query.", e);
                throw new RuntimeException(e.Message);
            }
        }
    }
}