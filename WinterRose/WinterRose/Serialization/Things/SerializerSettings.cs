using System;

namespace WinterRose.Serialization
{
    #region Settings Class

    /// <summary>
    /// Settings for the serializer. these settings are only used for operations on a collection of classes. they are not used for operations on single classes.
    /// </summary>
    public class SerializerSettings
    {
        /// <summary>
        /// the action that is called wheneve the serializer handled items. this number can be set at <see cref="ReportEvery"/> -- <b>NOTE:</b> this only happens on serializing a collection of objects
        /// </summary>
        public Action<ProgressReporter> ProgressReporter { get; set; } = delegate { };
        /// <summary>
        /// how often <see cref="ProgressReporter"/> should be invoked when serializing or deserialzing
        /// </summary>
        public int ReportEvery { get; set; } = 5000;

        /// <summary>
        /// States that the serialzier should include the type of the object in the serialzied string<br></br>
        /// This is used to deserialize the object if it uses inheritance and the serializer is requested to deserialzie to a base class
        /// </summary>
        public bool IncludeType { get; set; } = false;
        /// <summary>
        /// States that the serializer should ignore when a field is not found in the class. This is useful when you want to deserialize an older version of a class that has fields that are no longer used<br></br>
        /// Note that the data for the field will not be deserialized and will be lost
        /// </summary>
        public bool IgnoreNotFoundFields { get; set; } = false;

        /// <summary>
        /// Tells the serializer to stress the CPU as much as it possibly can. This can hurt the speed of the operation on lower end machines
        /// </summary>
        public bool LudicrusMode { get; set; } = false;

        internal bool includePropertiesForField = false;
        internal bool includePrivateFieldsForField = false;

        /// <summary>
        /// When the serializer encounters an object of type <see cref="object"/> it will assume that the object is an anonymous object and will serialize/deserialize it as such
        /// </summary>
        public bool AssumeObjectIsAnonymous { get; set; } = true;

        /// <summary>
        /// The number of how many threads the serializer may use to serialize or deserialize items in a list at once. 1 item per thread will be serialized or deserialized at a time.
        /// </summary>
        public int TheadsToUse { get; set;  }
        /// <summary>
        /// whether circle references are enabled, eg if object a has a field for object b, and object b has a reference to object a
        /// </summary>
        public bool CircleReferencesEnabled { get; set; } = false;

        /// <summary>
        /// Creates a new instance of <see cref="SerializerSettings"/> with the default settings. Automatically detects the number of cores on the machine and sets the number of threads to use to that number.
        /// </summary>
        public SerializerSettings() => TheadsToUse = Environment.ProcessorCount;

        public static SerializerSettings CreateFrom(SerializerSettings settings, Action<SerializerSettings>? fineTuneSettings = null)
        {
            var newSettings = new SerializerSettings()
            {
                IncludeType = settings.IncludeType,
                IgnoreNotFoundFields = settings.IgnoreNotFoundFields,
                LudicrusMode = settings.LudicrusMode,
                ReportEvery = settings.ReportEvery,
                ProgressReporter = settings.ProgressReporter,
                AssumeObjectIsAnonymous = settings.AssumeObjectIsAnonymous,
                CircleReferencesEnabled = settings.CircleReferencesEnabled
            };

            if (fineTuneSettings is not null)
                fineTuneSettings(newSettings);

            return newSettings;
        }
    }

    #endregion
}