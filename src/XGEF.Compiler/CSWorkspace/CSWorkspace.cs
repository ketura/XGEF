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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Text;

using MSSolutionID = Microsoft.CodeAnalysis.SolutionId;
using MSProjectID = Microsoft.CodeAnalysis.ProjectId;
using MSDocumentID = Microsoft.CodeAnalysis.DocumentId;
using System.IO;
using System.Reflection;

namespace XGEF.Compiler
{
	public class CSWorkspace 
	{
		public AdhocWorkspace Workspace { get; protected set; }

		public List<string> References { get; protected set; }

		public bool CanApplyChange(ApplyChangesKind feature) { return true;  }

		public bool CanOpenDocuments => true;

		/// <summary>
		/// Clears all projects and documents from the workspace.
		/// </summary>
		public void ClearSolution()
		{
			Workspace.ClearSolution();
		}

		public void AddReference(string path)
		{
			References.Add(path);
		}

		public void RemoveReference(string path)
		{
			References.Remove(path);
		}

		/// <summary>
		/// Adds an entire solution to the workspace, replacing any existing solution.
		/// </summary>
		public Solution AddSolution(SolutionInfo solutionInfo)
		{
			return Workspace.AddSolution(solutionInfo);
		}

		/// <summary>
		/// Adds a project to the workspace. All previous projects remain intact.
		/// </summary>
		public ProjectID AddProject(string name)
		{
			return Workspace.AddProject(name, LanguageNames.CSharp).Id;
		}

		/// <summary>
		/// Adds a document to the workspace.
		/// </summary>
		public DocumentID AddDocument(ProjectID projectId, string name, string text)
		{
			return Workspace.AddDocument(projectId, name, SourceText.From(text)).Id;
		}

		public IEnumerable<ProjectID> GetProjects()
		{
			var ids = Workspace.CurrentSolution.GetProjectDependencyGraph().GetTopologicallySortedProjects();
			var list = new List<ProjectID>();
			foreach (var id in ids)
			{
				list.Add(id);
			}
			return list;
		}

		public string GetAssemblyName(ProjectID id)
		{
			return Workspace.CurrentSolution.GetProject(id).AssemblyName;
		}

		public (Assembly DLL, IEnumerable<CSDiagnostic> ErrorMessages) CompileProjectToDLL(ProjectID id, IEnumerable<string> assemblyFilenames = null)
		{
			if (assemblyFilenames == null)
				assemblyFilenames = References;
			//(Assembly DLL, IEnumerable<Diagnostic> ErrorMessages)
			Assembly dll = null;
			IEnumerable<Diagnostic> compileErrors = null;

			var references = new List<PortableExecutableReference>()
			{
				MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
			};

			foreach(string name in assemblyFilenames)
			{
				references.Add(MetadataReference.CreateFromFile(name));
			}

			var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
			Project project = Workspace.CurrentSolution.GetProject(id);
			project = project.WithCompilationOptions(options).WithMetadataReferences(references);
			using (var stream = new MemoryStream())
			{
				EmitResult result = project.GetCompilationAsync().Result.Emit(stream);
				if (!result.Success)
				{
					compileErrors = result.Diagnostics;
				}
				else
				{
					string filename = $"{GetAssemblyName(id)}.dll";
					using (FileStream file = File.Create(filename))
					{
						stream.Seek(0, SeekOrigin.Begin);
						stream.CopyTo(file);
					}
					dll = Assembly.LoadFrom(Path.GetFullPath(filename));
				}
			}

			return (dll, compileErrors?.Select(x => new CSDiagnostic(x)));
		}

		public IEnumerable<CSDocument> GetProjectDocuments(ProjectID project)
		{
			var docs = Workspace.CurrentSolution.GetProject(project).Documents;
			var list = new List<CSDocument>();
			foreach(var doc in docs)
			{
				list.Add(new CSDocument(doc));
			}
			return list;
		}

		//hmm, should this be cached?
		public CSDocument GetDocument(DocumentID did)
		{
			return new CSDocument(Workspace.CurrentSolution.GetDocument(did));
		}


		/// <summary>
		/// Puts the specified document into the open state.
		/// </summary>
		public void OpenDocument(DocumentID documentId, bool activate = true)
		{
			Workspace.OpenDocument((MSDocumentID)documentId, activate);
		}

		/// <summary>
		/// Puts the specified document into the closed state.
		/// </summary>
		public void CloseDocument(DocumentID documentId)
		{
			Workspace.CloseDocument((MSDocumentID)documentId);
		}

		/// <summary>
		/// Puts the specified additional document into the open state.
		/// </summary>
		public void OpenAdditionalDocument(DocumentID documentId, bool activate = true)
		{
			Workspace.OpenAdditionalDocument((MSDocumentID)documentId, activate);
		}

		/// <summary>
		/// Puts the specified additional document into the closed state
		/// </summary>
		public void CloseAdditionalDocument(DocumentID documentId)
		{
			Workspace.CloseAdditionalDocument((MSDocumentID)documentId);
		}

		protected void Init()
		{
			try
			{
				Workspace = new AdhocWorkspace();
				Workspace.AddSolution(SolutionInfo.Create(MSSolutionID.CreateNewId(), VersionStamp.Default));
				References = new List<string>(DefaultReferences);
			}
			catch (ReflectionTypeLoadException ex)
			{
				StringBuilder sb = new StringBuilder();
				foreach (Exception exSub in ex.LoaderExceptions)
				{
					sb.AppendLine(exSub.Message);
					if (exSub is FileNotFoundException exFileNotFound)
					{
						if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
						{
							sb.AppendLine("Fusion Log:");
							sb.AppendLine(exFileNotFound.FusionLog);
						}
					}
					sb.AppendLine();
				}
				string errorMessage = sb.ToString();
				//Display or log the error based on your application.
				throw;
			}
			
		}

		public static readonly List<string> DefaultReferences = new List<string>()
		{
			"XGEF/XGEF.dll",
			"XGEF/XGEF.Core.dll",
			"XGEF/XGEF.Compiler.dll",
			"XGEF/XGEF.Common.dll",
			"XGEF/Esent.Interop.dll",
			"XGEF/log4net.dll",
			"XGEF/Microsoft.CodeAnalysis.CSharp.dll",
			"XGEF/Microsoft.CodeAnalysis.CSharp.Workspaces.dll",
			"XGEF/Microsoft.CodeAnalysis.dll",
			"XGEF/Microsoft.CodeAnalysis.Workspaces.Desktop.dll",
			"XGEF/Microsoft.CodeAnalysis.Workspaces.dll",
			"XGEF/Microsoft.CSharp.dll",
			"XGEF/MiscUtil.dll",
			"XGEF/Newtonsoft.Json.dll",
			"XGEF/System.AppContext.dll",
			"XGEF/System.Collections.Immutable.dll",
			"XGEF/System.ComponentModel.Composition.dll",
			"XGEF/System.Composition.AttributedModel.dll",
			"XGEF/System.Composition.Convention.dll",
			"XGEF/System.Composition.Hosting.dll",
			"XGEF/System.Composition.Runtime.dll",
			"XGEF/System.Composition.TypedParts.dll",
			"XGEF/System.Console.dll",
			"XGEF/System.Core.dll",
			"XGEF/System.Data.DataSetExtensions.dll",
			"XGEF/System.Data.dll",
			"XGEF/System.Diagnostics.FileVersionInfo.dll",
			"XGEF/System.Diagnostics.StackTrace.dll",
			"XGEF/System.dll",
			"XGEF/System.IO.Compression.dll",
			"XGEF/System.IO.FileSystem.dll",
			"XGEF/System.IO.FileSystem.Primitives.dll",
			"XGEF/System.Numerics.dll",
			"XGEF/System.Reflection.dll",
			"XGEF/System.Reflection.Metadata.dll",
			"XGEF/System.Runtime.dll",
			"XGEF/System.Runtime.Extensions.dll",
			"XGEF/System.Runtime.InteropServices.dll",
			"XGEF/System.Security.Cryptography.Algorithms.dll",
			"XGEF/System.Security.Cryptography.Encoding.dll",
			"XGEF/System.Security.Cryptography.Primitives.dll",
			"XGEF/System.Security.Cryptography.X509Certificates.dll",
			"XGEF/System.Text.Encoding.CodePages.dll",
			"XGEF/System.Threading.Thread.dll",
			"XGEF/System.ValueTuple.dll",
			"XGEF/System.Xml.dll",
			"XGEF/System.Xml.Linq.dll",
			"XGEF/System.Xml.ReaderWriter.dll",
			"XGEF/System.Xml.XmlDocument.dll",
			"XGEF/System.Xml.XPath.dll",
			"XGEF/System.Xml.XPath.XDocument.dll",
			"XGEF/netstandard.dll"
		};

		public CSWorkspace()
		{
			Init();
		}
	}
}
