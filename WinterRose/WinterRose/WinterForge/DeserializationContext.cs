using System.Collections.Generic;

namespace WinterRose.WinterForge
{
    public class DeserializationContext
    {
        public Dictionary<int, object> ObjectTable { get; } = new();
        public Stack<object> ValueStack { get; } = new();
        public List<DeferredObject> DeferredObjects { get; } = new();

        public void AddObject(int id, object instance)
        {
            ObjectTable[id] = instance;
        }

        public object? GetObject(int id)
        {
            ObjectTable.TryGetValue(id, out var obj);
            return obj;
        }
    }

}
