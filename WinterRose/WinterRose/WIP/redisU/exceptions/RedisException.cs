using System;
using WinterRose.Exceptions;

namespace WinterRose.WIP.Redis.Exceptions
{
	public sealed class RedisException(string msg, Exception? inner) : WinterException(msg, inner)
    {
        public RedisException(string msg) : this(msg, null) { }
	}
}

