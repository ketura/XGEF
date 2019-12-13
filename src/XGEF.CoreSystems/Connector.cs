using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XGEF.Networking
{
	//public delegate void ServerAction(string info, IServer server, IClient client);

	//public interface IReceiver<TConnector>
	//{
	//	bool Receive(long key, string name, string info, TConnector sender);
	//}

	//public interface IConnector<TReceiver, TISelf> : IReceiver<TReceiver>
	//	where TReceiver : class, IReceiver<TISelf>
	//{
	//	bool Perform(string name, string info);
	//	bool PerformAndRaise(string name, string info);
	//	void Raise(string name, string info);

	//	bool Receive(long key, string name, string info, IReceiver<TISelf> sender);
	//	void Subscribe(string name, ServerAction del);
	//	void Unsubscribe(string name, ServerAction del);
	//}

	//public abstract class Connector<TReceiver, TISelf> : INetworkWrapper, IConnector<TReceiver, TISelf>
	//	where TReceiver : class, IReceiver<TISelf>
	//{
	//	protected Dictionary<string, ServerAction> EventTable { get; set; }

	//	public bool Receive(long key, string name, string info, IReceiver<TISelf> sender)
	//	{
	//		if (!(sender is TReceiver))
	//			throw new ArgumentException($"Argument 'sender' must be of type {typeof(TReceiver)}, but is type {sender.GetType()}.");

	//		return Receive(key, name, info, sender as TReceiver);
	//	}
	//	public abstract bool Receive(long key, string name, string info, TReceiver sender);
	//	public abstract bool Perform(string name, string info);
	//	public virtual bool PerformAndRaise(string name, string info)
	//	{
	//		Raise(name, info);
	//		return Perform(name, info);
	//	}
	//	public abstract void Subscribe(string name, ServerAction del);
	//	public abstract void Unsubscribe(string name, ServerAction del);
	//	public abstract void Raise(string name, string info);

	//	protected Connector()
	//	{
	//		EventTable = new Dictionary<string, ServerAction>();
	//	}
	//}
}
