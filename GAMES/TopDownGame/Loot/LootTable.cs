using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WinterRose;

namespace TopDownGame.Loot
{

    /// <summary>
    /// Factory clas for <see cref="LootTable{T}"/>
    /// </summary>
    public static class LootTable
    {
        public static LootTable<T> WithName<T>(string name) where T : class
        {
            return LootTable<T>.WithName(name);
        }

        /// <summary>
        /// Transforms the type of the loottable item from <typeparamref name="TSource"/> into 
        /// <typeparamref name="TTarget"/> using <see cref="Unsafe.As{T}(object?)"/> 
        /// with some extra safeguards in place so this wont fail in runtime
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static LootTable<TTarget> Cast<TSource, TTarget>(this LootTable<TSource> source)
            where TSource : class, TTarget where TTarget : class
        {
            return Unsafe.As<LootTable<TTarget>>(source);
        }
    }
}
