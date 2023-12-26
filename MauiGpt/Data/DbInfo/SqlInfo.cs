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

    public async Task<IReadOnlyList<ColumnInfoDto>> GetColumnInfoAsync(string tableName)
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

        var result = (await dbConnection.QueryAsync<ColumnInfoDto>(query, new { tableName })).ToList();

        return result;
    }

    public async Task<IReadOnlyList<TableInfoDto>> GetTableInfoAsync()
    {
        await using var dbConnection = new SqlConnection(_connectionString);
        dbConnection.Open();

        const string query = """
                             SELECT
                                 t.name AS TableName,
                                 s.name AS SchemaName,
                                 t.create_date AS CreationDate,
                                 t.modify_date AS LastModifiedDate,
                                 p.rows AS RowCounts,
                                 SUM(a.total_pages) * 8 AS TotalSpaceKB,
                                 SUM(a.used_pages) * 8 AS UsedSpaceKB,
                                 (SUM(a.total_pages) - SUM(a.used_pages)) * 8 AS UnusedSpaceKB
                             FROM 
                                 sys.tables t
                             INNER JOIN 
                                 sys.indexes i ON t.object_id = i.object_id
                             INNER JOIN 
                                 sys.partitions p ON i.object_id = p.object_id AND i.index_id = p.index_id
                             INNER JOIN 
                                 sys.allocation_units a ON p.partition_id = a.container_id
                             LEFT OUTER JOIN 
                                 sys.schemas s ON t.schema_id = s.schema_id
                             WHERE 
                                 t.type = 'U' -- U = User table
                             GROUP BY 
                                 t.name, s.name, t.create_date, t.modify_date, p.rows
                             ORDER BY 
                                 t.name;
                             """;

        var result = (await dbConnection.QueryAsync<TableInfoDto>(query)).ToList();

        return result;
    }
}