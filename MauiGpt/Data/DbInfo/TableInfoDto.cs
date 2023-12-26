namespace MauiGpt.Data.DbInfo;

public class TableInfoDto
{
    public string TableName { get; set; }
    public string SchemaName { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime LastModifiedDate { get; set; }
    public long RowCounts { get; set; }
    public long TotalSpaceKB { get; set; }
    public long UsedSpaceKB { get; set; }
    public long UnusedSpaceKB { get; set; }
}