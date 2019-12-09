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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace XGEF.Compiler
{
	//this might actually need made
	public class CSDocument
	{
		protected Document Doc { get; set; }
		public string Name { get { return Doc.Name;  } }
		public SyntaxTree SyntaxTree { get { return Doc.GetSyntaxTreeAsync().Result; } }
		public SyntaxNode Root { get { return SyntaxTree.GetRoot(); } }
		public SemanticModel SemanticModel { get { return Doc.GetSemanticModelAsync().Result; } }
		public IEnumerable<ClassDeclarationSyntax> Classes { get { return GetNodesOfType<ClassDeclarationSyntax>(); } }

		public IEnumerable<T> GetNodesOfType<T>()
			where T : TypeDeclarationSyntax
		{
			return Root.DescendantNodes().OfType<T>();
		}

		public CSDocument(Document doc)
		{
			Doc = doc;
		}
	}
}
