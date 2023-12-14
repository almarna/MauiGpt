using Dapper;
using Microsoft.Data.SqlClient;

namespace MauiGpt.Data.DbInfo;

public class SqlInfo
{
    private readonly string _connectionString;

    public SqlInfo(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<IReadOnlyList<SqlColumnInformation>> GetColumnInformationAsync(string tableName)
    {
        await using var dbConnection = new SqlConnection(_connectionString);
        dbConnection.Open();

        const string query = """

                             SELECT
                               COLUMN_NAME,
                               DATA_TYPE,
                               NUMERIC_PRECISION,
                               CHARACTER_MAXIMUM_LENGTH,
                               IS_NULLABLE,
                               CASE WHEN COLUMNPROPERTY(object_id(TABLE_NAME), COLUMN_NAME, 'IsIdentity') > 0 THEN 'YES' ELSE 'NO' END AS IS_IDENTITY,
                               CASE WHEN pk.ColumnName IS NOT NULL THEN 'YES' ELSE 'NO' END AS IS_PRIMARY_KEY
                             FROM INFORMATION_SCHEMA.COLUMNS ISC
                               LEFT JOIN (SELECT c.name AS ColumnName FROM sys.indexes i
                               INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
                               INNER JOIN sys.columns c ON ic.object_id = c.object_id AND c.column_id = ic.column_id
                               INNER JOIN sys.tables t ON i.object_id = t.object_id
                                 WHERE i.is_primary_key = 1 AND t.name = @tableName) pk
                                 ON pk.ColumnName = ISC.COLUMN_NAME
                             WHERE TABLE_NAME = @tableName

                             """;

        var result = (await dbConnection.QueryAsync<SqlColumnInformation>(query, new { tableName })).ToList();

        return result;
    }
}