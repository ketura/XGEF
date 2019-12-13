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

using Newtonsoft.Json;

namespace XGEF
{
	public abstract class Serializer
	{
		public abstract object Serialize(object obj);
		public abstract object Deserialize(object obj);

		public virtual T Serialize<T, U>(U obj)
		{
			return (T)Serialize(obj);
		}

		public virtual T Serialize<T>(object obj)
		{
			return (T)Serialize(obj);
		}

		public virtual T Deserialize<T, U>(U obj)
		{
			return (T)Deserialize(obj);
		}

		public virtual T Deserialize<T>(object obj)
		{
			return (T)Deserialize(obj);
		}
	}

	public class JsonSerializer : Serializer
	{
		public static readonly JsonSerializerSettings DefaultSettings = new JsonSerializerSettings()
		{
			NullValueHandling = NullValueHandling.Ignore,
			DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
			Formatting = Formatting.Indented,
		};

		public static readonly JsonSerializerSettings TemplateSettings = new JsonSerializerSettings()
		{
			NullValueHandling = NullValueHandling.Include,
			DefaultValueHandling = DefaultValueHandling.Populate,
			Formatting = Formatting.Indented,
		};

		
		public static string SerializeJson(object obj, JsonSerializerSettings settings=null)
		{
			if (settings == null)
				settings = DefaultSettings;
			return JsonConvert.SerializeObject(obj, settings);
		}

		public static object DeserializeJson(string json, JsonSerializerSettings settings = null)
		{
			if (settings == null)
				settings = DefaultSettings;
			return JsonConvert.DeserializeObject(json, settings);
		}

		public static T DeserializeJson<T>(string json, JsonSerializerSettings settings = null)
		{
			if (settings == null)
				settings = DefaultSettings;
			return JsonConvert.DeserializeObject<T>(json, settings);
		}

		//--

		public override object Serialize(object obj)
		{
			return SerializeJson(obj);
		}

		public override T Serialize<T, U>(U obj)
		{
			return (T)(SerializeJson(obj) as object);
		}

		public override T Serialize<T>(object obj)
		{
			return (T)(SerializeJson(obj) as object);
		}

		public override object Deserialize(object obj)
		{
			return DeserializeJson(obj.ToString());
		}

		public override T Deserialize<T, U>(U obj)
		{
			return DeserializeJson<T>(obj.ToString());
		}

		public override T Deserialize<T>(object obj)
		{
			return DeserializeJson<T>(obj.ToString());
		}
	}

}
