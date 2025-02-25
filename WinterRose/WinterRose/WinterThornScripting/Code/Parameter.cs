using System;

namespace WinterRose.WinterThornScripting
{
    /// <summary>
    /// A function parameter.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <param name="class">The type this parameter should be</param>
    public class Parameter(string name, string description, Class @class)
    {
        public string Name => name;
        public string Description => description;
        public Class Class => @class;

        public Parameter(string name, string description, string typeName) : this(name, description, new Class(typeName, "")) { }
    }
}