namespace PlanarGraph.Graphs.DataStructures
{
	using System.Collections.Generic;

	/// <summary>
	/// Interface describing a generic graph.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IGraph<T>
	{
		/// <summary>
		/// Checks if the graph is directed.
		/// </summary>
		bool IsDirected { get; }

		/// <summary>
		/// Gets all vertices of the graph.
		/// </summary>
		IEnumerable<T> Vertices { get; }

		/// <summary>
		/// Gets all edges of the graph.
		/// </summary>
		IEnumerable<IEdge<T>> Edges { get; }

		/// <summary>
		/// Gets the total number of vertices.
		/// </summary>
		int VerticesCount { get; }

		/// <summary>
		/// Adds a vertex.
		/// </summary>
		/// <param name="vertex"></param>
		T AddVertex(T vertex);

		/// <summary>
		/// Adds an edge.
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		/// <param name="dir"></param>
		IEdge<T> AddEdge(T from, T to, EdgeDirection dir = EdgeDirection.Bi);

		/// <summary>
		/// Gets all neighbours of a given vertex.
		/// </summary>
		/// <param name="vertex"></param>
		/// <returns></returns>
		IEnumerable<T> GetNeighbours(T vertex);

		/// <summary>
		/// Gets all edges to which this vertex connects
		///
		/// TODO: Inefficient implementation due to how we store edges
		/// </summary>
		/// <param name="vertex"></param>
		/// <returns></returns>
		IEnumerable<IEdge<T>> GetNeighbouringEdges(T vertex);
	}
}