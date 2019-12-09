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

using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel;

using XGEF.Common.Logging;
using System.Reflection;

namespace XGEF
{
	[JsonObject]
	public class XGEFSettings
	{
		public static XGEFSettings ConsolidateFromFile(string path)
		{
			List<DummySettings> list = DummyFromFile(path).ToList();
			XGEFSettings settings = GetDefaultSettings();

			foreach (var dummy in list)
			{
				foreach (PropertyInfo dummyProp in typeof(DummySettings).GetProperties())
				{
					PropertyInfo prop = typeof(XGEFSettings).GetProperty(dummyProp.Name);
					object dummyField = dummyProp.GetValue(dummy, null);
					object field = prop.GetValue(settings, null);
					if (dummyField == null)
						continue;

					if (!dummyField.Equals(field))
					{
						prop.SetValue(settings, dummyField);
					}
				}
			}

			return settings;
		}
		public static IEnumerable<XGEFSettings> FromFile(string path)
		{
			return JsonSerializer.DeserializeJson<List<XGEFSettings>>(File.ReadAllText(path));
		}

		private static IEnumerable<DummySettings> DummyFromFile(string path)
		{
			return JsonSerializer.DeserializeJson<List<DummySettings>>(File.ReadAllText(path));
		}

		public static void WriteDefaultSettings(string path)
		{
			WriteSettings(path, GetDefaultSettings());
		}

		public static void WriteSettings(string path, IEnumerable<XGEFSettings> settingsList)
		{
			WriteSettings(path, settingsList.ToArray());
		}

		public static void WriteSettings(string path, params XGEFSettings[] settingsList)
		{
			var list = new List<XGEFSettings>(settingsList);
			string json = JsonSerializer.SerializeJson(list, JsonSerializer.TemplateSettings);
			File.WriteAllText(path, json);
		}

		[DefaultValue("CoreMods")]
		public string CoreModAssemblyName { get; set; }
		[DefaultValue("Engine")]
		public string EngineRelativePath { get; set; }
		[DefaultValue("Game")]
		public string GameRelativePath { get; set; }


		[JsonConverter(typeof(StringEnumConverter))]
		[DefaultValue(LogLevel.Info)]
		public LogLevel LogOutput { get; set; }
		[DefaultValue(false)]
		public bool LogMethodTrace { get; set; }

		[DefaultValue(false)]
		public bool SaveConsolidatedCode { get; set; }
		[DefaultValue("ConsolidatedCode")]
		public string ConsolidatedCodeLocation { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		[DefaultValue(RequirementHandling.None)]
		public RequirementHandling IgnoreCircularDependencies { get; set; }
		public List<Guid> CircularExceptions { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		[DefaultValue(RequirementHandling.None)]
		public RequirementHandling IgnoreModRequirements { get; set; }
		public List<Guid> ModRequirementExceptions { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		[DefaultValue(RequirementHandling.None)]
		public RequirementHandling IgnoreModConflicts { get; set; }
		public List<Guid> ModConflictExceptions { get; set; }

		//Using this class allows us to load a settings file that has only had important settings set, the rest assumed to be
		// defaults.  Note that everything is nullable for this reason.
		private class DummySettings
		{
			[JsonConverter(typeof(StringEnumConverter))]
			public LogLevel? LogOutput { get; set; }
			public bool? LogMethodTrace { get; set; }
			public bool? SaveConsolidatedCode { get; set; }
			public string ConsolidatedCodeLocation { get; set; }

			[JsonConverter(typeof(StringEnumConverter))]
			public RequirementHandling? IgnoreCircularDependencies { get; set; }
			public List<Guid> CircularExceptions { get; set; }

			[JsonConverter(typeof(StringEnumConverter))]
			public RequirementHandling? IgnoreModRequirements { get; set; }
			public List<Guid> ModRequirementExceptions { get; set; }

			[JsonConverter(typeof(StringEnumConverter))]
			public RequirementHandling? IgnoreModConflicts { get; set; }
			public List<Guid> ModConflictExceptions { get; set; }
		}


		public static XGEFSettings GetDefaultSettings()
		{
			XGEFSettings settings = new XGEFSettings()
			{
				CoreModAssemblyName = "CoreMods",
				EngineRelativePath = "Engine",
				GameRelativePath = "Game",
				LogOutput = LogLevel.Info,
				LogMethodTrace = false,
				IgnoreCircularDependencies = RequirementHandling.None,
				CircularExceptions = new List<Guid>(),
				IgnoreModRequirements = RequirementHandling.None,
				ModRequirementExceptions = new List<Guid>(),
				IgnoreModConflicts = RequirementHandling.None,
				ModConflictExceptions = new List<Guid>()
			};

			return settings;
		}

	}
}