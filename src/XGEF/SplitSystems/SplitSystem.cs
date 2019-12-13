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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XGEF
{
	public interface ISplitSystem
	{
		string Name { get; } 

		void LoadSettings(XGEFSettings settings);

		void PreInit();
		void Init();
		void PostInit();

		void PreProcess();
		void Process();
		void PostProcess();

		bool Initialized { get; }
		SystemManager Manager { get; set; }
	}

	

	//The primary means through which a game is defined.
	public abstract class SplitSystem : ISplitSystem
	{
		public virtual void LoadSettings(XGEFSettings settings) { }

		public virtual void PreInit() { }
		public abstract void Init();
		public virtual void PostInit() { }

		public virtual void PreProcess() { }
		public abstract void Process();
		public virtual void PostProcess() { }

		public bool Initialized { get; protected set; }
		public string Name { get; protected set; } 
		public SystemManager Manager { get; set; }
		//public virtual IEnumerable<EntityDefinition> EntityDefinitions { get; protected set; }

		protected SplitSystem() : base()
		{
			Name = this.GetType().Name;
			//EntityDefinitions = new List<EntityDefinition>();
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
