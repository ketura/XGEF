
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

using XGEF;

namespace XGEF.Core.Meta
{
	public abstract class ModBootstrapSystemBase : ModSystem<CoreBootstrapSystem>
	{
		public event Action<SystemManager> OnPreInit;
		public event Action<SystemManager> OnInit;
		public event Action<SystemManager> OnPostInit;

		public event Action<SystemManager> OnPreProcess;
		public event Action<SystemManager> OnProcess;
		public event Action<SystemManager> OnPostProcess;

		public override void PreInit()
		{
			OnPreInit?.Invoke(Manager);
		}
		public override void Init()
		{
			OnInit?.Invoke(Manager);
		}
		public override void PostInit()
		{
			//Since this system should be called last, this is the absolute last system called before game processing is enabled
			//This should be used to interface the game engine proper with all mod libraries post-compile. 
			Console.WriteLine("Bootstrap entered.");
			OnInit?.Invoke(Manager);
			Console.WriteLine("Bootstrap exited.");
		}

		public override void PreProcess()
		{
			OnPreProcess?.Invoke(Manager);
		}
		public override void Process()
		{
			OnProcess?.Invoke(Manager);
		}
		public override void PostProcess()
		{
			OnPostProcess?.Invoke(Manager);
		}
	}

	public class CoreBootstrapSystem : CoreSystem<ModBootstrapSystemBase>
	{
		public CoreBootstrapSystem()
		{
			Name = "CoreBootstrapSystem";
			//this field should be superceded by the systems.cs file.
			ModdedSystemPath = "Engine/Meta/BootstrapSystem.cs";
			ModdedSystemName = "BootstrapSystem";

			AutomaticallyPair = false;
		}
	}
}
