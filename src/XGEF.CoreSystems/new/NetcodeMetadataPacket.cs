
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
using System.Text;

using XGEF.Core.Events;

namespace XGEF.Core.Networking
{
	public struct NetcodeMetadataPacket
	{
		public bool MetadataDefined { get; set; }
		public ulong OriginatorID { get; set; }
		public int EventID { get; set; }
		public byte EventNameLength { get; set; }
		public string EventName { get; set; }
		public byte[] Payload { get; set; }

		public byte[] ConsolidateBytes()
		{
			List<byte[]> fields = new List<byte[]>
			{
				BitConverter.GetBytes(MetadataDefined),
				BitConverter.GetBytes(OriginatorID),
				BitConverter.GetBytes(EventID),
				new byte[] { EventNameLength },
				Encoding.UTF8.GetBytes(EventName),
				Payload
			};

			return CommonUtils.ConsolidateBitstreams(fields);
		}

		public NetcodeMetadataPacket(XEventArgs args)
		{
			if (args == null)
			{
				MetadataDefined = false;
				OriginatorID = 0;
				EventID = -1;
				EventNameLength = 0;
				EventName = "";
				Payload = new byte[0];
			}
			else
			{
				MetadataDefined = true;
				OriginatorID = args.OriginatorID;
				EventID = args.EventID;
				EventNameLength = (byte)Encoding.UTF8.GetByteCount(args.EventName);
				EventName = args.EventName;
				Payload = args.SerializeToBitstream();
			}
		}

		public NetcodeMetadataPacket(byte[] data)
		{
			int offset = 0;
			MetadataDefined = BitConverter.ToBoolean(data, offset);
			offset++;
			if(MetadataDefined)
			{
				OriginatorID = BitConverter.ToUInt64(data, offset);
				offset += sizeof(ulong);
				EventID = BitConverter.ToInt32(data, offset);
				offset += sizeof(int);
				EventNameLength = data[offset];
				offset++;
				EventName = Encoding.UTF8.GetString(data, offset, EventNameLength);
				offset += EventNameLength;
				Payload = data.Skip(offset).ToArray();
			}
			else
			{
				OriginatorID = 0;
				EventID = -1;
				EventNameLength = 0;
				EventName = "";
				Payload = new byte[0];
			}
		}
	}
}
