using System.Text;
using SharedLibrary.Framework.Logger.Interface;

namespace SharedLibrary.main.sqlQueries
{
    public class SqlServerQueries(ILogging logging)
    {
        public string UpdateQuerySql(string tableName, Dictionary<string, string> keyValue, string whereClause)
        {
            logging.LogInformation("sqlServerQueries - updateQuerySQL");

            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentException("Table name cannot be null or empty.");

            if (keyValue == null || keyValue.Count == 0)
                throw new ArgumentException("Columns dictionary cannot be empty.");

            StringBuilder queryBuilder = new StringBuilder();
            queryBuilder.Append("UPDATE ").Append(tableName).Append(" SET ");

            int parameterCount = 0;
            foreach (var column in keyValue)
            {
                if (parameterCount > 0)
                    queryBuilder.Append(", ");
                queryBuilder.Append(column.Key).Append(" = '").Append(column.Value).Append("'");
                parameterCount++;
            }

            queryBuilder.Append(" WHERE TEST_ID = '").Append(whereClause).Append("';");

            return queryBuilder.ToString();
        }

        public string UpdateQuerySql(string tableName, string column, string valueToSet, string searchEle, string whereClause)
        {
            logging.LogInformation("sqlServerQueries - UpdateQuerySql");

            return $"UPDATE {tableName} SET {column} = '{valueToSet}' WHERE {searchEle} = '{whereClause}';";
        }

        public string TruncateTable(string tableName)
        {
            logging.LogInformation("sqlServerQueries - CreateTable");

            return $"TRUNCATE TABLE {tableName}";
        }

        public string InsertQuerySql(string tableName, Dictionary<string, string> keyValue)
        {
            logging.LogInformation("sqlServerQueries - InsertQuerySql");
            
            var columnsPart = new StringBuilder();
            var valuesPart = new StringBuilder();

            foreach (var entry in keyValue)
            {
                columnsPart.Append(entry.Key + ", ");
                valuesPart.Append("'" + entry.Value.Replace("'", "''") + "', ");
            }

            columnsPart.Length -= 2;
            valuesPart.Length -= 2;
            
            var insertQuery = $"INSERT INTO {tableName} ({columnsPart}) VALUES ({valuesPart});";
            return insertQuery;
        }
    }
}
