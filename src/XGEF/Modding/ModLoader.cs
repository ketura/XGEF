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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;

using XGEF.Compiler;
using XGEF.XMath;

namespace XGEF
{
	public class ModLoader : Utility
	{
		public string CoreLocation { get; set; }
		public List<string> ModLocations { get; set; }

		public ModInfo CoreMod { get; protected set; }
		public Dictionary<Guid, ModInfo> LoadedMods { get; protected set; }
		public Dictionary<Guid, ModInfo> UnloadedMods { get; protected set; }
		public List<ModInfo> AllMods { get { return LoadedMods.Values.Concat(UnloadedMods.Values).ToList(); } }
		public ReadOnlyDictionary<uint, ModInfo> ModsByLoadPriority { get; protected set; }
		public ReadOnlyDictionary<uint, ModInfo> ModsByRunPriority { get; protected set; }

		public Dictionary<string, Assembly> Assemblies { get; protected set; }
		public Dictionary<CoreSystem, ModSystem> SystemPairs { get; protected set; }

		public CSWorkspace Workspace { get; protected set; }

		public bool WriteConsolidatedFiles { get; protected set; }
		public string ConsolidatedFilesLocation { get; protected set; }

		public RequirementHandling IgnoreCircularReferences { get; protected set; }
		public RequirementHandling IgnoreRequiredMods { get; protected set; }
		public RequirementHandling IgnoreConflictingMods { get; protected set; }

		public List<Guid> CircularExceptions { get; protected set; }
		public List<Guid> ModRequirementExceptions { get; protected set; }
		public List<Guid> ModConflictExceptions { get; protected set; }

		protected AssetRegistry Index { get { return Manager["AssetRegistry"] as AssetRegistry; } }


		public ModLoader()
		{
			//This all needs to be pulled from a settings file; base in the game directory, overridden by one
			// in the default mods folder?
			CoreLocation = AppDomain.CurrentDomain.BaseDirectory;
			ModLocations = new List<string>
			{
				"Mods"
			};
		}

		public override void LoadSettings(XGEFSettings settings)
		{
			CoreMod = GetCoreModInfo(settings);

			WriteConsolidatedFiles = settings.SaveConsolidatedCode;
			ConsolidatedFilesLocation = settings.ConsolidatedCodeLocation;

			if (WriteConsolidatedFiles)
				Directory.CreateDirectory(ConsolidatedFilesLocation);

			IgnoreCircularReferences = settings.IgnoreCircularDependencies;
			IgnoreConflictingMods = settings.IgnoreModConflicts;
			IgnoreRequiredMods = settings.IgnoreModRequirements;

			CircularExceptions = settings.CircularExceptions;
			ModRequirementExceptions = settings.ModRequirementExceptions;
			ModConflictExceptions = settings.ModConflictExceptions;

			if (IgnoreCircularReferences == RequirementHandling.Specify && CircularExceptions.Count == 0)
			{
				Manager.Warn($"IgnoreCircularDependencies is set to '{RequirementHandling.Specify.ToString()}', but CircularExceptions has not been populated with any mod IDs!");
			}

			if (IgnoreRequiredMods == RequirementHandling.Specify && ModRequirementExceptions.Count == 0)
			{
				Manager.Warn($"IgnoreRequiredMods is set to '{RequirementHandling.Specify.ToString()}', but ModRequirementExceptions has not been populated with any mod IDs!");
			}

			if (IgnoreConflictingMods == RequirementHandling.Specify && ModRequirementExceptions.Count == 0)
			{
				Manager.Warn($"IgnoreConflictingMods is set to '{RequirementHandling.Specify.ToString()}', but ModRequirementExceptions has not been populated with any mod IDs!");
			}
		}

		public override void PreInit()
		{
			if(Initialized)
			{
				throw new InvalidOperationException("Cannot initialize ModLoader!  It has already been initialized!");
			}
			base.PreInit();
			Initialized = LoadMods() && CompileLoadedMods();
			if(!Initialized)
			{
				throw new InvalidDataException("The mods could not be loaded or compiled.  See log output for details.");
			}
		}

		public override void Init() { }

		public bool LoadMods(IEnumerable<string> locations = null)
		{
			if (locations != null)
			{
				ModLocations.AddRange(locations.Except(ModLocations));
			}

			SearchModFolders();
			ResolveModDependencies();
			PrioritizeMods();
			LoadModFiles();
			return true;
		}

		public bool CompileLoadedMods()
		{
			AnalyzeMods();
			WorkspaceSetup();

			try
			{
				CompileMods();
			}
			catch (CompilationErrorException ex)
			{
				Manager.Error($"One or more errors was encountered while attempting to compile mods.  Please see above this message for details.");
				int i = 0;
				foreach(string error in ex.Errors)
				{
					i++;
					Manager.Error($"\t[{i}]:  {error}");
				}
				return false;
			}
			PairMods();
			return true;
		}

		private void UnloadMod(ModInfo mod)
		{
			LoadedMods.Remove(mod.ModID);
			if (mod.ModID != null)
				UnloadedMods.Add(mod.ModID, mod);
			mod.Active = false;
		}

		private void SearchModFolders()
		{
			LoadedMods = new Dictionary<Guid, ModInfo>();
			UnloadedMods = new Dictionary<Guid, ModInfo>();

			LoadedMods.Add(CoreMod.ModID, CoreMod);

			Manager.Info($"Searching for mods in {ModLocations.Count} different locations...");
			foreach (string path in ModLocations)
			{
				if (!Directory.Exists(path))
				{
					Manager.Warn($"Mod path {path} not found.  Skipping.");
					continue;
				}

				foreach (var modPath in Directory.GetDirectories(path))
				{
					FileLoader loader = new FileLoader(Path.GetFullPath(modPath));
					loader.Crawl();

					var mod = GetModInfo(loader);
					if (mod == null)
					{
						Manager.Warn($"Mod path '{modPath}' did not contain a {ModInfo.DefaultFilename}.  Skipping.");
						//unfortunately don't have a good way of saving, since naturally there's no good info to load.
						continue;
					}

					if (mod.Active == false)
					{
						Manager.Warn($"Mod '{mod.ModName}' contained neither an Engine nor a Content folder.  Skipping.");
						UnloadMod(mod);
						continue;
					}

					if (LoadedMods.ContainsKey(mod.ModID))
					{
						Manager.Warn($"Mod '{mod.ModName}' has a GUID '{mod.ModID}' that has already been claimed by loaded mod '{LoadedMods[mod.ModID].ModName}'!  Skipping.");
						UnloadMod(mod);
						continue;
					}

					Manager.Debug($"Loaded info for mod '{mod.ModName}'.");
					LoadedMods.Add(mod.ModID, mod);
				}
			}
		}

		private bool CanIgnoreCircularReference(Guid modID)
		{
			if (IgnoreCircularReferences == RequirementHandling.All)
				return true;

			if (IgnoreCircularReferences == RequirementHandling.None)
				return false;

			//must be RequirementHandling.Some
			return CircularExceptions.Contains(modID);
		}

		private bool CanIgnoreCircularReferences(IEnumerable<Guid> ids)
		{
			return ids.All(x => CanIgnoreCircularReference(x));
		}

		private bool CanIgnoreModRequirement(Guid modID)
		{
			if (IgnoreRequiredMods == RequirementHandling.All)
				return true;

			if (IgnoreRequiredMods == RequirementHandling.None)
				return false;

			//must be RequirementHandling.Some
			return ModRequirementExceptions.Contains(modID);
		}

		private bool CanIgnoreModRequirements(IEnumerable<Guid> ids)
		{
			return ids.All(x => CanIgnoreModRequirement(x));
		}

		private bool CanIgnoreModConflict(Guid modID)
		{
			if (IgnoreConflictingMods == RequirementHandling.All)
				return true;

			if (IgnoreConflictingMods == RequirementHandling.None)
				return false;

			//must be RequirementHandling.Some
			return ModConflictExceptions.Contains(modID);
		}

		private bool CanIgnoreModConflicts(IEnumerable<Guid> ids)
		{
			return ids.All(x => CanIgnoreModConflict(x));
		}


		private void ResolveModDependencies()
		{
			Manager.Info("Resolving mod dependencies...");

			//First, detect if any currently loaded mods conflict with any other loaded mods, and if so, remove it.
			//This is a bit awkward as the first step, considering that the conflict might be resolved due to
			// the other mod failing its requirements later on in the process, but I can't find an elegant way
			// to solve this.  Arbitrariness will have to do.
			foreach (var mod in LoadedMods.Values.ToList())
			{
				var conflicts = mod.ConflictingMods.Where(x => LoadedMods.ContainsKey(x));
				if (conflicts.Count() > 0)
				{
					Manager.Warn($"Mod '{mod.ModName}' declares it is incompatible with the following mods: {conflicts.ToCSVString()}.");
					if (CanIgnoreModConflict(mod.ModID))
					{
						Manager.Warn($"Due to config settings, the above conflicts are being ignored. This may cause problems.");
					}
					else
					{
						Manager.Info($"Mod '{mod.ModName}' is being unloaded due to conflict.  If this is not desired, alter the IgnoreModConflicts setting within {Constants.SettingsLocation}.");
						UnloadMod(mod);
					}
				}
			}

			//Next we check for cycles in the dependency graph.  
			var (sorted, cycles) = GetGraph(LoadedMods.Values).TarjanSort();

			//change this to continue, unload, or fail?
			if (cycles.Count() > 0)
			{
				Manager.Warn($"Warning! One or more circular dependencies in the requested mod load order have been detected.");
				Manager.Warn("The following cycles were found:");
				List<string> errorMessages = new List<string>();
				bool failure = false;
				foreach (var cycle in cycles)
				{
					var loop = cycle.Select(x => LoadedMods[x].ModName).ToList();
					//this shows the loop a bit better in the log; instead of just [D -> E -> A], it shows [D -> E -> A -> D]
					loop.Add(LoadedMods[cycle[0]].ModName);
					string message = $"[{loop.ToStringList(" -> ")}]";
					errorMessages.Add(message);
					Manager.Warn(message);
					failure = failure || !CanIgnoreCircularReferences(cycle);
				}

				if (failure)
				{
					throw new CircularReferenceException($"One or more disallowed circular mod dependencies were detected: {errorMessages.ToStringList()}.  This error can be ignored by setting PermitCircularDependencies to All in the XGEF_settings.json.");
				}
				else
				{
					Manager.Warn("Compilation will continue, but note that mod loading order is now essentially arbitrary for the affected mods.");
				}
			}

			//Lastly, now that mods have been determined to be compatible, we'll check to see if anyone is
			// missing a required mod (possibly due to flunking out of the above steps). 
			var mods = LoadedMods.Values.ToList();
			for (int i = 0; i < mods.Count; i++)
			{
				var mod = mods[i];
				if (!LoadedMods.ContainsKey(mod.ModID))
					continue;

				var missing = mod.RequiredMods.Except(LoadedMods.Keys);
				if (missing.Count() > 0)
				{
					Manager.Warn($"Mod '{mod.ModName}' declares it requires the existence of the following mods: {missing.ToCSVString()}.");
					if (CanIgnoreModRequirement(mod.ModID))
					{
						Manager.Warn($"Due to config settings, the above missing requirements are being ignored. This may cause problems.");
					}
					else
					{
						Manager.Warn($"Mod '{mod.ModName}' is being unloaded due to missing requirements.  If this is not desired, alter the IgnoreModRequirements setting within {Constants.SettingsLocation}.");
						UnloadMod(mod);
						//if a mod is removed, there's a possibility that another mod becomes invalidated because of it.
						//thus, we start the entire loop over again.
						i = 0;
					}
				}
			}
		}

		private void PrioritizeMods()
		{
			Manager.Info("Prioritizing loaded mods...");
			var modGroups = GetGraph(LoadedMods.Values).DepthSort();
			Manager.Debug($"Found {modGroups.Count()} separate groups of mod dependencies.");

			//the key is the index of the group within modGroups
			Dictionary<uint, Priority> groupPrios = new Dictionary<uint, Priority>();

			//Find the highest load priority of each group.
			foreach (var group in modGroups)
			{
				Guid highest = Guid.Empty;
				foreach (var id in group)
				{
					if (LoadedMods[id].RequestedLoadPriority > LoadedMods[highest].RequestedLoadPriority)
					{
						highest = id;
					}
				}
				groupPrios[(uint)modGroups.IndexOf(group)] = (Priority)LoadedMods[highest].RequestedLoadPriority;
			}

			var loadPriorities = new SortedDictionary<uint, ModInfo>();
			uint currentPriority = 0;
			loadPriorities[currentPriority++] = CoreMod;
			Manager.Debug($"CoreMod was assigned load priority {CoreMod.LoadPriority}");

			//Now that we've determined the highest load priority of each group, we're going to ensure that each mod has its 
			// individual priority assigned accordingly. This involves looping through each group and sequentially incrementing 
			// the load priority of each mod. 
			foreach (Priority prio in new Priority().GetValues())
			{
				var mods = groupPrios.Where(x => x.Value == prio);
				foreach (var pair in mods)
				{
					foreach (var id in modGroups[(int)pair.Key])
					{
						if (id == ModInfo.CoreModID)
							continue;
						var mod = LoadedMods[id];
						mod.SetLoadPriority(currentPriority++);
						loadPriorities[mod.LoadPriority] = mod;
						Manager.Debug($"Mod {mod.ModName} was assigned load priority {mod.LoadPriority}");
					}
				}
			}
			ModsByLoadPriority = new ReadOnlyDictionary<uint, ModInfo>(loadPriorities);

			var runPriorities = new SortedDictionary<uint, ModInfo>();
			currentPriority = 0;
			//Run priority is a bit simpler.  Simply sort by requested priority, and in the event of conflict, sort alphabetically.
			foreach (Priority prio in new Priority().GetValues().Reverse())
			{
				var mods = LoadedMods.Values.Where(x => (Priority)x.RequestedRunPriority == prio).ToList();
				mods.Sort((x, y) => { return x.ModName.CompareTo(y.ModName); });
				foreach (var mod in mods)
				{
					if (mod.IsCoreMod)
						continue;
					mod.RunPriority = currentPriority++;
					runPriorities[mod.LoadPriority] = mod;
					Manager.Debug($"Mod {mod.ModName} was assigned run priority {mod.RunPriority}");
				}
				//Set the CoreMod to have the highest Normal priority.
				if (prio == Priority.Normal)
				{
					CoreMod.RunPriority = currentPriority++;
					runPriorities[CoreMod.RunPriority] = CoreMod;
					Manager.Debug($"CoreMod was assigned run priority {CoreMod.RunPriority}");
				}
			}
			ModsByRunPriority = new ReadOnlyDictionary<uint, ModInfo>(loadPriorities);
		}

		private void UnloadModFiles()
		{

		}

		private void LoadModFiles()
		{
			foreach (var mod in ModsByLoadPriority.Values)
			{
				//Manager.Info($"Crawling engine folders for mod {mod.ModName}...");
				mod.CrawlEngine();
				//Manager.Info($"Crawling content folders for mod {mod.ModName}...");
				mod.CrawlContent();

				Manager.Debug($"Loading engine files for mod {mod.ModName}...");
				mod.LoadEngine();
				Index.AddFiles(mod.EngineLoader);
			}
		}

		private void AnalyzeMods()
		{
			Manager.Info("Analyzing Mods...");
			foreach(var file in Index.GetFilePathsOfType(FileType.Code))
			{
				CodeFile final = new CodeFile(file, uint.MaxValue, "[[compiled]]");
				final.SetData(Index.GetHighestFile<CodeFile>(file).Data);
				Index.AddFile(final);
			}
		}

		private void WorkspaceSetup()
		{
			Manager.Info("Setting up compilation workspace...");
			Workspace = new CSWorkspace();

			foreach (var mod in ModsByLoadPriority.Values)
			{
				if (mod.EngineLoader == null)
					continue;
				//Is this right?  maybe just one big project
				Manager.Info($"Creating project for mod {mod.ModName}...");
				mod.Project = Workspace.AddProject(mod.ModName);

				foreach(string reference in mod.References)
				{
					Workspace.AddReference(reference);
				}
				
				foreach (string file in mod.EngineLoader?.Files?.Keys)
				{
					var code = Index.GetHighestFile(file) as CodeFile;
					if (code == null)
						continue;

					if(WriteConsolidatedFiles)
					{
						var fileinfo = new FileInfo(Path.Combine(ConsolidatedFilesLocation, file));
						fileinfo.Directory.Create();
						File.WriteAllText(fileinfo.FullName, code.Data);
					}

					var docID = Workspace.AddDocument(mod.Project, code.Filename, code.Data);
					var doc = Workspace.GetDocument(docID);

					foreach (var item in doc.Classes)
					{
						var classInfo = doc.SemanticModel.GetDeclaredSymbol(item);
						Manager.Debug($"Found class {classInfo.Name}");
					}
				}
			}
		}

		private void CompileMods()
		{
			Manager.Info("Compiling Mods...");
			var projectIds = Workspace.GetProjects();
			Assemblies = new Dictionary<string, Assembly>();

			List<CSDiagnostic> results = new List<CSDiagnostic>();
			foreach (var id in projectIds)
			{
				var (dll, errors) = Workspace.CompileProjectToDLL(id);

				if (errors != null)
				{
					Manager.Error($"Error while compiling {Workspace.GetAssemblyName(id)}");
					foreach (var error in errors)
					{
						Manager.Error(error.ToString());
						results.Add(error);
					}
					if (results.Any(x => x.ErrorLevel == CompilerErrorLevel.Error))
					{
						continue;
					}
				}
				Assemblies[dll.Location] = dll;
			}

			if (results.Any(x => x.ErrorLevel == CompilerErrorLevel.Error))
				throw new CompilationErrorException("There were errors while compiling mods.  Aborting.");

			Manager.Info("Compile complete");
		}

		private void PairMods()
		{
			SystemPairs = new Dictionary<CoreSystem, ModSystem>();

			Manager.Info("Searching for Systems that require a modded pair...");
			foreach (var system in Manager.Systems.Values)
			{
				CoreSystem ms = system as CoreSystem;
				if (ms == null || !ms.AutomaticallyPair || ms.ModSystemBase != null)
					continue;

				Manager.Info($"Found ModdableSystem {ms.GetType().ToString()}, which requires a modded {ms.ModdedSystemType.ToString()}.");
				SystemPairs[ms] = null;
			}

			Manager.Info("Searching for modded pairs...");
			foreach (var assembly in Assemblies.Values)
			{
				var types = assembly.GetExportedTypes();

				foreach (var t in types)
				{
					//var system = SystemHalves.Single().Key;
					//bool assignable = system.ModdedSystemType.IsAssignableFrom(t);
					//bool name = system.ModdedSystemName == t.Name;
					var system = SystemPairs.Where(x => x.Key.ModdedSystemType.IsAssignableFrom(t)
						&& x.Key.ModdedSystemName == t.Name).SingleOrDefault().Key;

					if (system == null)
						continue;

					Manager.Info($"Found class {t.FullName} in assembly {assembly.FullName}");
					SystemPairs[system] = Activator.CreateInstance(t) as ModSystem;
					var coreSystem = Manager.Systems.Values.Where(x => x == system) as CoreSystem;
					system.Pair(SystemPairs[system]);
				}
			}
		}



		public ModInfo GetModInfo(FileLoader loader)
		{
			if (!loader.ContainsFile(ModInfo.DefaultFilename))
				return null;

			ModInfo mod = ModInfo.FromFile(Path.Combine(loader.RootPath, ModInfo.DefaultFilename));
			mod.SetLocation(loader.RootPath);
			mod.CleanLists();

			return mod;
		}

		protected ModInfo GetCoreModInfo(XGEFSettings settings)
		{
			ModInfo core = ModInfo.FromFile(Path.Combine(CoreLocation, settings.EngineRelativePath, ModInfo.DefaultFilename));
			core.ModName = settings.CoreModAssemblyName;
			core.ModDescription = "A collection of default systems that make up the core of the game.";
			core.RequestedRunPriority = RequestedPriority.Normal;
			core.ModID = ModInfo.CoreModID;
			core.ModAuthor = "XGEF";

			core.SetLocation(CoreLocation);
			core.CleanLists();

			return core;
		}

		public static Graph<Guid> GetGraph(IEnumerable<ModInfo> list)
		{
			Graph<Guid> graph = new Graph<Guid>();

			foreach (var mod in list)
			{
				//if (mod.IsCoreMod)
				//	continue;

				graph.Add(mod.ModID);
			}

			foreach (var mod in list)
			{
				if (mod.IsCoreMod)
					continue;

				graph.AddDirEdge(mod.ModID, ModInfo.CoreModID);

				foreach (var before in mod.LoadTheseBefore)
				{
					if (before == ModInfo.CoreModID)
						continue;
					graph.AddDirEdge(mod.ModID, before);
				}

				foreach (var after in mod.LoadTheseAfter)
				{
					if (after == ModInfo.CoreModID)
						continue;

					graph.AddDirEdge(after, mod.ModID);
				}
			}

			return graph;
		}

		public void CompileFile(IFileData file)
		{
			if (file is CodeFile)
				CompileFile(file as CodeFile);
		}

		public void CompileFile(CodeFile file)
		{
			//file.Tree = CompileText(File.ReadAllText(file.AbsoluteLocation), file.Filename);
		}


	}
}