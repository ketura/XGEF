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
using System.Runtime.CompilerServices;

namespace XGEF.Common.Logging
{
	[Flags]
	public enum LogLevel
	{
		Debug = 0b00000001,
		Info =  0b00000010,
		Warn =  0b00000100,
		Error = 0b00001000
	}

	public interface ILogger
	{
		bool LogActive { get; set; }
		bool LogDebugInfo { get; set; }
		LogLevel LogOutputLevel { get; set; }

		void Debug(string message,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0);

		void Info(string message,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0);

		void Warn(string message,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0);
		
		void Error(string message, Exception ex = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0);
	}

	public class Logger : ILogger
	{
		public static Logger Instance { get; set; }

		static Logger()
		{
			Instance = new Logger();
		}

		protected Logger() { }


		public bool LogActive { get; set; } = true;
		public bool LogDebugInfo { get; set; } = false;
		public LogLevel LogOutputLevel { get; set; } = LogLevel.Debug;

		protected static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


		public static void Flush()
		{
			foreach (IAppender appender in LogManager.GetRepository("XGEFLogger").GetAppenders())
			{
				if (appender is BufferingAppenderSkeleton buffer)
				{
					buffer.Flush();
				}
			}
		}

		protected void Log(LogLevel level, string message, string memberName, string sourceFilePath, int sourceLineNumber)
		{
			if (!LogActive || !LogOutputLevel.HasFlag(level))
				return;

			if (LogDebugInfo)
			{
				message = $"{message}\n\n[{memberName}:{sourceLineNumber}@'{sourceFilePath}']";
			}

			switch (level.LowestFlagBit<LogLevel>())
			{

				case LogLevel.Debug:
					_log.Debug(message);
					break;
				case LogLevel.Info:
					_log.Info(message);
					break;
				case LogLevel.Warn:
					_log.Warn(message);
					break;
				default:
				case LogLevel.Error:
					_log.Error(message);
					break;
			}
		}

		public void Debug(string message,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			Log(LogLevel.Debug, message, memberName, sourceFilePath, sourceLineNumber);
		}

		public void Info(string message,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			Log(LogLevel.Info, message, memberName, sourceFilePath, sourceLineNumber);
		}

		public void Warn(string message,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			Log(LogLevel.Warn, message, memberName, sourceFilePath, sourceLineNumber);
		}

		public void Error(string message, Exception ex = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			if (ex != null)
			{
				message = $"{message}\n\nException Details:\n\n{ex.ToString()}";
			}
			Log(LogLevel.Error, message, memberName, sourceFilePath, sourceLineNumber);
		}
	}
}