
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

using Newtonsoft.Json;

using XGEF.Core.Events;

namespace XGEF.Core.Networking
{
	public static class DefaultBitstreamConverter
	{
		public static T DeserializeJsonBitstream<T>(byte[] obj) where T : XEventArgs
		{
			string json = Encoding.UTF8.GetString(obj);
			return JsonConvert.DeserializeObject<T>(json);
		}

		public static byte[] SerializeJsonBitstream<T>(T args) where T : XEventArgs
		{
			string json = JsonConvert.SerializeObject(args);
			return Encoding.UTF8.GetBytes(json.ToCharArray());
		}
	}
}
