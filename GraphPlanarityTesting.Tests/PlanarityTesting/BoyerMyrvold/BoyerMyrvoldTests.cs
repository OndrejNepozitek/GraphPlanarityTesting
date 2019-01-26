namespace GraphPlanarityTesting.Tests.PlanarityTesting.BoyerMyrvold
{
	using System.Collections.Generic;
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

		[Test]
		public void Test3()
		{
			var boyerMyrvold = new BoyerMyrvold();
			var graph = new UndirectedAdjacencyListGraph<int>();

			for (int i = 0; i < 6; i++)
			{
				graph.AddVertex(i);
			}

			graph.AddEdge(0, 3);
			graph.AddEdge(0, 4);
			graph.AddEdge(1, 2);
			graph.AddEdge(1, 3);
			graph.AddEdge(1, 4);
			graph.AddEdge(1, 5);
			graph.AddEdge(2, 3);
			graph.AddEdge(2, 4);
			graph.AddEdge(2, 5);
			graph.AddEdge(3, 5);

			boyerMyrvold.IsPlanar(graph, out var embedding);
		}

		[Test]
		public void Test4()
		{
			var boyerMyrvold = new BoyerMyrvold();
			var graph = new UndirectedAdjacencyListGraph<int>();

			for (int i = 0; i < 5; i++)
			{
				graph.AddVertex(i);
			}

			graph.AddEdge(0, 1);
			graph.AddEdge(0, 2);
			graph.AddEdge(0, 3);
			graph.AddEdge(1, 2);
			graph.AddEdge(1, 3);
			graph.AddEdge(2, 3);

			boyerMyrvold.IsPlanar(graph, out var embedding);
		}

		[Test]
		public void Test5()
		{
			var boyerMyrvold = new BoyerMyrvold();
			var graph = new UndirectedAdjacencyListGraph<int>();
			var verticesCount = 10;

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

			boyerMyrvold.IsPlanar(graph, out var embedding);
		}

		[Test]
		public void Test2()
		{
			var verticesCount = 7;
			var graphs = new List<List<IEdge<int>>>();
			graphs.Add(new List<IEdge<int>>());

			for (int i = 0; i < verticesCount - 1; i++)
			{
				for (int j = i + 1; j < verticesCount; j++)
				{
					var newGraphs = new List<List<IEdge<int>>>();

					foreach (var graph in graphs)
					{
						var newGraph = new List<IEdge<int>>(graph) {new Edge<int>(i, j)};
						newGraphs.Add(newGraph);
					}

					graphs.AddRange(newGraphs);
				}
			}

			var boyerMyrvold = new BoyerMyrvold();
			foreach (var edges in graphs)
			{
				var graph = new UndirectedAdjacencyListGraph<int>();

				for (int i = 0; i < verticesCount; i++)
				{
					graph.AddVertex(i);
				}

				foreach (var edge in edges)
				{
					graph.AddEdge(edge.Source, edge.Target);
				}

				boyerMyrvold.IsPlanar(graph, out var embedding);
			}
		}
	}
}