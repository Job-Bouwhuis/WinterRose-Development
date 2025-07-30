using ForgeMantle.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose;

namespace ForgeMantle.Models;
public class ConfigSnapshot
{
    [IncludeWithSerialization]
    public Dictionary<string, IConfigValue> State { get; init; } = new();
}

