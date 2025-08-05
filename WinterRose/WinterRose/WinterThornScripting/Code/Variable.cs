using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using WinterRose.WinterThornScripting.Interpreting;

namespace WinterRose.WinterThornScripting
{
    /// <summary>
    /// A variable in WinterThorn.
    /// </summary>
    [DebuggerDisplay("Var: {Name} = {Value}")]
    public class Variable
    {
        /// <summary>
        /// The name of the variable. This is used to access the variable in WinterThorn.
        /// </summary>
        [WFInclude]
        public string Name { get; private set; }
        /// <summary>
        /// A description of the variable.
        /// </summary>
        [WFInclude]
        public string Description { get; private set; }
        /// <summary>
        /// The class in which the variable was declared. currently unused.
        /// </summary>
        public Class DeclaredClass { get; set; }
        private object? value;
        /// <summary>
        /// The value of the variable.
        /// <br></br> can be a bunch of things. some types have special meanings.
        /// </summary>
        public object? Value
        {
            get
            {
                if (Type is VariableType.CSharpDelegate)
                    return ((Delegate)value!).DynamicInvoke();
                return value;
            }
            set
            {
                if (ReadOnly)
                    throw new WinterThornExecutionError(ThornError.SyntaxError, "WT-00017", $"Cannot set the value of a read-only variable.");

                if (value is Variable var)
                    value = var.Value;


                if(Type is VariableType.CSharpDelegate)
                {
                    Setter.DynamicInvoke(value);
                    return;
                }

                this.value = value;
                Type = value switch
                {
                    string => VariableType.String,
                    double => VariableType.Number,
                    bool => VariableType.Boolean,
                    null => VariableType.Null,
                    Function => VariableType.Function,
                    Class or IEnumerable or IEnumerable<Variable> or CSharpClass => VariableType.Class,
                    Delegate => VariableType.CSharpDelegate,
                    WinterThornScripting.Break => VariableType.Break,
                    WinterThornScripting.Continue => VariableType.Continue,
                    _ => throw new WinterThornExecutionError(ThornError.SyntaxError, "WT-0005", $"Invalid type {value.GetType()} found in variable."),
                };
            }
        }
        /// <summary>
        /// When the Value is of type <see cref="VariableType.CSharpDelegate"/>, this is the setter for the variable. it must be set for the variable to work in this way.
        /// </summary>
        public Delegate? Setter { get; set; }

        public bool ReadOnly { get; set; }

        /// <summary>
        /// The type of the variable.
        /// </summary>
        [WFInclude]
        public VariableType Type { get; private set; }
        /// <summary>
        /// The access modifiers of the variable. currently unused.
        /// </summary>
        [WFInclude]
        public AccessControl AccessModifiers { get; private set; }

        /// <summary>
        /// A null value.
        /// </summary>
        public static Variable Null { get; } = new Variable("null", "A null value.", null, AccessControl.Private);
        /// <summary>
        /// A false boolean value.
        /// </summary>
        public static Variable False { get; } = new Variable("False", "A false boolean value", false, AccessControl.Private);
        /// <summary>
        /// A true boolean value.
        /// </summary>
        public static Variable True { get; } = new Variable("True", "A true boolean value", true, AccessControl.Private);
        /// <summary>
        /// A break statement.
        /// </summary>
        public static Variable Break { get; } = new Variable("break", "A break statement", new Break(), AccessControl.Private);
        /// <summary>
        /// A continue statement.
        /// </summary>
        public static Variable Continue { get; } = new Variable("Continue", "A continue statement", new Continue(), AccessControl.Private);

        /// <summary>
        /// Creates a new variable with the given name, description, value and access modifiers.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="value"></param>
        /// <param name="accessModifiers"></param>
        public Variable(string name, string description, object? value, AccessControl accessModifiers = AccessControl.Private)
            : this(name, description, accessModifiers)
        {
            if (value is Variable var)
                Value = var.Value;
            else
                Value = value;
        }

        /// <summary>
        /// Creates a new variable with the given name, description and access modifiers.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="accessModifiers"></param>
        /// <exception cref="WinterThornExecutionError"></exception>
        [DefaultArguments("", "", AccessControl.Public)]
        public Variable(string name, string description, AccessControl accessModifiers = AccessControl.Private)
        {
            Name = name;
            Description = description;
            AccessModifiers = accessModifiers;

            if (value is Delegate && Setter is null)
                throw new WinterThornExecutionError(ThornError.SyntaxError, "WT-0011", "A C# bound variable having the value as a Delegate must have the property Setter be set aswell.");
        }

        /// <summary>
        /// Creates a new variable with the given name, description, value and access modifiers.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Variable(string value) => new Variable("string", "A string value.", value);
        /// <summary>
        /// Creates a new variable with the given name, description, value and access modifiers.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Variable(double value) => new Variable("number", "An integer value.", value);
        /// <summary>
        /// Creates a new variable with the given name, description, value and access modifiers.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Variable(bool value) => new Variable("bool", "A boolean value.", value);

        /// <summary>
        /// Creates an exact copy of this variable.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        internal Variable CreateCopy(Block result)
        {
            if(Type is VariableType.Number or VariableType.String or VariableType.Boolean or VariableType.Null)
                return new(Name, Description, Value, AccessModifiers);
            if(Type is VariableType.Class)
            {
                Class c = (Class)Value!;
                return new(Name, Description, c.CreateInstanceNoConstructor(), AccessModifiers);
            }
            return new(Name, Description, AccessModifiers);
        }
    }
}