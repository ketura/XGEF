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


using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace XGEF.XMath
{
	public class GraphNode<T> : IEnumerable<T>
	{
		public bool Visited { get; set; }
		public int? Index { get; set; }
		public int Lowlink { get; set; }
		public T Value { get; protected set; }
		public List<T> Edges { get; protected set; }

		public void Add(T value)
		{
			if (!Edges.Contains(value))
				Edges.Add(value);
		}

		public void Add(GraphNode<T> other)
		{
			if (!Edges.Contains(other.Value))
				Edges.Add(other.Value);
		}

		public bool Remove(T value)
		{
			return Edges.Remove(value);
		}

		public bool Remove(GraphNode<T> node)
		{
			return Edges.Remove(node.Value);
		}

		public bool Contains(T value)
		{
			return Edges.Contains(value);
		}

		public bool Contains(GraphNode<T> node)
		{
			return Edges.Contains(node.Value);
		}

		public int Count { get { return Edges.Count; } }

		public IEnumerator<T> GetEnumerator() { return Edges.GetEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return Edges.GetEnumerator(); }

		public GraphNode() : this(default, null) { }
		public GraphNode(T data) : this(data, null) { }
		public GraphNode(T data, IEnumerable<T> list)
		{
			Index = null;
			Value = data;

			if (list == null)
				Edges = new List<T>();
			else
				Edges = list.ToList();
		}
	}
}
