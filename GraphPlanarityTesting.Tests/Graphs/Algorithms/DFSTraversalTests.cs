namespace GraphPlanarityTesting.Tests.Graphs.Algorithms
{
	using System.Collections.Generic;
	using GraphPlanarityTesting.Graphs.Algorithms;
	using GraphPlanarityTesting.Graphs.DataStructures;
	using NUnit.Framework;

	[TestFixture]
	public class DFSTraversalTests
	{
		[TestCaseSource(nameof(GetTestGraphs))]
		public void AllVerticesDiscovered(IGraph<int> graph)
		{
			var visitor = new BasicVisitor<int>();
			var dfsTraversal = new DFSTraversal();

			dfsTraversal.TraverseRecursive(graph, 0, visitor);

			Assert.That(visitor.DiscoveredVertices, Is.EquivalentTo(graph.Vertices));
		}

		public static IEnumerable<IGraph<int>> GetTestGraphs()
		{
			for (int i = 1; i < 10; i++)
			{
				yield return GetCompleteGraph(i);
			}
		}

		public static IGraph<int> GetCompleteGraph(int verticesCount)
		{
			var graph = new UndirectedAdjacencyListGraph<int>();

			for (int i = 0; i < verticesCount; i++)
			{
				graph.AddVertex(i);
			}

			for (int i = 0; i < verticesCount - 1; i++)
			{
				for (int j = i + 1; j < verticesCount; j++)
				{
					graph.AddEdge(i, j);
				}
			}

			return graph;
		}

		private class BasicVisitor<T> : BaseDFSTraversalVisitor<T>
		{
			public List<T> DiscoveredVertices { get; } = new List<T>();

			public override void DiscoverVertex(T vertex, IGraph<T> graph)
			{
				DiscoveredVertices.Add(vertex);
			}
		}
	}
}