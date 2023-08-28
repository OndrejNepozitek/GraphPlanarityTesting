namespace PlanarGraph.Graphs.DataStructures
{
	public enum EdgeDirection {
		Bi,
		Uni
	} 

	/// <summary>
	/// Interface describing an edge of a graph.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IEdge<T>
	{
		/// <summary>
		/// First vertex of the edge.
		/// </summary>
		T Source { get; }

		/// <summary>
		/// Second vertex of the edge.
		/// </summary>
		T Target { get; }

		/// <summary>
		/// Edge weighting
		/// </summary>
		int Weight { get; set; }

		/// <summary>
		/// Temporary workaround to support direction without maintaining multiple edges per direction
		/// since the initial fork of this code was not designed to work with directed graphs
		///
		/// TODO: Support directed graphs.
		/// </summary>
   		EdgeDirection Direction { get; set; }
	}
}