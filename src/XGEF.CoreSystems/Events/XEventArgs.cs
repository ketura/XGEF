
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
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using XGEF.Core.Networking;

namespace XGEF.Core.Events
{
	public class XEventArgs : EventArgs
	{
		//ID of the client or server instance running the game/service that spawned the event
		[JsonIgnore]
		public ulong OriginatorID { get; set; }

		[JsonIgnore]
		public string EventName { get; set; }
		private int? _eventID { get; set; }

		public int EventID
		{
			get
			{
				if(!_eventID.HasValue)
				{
					_eventID = SystemLoader.GetSystem<CoreEventSystem>().GetEventID(EventName);
				}

				return _eventID.Value;
			}

			set
			{
				_eventID = value;
			}
		}
		[JsonIgnore]
		public bool SendOverNetwork { get; protected set; } = true;
		[JsonIgnore]
		public bool NetworkedEvent { get; protected set; } = false;
		[JsonIgnore]
		public bool Aborted { get; protected set; }
		[JsonIgnore]
		public string AbortReason { get; protected set; }
		[JsonIgnore]
		public object Aborter { get; protected set; }

		public Priority PriorityTier { get; set; }

		public void Abort(string reason, object aborter)
		{
			Aborted = true;
			AbortReason = reason;
			Aborter = aborter;
		}

		public void CancelAbort(string reason, object antiAborter)
		{
			Aborted = false;
			AbortReason = reason;
			Aborter = antiAborter;
		}

		public void SetNetworked()
		{
			if(SendOverNetwork)
			{
				NetworkedEvent = true;
			}
		}

		public static T DeserializeFromBitstream<T>(byte[] obj)
			where T : XEventArgs, new()
		{
			return DefaultBitstreamConverter.DeserializeJsonBitstream<T>(obj);
		}

		public virtual byte[] SerializeToBitstream()
		{
			return DefaultBitstreamConverter.SerializeJsonBitstream(this);
		}

		public XEventArgs() : this(false) { }
		public XEventArgs(bool networked)
		{
			EventName = "DefaultEvent";
			NetworkedEvent = networked;
		}
	}
}
