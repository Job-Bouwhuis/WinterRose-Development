using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace WinterRose
{
    /// <summary>
    /// A First in First Out (FIFO) stack.
    /// </summary>
    [SerializeAs<FIFOStack>, IncludePrivateFields]
    public sealed class FIFODictionaryStack : IEnumerable<object>
    {
        private readonly List<object> _items = new List<object>();

        /// <summary>
        /// Creates a new First in First out (FIFO) Stack with default stack entries in the same order as the given default collection
        /// </summary>
        /// <param name="initialValues"></param>
        /// <exception cref="InvalidArgumentException"></exception>
        public FIFODictionaryStack(IEnumerable initialValues)
        {
            throw new NotImplementedException();
            if (initialValues is IDictionary)
                throw new ArgumentException("The initial items were of type Dictionary. which is not valid for this stack.");

            _items = initialValues.Cast<object>().ToList();
        }

        /// <summary>
        /// Creates a new empty First in First out (FIFO) Stack
        /// </summary>
        public FIFODictionaryStack() { throw new NotImplementedException(); }

        /// <summary>
        /// Pushes an item on the bottom of the stack.
        /// </summary>
        /// <param name="item"></param>
        public void Push(object item)
        {
            _items.Add(item);
        }

        /// <summary>
        /// Removes and returns the object at the beginning of the stack.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public object Pop()
        {
            if (_items.Count == 0)
                throw new InvalidOperationException("Stack is empty");

            var item = _items[0];
            _items.RemoveAt(0);
            return item;
        }

        /// <summary>
        /// Returns the first item on the stack without removing it
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public object Peek()
        {
            if (_items.Count == 0)
                throw new InvalidOperationException("Stack is empty");

            return _items[0];
        }

        /// <summary>
        /// The number of items on the stack
        /// </summary>
        public int Count
        {
            get { return _items.Count; }
        }

        /// <summary>
        /// Gets the stack as a list. does not allocate extra memory
        /// </summary>
        /// <returns></returns>
        public ReadOnlyCollection<object> AsList() => _items.AsReadOnly();


        /// <summary>
        /// Gets the enumerator of the stack
        /// </summary>
        /// <returns></returns>
        public IEnumerator<object> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }
    }


}

