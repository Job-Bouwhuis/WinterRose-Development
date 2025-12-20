namespace WinterRose.WinterThornScripting
{
    /// <summary>
    /// The version of a WinterThorn script.
    /// </summary>
    public class Version
    {
        /// <summary>
        /// The major version of the script.
        /// </summary>
        [WFInclude]
        public int Major { get; set; }
        /// <summary>
        /// The minor version of the script.
        /// </summary>
        [WFInclude]
        public int Minor { get; set; }
        /// <summary>
        /// The patch version of the script.
        /// </summary>
        [WFInclude]
        public int Patch { get; set; }

        /// <summary>
        /// The versions of its compatible versions. Can be used to allow backwards compatibility between your scripts.
        /// </summary>
        [WFInclude]
        public Version[] CompatibleWith { get; set; } = new Version[0];

        [DefaultArguments(0, 0, 0)]
        public Version(int major, int minor, int patch)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
        }
    }
}