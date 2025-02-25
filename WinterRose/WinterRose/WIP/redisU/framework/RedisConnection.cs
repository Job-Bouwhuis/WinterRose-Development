using System;
using System.Collections;
using System.Threading;
using WinterRose.WIP.Redis.Utils;
using WinterRose.WIP.Redis.Framework;
using WinterRose.WIP.Redis;
using System.Collections.Generic;
using WinterRose.WIP.Redis.Exceptions;

namespace WinterRose.WIP.Redis
{
    /// <summary>
    /// A class object that provides methods to communicate with a Redis database
    /// </summary>
    public sealed class RedisConnection : RedisConnectionBase, IClearDisposable
    {
        ThreadAbortToken listeningThreadAbortToken = new();
        private Thread? dataListener = null;
        private bool closeCalled;

        public event Action<ProgressReporter>? ProgressReporter
        {
            add
            {
                base.ProgressReporter += value;
                redisStream.ProgressReporter += value;
            }
            remove
            {
                base.ProgressReporter -= value;
                redisStream.ProgressReporter -= value;
            }
        }

        /// <summary>
        /// Gets whether there is a valid connection to Redis
        /// </summary>
        public new bool IsConnected { get => base.IsConnected; }

        public bool IsDisposed { get; private set; }



        /// <summary>
        /// Creates an instance of this class. This constructor does not automatically connect to a Redis server.
        /// </summary>
        public RedisConnection() { }
        /// <summary>
        /// Creates an instance if the RedisConnection class, and tries to establish a connection with the given host and port
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public RedisConnection(string host, int port)
        {
            MakeConnection(host, port);
        }
        /// <summary>
        /// Creates an instance if the RedisConnection class, and tries to establish a connection with the given host and port. then if the connection established successfully, attempts to authenticate with the given password
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public RedisConnection(string host, int port, string password)
        {
            MakeConnection(host, port);
            if (IsConnected)
                Authenticate(password);
        }
        /// <summary>
        /// Attempts to make a connection to Redis with the given host and port
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns>A RedisAnswer containing a string. if the string is empty the connection was successful. otherwise the string will contain the error message</returns>
        public RedisAnswer<string> MakeConnection(string host, int port)
        {
            try
            {
                CreateConnection(host, port);
                StartDataListener();
                return new(null);
            }
            catch (Exception ex)
            {
                return new RedisAnswer<string>($"{ex.GetType()} -- {ex.Message}", false);
            }
        }
        /// <summary>
        /// Ends a connection to Redis if there is one
        /// </summary>
        public void EndConnection()
        {
            if (!IsConnected)
                return;

            Dispose();
        }
        /// <summary>
        /// Deletes the given keys from the database
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="keys"></param>
        /// <returns>The number of keys successfully deleted</returns>
        public RedisAnswer<int> DeleteKeys<K>(params K[] keys)
        {
            if (!IsConnected)
                return new(0, false);
            ValidateArguments(keys);
            string[] reply = ExecuteCommand(RedisCommand.DEL, ChangeTypeArrayToString(keys));
            return new(ConvertReplyToInt(reply));
        }
        /// <summary>
        /// Gets the keys that match the pattern. Use " <b>*</b> " to get all keys
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public RedisAnswer<string[]> GetKeysByPattern(string pattern)
        {
            if (!IsConnected)
                return new(Array.Empty<string>(), false);
            ValidateArguments(pattern);
            return new(ExecuteCommand(RedisCommand.KEYS, pattern));
        }
        /// <summary>
        /// Gets a random key from Redis
        /// </summary>
        /// <returns></returns>
        public RedisAnswer<string> GetRandomKey()
        {
            if (!IsConnected)
                return new("", false);
            string[] reply = ExecuteCommand(RedisCommand.RANDOMKEY);
            return new(ConvertReplyToString(reply));
        }
        /// <summary>
        /// Returns the timeout for the specified key in seconds
        /// </summary>
        public RedisAnswer<int> GetKeyTimeout<K>(K key)
        {
            if (!IsConnected)
                return new(0, false);
            ValidateArguments(key);
            string[] reply = ExecuteCommand(RedisCommand.TTL, ChangeTypeToString(key));
            return new(ConvertReplyToInt(reply));
        }
        /// <summary>
        /// Checks if the given key exists within the database
        /// </summary>
        /// <returns>True if the key exists, else false</returns>
        public RedisAnswer<bool> KeyExists<K>(K key)
        {
            if (!IsConnected)
                return new(false, false);
            ValidateArguments(key);
            string[] reply = ExecuteCommand(RedisCommand.EXISTS, ChangeTypeToString(key));
            return new(ConvertReplyToBool(reply));
        }
        /// <summary>
        /// Renames the given key to the specified new name
        /// </summary>
        /// <typeparam name="K1"></typeparam>
        /// <typeparam name="K2"></typeparam>
        /// <param name="key"></param>
        /// <param name="newKey"></param>
        /// <returns>StatusCode.OK if succeeded, otherwise StatusCode.Faulted</returns>
        public RedisAnswer<RedisAnswerStatusCode> RenameKey<K1, K2>(K1 key, K2 newKey)
        {
            if (!IsConnected)
                return new(RedisAnswerStatusCode.Undefined, false);
            ValidateArguments(key);
            ValidateArguments(newKey);
            string[] reply = ExecuteCommand(RedisCommand.RENAME, ChangeTypeToString(key), ChangeTypeToString(newKey));
            return (RedisAnswerStatusCode)Enum.Parse(typeof(RedisAnswerStatusCode), ConvertReplyToString(reply), true);
        }
        /// <summary>
        /// Gets the type of the key
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="key"></param>
        /// <returns>the <see cref="KeyType"/> of the given key</returns>
        public RedisAnswer<KeyType> GetKeyType<K>(K key)
        {
            if (!IsConnected)
                return new(KeyType.Undefined, false);
            ValidateArguments(key);
            string[] reply = ExecuteCommand(RedisCommand.TYPE, ChangeTypeToString(key));
            string retVal = ConvertReplyToString(reply);
            if (retVal == null)
                return KeyType.Undefined;
            else
                return ConvertReplyToEnum<KeyType>(retVal);  //(KeyType)Enum.Parse(typeof(KeyType), retVal, true);
        }
        /// <summary>
        /// Sets the expiration time of the given key at the given amount of seconds
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="key"></param>
        /// <param name="timeoutInSeconds"></param>
        /// <returns>true if the operation succeeded, false if not</returns>
        public RedisAnswer<bool> SetKeyExpiry<K>(K key, int timeoutInSeconds)
        {
            if (!IsConnected)
                return new(false, false);
            ValidateArguments(key);
            string[] reply = ExecuteCommand(RedisCommand.EXPIRE, ChangeTypeToString(key), Convert.ToString(timeoutInSeconds));
            return ConvertReplyToBool(reply);
        }
        /// <summary>
        /// Removes the timeout of the given key
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public RedisAnswer<bool> RemoveTimeout<K>(K key)
        {
            if (!IsConnected)
                return new(false, false);
            ValidateArguments(key);
            string[] reply = ExecuteCommand(RedisCommand.PERSIST, ChangeTypeToString<K>(key));
            return ConvertReplyToBool(reply);
        }
        /// <summary>
        /// Gets the value at the given key
        /// </summary>
        /// <returns>The value at the key</returns>
        public RedisAnswer<T> Get<K, T>(K key)
        {
            if (!IsConnected)
                return new(default, false);
            ValidateArguments(key);
            string[] reply = ExecuteCommand(RedisCommand.GET, ChangeTypeToString<K>(key));
            return ChangeStringToType<T>(ConvertReplyToString(reply));
        }
        /// <summary>
        /// Sets the given key to the given value
        /// </summary>
        /// <returns><see cref="RedisAnswerStatusCode.OK"/> if the operation succeeded, else <see cref="RedisAnswerStatusCode.Faulted"/></returns>
        public RedisAnswer<RedisAnswerStatusCode> Set<K, V>(K key, V val)
        {
            if (!IsConnected)
                return new(RedisAnswerStatusCode.Undefined, false);
            ValidateArguments(key);
            ValidateArguments(val);
            // 150 kb max size for a value 
            if (val is string s)
            {
                int numberOfChunks = Math.Max(s.Length / (1024 * 1024 * 1), 1);
                var chunks = s.Partition(numberOfChunks);

                foreach (int i in 0..chunks.Length)
                {
                    if (i == chunks.Length) break;

                    string value = new string(chunks[i].ToArray());
                    if(string.IsNullOrEmpty(value))
                        continue;

                    string[] reply;

                    if(i is 0)
                        reply = ExecuteCommand(RedisCommand.SET, ChangeTypeToString(key), value);
                    else
                        reply = ExecuteCommand(RedisCommand.APPEND, ChangeTypeToString(key), value);

                    bool b = TypeWorker.TryCastPrimitive(reply[0], out int replyNum);
                    var e = ConvertReplyToEnum<RedisAnswerStatusCode>(ConvertReplyToString(reply));
                    if (e is not RedisAnswerStatusCode.OK && !b)
                    {
                        RedisAnswerStatusCode code = (RedisAnswerStatusCode)replyNum;
                        return new(code, false);
                    }

                    float progress = (float)i / chunks.Length;
                    CallProgressReporter(new ProgressReporter(progress * 100, $"handled {i} chunks of {chunks.Length}"));
                   
                }
                return new(RedisAnswerStatusCode.OK, true);
            }
            else
            {
                string[] reply = ExecuteCommand(RedisCommand.SET, ChangeTypeToString(key), ChangeTypeToString(val));
                return ConvertReplyToEnum<RedisAnswerStatusCode>(ConvertReplyToString(reply));
            }
        }
        /// <summary>
        /// Appends the <paramref name="val"/> to the specified key's existing value.
        /// </summary>
        /// <returns>The length of the final apended string.</returns>
        public RedisAnswer<int> Append<K, V>(K key, V val)
        {
            if (!IsConnected)
                return new(0, false);
            ValidateArguments(key);
            ValidateArguments(val);
            string[] reply = ExecuteCommand(RedisCommand.APPEND, ChangeTypeToString(key), ChangeTypeToString<V>(val));
            return ConvertReplyToInt(reply);
        }
        /// <summary>
        /// Gets the value of the given key, then overrides it with the new value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="key"></param>
        /// <param name="val"></param>
        /// <returns>the value that was found</returns>
        public RedisAnswer<T> GetSet<T, K, V>(K key, V val)
        {
            if (!IsConnected)
                return new(default, false);
            ValidateArguments(key);
            ValidateArguments(val);
            string[] reply = ExecuteCommand(RedisCommand.GETSET, ChangeTypeToString(key), ChangeTypeToString(val));
            return ChangeStringToType<T>(ConvertReplyToString(reply));
        }
        /// <summary>
        /// Gets the values of multiple keys at the same time
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <param name="keys"></param>
        /// <returns>an array of all the values</returns>
        public RedisAnswer<T[]> GetMultiple<T, K>(params K[] keys)
        {
            if (!IsConnected)
                return new(Array.Empty<T>(), false);
            ValidateArguments(keys);
            string[] reply = ExecuteCommand(RedisCommand.MGET, ChangeTypeArrayToString<K>(keys));
            return ChangeStringArrayToType<T>(reply);
        }
        /// <summary>
        /// Sets the key with an immidiate expire time of the specified number of seconds
        /// </summary>
        /// <returns><see cref="RedisAnswerStatusCode.OK"/> if succeeded, else <see cref="RedisAnswerStatusCode.Faulted"/></returns>
        public RedisAnswer<RedisAnswerStatusCode> SetKeyWithExpiry<K, V>(K key, int timeoutSec, V val)
        {
            ValidateArguments(key);
            ValidateArguments(val);
            string[] reply = ExecuteCommand(RedisCommand.SETEX, ChangeTypeToString<K>(key), Convert.ToString(timeoutSec), ChangeTypeToString<V>(val));
            return ConvertReplyToEnum<RedisAnswerStatusCode>(ConvertReplyToString(reply));
        }
        /// <summary>
        /// Disposes this connection by closing it.
        /// </summary>
        public new void Dispose()
        {
            if (IsDisposed)
                return;
            IsDisposed = true;

            if (IsConnected && !closeCalled)
            {
                StopDataListener();
                Close();
            }
            base.Dispose();
        }
        /// <summary>
        /// Authenticates the established connection
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public RedisAnswer<string> Authenticate(string password, bool throwOnUnauthorized = true)
        {
            if (!IsConnected)
                return new("", false);
            ValidateArguments(password);
            string[] reply = ExecuteCommand(RedisCommand.AUTH, password);
            return ConvertReplyToString(reply);
        }
        /// <summary>
        /// Pings the database and checks whether it is accessable
        /// </summary>
        /// <returns><see cref="RedisAnswerStatusCode.Pong"/> if the database is reached, else <see cref="RedisAnswerStatusCode.Undefined"/> with a <see cref="RedisAnswer{T}.HasValue"/> set to false</returns>
        public RedisAnswer<RedisAnswerStatusCode> Ping()
        {
            if (!IsConnected)
                return new(RedisAnswerStatusCode.Undefined, false);
            string[] reply = ExecuteCommand(RedisCommand.PING);
            return ConvertReplyToEnum<RedisAnswerStatusCode>(ConvertReplyToString(reply));
        }
        /// <summary>
        /// Connection is closed as soon as all the pending replies are written to the client.
        /// </summary>
        private void Close()
        {
            if (!IsConnected)
                return;
            closeCalled = true;
            ExecuteCommand(RedisCommand.QUIT);
            EndConnection();
        }

        /// <summary>
        /// Gets the size of the database in bytes
        /// </summary>
        /// <returns></returns>
        public RedisAnswer<int> GetDBSize()
        {
            if (!IsConnected)
                return new(0, false);
            string[] reply = ExecuteCommand(RedisCommand.DBSIZE);
            return ConvertReplyToInt(reply);
        }


        public RedisAnswer<int> DeleteHashFields(string key, params string[] fields)
        {
            ValidateArguments(key);
            ValidateArguments(fields);
            string[] commandParams = new string[fields.Length + 1];
            commandParams[0] = key;
            Array.Copy(fields, 0, commandParams, 1, fields.Length);
            string[] reply = ExecuteCommand(RedisCommand.HDEL, commandParams);
            return ConvertReplyToInt(reply);
        }

        public RedisAnswer<Hashtable> GetHashAllFieldsAndValues(string key)
        {
            ValidateArguments(key);
            string[] data = ExecuteCommand(RedisCommand.HGETALL, key);
            Hashtable fields = new Hashtable();
            for (int index = 0; index < data.Length; index += 2)
                fields.Add(data[index], data[index + 1]);

            return fields;
        }

        public RedisAnswer<int> GetHashFieldCount(string key)
        {
            ValidateArguments(key);
            string[] reply = ExecuteCommand(RedisCommand.HLEN, key);
            return ConvertReplyToInt(reply);
        }

        public RedisAnswer<int> SetHashField(string key, string field, string val)
        {
            ValidateArguments(key, field, val);
            string[] reply = ExecuteCommand(RedisCommand.HSET, key, field, val);
            return ConvertReplyToInt(reply);
        }

        public RedisAnswer<bool> IfHashFieldExist(string key, string field)
        {
            ValidateArguments(key, field);
            string[] reply = ExecuteCommand(RedisCommand.HEXISTS, key, field);
            return ConvertReplyToBool(reply);
        }

        public RedisAnswer<int> IncrementHashFieldBy(string key, string field, int incrementby)
        {
            ValidateArguments(key, field);
            string[] reply = ExecuteCommand(RedisCommand.HINCRBY, key, field, Convert.ToString(incrementby));
            return ConvertReplyToInt(reply);
        }

        public RedisAnswer<string[]> GetHashMultipleFieldsValue(string key, params string[] fields)
        {
            ValidateArguments(key);
            ValidateArguments(fields);
            string[] commandParams = new string[fields.Length + 1];
            commandParams[0] = key;
            Array.Copy(fields, 0, commandParams, 1, fields.Length);
            return ExecuteCommand(RedisCommand.HMGET, commandParams);
        }

        public RedisAnswer<int> SetHashFieldIfNotExist(string key, string field, string val)
        {
            ValidateArguments(key, field, val);
            string[] reply = ExecuteCommand(RedisCommand.HSETNX, key, field, val);
            return ConvertReplyToInt(reply);
        }

        public RedisAnswer<string> GetHashFieldValue(string key, string field)
        {
            ValidateArguments(key, field);
            string[] reply = ExecuteCommand(RedisCommand.HGET, key, field);
            return ConvertReplyToString(reply);
        }

        public RedisAnswer<string[]> GetHashAllFields(string key)
        {
            ValidateArguments(key);
            return ExecuteCommand(RedisCommand.HKEYS, key);
        }

        public RedisAnswer<RedisAnswerStatusCode> SetHashMultipleFieldsValue(string key, Hashtable fieldValues)
        {
            string[] commandParams = new string[fieldValues.Count * 2 + 1];
            int index = 0;
            commandParams[index++] = key;
            foreach (string field in fieldValues.Keys)
            {
                commandParams[index++] = field;
                commandParams[index++] = fieldValues[field] as string;
            }
            ValidateArguments(commandParams);
            string[] reply = ExecuteCommand(RedisCommand.HMSET, commandParams);
            return (RedisAnswerStatusCode)Enum.Parse(typeof(RedisAnswerStatusCode), ConvertReplyToString(reply), true);
        }

        public RedisAnswer<string[]> GetHashAllValues(string key)
        {
            ValidateArguments(key);
            return ExecuteCommand(RedisCommand.HVALS, key);
        }
        public RedisAnswer<int> ListLength(string key)
        {
            ValidateArguments(key);
            string[] reply = ExecuteCommand(RedisCommand.LLEN, key);
            return ConvertReplyToInt(reply);
        }

        public RedisAnswer<int> ListRemoveElementByCount(string key, int count, string val)
        {
            ValidateArguments(key, Convert.ToString(count), val);
            string[] reply = ExecuteCommand(RedisCommand.LREM, key, Convert.ToString(count), val);
            return ConvertReplyToInt(reply);
        }

        public RedisAnswer<int> ListRightPush(string key, string val)
        {
            ValidateArguments(key, val);
            string[] reply = ExecuteCommand(RedisCommand.RPUSH, key, val);
            return ConvertReplyToInt(reply);
        }
        public RedisAnswer<string> ListLeftPop(string key)
        {
            ValidateArguments(key);
            string[] reply = ExecuteCommand(RedisCommand.LPOP, key);
            return ConvertReplyToString(reply);
        }

        public RedisAnswer<RedisAnswerStatusCode> ListSetValueAtIndex(string key, int index, string val)
        {
            ValidateArguments(key, val);
            string[] reply = ExecuteCommand(RedisCommand.LSET, key, Convert.ToString(index), val);
            return (RedisAnswerStatusCode)Enum.Parse(typeof(RedisAnswerStatusCode), ConvertReplyToString(reply), true);
        }
        public RedisAnswer<int> ListRightPushIfExist(string key, string val)
        {
            ValidateArguments(key, val);
            string[] reply = ExecuteCommand(RedisCommand.RPUSHX, key, val);
            return ConvertReplyToInt(reply);
        }
        public RedisAnswer<int> ListLeftPush(string key, string val)
        {
            ValidateArguments(key, val);
            string[] reply = ExecuteCommand(RedisCommand.LPUSH, key, val);
            return ConvertReplyToInt(reply);
        }
        public RedisAnswer<string> ListRightPop(string key)
        {
            ValidateArguments(key);
            string[] reply = ExecuteCommand(RedisCommand.RPOP, key);
            return ConvertReplyToString(reply);
        }
        public RedisAnswer<string[]> ListGetElementsByRange(string key, int start, int stop)
        {
            ValidateArguments(key);
            return ExecuteCommand(RedisCommand.LRANGE, key, Convert.ToString(start), Convert.ToString(stop));
        }
        public RedisAnswer<int> SETAddMembers(string key, params string[] members)
        {
            string[] commandParams = new string[members.Length + 1];
            commandParams[0] = key;
            Array.Copy(members, 0, commandParams, 1, members.Length);
            ValidateArguments(commandParams);
            string[] reply = ExecuteCommand(RedisCommand.SADD, commandParams);
            return ConvertReplyToInt(reply);
        }

        public RedisAnswer<string[]> SETFindIntersection(params string[] keys)
        {
            ValidateArguments(keys);
            return ExecuteCommand(RedisCommand.SINTER, keys);
        }

        public RedisAnswer<int> SETMoveMember(string srcSet, string destSet, string member)
        {
            ValidateArguments(srcSet, destSet, member);
            string[] reply = ExecuteCommand(RedisCommand.SMOVE, srcSet, destSet, member);
            return ConvertReplyToInt(reply);
        }
        public RedisAnswer<string[]> SETFindUnion(params string[] keys)
        {
            ValidateArguments(keys);
            return ExecuteCommand(RedisCommand.SUNION, keys);
        }

        public RedisAnswer<int> SETTotalMembers(string key)
        {
            ValidateArguments(key);
            string[] reply = ExecuteCommand(RedisCommand.SCARD, key);
            return ConvertReplyToInt(reply);
        }
        public RedisAnswer<string> SETPopElement(string key)
        {
            ValidateArguments(key);
            string[] reply = ExecuteCommand(RedisCommand.SPOP, key);
            return ConvertReplyToString(reply);
        }
        public RedisAnswer<string[]> SETFindDifference(params string[] keys)
        {
            ValidateArguments(keys);
            return ExecuteCommand(RedisCommand.SDIFF, keys);
        }

        public RedisAnswer<bool> SETIsMember(string key, string member)
        {
            ValidateArguments(key, member);
            string[] reply = ExecuteCommand(RedisCommand.SISMEMBER, key, member);
            return ConvertReplyToBool(reply);
        }

        public RedisAnswer<string> SETGetRandomMember(string key)
        {
            ValidateArguments(key);
            string[] reply = ExecuteCommand(RedisCommand.SRANDMEMBER, key);
            return ConvertReplyToString(reply);
        }
        public RedisAnswer<string[]> SETGetAllMembers(string key)
        {
            ValidateArguments(key);
            return ExecuteCommand(RedisCommand.SMEMBERS, key);
        }

        public RedisAnswer<bool> SETRemoveMember(string key, string member)
        {
            ValidateArguments(key, member);
            string[] reply = ExecuteCommand(RedisCommand.SREM, key, member);
            return ConvertReplyToBool(reply);
        }


        #region Private stuff      
        private RedisAnswer<string> GetObject<K>(RedisSubCommand subCommand, params K[] args)
        {
            ValidateArguments(args);
            string[] commandParams = new string[args.Length + 1];
            commandParams[0] = subCommand.ToString();
            Array.Copy(ChangeTypeArrayToString(args), 0, commandParams, 1, args.Length);
            string[] reply = ExecuteCommand(RedisCommand.OBJECT, commandParams);
            return ConvertReplyToString(reply);
        }

        private RedisAnswer<bool> RenameKeyIfNotExist<K1, K2>(K1 key, K2 newKey)
        {
            ValidateArguments(key);
            ValidateArguments(newKey);
            string[] reply = ExecuteCommand(RedisCommand.RENAMENX, ChangeTypeToString(key), ChangeTypeToString(newKey));
            return ConvertReplyToBool(reply);
        }

        /// <summary>
        /// Sets the expiry for the specified key.
        /// timeout is specified in unix timestamp i.e. seconds since January 1, 1970.
        /// </summary>
        /// <returns>
        /// True if the operation succeeded, false if not
        /// </returns>
        private RedisAnswer<bool> SetExpireAt<K>(K key, int timeout)
        {
            ValidateArguments(key);
            string[] reply = ExecuteCommand(RedisCommand.EXPIREAT, ChangeTypeToString<K>(key), Convert.ToString(timeout));
            return ConvertReplyToBool(reply);
        }

        private RedisAnswer<string> GetRangeOfValue<K>(K key, int start, int end)
        {
            ValidateArguments(key);
            string[] reply = ExecuteCommand(RedisCommand.GETRANGE, ChangeTypeToString(key), Convert.ToString(start), Convert.ToString(end));
            return ConvertReplyToString(reply);
        }

        private RedisAnswer<bool> SetIfNotExist<K, V>(K key, V val)
        {
            ValidateArguments(key);
            ValidateArguments(val);
            string[] reply = ExecuteCommand(RedisCommand.SETNX, ChangeTypeToString(key), ChangeTypeToString(val));
            return ConvertReplyToBool(reply);
        }

        private RedisAnswer<int> Decrement<K>(K key)
        {
            ValidateArguments(key);
            string[] reply = ExecuteCommand(RedisCommand.DECR, ChangeTypeToString(key));
            return ConvertReplyToInt(reply);
        }
        private RedisAnswer<bool> MoveKey<K>(K key, int db)
        {
            ValidateArguments(key);
            string[] reply = ExecuteCommand(RedisCommand.MOVE, ChangeTypeToString(key), Convert.ToString(db));
            return ConvertReplyToBool(reply);
        }

        private void SetMultipleKeysIfNotExist<K>(params K[] keys)
        {
            ValidateArguments(keys);
            ExecuteCommand(RedisCommand.MSETNX, ChangeTypeArrayToString<K>(keys));
        }

        private void QueueSetMultipleKeysIfNotExist<K>(params K[] keys)
        {
            ValidateArguments(keys);
            EnqueueAndSendCommand(RedisCommand.MSETNX, ChangeTypeArrayToString<K>(keys));
        }

        private void SetValueAtOffset<K, V>(K key, int offset, V val)
        {
            ValidateArguments(key);
            ValidateArguments(val);
            ExecuteCommand(RedisCommand.SETRANGE, ChangeTypeToString(key), Convert.ToString(offset), ChangeTypeToString(val));
        }

        private void QueueSetValueAtOffset<K, V>(K key, int offset, V val)
        {
            ValidateArguments(key);
            ValidateArguments(val);
            EnqueueAndSendCommand(RedisCommand.SETRANGE, ChangeTypeToString(key), Convert.ToString(offset), ChangeTypeToString(val));
        }

        private RedisAnswer<int> DecrementBy<K>(K key, int decrementOffset)
        {
            ValidateArguments(key);
            string[] reply = ExecuteCommand(RedisCommand.DECRBY, ChangeTypeToString(key), Convert.ToString(decrementOffset));
            return ConvertReplyToInt(reply);
        }

        private RedisAnswer<int> Increment<K>(K key)
        {
            ValidateArguments(key);
            string[] reply = ExecuteCommand(RedisCommand.INCR, ChangeTypeToString(key));
            return ConvertReplyToInt(reply);
        }

        private RedisAnswer<int> Strlen<K>(K key)
        {
            ValidateArguments<K>(key);
            string[] reply = ExecuteCommand(RedisCommand.STRLEN, ChangeTypeToString(key));
            return ConvertReplyToInt(reply);
        }

        private RedisAnswer<int> IncrementBy<K>(K key, int incrementOffset)
        {
            ValidateArguments(key);
            string[] reply = ExecuteCommand(RedisCommand.INCRBY, ChangeTypeToString(key), Convert.ToString(incrementOffset));
            return ConvertReplyToInt(reply);
        }

        private RedisAnswer<int> SetBitAtOffset<K>(K key, int offset, string val)
        {
            ValidateArguments<K>(key);
            ValidateArguments<string>(val);
            string[] reply = ExecuteCommand(RedisCommand.SETBIT, ChangeTypeToString(key), Convert.ToString(offset), val);
            return ConvertReplyToInt(reply);
        }

        private RedisAnswer<int> GetBitAtOffset<K>(K key, int offset)
        {
            ValidateArguments(key);
            string[] reply = ExecuteCommand(RedisCommand.GETBIT, ChangeTypeToString(key), Convert.ToString(offset));
            return ConvertReplyToInt(reply);
        }

        /// <summary>
        /// Returns the left most element of the first non-empty list encountered.
        /// If all of the specified lists are empty, the connection is blocked until an element
        /// is inserted using LPUSH or RPUSH or timeout expires.
        /// A timeout of zero can be used to block indefinitely.
        /// </summary>
        /// <returns>
        /// The pop left blocking.
        /// </returns>
        /// <param name='timeoutSeconds'>
        /// Timeout seconds.
        /// </param>
        /// <param name='keys'>
        /// Keys.
        /// </param>
        private RedisAnswer<string[]> ListPopLeftBlocking(int timeoutSeconds, params string[] keys)
        {
            string[] commandParams = new string[keys.Length + 1];
            Array.Copy(keys, 0, commandParams, 0, keys.Length);
            commandParams[keys.Length] = Convert.ToString(timeoutSeconds);
            ValidateArguments(commandParams);
            return ExecuteCommand(RedisCommand.BLPOP, commandParams);
        }


        /// <summary>
        /// Returns the right most element of the first non-empty list encountered.
        /// If all of the specified lists are empty, the connection is blocked until an element
        /// is inserted using LPUSH or RPUSH or timeout expires.
        /// A timeout of zero can be used to block indefinitely.
        /// </summary>
        /// <returns>
        /// The pop right blocking.
        /// </returns>
        /// <param name='timeoutSeconds'>
        /// Timeout seconds.
        /// </param>
        /// <param name='keys'>
        /// Keys.
        /// </param>
        private RedisAnswer<string[]> ListPopRightBlocking(int timeoutSeconds, params string[] keys)
        {
            string[] commandParams = new string[keys.Length + 1];
            Array.Copy(keys, 0, commandParams, 0, keys.Length);
            commandParams[keys.Length] = Convert.ToString(timeoutSeconds);
            ValidateArguments(commandParams);
            return ExecuteCommand(RedisCommand.BRPOP, commandParams);
        }


        private void QueueListSetValueAtIndex(string key, int index, string val)
        {
            ValidateArguments(key, val);
            EnqueueAndSendCommand(RedisCommand.LSET, key, Convert.ToString(index), val);
        }


        private void QueueListRightPushIfExist(string key, string val)
        {
            ValidateArguments(key, val);
            EnqueueAndSendCommand(RedisCommand.RPUSHX, key, val);
        }

        private RedisAnswer<string> ListRightPopLeftPushBlocking(string srcKey, string destKey, int timeout)
        {
            ValidateArguments(srcKey, destKey);
            string[] reply = ExecuteCommand(RedisCommand.BRPOPLPUSH, srcKey, destKey, Convert.ToString(timeout));
            return ConvertReplyToString(reply);
        }


        private void QueueListLeftPush(string key, string val)
        {
            ValidateArguments(key, val);
            EnqueueAndSendCommand(RedisCommand.LPUSH, key, val);
        }

        private RedisAnswer<RedisAnswerStatusCode> ListLeftTrim(string key, int start, int stop)
        {
            ValidateArguments(key);
            string[] reply = ExecuteCommand(RedisCommand.LTRIM, key, Convert.ToString(start), Convert.ToString(stop));
            return (RedisAnswerStatusCode)Enum.Parse(typeof(RedisAnswerStatusCode), ConvertReplyToString(reply), true);
        }

        private void QueueListLeftTrim(string key, int start, int stop)
        {
            ValidateArguments(key);
            EnqueueAndSendCommand(RedisCommand.LTRIM, key, Convert.ToString(start), Convert.ToString(stop));
        }

        private RedisAnswer<string> ListGetElementAtIndex(string key, int index)
        {
            ValidateArguments(key);
            string[] reply = ExecuteCommand(RedisCommand.LINDEX, key, Convert.ToString(index));
            return ConvertReplyToString(reply);
        }

        private RedisAnswer<int> ListLeftPushIfExist(string key, string val)
        {
            ValidateArguments(key, val);
            string[] reply = ExecuteCommand(RedisCommand.LPUSHX, key, val);
            return ConvertReplyToInt(reply);
        }

        private void QueueListLeftPushIfExist(string key, string val)
        {
            ValidateArguments(key, val);
            EnqueueAndSendCommand(RedisCommand.LPUSHX, key, val);
        }


        private RedisAnswer<int> ListInsertWithPivot(string key, ListInsert direction, string pivot, string val)
        {
            ValidateArguments(key, pivot, val);
            string[] reply = ExecuteCommand(RedisCommand.LINSERT, key, direction.ToString(), pivot, val);
            return ConvertReplyToInt(reply);
        }

        private void QueueListInsertWithPivot(string key, ListInsert direction, string pivot, string val)
        {
            ValidateArguments(key, pivot, val);
            EnqueueAndSendCommand(RedisCommand.LINSERT, key, direction.ToString(), pivot, val);
        }


        private RedisAnswer<string> ListRightPopLeftPush(string srcKey, string destKey)
        {
            ValidateArguments(srcKey, destKey);
            string[] reply = ExecuteCommand(RedisCommand.BRPOPLPUSH, srcKey, destKey);
            return ConvertReplyToString(reply);
        }


        private void QueueSETMoveMember(string srcSet, string destSet, string member)
        {
            ValidateArguments(srcSet, destSet, member);
            EnqueueAndSendCommand(RedisCommand.SMOVE, srcSet, destSet, member);
        }


        private RedisAnswer<int> SETFindIntersectAndStore(string destKey, params string[] srcKeys)
        {
            string[] commandParams = new string[srcKeys.Length + 1];
            commandParams[0] = destKey;
            Array.Copy(srcKeys, 0, commandParams, 1, srcKeys.Length);
            ValidateArguments(commandParams);
            string[] reply = ExecuteCommand(RedisCommand.SINTERSTORE, commandParams);
            return ConvertReplyToInt(reply);
        }


        private RedisAnswer<int> SETFindUnionAndStore(string destKey, params string[] srcKeys)
        {
            string[] commandParams = new string[srcKeys.Length + 1];
            commandParams[0] = destKey;
            Array.Copy(srcKeys, 0, commandParams, 1, srcKeys.Length);
            ValidateArguments(commandParams);
            string[] reply = ExecuteCommand(RedisCommand.SUNIONSTORE, commandParams);
            return ConvertReplyToInt(reply);
        }


        private RedisAnswer<int> SETFindDifferenceAndStore(params string[] keys)
        {
            ValidateArguments(keys);
            string[] reply = ExecuteCommand(RedisCommand.SDIFF, keys);
            return ConvertReplyToInt(reply);
        }



        private RedisAnswer<int> SSAddMemberWithScore(string key, double score, string member)
        {
            ValidateArguments(key, member);
            score = Math.Round(score, 2);
            string[] reply = ExecuteCommand(RedisCommand.ZADD, key, Convert.ToString(score), member);
            return ConvertReplyToInt(reply);
        }

        private RedisAnswer<int> SSIntersection(string destKey, int numKeys, string[] keys, string[] weights, SSResultScore resultType)
        {
            string[] commandParams = new string[keys.Length + weights.Length + 5];
            commandParams[0] = destKey;
            commandParams[1] = Convert.ToString(numKeys);
            Array.Copy(keys, 0, commandParams, 2, keys.Length);
            commandParams[keys.Length + 2] = SSParam.Weight.ToString();
            Array.Copy(weights, 0, commandParams, keys.Length + 3, weights.Length);
            commandParams[keys.Length + weights.Length + 3] = SSParam.Aggregate.ToString();
            commandParams[keys.Length + weights.Length + 4] = resultType.ToString();
            ValidateArguments(commandParams);
            string[] reply = ExecuteCommand(RedisCommand.ZINTERSTORE, commandParams);
            return ConvertReplyToInt(reply);
        }

        private RedisAnswer<int> SSRemoveMember(string key, string member)
        {
            ValidateArguments(key, member);
            string[] reply = ExecuteCommand(RedisCommand.ZREM, key, member);
            return ConvertReplyToInt(reply);
        }

        private RedisAnswer<string[]> SSGetElementByScoreRangeDescending(string key, int minScore, int maxScore, bool withScores)
        {
            ValidateArguments(key);
            if (withScores)
                return ExecuteCommand(RedisCommand.ZREVRANGEBYSCORE, key, Convert.ToString(minScore), Convert.ToString(maxScore), SSParam.WithScores.ToString());
            else
                return ExecuteCommand(RedisCommand.ZREVRANGEBYSCORE, key, Convert.ToString(minScore), Convert.ToString(maxScore));
        }

        private RedisAnswer<string[]> SSGetElementsByScoreRangeDescending(string key, int minScore, int maxScore, bool withScores, int offset, int count)
        {
            ValidateArguments(key);
            if (withScores)
                return ExecuteCommand(RedisCommand.ZREVRANGEBYSCORE, key, Convert.ToString(minScore), Convert.ToString(maxScore), SSParam.WithScores.ToString(), SSParam.Limits.ToString(), Convert.ToString(offset), Convert.ToString(count));
            else
                return ExecuteCommand(RedisCommand.ZREVRANGEBYSCORE, key, Convert.ToString(minScore), Convert.ToString(maxScore), SSParam.Limits.ToString(), Convert.ToString(offset), Convert.ToString(count));
        }

        private RedisAnswer<int> SSGetCardinality(string key)
        {
            ValidateArguments(key);
            string[] reply = ExecuteCommand(RedisCommand.ZCARD, key);
            return ConvertReplyToInt(reply);
        }

        private RedisAnswer<string[]> SSGetElementsByIndexRangeAscending(string key, int minIndex, int maxIndex, bool withScores)
        {
            ValidateArguments(key);
            if (withScores)
                return ExecuteCommand(RedisCommand.ZRANGE, key, Convert.ToString(minIndex), Convert.ToString(maxIndex), SSParam.WithScores.ToString());
            else
                return ExecuteCommand(RedisCommand.ZRANGE, key, Convert.ToString(minIndex), Convert.ToString(maxIndex));
        }

        private RedisAnswer<int> SSRemoveElementsByIndexRange(string key, int minIndex, int maxIndex)
        {
            ValidateArguments(key);
            string[] reply = ExecuteCommand(RedisCommand.ZREMRANGEBYRANK, key, Convert.ToString(minIndex), Convert.ToString(maxIndex));
            return ConvertReplyToInt(reply);
        }

        private RedisAnswer<string> SSGetRankDescending(string key, string member)
        {
            ValidateArguments(key, member);
            string[] reply = ExecuteCommand(RedisCommand.ZREVRANK, key, member);
            return ConvertReplyToString(reply);
        }

        private RedisAnswer<int> SSGetCountByScoreRange(string key, int minScore, int maxScore)
        {
            ValidateArguments(key);
            string[] reply = ExecuteCommand(RedisCommand.ZCOUNT, key, Convert.ToString(minScore), Convert.ToString(maxScore));
            return ConvertReplyToInt(reply);
        }

        private RedisAnswer<string[]> SSGetElementByScoreRangeAscending(string key, int minScore, int maxScore, bool withScores)
        {
            ValidateArguments(key);
            if (withScores)
                return ExecuteCommand(RedisCommand.ZRANGEBYSCORE, key, Convert.ToString(minScore), Convert.ToString(maxScore), SSParam.WithScores.ToString());
            else
                return ExecuteCommand(RedisCommand.ZRANGEBYSCORE, key, Convert.ToString(minScore), Convert.ToString(maxScore));
        }

        private RedisAnswer<string[]> SSGetElementsByScoreRangeAscending(string key, int minScore, int maxScore, bool withScores, int offset, int count)
        {
            ValidateArguments(key);
            if (withScores)
                return ExecuteCommand(RedisCommand.ZRANGEBYSCORE, key, Convert.ToString(minScore), Convert.ToString(maxScore), SSParam.WithScores.ToString(), SSParam.Limits.ToString(), Convert.ToString(offset), Convert.ToString(count));
            else
                return ExecuteCommand(RedisCommand.ZRANGEBYSCORE, key, Convert.ToString(minScore), Convert.ToString(maxScore), SSParam.Limits.ToString(), Convert.ToString(offset), Convert.ToString(count));
        }

        private RedisAnswer<int> SSRemoveElementsByScoreRange(string key, int minScore, int maxScore)
        {
            ValidateArguments(key);
            string[] reply = ExecuteCommand(RedisCommand.ZREMRANGEBYSCORE, key, Convert.ToString(minScore), Convert.ToString(maxScore));
            return ConvertReplyToInt(reply);
        }

        private RedisAnswer<string> SSGetScore(string key, string member)
        {
            ValidateArguments(key, member);
            string[] reply = ExecuteCommand(RedisCommand.ZSCORE, key, member);
            return ConvertReplyToString(reply);
        }

        private RedisAnswer<string> SSIncrementScore(string key, int incrementBy, string member)
        {
            ValidateArguments(key, member);
            string[] reply = ExecuteCommand(RedisCommand.ZINCRBY, key, Convert.ToString(incrementBy), member);
            return ConvertReplyToString(reply);
        }

        private RedisAnswer<string> SSGetRankAscending(string key, string member)
        {
            ValidateArguments(key, member);
            string[] reply = ExecuteCommand(RedisCommand.ZRANK, key, member);
            return ConvertReplyToString(reply);
        }

        private RedisAnswer<string[]> SSGetElementsByIndexRangeDescending(string key, int minIndex, int maxIndex, bool withScores)
        {
            ValidateArguments(key);
            if (withScores)
                return ExecuteCommand(RedisCommand.ZREVRANGE, key, Convert.ToString(minIndex), Convert.ToString(maxIndex), SSParam.WithScores.ToString());
            else
                return ExecuteCommand(RedisCommand.ZREVRANGE, key, Convert.ToString(minIndex), Convert.ToString(maxIndex));
        }

        private RedisAnswer<int> SSUnion(string destKey, int numKeys, string[] keys, string[] weights, SSResultScore resultType)
        {
            string[] commandParams = new string[keys.Length + weights.Length + 5];
            commandParams[0] = destKey;
            commandParams[1] = Convert.ToString(numKeys);
            Array.Copy(keys, 0, commandParams, 2, keys.Length);
            commandParams[keys.Length + 2] = SSParam.Weight.ToString();
            Array.Copy(weights, 0, commandParams, keys.Length + 3, weights.Length);
            commandParams[keys.Length + weights.Length + 3] = SSParam.Aggregate.ToString();
            commandParams[keys.Length + weights.Length + 4] = resultType.ToString();
            ValidateArguments(commandParams);
            string[] reply = ExecuteCommand(RedisCommand.ZUNIONSTORE, commandParams);
            return ConvertReplyToInt(reply);
        }

        private void RollbackTransaction()
        {
            EnqueueAndSendCommand(RedisCommand.DISCARD);
        }

        private void BeginTransaction()
        {
            isInTransaction = true;
            EnqueueAndSendCommand(RedisCommand.MULTI);
        }

        private RedisAnswer<string[]> CommitTransaction()
        {
            isInTransaction = false;
            return ExecuteCommand(RedisCommand.EXEC);
        }

        private void AddWatch(params string[] keys)
        {
            ValidateArguments(keys);
            EnqueueAndSendCommand(RedisCommand.WATCH, keys);
        }

        private void RemoveAllWatch()
        {
            EnqueueAndSendCommand(RedisCommand.UNWATCH);
        }

        private RedisAnswer<RedisAnswerStatusCode> ChangeDB(int index)
        {
            string[] reply = ExecuteCommand(RedisCommand.SELECT, Convert.ToString(index));
            return (RedisAnswerStatusCode)Enum.Parse(typeof(RedisAnswerStatusCode), ConvertReplyToString(reply), true);
        }

        private void RewriteAppendOnlyFile()
        {
            ExecuteCommand(RedisCommand.BGREWRITEAOF);
        }

        private RedisAnswer<string> GetServerInfo()
        {
            string[] reply = ExecuteCommand(RedisCommand.INFO);
            return ConvertReplyToString(reply);
        }

        private RedisAnswer<RedisAnswerStatusCode> ChangeMaster(string host, int port)
        {
            string[] reply = ExecuteCommand(RedisCommand.SLAVEOF, host, Convert.ToString(port));
            return (RedisAnswerStatusCode)Enum.Parse(typeof(RedisAnswerStatusCode), ConvertReplyToString(reply), true);
        }

        private RedisAnswer<RedisAnswerStatusCode> SaveInBackground()
        {
            string[] reply = ExecuteCommand(RedisCommand.BGSAVE);
            return (RedisAnswerStatusCode)Enum.Parse(typeof(RedisAnswerStatusCode), ConvertReplyToString(reply), true);
        }

        private RedisAnswer<string> GetDebugInfo(string key)
        {
            ValidateArguments(key);
            string[] reply = ExecuteCommand(RedisCommand.DEBUG, RedisSubCommand.OBJECT.ToString(), key);
            return ConvertReplyToString(reply);
        }

        private RedisAnswer<int> GetLastSaveTimestamp()
        {
            string[] reply = ExecuteCommand(RedisCommand.LASTSAVE);
            return ConvertReplyToInt(reply);
        }

        private RedisAnswer<string> GetConfigParams(string pattern)
        {
            string[] reply = ExecuteCommand(RedisCommand.CONFIG, RedisSubCommand.GET.ToString(), pattern);
            return ConvertReplyToString(reply);
        }

        private RedisAnswer<RedisAnswerStatusCode> SetConfigParam(string parameter, string val)
        {
            ValidateArguments(parameter, val);
            string[] reply = ExecuteCommand(RedisCommand.CONFIG, RedisSubCommand.SET.ToString(), parameter, val);
            return (RedisAnswerStatusCode)Enum.Parse(typeof(RedisAnswerStatusCode), ConvertReplyToString(reply), true);
        }

        /// <summary>
        /// <b>WARNING</b>. This will delete <b><em>every</em></b> entry within your database. Use with caution
        /// </summary>
        /// <returns><see cref="RedisAnswerStatusCode.OK"/> if the operation succeeded. else <see cref="RedisAnswerStatusCode.Faulted"/></returns>
        public RedisAnswer<RedisAnswerStatusCode> Flush()
        {
            string[] reply = ExecuteCommand(RedisCommand.FLUSHALL);
            return (RedisAnswerStatusCode)Enum.Parse(typeof(RedisAnswerStatusCode), ConvertReplyToString(reply), true);
        }

        private RedisAnswer<RedisAnswerStatusCode> ResetStats()
        {
            string[] reply = ExecuteCommand(RedisCommand.CONFIG, RedisSubCommand.RESETSTAT.ToString());
            return (RedisAnswerStatusCode)Enum.Parse(typeof(RedisAnswerStatusCode), ConvertReplyToString(reply), true);
        }

        private RedisAnswer<RedisAnswerStatusCode> ClearCurrentDB()
        {
            string[] reply = ExecuteCommand(RedisCommand.FLUSHDB);
            return (RedisAnswerStatusCode)Enum.Parse(typeof(RedisAnswerStatusCode), ConvertReplyToString(reply), true);
        }

        private void Publish(string channel, string message)
        {
            ValidateArguments(channel, message);
            SendCommand(RedisCommand.PUBLISH, channel, message);
        }

        private void StartDataListener()
        {

            dataListener = new Thread(() => Listen(listeningThreadAbortToken));
            dataListener.Start();
        }

        private void StopDataListener()
        {
            if (dataListener != null)
            {
                listeningThreadAbortToken.RequestAbort();
                while (!listeningThreadAbortToken.abortSuccessful) { }
            }
        }

        private void Listen(ThreadAbortToken abortToken)
        {
            while (!abortToken.shouldAbort)
            {
                if (redisStream.IsDataAvailable() && sendCommanSyncQueue.Count > 0)
                {
                    redisStream.HandleReply();
                    sendCommanSyncQueue.Dequeue();
                }
            }
            abortToken.abortSuccessful = true;
        }

        private record ThreadAbortToken()
        {
            public bool shouldAbort { get; private set; } = false;
            public bool abortSuccessful = false;
            public void RequestAbort()
            {
                shouldAbort = true;
            }
        }
        #endregion
    }
}