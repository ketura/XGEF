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
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

namespace XGEF
{
	public enum ExtensionFilterMode { None, Whitelist, Blacklist }

	public class FileLoader
	{
		public ExtensionFilterMode FilterMode { get; set; }
		public HashSet<string> ExtensionFilter { get; set; }

		public string RootPath { get; protected set; }
		//public string ParentPath { get; protected set; }
		//pairs the relative engine file structure with absolute system paths

		public Dictionary<string, IFileData> Files { get; protected set; }

		public uint DefaultPriority { get; set; }

		public static string NormalizePath(string path)
		{
			path = Path.GetFullPath(path);
			path = Path.GetFullPath(new Uri(path).LocalPath)
								 .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			return path.Replace('\\', '/');
		}

		public static string NormalizeCombine(params string[] paths)
		{
			string combined = Path.Combine(paths);
			return NormalizePath(combined);
		}

		public FileLoader(string path, uint priority = (uint)Priority.Core)
		{
			RootPath = NormalizePath(path);
			DefaultPriority = priority;
			ExtensionFilter = new HashSet<string>();
		}

		public void Crawl()
		{
			if (string.IsNullOrWhiteSpace(RootPath))
			{
				Console.WriteLine("Cannot crawl FileSystem without a root being set.");
				return;
			}

			Files = new Dictionary<string, IFileData>();
			WalkDirectoryTree(new DirectoryInfo(RootPath));
		}

		//mostly taken from https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/file-system/how-to-iterate-through-a-directory-tree
		protected void WalkDirectoryTree(DirectoryInfo root)
		{
			FileInfo[] files;

			try
			{
				files = root.GetFiles("*", SearchOption.AllDirectories);
			}
			// This is thrown if even one of the files requires permissions greater
			// than the application provides.
			catch (UnauthorizedAccessException)
			{
				throw;
			}
			catch (DirectoryNotFoundException)
			{
				throw;
			}

			if (files != null)
			{
				foreach (FileInfo fi in files)
				{
					//switch (FilterMode)
					//{
					//	case ExtensionFilterMode.Whitelist:
					//		if (ExtensionFilter.Any(x => x == fi.Extension))
					//		{
					//			AddPath(fi.FullName);
					//		}
					//		break;

					//	case ExtensionFilterMode.Blacklist:
					//		if (!ExtensionFilter.Any(x => x == fi.Extension))
					//		{
					//			AddPath(fi.FullName);
					//		}
					//		break;

					//	case ExtensionFilterMode.None:
					//	default:
					AddPath(fi.FullName);
					//		break;
					//}

				}
			}
		}

		protected string TruncatePath(string path)
		{
			path = NormalizePath(path);
			if (path.Contains(RootPath))
				path = path.Replace(RootPath, "");

			return Regex.Replace(path, @"^/", "");
		}

		protected void AddPath(string fullpath)
		{
			Files[TruncatePath(fullpath)] = null;
		}


		public void LoadAllFiles()
		{
			if (Files == null)
				Crawl();

			foreach (string relativePath in Files.Keys.ToList())
			{
				LoadFile(relativePath);
			}
		}

		public void LoadFile(string relativePath)
		{
			if (!Files.ContainsKey(relativePath))
				return;

			try
			{
				string fullPath = NormalizeCombine(RootPath, relativePath);
				IFileData content = null;
				switch (Path.GetExtension(fullPath))
				{
					case ".cs":
						content = new CodeFile(relativePath, DefaultPriority, fullPath);
						break;
					default:
						content = new TextFile(relativePath, DefaultPriority, fullPath);
						break;
				}


				AddFile(relativePath, content);
			}
			catch (FileNotFoundException)
			{
				throw;
			}
		}

		protected void AddFile(string path, IFileData content)
		{
			content.Load();

			Files[path] = content;
		}

		public bool ContainsFile(string filename)
		{
			return Files.Keys.Contains(filename);
		}

		public bool ContainsAnyFile(string filename)
		{
			return Files.Any(x => Path.GetFileName(x.Key).ToLower() == filename.ToLower());
		}

		public IEnumerable<IFileData> GetFile(string filename)
		{
			return Files.Values.Where(x => x != null && x.Filename.ToLower() == filename.ToLower());
		}

		public IFileData GetFileAtPath(string path)
		{
			if (Files[path] == null)
				LoadFile(path);

			return Files[path];
		}

		//public void AddExtension(string ext)
		//{
		//	ExtensionFilter.Add(SanitizeExt(ext));
		//}

		//public void RemoveExtension(string ext)
		//{
		//	ExtensionFilter.Remove(SanitizeExt(ext));
		//}

		//protected string SanitizeExt(string ext)
		//{
		//	return Regex.Replace(ext, @"^\*?\.?", ".");
		//}

		//public Dictionary<string, List<IFileData>> GetAllFilesOfType(string ext)
		//{
		//	ext = SanitizeExt(ext);

		//	return FileContent.Where(x => SanitizeExt(Path.GetExtension(x.Key)) == ext)
		//		.ToDictionary(pair => pair.Key, pair => pair.Value);
		//}

		//public Dictionary<string, string> GetAllFilesInDirectory(string path)
		//{
		//	return DirectoryTree.Where(x => x.Key.StartsWith(path) && File.Exists(x.Value))
		//		.ToDictionary(pair => pair.Key, pair => pair.Value);
		//}

		//public List<string> GetAllDirsInDirectory(string path)
		//{
		//	return DirectoryTree.Keys.Where(x => x.StartsWith(path))
		//		.Select(x => x.Replace(path, "").Split('/').First()).ToList();
		//}

		//public List<string> GetAllTopLevelDirs()
		//{
		//	return DirectoryTree.Keys.Select(x => x.Split('/').First()).ToList();
		//}


	}
}