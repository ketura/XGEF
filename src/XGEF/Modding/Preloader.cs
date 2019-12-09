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

namespace XGEF
{
	public static class Preloader
	{
		public static SystemManager LoadByNames(params string[] objects)
		{
			return LoadByNames(objects.ToList());
		}

		public static SystemManager LoadByNames(IEnumerable<string> objects)
		{
			return LoadNamesInto(new SystemManager(), objects);
		}

		public static SystemManager LoadDefaultByNames(params string[] objects)
		{
			return LoadNamesInto(LoadDefault(), objects);
		}

		public static SystemManager LoadDefaultByNames(IEnumerable<string> objects)
		{
			return LoadNamesInto(LoadDefault(), objects);
		}

		private static SystemManager LoadNamesInto(SystemManager manager, IEnumerable<string> objects)
		{
			List<Type> types = new List<Type>();

			foreach (string obj in objects)
			{
				var type = Type.GetType(obj);

				if (type == null && !obj.StartsWith("XGEF."))
					type = Type.GetType("XGEF." + obj);
				
				if (type != null)
					types.Add(type);
			}

			return LoadTypesInto(manager, types);
		}

		public static SystemManager LoadByTypes(params Type[] objects)
		{
			return LoadByTypes(objects.ToList());
		}

		public static SystemManager LoadByTypes(IEnumerable<Type> objects)
		{
			return LoadTypesInto(new SystemManager(), objects);
		}

		public static SystemManager LoadDefaultWithTypes(IEnumerable<Type> objects)
		{
			return LoadTypesInto(LoadDefault(), objects);
		}

		public static SystemManager LoadDefaultWithTypes(params Type[] objects)
		{
			return LoadTypesInto(LoadDefault(), objects);
		}

		private static SystemManager LoadTypesInto(SystemManager manager, IEnumerable<Type> objects)
		{
			foreach (Type t in objects)
			{
				if (Activator.CreateInstance(t) is ISplitSystem sys)
				{
					manager.RegisterSystem(sys);
				}
			}

			return manager;
		}

		public static SystemManager LoadDefault()
		{
			return LoadByTypes(typeof(LogSystem), typeof(ModLoader), typeof(AssetRegistry));
		}

		//public static SystemManager LoadConfig(string filename)

	}
}
