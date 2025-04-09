using NPOI.Util;
using SharedLibrary.Framework.Logger.Interface;
using SharedLibrary.Framework.Logger.Service;
using SharedLibrary.main.auto.framework.constants;
using SharedLibrary.main.auto.framework.utilities;

namespace SharedLibrary.main.auto.framework.sqlDatabase
{
    public class SqlTableUtilsQat
    {
        private static string _query = "";

        private readonly ILogging _logging;
        public SqlTableUtilsQat(ILogging logging) => _logging = logging;

        public Dictionary<string, string> GetSqlTableMap(string tableName, string database, string rowName)
        {
            _logging.LogInformation($"SqlTableUtilsQAT - GetSqlTableMap: {tableName}");

            var databaseMethods = new DatabaseMethods(new Logging());

            try
            {
                _query = "SELECT * FROM " + tableName + " WHERE TEST_ID = '" + rowName + "';";

                return databaseMethods.GetValuesFromDb(DatabaseConstants.QAT_DB_HOST, database,
                    DatabaseConstants.QAT_DB_USER, DatabaseConstants.QAT_DB_PASS, DatabaseConstants.DB_PORT, _query);
            }
            catch (Exception ex)
            {
                _logging.LogError("Error :: ", ex);
                Assert.Fail();
                Assert.Fail();
                throw new RuntimeException("Error While Creating Dictionary while reading database table:" + tableName +
                                           " .");
            }
        }

        public Dictionary<string, string> GetSqlTableMap(string database, string query)
        {
            _logging.LogInformation("SqlTableUtilsQAT - GetValuesFromDb");

            var databaseMethods = new DatabaseMethods(new Logging());

            try
            {
                Thread.Sleep(20 * 1000);
                return databaseMethods.GetValuesFromDb(DatabaseConstants.QAT_DB_HOST, database,
                    DatabaseConstants.QAT_DB_USER, DatabaseConstants.QAT_DB_PASS, DatabaseConstants.DB_PORT, query);
            }
            catch (Exception ex)
            {
                _logging.LogError("Error :: ", ex);
                Assert.Fail();
                throw new RuntimeException("Error While Creating Dictionary while reading database table.");
            }
        }

        public string GetCellData(string database, string tableName, string rowName, string searchColumn,
            string columnName)
        {
            _logging.LogInformation($"SqlTableUtilsQAT - GetCellData : {tableName} - {rowName}");

            var databaseMethods = new DatabaseMethods(new Logging());

            try
            {
                _query = "SELECT " + columnName + " FROM " + tableName + " WHERE " + searchColumn + " = '" +
                         rowName + "';";
                _logging.LogInformation("Query :: " + _query);
                return databaseMethods.GetCellValueFromDb(DatabaseConstants.QAT_DB_HOST, database,
                    DatabaseConstants.QAT_DB_USER, DatabaseConstants.QAT_DB_PASS, DatabaseConstants.DB_PORT, _query);
            }
            catch (Exception ex)
            {
                _logging.LogError("Error :: ", ex);
                Assert.Fail();
                throw new RuntimeException("Error while reading database table cell:" + tableName + " .");
            }
        }

        public int GetCellData(string database, string tableName, int rowName, string searchColumn,
            string columnName)
        {
            _logging.LogInformation("SqlTableUtilsQAT - GetCellData : {tableName} - {rowName}");

            var databaseMethods = new DatabaseMethods(new Logging());

            try
            {
                _query = "SELECT " + columnName + " FROM " + tableName + " WHERE " + searchColumn + " = " + rowName +
                         ";";
                _logging.LogInformation("Query :: " + _query);

                return databaseMethods.GetIntCellValueFromDb(DatabaseConstants.QAT_DB_HOST, database,
                    DatabaseConstants.QAT_DB_USER, DatabaseConstants.QAT_DB_PASS, DatabaseConstants.DB_PORT, _query);
            }
            catch (Exception ex)
            {
                _logging.LogError("Error :: ", ex);
                Assert.Fail();
                throw new RuntimeException("Error while reading database table cell:" + tableName + " .");
            }
        }

        public int GetCellData(string database, string query)
        {
            _logging.LogInformation("SqlTableUtilsQAT - GetCellData");

            var databaseMethods = new DatabaseMethods(new Logging());

            try
            {
                return databaseMethods.GetIntCellValueFromDb(DatabaseConstants.QAT_DB_HOST, database,
                    DatabaseConstants.QAT_DB_USER, DatabaseConstants.QAT_DB_PASS, DatabaseConstants.DB_PORT, query);
            }
            catch (Exception ex)
            {
                _logging.LogError("Error :: ", ex);
                Assert.Fail();
                throw new RuntimeException("Error while reading database table cell. \n");
            }
        }

        public int GetCountData(string database, string tableName, string rowName, string searchColumn)
        {
            _logging.LogInformation($"SqlTableUtilsQAT - GetCellData : {tableName} - {rowName}");

            var databaseMethods = new DatabaseMethods(new Logging());

            try
            {
                _query = "SELECT COUNT(*) FROM " + tableName + " WHERE " + searchColumn + " = '" + rowName + "';";
                _logging.LogInformation("Query :: " + _query);

                return databaseMethods.GetIntCellValueFromDb(DatabaseConstants.QAT_DB_HOST, database,
                    DatabaseConstants.QAT_DB_USER, DatabaseConstants.QAT_DB_PASS, DatabaseConstants.DB_PORT, _query);
            }
            catch (Exception ex)
            {
                _logging.LogError("Error :: ", ex);
                Assert.Fail();
                throw new RuntimeException("Error while reading database table cell:" + tableName + " .");
            }
        }

        public string GetCellDataLike(string database, string tableName, string rowName, string searchColumn,
            string columnName)
        {
            _logging.LogInformation($"SqlTableUtilsQAT - GetCellData : {tableName} - {rowName}");

            var databaseMethods = new DatabaseMethods(new Logging());

            try
            {
                _query = "SELECT " + columnName + " FROM " + tableName + " WHERE " + searchColumn + " LIKE '%" +
                         rowName + "';";
                _logging.LogInformation("Query :: " + _query);

                return databaseMethods.GetCellValueFromDb(DatabaseConstants.QAT_DB_HOST, database,
                    DatabaseConstants.QAT_DB_USER, DatabaseConstants.QAT_DB_PASS, DatabaseConstants.DB_PORT, _query);
            }
            catch (Exception ex)
            {
                _logging.LogError("Error :: ", ex);
                Assert.Fail();
                throw new RuntimeException("Error while reading database table cell:" + tableName + " .");
            }
        }

        private Dictionary<string, string> GetSqlTableMapCommon(string tableName, string database,
            string rowName)
        {
            _logging.LogInformation($"SqlTableUtilsQAT - getSqlTableMap : {tableName}");

            var databaseMethods = new DatabaseMethods(new Logging());

            try
            {
                _query = "SELECT * FROM " + tableName + " WHERE ENVIRONMENT = '" + rowName + "';";
                _logging.LogInformation(_query);

                return databaseMethods.GetValuesFromDb(DatabaseConstants.QAT_DB_HOST, database,
                    DatabaseConstants.QAT_DB_USER, DatabaseConstants.QAT_DB_PASS, DatabaseConstants.DB_PORT, _query);
            }
            catch (Exception ex)
            {
                _logging.LogError("Error :: ", ex);
                Assert.Fail();
                throw new RuntimeException("Error While Creating HASHMap while reading database table:" + tableName +
                                           " .");
            }
        }

        public Dictionary<string, string> GetMapFromTwoTables(string tableName1, string tableName2,
            string commonRowName, string database)
        {
            _logging.LogInformation($"Getting maps from two tables: {tableName1} & {tableName2}");

            var databaseMethods = new DatabaseMethods(new Logging());

            var dataMap = new Dictionary<string, string>();
            try
            {
                var map1 = GetSqlTableMapCommon(tableName1, database, commonRowName);
                var map2 = GetSqlTableMapCommon(tableName2, database, commonRowName);

                foreach (var pair in map1)
                {
                    if (!dataMap.ContainsKey(pair.Key))
                        dataMap.Add(pair.Key, pair.Value);
                }

                foreach (var pair in map2)
                {
                    if (!dataMap.ContainsKey(pair.Key))
                        dataMap.Add(pair.Key, pair.Value);
                }
            }
            catch (Exception ex)
            {
                _logging.LogError("Exception encountered: " + "Error :: ", ex);
                throw new ApplicationException("Error while creating hash map while reading database two tables: " +
                                               tableName1 + " - " + tableName2 + ".");
            }

            return dataMap;
        }

        public List<Dictionary<string, string>> GetMapFromThreeTables(string tableName1, string tableName2,
            string tableName3,
            string commonRowName, string databaseOne, string databaseTwo)
        {
            _logging.LogInformation($"Getting maps from three tables: {tableName1}, {tableName1} & {tableName3}");

            var databaseMethods = new DatabaseMethods(new Logging());

            var dataList = new List<Dictionary<string, string>>();
            try
            {
                var map1 = GetSqlTableMapCommon(tableName1, databaseOne, commonRowName);
                var map2 = GetSqlTableMapCommon(tableName2, databaseOne, commonRowName);
                var map3 = GetSqlTableMapCommon(tableName3, databaseTwo, commonRowName);

                dataList.Add(map1);
                dataList.Add(map2);
                dataList.Add(map3);
            }
            catch (Exception ex)
            {
                _logging.LogError("Exception encountered an Error :: ", ex);
                throw new ApplicationException(
                    "Error while creating list of hash maps while reading database three tables: " +
                    tableName1 + ", " + tableName2 + " - " + tableName3 + ".");
            }

            return dataList;
        }

        public void RunInsertUpdateQuerySql(string query, string database)
        {
            _logging.LogInformation("SqlTableUtilsQAT - RunInsertUpdateQuerySql");

            var databaseMethods = new DatabaseMethods(new Logging());

            try
            {
                databaseMethods.RunInsertUpdateScriptOnDb(DatabaseConstants.QAT_DB_HOST, database,
                    DatabaseConstants.QAT_DB_USER, DatabaseConstants.QAT_DB_PASS, DatabaseConstants.DB_PORT, query);
            }
            catch (Exception ex)
            {
                _logging.LogError("Exception encountered an Error :: ", ex);
                throw new ApplicationException("Error while update the table in sql.");
            }
        }

        public List<Dictionary<string, string>> GetSqlTableMapList(string tableName, string database)
        {
            _logging.LogInformation("SqlTableUtilsQAT - getSqlTableMapList");

            var databaseMethods = new DatabaseMethods(new Logging());

            try
            {
                _query = $"SELECT * from {tableName};";
                return databaseMethods.GetRowsFromDb(DatabaseConstants.QAT_DB_HOST, database,
                    DatabaseConstants.QAT_DB_USER, DatabaseConstants.QAT_DB_PASS, DatabaseConstants.DB_PORT, _query);
            }
            catch (Exception ex)
            {
                _logging.LogError("Error while retrieving data.", ex);
                throw new RuntimeException("Error While Creating HASHMap while reading database table:" + tableName +
                                           " .");
            }
        }

        public void CreateTable(string query, string database)
        {
            _logging.LogInformation("SqlTableUtilsQAT - createTable");

            var databaseMethods = new DatabaseMethods(new Logging());

            try
            {
                databaseMethods.CreateTableDb(DatabaseConstants.QAT_DB_HOST, database, DatabaseConstants.QAT_DB_USER,
                    DatabaseConstants.QAT_DB_PASS, DatabaseConstants.DB_PORT, query);
            }
            catch (Exception ex)
            {
                _logging.LogError("Not able to create Table in Sql Database.", ex);
                throw new RuntimeException("Error while creating Table into SQL Database.");
            }
        }

        public void TruncateTable(string query, string database)
        {
            _logging.LogInformation("SqlTableUtilsQAT - TruncateTable");

            var databaseMethods = new DatabaseMethods(new Logging());

            try
            {
                databaseMethods.TruncateTableDb(DatabaseConstants.QAT_DB_HOST, database,
                    DatabaseConstants.QAT_DB_USER,
                    DatabaseConstants.QAT_DB_PASS, DatabaseConstants.DB_PORT, query);
            }
            catch (Exception ex)
            {
                _logging.LogError("Not able to truncate Table in Sql Database.", ex);
                throw new RuntimeException("Error while truncate Table into SQL Database." + "Error :: ", ex);
            }
        }
    }
}