namespace WinterRose.WIP.Redis.Framework
{
	public enum RedisCommand
	{
		//Keys Commands
		DEL,
		KEYS,
		RANDOMKEY,
		TTL,
		EXISTS,
		MOVE,
		RENAME,
		TYPE,
		EXPIRE,
		OBJECT,
		RENAMENX,
		EXPIREAT,
		PERSIST,
		SORT,
		//Strings Commands
		APPEND,
		GETRANGE,
		MSET,
		SETNX,
		DECR,
		GETSET,
		MSETNX,
		SETRANGE,
		DECRBY,
		INCR,
		SET,
		STRLEN,
		GET,
		INCRBY,
		SETBIT,
		GETBIT,
		MGET,
		SETEX,
		//Hashes
		HDEL,
		HGETALL,
		HLEN,
		HSET,
		HEXISTS,
		HINCRBY,
		HMGET,
		HSETNX,
		HGET,
		HKEYS,
		HMSET,
		HVALS,
		//Lists
		BLPOP,
		LLEN,
		LREM,
		RPUSH,
		BRPOP,
		LPOP,
		LSET,
		RPUSHX,
		BRPOPLPUSH,
		LPUSH,
		LTRIM,
		LINDEX,
		LPUSHX,
		RPOP,
		LINSERT,
		LRANGE,
		RPOPLPUSH,
		//Sets
		SADD,
		SINTER,
		SMOVE,
		SUNION,
		SCARD,
		SINTERSTORE,
		SPOP,
		SUNIONSTORE,
		SDIFF,
		SISMEMBER,
		SRANDMEMBER,
		SDIFFSTORE,
		SMEMBERS,
		SREM,
		//SortedSets
		ZADD,
		ZINTERSTORE,
		ZREM,
		ZREVRANGEBYSCORE,
		ZCARD,
		ZRANGE,
		ZREMRANGEBYRANK,
		ZREVRANK,
		ZCOUNT,
		ZRANGEBYSCORE,
		ZREMRANGEBYSCORE,
		ZSCORE,
		ZINCRBY,
		ZRANK,
		ZREVRANGE,
		ZUNIONSTORE,
		//Pub/Sub
		PSUBSCRIBE,
		PUNSUBSCRIBE,
		UNSUBSCRIBE,
		PUBLISH,
		SUBSCRIBE,
		//Transactions
		DISCARD,
		MULTI,
		WATCH,
		EXEC,
		UNWATCH,
		//CONNECTION
		AUTH,
		PING,
		SELECT,
		ECHO,
		QUIT,
		//Server
		BGREWRITEAOF,
		DBSIZE,
		INFO,
		SLAVEOF,
		BGSAVE,
		DEBUG,
		LASTSAVE,
		SLOWLOG,
		CONFIG,
		MONITOR,
		SYNC,
		FLUSHALL,
		SAVE,
		FLUSHDB,
		SHUTDOWN
	}
	
	public enum RedisSubCommand
	{
		REFCOUNT = 0,
		ENCODING,
		IDLETIME,
		OBJECT,
		GET,
		SEGFAULT,
		SET,
		RESETSTAT
	};
}

