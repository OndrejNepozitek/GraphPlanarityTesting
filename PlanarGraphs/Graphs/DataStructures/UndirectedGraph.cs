namespace PlanarGraph.Graphs.DataStructures
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// A graph where edges are represented as an adjacency lists.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class UndirectedGraph<T> : IGraph<T>
	{
		/// <inheritdoc />
		public bool IsDirected { get; } = false;

		/// <inheritdoc />
		public IEnumerable<T> Vertices => adjacencyLists.Keys;

		/// <inheritdoc />
		public IEnumerable<IEdge<T>> Edges => edges;

		/// <inheritdoc />
		public int VerticesCount => adjacencyLists.Count;

		private readonly Dictionary<T, List<T>> adjacencyLists = new Dictionary<T, List<T>>();

		private readonly List<IEdge<T>> edges = new List<IEdge<T>>();

		/// <inheritdoc />
		public T AddVertex(T vertex)
		{
			if (adjacencyLists.ContainsKey(vertex))
				throw new ArgumentException("Vertex already exists");

			adjacencyLists[vertex] = new List<T>();

			return vertex;
		}

		/// <inheritdoc />
		public IEdge<T> AddEdge(T from, T to, EdgeDirection dir = EdgeDirection.Bi)
		{
			if (!adjacencyLists.TryGetValue(from, out var fromList) || !adjacencyLists.TryGetValue(to, out var toList))
				throw new ArgumentException("One of the vertices does not exist");

			if (fromList.Contains(to))
				throw new ArgumentException("The edge was already added");

			Edge<T> edge = new Edge<T>(from, to, dir);
			edges.Add(edge);

			fromList.Add(to);
			toList.Add(from);

			return edge;
		}

		/// <inheritdoc />
		public IEnumerable<T> GetNeighbours(T vertex)
		{
			if (!adjacencyLists.TryGetValue(vertex, out var neighbours))
				throw new ArgumentException("The vertex does not exist");

			return neighbours;
		}

		public IEnumerable<IEdge<T>> GetNeighbouringEdges(T vertex)
		{
			foreach (IEdge<T> edge in edges) {
				// TODO: Figure out a value compare?
				//
				if (edge.Source.Equals(vertex) || edge.Target.Equals(vertex)) 
					yield return edge;
			}
		}
	}
}