using System;
using System.Threading;
using WinterRose.WIP.Redis.Events;
using WinterRose.WIP.Redis.Exceptions;

namespace WinterRose.WIP.Redis.Framework
{
	public sealed class RedisSubscription : RedisConnectionBase
	{
		public delegate void MessageHandler(object sender, MessageEventArgs args);
		public event MessageHandler OnMessageReceived;
		
		public delegate void SubscriptionHandler(object sender, SubscriptionEventArgs args);
		public event SubscriptionHandler OnSubscribe;
		public event SubscriptionHandler OnUnsubscribe;
		
		private object _sendLock = new object();
		private object _listenLock = new object();
		private Thread dataListener = null;
		private Thread dataSender = null;
		
		private enum PubSubResponse
		{
			subscribe,
			unsubscribe,
			message
		}
		
		
		public RedisSubscription(string host, int port)
		{
			CreateConnection(host, port);
			StartDataSender();
			StartDataListener();
		}
		
		private void StartDataListener()
		{
			dataListener = new Thread(new ThreadStart(this.Listen));
			dataListener.Start();
		}
		
		private void StartDataSender()
		{
			dataSender = new Thread(new ThreadStart(this.Send));
			dataSender.Start();
		}
		
		public void Subscribe(params string[] channels)
		{
			sendCommanSyncQueue.Enqueue(new CommandInfo(RedisCommand.SUBSCRIBE, channels));
		}
		
		public void Unsubscribe(params string[] channels)
		{
			sendCommanSyncQueue.Enqueue(new CommandInfo(RedisCommand.UNSUBSCRIBE, channels));
		}
		
		public void SubscribeByPattern(params string[] channelPattern)
		{
			sendCommanSyncQueue.Enqueue(new CommandInfo(RedisCommand.PSUBSCRIBE, channelPattern));
		}
		
		public void UnsubscribeByPattern(params string[] channelPattern)
		{
			sendCommanSyncQueue.Enqueue(new CommandInfo(RedisCommand.PUNSUBSCRIBE, channelPattern));
		}
		
		public void Send()
		{
			while(true)
			{
				if(sendCommanSyncQueue.Count > 0)
				{
					while(sendCommanSyncQueue.Count > 0)
					{
						CommandInfo commandInfo = (CommandInfo)sendCommanSyncQueue.Dequeue();
						SendCommand(commandInfo.GetCommand(), commandInfo.GetArgs());
					}

					lock(_listenLock)
					{
						Monitor.Pulse(_listenLock);	
					}
				}
				lock(_sendLock)
				{
					Monitor.Wait(_sendLock);
				}
			}
		}
		
		public void Listen()
		{
			while(true)
			{
				if(sendCommanSyncQueue.Count > 0)
				{
					lock(_sendLock)
					{
						Monitor.Pulse(_sendLock);
					}
					lock(_listenLock)
					{
						Monitor.Wait(_listenLock);
					}
				}
				
				if(!redisStream.IsDataAvailable()) continue;
				string[] messageInfo = redisStream.HandleChannelMessage();
				if(VerifyChannelMessageData(messageInfo))
				{
					PubSubResponse response = (PubSubResponse)Enum.Parse(typeof(PubSubResponse), messageInfo[0]);
					switch(response)
					{
					case PubSubResponse.subscribe:
						OnSubscribe(this, new SubscriptionEventArgs(messageInfo[1]));
						break;
					case PubSubResponse.unsubscribe:
						OnUnsubscribe(this, new SubscriptionEventArgs(messageInfo[1]));
						break;
					case PubSubResponse.message:
						OnMessageReceived(this, new MessageEventArgs(messageInfo[1], messageInfo[2]));
						break;
					}
				}
			}
		}
		
		private bool VerifyChannelMessageData(string[] messageInfo)
		{
			if(messageInfo == null || messageInfo[0] == null || messageInfo[1] == null)
				return false;
			
			return true;
		}
		
		public void Destroy()
		{
			StopDataListener();
			StopDataSender();
			Dispose();
		}
		
		private void StopDataListener()
		{
			if(dataListener != null)
			{
				try
				{
					dataListener.Abort();
					dataListener = null;
				}
				catch(ThreadAbortException)
				{
					//Do nothing. Nothing to clean up.
				}
			}
		}
		
		private void StopDataSender()
		{
			if(dataSender != null)
			{
				try
				{
					dataSender.Abort();
					dataSender = null;
				}
				catch(ThreadAbortException)
				{
					//Do nothing. Nothing to clean up.
				}
			}
		}
	}
	
	internal class CommandInfo
	{
		private RedisCommand command;
		private string[] args;
		
		public CommandInfo(RedisCommand command, string[] args)
		{
			this.command = command;
			this.args = args;
		}
		
		public RedisCommand  GetCommand()
		{
			return command;	
		}
		
		public string[] GetArgs()
		{
			return args;	
		}
	}
}

