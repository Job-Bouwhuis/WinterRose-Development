using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using WinterRose.WinterThornScripting.Interpreting;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WinterRose.WinterThornScripting
{
    /// <summary>
    /// A class defined in a WinterThorn script.
    /// </summary>
    [DebuggerDisplay("Class: {Name}")]
    public class Class
    {
        /// <summary>
        /// The name of the class
        /// </summary>
        [IncludeWithSerialization]
        public string Name { get; private set; }
        /// <summary>
        /// The description of the class
        /// </summary>
        [IncludeWithSerialization]
        public string Description { get; private set; }
        /// <summary>
        /// The constructors the class has defined
        /// </summary>
        [IncludeWithSerialization]
        public List<Constructor> Constructors { get; private set; }
        /// <summary>
        /// The classes namespace
        /// </summary>
        public Namespace Namespace { get; internal set; }
        /// <summary>
        /// The block of the class, here its variables, functions, ect. are defined.
        /// </summary>
        [IncludeWithSerialization]
        public Block Block { get; private set; }
        /// <summary>
        /// Whether or not this class is a reference to a C# class.
        /// </summary>
        public bool IsCSharpClass => CSharpClass != null;
        /// <summary>
        /// The C# class this class is a reference to. Null if it is not a reference to a C# class.
        /// </summary>
        [IncludeWithSerialization]
        public CSharpClass? CSharpClass { get; set; } = null;

        /// <summary>
        /// Creates a new class with the specified name, description, constructors and block.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="constructors"></param>
        /// <param name="block"></param>
        public Class(string name, string description, IEnumerable<Constructor> constructors, Block block) : this(name, description)
        {
            Name = name;
            Description = description;
            Constructors = [.. constructors];
            Constructors.Foreach(c => c.DelcaredClass = this);
            Block = block;

            Block.Functions.Foreach(f => f.DelcaredClass = this);
            block.Variables.Foreach(v => v.DeclaredClass = this);
        }

        /// <summary>
        /// Creates a new class with the specified name and description.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        [DefaultArguments("", "")]
        public Class(string name, string description)
        {
            Name = name;
            Description = description;
            Constructors = [];
            Block = new Block(null);
        }

        /// <summary>
        /// Declares a constructor for this class.
        /// </summary>
        /// <param name="constructor"></param>
        public void DeclareConstructor(Constructor constructor)
        {
            constructor.DelcaredClass = this;
            constructor.Body.Parent = Block;
            Constructors.Add(constructor);
        }
        /// <summary>
        /// Declares a function for this class.
        /// </summary>
        /// <param name="function"></param>
        public void DeclareFunction(Function function)
        {
            function.DelcaredClass = this;
            function.Body.Parent = Block;
            Block.DeclareFunction(function);
        }

        /// <summary>
        /// Declares a variable for this class.
        /// </summary>
        /// <param name="variable"></param>
        public void DeclareVariable(Variable variable)
        {
            variable.DeclaredClass = this;
            Block.DeclareVariable(variable);
        }

        /// <summary>
        /// Creates an instance of this class.
        /// </summary>
        /// <returns></returns>
        public Class? CreateInstance(Variable[] args)
        {
            if (IsCSharpClass)
            {
                Class c = CSharpClass!.GetClass();
                c.CSharpClass.Constructor(args);

                c.Namespace = Namespace;
                return c;
            }

            Class result = CreateInstanceNoConstructor();

            Function? constructor = result.Constructors.FirstOrDefault(x =>
            {
                if (args.Length == 0 && x.Parameters == null)
                    return true;
                else if (x.Parameters.Length == args.Length)
                    return true;

                return false;
            });

            if (constructor == null)
            {
                if (args.Length != 0)
                {
                    throw new WinterThornExecutionError(ThornError.InvalidParameters, "WR-0020", $"Constructor does not take {args.Length} arguments.");
                }
            }
            else
            {
                constructor.Invoke(args);
            }


            result.Namespace = Namespace;
            return result;
        }

        public Class? CreateInstanceNoConstructor()
        {
            if (IsCSharpClass)
            {
                Class c = CSharpClass!.GetClass();
                c.Namespace = Namespace;
                return c;
            }

            Block newBlock = Block.CreateCopy();

            List<Constructor> newConstructors = [];
            foreach(Constructor oldCtor in Constructors)
            {
                newConstructors.Add(oldCtor.CreateCopy(newBlock));
            }

            Class result = new Class(Name, Description, newConstructors, newBlock);

            result.Namespace = Namespace;
            return result;
        }
    }
}