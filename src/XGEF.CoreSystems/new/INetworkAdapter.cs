
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
using System.Text;
using XGEF.Core.Events;

namespace XGEF.Core.Networking
{
	public enum ConnectionStatus
	{
		Idle,
		Connecting,
		Connected,
		TimedOut,
		Disconnected
	}

	public interface INetworkAdapter
	{
		ulong ID { get; }
		IPEndPoint FullIPAddress { get; }
		string HostName { get; }
		ushort ProtocolID { get; }
		byte[] PrivateKey { get; }
		ushort Timeout { get; }

		void SendMessage(XEventArgs data);
		void SendRawData(byte[] data);

		void Populate(ConnectionInfo info);

		event Action<INetworkAdapter, NetcodeMetadataPacket> OnMessageReceived;

		void Update();
		void Reset();
	}

	public interface IClientAdapter : INetworkAdapter
	{
		ConnectionStatus CurrentStatus { get; }
		IServerAdapter ServerAdapter { get; }
		void Connect(IPEndPoint ip);
		void Disconnect();
		event Action<ConnectionStatus> OnStateChanged;
		void ReceiveMessage(byte[] payload, int payloadSize);
	}

	public interface IServerAdapter : INetworkAdapter
	{
		int ServerSlots { get; }
		IEnumerable<IClientAdapter> ConnectedClients { get; }
		void Disconnect(IClientAdapter client);
		void DisconnectAll();
		void SendMessage(IClientAdapter client, XEventArgs data);
		void SendRawData(IClientAdapter client, byte[] data);
		event Action<IClientAdapter> OnClientConnected;
		event Action<IClientAdapter> OnClientDisconnected;
	}

	public struct ConnectionInfo : IEquatable<ConnectionInfo>
	{
		public ulong ID { get; set; }
		public ushort ProtocolID { get; set; }
		public byte[] PrivateKey { get; set; }
		public ushort Timeout { get; set; }

		public override bool Equals(object obj)
		{
			return obj is ConnectionInfo info && Equals(info);
		}

		public bool Equals(ConnectionInfo other)
		{
			return ID == other.ID &&
						 ProtocolID == other.ProtocolID &&
						 EqualityComparer<byte[]>.Default.Equals(PrivateKey, other.PrivateKey) &&
						 Timeout == other.Timeout;
		}

		public override int GetHashCode()
		{
			var hashCode = -1534566933;
			hashCode = hashCode * -1521134295 + ID.GetHashCode();
			hashCode = hashCode * -1521134295 + ProtocolID.GetHashCode();
			hashCode = hashCode * -1521134295 + EqualityComparer<byte[]>.Default.GetHashCode(PrivateKey);
			hashCode = hashCode * -1521134295 + Timeout.GetHashCode();
			return hashCode;
		}

		public static bool operator ==(ConnectionInfo left, ConnectionInfo right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(ConnectionInfo left, ConnectionInfo right)
		{
			return !(left == right);
		}
	}

	public static class NetworkAdapter
	{
		//https://stackoverflow.com/questions/415291/best-way-to-combine-two-or-more-byte-arrays-in-c-sharp
		public static byte[] SerializeNetworkInfo(this INetworkAdapter adapter)
		{
			List<byte[]> fields = new List<byte[]>
			{
				BitConverter.GetBytes(adapter.ID),
				BitConverter.GetBytes(adapter.ProtocolID),
				adapter.PrivateKey,
				BitConverter.GetBytes(adapter.Timeout)
			};

			byte[] info = new byte[fields.Sum(x => x.Length)];
			int offset = 0;
			
			foreach(byte[] field in fields)
			{
				Buffer.BlockCopy(field, 0, info, offset, field.Length);
				offset += field.Length;
			}

			return info;
		}

		public static ConnectionInfo DeserializeNetworkInfo(byte[] data)
		{
			ConnectionInfo info = new ConnectionInfo();

			int offset = 0;
			info.ID = BitConverter.ToUInt64(data, offset);

			offset += sizeof(ushort); //16-bit port
			info.ProtocolID = BitConverter.ToUInt16(data, offset);

			offset += sizeof(ushort);
			info.PrivateKey = data.Skip(offset).Take(16).ToArray();

			offset += 32; //256-bit private key
			info.Timeout = BitConverter.ToUInt16(data, offset);

			return info;
		}

		public static ConnectionInfo GetConnectionInfo(this INetworkAdapter adapter)
		{
			return new ConnectionInfo()
			{
				ID = adapter.ID,
				ProtocolID = adapter.ProtocolID,
				PrivateKey = adapter.PrivateKey,
				Timeout = adapter.Timeout
			};
		}
	}


}
