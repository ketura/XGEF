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

namespace XGEF.XMath
{
	public class Graph<T>
	{
		public List<GraphNode<T>> Nodes { get; protected set; }

		public void Add(GraphNode<T> node)
		{
			Nodes.Add(node);
		}

		public GraphNode<T> Add(T value)
		{
			var node = new GraphNode<T>(value);
			Nodes.Add(node);
			return node;
		}

		public bool Remove(T value)
		{
			var node = Nodes.FirstOrDefault(x => x.Value.Equals(value));
			if (node == null)
				return false;
			return Remove(node);
		}

		public bool Remove(GraphNode<T> node)
		{
			if (!Nodes.Contains(node))
				return false;

			Nodes.Remove(node);

			foreach (var other in Nodes)
			{
				other.Remove(node);
			}

			return true;
		}

		public void AddDirEdge(GraphNode<T> from, T to) { from.Add(to); }
		public void AddDirEdge(GraphNode<T> from, GraphNode<T> to) { from.Add(to); }
		public void AddDirEdge(T from, GraphNode<T> to) { AddDirEdge(from, to.Value); }
		public void AddDirEdge(T from, T to)
		{
			var node = GetNode(from);
			if (node != null && this.Contains(to))
				node.Add(to);
		}

		public bool Contains(GraphNode<T> node)
		{
			return Nodes.Contains(node);
		}

		public bool Contains(T value)
		{
			return Nodes.Any(x => x.Value.Equals(value));
		}

		public int Count { get { return Nodes.Count; } }

		public GraphNode<T> GetNode(T value)
		{
			return Nodes.FirstOrDefault(x => x.Value.Equals(value));
		}

		public IEnumerable<GraphNode<T>> GetStartNodes()
		{
			List<GraphNode<T>> list = Nodes.ToList();

			foreach (var node in Nodes)
			{
				list.RemoveAll(x => node.Contains(x));
			}

			return list;
		}

		public IEnumerable<GraphNode<T>> GetEndNodes()
		{
			return Nodes.Where(x => x.Edges.Count == 0).ToList();
		}

		public IEnumerable<GraphNode<T>> GetUnmarkedNodes()
		{
			return Nodes.Where(x => !x.Visited).ToList();
		}

		public void UnmarkAll()
		{
			foreach (var node in Nodes)
			{
				node.Visited = false;
				node.Index = null;
				node.Lowlink = 0;
			}
		}


		public Graph() : this(null) { }
		public Graph(IEnumerable<T> data)
		{
			Nodes = new List<GraphNode<T>>();
			if (data != null)
			{
				Nodes.AddRange(data.Select(x => new GraphNode<T>(x)));
			}
		}
	}
}
