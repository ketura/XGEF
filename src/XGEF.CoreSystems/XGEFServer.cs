using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace XGEF.Networking
{
	

	//public interface IServer : IReceiver<IClient>, INetworkWrapper
	//{
	//	List<IClient> ConnectedClients { get; }
	//	bool Perform(string name, string info, IClient client);
	//	bool PerformAndSend(string name, string info, IClient client);
	//	long Connect(IClient client);
	//	bool Disconnect(IClient client);
	//	void Raise(string name, string info, IClient sender);
	//}

	//public class XGEFServer : Connector<IClient, IServer>, IServer
	//{
	//	public List<IClient> ConnectedClients { get; set; }

	//	protected long SecretID { get; set; }

	//	public long Connect(IClient client)
	//	{
	//		if (!ConnectedClients.Contains(client))
	//		{
	//			ConnectedClients.Add(client);
	//			Raise("Connect", null);
	//			return SecretID;
	//		}

	//		return 0;
	//	}

	//	public bool Disconnect(IClient client)
	//	{
	//		if (ConnectedClients.Contains(client))
	//		{
	//			ConnectedClients.Remove(client);
	//			Raise("Disconnect", null);
	//			return true;
	//		}

	//		return false;
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
	//		name = name.ToLower();

	//		foreach (var client in ConnectedClients)
	//		{
	//			EventTable[name]?.Invoke(info, this, client);
	//		}
	//	}

	//	public void Raise(string name, string info, IClient sender)
	//	{
	//		name = name.ToLower();

	//		EventTable[name]?.Invoke(info, this, sender);
	//	}

	//	public override bool Perform(string name, string info)
	//	{
	//		name = name.ToLower();

	//		bool response = true;
	//		foreach (var client in ConnectedClients)
	//		{
	//			response = client.Receive(SecretID, name, info, this);
	//		}
	//		return response;
	//	}

	//	public bool Perform(string name, string info, IClient client)
	//	{
	//		name = name.ToLower();

	//		return client.Receive(SecretID, name, info, this);
	//	}

	//	public bool PerformAndSend(string name, string info, IClient client)
	//	{
	//		Raise(name, info);
	//		return Perform(name, info, client);
	//	}

	//	public override bool Receive(long key, string name, string info, IClient client)
	//	{
	//		name = name.ToLower();

	//		if (key != SecretID)
	//			return false;
	//		if (String.IsNullOrEmpty(name))
	//			return false;

	//		Raise(name, info, client);
	//		return true;
	//	}

		

	//	public XGEFServer() : base()
	//	{
	//		while (SecretID == 0)
	//		{
	//			SecretID = new Random(Guid.NewGuid().GetHashCode()).Next();
	//		}

	//		ConnectedClients = new List<IClient>();
	//	}
	//}
}
