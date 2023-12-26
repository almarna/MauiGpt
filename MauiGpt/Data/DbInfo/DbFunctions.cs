using System.ComponentModel;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Newtonsoft.Json;

namespace MauiGpt.Data.DbInfo;

public class DbFunctions
{
    private readonly IDictionary<string, string> _databases;
    private readonly ChatMemory _chatMemory;


    public DbFunctions(IDictionary<string, string> databases, ChatMemory chatMemory)
    {
        _databases = databases;
        _chatMemory = chatMemory;
    }

     [SKFunction, Description("Get database table definition from SQL server. Given a database name and a table name this function will return a table definition in json format.")]
    public async Task<string> DbDescriptionAsync(
        [Description("The database name")] string database,
        [Description("The table name")] string table
    )
    {
        try
        {
            if (!_databases.TryGetValue(database.ToLower(), out var connectionString))
            {
                return "Selected database does not exist!";
            }

            var sqlInfo = new SqlInfo(connectionString);
            var tableData = await sqlInfo.GetColumnInfoAsync(table);
            if (tableData.Count == 0)
            {
                return "Selected table does not exist!";
            }

            var serialized = JsonConvert.SerializeObject(tableData, Formatting.None);

            var result = $"Table description in json format: {{ database: {database}, table: {table}, fields: {serialized} }}";

            _chatMemory.AddMessage(AuthorRole.User, result);
            return result;
        }
        catch (Exception e)
        {
            return $"Sql server returned error: {e.Message}";
        }
    }

    [SKFunction, Description("Get information about all tables in the database from SQL server. Given a database name this function will return a list of info about all tables in the in json format. The information contains table name, schema, creation, modification date, current row count and used space info")]
    public async Task<string> DbTableDescriptionAsync(
        [Description("The database name")] string database
    )
    {
        try
        {
            if (!_databases.TryGetValue(database.ToLower(), out var connectionString))
            {
                return "Selected database does not exist!";
            }

            var sqlInfo = new SqlInfo(connectionString);
            var tableData = await sqlInfo.GetTableInfoAsync();
            if (tableData.Count == 0)
            {
                return "Can not get information of any table!";
            }

            var serialized = JsonConvert.SerializeObject(tableData, Formatting.None);

            var result = $"Information for all tables in json format: {{ {serialized} }}";

            _chatMemory.AddMessage(AuthorRole.User, result);
            return result;
        }
        catch (Exception e)
        {
            return $"Sql server returned error: {e.Message}";
        }
    }
}
