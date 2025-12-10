using WinterRose.AnonymousTypes;
using WinterRose.ForgeCodex;
using WinterRose.ForgeCodex.AutoKeys;
using WinterRose.ForgeCodex.Parsing.Ast;
using WinterRose.ForgeCodex.Storage.DefaultStorages;
using WinterRose.ForgeCodex.Storage.Serialization;
using WinterRose.ForgeGuardChecks;

/*
 
     Batch operations with [] syntax for multiple adds
     Upsert/add-or-update syntax
     Conditional field updates with if/then/else
     Wildcard * in take with except clause
     Order by and limit for pagination
     Change from to for for contextual operations (add/update/remove)
     Rename auto_increment to Auto<T>

 */

internal class Program
{
    private static void Main(string[] args)
    {
        CodexDatabase database = new CodexDatabase(new CodexFileStorage("Codex", new CodexWFSerializer()));
        //ProgramHelpers.databaseAPITests(database);

        //create(database);
        addRow(database);
        getRow(database);
        //deleteRow(database);
        //dropTable(database);
       
    }

    private static void dropTable(CodexDatabase database)
    {
        string test = """
                      drop table players
                      """;

        database.Evaluate(test);
    }

    private static void deleteRow(CodexDatabase database)
    {
        string test = """
                      remove from players where Name == "snow"
                      """;

        database.Evaluate(test);
    }

    private static void getRow(CodexDatabase database)
    {
        string test = """
                      from players take {
                        *
                      }
                      """;

        var rows = (IReadOnlyList<Anonymous>)database.Evaluate(test);
        foreach(var row in rows)
            Console.WriteLine(row.ToString());
    }

    private static void addRow(CodexDatabase database)
    {
        string test = """
                      add to players {
                          Name: "snow",
                          Level: 1,
                          Active: true
                      }
                      """;

        database.Evaluate(test);
        database.Save();
    }

    private static void create(CodexDatabase database)
    {
        string test = """
                      create table players {
                        Id: Auto<int> pk,
                        Name: string,
                        Level: int,
                        Active: bool
                      }
                      """;

        database.Evaluate(test);
        database.Save();
    }
}
