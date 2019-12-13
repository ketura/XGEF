using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace XGEF.Networking
{
	//public interface IClient : IReceiver<IServer>, INetworkWrapper
	//{
	//	IServer ConnectedServer { get; }
	//	bool Connect(IServer server);
	//	bool Disconnect();
	//	void Raise(string name, string info, IServer sender);
	//}

	//public class XGEFClient : Connector<IServer, IClient>, IClient
	//{
	//	public IServer ConnectedServer { get; protected set; }
	//	public long Key { get; protected set; }

	//	public bool Connect(IServer server)
	//	{
	//		ConnectedServer = server;
	//		Key = server.Connect(this);
	//		Raise("Connect", null);
	//		return Key != 0;
	//	}

	//	public bool Disconnect()
	//	{
	//		if (ConnectedServer == null)
	//			return false;

	//		ConnectedServer.Disconnect(this);
	//		Raise("Disconnect", null);
	//		return true;
	//	}

	//	public override void Subscribe(string name, ServerAction del)
	//	{
	//		name = name.ToLower();

	//		if (!EventTable.ContainsKey(name))
	//		{
	//			EventTable.Add(name, null);
	//		}

	//		EventTable[name] = EventTable[name] + del;
	//	}

	//	public override void Unsubscribe(string name, ServerAction del)
	//	{
	//		name = name.ToLower();

	//		if (!EventTable.ContainsKey(name))
	//		{
	//			return;
	//		}

	//		EventTable[name] = EventTable[name] - del;
	//	}

	//	public override void Raise(string name, string info)
	//	{
	//		Raise(name, info, ConnectedServer);
	//	}

	//	public void Raise(string name, string info, IServer sender)
	//	{
	//		name = name.ToLower();

	//		EventTable[name]?.Invoke(info, sender, this);
	//	}

	//	public override bool Receive(long key, string name, string info, IServer sender)
	//	{
	//		name = name.ToLower();

	//		if (key != Key)
	//			return false;
	//		if (String.IsNullOrEmpty(name))
	//			return false;

	//		Raise(name, info, sender);

	//		return key == Key;
	//	}

	//	public override bool Perform(string name, string info)
	//	{
	//		name = name.ToLower();

	//		if (ConnectedServer == null)
	//			return false;

	//		return ConnectedServer.Receive(Key, name, info, this);
	//	}

	//	public XGEFClient() : base() { }
		
	//}

}
