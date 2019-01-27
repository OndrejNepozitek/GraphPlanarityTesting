namespace GraphPlanarityTesting.Tests.PlanarityTesting.BoyerMyrvold
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using GraphPlanarityTesting.Graphs.DataStructures;
	using GraphPlanarityTesting.PlanarityTesting.BoyerMyrvold;
	using GraphPlanarityTesting.PlanarityTesting.PlanarFaceTraversal;
	using NUnit.Framework;

	/// <summary>
	/// Compares results from the algorithm with results from the Boost implementation.
	/// </summary>
	[TestFixture]
	public class BoostComparisonTests
	{
		[Test]
		public void _5000LargeSparseGraphs()
		{
			CompareWithBoost("5000_large_sparse_graphs.txt");
		}

		[Test]
		public void _10000SmallSparseGraphs()
		{
			CompareWithBoost("10000_small_sparse_graphs.txt");
		}

		[Test]
		public void _5000SmallDenseGraphs()
		{
			CompareWithBoost("5000_small_dense_graphs.txt");
		}

		public void CompareWithBoost(string filename)
		{
			var path = TestContext.CurrentContext.TestDirectory + "\\Resources\\" + filename;
			var boyerMyrvold = new BoyerMyrvold<int>();

			using (var reader = File.OpenText(path))
			{
				while (!reader.EndOfStream)
				{
					var testData = LoadData(reader);
					var graph = ConstructGraph(testData);

					if (boyerMyrvold.IsPlanar(graph, out var embedding))
					{
						Assert.That(testData.IsPlanar, Is.EqualTo(true));

						foreach (var vertex in graph.Vertices)
						{
							var vertexEmbedding = embedding.GetEdgesAroundVertex(vertex);
							Assert.That(vertexEmbedding, Is.EquivalentTo(testData.Embedding[vertex]));
						}

						var planarFaceTraversal = new PlanarFaceTraversal();
						var planarFaceVisitor = new GetPlanarFacesVisitor<int>();
						planarFaceTraversal.Traverse(graph, embedding, planarFaceVisitor);

						Assert.That(planarFaceVisitor.Faces.Count, Is.EqualTo(testData.Faces.Count));

						for (var i = 0; i < planarFaceVisitor.Faces.Count; i++)
						{
							var face = planarFaceVisitor.Faces[i];
							Assert.That(face, Is.EquivalentTo(testData.Faces[i]));
						}
					}
					else
					{
						Assert.That(testData.IsPlanar, Is.EqualTo(false));
					}
				}
			}
		}

		private TestData LoadData(TextReader reader)
		{
			var testData = new TestData();

			reader.ReadLine(); // INPUT:
			testData.VerticesCount = int.Parse(reader.ReadLine());
			var edgesLine = reader.ReadLine();

			// Load edges
			testData.Edges = new List<IEdge<int>>();
			foreach (var edgeString in edgesLine.Split(';', StringSplitOptions.RemoveEmptyEntries))
			{
				var vertices = edgeString.Split(',');
				var source = int.Parse(vertices[0]);
				var target = int.Parse(vertices[1]);

				testData.Edges.Add(new Edge<int>(source, target));
			}

			reader.ReadLine(); // OUTPUT:

			// Load embedding
			testData.Embedding = new List<List<IEdge<int>>>();
			while (true)
			{
				var line = reader.ReadLine();

				if (line == "Not planar")
				{
					testData.IsPlanar = false;
					reader.ReadLine();
					break;
				}

				if (line == "Faces:")
				{
					break;
				}

				if (line == "Embedding:")
				{
					continue;
				}

				var edges = new List<IEdge<int>>();
				foreach (var edgeString in line.Split(';', StringSplitOptions.RemoveEmptyEntries))
				{
					var vertices = edgeString.Split(',');
					var source = int.Parse(vertices[0]);
					var target = int.Parse(vertices[1]);

					edges.Add(new Edge<int>(source, target));
				}
				testData.Embedding.Add(edges);
			}

			// Load faces
			if (testData.IsPlanar)
			{
				testData.Faces = new List<List<int>>();

				while (true)
				{
					var line = reader.ReadLine();

					if (line == "")
					{
						break;
					}

					var face = new List<int>();
					foreach (var vertex in line.Split(',', StringSplitOptions.RemoveEmptyEntries))
					{
						face.Add(int.Parse(vertex));
					}
					testData.Faces.Add(face);
				}
			}

			return testData;
		}

		private IGraph<int> ConstructGraph(TestData data)
		{
			var graph = new UndirectedAdjacencyListGraph<int>();

			for (int i = 0; i < data.VerticesCount; i++)
			{
				graph.AddVertex(i);
			}

			foreach (var edge in data.Edges)
			{
				graph.AddEdge(edge.Source, edge.Target);
			}

			return graph;
		}

		private class TestData
		{
			public int VerticesCount { get; set; }

			public List<IEdge<int>> Edges { get; set; }

			public bool IsPlanar { get; set; } = true;

			public List<List<IEdge<int>>> Embedding { get; set; }

			public List<List<int>> Faces { get; set; }

		}
	}
}