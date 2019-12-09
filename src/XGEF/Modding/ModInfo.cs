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
using System.IO;
using System.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using XGEF.Compiler;
using System.ComponentModel;
using System.Diagnostics;

namespace XGEF
{
	[DebuggerDisplay("{ModName}: {ModID}")]
	public class ModInfo
	{
		public static readonly string DefaultFilename = "mod_info.json";

		public static readonly Guid CoreModID = Guid.Empty;

		protected static readonly XGEF.Common.Logging.Logger Log = XGEF.Common.Logging.Logger.Instance;

		[JsonIgnore]
		public bool Active { get; set; } = true;
		public string ModName { get; set; }
		public Version ModVersion { get; set; }
		public Guid ModID { get; set; }
		public string ModDescription { get; set; }
		public string ModCopyrightInfo { get; set; }
		public string ModAuthor { get; set; }
		public string ModURL { get; set; }
		public string ModIconPath { get; set; }
		public bool Selectable { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		[DefaultValue(XGEF.RequestedPriority.Normal)]
		public RequestedPriority RequestedLoadPriority { get; set; }
		[JsonConverter(typeof(StringEnumConverter))]
		[DefaultValue(XGEF.RequestedPriority.Normal)]
		public RequestedPriority RequestedRunPriority { get; set; }
		public List<Guid> RequiredMods { get; set; }
		public List<Guid> ConflictingMods { get; set; }
		public List<Guid> LoadTheseBefore { get; set; }
		public List<Guid> LoadTheseAfter { get; set; }
		public List<string> References { get; set; }

		[JsonIgnore]
		public string RootLocation { get; protected set; }
		[JsonIgnore]
		public FileLoader EngineLoader { get; protected set; }
		[JsonIgnore]
		public FileLoader ContentLoader { get; protected set; }
		[JsonIgnore]
		public ProjectID Project { get; set; }
		[JsonIgnore]
		public string AssemblyFilename { get; set; }
		[JsonIgnore]
		public uint LoadPriority { get; set; }
		[JsonIgnore]
		public uint RunPriority { get; set; }
		[JsonIgnore]
		public bool IsCoreMod { get { return ModID == CoreModID; } }

		public void SetLocation(string root)
		{
			RootLocation = Path.GetFullPath(root);
			if (!Directory.Exists(root))
				throw new ArgumentException("Root location for ModInfo must be an existing directory!");

			string enginePath = Path.Combine(RootLocation, "Engine");
			if (Directory.Exists(enginePath))
			{
				//Log.Info($"Engine directory structure found for {ModName}.");
				EngineLoader = new FileLoader(enginePath);
			}

			string contentPath = Path.Combine(RootLocation, "Content");
			if (Directory.Exists(contentPath))
			{
				//Log.Info($"Content directory structure found for {ModName}.");
				ContentLoader = new FileLoader(contentPath);
			}

			if (EngineLoader == null && ContentLoader == null)
			{
				//Log.Info($"No mod directories found for {ModName}.");
				Active = false;
			}
		}

		public void SaveToFile(string path = null)
		{
			if (path == null)
				path = Path.Combine(RootLocation, DefaultFilename);
			File.WriteAllText(path, JsonSerializer.SerializeJson(this));
		}

		public void SetLoadPriority(uint  prio)
		{
			LoadPriority = prio;
			if (EngineLoader != null)
			{
				EngineLoader.DefaultPriority = prio;
			}

			if (ContentLoader != null)
			{
				ContentLoader.DefaultPriority = prio;
			}
		}

		public void CrawlEngine()
		{
			if (EngineLoader != null)
				EngineLoader.Crawl();
		}

		public void CrawlContent()
		{
			if (ContentLoader != null)
				ContentLoader.Crawl();
		}

		public void CrawlAllFiles()
		{
			CrawlEngine();
			CrawlContent();
		}

		public void LoadEngine()
		{
			if (EngineLoader != null)
				EngineLoader.LoadAllFiles();
		}

		public void LoadContent()
		{
			if (ContentLoader != null)
				ContentLoader.LoadAllFiles();
		}

		public void LoadAllFiles()
		{
			LoadEngine();
			LoadContent();
		}

		public void CleanLists()
		{
			if (RequiredMods == null)
				RequiredMods = new List<Guid>();
			if (ConflictingMods == null)
				ConflictingMods = new List<Guid>();
			if (LoadTheseBefore == null)
				LoadTheseBefore = new List<Guid>();
			if (LoadTheseAfter == null)
				LoadTheseAfter = new List<Guid>();
			if (References == null)
				References = new List<string>();

			RequiredMods.Remove(Guid.Empty);
			ConflictingMods.Remove(Guid.Empty);
			LoadTheseBefore.Add(Guid.Empty);
			LoadTheseAfter.Remove(Guid.Empty);

			RequiredMods = RequiredMods.Distinct().ToList();
			ConflictingMods = ConflictingMods.Distinct().ToList();
			LoadTheseBefore = LoadTheseBefore.Distinct().ToList();
			LoadTheseAfter = LoadTheseAfter.Distinct().ToList();
		}

		public static ModInfo FromFile(string path)
		{
			return FromString(File.ReadAllText(path));
		}

		public static ModInfo FromString(string data)
		{
			return JsonSerializer.DeserializeJson<ModInfo>(data);
		}

		public static ModInfo GetNewModInfo()
		{
			ModInfo info = new ModInfo()
			{
				ModName = "Unnamed Mod",
				ModDescription = "A brand new baby mod.",
				ModAuthor = "Your name here",
				ModVersion = new Version("0.1.0.0"),
				ModID = Guid.NewGuid(),
				RequestedLoadPriority = RequestedPriority.Normal,
				RequestedRunPriority = RequestedPriority.Normal,
			};

			info.CleanLists();
			return info;
		}
	}
}