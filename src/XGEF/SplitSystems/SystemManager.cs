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

using XGEF.Common.Logging;


namespace XGEF
{
	public partial class SystemManager
	{
		public Dictionary<string, SplitSystem> Systems { get; protected set; }
		public Dictionary<string, Utility> Utilities { get; protected set; }
		public List<ILogger> Loggers { get; protected set; }
		public bool LogActive { get; set; } = true;

		public XGEFSettings Settings { get; protected set; }

		public bool Initialized { get; protected set; }

		public static SystemManager Instance { get; private set; }
		static SystemManager()
		{
			Instance = LoadBasicSystems();
		}

		private static SystemManager LoadBasicSystems()
		{
			if(Instance == null)
			{
				Instance = XGEF.Preloader.LoadDefaultWithTypes();
			}
			
			//Instance.RegisterSystem(new CoreActionSystem());
			return Instance;
		}

		public SystemManager()
		{
			Systems = new Dictionary<string, SplitSystem>();
			Utilities = new Dictionary<string, Utility>();
			Initialized = false;
			Loggers = new List<ILogger>();

			Settings = XGEFSettings.ConsolidateFromFile(Constants.SettingsLocation);
			SetLogLevel(Settings.LogOutput);
		}

		public ISplitSystem this[string name]
		{
			get
			{
				if (Systems.ContainsKey(name))
					return Systems[name] as ISplitSystem;

				if (Utilities.ContainsKey(name))
					return Utilities[name] as ISplitSystem;

				return null;
			}
		}

		public ISplitSystem this[Type t]
		{
			get
			{
				return this[t.Name];
			}
		}

		public T GetSystem<T>(string name)
			where T : class, ISplitSystem
		{
			return this[name] as T;
		}

		public T GetSystem<T>()
			where T : class, ISplitSystem
		{
			return this[typeof(T).Name] as T;
		}

		public void PairSystems<TCore, TMod>(TMod modSystem)
			where TCore : CoreSystem
			where TMod  : ModSystem
		{
			GetSystem<TCore>().Pair(modSystem);
		}

		public void RegisterSystems(params ISplitSystem[] systems)
		{
			RegisterSystems(systems.ToList());
		}

		public void RegisterSystems(IEnumerable<ISplitSystem> systems)
		{
			foreach (ISplitSystem s in systems)
			{
				RegisterSystem(s);
			}
		}

		public void RegisterSystem(ISplitSystem s)
		{
			if (Initialized)
				throw new InvalidOperationException($"Cannot register system {s.ToString()}; this SystemManager has already been initialized!");

			if (string.IsNullOrWhiteSpace(s.Name))
				throw new ArgumentException($"System {s.ToString()} has no name set and cannot be registered!");

			if (Systems.ContainsKey(s.Name))
				throw new ArgumentException($"System {s.Name} has already been registered!");

			if (s is SplitSystem sys)
			{
				Systems[sys.Name] = sys;
				sys.Manager = this;
			}

			if (s is Utility util)
			{
				Utilities[util.Name] = util;
				util.Manager = this;
			}


			if (s is ILogger logger)
			{
				Loggers.Add(logger);
				logger.SetLogLevel(LogOutput);
			}

		}

		public void Init()
		{
			if (Initialized)
			{
				throw new InvalidOperationException("Cannot initialize SystemManager twice!");
			}

			DoInit();
			Initialized = true;
		}

		protected virtual void DoInit()
		{
			foreach (Utility u in Utilities.Values)
			{
				u.LoadSettings(Settings);
			}
			foreach (SplitSystem s in Systems.Values)
			{
				s.LoadSettings(Settings);
			}


			foreach (Utility u in Utilities.Values)
			{
				u.PreInit();
			}

			foreach (Utility u in Utilities.Values)
			{
				u.Init();
			}

			foreach (Utility u in Utilities.Values)
			{
				u.PostInit();
			}


			foreach (SplitSystem s in Systems.Values)
			{
				s.PreInit();
			}

			foreach (SplitSystem s in Systems.Values)
			{
				s.Init();
			}

			foreach (SplitSystem s in Systems.Values)
			{
				s.PostInit();
			}
		}

		public void Process()
		{
			if (!Initialized)
				throw new InvalidOperationException("SystemManager cannot process before it has been initialized!");

			foreach (SplitSystem s in Systems.Values)
			{
				s.PreProcess();
			}

			foreach (SplitSystem s in Systems.Values)
			{
				s.Process();
			}

			foreach (SplitSystem s in Systems.Values)
			{
				s.PostProcess();
			}
		}

	}

	#region Log Pass-throughs

	public partial class SystemManager : ILogger
	{
		public string Name { get { return "SystemManager"; } }

		public LogLevel LogOutput { get; protected set; }
		public void SetLogLevel(LogLevel level)
		{
			LogOutput = level;
			foreach (var logger in Loggers)
			{
				logger.SetLogLevel(level);
			}
		}

		public void Debug(string message, int level = 0)
		{
			if (LogActive)
			{
				if (Loggers.Count == 0)
				{
					Console.WriteLine(message);
				}
				else
				{
					foreach (var logger in Loggers)
						logger.Debug(message, level);
				}
			}
		}

		public void Debug(int level, params object[] messages)
		{
			if (LogActive)
			{
				if (Loggers.Count == 0)
				{
					Console.WriteLine(messages);
				}
				else
				{
					foreach (var logger in Loggers)
						logger.Debug(messages);
				}
			}
		}

		public void Debug(params object[] messages)
		{
			Debug(0, messages);
		}

		public void Info(string message, int level = 0)
		{
			if (LogActive)
			{
				if (Loggers.Count == 0)
				{
					Console.WriteLine(message);
				}
				else
				{
					foreach (var logger in Loggers)
						logger.Info(message, level);
				}
			}
		}

		public void Info(int level, params object[] messages)
		{
			if (LogActive)
			{
				if (Loggers.Count == 0)
				{
					Console.WriteLine(messages);
				}
				else
				{
					foreach (var logger in Loggers)
						logger.Info(messages);
				}
			}
		}

		public void Info(params object[] messages)
		{
			Info(0, messages);
		}

		public void Warn(string message, int level = 0)
		{
			if (LogActive)
			{
				if (Loggers.Count == 0)
				{
					Console.WriteLine(message);
				}
				else
				{
					foreach (var logger in Loggers)
						logger.Warn(message, level);
				}
			}
		}

		public void Warn(int level, params object[] messages)
		{
			if (LogActive)
			{
				if (Loggers.Count == 0)
				{
					Console.WriteLine(messages);
				}
				else
				{
					foreach (var logger in Loggers)
						logger.Warn(messages);
				}
			}
		}

		public void Warn(params object[] messages)
		{
			Warn(0, messages);
		}

		public void Error(string message, int level = 0)
		{
			if (LogActive)
			{
				if (Loggers.Count == 0)
				{
					Console.WriteLine(message);
				}
				else
				{
					foreach (var logger in Loggers)
						logger.Error(message, level);
				}
			}
		}

		public void Error(int level, params object[] messages)
		{
			if (LogActive)
			{
				if (Loggers.Count == 0)
				{
					Console.WriteLine(messages);
				}
				else
				{
					foreach (var logger in Loggers)
						logger.Error(messages);
				}
			}
		}

		public void Error(params object[] messages)
		{
			Error(0, messages);
		}

		public void Error(string message, Exception ex, int level = 0)
		{
			if (LogActive)
			{
				if (Loggers.Count == 0)
				{
					Console.WriteLine(message);
				}
				else
				{
					foreach (var logger in Loggers)
						logger.Error(message, ex, level);
				}
			}
		}
	}

	#endregion
}