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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XGEF
{
	public class AssetRegistry : Utility
	{
		public Dictionary<string, SortedDictionary<uint, IFileData>> Hierarchy { get; protected set; }

		public AssetRegistry()
		{
			Hierarchy = new Dictionary<string, SortedDictionary<uint, IFileData>>();
		}

		public void AddFile(IFileData file)
		{
			if (!Hierarchy.ContainsKey(file.RelativeLocation))
			{
				Hierarchy[file.RelativeLocation] = new SortedDictionary<uint, IFileData>();
			}

			if (!Hierarchy[file.RelativeLocation].ContainsKey(file.ModPriority))
			{
				Hierarchy[file.RelativeLocation][file.ModPriority] = file;
			}
		}

		public void AddFiles(FileLoader loader)
		{
			if (loader == null)
				return;

			foreach (var pair in loader.Files)
				AddFile(pair.Value);
		}

		public IEnumerable<IFileData> GetFiles(string path)
		{
			if (Hierarchy.ContainsKey(path))
				return Hierarchy[path].Values;

			return null;
		}

		public IEnumerable<string> GetFilesAsString(string path)
		{
			if (Hierarchy.ContainsKey(path))
				return Hierarchy[path].Values
					.Where(x => x.DataType == FileType.Code)
					.Select(x => ((CodeFile)x).Data);

			return null;
		}



		public IFileData GetHighestFile(string path)
		{
			if (Hierarchy.ContainsKey(path))
				return Hierarchy[path].LastOrDefault().Value;

			return null;
		}

		public TFileData GetHighestFile<TFileData>(string path)
			where TFileData : class, IFileData
			
		{
			if (Hierarchy.ContainsKey(path))
				return Hierarchy[path].LastOrDefault().Value as TFileData;

			return null;
		}

		public IFileData GetCoreFile(string path)
		{
			if (Hierarchy.ContainsKey(path))
				return Hierarchy[path].FirstOrDefault().Value;

			return null;
		}

		public IEnumerable<string> GetFilePathsOfType(FileType ftype)
		{
			return Hierarchy.Keys
				.Where(x => Hierarchy[x]
				.Any(y => y.Value.DataType == ftype));
		}
	}
}
