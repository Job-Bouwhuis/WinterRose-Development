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
        [WFInclude]
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

        [WFInclude] public string Name { get; set; } = "New Panel";
        [WFInclude] public string Brand { get; set; } = "";

        [WFInclude] public float WattPerHour { get; set; } = 0;
        [WFInclude] public float CostPerHour { get; set; } = 0;
        [WFInclude] public float ProfitPerHour { get; set; } = 0;
        [WFInclude] public Vector3 Size { get; set; } = Vector3.Zero;

        public GameObject? ModelPrefab { get; set; }

        public string Source => source;
        private string source = "";

        public void SetSource(string newSource) => source = newSource;

        public override string ToString() => $"Panel: {Name} -- Brand: {Brand}";
    }
}
