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
using System.Diagnostics;
using Roslyn.Utilities;

using MSProjectID = Microsoft.CodeAnalysis.ProjectId;

namespace XGEF.Compiler
{
	/// <summary>
	/// An identifier that can be used to refer to the same <see cref="Project"/> across versions.
	/// </summary>
	[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
	public class ProjectID : BaseID, IEquatable<ProjectID>
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
			return this.Equals(obj as ProjectID);
		}

		public bool Equals(ProjectID other)
		{
			return
					!ReferenceEquals(other, null) &&
					this.ID == other.ID;
		}

		public static bool operator ==(ProjectID left, ProjectID right)
		{
			return EqualityComparer<ProjectID>.Default.Equals(left, right);
		}

		public static bool operator !=(ProjectID left, ProjectID right)
		{
			return !(left == right);
		}

		public override int GetHashCode()
		{
			return this.ID.GetHashCode();
		}

		public static implicit operator MSProjectID(ProjectID id)
		{
			return MSProjectID.CreateFromSerialized(id.ID, id.Name);
		}

		public static implicit operator ProjectID(MSProjectID id)
		{
			return new ProjectID(id.Id, id.ToString().Split(',')[0]);
		}

		public ProjectID() : base() { }
		public ProjectID(Guid guid, string debugName=null) : base(guid, debugName) { }
	}
}
