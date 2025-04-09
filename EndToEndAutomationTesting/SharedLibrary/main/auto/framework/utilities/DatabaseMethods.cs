using System.Data;
using Microsoft.Data.SqlClient;
using NPOI.Util;
using SharedLibrary.Framework.Logger.Interface;

namespace SharedLibrary.main.auto.framework.utilities
{
    public class DatabaseMethods
    {
        private readonly ILogging _logging;
        public DatabaseMethods(ILogging logging) => _logging = logging;

        private SqlConnection SetDatabase(string server, string port, string database, string username,
            string password)
        {
            _logging.LogInformation("DatabaseMethods - SetDatabase");

            var connectionString =
                $"Server={server},{port};Database={database};User ID={username};Password={password};Encrypt=True;TrustServerCertificate=True;";
            return new SqlConnection(connectionString);
        }

        public Dictionary<string, string> GetValuesFromDb(string server, string database, string username,
            string password, string port, string query)
        {
            _logging.LogInformation("DatabaseMethods - GetValuesFromDb");

            try
            {
                var dbValues = new Dictionary<string, string>();

                using (var con = SetDatabase(server, port, database, username, password))
                {
                    con.Open();
                    using (var cmd = new SqlCommand(query, con))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    var key = reader.GetName(i);
                                    var value = reader.IsDBNull(i) ? null : reader[i].ToString();
                                    dbValues[key] = value ?? "";
                                }
                            }
                        }
                    }
                }

                return dbValues;
            }
            catch (Exception ex)
            {
                _logging.LogError("Error :: ", ex);
                Assert.Fail();
                throw new Exception();
            }
        }

        public string GetCellValueFromDb(string server, string database, string username, string password,
            string port, string query)
        {
            _logging.LogInformation("DatabaseMethods - GetCellValueFromDb");

            try
            {
                var dbValues = new Dictionary<string, string>();

                using (var con = SetDatabase(server, port, database, username, password))
                {
                    con.Open();
                    using (var cmd = new SqlCommand(query, con))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                for (var i = 0; i < reader.FieldCount; i++)
                                {
                                    dbValues[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetString(i);
                                }
                            }
                        }
                    }
                }

                return dbValues.Values.FirstOrDefault() ?? "";
            }
            catch (Exception ex)
            {
                _logging.LogError("Error :: ", ex);
                Assert.Fail();
                throw new Exception();
            }
        }

        public int GetIntCellValueFromDb(string server, string database, string username, string password,
            string port, string query)
        {
            _logging.LogInformation("DatabaseMethods - GetIntCellValueFromDb");
            var result = 0;

            using (var connection = SetDatabase(server, port, database, username, password))
            {
                using (var command = new SqlCommand(query, connection))
                {
                    try
                    {
                        connection.Open();
                        var queryResult = command.ExecuteScalar();

                        if (queryResult != null)
                        {
                            result = Convert.ToInt32(queryResult);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logging.LogError("Error occurred", ex);
                        Assert.Fail();
                    }
                }
            }

            return result;
        }

        public bool RunInsertUpdateScriptOnDb(string server, string database, string username, string password,
            string port, string query)
        {
            _logging.LogInformation("DatabaseMethods - runInsertUpdateScriptOnDb");

            try
            {
                using (var connection = SetDatabase(server, port, database, username, password))
                {
                    connection.Open();
                    using (var command = new SqlCommand(query, connection))
                    {
                        var rowsAffected = command.ExecuteNonQuery();
                        _logging.LogInformation($"{rowsAffected} rows were updated.");
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logging.LogError("Error occurred", ex);
                Assert.Fail();
                throw new RuntimeException("Exception While Inserting/Updating Data into DataBase Server.\n" + ex);
            }
        }

        public List<Dictionary<string, string>> GetRowsFromDb(string server, string database, string username,
            string password, string port,
            string query)
        {
            var dbDetails = new List<Dictionary<string, string>>();

            using (var con = SetDatabase(server, port, database, username, password))
            {
                try
                {
                    con.Open();
                    using (var cmd = new SqlCommand(query, con))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            var schemaTable = reader.GetSchemaTable();
                            while (reader.Read())
                            {
                                Dictionary<string, string> row = new Dictionary<string, string>();
                                foreach (DataRow column in schemaTable.Rows)
                                {
                                    var columnName = column.Field<string>("ColumnName");
                                    row[columnName] = reader[columnName].ToString() ?? "";
                                }

                                dbDetails.Add(row);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    _logging.LogError("Error occurred", e);
                    throw new Exception("Exception while running script on database.", e);
                }
            }

            return dbDetails;
        }

        public void CreateTableDb(string server, string database, string username,
            string password, string port,
            string query)
        {
            using (var con = SetDatabase(server, port, database, username, password))
            {
                try
                {
                    con.Open();

                    using (var cmd = new SqlCommand(query, con))
                    {
                        cmd.ExecuteNonQuery();
                        _logging.LogInformation("Table created successfully.");
                    }
                }
                catch (Exception e)
                {
                    _logging.LogError("Error occurred", e);
                    Assert.Fail();
                    throw new Exception(e.StackTrace);
                }
            }
        }

        public void TruncateTableDb(string server, string database, string username,
            string password, string port,
            string query)
        {
            using (var con = SetDatabase(server, port, database, username, password))
            {
                try
                {
                    con.Open();

                    using (var command = new SqlCommand(query, con))
                    {
                        command.ExecuteNonQuery();
                        _logging.LogInformation("Table Successfully truncated.");
                    }
                }
                catch (Exception ex)
                {
                    _logging.LogError("Error occurred", ex);
                    Assert.Fail();
                }
            }
        }
    }
}