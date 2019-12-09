///////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////
////                                                                               ////
/////    Copyright 2019-2020 Christian 'ketura' McCarty                             ////
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
using System.Diagnostics;
using Roslyn.Utilities;

using MSSolutionID = Microsoft.CodeAnalysis.SolutionId;

namespace XGEF.Compiler
{
	[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
	public class SolutionID : BaseID, IEquatable<SolutionID>
	{
		protected override string GetDebuggerDisplay()
		{
			return string.Format("({0}, #{1} - {2})", this.GetType().Name, this.ID, Name);
		}

		public override string ToString()
		{
			return GetDebuggerDisplay();
		}

		public override bool Equals(object obj)
		{
			return this.Equals(obj as SolutionID);
		}

		public bool Equals(SolutionID other)
		{
			return
					!ReferenceEquals(other, null) &&
					this.ID == other.ID;
		}

		public static bool operator ==(SolutionID left, SolutionID right)
		{
			return EqualityComparer<SolutionID>.Default.Equals(left, right);
		}

		public static bool operator !=(SolutionID left, SolutionID right)
		{
			return !(left == right);
		}

		public override int GetHashCode()
		{
			return this.ID.GetHashCode();
		}

		public static implicit operator MSSolutionID(SolutionID id)
		{
			return MSSolutionID.CreateFromSerialized(id.ID, id.Name);
		}

		public static implicit operator SolutionID(MSSolutionID id)
		{
			return new SolutionID(id.Id, id.ToString().Split(',')[0]);
		}

		public SolutionID() : base() { }
		public SolutionID(Guid guid, string debugName) : base(guid, debugName) { }
	}
}
