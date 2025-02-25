using System;

namespace WinterRose.WinterThornScripting
{
    [Flags]
    public enum ThornError
    {
        /// <summary>
        /// Syntax fault
        /// </summary>
        SyntaxError,
        /// <summary>
        /// Error in the base interpreting
        /// </summary>
        InterpreterError,
        /// <summary>
        /// A value was null
        /// </summary>
        NullReference,
        /// <summary>
        /// The index was out of range
        /// </summary>
        IndexOutOfRange,
        /// <summary>
        /// Invalid type passed to the nethod
        /// </summary>
        InvalidType,
        /// <summary>
        /// More than 1 reference with the given name are present in the current context
        /// </summary>
        AmbiguousDefinition,
        /// <summary>
        /// The expression is a problem
        /// </summary>
        ExpressionError,
        /// <summary>
        /// An error occured while concatenating the string
        /// </summary>
        StringConcatFault,
        InvalidParameters,
    }
}