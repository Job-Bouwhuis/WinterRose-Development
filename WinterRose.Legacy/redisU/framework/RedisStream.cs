using System;

namespace WinterRose.WIP.Redis.Framework
{
	public interface RedisStream
	{	
		event Action<float> ProgressReporter;
		string[] GetResponse(byte[] bytes);
		
		void SendData(byte[] bytes);
		
		void HandleReply();
		
		string HandleBulkReply();
		
		string[] HandleMultiBulkReply();
		
		void HandleErrorReply();
		
		string HandleIntegerReply();
		
		string HandleSingleLineReply();
		
		string[] HandleChannelMessage();
		
		void CloseConnection();
		
		bool IsDataAvailable();
    }
}

