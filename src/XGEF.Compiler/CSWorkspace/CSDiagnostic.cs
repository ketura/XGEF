
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
	public enum CompilerErrorLevel
	{
		Hidden = 0,
		Info = 1,
		Warning = 2,
		Error = 3
	}


	public class CSDiagnostic
	{
		protected Diagnostic Diag { get; set; }

		public string ID { get { return Diag.Id; } }
		public bool Supressed { get { return Diag.IsSuppressed; } }
		public string File { get { return Diag.Location.GetLineSpan().Path; } }
		public int LineNumber { get { return Diag.Location.GetMappedLineSpan().StartLinePosition.Line + 1; } }
		public int Column { get { return Diag.Location.GetMappedLineSpan().StartLinePosition.Character + 1; } }
		public CompilerErrorLevel ErrorLevel { get { return ConvertDiagnosticLevel(Diag.Severity); } }
		public string Message { get { return Diag.GetMessage(); } }
		public string DebugInfo { get { return Diag.ToString(); } }

		public CSDiagnostic(Diagnostic d)
		{
			Diag = d;
		}

		public static CompilerErrorLevel ConvertDiagnosticLevel(DiagnosticSeverity ds)
		{
			return (CompilerErrorLevel)((int)ds);
		}

		public override string ToString()
		{
			return $"{ErrorLevel.ToString()} {ID} {File}({LineNumber}, {Column}) {Message}";
		}
	}
}
