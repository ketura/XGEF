
///////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////
////                                                                               ////
////    Copyright 2017-2018 Christian 'ketura' McCarty                             ////
////                                                                               ////
////    Licensed under the Apache License, Version 2.0 (the "License");            ////
////    you may not use this file except in compliance with the License.           ////
////    You may obtain a copy of the License at                                    ////
////                                                                               ////
////                http://www.apache.org/licenses/LICENSE-2.0                     ////
////                                                                               ////
////    Unless required by applicable law or agreed to in writing, software        ////
////    distributed under the License is distributed on an "AS IS" BASIS,          ////
////    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.   ////
////    See the License for the specific language governing permissions and        ////
////    limitations under the License.                                             ////
////                                                                               ////
///////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Net;

using NetcodeIO.NET;

using XGEF.Core.Events;


namespace XGEF.Core.Networking
{

	public class NetcodeIOClientAdapter : IClientAdapter
	{
		public ulong ID { get; private set; }
		public ushort ProtocolID { get; private set; }
		public byte[] PrivateKey { get; private set; }
		public IPEndPoint FullIPAddress { get; private set; }
		public string HostName { get; private set; }
		public ushort Timeout { get; set; }
		public int TokenLifetime { get; private set; }
		public ConnectionStatus CurrentStatus { get; private set; }

		public IPEndPoint Server { get; protected set; }

		public event Action<INetworkAdapter, NetcodeMetadataPacket> OnMessageReceived;
		public event Action<ConnectionStatus> OnStateChanged;

		protected NetcodeIO.NET.Client Client { get; set; }

		public IServerAdapter ServerAdapter => throw new NotImplementedException();

		public virtual void ReceiveMessage(byte[] payload, int payloadSize)
		{
			var packet = new NetcodeMetadataPacket(payload.Take(payloadSize).ToArray());
			OnMessageReceived?.Invoke(this, packet);
		}

		public virtual void SendMessage(XEventArgs data)
		{
			data.OriginatorID = ID;
			var packet = new NetcodeMetadataPacket(data);
			SendRawData(packet.ConsolidateBytes());
		}

		public virtual void SendRawData(byte[] data)
		{
			Client.Send(data, data.Length);
		}

		public virtual void StateChanged(ClientState state)
		{
			switch (state)
			{
				case ClientState.ConnectionTimedOut:
				case ClientState.ChallengeResponseTimedOut:
				case ClientState.ConnectionRequestTimedOut:
					CurrentStatus = ConnectionStatus.TimedOut;
					Server = null;
					break;
				case ClientState.ConnectTokenExpired:
				case ClientState.InvalidConnectToken:
				case ClientState.ConnectionDenied:
				case ClientState.Disconnected:
					CurrentStatus = ConnectionStatus.Disconnected;
					Server = null;
					break;
				case ClientState.SendingConnectionRequest:
				case ClientState.SendingChallengeResponse:
					CurrentStatus = ConnectionStatus.Connecting;
					break;
				case ClientState.Connected:
					CurrentStatus = ConnectionStatus.Connected;
					ID = (ushort)Client.ClientIndex;
					break;
			}

			OnStateChanged?.Invoke(CurrentStatus);
		}

		public virtual void Connect(IPEndPoint ip)
		{
			if(ip == null)
			{
				throw new ArgumentNullException("Client cannot connect to null server!");
			}

			TokenFactory factory = new TokenFactory(ProtocolID, PrivateKey);
			var serverList = new IPEndPoint[] 
				{
					ip,
					new IPEndPoint(IPAddress.Any, ip.Port),
					new IPEndPoint(IPAddress.Loopback, ip.Port)
				};
			var token = factory.GenerateConnectToken(serverList, TokenLifetime, Timeout, 1, 1, this.SerializeNetworkInfo());
			Client.Connect(token);
			Server = ip;
		}

		public virtual void Disconnect()
		{
			Client.Disconnect();
			Server = null;
		}

		public virtual void Update() { } 

		public virtual void Populate(ConnectionInfo info)
		{
			ID = info.ID;
			ProtocolID = info.ProtocolID;
			PrivateKey = info.PrivateKey;
			Timeout = info.Timeout;
		}

		public virtual void Reset() { }

		public NetcodeIOClientAdapter() : this(CoreNetworkSystem.DefaultClientIPAddress, CoreNetworkSystem.DefaultClientPort) { }
		public NetcodeIOClientAdapter(byte[] publicIP, ushort port) : this(new IPAddress(publicIP), port) { }
		public NetcodeIOClientAdapter(string publicIP, ushort port) : this(IPAddress.Parse(publicIP), port) { }
		public NetcodeIOClientAdapter(IPAddress publicIP, ushort port)
		{
			Client = new Client()
			{
				Tickrate = CoreNetworkSystem.DefaultClientTickRate
			};

			Client.OnMessageReceived += ReceiveMessage;
			Client.OnStateChanged += StateChanged;

			ID = CoreNetworkSystem.GenerateNetworkID();
			ProtocolID = CoreNetworkSystem.ProtocolID;
			PrivateKey = CoreNetworkSystem.PrivateKey;
			Timeout = CoreNetworkSystem.DefaultClientTimeout;
			TokenLifetime = CoreNetworkSystem.TokenLifetime;

			FullIPAddress = new IPEndPoint(publicIP, port);
			HostName = CoreNetworkSystem.DefaultClientHostname;
			CurrentStatus = ConnectionStatus.Idle;
		}
	}

	public class NetcodeIORemoteClientAdapter : IClientAdapter
	{
		public ConnectionStatus CurrentStatus { get; private set; }
		public ulong ID { get; private set; }
		public ushort ProtocolID { get; private set; }
		public byte[] PrivateKey { get; private set; }
		public IPEndPoint FullIPAddress { get; private set; }
		public string HostName { get; private set; }
		public ushort Timeout { get; set; }
		public int TokenLifetime { get; private set; }

		public IServerAdapter ServerAdapter { get; protected set; }
		internal RemoteClient Client { get; set; }
#pragma warning disable CS0067 // event is never used
		public event Action<INetworkAdapter, NetcodeMetadataPacket> OnMessageReceived;
		public event Action<ConnectionStatus> OnStateChanged;
#pragma warning restore CS0067 // event is never used

		void IClientAdapter.Connect(IPEndPoint ip)
		{
			throw new NotImplementedException("Remote client should not be directly invoked to connect.");
		}

		public void Disconnect()
		{
			ServerAdapter.Disconnect(this);
		}

		void IClientAdapter.ReceiveMessage(byte[] payload, int payloadSize)
		{
			throw new NotImplementedException("Remote client should not be directly invoked to receive messages.");
		}

		void INetworkAdapter.SendMessage(XEventArgs data)
		{
			throw new NotImplementedException("Remote client should not be directly invoked to send messages.");
		}

		void INetworkAdapter.SendRawData(byte[] data)
		{
			throw new NotImplementedException("Remote client should not be directly invoked to send messages.");
		}

		public void Update() { }

		public void Populate(ConnectionInfo info)
		{
			ID = info.ID;
			ProtocolID = info.ProtocolID;
			PrivateKey = info.PrivateKey;
			Timeout = info.Timeout;
		}

		public void Reset()
		{
			throw new NotImplementedException();
		}

		public NetcodeIORemoteClientAdapter(RemoteClient client, NetcodeIOServerAdapter server)
		{
			Client = client;
			ServerAdapter = server;

			if(Client.RemoteEndpoint is DnsEndPoint dnsEndpoint)
			{
				HostName = dnsEndpoint.Host;
				IPAddress address = Dns.GetHostAddresses(dnsEndpoint.Host).FirstOrDefault();
				if(address == null)
				{
					address = CoreNetworkSystem.DefaultClientIPAddress;
				}
				FullIPAddress = new IPEndPoint(address, (ushort)dnsEndpoint.Port);
			}
			else if(Client.RemoteEndpoint is IPEndPoint ipEndpoint)
			{
				FullIPAddress = ipEndpoint;
				HostName = ipEndpoint.ToString();
			}

			Populate(NetworkAdapter.DeserializeNetworkInfo(client.UserData));
		}
	}

	public class NetcodeIOServerAdapter : IServerAdapter
	{
		protected NetcodeIO.NET.Server Server { get; set; }

		public IEnumerable<IClientAdapter> ConnectedClients { get { return Clients.Keys; } }
		protected ConcurrentDictionary<NetcodeIORemoteClientAdapter, long> Clients { get; set; }

		public ulong ID { get; private set; }
		public int ServerSlots { get; private set; }
		public ushort ProtocolID { get; private set; }
		public byte[] PrivateKey { get; private set; }
		public IPEndPoint FullIPAddress { get; private set; }
		public string HostName { get; private set; }
		public ushort Timeout { get; set; }
		public int TokenLifetime { get; private set; }

		public event Action<INetworkAdapter, NetcodeMetadataPacket> OnMessageReceived;
		public event Action<IClientAdapter> OnClientConnected;
		public event Action<IClientAdapter> OnClientDisconnected;

		public virtual NetcodeIORemoteClientAdapter FindByID(ulong id)
		{
			return Clients.Where(x => x.Key.ID == id).FirstOrDefault().Key;
		}

		public virtual IEnumerable<NetcodeIORemoteClientAdapter> FindAllByID(ulong id)
		{
			return Clients.Where(x => x.Key.ID == id).Select(x => x.Key).ToList();
		}

		public virtual NetcodeIORemoteClientAdapter FindByIP(IPEndPoint ip)
		{
			return Clients.Where(x => ip.Address == x.Key.FullIPAddress.Address && ip.Port == x.Key.FullIPAddress.Port).FirstOrDefault().Key;
		}

		public virtual IEnumerable<NetcodeIORemoteClientAdapter> FindAllByIP(IPEndPoint ip)
		{
			return Clients.Where(x => ip.Address == x.Key.FullIPAddress.Address && ip.Port == x.Key.FullIPAddress.Port).Select(x => x.Key).ToList();
		}

		public virtual NetcodeIORemoteClientAdapter FindByHost(string hostname, ushort port)
		{
			return Clients.Where(x => x.Key.HostName.Equals(hostname) && port == x.Key.FullIPAddress.Port).FirstOrDefault().Key;
		}

		public virtual IEnumerable<NetcodeIORemoteClientAdapter> FindAllByHost(string hostname, ushort port)
		{
			return Clients.Where(x => x.Key.HostName.Equals(hostname) && port == x.Key.FullIPAddress.Port).Select(x => x.Key).ToList();
		}

		public virtual NetcodeIORemoteClientAdapter MatchAdapter(IClientAdapter adapter)
		{
			return Clients.Where(x => x.Key.FullIPAddress.Port == adapter.FullIPAddress.Port
												&& (x.Key.HostName.Equals(adapter.HostName) || x.Key.FullIPAddress.Address == adapter.FullIPAddress.Address))
												.FirstOrDefault().Key;
		}

		public virtual NetcodeIORemoteClientAdapter MatchRemote(RemoteClient client)
		{
			if (client.RemoteEndpoint is DnsEndPoint dnsEndpoint)
			{
				return Clients.Where(x => x.Key.FullIPAddress.Port == dnsEndpoint.Port && x.Key.HostName == dnsEndpoint.Host).FirstOrDefault().Key;
			}
			else if (client.RemoteEndpoint is IPEndPoint ipEndpoint)
			{
				return Clients.Where(x => x.Key.FullIPAddress.Port == ipEndpoint.Port && x.Key.FullIPAddress.Address == ipEndpoint.Address).FirstOrDefault().Key;
			}

			return null;
		}

		public virtual IEnumerable<NetcodeIORemoteClientAdapter> MatchAllRemote(IClientAdapter adapter)
		{
			return Clients.Where(x => x.Key.FullIPAddress.Port == adapter.FullIPAddress.Port
												&& (x.Key.HostName.Equals(adapter.HostName) || x.Key.FullIPAddress.Address == adapter.FullIPAddress.Address)).Select(x => x.Key).ToList();
		}

		public virtual IClientAdapter Match(IClientAdapter adapter)
		{
			return MatchAdapter(adapter);
		}

		public virtual IEnumerable<IClientAdapter> MatchAll(IClientAdapter adapter)
		{
			return MatchAllRemote(adapter);
		}

		public virtual void ReceiveMessage(RemoteClient sender, byte[] payload, int payloadSize)
		{
			ReceiveMessage(MatchRemote(sender), payload, payloadSize);
		}

		public virtual void ReceiveMessage(IClientAdapter adapter, byte[] payload, int payloadSize)
		{
			var packet = new NetcodeMetadataPacket(payload.Take(payloadSize).ToArray());
			OnMessageReceived?.Invoke(adapter, packet);
		}

		public virtual void Disconnect(IClientAdapter client)
		{
			var connectedClients = FindAllByID(client.ID);
			foreach(var connectedClient in connectedClients)
			{
				Server.Disconnect(connectedClient.Client);
			}
		}

		public virtual void DisconnectAll()
		{
			foreach(var client in Clients.Keys)
			{
				Disconnect(client);
			}
		}

		public virtual void Reset() { }

		public virtual void ClientConnected(RemoteClient client)
		{
			var clientAdapter = new NetcodeIORemoteClientAdapter(client, this);
			var clientsWithIP = MatchAllRemote(clientAdapter);
			if(clientsWithIP.Any())
			{
				foreach (var existingClient in clientsWithIP)
				{
					existingClient.Disconnect();
				}
			}

			Clients.AddOrUpdate(clientAdapter, DateTime.Now.Ticks, (x, y) => { return DateTime.Now.Ticks; });
			OnClientConnected?.Invoke(clientAdapter);
		}

		public virtual void ClientDisconnected(RemoteClient client)
		{
			var clientAdapter = new NetcodeIORemoteClientAdapter(client, this);
			var clientsWithIP = MatchAllRemote(clientAdapter);

			if (clientsWithIP.Any())
			{
				foreach (var existingClient in clientsWithIP)
				{
					bool result = Clients.TryRemove(existingClient, out long removed);
				}
			}
			
			OnClientDisconnected?.Invoke(clientAdapter);
		}

		

		

		public virtual void SendMessage(XEventArgs data)
		{
			data.OriginatorID = ID;
			var packet = new NetcodeMetadataPacket(data);
			SendRawData(packet.ConsolidateBytes());
		}

		//TODO: bring this to parity with the all-client version
		public virtual void SendMessage(IClientAdapter client, XEventArgs data)
		{
			data.OriginatorID = ID;
			SendRawData(client, data.SerializeToBitstream());
		}

		public virtual void SendRawData(byte[] data)
		{
			foreach (var adapter in Clients)
			{
				Server.SendPayload(adapter.Key.Client, data, data.Length);
			}
		}

		public virtual void SendRawData(IClientAdapter client, byte[] data)
		{
			Server.SendPayload(MatchAdapter(client).Client, data, data.Length);
		}

		public virtual void Update() { }

		public virtual void Populate(ConnectionInfo info)
		{
			ID = info.ID;
			ProtocolID = info.ProtocolID;
			PrivateKey = info.PrivateKey;
			Timeout = info.Timeout;
		}

		public NetcodeIOServerAdapter() : this(CoreNetworkSystem.DefaultServerIPAddress, CoreNetworkSystem.DefaultServerPort) { }
		public NetcodeIOServerAdapter(byte[] publicIP, ushort port) : this(new IPAddress(publicIP), port) { }
		public NetcodeIOServerAdapter(string publicIP, ushort port) : this(IPAddress.Parse(publicIP), port) { }
		public NetcodeIOServerAdapter(IPAddress publicIP, ushort port)
		{
			Clients = new ConcurrentDictionary<NetcodeIORemoteClientAdapter, long>();

			ID = CoreNetworkSystem.GenerateNetworkID();
			ProtocolID = CoreNetworkSystem.ProtocolID;
			PrivateKey = CoreNetworkSystem.PrivateKey;
			Timeout = CoreNetworkSystem.DefaultClientTimeout;
			TokenLifetime = CoreNetworkSystem.TokenLifetime;

			ServerSlots = CoreNetworkSystem.DefaultMaxClients;
			FullIPAddress = new IPEndPoint(publicIP, port);
			HostName = CoreNetworkSystem.DefaultClientHostname;

			Server = new Server(ServerSlots, FullIPAddress.Address.ToString(), FullIPAddress.Port, ProtocolID, PrivateKey)
			{
				LogLevel = NetcodeLogLevel.Debug,
				Tickrate = CoreNetworkSystem.DefaultServerTickRate
			};
			Server.Start();

			Server.OnClientMessageReceived += ReceiveMessage;
			Server.OnClientConnected += ClientConnected;
			Server.OnClientDisconnected += ClientDisconnected;
		}
	}
}
