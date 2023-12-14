namespace MauiGpt.Data.DbInfo;

public class SqlColumnInformation
{
    public string COLUMN_NAME { get; set; }
    public string DATA_TYPE { get; set; }
    public int? NUMERIC_PRECISION { get; set; }
    public int? CHARACTER_MAXIMUM_LENGTH { get; set; }
    public string IS_NULLABLE { get; set; }
    public string IS_PRIMARY_KEY { get; set; }
}