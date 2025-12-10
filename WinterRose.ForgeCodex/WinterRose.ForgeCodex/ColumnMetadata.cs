using WinterRose.WinterForgeSerializing.Attributes;

namespace WinterRose.ForgeCodex;

// Represents per-column metadata
public sealed class ColumnMetadata
{
    [SkipWhen(""""
              #template SkipIf object actual 
              {
                  Test : t;
                  return t->Foo();

                  #container Test 
                  {
                    #variables 
                    {
                      IsPrimaryKey;
                    }

                    #template Foo 
                    {
                      return IsPrimaryKey;
                    }
                  }
              }
              """")]
    public bool IsPrimaryKey { get; set; }
    public bool IsUnique { get; set; }

    // Optional foreign key info
    public string? ForeignTable { get; set; }
    public string? ForeignColumn { get; set; }
}

