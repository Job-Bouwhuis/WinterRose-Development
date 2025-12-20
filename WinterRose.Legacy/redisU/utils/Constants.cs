namespace WinterRose.WIP.Redis
{
    public enum RedisAnswerStatusCode
    {
        /// <summary>
        /// An undefined answer statuscode
        /// </summary>
        Undefined = -1,
        /// <summary>
        /// This is returned from the database when the requested operation completed successfully
        /// </summary>
        OK,
        /// <summary>
        /// This is returned from the database when the requested operation failed to complete
        /// </summary>
        Faulted,
        /// <summary>
        /// Gets returned when <see cref="RedisConnection.Ping"></see> is called and a connection is established
        /// </summary>
        Pong
    };
}

namespace WinterRose.WIP.Redis.Utils
{
    public enum Stream
    {
        Synchronous = 0, //Default value
        Asynchronous = 1
    };

    public enum KeyType
    {
        String,
        List,
        Set,
        Zset,
        Hash,
        Undefined
    };

    public enum ListInsert
    {
        Before = 0,
        After
    };

    public enum SSParam
    {
        Weight = 0,
        Aggregate,
        WithScores,
        Limits
    };

    public enum SSResultScore
    {
        Sum = 0,
        Min,
        Max
    };

    public class Constants
    {
        public const string DEFAULT_REDIS_HOST = "localhost";
        public const int DEFAULT_REDIS_PORT = 6379;

        //Socket/NetworkStream timeout settings
        public const int SOCKET_SEND_TIMEOUT_MS = 1000;
        public const int SOCKET_RECEIVE_TIMEOUT_MS = 1000;
        public const int NS_READ_TIMEOUT_MS = 30000;
        public const int NS_WRITE_TIMEOUT_MS = 30000;

        //Data constants
        public const string NO_OP = "0";

        //Redis command format options
        public const string CRLF = "\r\n";
        public const string NUM_ARGUMENTS = "*";
        public const string NUM_BYTES_ARGUMENT = "$";
        public const string REPLY_SINGLE_LINE = "+";
        public const string REPLY_ERROR = "-";
        public const string REPLY_INTEGER = ":";
        public const string REPLY_BULK = "$";
        public const string REPLY_MULTIBULK = "*";

        //Error Messages
        public const string ERR_MSG_EXECUTE_CMD = "Regular command not allowed when in subscribed mode.";
        public const string ERR_MSG_RECREATE_SUBSCRIPTION = "The previous subscription session was ended. Recreate the subscription to start listening for messages.";
    }
}

