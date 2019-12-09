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

using MSDocumentID = Microsoft.CodeAnalysis.DocumentId;
using MSProjectID = Microsoft.CodeAnalysis.ProjectId;

namespace XGEF.Compiler
{
	[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
	public class DocumentID : BaseID, IEquatable<DocumentID>
	{
		public ProjectID ProjectID { get; }

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
			return this.Equals(obj as DocumentID);
		}

		public bool Equals(DocumentID other)
		{
			return
					!ReferenceEquals(other, null) &&
					this.ID == other.ID;
		}

		public static bool operator ==(DocumentID left, DocumentID right)
		{
			return EqualityComparer<DocumentID>.Default.Equals(left, right);
		}

		public static bool operator !=(DocumentID left, DocumentID right)
		{
			return !(left == right);
		}

		public override int GetHashCode()
		{
			return this.ID.GetHashCode();
		}

		public static implicit operator MSDocumentID(DocumentID id)
		{
			return MSDocumentID.CreateFromSerialized((MSProjectID)id.ProjectID, id.ID, id.Name);
		}

		public static implicit operator DocumentID(MSDocumentID id)
		{
			return new DocumentID((ProjectID)id.ProjectId, id.Id, id.ToString().Split(',')[0]);
		}

		public DocumentID() : base() { }
		public DocumentID(ProjectID pid, Guid guid=default(Guid), string debugName=null) 
			: base(guid, debugName)
		{
			ProjectID = pid;
		}
	}
}
