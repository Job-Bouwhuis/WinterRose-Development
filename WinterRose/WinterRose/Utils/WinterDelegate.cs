using System;
using System.Collections.Generic;
using System.Text;

namespace WinterRose
{
    public class WinterDelegate
    {
        private Delegate method;
        private object[] args;
        private Type returnType;

        /// <summary>
        /// Creates a new <see cref="WinterDelegate"/> with the given arguments
        /// </summary>
        /// <param name="method"></param>
        /// <param name="args"></param>
        public WinterDelegate(Delegate method, params object[] args)
        {
            this.method = method;
            this.args = args;

            returnType = method.Method.ReturnType;
        }

        /// <summary>
        /// Invokes the delegate
        /// </summary>
        /// <returns></returns>
        public object Invoke() => method.DynamicInvoke(args);
        /// <summary>
        /// Creates a new <see cref="WinterDelegate"/> with the given arguments
        /// </summary>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <returns>The newly created <see cref="WinterDelegate"/></returns>
        public static WinterDelegate Create(Delegate method, params object[] args) => new WinterDelegate(method, args);

        /// <summary>
        /// Creates a new <see cref="WinterDelegate"/> with the given arguments
        /// </summary>
        /// <returns>The newly created <see cref="WinterDelegate"/></returns>
        public static WinterDelegate Create(Action action) => new WinterDelegate(action, null);

        /// <summary>
        /// Creates a new <see cref="WinterDelegate"/> with the given arguments
        /// </summary>
        /// <returns>The newly created <see cref="WinterDelegate"/></returns>
        public static implicit operator WinterDelegate(Delegate method) => new WinterDelegate(method, null);
        public static WinterDelegate operator +(WinterDelegate a, WinterDelegate b)
        {
            if (!ValidateDelegate(a, b.method))
                throw new ArgumentException("Delegate return type does not match");
            Delegate d = Delegate.Combine(a.method, b.method);
            return new WinterDelegate(d, a.args);
        }
        public static WinterDelegate operator +(WinterDelegate a, Delegate b)
        {
            if (!ValidateDelegate(a, b))
                throw new ArgumentException("Delegate return type does not match");
            Delegate d = Delegate.Combine(a.method, b);
            return new WinterDelegate(d, a.args);
        }

        private static bool ValidateDelegate(WinterDelegate winterDelegate, Delegate method)
        {
            bool valid = winterDelegate.returnType == method.Method.ReturnType;
            if (!valid)
                return false;

            valid = winterDelegate.method.Method.GetParameters().Length == method.Method.GetParameters().Length;
            if (!valid)
                return false;

            for (int i = 0; i < winterDelegate.method.Method.GetParameters().Length; i++)
            {
                valid = winterDelegate.method.Method.GetParameters()[i].ParameterType == method.Method.GetParameters()[i].ParameterType;
                if (!valid)
                    return false;
            }
            return true;
        }


    }
}
