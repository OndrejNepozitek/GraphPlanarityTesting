namespace GraphPlanarityTesting.PlanarityTesting.BoyerMyrvold
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Graphs.Algorithms;
	using Graphs.DataStructures;

	public class BoyerMyrvold
	{
		public static int NullVertex = int.MinValue;

		private Dictionary<int, int> dfsNumberMap;

		private Dictionary<int, int> parentMap;

		// Zero or more descendant DFS edges and then one back edge.
		// We are interested in a vertex with the lowest DFS number
		private Dictionary<int, int> lowPointMap;

		// Back edge ancestor with the lowest DFS number
		// Apparently the DFS parent counts as the least ancestor
		private Dictionary<int, int> leastAncestorMap;

		private Dictionary<int, IEdge<int>> dfsEdgeMap;

		private int[] verticesByDFSNumber;

		private int[] verticesByLowPoint;

		private Dictionary<int, FaceHandle<int>> faceHandlesMap;

		private Dictionary<int, FaceHandle<int>> dfsChildHandlesMap;

		private Dictionary<int, int> canonicalDFSChildMap;

		private Dictionary<int, List<FaceHandle<int>>> pertinentRootsMap;

		private Dictionary<int, LinkedList<int>> separatedDFSChildListMap;

		private Dictionary<int, LinkedListNode<int>> separatedNodeInParentList;

		private List<IEdge<int>> selfLoops;

		private Dictionary<int, List<IEdge<int>>> backedges;

		private Dictionary<int, int> backedgeFlag;

		private Dictionary<int, int> visited;

		private IGraph<int> graph;

		public bool IsPlanar(IGraph<int> graph)
		{
			dfsNumberMap = new Dictionary<int, int>();
			parentMap = new Dictionary<int, int>();
			lowPointMap = new Dictionary<int, int>();
			leastAncestorMap = new Dictionary<int, int>();
			dfsEdgeMap = new Dictionary<int, IEdge<int>>();
			faceHandlesMap = new Dictionary<int, FaceHandle<int>>();
			dfsChildHandlesMap = new Dictionary<int, FaceHandle<int>>();
			pertinentRootsMap = new Dictionary<int, List<FaceHandle<int>>>();
			separatedDFSChildListMap = new Dictionary<int, LinkedList<int>>();
			separatedNodeInParentList = new Dictionary<int, LinkedListNode<int>>();
			canonicalDFSChildMap = new Dictionary<int, int>();
			selfLoops = new List<IEdge<int>>();
			backedges = new Dictionary<int, List<IEdge<int>>>();
			backedgeFlag = new Dictionary<int, int>();
			visited = new Dictionary<int, int>();
			this.graph = graph;

			var visitor = new DFSTraversalVisitor<int>(dfsNumberMap, parentMap, lowPointMap, leastAncestorMap, dfsEdgeMap);
			var dfsTraversal = new DFSTraversal();
			dfsTraversal.TraverseRecursive(graph, visitor);

			// Init backedges
			foreach (var vertex in graph.Vertices)
			{
				backedges.Add(vertex, new List<IEdge<int>>());
				visited[vertex] = Int32.MaxValue; // TODO: not ideal
			}

			// Sort vertices by dfs number ASC
			verticesByDFSNumber = BucketSort.Sort(dfsNumberMap, graph.VerticesCount);

			// Sort vertices by low point ASC
			verticesByLowPoint = BucketSort.Sort(lowPointMap, graph.VerticesCount);

			foreach (var vertex in graph.Vertices)
			{
				var dfsParent = parentMap[vertex];

				if (!vertex.Equals(dfsParent))
				{
					var parentEdge = dfsEdgeMap[vertex];
					// add_to_embedded_edges(parent_edge, StoreOldHandlesPolicy());
					faceHandlesMap[vertex] = new FaceHandle<int>(vertex, parentEdge);
					dfsChildHandlesMap[vertex] = new FaceHandle<int>(dfsParent, parentEdge);
				}
				else
				{
					faceHandlesMap[vertex] = new FaceHandle<int>(vertex);
					dfsChildHandlesMap[vertex] = new FaceHandle<int>(dfsParent);
				}

				canonicalDFSChildMap[vertex] = vertex;
				pertinentRootsMap[vertex] = new List<FaceHandle<int>>();
				separatedDFSChildListMap[vertex] = new LinkedList<int>();
			}


			foreach (var vertex in verticesByLowPoint)
			{
				var dfsParent = parentMap[vertex];

				if (!dfsParent.Equals(vertex))
				{
					var node = separatedDFSChildListMap[dfsParent].AddLast(vertex);
					separatedNodeInParentList[vertex] = node;
				}
			}
			
			// TODO: reserve stack

			foreach (var vertex in verticesByDFSNumber.Reverse())
			{
				Walkup(vertex);
			}

			return true;
		}

		private void Walkup(int vertex)
		{
			Console.WriteLine($"Walkup {vertex}");

			foreach (var neighbour in graph.GetNeighbours(vertex))
			{
				Walkup(vertex, new Edge<int>(vertex, neighbour));
			}

			Console.WriteLine($"Walkup end");
			Console.WriteLine();
		}

		private void Walkup(int vertex, IEdge<int> edge)
		{
			Console.WriteLine($"Edge {edge}");

			var source = edge.Source;
			var target = edge.Target;

			if (source.Equals(target))
			{
				selfLoops.Add(edge);
				return;
			}

			var w = vertex.Equals(source) ? target : vertex;

			if (dfsNumberMap[w] < dfsNumberMap[vertex] || edge.Equals(dfsEdgeMap[w]))
				return;

			Console.WriteLine("Edge not embedded and back edge");

			backedges[w].Add(edge);
			var timestamp = dfsNumberMap[vertex];
			backedgeFlag[w] = timestamp;


			foreach (var faceVertex in IterateFirstSide(w))
			{
				Console.WriteLine($"Face vertex {faceVertex}");
			}

			var leadVertex = w;

			while (true)
			{
				var foundRoot = true;

				// TODO: this is slow - should be walked in parallel
				foreach (var faceVertex in IterateFirstSide(leadVertex, false))
				{
					if (visited[faceVertex] == timestamp)
					{
						foundRoot = false;
						break;
					}

					leadVertex = faceVertex;
					visited[leadVertex] = timestamp;
					Console.WriteLine($"Lead vertex {leadVertex}");
				}

				if (foundRoot)
				{
					var dfsChild = canonicalDFSChildMap[leadVertex];
					var parent = parentMap[dfsChild];

					Console.WriteLine($"Found DFS child {dfsChild}, found parent {parent}");

					if (lowPointMap[dfsChild] < dfsNumberMap[vertex] ||
					    leastAncestorMap[dfsChild] < dfsNumberMap[vertex]
					)
					{
						pertinentRootsMap[parent].Add(dfsChildHandlesMap[dfsChild]);
					}
					else
					{
						pertinentRootsMap[parent].Insert(0, dfsChildHandlesMap[dfsChild]);
					}

					if (parent != vertex && visited[parent] != timestamp)
					{
						leadVertex = parent;
					}
					else
						break;
				}
				else
				{
					break;
				}
			}
		}

		public IEnumerable<int> IterateFirstSide(int vertex, bool visitLead = true)
		{
			var face = faceHandlesMap[vertex];

			return IterateFace(face, x => x.FirstVertex, visitLead);
		}

		public IEnumerable<int> IterateSecondSide(int vertex, bool visitLead = true)
		{
			var face = faceHandlesMap[vertex];

			return IterateFace(face, x => x.SecondVertex, visitLead);
		}

		public IEnumerable<int> IterateFace(FaceHandle<int> face, Func<FaceHandle<int>, int> leadSelector, bool visitLead = true)
		{
			var follow = face.Anchor;
			var lead = leadSelector(face);

			while (true)
			{
				// TODO: may be wrong
				if (visitLead)
				{
					yield return lead;
				}
				else
				{
					yield return follow;
				}
				
				var first = face.FirstVertex;
				var second = face.SecondVertex;

				if (first.Equals(follow))
				{
					follow = lead;
					lead = second;
				}
				else if (second.Equals(follow))
				{
					follow = lead;
					lead = first;
				}
				else
				{
					yield break;
				}

				face = faceHandlesMap[lead];
			}
		}
	}
}