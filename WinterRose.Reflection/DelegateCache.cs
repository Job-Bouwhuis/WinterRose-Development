using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Reflection
{
    public static class DelegateCache
    {
        private static readonly Dictionary<MemberData, Func<object, object>> _getterCache = new();
        private static readonly Dictionary<MemberData, Action<object, object>> _setterCache = new();

        public static Func<object, object> GetGetter(MemberData member)
        {
            if (!_getterCache.TryGetValue(member, out var getter))
            {
                getter = MemberAccessFactory.CreateGetter(member);
                _getterCache[member] = getter;
            }
            return getter;
        }

        public static Action<object, object> GetSetter(MemberData member)
        {
            if (!_setterCache.TryGetValue(member, out var setter))
            {
                setter = MemberAccessFactory.CreateSetter(member);
                _setterCache[member] = setter;
            }
            return setter;
        }
    }
}
