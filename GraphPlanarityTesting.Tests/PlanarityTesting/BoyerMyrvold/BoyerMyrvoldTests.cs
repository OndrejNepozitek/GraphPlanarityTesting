namespace GraphPlanarityTesting.Tests.PlanarityTesting.BoyerMyrvold
{
	using System;
	using System.Collections.Generic;
	using GraphPlanarityTesting.Graphs.DataStructures;
	using GraphPlanarityTesting.PlanarityTesting.BoyerMyrvold;
	using GraphPlanarityTesting.PlanarityTesting.PlanarFaceTraversal;
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

		[Test]
		public void Test7()
		{
			var boyerMyrvold = new BoyerMyrvold();
			var graph = GetOneCycle(4);
			boyerMyrvold.IsPlanar(graph, out var embedding);

			var planarFaceTraversal = new PlanarFaceTraversal();
			var planarFaceVisitor = new GetPlanarFacesVisitor<int>();
			planarFaceTraversal.Traverse(graph, embedding, planarFaceVisitor);
		}

		/// <summary>
		/// Checks that graphs consisting of one big cycle are planar.
		/// </summary>
		[Test]
		public void IsPlanar_OneCycleGraphs()
		{
			var boyerMyrvold = new BoyerMyrvold();
			const int minVertices = 3;
			const int maxVertices = 1000;

			for (int i = minVertices; i <= maxVertices; i++)
			{
				var graph = GetOneCycle(i);

				Assert.That(boyerMyrvold.IsPlanar(graph, out var embedding), Is.EqualTo(true));
				Assert.That(GetFaces(graph, embedding).Count, Is.EqualTo(2));
			}
		}

		/// <summary>
		/// Creates random graphs with 3 * verticesCount edges.
		/// All such graphs are non-planar.
		/// </summary>
		[Test]
		public void IsPlanar_RandomNonPlanarGraphs()
		{
			var boyerMyrvold = new BoyerMyrvold();
			var random = new Random();
			const int graphsCount = 1000;
			const int minVertices = 5;
			const int maxVertices = 1000;

			for (int i = 0; i < graphsCount; i++)
			{
				var verticesCount = random.Next(minVertices, maxVertices + 1);
				var edgesCount = Math.Min(verticesCount * (verticesCount - 1), 3 * verticesCount);
				var graph = GetRandomGraph(verticesCount, edgesCount, random);

				Assert.That(boyerMyrvold.IsPlanar(graph, out var _), Is.EqualTo(false));
			}
		}

		/// <summary>
		/// Get a random graph with a given number of vertices and edges.
		/// </summary>
		/// <remarks>
		/// We try to add random edges to the graph. If the number of edges is close to the
		/// total number of possible edges, the algorithm stops adding them randomly and
		/// simply fills remaining edges deterministically.
		/// </remarks>
		/// <param name="verticesCount"></param>
		/// <param name="edgesCount"></param>
		/// <param name="random"></param>
		/// <param name="allowSelfLoops"></param>
		/// <returns></returns>
		private IGraph<int> GetRandomGraph(int verticesCount, int edgesCount, Random random, bool allowSelfLoops = false)
		{
			if (edgesCount > verticesCount * (verticesCount - 1))
				throw new ArgumentException("It is not possible to create an undirected graph with that many edges.");

			var graph = new UndirectedAdjacencyListGraph<int>();

			for (int i = 0; i < verticesCount; i++)
			{
				graph.AddVertex(i);
			}

			var edges = new HashSet<IEdge<int>>();
			var iterationsCount = 0;

			// Add random edges until we have enough
			while (edges.Count < edgesCount)
			{
				iterationsCount++;

				// Break if it takes too long
				if (iterationsCount > 10 * edgesCount)
				{
					break;
				}

				var source = random.Next(verticesCount);
				var target = random.Next(verticesCount);

				if (source == target && !allowSelfLoops)
					continue;

				var edge1 = new Edge<int>(source, target);
				var edge2 = new Edge<int>(target, source);

				if (edges.Contains(edge1) || edges.Contains(edge2))
					continue;

				if (random.Next(2) == 0)
				{
					graph.AddEdge(source, target);
				}
				else
				{
					graph.AddEdge(target, source);
				}

				edges.Add(edge1);
			}

			// If there is not enough edges, fill them up deterministically
			if (edges.Count < edgesCount)
			{
				for (int length = 1; length < verticesCount; length++)
				{
					for (int i = 0; i < verticesCount - length; i++)
					{
						var j = i + length;

						if (edges.Count == edgesCount)
							break;

						var edge1 = new Edge<int>(i, j);
						var edge2 = new Edge<int>(j, i);

						if (edges.Contains(edge1) || edges.Contains(edge2))
							continue;

						graph.AddEdge(i, j);
					}
				}
			}

			return graph;
		}

		/// <summary>
		/// Creates a cycle graph on a given number of vertices.
		/// </summary>
		/// <param name="verticesCount"></param>
		/// <returns></returns>
		private IGraph<int> GetOneCycle(int verticesCount)
		{
			var graph = new UndirectedAdjacencyListGraph<int>();

			for (int i = 0; i < verticesCount; i++)
			{
				graph.AddVertex(i);
			}

			for (int i = 0; i < verticesCount - 1; i++)
			{
				graph.AddEdge(i, i + 1);
			}

			graph.AddEdge(verticesCount - 1, 0);

			return graph;
		}

		private List<List<int>> GetFaces(IGraph<int> graph, Dictionary<int, List<IEdge<int>>> embedding)
		{
			var planarFaceTraversal = new PlanarFaceTraversal();
			var planarFaceVisitor = new GetPlanarFacesVisitor<int>();
			planarFaceTraversal.Traverse(graph, embedding, planarFaceVisitor);

			return planarFaceVisitor.Faces;
		}
	}
}