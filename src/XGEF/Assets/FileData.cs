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
using System.IO;
//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp;
//using Microsoft.CodeAnalysis.Text;

namespace XGEF
{
	public interface IFileData : IComparable
	{
		FileType DataType { get; }

		uint ModPriority { get; }

		string RelativeLocation { get; }
		string AbsoluteLocation { get; }

		string Filename { get; }
		string Extension { get; }

		bool Loaded { get; }

		void Load();
	}

	public abstract class FileData<T> : IFileData
	{
		public FileType DataType { get; protected set; }
		public uint ModPriority { get; protected set; }
		public IEnumerable<string> Extensions { get; protected set; }
		public string RelativeLocation { get; }
		public string AbsoluteLocation { get; }

		public string Filename { get { return Path.GetFileName(RelativeLocation); } }
		public string Extension { get { return Path.GetExtension(RelativeLocation); } }

		public T Data { get; protected set; }
		public bool Loaded { get; protected set; }

		public FileData(FileType dataType, string relativeLocation, uint priority = 0, string absoluteLoc = "")
		{
			DataType = dataType;
			RelativeLocation = relativeLocation;
			AbsoluteLocation = absoluteLoc;
			ModPriority = priority;
		}

		public void Load()
		{
			if (!Loaded)
			{
				DoLoad();
				Loaded = true;
			}
		}

		protected abstract void DoLoad();

		public int CompareTo(object obj)
		{
			if (obj is FileData<T> other)
			{
				if (other.ModPriority > this.ModPriority)
					return 1;

				if (other.ModPriority < this.ModPriority)
					return -1;

				return 0;
			}
			else
			{
				throw new ArgumentException("Cannot compare to a non FileData object.");
			}
		}

		public override string ToString()
		{
			return AbsoluteLocation;
		}
	}

	public class TextFile : FileData<string>
	{
		public TextFile(string relativeLocation, uint priority = 0, string absoluteLoc = "")
			: base(FileType.Text, relativeLocation, priority, absoluteLoc) { }

		protected override void DoLoad()
		{
			Data = File.ReadAllText(AbsoluteLocation);
		}
	}

	public class CodeFile : TextFile
	{
		public CodeFile(string relativeLocation, uint priority = 0, string absoluteLoc = "")
			: base(relativeLocation, priority, absoluteLoc)
		{
			DataType = FileType.Code;
		}

		public void SetData(string data)
		{
			Data = data;
		}

		//public IEnumerable<Diagnostic> Errors { get; protected set; }
		//public bool HasErrors { get { return !(Errors == null || Errors.Count() == 0); } }
		//public something Classes
		//public something Methods

		//private SourceText _source;
		//public SourceText Source
		//{
		//	get
		//	{
		//		if (_source == null && Data != null)
		//			_source = SourceText.From(Data);

		//		return _source;
		//	}
		//}

	}
}