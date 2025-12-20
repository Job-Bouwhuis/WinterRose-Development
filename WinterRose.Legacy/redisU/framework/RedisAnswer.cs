namespace WinterRose.WIP.Redis
{
    /// <summary>
    /// Provides an easy way of telling the result of the Redis response
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct RedisAnswer<T>
    {
        /// <summary>
        /// The value that was returned by the Redis server
        /// </summary>
        public T Value { get; private set; }

        /// <summary>
        /// Indicates if this answer has a valid value
        /// </summary>
        public bool HasValue { get; private set; }

        public RedisAnswer(T Value)
        {
            this.Value = Value;
            HasValue = Value is not null;
        }
        internal RedisAnswer(T value, bool IsValid)
        {
            Value = value;
            HasValue = IsValid;
        }
        /// <summary>
        /// Implicidly converts the RedisAnswer to its value
        /// </summary>
        /// <param name="answer"></param>
        public static implicit operator T(RedisAnswer<T> answer)
        {
            return answer.Value;
        }

        public static implicit operator RedisAnswer<T>(T value)
        {
            return new RedisAnswer<T>(value);
        }
    }
}


