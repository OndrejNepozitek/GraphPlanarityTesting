namespace GraphPlanarityTesting.PlanarityTesting.BoyerMyrvold
{
	using System;
	using System.Collections.Generic;
	using Graphs.DataStructures;

	public class PlanarEmbedding<T>
	{
		private readonly Dictionary<T, List<IEdge<T>>> embedding;

		public PlanarEmbedding(Dictionary<T, List<IEdge<T>>> embedding)
		{
			this.embedding = embedding;
		}

		public List<IEdge<T>> GetEdgesAroundVertex(T vertex)
		{
			if (embedding.TryGetValue(vertex, out var edges))
			{
				return edges;
			}

			throw new ArgumentException("Given vertex was not found");
		}
	}
}