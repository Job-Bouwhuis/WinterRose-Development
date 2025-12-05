using WinterRose.ForgeCodex;
using WinterRose.ForgeCodex.AutoKeys;

internal static class ProgramHelpers
{
    public static void databaseAPITests(CodexDatabase database)
    {
        Table playerTable = database.CreateTable("players");
        playerTable.AddColumn("Id", typeof(Auto<int>), new ColumnMetadata { IsPrimaryKey = true });
        playerTable.AddColumn("Name", typeof(string));
        playerTable.AddColumn("Level", typeof(int));
        playerTable.AddColumn("Active", typeof(bool));

        // Add some rows
        playerTable.AddRow(new Dictionary<string, object?>
        {
            { "Name", "Alice" },
            { "Level", 5 },
            { "Active", true }
        });

        playerTable.AddRow(new Dictionary<string, object?>
        {
            { "Name", "Bob" },
            { "Level", 10 },
            { "Active", false }
        });

        playerTable.AddRow(new Dictionary<string, object?>
        {
            { "Name", "Charlie" },
            { "Level", 5 },
            { "Active", true }
        });

        Console.WriteLine("All rows:");
        for (int i = 0; i < playerTable.RowCount; i++)
        {
            var row = playerTable.GetRow(i);
            Console.WriteLine($"Id: {row["Id"]}, Name: {row["Name"]}, Level: {row["Level"]}, Active: {row["Active"]}");
        }

        // Query by primary key
        var pkRow = playerTable.GetRowByPrimaryKey("Id", 2);
        Console.WriteLine("\nLookup by primary key Id=2:");
        if (pkRow != null)
            Console.WriteLine($"Id: {pkRow["Id"]}, Name: {pkRow["Name"]}, Level: {pkRow["Level"]}, Active: {pkRow["Active"]}");

        // Query by column value
        var level5Rows = playerTable.QueryRows("Level", 5);
        Console.WriteLine("\nRows with Level == 5:");
        foreach (var rowIndex in level5Rows)
        {
            var row = playerTable.GetRow(rowIndex);
            Console.WriteLine($"Id: {row["Id"]}, Name: {row["Name"]}, Level: {row["Level"]}, Active: {row["Active"]}");
        }

        // Test adding duplicate primary key (should throw)
        try
        {
            playerTable.AddRow(new Dictionary<string, object?>
            {
                { "Id", 1 },
                { "Name", "Duplicate" },
                { "Level", 1 },
                { "Active", true }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nExpected error on duplicate PK: {ex.Message}");
        }

        // Test normal column query
        var activeRows = playerTable.QueryRows("Active", true);
        Console.WriteLine("\nRows with Active == true:");
        foreach (var rowIndex in activeRows)
        {
            var row = playerTable.GetRow(rowIndex);
            Console.WriteLine($"Id: {row["Id"]}, Name: {row["Name"]}, Level: {row["Level"]}, Active: {row["Active"]}");
        }

        database.Save();
    }
}