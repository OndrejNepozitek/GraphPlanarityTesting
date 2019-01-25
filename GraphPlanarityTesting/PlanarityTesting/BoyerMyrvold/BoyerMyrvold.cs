namespace GraphPlanarityTesting.PlanarityTesting.BoyerMyrvold
{
	using System;
	using System.Collections.Generic;
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

			var visitor = new DFSTraversalVisitor<int>(dfsNumberMap, parentMap, lowPointMap, leastAncestorMap, dfsEdgeMap);
			var dfsTraversal = new DFSTraversal();
			dfsTraversal.TraverseRecursive(graph, visitor);

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


			throw new NotImplementedException();
		}
	}
}