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
	/// <summary>
	/// Some assembly references themselves refer to other assemblies.  However, the build process will not
	/// automatically include these references-of-references, unless it detects they are being actively used.
	/// This is the place to put those dummy references, to force the relevant DLLs to be copied on build.
	/// </summary>
	internal class DummyReferences
	{
		public static void PrimeReferences()
		{ 
			//may need to add in a no-op lamda that takes a type, then loop through this and pass them all in
			Dictionary<string, Type> dummies = new Dictionary<string, Type>()
			{
				//["Microsoft.CodeAnalysis.CSharp"] = typeof(Microsoft.CodeAnalysis.CSharp.Formatting.CSharpFormattingOptions)
				//["System.IO.FileSystem"] = typeof(global::System.IO.FileInfo)
				["System.Reflection"] = typeof(global::System.Reflection.Assembly)
			};
		}

	}
}
