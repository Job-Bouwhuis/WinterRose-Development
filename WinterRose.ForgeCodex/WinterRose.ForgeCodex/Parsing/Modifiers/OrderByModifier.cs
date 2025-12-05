using System;
using System.Collections.Generic;
using System.Text;

namespace WinterRose.ForgeCodex.Parsing.Modifiers;

public class OrderByModifier : Modifier
{
    public string FieldName { get; set; }
    public bool Descending { get; set; }

    public OrderByModifier(string fieldName, bool descending)
    {
        FieldName = fieldName;
        Descending = descending;
    }

}

public class LimitModifier : Modifier
{
    public int Amount { get; set; }

    public LimitModifier(int amount) => Amount = amount;
}
