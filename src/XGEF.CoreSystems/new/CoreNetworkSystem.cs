
///////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////
////                                                                               ////
////    Copyright 2017 Christian 'ketura' McCarty                                  ////
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
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using XGEF;
using XGEF.Core.Events;

namespace XGEF.Core.Networking
{



	public abstract class ModNetworkSystemBase : ModSystem<CoreNetworkSystem>
	{
		internal IClientAdapter ClientAdapter { get; set; }
		internal IServerAdapter ServerAdapter { get; set; }

		protected void SetClient(IClientAdapter adapter)
		{
			ClientAdapter = adapter;
		}

		protected void SetServer(IServerAdapter adapter)
		{
			ServerAdapter = adapter;
		}

		public virtual void Connect(IPAddress publicIP, ushort port)
		{
			if (CoreSystem.IsServer)
			{
				throw new InvalidOperationException("This instance of CoreNetworkSystem is a server!  It cannot be told to connect to a remote client; the client must be told to connect here.");
			}

			ClientAdapter.Connect(new IPEndPoint(publicIP, port));
		}

		protected ModNetworkSystemBase() : base() { }
	}

	public class CoreNetworkSystem : CoreSystem<ModNetworkSystemBase>
	{
		private static ulong NextID = 0;
		public static ulong GenerateNetworkID()
		{
			return NextID++;
		}

		//increment this manually every time we have a breaking change to the protocol
		public static ushort ProtocolID { get; } = 1;
		public static readonly byte[] PrivateKey;
		public static readonly int TokenLifetime;
		public static readonly int DefaultMaxClients;
		public static readonly IPAddress DefaultServerIPAddress;
		public static readonly IPAddress DefaultClientIPAddress;
		public static readonly string DefaultServerHostname;
		public static readonly string DefaultClientHostname;
		public static readonly ushort DefaultServerPort;
		public static readonly ushort DefaultClientPort;
		public static readonly ushort DefaultServerTimeout;
		public static readonly ushort DefaultClientTimeout;
		public static readonly int DefaultServerTickRate;
		public static readonly int DefaultClientTickRate;
		public static readonly int UpdateInterval;
		static CoreNetworkSystem()
		{
			string password = @"{Q<M;!#+/yQk82[lhT1=yfWrg8p3uQ[@";
			byte[] Salt =	new byte[] { 40, 77, 52, 22, 55, 78, 08, 20 };
			int Iterations = 300;

			var keyGenerator = new Rfc2898DeriveBytes(password, Salt, Iterations);
			//256 bit key
			PrivateKey = keyGenerator.GetBytes(32);

			//24 hours in seconds
			TokenLifetime = 86_400;

			DefaultMaxClients = 5;

			///TODO: Load these values from config at some point
			//DefaultServerIPAddress = new IPAddress(new byte[] { 127, 0, 0, 1 });
			DefaultServerIPAddress = IPAddress.Any;
			//108.81.83.60
			DefaultClientIPAddress = new IPAddress(new byte[] { 127, 0, 0, 1 });
			DefaultServerHostname = "localhost";
			DefaultClientHostname = "localhost";
			DefaultServerPort = 1337;
			DefaultClientPort = 1338;
			DefaultServerTimeout = 30;
			DefaultClientTimeout = 30;
			DefaultServerTickRate = 100;
			DefaultClientTickRate = 100;

			UpdateInterval = 1;

			//If this is ever updated, then the netcode adapters need to be altered at the same time
			ProtocolID = 1;
		}

		protected bool _isServer { get; set; }
		public bool IsServer
		{
			get { return _isServer; }
			set
			{
				if(Initialized)
				{
					throw new InvalidOperationException("The CoreNetworkSystem has already been initialized!  Whether or not it's a server can no longer be changed.");
				}

				_isServer = value;
			}
		}

		public IPEndPoint CurrentIPAddress
		{
			get
			{
				if(IsServer)
				{
					return ModSystem?.ServerAdapter.FullIPAddress;
				}
				else
				{
					return ModSystem?.ClientAdapter.FullIPAddress;
				}
			}
		}

		public CoreEventSystem EventSystem { get; private set; }
		protected int ConnectedEventID { get; set; }
		protected int DisconnectedEventID { get; set; }

		public void ReceiveMessage(INetworkAdapter adapter, NetcodeMetadataPacket data)
		{
			Manager.Debug($"Event packet {data.EventID} received.");
			if(EventSystem.EventIDExists(data.EventID))
			{
				EventSystem.Invoke(adapter, data.EventID, data.Payload, true);
			}
			else if(EventSystem.EventExists(data.EventName))
			{
				EventSystem.Invoke(adapter, data.EventID, data.Payload, true);
			}
			else
			{
				Manager.Warn($"CoreNetworkSystem received an unregistered event: {data.EventID}, {data.EventName}.");
			}
			
		}

		public void SendMessage(XEventArgs data)
		{
			
			if(IsServer)
			{
				if(ModSystem.ServerAdapter.ConnectedClients.Any())
				{
					Manager.Debug($"Server sending packet {data.EventID}: {data.EventName}.");
					ModSystem.ServerAdapter.SendMessage(data);
				}
			}
			else 
			{
				if (ModSystem.ClientAdapter.CurrentStatus == ConnectionStatus.Connected)
				{
					Manager.Debug($"Client sending packet {data.EventID}: {data.EventName}.");
					ModSystem.ClientAdapter.SendMessage(data);
				}
			}
			
		}

		public void SendRawData(byte[] data)
		{
			Manager.Debug($"Sending raw packet of length {data.Length}.");
			if (IsServer)
			{
				ModSystem.ServerAdapter.SendRawData(data);
			}
			else
			{
				ModSystem.ClientAdapter.SendRawData(data);
			}
		}

		public override void PreInit()
		{
			base.PreInit();
			EventSystem = Manager.GetSystem<CoreEventSystem>();
			EventSystem.RegisterEvent<ClientConnectEventArgs>();
			EventSystem.RegisterEvent<ClientDisconnectEventArgs>();
			EventSystem.RegisterEvent<MessageEventArgs>();


		}

		public override void Init()
		{
			base.Init();

			if (IsServer)
			{
				ModSystem.ServerAdapter.OnMessageReceived += ReceiveMessage;
				ModSystem.ServerAdapter.OnClientConnected += OnClientConnect;
				ModSystem.ServerAdapter.OnClientDisconnected += OnClientDisconnect;
			}
			else
			{
				ModSystem.ClientAdapter.OnStateChanged += OnStatusChanged;
				ModSystem.ClientAdapter.OnMessageReceived += ReceiveMessage;
			}
		}

		public override void PostInit()
		{
			base.PostInit();

			ConnectedEventID = EventSystem.GetEventID(new ClientConnectEventArgs().EventName);
			DisconnectedEventID = EventSystem.GetEventID(new ClientDisconnectEventArgs().EventName);
		}

		private long lastUpdate; 

		public override void Process()
		{
			base.Process();

			long time = DateTime.Now.Ticks;
			if(time - lastUpdate > UpdateInterval)
			{
				if (IsServer)
				{
					ModSystem.ServerAdapter.Update();
				}
				else
				{
					ModSystem.ClientAdapter.Update();
				}
			}

			lastUpdate = time;

		}


		public void Connect()
		{
			Connect(DefaultServerIPAddress, DefaultServerPort);
		}
		public void Connect(byte[] publicIP, ushort port)
		{
			Connect(new IPAddress(publicIP), port);
		}
		public void Connect(string publicIP, ushort port)
		{
			Connect(IPAddress.Parse(publicIP), port);
		}
		public void Connect(IPAddress publicIP, ushort port)
		{
			if (IsServer)
			{
				throw new InvalidOperationException("This instance of CoreNetworkSystem is a server!  It cannot be told to connect to a remote client; the client must be told to connect here.");
			}

			ModSystem.Connect(publicIP, port);
		}

		public void Disconnect()
		{
			if(!IsServer)
			{
				ModSystem.ClientAdapter.Disconnect();
			}
			else
			{
				ModSystem.ServerAdapter.DisconnectAll();
			}
		}

		public void OnStatusChanged(ConnectionStatus status)
		{
			Manager.Debug($"Client status changed: {status}");
			switch (status)
			{
				case ConnectionStatus.Idle:
					break;
				case ConnectionStatus.Connecting:
					break;
				case ConnectionStatus.Connected:
					EventSystem.Invoke(this, "ClientConnectEvent");
					break;
				case ConnectionStatus.TimedOut:
					break;
				case ConnectionStatus.Disconnected:
					EventSystem.Invoke(this, "ClientDisconnectEvent");
					break;
				default:
					break;
			}
		}

		public void OnClientConnect(IClientAdapter client)
		{
			Manager.Debug($"Client connected: {client.FullIPAddress.ToString()}");
			EventSystem.InvokeBlocking(this, new ClientConnectEventArgs(client.FullIPAddress));
			if (IsServer)
			{
				ModSystem.ServerAdapter.Reset();
			}
			else
			{
				ModSystem.ClientAdapter.Reset();
			}
		}

		public void OnClientDisconnect(IClientAdapter client)
		{
			Manager.Debug($"Client disconnected: {client.FullIPAddress.ToString()}");
			EventSystem.InvokeBlocking(this, new ClientDisconnectEventArgs(client.FullIPAddress));
			if (IsServer)
			{
				ModSystem.ServerAdapter.Reset();
			}
			else
			{
				ModSystem.ClientAdapter.Reset();
			}
		}


		public CoreNetworkSystem() : this(true) { }
		public CoreNetworkSystem(bool server)
		{
			Name = "CoreNetworkSystem";
			//this field should be superceded by the systems.cs file.
			ModdedSystemPath = "Engine/Networking/NetworkSystem.cs";
			ModdedSystemName = "NetworkSystem";

			IsServer = server;
		}
	}

	


}
