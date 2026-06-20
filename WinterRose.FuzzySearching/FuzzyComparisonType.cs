namespace WinterRose.FuzzySearching
{
    /// <summary>
    /// The searchType of comparison to use when searching for a string.
    /// </summary>
    [Flags]
    public enum FuzzyComparisonType
    {
        /// <summary>
        /// Default comparison searchType.
        /// </summary>
        None,
        /// <summary>
        /// The search will ignore the case of the characters.
        /// </summary>
        IgnoreCase,
        /// <summary>
        /// The search will trim the strings before comparing them.
        /// </summary>
        Trim
    }
}


