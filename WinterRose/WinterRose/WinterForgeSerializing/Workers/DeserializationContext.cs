using System.Collections.Generic;

namespace WinterRose.WinterForgeSerializing.Workers
{
    internal class DeserializationContext : IClearDisposable
    {
        public Dictionary<int, object> ObjectTable { get; } = new();
        public Stack<object> ValueStack { get; } = new();
        public List<DeferredObject> DeferredObjects { get; } = new();

        public bool IsDisposed { get; private set; }

        public void AddObject(int id, ref object instance)
        {
            ObjectTable.Add(id, instance);
        }

        public void Dispose()
        {
            ObjectTable.Clear();
            ValueStack.Clear();
            DeferredObjects.Clear();
        }

        public object? GetObject(int id)
        {
            ObjectTable.TryGetValue(id, out var obj);
            return obj;
        }
    }

}
