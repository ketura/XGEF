
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
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using ReliableNetcode;
using NetcodeIO.NET;

using XGEF.Core.Events;


namespace XGEF.Core.Networking
{
	public class ReliableIOClientAdapter : NetcodeIOClientAdapter
	{
		protected ReliableEndpoint Endpoint { get; set; }
		protected QosType Reliability { get; set; }

		//public void NetcodeReceiveMessage(byte[] payload, int payloadSize)
		//{
		//	Endpoint.ReceivePacket(payload, payloadSize);
		//}

		//public void ReliableReceive(byte[] data, int size)
		//{
		//	ReceiveMessage(data, size);
		//}

		public override void SendRawData(byte[] data)
		{
			if (CurrentStatus != ConnectionStatus.Connected)
				return;

			Endpoint.SendMessage(data, data.Length, Reliability);
			//Endpoint.Update();
		}

		public void ReliableTransmit(byte[] data, int size)
		{
			Client.Send(data.Take(size).ToArray(), size);
		}

		public override void Update()
		{
			if (CurrentStatus != ConnectionStatus.Connected)
				return;

			base.Update();
			Endpoint.Update();
		}

		public override void Reset()
		{
			base.Reset();
			Endpoint.Reset();
		}

		public ReliableIOClientAdapter() : this(CoreNetworkSystem.DefaultClientIPAddress, CoreNetworkSystem.DefaultClientPort) { }
		public ReliableIOClientAdapter(byte[] publicIP, ushort port) : this(new IPAddress(publicIP), port) { }
		public ReliableIOClientAdapter(string publicIP, ushort port) : this(IPAddress.Parse(publicIP), port) { }
		public ReliableIOClientAdapter(IPAddress publicIP, ushort port) : base(publicIP, port)
		{
			Endpoint = new ReliableEndpoint
			{
				ReceiveCallback = ReceiveMessage,
				TransmitCallback = ReliableTransmit
			};

			Reliability = QosType.UnreliableOrdered;

			Client.OnMessageReceived -= ReceiveMessage;
			Client.OnMessageReceived += Endpoint.ReceivePacket;
		}
	}


	public class ReliableIOServerAdapter : NetcodeIOServerAdapter
	{
		protected ReliableEndpoint Endpoint { get; set; }
		protected QosType Reliability { get; set; }

		public void NetcodeReceiveMessage(RemoteClient sender, byte[] payload, int payloadSize)
		{
			Endpoint.ReceivePacket(payload.Take(payloadSize).ToArray(), payloadSize);
		}

		public override void SendRawData(byte[] data)
		{
			foreach (var adapter in Clients)
			{
				Endpoint.SendMessage(data, data.Length, Reliability);
			}
		}

		public void ReliableReceive(byte[] data, int size)
		{
			ulong id = BitConverter.ToUInt64(data, 0);
			ReceiveMessage(FindByID(id), data, size);
		}

		public void ReliableTransmit(byte[] data, int size)
		{
			//TODO: change this to read the packet for client target?
			foreach (var adapter in Clients)
			{
				Server.SendPayload(adapter.Key.Client, data, size);
			}
		}

		public override void Reset()
		{
			base.Reset();
			Endpoint.Reset();
		}

		public override void Update()
		{
			base.Update();
			Endpoint.Update();
		}

		public ReliableIOServerAdapter() : this(CoreNetworkSystem.DefaultServerIPAddress, CoreNetworkSystem.DefaultServerPort) { }
		public ReliableIOServerAdapter(byte[] publicIP, ushort port) : this(new IPAddress(publicIP), port) { }
		public ReliableIOServerAdapter(string publicIP, ushort port) : this(IPAddress.Parse(publicIP), port) { }
		public ReliableIOServerAdapter(IPAddress publicIP, ushort port) : base(publicIP, port)
		{
			Endpoint = new ReliableEndpoint
			{
				ReceiveCallback = ReliableReceive,
				TransmitCallback = ReliableTransmit
			};

			Reliability = QosType.UnreliableOrdered;

			Server.OnClientMessageReceived -= ReceiveMessage;
			Server.OnClientMessageReceived += NetcodeReceiveMessage;
		}
	}
}
