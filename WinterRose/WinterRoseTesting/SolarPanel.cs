using WinterRose.Vectors;

namespace WinterRose
{
    public class GameObject
    {
        public string name = "";
    }

    [System.Diagnostics.DebuggerDisplay("Panel: {Name} -- Brand: {Brand}")]
    public class SolarPanel
    {
        [IncludeWithSerialization]
        public string ModelName
        {
            get
            {
                if (ModelPrefab == null)
                    return "";
                return ModelPrefab.name;
            }
            set
            {
                ModelPrefab ??= new();
                ModelPrefab.name = value;
            }
        }

        [IncludeWithSerialization] public string Name { get; set; } = "New Panel";
        [IncludeWithSerialization] public string Brand { get; set; } = "";

        [IncludeWithSerialization] public float WattPerHour { get; set; } = 0;
        [IncludeWithSerialization] public float CostPerHour { get; set; } = 0;
        [IncludeWithSerialization] public float ProfitPerHour { get; set; } = 0;
        [IncludeWithSerialization] public Vector3 Size { get; set; } = Vector3.Zero;

        public GameObject? ModelPrefab { get; set; }

        public string Source => source;
        private string source = "";

        public void SetSource(string newSource) => source = newSource;

        public override string ToString() => $"Panel: {Name} -- Brand: {Brand}";
    }
}
