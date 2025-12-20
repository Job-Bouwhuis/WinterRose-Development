namespace WinterRose.WinterThornScripting
{
    /// <summary>
    /// A type of variable.
    /// </summary>
    public enum VariableType
    {
        /// <summary>
        /// A number.
        /// </summary>
        Number,
        /// <summary>
        /// A string.
        /// </summary>
        String,
        /// <summary>
        /// A boolean.
        /// </summary>
        Boolean,
        /// <summary>
        /// A function.
        /// </summary>
        Function,
        /// <summary>
        /// A class.
        /// </summary>
        Class,
        /// <summary>
        /// A null value.
        /// </summary>
        Null,
        /// <summary>
        /// A variable type that is not known.
        /// </summary>
        Unknown,
        /// <summary>
        /// The value is a C# delegate, meaning it is a variable that references a C# function to get its value.
        /// </summary>
        CSharpDelegate,
        /// <summary>
        /// The value is a break statement.
        /// </summary>
        Break,
        /// <summary>
        /// The value is a continue statement.
        /// </summary>
        Continue
    }
}