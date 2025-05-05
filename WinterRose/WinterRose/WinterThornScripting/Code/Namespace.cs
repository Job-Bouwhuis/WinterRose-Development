using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using WinterRose.WinterThornScripting.Generation;
using WinterRose;
using System.Linq;

namespace WinterRose.WinterThornScripting
{
    /// <summary>
    /// A namespace in WinterScript. Defines a collection of types.
    /// </summary>
    [IncludePrivateFields]
    public class Namespace
    {
        /// <summary>
        /// Classes defined in this namespace.
        /// </summary>
        public Class[] Classes => classes.ToArray();

        private List<Class> classes;
        /// <summary>
        /// The name of this namespace
        /// </summary>
        [IncludeWithSerialization]
        public string Name { get; private set; }
        /// <summary>
        /// The description of this namespace
        /// </summary>
        [IncludeWithSerialization]
        public string Description { get; private set; }

        /// <summary>
        /// Creates a new namespace with the specified name, description and types.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="types"></param>
        public Namespace(string name, IEnumerable<Class> types)
        {
            Name = name;
            Description = "";
            classes = types.ToList();
            Classes.Foreach(x => x.Namespace = this);
        }

        private Namespace() { } // exists for serialization

        /// <summary>
        /// Gets the class with the specified name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>The found class, or <see langword="null"/> if none were found</returns>
        public Class? GetClass(string name)
        {
            foreach (var type in Classes)
            {
                if (type.Name == name)
                {
                    return type;
                }
            }

            return null;
        }

        internal void Merge(Namespace @namespace)
        {
            foreach (var c in @namespace.Classes)
            {
                Class existing = GetClass(c.Name);
                if (existing is not null)
                {
                    if (OperatingSystem.IsWindows())
                    {
                        var result = Windows.MessageBox($"Ambiguous Definition for type {c.Name} in namespace {Name}.\n" +
                            $"Do you want to add it anyway?\n\n" +
                            $"Click Cancel to stop the adding of the namespace merging process.", "Alert", Windows.MessageBoxButtons.YesNoCancel);
                        if (result == Windows.DialogResult.No)
                            continue;
                        else if (result == Windows.DialogResult.Yes)
                            classes.Add(c);
                    }
                    throw new WinterThornCompilationError(ThornError.AmbiguousDefinition, "WT-0010", $"Class {c.Name} already exists in namespace {Name}");
                }
                else
                {
                    classes.Add(c);
                }
            }
        }
    }
}