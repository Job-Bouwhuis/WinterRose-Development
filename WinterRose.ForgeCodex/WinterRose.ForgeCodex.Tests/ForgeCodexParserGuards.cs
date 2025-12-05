using WinterRose.ForgeCodex;
using WinterRose.ForgeCodex.Parsing.Ast;
using WinterRose.ForgeGuardChecks;

[GuardClass("ForgeCodexParser")]
public class ForgeCodexParserGuards
{
    [Guard]
    public void Parse_SelectWithRelation()
    {
        var input = """
            from Player where level > 10
            take {
                name,
                inventory {
                    id,
                    value
                }
            }
            """;

        var root = Codex.Parse(input);

        Forge.Expect(root).Not.Null();
        Forge.Expect(root.From).OfType<QueryFrom>();
    }

    [Guard]
    public void Parse_CreateTable()
    {
        var input = """
            create table Inventory {
                id: Auto<int>,
                name: string,
                quantity: int
            }
            """;

        var root = Codex.Parse(input);

        Forge.Expect(root).Not.Null();
        Forge.Expect(root.From).OfType<CreateTableStatement>();
    }

    [Guard]
    public void Parse_DropTable()
    {
        var input = """
            drop table Inventory
            """;

        var root = Codex.Parse(input);

        Forge.Expect(root).Not.Null();
        Forge.Expect(root.From).OfType<DropTableStatement>();
    }

    [Guard]
    public void Parse_AddRecord()
    {
        var input = """
            add to Player {
                name: "NewPlayer",
                level: 1,
                active: true
            }
            """;

        var root = Codex.Parse(input);

        Forge.Expect(root).Not.Null();
        Forge.Expect(root.From).OfType<AddStatement>();
    }

    [Guard]
    public void Parse_AddRelation()
    {
        var input = """
            from Player where id == 5
            add to inventory {
                name: "Stone",
                quantity: 10
            }
            """;

        var root = Codex.Parse(input);

        Forge.Expect(root).Not.Null();
        Forge.Expect(root.From).OfType<AddStatement>();
    }

    [Guard]
    public void Parse_UpdateRecord()
    {
        var input = """
            from Player where id == 5
            update {
                name: "SnowTheGreat",
                level: 2
            }
            """;

        var root = Codex.Parse(input);

        Forge.Expect(root).Not.Null();
        Forge.Expect(root.From).OfType<UpdateStatement>();
    }

    [Guard]
    public void Parse_BulkUpdate()
    {
        var input = """
            from Player where level < 10
            update {
                level: level + 1
            }
            """;

        var root = Codex.Parse(input);

        Forge.Expect(root).Not.Null();
        Forge.Expect(root.From).OfType<UpdateStatement>();
    }

    [Guard]
    public void Parse_RemoveRecords()
    {
        var input = """
            remove from Player where active == false
            """;

        var root = Codex.Parse(input);

        Forge.Expect(root).Not.Null();
        Forge.Expect(root.From).OfType<RemoveStatement>();
    }

    [Guard]
    public void Parse_RemoveRelation()
    {
        var input = """
            from Player where id == 5
            remove from inventory where name == "Stone"
            """;

        var root = Codex.Parse(input);

        Forge.Expect(root).Not.Null();
        Forge.Expect(root.From).OfType<RemoveStatement>();
    }

    [Guard]
    public void Parse_AddBatch()
    {
        var input = """
        add to Player [
            { name: "A", level: 1 },
            { name: "B", level: 2 },
            { name: "C", level: 3 }
        ]
        """;

        var root = Codex.Parse(input);

        Forge.Expect(root).Not.Null();
        Forge.Expect(root.From).OfType<AddBatchStatement>();
    }

    [Guard]
    public void Parse_UpsertRecord()
    {
        var input = """
        for Player where id == 5 update or add {
            name: "UpdatedSnow",
            level: 99
        }
        """;

        var root = Codex.Parse(input);

        Forge.Expect(root).Not.Null();
        Forge.Expect(root.From).OfType<UpdateStatement>();
        Forge.Expect(((UpdateStatement)root.From).ElseAdd).True();
    }

    public void Parse_ConditionalUpdate()
    {
        var input = """
        from Player where id == 5
        update {
            rank: if level > 50 then "Veteran" else "Novice"
        }
        """;

        var root = Codex.Parse(input);

        Forge.Expect(root).Not.Null();
        Forge.Expect(root.From).OfType<UpdateStatement>();
    }

    [Guard]
    public void Parse_TakeWildcardExcept()
    {
        var input = """
        from Player
        take {
            * except 
            {
                password,
                secretToken 
            }
        }
        """;

        var root = Codex.Parse(input);

        Forge.Expect(root).Not.Null();
        Forge.Expect(root.From).OfType<QueryFrom>();
    }

    [Guard]
    public void Parse_SelectWithOrderAndLimit()
    {
        var input = """
        from Player where level >= 5
        take { name, level }
        order by level descending
        limit 20
        """;

        var root = Codex.Parse(input);

        Forge.Expect(root).Not.Null();
        Forge.Expect(root.From).OfType<QueryFrom>();
    }

    [Guard]
    public void Parse_ContextualForUpdate()
    {
        var input = """
        for Player where id == 3
        update {
            name: "ContextualSnow"
        }
        """;

        var root = Codex.Parse(input);

        Forge.Expect(root).Not.Null();
        Forge.Expect(root.From).OfType<UpdateStatement>();
    }

    [Guard]
    public void Parse_AddRelationBatch()
    {
        var input = """
        from Player where id == 10
        add to inventory [
            { name: "Iron", quantity: 5 },
            { name: "Gold", quantity: 1 }
        ]
        """;

        var root = Codex.Parse(input);

        Forge.Expect(root).Not.Null();
        Forge.Expect(root.From).OfType<AddBatchStatement>();
    }
}
