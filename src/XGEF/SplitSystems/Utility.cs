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
	//Basically a system that is never modded, doesn't define game asset types, and doesn't directly interact with the game.
	public abstract class Utility : ISplitSystem
	{
		public string Name { get; protected set; }

		public SystemManager Manager { get; set; }

		public bool Initialized { get; protected set; }

		public Utility()
		{
			Name = this.GetType().Name;
		}

		public virtual void Init()
		{
			if (Initialized)
				throw new InvalidOperationException($"Cannot initialize {Name}!  It has already been initialized!");

			Initialized = true;
		}
		public virtual void Process() { }

		public override string ToString()
		{
			return Name;
		}

		public virtual void LoadSettings(XGEFSettings settings) { }
		public virtual void PreInit() { }
		public virtual void PostInit() { }
		public virtual void PreProcess() { }
		public virtual void PostProcess() { }
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
