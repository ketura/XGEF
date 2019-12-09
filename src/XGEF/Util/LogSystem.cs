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

using XGEF.Common.Logging;


namespace XGEF
{

	/// <summary>
	/// A wrapper for the external XGEF.Logging instance.
	/// </summary>
	public class LogSystem : Utility, ILogger
	{
		public bool LogActive { get; set; } = true;

		public bool IncludeCaller { get; set; } = true;

		public bool ForceFlushOnProcess { get; set; } = true;

		public LogLevel LogOutput { get; protected set; }

		protected static readonly Logger Log = Logger.Instance;

		public LogSystem()
		{
			Name = "LogSystem";
		}

		public void SetLogLevel(LogLevel level)
		{
			LogOutput = level;
			Log.SetLogLevel(level);
		}

		public override void LoadSettings(XGEFSettings settings)
		{
			IncludeCaller = settings.LogMethodTrace;
			Log.IncludeCaller = IncludeCaller;
			SetLogLevel(settings.LogOutput);
		}

		public override void Init() { }

		public override void Process()
		{
			if (ForceFlushOnProcess)
			{
				Log.Flush();
			}
		}

		#region Log API pass-throughs

		public void Debug(string message, int level = 0)
		{
			if (LogActive)
			{
				Log.Debug(message, level + 1);
			}
		}

		public void Debug(int level, params object[] messages)
		{
			if (LogActive)
			{
				Log.Debug(messages, level + 1);
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
				Log.Info(message, level + 1);
			}
		}

		public void Info(int level, params object[] messages)
		{
			if (LogActive)
			{
				Log.Info(messages, level + 1);
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
				Log.Warn(message, level + 1);
			}
		}

		public void Warn(int level, params object[] messages)
		{
			if (LogActive)
			{
				Log.Warn(messages, level + 1);
			}
		}

		public void Warn(params object[] messages)
		{
			Warn(0, messages);
		}

		public void Error(string message, int level = 0)
		{
			Error(message, null, 0);
		}

		public void Error(string message, Exception ex, int level = 0)
		{
			if (LogActive)
			{
				Log.Error(message, ex, level + 1);
			}
		}

		public void Error(int level, params object[] messages)
		{
			if (LogActive)
			{
				Log.Error(messages, level + 1);
			}
		}

		public void Error(params object[] messages)
		{
			Error(0, messages);
		}

		#endregion
	}
}