using CSharpSqliteORM.Structure;

namespace Logic.Database;

public class dbo_ScreenSettings : IDatabase_Table
{
    public static string tableName => "screen_settings";

    public required string screenName { get; set; }
    public int? screenOrder { get; set; }

    public static Database_Column[] getColumns => [
        new Database_Column() { columnName = nameof(screenName), columnType = Database_ColumnType.TEXT, allowNull = false },
        new Database_Column() { columnName = nameof(screenOrder), columnType = Database_ColumnType.INTEGER, allowNull = true },
    ];
}
