///////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////
////                                                                               ////
////    Copyright 2019-2020 Christian 'ketura' McCarty                             ////
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

namespace XGEF
{
	//A system that is intended to be modded.  This class should probably not ever be inherited from
	// directly, but it is available due to not being able to easily instantiate generic types that
	// are not known at compile time.
	public abstract class CoreSystem : SplitSystem
	{
		public ModSystem ModSystemBase { get; protected set; }
		public virtual Type ModdedSystemType { get; protected set; }

		public bool AutomaticallyPair { get; protected set; } = true;

		public string ModdedSystemPath { get; protected set; }
		public string ModdedSystemName { get; protected set; }

		public virtual void SetModdedSystem(ModSystem moddedSystem)
		{
			if (!ModdedSystemType.IsAssignableFrom(moddedSystem.GetType()))
				throw new InvalidOperationException($"Cannot accept {moddedSystem.Name} for {Name} as it is not of type {ModdedSystemType.Name}.");

			ModSystemBase = moddedSystem;
			ModdedSystemType = moddedSystem.GetType();
		}

		public virtual void SetModdedSystem(object moddedSystem)
		{
			SetModdedSystem(moddedSystem as ModSystem);
		}

		public override void PreInit()
		{
			if (ModSystemBase != null)
				ModSystemBase.PreInit();
		}

		public override void Init()
		{
			if (ModSystemBase == null)
			{
				if(!AutomaticallyPair)
				{
					Manager.Warn($"System {Name} is set to manual pair but no pair was ever provided by the user.  This system will be stripped from the game.");
					//TODO: actually remove this system from the manager.
					//Manager[Name] = null;
					return;
				}
				else
				{
					throw new InvalidOperationException($"Cannot initialize ModdableCoreSystem {Name}!  Its modded half was never provided!");
				}
			}

			ModSystemBase.Init();

			Initialized = true;
		}

		public override void PostInit()
		{
			if (ModSystemBase != null)
				ModSystemBase.PostInit();
		}

		public void Pair(ModSystem system)
		{
			SetModdedSystem(system);
			system.SetCoreSystem(this);
		}

		public override void PreProcess()
		{
			if(Initialized)
			{
				ModSystemBase.PreProcess();
			}
		}

		public override void Process()
		{
			if (Initialized)
			{
				ModSystemBase.Process();
			}
		}

		public override void PostProcess()
		{
			if (Initialized)
			{
				ModSystemBase.PostProcess();
			}
		}
	}


	public abstract class CoreSystem<T> : CoreSystem
		where T : ModSystem
	{
		public override Type ModdedSystemType { get { return typeof(T); } }
		public T ModSystem { get { return ModSystemBase as T; } }

		public CoreSystem() : base() { }

		public virtual void SetModdedSystem(T moddedSystem)
		{
			ModSystemBase = moddedSystem;
		}
	}

		//GeneratesEvent
		//AppendEvent
		//PrependEvent
		//OverrideClass
		//OverrideFunction
		//AppendFunction
		//PrependFunction
		//DeleteFunction

		//Don't Serialize attribute
		//naked code with no class wrapped in a class named after the file path?
	}
