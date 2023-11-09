using System.ComponentModel;
using MauiGpt.Data.DbInfo;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.ChatCompletion;
using Microsoft.SemanticKernel.Orchestration;
using Newtonsoft.Json;

namespace LabSemanticKernel.Lab3;

public class DbFunctions
{
    private readonly IDictionary<string, string> _databases;
    private readonly AiFunctions _lab;

    public DbFunctions(IDictionary<string, string> databases, AiFunctions lab)
    {
        _databases = databases;
        _lab = lab;
    }

     [SKFunction, Description("Get database table definition.")]
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
            var tableData = await sqlInfo.GetColumnInformationAsync(table);
            if (tableData.Count == 0)
            {
                return "Selected table does not exist!";
            }

            var serialized = JsonConvert.SerializeObject(tableData, Formatting.None);

            var result = $"Table description in json format: {{ database: {database}, table: {table}, fields: {serialized} }}";

            _lab.AddUserMessage(result);
            return result;
        }
        catch (Exception e)
        {
            return $"Sql server returned error: {e.Message}";
        }
    }
}
