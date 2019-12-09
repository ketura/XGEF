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

namespace XGEF.XMath
{
	public static class TopSort
	{
		//https://en.wikipedia.org/wiki/Topological_sorting#Algorithms
		//This will sort an acyclic graph, but will completely ignore any cyclical sections.  
		//It should be noted that this is a destructive algorithm.  The graph will be useless after use.
		public static List<T> KahnSort<T>(this Graph<T> graph)
		{
			List<T> sorted = new List<T>();
			Stack<T> nodes = new Stack<T>();

			graph.UnmarkAll();

			foreach (var node in graph.GetStartNodes())
			{
				nodes.Push(node.Value);
			}

			int count = graph.Count;

			while (nodes.Count > 0)
			{
				var node = nodes.Pop();
				sorted.Add(node);
				graph.Remove(node);
				var newnodes = graph.GetStartNodes().Select(x => x.Value);
				var except = newnodes.Except(nodes);
				foreach (var newnode in graph.GetStartNodes().Select(x => x.Value).Except(nodes))
				{
					nodes.Push(newnode);
				}

				count--;
				if (count < 0)
					throw new Exception("There is a cycle; more iterations than count");
			}
			return sorted;
		}

		//https://en.wikipedia.org/wiki/Topological_sorting#Depth-first_search
		public static List<List<T>> DepthSort<T>(this Graph<T> graph)
		{
			List<T> sorted = new List<T>();
			Stack<T> nodes = new Stack<T>();

			graph.UnmarkAll();

			foreach (var node in graph.GetUnmarkedNodes())
			{
				nodes.Push(node.Value);
			}

			List<List<T>> groups = new List<List<T>>();

			foreach (var node in graph.Nodes.ToList())
			{
				if (!node.Visited)
				{
					Visit(graph, node, sorted);
					groups.Add(sorted.ToList());
					sorted = new List<T>();
				}
			}

			return groups;
		}

		private static void Visit<T>(Graph<T> graph, GraphNode<T> node, List<T> sorted)
		{
			if (node.Visited)
				return;

			node.Visited = true;

			foreach (var childID in node.Edges)
			{
				var child = graph.GetNode(childID);
				if (!child.Visited)
					Visit(graph, child, sorted);
			}

			sorted.Add(node.Value);
		}

		//https://en.wikipedia.org/wiki/Tarjan%27s_strongly_connected_components_algorithm
		public static (IEnumerable<T> sorted, List<List<T>> cycles) TarjanSort<T>(this Graph<T> graph)
		{
			int index = 0;
			Stack<GraphNode<T>> stack = new Stack<GraphNode<T>>();

			graph.UnmarkAll();

			var output = new List<List<GraphNode<T>>>();

			foreach (var node in graph.Nodes.ToList())
			{
				if (!node.Index.HasValue)
				{
					StrongConnect(node, graph, ref index, stack, output);
				}
			}

			var sorted = new List<T>();
			var cycles = new List<List<T>>();
			foreach (var list in output)
			{
				sorted = sorted.Concat(list.Select(x => x.Value)).ToList();
				if (list.Count > 1)
					cycles.Add(list.Select(x => x.Value).ToList());
			}

			return (sorted, cycles);
		}

		private static void StrongConnect<T>(GraphNode<T> node, Graph<T> graph, ref int index, Stack<GraphNode<T>> stack, List<List<GraphNode<T>>> output)
		{
			node.Index = index;
			node.Lowlink = index;
			index++;
			stack.Push(node);
			//using Visited to mean "onStack"
			node.Visited = true;

			foreach (var id in node.Edges)
			{
				var w = graph.GetNode(id);
				if (!w.Index.HasValue)
				{
					StrongConnect(w, graph, ref index, stack, output);
					node.Lowlink = Math.Min(node.Lowlink, w.Lowlink);
				}
				else if (w.Visited)
				{
					node.Lowlink = Math.Min(node.Lowlink, w.Index.Value);
				}
			}

			if (node.Lowlink == node.Index)
			{
				List<GraphNode<T>> component = new List<GraphNode<T>>();
				GraphNode<T> w;
				do
				{
					w = stack.Pop();
					w.Visited = false;
					component.Add(w);
				} while (w != node);

				output.Add(component);
			}
		}
	}
}
