using System;

namespace WinterRose
{
    /// <summary>
    /// this attribute may provide default constructor arguments. Only works if any of the following methods are used:<br></br><br></br>
    /// 
    /// <see cref="ActivatorExtra.CreateInstance(Type)"/><br></br>
    /// <see cref="ActivatorExtra.CreateInstance(Type, int)"/><br></br>
    /// <see cref="ActivatorExtra.CreateInstance{T}"/><br></br>
    /// <see cref="ActivatorExtra.CreateInstance{T}(int)"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor, Inherited = false, AllowMultiple = false)]
    public sealed class DefaultArgumentsAttribute : Attribute
    {
        private object?[] arguments;

        // This is a positional argument
        public DefaultArgumentsAttribute(params object?[] args)
        {
            arguments = args;
        }

        public object?[] Arguments
        {
            get { return arguments; }
        }
    }
}


