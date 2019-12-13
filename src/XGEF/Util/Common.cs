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

namespace XGEF
{
	public static class Constants
	{
		public static readonly string CoreModName = "CoreMod";
		public static readonly string SettingsLocation = "XGEF_Settings.json";
	}

	public interface INameInfo
	{
		string Name { get; }
		string Description { get; }
		string Notes { get; }

		void SetName(string name);
		void SetDescription(string desc);
		void SetNotes(string notes);
	}

	public class NameInfo : INameInfo
	{
		public string Name { get; protected set; }
		public string Description { get; protected set; }
		public string Notes { get; protected set; }

		public void SetDescription(string desc) { Description = desc; }
		public void SetName(string name) { Name = name; }
		public void SetNotes(string notes) { Notes = notes; }

		public NameInfo(string name, string desc = null, string notes = null)
		{
			Name = name;
			Description = desc;
			Notes = notes;
		}
	}

	public interface IOwningModInfo
	{
		string ModName { get; }
		Priority Priority { get; }
		void SetPriority(Priority priority);
	}

	public class OwningModInfo : IOwningModInfo
	{
		public string ModName { get; protected set; }
		public Priority Priority { get; protected set; }

		public void SetPriority(Priority priority) { Priority = priority; }
		public OwningModInfo(string name = "Core", Priority prio = Priority.Normal)
		{
			ModName = name;
			Priority = prio;
		}
	}

	public class StringInfo : INameInfo, IOwningModInfo
	{
		public string Name { get; protected set; }
		public string Description { get; protected set; }
		public string Notes { get; protected set; }
		public string ModName { get; protected set; }
		public Priority Priority { get; protected set; }
		public string Value { get; set; }

		public void SetPriority(Priority priority) { Priority = priority; }
		public void SetDescription(string desc) { Description = desc; }
		public void SetName(string name) { Name = name; }
		public void SetNotes(string notes) { Notes = notes; }
		public StringInfo(string name, string value, string modname, Priority prio = Priority.Normal, string desc = null, string notes = null)
		{
			Name = name;
			Value = value;
			ModName = modname;
			Priority = prio;
			Description = desc;
			Notes = notes;
		}
	}

	public static class CommonUtils
	{
		public static byte[] ConsolidateBitstreams(IEnumerable<byte[]> arrays)
		{
			byte[] data = new byte[arrays.Sum(x => x.Length)];
			int offset = 0;

			foreach (byte[] array in arrays)
			{
				Buffer.BlockCopy(array, 0, data, offset, array.Length);
				offset += array.Length;
			}

			return data;
		}
	}



	public enum FileType { Code, Text, Image, Audio, Binary }

	public enum RequestedPriority
	{
		Lowest = 10922,
		Low = 21844,
		Normal = 32767,
		High = 43689,
		Highest = 54611
	}

	public enum Priority
	{
		Core = 0,
		Lowest = 10922,
		Low = 21844,
		Normal = 32767,
		High = 43689,
		Highest = 54611,
		Final = ushort.MaxValue
	}

	public enum RequirementHandling
	{
		None,
		Specify,
		All
	}

}