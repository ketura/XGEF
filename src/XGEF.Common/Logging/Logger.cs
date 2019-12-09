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
using System.Diagnostics;
using System.Reflection;

using log4net;
using log4net.Appender;
using System.Collections.Generic;

namespace XGEF.Common.Logging
{
	public enum LogLevel
	{
		Debug,
		Info,
		Warn,
		Error
	}

	public interface ILogger
	{
		bool LogActive { get; set; }
		LogLevel LogOutput { get; }
		void SetLogLevel(LogLevel level);

		string Name { get; }

		void Debug(string message, int level = 0);
		void Debug(params object[] messages);
		void Debug(int level, params object[] messages);
		void Info(string message, int level = 0);
		void Info(params object[] messages);
		void Info(int level, params object[] messages);
		void Warn(string message, int level = 0);
		void Warn(params object[] messages);
		void Warn(int level, params object[] messages);
		void Error(string message, int level = 0);
		void Error(string message, Exception ex, int level = 0);
		void Error(params object[] messages);
		void Error(int level, params object[] messages);

	}

	public class Logger : ILogger
	{
		public static Logger Instance { get; set; }

		static Logger()
		{
			Instance = new Logger();
		}


		public void SetLogLevel(LogLevel level)
		{
			Flush();

			switch (level)
			{
				case LogLevel.Debug:
					SetMassLogLevel(true, true, true, true);
					break;
				case LogLevel.Info:
					SetMassLogLevel(true, true, true, false);
					break;
				case LogLevel.Warn:
					SetMassLogLevel(true, true, false, false);
					break;
				case LogLevel.Error:
					SetMassLogLevel(true, false, false, false);
					break;
			}

			LogOutput = level;
		}

		protected void SetMassLogLevel(bool error, bool warn, bool info, bool debug)
		{
			EnabledLevels[LogLevel.Error] = error;
			EnabledLevels[LogLevel.Warn] = warn;
			EnabledLevels[LogLevel.Info] = info;
			EnabledLevels[LogLevel.Debug] = debug;
		}

		public Dictionary<LogLevel, bool> EnabledLevels { get; protected set; } = new Dictionary<LogLevel, bool>();
		public LogLevel LogOutput { get; protected set; }

		public bool LogActive { get; set; } = true;

		public string Name { get; protected set; } = "Logger";

		//Note that this will sssllooooooww every log call down.
		public bool IncludeCaller { get; set; } = true;

		public bool ForceFlushOnProcess { get; set; } = true;

		protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public Logger()
		{
			SetMassLogLevel(true, true, true, true);
		}

		public void Flush()
		{
			foreach (IAppender appender in LogManager.GetRepository("XGEFLogger").GetAppenders())
			{
				if (appender is BufferingAppenderSkeleton buffer)
				{
					buffer.Flush();
				}
			}
		}

		protected object[] PrependMessage(object[] messages, object message)
		{
			object[] newMessages = new object[messages.Length + 1];
			newMessages[0] = message;
			Array.Copy(messages, 0, newMessages, 1, messages.Length);
			return newMessages;
		}

		protected string ConcatMessages(object[] messages)
		{
			string combined = "";
			foreach (object obj in messages)
			{
				combined += obj.ToString();
			}

			return combined;
		}

		protected string GetCallerInfo(string message = "", int level = 0)
		{
			var caller = new StackTrace().GetFrame(3 + level).GetMethod();
			return $"[{caller.DeclaringType.FullName}->{caller.Name}]: {message}";
		}

		#region Log API pass-throughs

		public void Debug(string message, int level = 0)
		{
			if (LogActive && EnabledLevels[LogLevel.Debug])
			{
				if (IncludeCaller)
				{
					message = GetCallerInfo(message, level);
				}

				Log.Debug(message);
			}
		}

		public void Debug(int level, params object[] messages)
		{
			Debug(ConcatMessages(messages), level);
		}

		public void Debug(params object[] messages)
		{
			Debug(0, messages);
		}

		public void Info(string message, int level = 0)
		{
			if (LogActive && EnabledLevels[LogLevel.Info])
			{
				if (IncludeCaller)
				{
					message = GetCallerInfo(message, level);
				}

				Log.Info(message);
			}
		}

		public void Info(int level, params object[] messages)
		{
			Info(ConcatMessages(messages), level);
		}

		public void Info(params object[] messages)
		{
			Info(0, messages);
		}

		public void Warn(string message, int level = 0)
		{
			if (LogActive && EnabledLevels[LogLevel.Warn])
			{
				if (IncludeCaller)
				{
					message = GetCallerInfo(message);
				}

				Log.Warn(message);
			}
		}

		public void Warn(int level, params object[] messages)
		{
			Warn(ConcatMessages(messages), level);
		}

		public void Warn(params object[] messages)
		{
			Warn(0, messages);
		}

		public void Error(string message, int level = 0)
		{
			Error(message, null, level);
		}

		public void Error(string message, Exception ex = null, int level = 0)
		{
			if (LogActive && EnabledLevels[LogLevel.Error])
			{
				if (IncludeCaller)
				{
					message = GetCallerInfo(message);
				}

				if (ex != null)
					Log.Error(message, ex);
				else
					Log.Error(message);
			}
		}

		public void Error(int level, params object[] messages)
		{
			Error(ConcatMessages(messages), null, level);
		}

		public void Error(params object[] messages)
		{
			Error(0, messages);
		}
	}

	#endregion
}