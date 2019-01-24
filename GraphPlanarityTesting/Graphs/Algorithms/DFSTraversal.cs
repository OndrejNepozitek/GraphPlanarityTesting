namespace GraphPlanarityTesting.Graphs.Algorithms
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DataStructures;

	public class DFSTraversal
	{
		public void TraverseRecursive<T>(IGraph<T> graph, T startVertex, IDFSTraversalVisitor<T> visitor)
		{
			var vertices = graph.Vertices.ToList();
			var states = new Dictionary<T, State>();

			foreach (var vertex in vertices)
			{
				states.Add(vertex, State.NotDiscovered);
				visitor.InitializeVertex(vertex, graph);
			}

			visitor.StartVertex(startVertex, graph);
			TraverseRecursive(graph, startVertex, visitor, states);

			foreach (var vertex in vertices)
			{
				if (states[vertex] == State.NotDiscovered)
				{
					visitor.StartVertex(vertex, graph);
					TraverseRecursive(graph, vertex, visitor, states);
				}
			}
		}

		private void TraverseRecursive<T>(IGraph<T> graph, T vertex, IDFSTraversalVisitor<T> visitor, Dictionary<T, State> states)
		{
			states[vertex] = State.Discovered;
			visitor.DiscoverVertex(vertex, graph);

			foreach (var neighbour in graph.GetNeighbours(vertex))
			{
				var edge = new Edge<T>(vertex, neighbour);
				visitor.ExamineEdge(edge, graph);
				var neighbourState = states[neighbour];

				if (neighbourState == State.NotDiscovered)
				{
					visitor.TreeEdge(edge, graph);
					TraverseRecursive(graph, neighbour, visitor, states);
				}
				else if (neighbourState == State.Discovered)
				{
					visitor.BackEdge(edge, graph);
				}
				else
				{

				}

				visitor.FinishEdge(edge, graph);
			}

			states[vertex] = State.Closed;
			visitor.FinishVertex(vertex, graph);
		}

		public void Traverse<T>(IGraph<T> graph, T startVertex, IDFSTraversalVisitor<T> visitor)
		{
			throw new NotImplementedException();

			var vertices = graph.Vertices.ToList();
			var states = new Dictionary<T, State>();
			var stack = new Stack<VertexInfo<T>>();

			foreach (var vertex in vertices)
			{
				states.Add(vertex, State.NotDiscovered);
				visitor.InitializeVertex(vertex, graph);
			}

			stack.Push(new VertexInfo<T>(startVertex, null));
			states[startVertex] = State.Discovered;

			while (stack.Count != 0)
			{
				var vertexInfo = stack.Pop();
				var vertex = vertexInfo.Vertex;

				foreach (var neighbour in graph.GetNeighbours(vertex))
				{
					var edge = new Edge<T>(vertex, neighbour);
					visitor.ExamineEdge(edge, graph);
					var neighbourState = states[neighbour];

					if (neighbourState == State.NotDiscovered)
					{
						visitor.TreeEdge(edge, graph);
						visitor.DiscoverVertex(neighbour, graph);
					}
					else if (neighbourState == State.Discovered)
					{
						visitor.BackEdge(edge, graph);
						visitor.FinishEdge(edge, graph);
					}
					else
					{
						throw new ArgumentException("There must be only tree and back edges in undirected graphs");
					}
				}
			}
		}

		private enum State
		{
			NotDiscovered, Discovered, Closed
		}

		private class VertexInfo<T>
		{
			public T Vertex { get; }

			public Edge<T> IncomingEdge { get; }

			public VertexInfo(T vertex, Edge<T> incomingEdge)
			{
				Vertex = vertex;
				IncomingEdge = incomingEdge;
			}
		}
	}
}