
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using XGEF;
using XGEF.Core.Networking;

namespace XGEF.Core.Events
{
	public abstract class ModEventSystemBase : ModSystem<CoreEventSystem>
	{
		public Dictionary<string, IEventManager> Events { get; protected set; }
		public Dictionary<int, IEventManager> EventsByID { get; protected set; }

		protected abstract void RegisterEvents();

		public override void PreInit()
		{
			base.PreInit();
			RegisterEvents();
		}

		protected ModEventSystemBase()
		{
			
			Events = new Dictionary<string, IEventManager>();
			EventsByID = new Dictionary<int, IEventManager>();
		}
	}


	public class CoreEventSystem : CoreSystem<ModEventSystemBase>
	{
		protected ConcurrentQueue<(object caller, XEventArgs args)> EventQueue { get; set; }
		protected Dictionary<string, IEventManager> Events
		{
			get { return ModSystem.Events; }
		}

		protected Dictionary<int, IEventManager> EventsByID
		{
			get { return ModSystem.EventsByID; }
		}

		protected Dictionary<Type, string> EventNames { get; set; }


		public IEventManager GetEvent(string name)
		{
			if (Events.ContainsKey(name))
			{
				return Events[name];
			}

			return null;
		}

		public IEventManager<T> GetEvent<T>(string name)
			where T : XEventArgs
		{
			var manager = GetEvent(name);
			if(manager is IEventManager<T> casted)
			{
				return casted;
			}

			return null;
		}

		protected CoreNetworkSystem NetworkSystem { get; set; }

		public T GetDefaultEventArgs<T>(string eventName) where T : XEventArgs
		{
			return (T)Events[eventName].GetBlankDefaultArgs();
		}

		public XEventArgs GetDefaultEventArgs(string eventName)
		{
			return Events[eventName].GetBlankDefaultArgs();
		}

		public XEventArgs GetDefaultEventArgs(int eventID)
		{
			return EventsByID[eventID].GetBlankDefaultArgs();
		}

		public int GetEventID(string eventName)
		{
			if(!Initialized)
			{
				throw new InvalidOperationException("Event IDs cannot be had until after initialization!");
			}

			return ModSystem.Events[eventName].EventID;
		}

		public bool EventIDExists(int id)
		{
			return EventsByID.ContainsKey(id);
		}

		public bool EventExists(string name)
		{
			return Events.ContainsKey(name);
		}


		public override void Init()
		{
			base.Init();
			//instantiate the most basic events here
			//connect the data converter to the reliable io

			foreach(var _event in ModSystem.Events.Values)
			{
				_event.EventID = GenerateEventID();
				EventsByID[_event.EventID] = _event;
			}
			
		}

		public override void PostInit()
		{
			base.PostInit();
			NetworkSystem = Manager.GetSystem<CoreNetworkSystem>();
		}

		private int lastID = 0;
		private int GenerateEventID()
		{
			return lastID++;
		}

		public override void Process()
		{
			bool queuedEvents;
			do
			{
				queuedEvents = EventQueue.TryDequeue(out var output);
				if(queuedEvents)
				{
					//(object caller, XEventArgs args) = output;
					InvokeBlocking(output.caller, output.args);
				}
			}
			while (queuedEvents);
		}

		public void InvokeBlocking(object caller, XEventArgs args)
		{
			if (Events.ContainsKey(args.EventName))
			{
				Manager.Debug($"Event being sent to registered EventManager ({args.EventID}: {args.EventName}).");
				//TODO:
				//This will need to be adjusted to be asynchronous in the future
				if (Events[args.EventName].Invoke(caller, args))
				{
					if (args.SendOverNetwork && !args.NetworkedEvent)
					{
						NetworkSystem.SendMessage(args);
					}

				}
			}
		}

		public void InvokeBlocking(object caller, int eventID, byte[] argData)
		{
			if (!EventsByID.ContainsKey(eventID))
			{
				throw new InvalidOperationException($"EventManager has no record of the event with ID {eventID} requested by {caller.ToString()}!");
			}

			InvokeBlocking(caller, EventsByID[eventID].GetDefaultArgs(argData));
		}

		public bool RegisterEvent<T>()
			where T : XEventArgs, new()
		{
			return RegisterEvent<T>(new T().EventName);
		}

		public bool RegisterEvent<T>(string name)
			where T : XEventArgs, new()
		{
			if(Initialized)
			{
				throw new InvalidOperationException("Events cannot be registered after initialization!");
			}
			if (!Events.ContainsKey(name))
			{
				Events.Add(name, new EventManager<T>(name));
				EventNames[typeof(T)] = name;
				return true; 
			}

			return false;
		}



		public bool Subscribe(string name, EventHandler<XEventArgs> callback)
		{
			return Subscribe(name, callback, Priority.Normal);
		}

		public bool Subscribe(string name, EventHandler<XEventArgs> callback, Priority priority)
		{
			if(Events.ContainsKey(name))
			{
				return Events[name].Subscribe(callback, priority);
			}

			return false;
		}

		public bool Subscribe<T>(EventHandler<T> callback)
			where T : XEventArgs, new()
		{
			return Subscribe<T>(callback, Priority.Normal);
		}

		public bool Subscribe<T>(EventHandler<T> callback, Priority priority)
			where T : XEventArgs, new()
		{
			string name; ;
			if (EventNames.ContainsKey(typeof(T)))
			{
				name = EventNames[typeof(T)];
			}
			else
			{
				return false;
			}

			return Subscribe<T>(name, callback, priority);
		}

		public bool Subscribe<T>(string name, EventHandler<T> callback)
			where T : XEventArgs, new()
		{
			return Subscribe<T>(name, callback, Priority.Normal);
		}

		public bool Subscribe<T>(string name, EventHandler<T> callback, Priority priority)
			where T : XEventArgs, new()
		{
			if (Events.ContainsKey(name))
			{
				return (Events[name] as IEventManager<T>).Subscribe(callback, priority);
			}

			return false;
		}

		

		public void Invoke(object caller, XEventArgs args)
		{
			if (!Events.ContainsKey(args.EventName))
			{
				throw new InvalidOperationException($"EventManager has no record of the event named {args.EventName} requested by {caller.ToString()}!");
			}

			Enqueue(caller, args, args.NetworkedEvent);
		}

		public void Invoke(object caller, int eventID, byte[] argData, bool networked=false)
		{
			if (!EventsByID.ContainsKey(eventID))
			{
				throw new InvalidOperationException($"EventManager has no record of the event with ID {eventID} requested by {caller.ToString()}!");
			}

			Enqueue(caller, EventsByID[eventID].GetDefaultArgs(argData), networked);
		}

		public void Invoke(object caller, string eventName, bool networked=false)
		{
			if (!Events.ContainsKey(eventName))
			{
				throw new InvalidOperationException($"EventManager has no record of the event named {eventName} requested by {caller.ToString()}!");
			}

			Enqueue(caller, Events[eventName].GetBlankDefaultArgs(), networked);
		}

		public void Invoke(object caller, string eventName, byte[] argData, bool networked=false)
		{
			if (!Events.ContainsKey(eventName))
			{
				throw new InvalidOperationException($"EventManager has no record of the event named {eventName} requested by {caller.ToString()}!");
			}

			Enqueue(caller, Events[eventName].GetDefaultArgs(argData), networked);
		}

		protected void Enqueue(object caller, XEventArgs args, bool networked)
		{
			if (string.IsNullOrEmpty(args.EventName))
			{
				throw new ArgumentException("Event arguments cannot have a blank name!  Ensure that you are setting it in the constructor.");
			}

			if(networked)
			{
				args.SetNetworked();
			}

			Manager.Debug($"Enqueueing event ({args.EventID}: {args.EventName}).");

			EventQueue.Enqueue((caller, args));
		}

		public CoreEventSystem() : base()
		{
			Name = "CoreEventSystem";
			//this field should be superceded by the systems.cs file.
			ModdedSystemPath = "Engine/Events/EventSystem.cs";
			ModdedSystemName = "EventSystem";

			EventQueue = new ConcurrentQueue<(object, XEventArgs)>();
			EventNames = new Dictionary<Type, string>();
		}
	}
}
