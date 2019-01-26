namespace GraphPlanarityTesting.PlanarityTesting.PlanarFaceTraversal
{
	using System.Collections.Generic;
	using Graphs.DataStructures;

	public class PlanarFaceTraversal
	{
		public void Traverse<T>(IGraph<T> graph, Dictionary<T, List<IEdge<T>>> embedding, IPlanarFaceTraversalVisitor<T> visitor)
		{
			var nextEdge = new Dictionary<IEdge<T>, Dictionary<T, IEdge<T>>>();

			foreach (var edge in graph.Edges)
			{
				nextEdge[edge] = new Dictionary<T, IEdge<T>>();
			}

			visitor.BeginTraversal();

			foreach (var vertex in graph.Vertices)
			{
				for (var i = 0; i < embedding[vertex].Count; i++)
				{
					var edge = embedding[vertex][i];
					var followingEdge = i != embedding[vertex].Count - 1 ? embedding[vertex][i + 1] : embedding[vertex][0];

					nextEdge[edge][vertex] = followingEdge;
				}
			}
		}
	}
}