namespace WinterRose.ForgeThread
{
    /// <summary>
    /// Priority of a scheduled job.
    /// </summary>
    public enum JobPriority
    {
        /// <summary>High priority - executed before normal and low jobs.</summary>
        High,
        /// <summary>Normal priority.</summary>
        Normal,
        /// <summary>Low priority - executed after other jobs.</summary>
        Low
    }

}
