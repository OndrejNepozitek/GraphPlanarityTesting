namespace GraphPlanarityTesting.Tests.PlanarityTesting.BoyerMyrvold
{
	using GraphPlanarityTesting.Graphs.DataStructures;
	using GraphPlanarityTesting.PlanarityTesting.BoyerMyrvold;
	using NUnit.Framework;

	[TestFixture]
	public class BoyerMyrvoldTests
	{
		[Test]
		public void Test()
		{
			var boyerMyrvold = new BoyerMyrvold();
			var graph = new UndirectedAdjacencyListGraph<int>();

			for (int i = 0; i < 5; i++)
			{
				graph.AddVertex(i);
			}

			//for (int i = 0; i < 4; ++i)
			//{
			//	for (int j = i + 1; j < 4; ++j)
			//	{
			//		graph.AddEdge(i, j);
			//	}
			//}

			graph.AddEdge(0, 1);
			//graph.AddEdge(0, 2);
			graph.AddEdge(0, 3);
			//graph.AddEdge(0, 4);
			graph.AddEdge(1, 2);
			graph.AddEdge(1, 3);
			graph.AddEdge(1, 4);
			graph.AddEdge(2, 3);
			graph.AddEdge(2, 4);
			// graph.AddEdge(3, 4);

			//graph.AddEdge(0, 5);
			//graph.AddEdge(5, 6);
			//graph.AddEdge(6, 7);
			//graph.AddEdge(7, 0);

			boyerMyrvold.IsPlanar(graph, out var embedding);
		}
	}
}