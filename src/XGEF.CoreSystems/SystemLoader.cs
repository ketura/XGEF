
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

using XGEF.Core.Events;
using XGEF.Core.Networking;
using XGEF.Core.Meta;

namespace XGEF.Core
{
	public static class SystemLoader
	{
		public static SystemManager SysManager { get; set; }

		static SystemLoader()
		{
			SysManager = SystemManager.Instance;

			SysManager.RegisterSystem(new CoreEventSystem());
			SysManager.RegisterSystem(new CoreNetworkSystem(false));


			//This one should go last so it gets hit last on PostInit
			SysManager.RegisterSystem(new CoreBootstrapSystem());
		}

		public static T GetSystem<T>(string name) where T : class, ISplitSystem
		{
			return SysManager.GetSystem<T>(name);
		}

		public static T GetSystem<T>() where T : class, ISplitSystem
		{
			return SysManager.GetSystem<T>();
		}
	}
}
