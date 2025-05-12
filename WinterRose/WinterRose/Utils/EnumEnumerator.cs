using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose
{
    public static class EnumEnumeratorHelpers
    {
        /// <summary>
        /// Gets the enumerator for the given enum, allowing you to iterate over the flags of this enum variable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="e"></param>
        /// <returns></returns>
        public static IEnumerator<T> GetEnumerator<T>(this T e) where T : Enum
        {
            return new EnumEnumerator<T>(e);
        }
    }

    internal class EnumEnumerator<T> : IEnumerator<T> where T : Enum
    {
        public T Current => flagList[currentIndex];
        object IEnumerator.Current => flagList[currentIndex];

        List<T> flagList = new List<T>();
        int currentIndex = -1;

        private T e;

        public EnumEnumerator(T e)
        {
            this.e = e;
            foreach (T value in Enum.GetValues(typeof(T)))
                if (e.HasFlag(value))
                    flagList.Add(value);
        }

        public void Dispose()
        {
            flagList.Clear();
        }

        public bool MoveNext()
        {
            currentIndex++;
            return currentIndex < flagList.Count;
        }

        public void Reset()
        {
            currentIndex = -1;
        }
    }
}
