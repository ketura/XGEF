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
	//One-half of a moddable system, this is the half that will be packed into the game files as regular code and compiled as needed.
	public abstract class ModSystem : SplitSystem
	{
		public CoreSystem CoreSystemBase { get; protected set; }
		public virtual Type CoreSystemType { get; protected set; }

		public virtual void SetCoreSystem(CoreSystem system)
		{
			CoreSystemBase = system;
			CoreSystemType = system.GetType();
			Manager = system.Manager;
		}

		public virtual void SetCoreSystem(object system)
		{
			SetCoreSystem(system as CoreSystem);
		}

		public override void Init()
		{
			if (CoreSystemBase == null)
				throw new InvalidOperationException("Cannot initialize ModSystem!  Its compiled half was never provided!");
		}

		public void Pair(CoreSystem system)
		{
			SetCoreSystem(system);
			system.SetModdedSystem(this);
		}

	}


	public abstract class ModSystem<T> : ModSystem
		where T : CoreSystem
	{
		public T CoreSystem { get { return CoreSystemBase as T; } }
		public override Type CoreSystemType { get { return typeof(T); } }

		public virtual void SetCoreSystem(T system)
		{
			SetCoreSystem(system as CoreSystem);
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
