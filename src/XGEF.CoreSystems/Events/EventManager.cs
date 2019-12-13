
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
using System.Threading.Tasks;

namespace XGEF.Core.Events
{
	public interface IEventManager
	{
		string EventName { get; }
		int EventID { get; set; }
		bool Subscribe(EventHandler<XEventArgs> callback);
		bool Subscribe(EventHandler<XEventArgs> callback, Priority priority);
		bool Unsubscribe(EventHandler<XEventArgs> callback, Priority priority);
		void UnsubscribeAll(EventHandler<XEventArgs> callback);
		void Clear();
		bool Invoke(object caller, XEventArgs data);
		XEventArgs GetBlankDefaultArgs();
		XEventArgs GetDefaultArgs(byte[] data);
	}

	public interface IEventManager<T> : IEventManager
		where T : XEventArgs
	{
		bool Subscribe(EventHandler<T> callback, Priority priority);
		bool Subscribe(EventHandler<T> callback);
		bool Unsubscribe(EventHandler<T> callback, Priority priority);
		void UnsubscribeAll(EventHandler<T> callback);
		bool Invoke(object caller, T data);
		T GetBlankArgs();
		T GetArgs(byte[] data);
	}


	public class EventManager<T> : IEventManager<T>
		where T : XEventArgs, new()
	{
		public string EventName { get; }
		private int? _id;
		public int EventID
		{
			get { return _id ?? -1; }
			set
			{
				if(_id.HasValue)
				{
					throw new InvalidOperationException($"EventManager for the {EventName} event has already had its ID value set to {_id.Value}, but a second attempt to set it to {value} has occurred!");
				}

				_id = value;
			}
		}

		public Type HandlerType { get { return typeof(EventHandler<T>); } }
		public Type ArgsType { get { return typeof(T); } }

		public delegate void Function(object sender, T args);

		public Dictionary<Priority, HashSet<EventHandler<T>>> Callbacks { get; protected set; }

		public static EventManager<T> operator +(EventManager<T> ev, EventHandler<T> callback)
		{
			ev.Subscribe(callback);

			return ev;
		}

		public bool HasPriority(Priority priority)
		{
			return Callbacks[priority].Count > 0;
		}

		public XEventArgs GetBlankDefaultArgs()
		{
			return GetBlankArgs();
		}

		public XEventArgs GetDefaultArgs(byte[] data)
		{
			return GetArgs(data);
		}

		public T GetBlankArgs()
		{
			return new T() { EventName = this.EventName, EventID = this.EventID };
		}

		public T GetArgs(byte[] data)
		{
			return XEventArgs.DeserializeFromBitstream<T>(data);
		}

		public bool Subscribe(EventHandler<XEventArgs> callback)
		{
			return Subscribe(callback, Priority.Normal);
		}

		public bool Subscribe(EventHandler<XEventArgs> callback, Priority priority)
		{
			if (!callback.CanConvertTo(HandlerType))
			{
				throw new ArgumentException($"Event delegate must match the event's registered type: {callback.GetType()} is not a {HandlerType}!");
			}

			return Subscribe(new EventHandler<T>(callback), priority);
		}

		public bool Subscribe(EventHandler<T> callback)
		{
			return Subscribe(callback, Priority.Normal);
		}

		public bool Subscribe(EventHandler<T> callback, Priority priority)
		{
			if (Callbacks[priority].Contains(callback))
			{
				return false;
			}

			Callbacks[priority].Add(callback);
			return true;
		}

		public bool Unsubscribe(EventHandler<XEventArgs> callback, Priority priority = Priority.Normal)
		{
			if (!callback.CanConvertTo(HandlerType))
			{
				throw new ArgumentException($"Event delegate must match the event's registered type: {callback.GetType()} is not a {HandlerType}!");
			}

			return Unsubscribe(new EventHandler<T>(callback), priority);
		}

		public bool Unsubscribe(EventHandler<T> callback, Priority priority = Priority.Normal)
		{
			if (!Callbacks[priority].Contains(callback))
			{
				return false;
			}

			Callbacks[priority].Remove(callback);
			return true;
		}

		public void UnsubscribeAll(EventHandler<XEventArgs> callback)
		{
			if (!callback.CanConvertTo(HandlerType))
			{
				throw new ArgumentException($"Event delegate must match the event's registered type: {callback.GetType()} is not a {HandlerType}!");
			}

			UnsubscribeAll(new EventHandler<T>(callback));
		}

		public void UnsubscribeAll(EventHandler<T> callback)
		{
			foreach (var priority in new Priority().GetValues())
			{
				Unsubscribe(callback, priority);
			}
		}

		public bool Invoke(object caller)
		{
			return Invoke(caller, new T());
		}

		public bool Invoke(object caller, XEventArgs data)
		{
			if (!data.CanConvertTo(ArgsType))
			{
				throw new ArgumentException($"Event arguments must match the event's registered type: {data.GetType()} is not a {ArgsType}!");
			}

			return Invoke(caller, (T)data);
		}

		public bool Invoke(object caller, T data)
		{
			foreach (var priority in new Priority().GetValues().Reverse())
			{
				if (priority == Priority.High || priority == Priority.Highest)
				{
					foreach (var callback in Callbacks[priority])
					{
						callback.Invoke(caller, data);
					}

					//breaking outside of the loop permits all other subscribers at the same tier to un-abort
					if (data.Aborted)
					{
						return false;
					}
				}
				else
				{
					foreach (var callback in Callbacks[priority])
					{
						callback.Invoke(caller, data);

						//in lower priorities, the first abortion causes the chain to break
						if (data.Aborted)
						{
							return false;
						}
					}
				}
			}

			return true;
		}

		public void Clear()
		{
			Callbacks.Clear();
		}

		public EventManager(string eventName)
		{
			EventName = eventName;
			Callbacks = new Dictionary<Priority, HashSet<EventHandler<T>>>();

			foreach (var priority in new Priority().GetValues())
			{
				Callbacks[priority] = new HashSet<EventHandler<T>>();
			}
		}
	}
}
