namespace GraphPlanarityTesting.PlanarityTesting.BoyerMyrvold
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Graphs.Algorithms;
	using Graphs.DataStructures;

	public class BoyerMyrvold<T>
	{
		public static int NullVertex = -1;

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

		private Dictionary<int, LinkedList<FaceHandle<int>>> pertinentRootsMap;

		private Dictionary<int, LinkedList<int>> separatedDFSChildListMap;

		private Dictionary<int, LinkedListNode<int>> separatedNodeInParentList;

		private List<IEdge<int>> selfLoops;

		private Dictionary<int, List<IEdge<int>>> backedges;

		private Dictionary<int, int> backedgeFlag;

		private Dictionary<int, int> visited;

		private Stack<MergeInfo> mergeStack;

		private Dictionary<int, bool> flipped;

		private IGraph<int> graphOld;

		// ---- NEW ----

		private IGraph<T> graph;

		private IGraph<Vertex<T>> transformedGraph;

		private Dictionary<T, Vertex<T>> originalVertexMapping;

		private Vertex<T>[] verticesByDFSNumberNew;

		private Vertex<T>[] verticesByLowPointNew;

		private List<IEdge<Vertex<T>>> selfLoopsNew;

		private Stack<MergeInfoNew> mergeStackNew;


		public bool IsPlanar(IGraph<T> graph, out Dictionary<T, List<IEdge<T>>> embedding)
		{
			this.graph = graph;
			transformedGraph = new UndirectedAdjacencyListGraph<Vertex<T>>();

			originalVertexMapping = new Dictionary<T, Vertex<T>>();

			foreach (var vertex in graph.Vertices)
			{
				var newVertex = new Vertex<T>(vertex);
				originalVertexMapping.Add(vertex, newVertex);
				transformedGraph.AddVertex(newVertex);
			}

			foreach (var edge in graph.Edges)
			{
				var source = originalVertexMapping[edge.Source];
				var target = originalVertexMapping[edge.Target];
				transformedGraph.AddEdge(source, target);
			}

			var isPlanar = IsPlanar(out var embeddingOld);
			embedding = embeddingOld;

			return isPlanar;
		}

		private bool IsPlanar(out Dictionary<T, List<IEdge<T>>> embedding)
		{
			//dfsNumberMap = new Dictionary<int, int>();
			//parentMap = new Dictionary<int, int>();
			//lowPointMap = new Dictionary<int, int>();
			//leastAncestorMap = new Dictionary<int, int>();
			//dfsEdgeMap = new Dictionary<int, IEdge<int>>();
			//faceHandlesMap = new Dictionary<int, FaceHandle<int>>();
			//dfsChildHandlesMap = new Dictionary<int, FaceHandle<int>>();
			//pertinentRootsMap = new Dictionary<int, LinkedList<FaceHandle<int>>>();
			//separatedDFSChildListMap = new Dictionary<int, LinkedList<int>>();
			//separatedNodeInParentList = new Dictionary<int, LinkedListNode<int>>();
			//canonicalDFSChildMap = new Dictionary<int, int>();
			//selfLoops = new List<IEdge<int>>();
			//backedges = new Dictionary<int, List<IEdge<int>>>();
			//backedgeFlag = new Dictionary<int, int>();
			//visited = new Dictionary<int, int>();
			//mergeStack = new Stack<MergeInfo>();
			//flipped = new Dictionary<int, bool>();
			//this.graphOld = graph;

			selfLoopsNew = new List<IEdge<Vertex<T>>>();
			mergeStackNew = new Stack<MergeInfoNew>();

			//var visitor = new DFSTraversalVisitorOld<int>(dfsNumberMap, parentMap, lowPointMap, leastAncestorMap, dfsEdgeMap);
			//var dfsTraversal = new DFSTraversal();
			//dfsTraversal.TraverseRecursive(graph, visitor);

			var visitorNew = new DFSTraversalVisitor<T>();
			var dfsTraversalNew = new DFSTraversal();
			dfsTraversalNew.TraverseRecursive(transformedGraph, visitorNew);

			//// Init backedges
			//foreach (var vertex in graph.Vertices)
			//{
			//	backedges.Add(vertex, new List<IEdge<int>>());
			//	visited[vertex] = Int32.MaxValue; // TODO: not ideal
			//	backedgeFlag[vertex] = graph.VerticesCount + 1;
			//	flipped[vertex] = false;
			//}

			// Init backedges
			foreach (var vertex in transformedGraph.Vertices)
			{
				vertex.BackEdges = new List<IEdge<Vertex<T>>>();
				vertex.Visited = int.MaxValue;
				vertex.BackedgeFlag = transformedGraph.VerticesCount + 1;
				vertex.Flipped = false;
			}

			//// Sort vertices by dfs number ASC
			//verticesByDFSNumber = BucketSort.Sort(dfsNumberMap, graph.VerticesCount);
			verticesByDFSNumberNew = BucketSort.Sort(transformedGraph.Vertices, x => x.DFSNumber, transformedGraph.VerticesCount);

			// Sort vertices by low point ASC
			//verticesByLowPoint = BucketSort.Sort(lowPointMap, graph.VerticesCount);
			verticesByLowPointNew = BucketSort.Sort(transformedGraph.Vertices, x => x.LowPoint, transformedGraph.VerticesCount);

			//foreach (var vertex in graph.Vertices)
			//{
			//	var dfsParent = parentMap[vertex];

			//	if (!vertex.Equals(dfsParent))
			//	{
			//		var parentEdge = dfsEdgeMap[vertex];
			//		// add_to_embedded_edges(parent_edge, StoreOldHandlesPolicy());
			//		faceHandlesMap[vertex] = new FaceHandle<int>(vertex, parentEdge);
			//		dfsChildHandlesMap[vertex] = new FaceHandle<int>(dfsParent, parentEdge);
			//	}
			//	else
			//	{
			//		faceHandlesMap[vertex] = new FaceHandle<int>(vertex, NullVertex);
			//		dfsChildHandlesMap[vertex] = new FaceHandle<int>(dfsParent, NullVertex);
			//	}

			//	canonicalDFSChildMap[vertex] = vertex;
			//	pertinentRootsMap[vertex] = new LinkedList<FaceHandle<int>>();
			//	separatedDFSChildListMap[vertex] = new LinkedList<int>();
			//}

			foreach (var vertex in transformedGraph.Vertices)
			{
				var dfsParent = vertex.Parent;

				if (vertex != dfsParent)
				{
					var parentEdge = vertex.DFSEdge;
					vertex.FaceHandle = new FaceHandle<Vertex<T>>(vertex, parentEdge);
					vertex.DFSChildHandle = new FaceHandle<Vertex<T>>(dfsParent, parentEdge);
				}
				else
				{
					vertex.FaceHandle = new FaceHandle<Vertex<T>>(vertex, (Vertex<T>) null); // TODO: change
					vertex.DFSChildHandle = new FaceHandle<Vertex<T>>(dfsParent, (Vertex<T>) null);
				}

				vertex.CanonicalDFSChild = vertex;
				vertex.PertinentRoots = new LinkedList<FaceHandle<Vertex<T>>>();
				vertex.SeparatedDFSChildList = new LinkedList<Vertex<T>>();
			}


			//foreach (var vertex in verticesByLowPoint)
			//{
			//	var dfsParent = parentMap[vertex];

			//	if (!dfsParent.Equals(vertex))
			//	{
			//		var node = separatedDFSChildListMap[dfsParent].AddLast(vertex);
			//		separatedNodeInParentList[vertex] = node;
			//	}
			//}

			foreach (var vertex in verticesByLowPointNew)
			{
				var dfsParent = vertex.Parent;

				if (vertex != dfsParent)
				{
					var node = dfsParent.SeparatedDFSChildList.AddLast(vertex);
					vertex.SeparatedNodeInParentList = node;
				}
			}


			// TODO: reserve stack

			foreach (var vertex in verticesByDFSNumberNew.Reverse())
			{
				Walkup(vertex);

				if (!Walkdown(vertex))
				{
					embedding = null;
					return false;
				}

				// Dump();
			}

			CleanupNew();
			Dump();

			embedding = new Dictionary<T, List<IEdge<T>>>();
			foreach (var vertex in transformedGraph.Vertices)
			{
				var faceHandle = vertex.FaceHandle;
				embedding[vertex.Value] = faceHandle.GetEdges()?.Select(x => (IEdge < T >)  new Edge<T>(x.Source.Value, x.Target.Value)).ToList(); // TODO: change
			}

			return true;
		}

		//private void Walkup(int vertex)
		//{
		//	// Console.WriteLine($"Walkup {vertex}");

		//	foreach (var neighbour in graphOld.GetNeighbours(vertex))
		//	{
		//		Walkup(vertex, new Edge<int>(vertex, neighbour));
		//	}

		//	// Console.WriteLine($"Walkup end");
		//	// Console.WriteLine();
		//}

		private void Walkup(Vertex<T> vertex)
		{
			// Console.WriteLine($"Walkup {vertex}");

			foreach (var neighbour in transformedGraph.GetNeighbours(vertex))
			{
				Walkup(vertex, new Edge<Vertex<T>>(vertex, neighbour));
			}

			// Console.WriteLine($"Walkup end");
			// Console.WriteLine();
		}

		//private void Walkup(int vertex, IEdge<int> edge)
		//{
		//	// Console.WriteLine($"Edge {edge}");

		//	var source = edge.Source;
		//	var target = edge.Target;

		//	if (source.Equals(target))
		//	{
		//		selfLoops.Add(edge);
		//		return;
		//	}

		//	var w = vertex.Equals(source) ? target : vertex;

		//	if (dfsNumberMap[w] < dfsNumberMap[vertex] || edge.Equals(dfsEdgeMap[w]))
		//		return;

		//	// Console.WriteLine("Edge not embedded and back edge");

		//	backedges[w].Add(edge);
		//	var timestamp = dfsNumberMap[vertex];
		//	backedgeFlag[w] = timestamp;


		//	foreach (var faceVertex in IterateFirstSide(w))
		//	{
		//		// Console.WriteLine($"Face vertex {faceVertex}");
		//	}

		//	var leadVertex = w;

		//	if (vertex == 0)
		//	{
		//		var ss = 1;
		//	}

		//	while (true)
		//	{
		//		var foundRoot = true;

		//		// TODO: this is slow - should be walked in parallel
		//		//foreach (var faceVertex in IterateFirstSide(leadVertex, false))
		//		//{
		//		//	if (visited[faceVertex] == timestamp)
		//		//	{
		//		//		foundRoot = false;
		//		//		break;
		//		//	}

		//		//	leadVertex = faceVertex;
		//		//	visited[leadVertex] = timestamp;
		//		//	// Console.WriteLine($"Lead vertex {leadVertex}");
		//		//}
		//		foreach (var faceVertex in IterateBothSides(leadVertex))
		//		{
		//			if (visited[faceVertex] == timestamp)
		//			{
		//				foundRoot = false;
		//				break;
		//			}

		//			leadVertex = faceVertex;
		//			visited[leadVertex] = timestamp;
		//			// Console.WriteLine($"Lead vertex {leadVertex}");
		//		}

		//		if (foundRoot)
		//		{
		//			var dfsChild = canonicalDFSChildMap[leadVertex];
		//			var parent = parentMap[dfsChild];

		//			// TODO: different than in Boost
		//			// Probably due to the fact that we don't traverse both side of the face in parallel,
		//			// it happened that the same dfsChild was added twice.
		//			// This check ensures
		//			if ((pertinentRootsMap[parent].Last != null && pertinentRootsMap[parent].Last.Value == dfsChildHandlesMap[dfsChild])
		//			    || (pertinentRootsMap[parent].First != null && pertinentRootsMap[parent].First.Value == dfsChildHandlesMap[dfsChild]))
		//			{
		//				throw new InvalidOperationException("Must not happen!");
		//			}

		//			// Console.WriteLine($"Found DFS child {dfsChild}, found parent {parent}");

		//			visited[dfsChildHandlesMap[dfsChild].FirstVertex] = timestamp;
		//			visited[dfsChildHandlesMap[dfsChild].SecondVertex] = timestamp;

		//			if (lowPointMap[dfsChild] < dfsNumberMap[vertex] ||
		//			    leastAncestorMap[dfsChild] < dfsNumberMap[vertex]
		//			)
		//			{
		//				pertinentRootsMap[parent].AddLast(dfsChildHandlesMap[dfsChild]);
		//			}
		//			else
		//			{
		//				pertinentRootsMap[parent].AddFirst(dfsChildHandlesMap[dfsChild]);
		//			}

		//			if (parent != vertex && visited[parent] != timestamp)
		//			{
		//				leadVertex = parent;
		//			}
		//			else
		//				break;
		//		}
		//		else
		//		{
		//			break;
		//		}
		//	}
		//}

		private void Walkup(Vertex<T> vertex, IEdge<Vertex<T>> edge)
		{
			// Console.WriteLine($"Edge {edge}");

			var source = edge.Source;
			var target = edge.Target;

			if (source.Equals(target))
			{
				selfLoopsNew.Add(edge);
				return;
			}

			var w = vertex.Equals(source) ? target : vertex;

			if (w.DFSNumber < vertex.DFSNumber || edge.Equals(w.DFSEdge))
				return;

			// Console.WriteLine("Edge not embedded and back edge");

			w.BackEdges.Add(edge);
			var timestamp = vertex.DFSNumber;
			w.BackedgeFlag = timestamp;

			foreach (var faceVertex in IterateFirstSide(w))
			{
				// Console.WriteLine($"Face vertex {faceVertex}");
			}

			var leadVertex = w;

			while (true)
			{
				var foundRoot = true;

				// TODO: this is slow - should be walked in parallel
				//foreach (var faceVertex in IterateFirstSide(leadVertex, false))
				//{
				//	if (visited[faceVertex] == timestamp)
				//	{
				//		foundRoot = false;
				//		break;
				//	}

				//	leadVertex = faceVertex;
				//	visited[leadVertex] = timestamp;
				//	// Console.WriteLine($"Lead vertex {leadVertex}");
				//}
				foreach (var faceVertex in IterateBothSides(leadVertex))
				{
					if (faceVertex.Visited == timestamp)
					{
						foundRoot = false;
						break;
					}

					leadVertex = faceVertex;
					leadVertex.Visited = timestamp;
					// Console.WriteLine($"Lead vertex {leadVertex}");
				}

				if (foundRoot)
				{
					var dfsChild = leadVertex.CanonicalDFSChild;
					var parent = dfsChild.Parent;

					// TODO: different than in Boost
					// Probably due to the fact that we don't traverse both side of the face in parallel,
					// it happened that the same dfsChild was added twice.
					// This check ensures
					if ((parent.PertinentRoots.Last != null && parent.PertinentRoots.Last.Value == dfsChild.DFSChildHandle)
						|| (parent.PertinentRoots.First != null && parent.PertinentRoots.First.Value == dfsChild.DFSChildHandle))
					{
						throw new InvalidOperationException("Must not happen!");
					}

					// Console.WriteLine($"Found DFS child {dfsChild}, found parent {parent}");

					dfsChild.DFSChildHandle.FirstVertex.Visited = timestamp;
					dfsChild.DFSChildHandle.SecondVertex.Visited = timestamp;

					if (dfsChild.LowPoint < vertex.DFSNumber ||
					    dfsChild.LeastAncestor < vertex.DFSNumber
					)
					{
						parent.PertinentRoots.AddLast(dfsChild.DFSChildHandle);
					}
					else
					{
						parent.PertinentRoots.AddFirst(dfsChild.DFSChildHandle);
					}

					if (parent != vertex && parent.Visited != timestamp)
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

		//private void Walkup(Vertex<T> vertex, IEdge<Vertex<T>> edge)
		//{
		//	// Console.WriteLine($"Edge {edge}");

		//	var source = edge.Source;
		//	var target = edge.Target;

		//	if (source.Equals(target))
		//	{
		//		selfLoopsNew.Add(edge);
		//		return;
		//	}

		//	var w = vertex.Equals(source) ? target : vertex;

		//	if (w.DFSNumber < vertex.DFSNumber || edge.Equals(w.DFSEdge))
		//		return;

		//	// Console.WriteLine("Edge not embedded and back edge");

		//	w.BackEdges.Add(edge);
		//	var timestamp = vertex.DFSNumber;
		//	w.BackedgeFlag = timestamp;


		//	foreach (var faceVertex in IterateFirstSide(w))
		//	{
		//		// Console.WriteLine($"Face vertex {faceVertex}");
		//	}

		//	var leadVertex = w;

		//	while (true)
		//	{
		//		var foundRoot = true;

		//		// TODO: this is slow - should be walked in parallel
		//		//foreach (var faceVertex in IterateFirstSide(leadVertex, false))
		//		//{
		//		//	if (visited[faceVertex] == timestamp)
		//		//	{
		//		//		foundRoot = false;
		//		//		break;
		//		//	}

		//		//	leadVertex = faceVertex;
		//		//	visited[leadVertex] = timestamp;
		//		//	// Console.WriteLine($"Lead vertex {leadVertex}");
		//		//}
		//		foreach (var faceVertex in IterateBothSides(leadVertex))
		//		{
		//			if (faceVertex.Visited == timestamp)
		//			{
		//				foundRoot = false;
		//				break;
		//			}

		//			leadVertex = faceVertex;
		//			leadVertex.Visited = timestamp;
		//			// Console.WriteLine($"Lead vertex {leadVertex}");
		//		}

		//		if (foundRoot)
		//		{
		//			var dfsChild = leadVertex.CanonicalDFSChild;
		//			var parent = dfsChild.Parent;

		//			// TODO: different than in Boost
		//			// Probably due to the fact that we don't traverse both side of the face in parallel,
		//			// it happened that the same dfsChild was added twice.
		//			// This check ensures
		//			//if ((pertinentRootsMap[parent].Last != null && pertinentRootsMap[parent].Last.Value == dfsChildHandlesMap[dfsChild])
		//			//	|| (pertinentRootsMap[parent].First != null && pertinentRootsMap[parent].First.Value == dfsChildHandlesMap[dfsChild]))
		//			//{
		//			//	throw new InvalidOperationException("Must not happen!");
		//			//}

		//			// Console.WriteLine($"Found DFS child {dfsChild}, found parent {parent}");

		//			dfsChild.DFSChildHandle.FirstVertex.Visited = timestamp;
		//			dfsChild.DFSChildHandle.SecondVertex.Visited = timestamp;

		//			if (dfsChild.LowPoint < vertex.DFSNumber ||
		//			    dfsChild.LeastAncestor < vertex.DFSNumber
		//			)
		//			{
		//				parent.PertinentRoots.AddLast(dfsChild.DFSChildHandle);
		//			}
		//			else
		//			{
		//				parent.PertinentRoots.AddFirst(dfsChild.DFSChildHandle);
		//			}

		//			if (parent != vertex && parent.Visited != timestamp)
		//			{
		//				leadVertex = parent;
		//			}
		//			else
		//				break;
		//		}
		//		else
		//		{
		//			break;
		//		}
		//	}
		//}

		//public bool Walkdown(int vertex)
		//{
		//	// Console.WriteLine($"Walkdown {vertex}");

		//	int w;

		//	mergeStack.Clear();

		//	while (pertinentRootsMap[vertex].Count != 0)
		//	{
		//		var rootFaceHandle = pertinentRootsMap[vertex].First.Value;
		//		pertinentRootsMap[vertex].RemoveFirst();
		//		var currentFaceHandle = rootFaceHandle;

		//		// Console.WriteLine($"Pertinent root anchor {rootFaceHandle.Anchor}");

		//		while (true)
		//		{
		//			var firstSideVertex = NullVertex;
		//			var secondSideVertex = NullVertex;
		//			var firstTail = currentFaceHandle.Anchor;
		//			var secondTail = currentFaceHandle.Anchor;
		//			var firstSideVertices = IterateFirstSide(currentFaceHandle);

		//			foreach (var faceVertex in firstSideVertices)
		//			{
		//				// Console.WriteLine($"First side iteration {faceVertex}, pertinent = {Pertinent(faceVertex, vertex)}, externally active = {ExternallyActive(faceVertex, vertex)}");

		//				if (Pertinent(faceVertex, vertex) || ExternallyActive(faceVertex, vertex))
		//				{
		//					// Console.WriteLine($"First side iteration pertinent or externally active");

		//					firstSideVertex = faceVertex;
		//					secondSideVertex = faceVertex;
		//					break;
		//				}

		//				firstTail = faceVertex;
		//			}

		//			if (firstSideVertex == NullVertex || firstSideVertex == currentFaceHandle.Anchor)
		//			{
		//				// Console.WriteLine($"Break");
		//				break;
		//			}

		//			foreach (var faceVertex in IterateSecondSide(currentFaceHandle))
		//			{
		//				// Console.WriteLine($"Second side iteration {faceVertex}, pertinent = {Pertinent(faceVertex, vertex)}, externally active = {ExternallyActive(faceVertex, vertex)}");

		//				if (Pertinent(faceVertex, vertex) || ExternallyActive(faceVertex, vertex))
		//				{
		//					// Console.WriteLine($"Second side iteration pertinent or externally active");

		//					secondSideVertex = faceVertex;
		//					break;
		//				}

		//				secondTail = faceVertex;
		//				// Console.WriteLine($"Second tail {faceVertex}");
		//			}

		//			var chosen = NullVertex;
		//			var choseFirstUpperPath = false;

		//			if (InternallyActive(firstSideVertex, vertex))
		//			{
		//				chosen = firstSideVertex;
		//				choseFirstUpperPath = true;
		//			}
		//			else if (InternallyActive(secondSideVertex, vertex))
		//			{
		//				chosen = secondSideVertex;
		//				choseFirstUpperPath = false;
		//			} 
		//			else if (Pertinent(firstSideVertex, vertex))
		//			{
		//				chosen = firstSideVertex;
		//				choseFirstUpperPath = true;
		//			}
		//			else if (Pertinent(secondSideVertex, vertex))
		//			{
		//				chosen = secondSideVertex;
		//				choseFirstUpperPath = false;
		//			}
		//			else
		//			{
		//				// If there's a pertinent vertex on the lower face
		//				// between the first_face_itr and the second_face_itr,
		//				// this graph isn't planar.

		//				foreach (var faceVertex in IterateFace(firstSideVertex, firstTail))
		//				{
		//					if (faceVertex == secondSideVertex)
		//					{
		//						break;
		//					}

		//					// Console.WriteLine($"Kuratowski check {faceVertex}");

		//					if (Pertinent(faceVertex, vertex))
		//					{
		//						// Console.WriteLine($"Kuratowski found");

		//						return false;
		//					}
		//				}


		//				// Otherwise, the fact that we didn't find a pertinent
		//				// vertex on this face is fine - we should set the
		//				// short-circuit edges and break out of this loop to
		//				// start looking at a different pertinent root.

		//				if (firstSideVertex == secondSideVertex)
		//				{
		//					if (firstTail != vertex)
		//					{
		//						var first = faceHandlesMap[firstTail].FirstVertex;
		//						var second = faceHandlesMap[firstTail].SecondVertex;

		//						var tmpFirstTail = firstTail;
		//						firstTail = first == firstSideVertex ? second : first;
		//						firstSideVertex = tmpFirstTail;
		//					}
		//					else if (secondTail != vertex)
		//					{
		//						var first = faceHandlesMap[secondTail].FirstVertex;
		//						var second = faceHandlesMap[secondTail].SecondVertex;

		//						var tmpSecondTrail = secondTail;
		//						secondTail = first == secondSideVertex ? second : first;
		//						secondSideVertex = tmpSecondTrail;
		//					}
		//					else
		//					{
		//						break;
		//					}
		//				}

		//				canonicalDFSChildMap[firstSideVertex] = canonicalDFSChildMap[rootFaceHandle.FirstVertex];
		//				canonicalDFSChildMap[secondSideVertex] = canonicalDFSChildMap[rootFaceHandle.SecondVertex];

		//				rootFaceHandle.SetFirstVertex(firstSideVertex);
		//				rootFaceHandle.SetSecondVertex(secondSideVertex);

		//				if (faceHandlesMap[firstSideVertex].FirstVertex == firstTail)
		//				{
		//					// Console.WriteLine("Case aa");
		//					faceHandlesMap[firstSideVertex].SetFirstVertex(vertex);
		//				}
		//				else
		//				{
		//					// Console.WriteLine("Case ab");
		//					faceHandlesMap[firstSideVertex].SetSecondVertex(vertex);
		//				}

		//				if (faceHandlesMap[secondSideVertex].FirstVertex == secondTail)
		//				{
		//					// Console.WriteLine("Case ba");
		//					faceHandlesMap[secondSideVertex].SetFirstVertex(vertex);
		//				}
		//				else
		//				{
		//					// Console.WriteLine("Case bb");
		//					faceHandlesMap[secondSideVertex].SetSecondVertex(vertex);
		//				}

		//				break;
		//			}

		//			// Console.WriteLine($"Chosen {chosen}");

		//			// When we unwind the stack, we need to know which direction
		//			// we came down from on the top face handle

		//			var choseFirstLowerPath = (choseFirstUpperPath && faceHandlesMap[chosen].FirstVertex == firstTail)
		//			                          || (!choseFirstUpperPath && faceHandlesMap[chosen].FirstVertex == secondTail);

		//			//If there's a backedge at the chosen vertex, embed it now

		//			if (backedgeFlag[chosen] == dfsNumberMap[vertex])
		//			{
		//				w = chosen;

		//				backedgeFlag[chosen] = graphOld.VerticesCount + 1; // TODO: check if consistent
		//				// add_to_merge_points(chosen, StoreOldHandlesPolicy());

		//				foreach (var edge in backedges[chosen])
		//				{
		//					// add_to_embedded_edges(e, StoreOldHandlesPolicy());

		//					if (choseFirstLowerPath)
		//					{
		//						// Console.WriteLine($"Push first {edge}");
		//						faceHandlesMap[chosen].PushFirst(edge);
		//					}
		//					else
		//					{
		//						// Console.WriteLine($"Push second {edge}");
		//						faceHandlesMap[chosen].PushSecond(edge);
		//					}
		//				}
		//			}
		//			else
		//			{
		//				mergeStack.Push(new MergeInfo(chosen, choseFirstUpperPath, choseFirstLowerPath));
		//				currentFaceHandle = pertinentRootsMap[chosen].First.Value;
		//				continue;
		//			}

		//			//Unwind the merge stack to the root, merging all bicomps
		//			var bottomPathFollowsFirst = false;
		//			var topPathFollowsFirst = false;
		//			var nextBottomFollowsFirst = choseFirstUpperPath;

		//			var mergePoint = chosen;

		//			while (mergeStack.Count != 0)
		//			{
		//				bottomPathFollowsFirst = nextBottomFollowsFirst;
		//				var mergeInfo = mergeStack.Pop();

		//				mergePoint = mergeInfo.Vertex;
		//				nextBottomFollowsFirst = mergeInfo.ChoseFirstUpperPath;
		//				topPathFollowsFirst = mergeInfo.ChoseFirstLowerPath;

		//				var topHandle = faceHandlesMap[mergePoint];
		//				var bottomHandle = pertinentRootsMap[mergePoint].First.Value;
		//				var bottomDFSChild = canonicalDFSChildMap[pertinentRootsMap[mergePoint].First.Value.FirstVertex];

		//				RemoveVertexFromSeparatedDFSChildList(canonicalDFSChildMap[pertinentRootsMap[mergePoint].First.Value.FirstVertex]);

		//				pertinentRootsMap[mergePoint].RemoveFirst();

		//				// add_to_merge_points(top_handle.get_anchor(), StoreOldHandlesPolicy());

		//				if (topPathFollowsFirst && bottomPathFollowsFirst)
		//				{
		//					// Console.WriteLine("Case 1");
		//					bottomHandle.Flip();
		//					topHandle.GlueFirstToSecond(bottomHandle);
		//				}
		//				else if (!topPathFollowsFirst && bottomPathFollowsFirst)
		//				{
		//					// Console.WriteLine("Case 2");
		//					flipped[bottomDFSChild] = true;
		//					topHandle.GlueSecondToFirst(bottomHandle);
		//				}
		//				else if (topPathFollowsFirst && !bottomPathFollowsFirst)
		//				{
		//					// Console.WriteLine("Case 3");
		//					flipped[bottomDFSChild] = true;
		//					topHandle.GlueFirstToSecond(bottomHandle);
		//				}
		//				else //!top_path_follows_first && !bottom_path_follows_first
		//				{
		//					// Console.WriteLine("Case 4");
		//					bottomHandle.Flip();
		//					topHandle.GlueSecondToFirst(bottomHandle);
		//				}
		//			}

		//			//Finally, embed all edges (v,w) at their upper end points

		//			canonicalDFSChildMap[w] = canonicalDFSChildMap[rootFaceHandle.FirstVertex];

		//			//add_to_merge_points(root_face_handle.get_anchor(),
		//			//	StoreOldHandlesPolicy()
		//			//);

		//			foreach (var edge in backedges[chosen])
		//			{
		//				if (nextBottomFollowsFirst)
		//				{
		//					// Console.WriteLine($"- Push first {edge}");
		//					rootFaceHandle.PushFirst(edge);
		//				}
		//				else
		//				{
		//					// Console.WriteLine($"- Push second {edge}");
		//					rootFaceHandle.PushSecond(edge);
		//				}
		//			}

		//			backedges[chosen].Clear();
		//			currentFaceHandle = rootFaceHandle;
		//		}
		//	}

		//	return true;
		//}

		public bool Walkdown(Vertex<T> vertex)
		{
			// Console.WriteLine($"Walkdown {vertex}");

			Vertex<T> w;

			mergeStackNew.Clear();

			while (vertex.PertinentRoots.Count != 0)
			{
				var rootFaceHandle = vertex.PertinentRoots.First.Value;
				vertex.PertinentRoots.RemoveFirst();
				var currentFaceHandle = rootFaceHandle;

				// Console.WriteLine($"Pertinent root anchor {rootFaceHandle.Anchor}");

				while (true)
				{
					Vertex<T> firstSideVertex = null;
					Vertex<T> secondSideVertex = null;
					var firstTail = currentFaceHandle.Anchor;
					var secondTail = currentFaceHandle.Anchor;
					var firstSideVertices = IterateFirstSide(currentFaceHandle);

					foreach (var faceVertex in firstSideVertices)
					{
						// Console.WriteLine($"First side iteration {faceVertex}, pertinent = {Pertinent(faceVertex, vertex)}, externally active = {ExternallyActive(faceVertex, vertex)}");

						if (Pertinent(faceVertex, vertex) || ExternallyActive(faceVertex, vertex))
						{
							// Console.WriteLine($"First side iteration pertinent or externally active");

							firstSideVertex = faceVertex;
							secondSideVertex = faceVertex;
							break;
						}

						firstTail = faceVertex;
					}

					if (firstSideVertex == null || firstSideVertex == currentFaceHandle.Anchor)
					{
						// Console.WriteLine($"Break");
						break;
					}

					foreach (var faceVertex in IterateSecondSide(currentFaceHandle))
					{
						// Console.WriteLine($"Second side iteration {faceVertex}, pertinent = {Pertinent(faceVertex, vertex)}, externally active = {ExternallyActive(faceVertex, vertex)}");

						if (Pertinent(faceVertex, vertex) || ExternallyActive(faceVertex, vertex))
						{
							// Console.WriteLine($"Second side iteration pertinent or externally active");

							secondSideVertex = faceVertex;
							break;
						}

						secondTail = faceVertex;
						// Console.WriteLine($"Second tail {faceVertex}");
					}

					Vertex<T> chosen = null;
					var choseFirstUpperPath = false;

					if (InternallyActive(firstSideVertex, vertex))
					{
						chosen = firstSideVertex;
						choseFirstUpperPath = true;
					}
					else if (InternallyActive(secondSideVertex, vertex))
					{
						chosen = secondSideVertex;
						choseFirstUpperPath = false;
					}
					else if (Pertinent(firstSideVertex, vertex))
					{
						chosen = firstSideVertex;
						choseFirstUpperPath = true;
					}
					else if (Pertinent(secondSideVertex, vertex))
					{
						chosen = secondSideVertex;
						choseFirstUpperPath = false;
					}
					else
					{
						// If there's a pertinent vertex on the lower face
						// between the first_face_itr and the second_face_itr,
						// this graph isn't planar.

						foreach (var faceVertex in IterateFace(firstSideVertex, firstTail))
						{
							if (faceVertex == secondSideVertex)
							{
								break;
							}

							// Console.WriteLine($"Kuratowski check {faceVertex}");

							if (Pertinent(faceVertex, vertex))
							{
								// Console.WriteLine($"Kuratowski found");

								return false;
							}
						}


						// Otherwise, the fact that we didn't find a pertinent
						// vertex on this face is fine - we should set the
						// short-circuit edges and break out of this loop to
						// start looking at a different pertinent root.

						if (firstSideVertex == secondSideVertex)
						{
							if (firstTail != vertex)
							{
								var first = firstTail.FaceHandle.FirstVertex;
								var second = firstTail.FaceHandle.SecondVertex;

								var tmpFirstTail = firstTail;
								firstTail = first == firstSideVertex ? second : first;
								firstSideVertex = tmpFirstTail;
							}
							else if (secondTail != vertex)
							{
								var first = secondTail.FaceHandle.FirstVertex;
								var second = secondTail.FaceHandle.SecondVertex;

								var tmpSecondTrail = secondTail;
								secondTail = first == secondSideVertex ? second : first;
								secondSideVertex = tmpSecondTrail;
							}
							else
							{
								break;
							}
						}

						firstSideVertex.CanonicalDFSChild = rootFaceHandle.FirstVertex.CanonicalDFSChild;
						secondSideVertex.CanonicalDFSChild = rootFaceHandle.SecondVertex.CanonicalDFSChild;

						rootFaceHandle.SetFirstVertex(firstSideVertex);
						rootFaceHandle.SetSecondVertex(secondSideVertex);

						if (firstSideVertex.FaceHandle.FirstVertex == firstTail)
						{
							// Console.WriteLine("Case aa");
							firstSideVertex.FaceHandle.SetFirstVertex(vertex);
						}
						else
						{
							// Console.WriteLine("Case ab");
							firstSideVertex.FaceHandle.SetSecondVertex(vertex);
						}

						if (secondSideVertex.FaceHandle.FirstVertex == secondTail)
						{
							// Console.WriteLine("Case ba");
							secondSideVertex.FaceHandle.SetFirstVertex(vertex);
						}
						else
						{
							// Console.WriteLine("Case bb");
							secondSideVertex.FaceHandle.SetSecondVertex(vertex);
						}

						break;
					}

					// Console.WriteLine($"Chosen {chosen}");

					// When we unwind the stack, we need to know which direction
					// we came down from on the top face handle

					var choseFirstLowerPath = (choseFirstUpperPath && chosen.FaceHandle.FirstVertex == firstTail)
											  || (!choseFirstUpperPath && chosen.FaceHandle.FirstVertex == secondTail);

					//If there's a backedge at the chosen vertex, embed it now

					if (chosen.BackedgeFlag == vertex.DFSNumber)
					{
						w = chosen;

						chosen.BackedgeFlag = transformedGraph.VerticesCount + 1; // TODO: check if consistent
																		   // add_to_merge_points(chosen, StoreOldHandlesPolicy());

						foreach (var edge in chosen.BackEdges)
						{
							// add_to_embedded_edges(e, StoreOldHandlesPolicy());

							if (choseFirstLowerPath)
							{
								// Console.WriteLine($"Push first {edge}");
								chosen.FaceHandle.PushFirst(edge);
							}
							else
							{
								// Console.WriteLine($"Push second {edge}");
								chosen.FaceHandle.PushSecond(edge);
							}
						}
					}
					else
					{
						mergeStackNew.Push(new MergeInfoNew(chosen, choseFirstUpperPath, choseFirstLowerPath));
						currentFaceHandle = chosen.PertinentRoots.First.Value;
						continue;
					}

					//Unwind the merge stack to the root, merging all bicomps
					var bottomPathFollowsFirst = false;
					var topPathFollowsFirst = false;
					var nextBottomFollowsFirst = choseFirstUpperPath;

					var mergePoint = chosen;

					while (mergeStackNew.Count != 0)
					{
						bottomPathFollowsFirst = nextBottomFollowsFirst;
						var mergeInfo = mergeStackNew.Pop();

						mergePoint = mergeInfo.Vertex;
						nextBottomFollowsFirst = mergeInfo.ChoseFirstUpperPath;
						topPathFollowsFirst = mergeInfo.ChoseFirstLowerPath;

						var topHandle = mergePoint.FaceHandle;
						var bottomHandle = mergePoint.PertinentRoots.First.Value;
						var bottomDFSChild = mergePoint.PertinentRoots.First.Value.FirstVertex.CanonicalDFSChild;

						RemoveVertexFromSeparatedDFSChildList(mergePoint.PertinentRoots.First.Value.FirstVertex.CanonicalDFSChild);

						mergePoint.PertinentRoots.RemoveFirst();

						// add_to_merge_points(top_handle.get_anchor(), StoreOldHandlesPolicy());

						if (topPathFollowsFirst && bottomPathFollowsFirst)
						{
							// Console.WriteLine("Case 1");
							bottomHandle.Flip();
							topHandle.GlueFirstToSecond(bottomHandle);
						}
						else if (!topPathFollowsFirst && bottomPathFollowsFirst)
						{
							// Console.WriteLine("Case 2");
							bottomDFSChild.Flipped = true;
							topHandle.GlueSecondToFirst(bottomHandle);
						}
						else if (topPathFollowsFirst && !bottomPathFollowsFirst)
						{
							// Console.WriteLine("Case 3");
							bottomDFSChild.Flipped = true;
							topHandle.GlueFirstToSecond(bottomHandle);
						}
						else //!top_path_follows_first && !bottom_path_follows_first
						{
							// Console.WriteLine("Case 4");
							bottomHandle.Flip();
							topHandle.GlueSecondToFirst(bottomHandle);
						}
					}

					//Finally, embed all edges (v,w) at their upper end points

					w.CanonicalDFSChild = rootFaceHandle.FirstVertex.CanonicalDFSChild;

					//add_to_merge_points(root_face_handle.get_anchor(),
					//	StoreOldHandlesPolicy()
					//);

					foreach (var edge in chosen.BackEdges)
					{
						if (nextBottomFollowsFirst)
						{
							// Console.WriteLine($"- Push first {edge}");
							rootFaceHandle.PushFirst(edge);
						}
						else
						{
							// Console.WriteLine($"- Push second {edge}");
							rootFaceHandle.PushSecond(edge);
						}
					}

					chosen.BackEdges.Clear();
					currentFaceHandle = rootFaceHandle;
				}
			}

			return true;
		}

		private void Cleanup()
		{
			// If the graph isn't biconnected, we'll still have entries
			// in the separated_dfs_child_list for some vertices. Since
			// these represent articulation points, we can obtain a
			// planar embedding no matter what order we embed them in.
			foreach (var vertex in graphOld.Vertices)
			{
				if (separatedDFSChildListMap[vertex].Count != 0)
				{
					foreach (var childVertex in separatedDFSChildListMap[vertex])
					{
						dfsChildHandlesMap[childVertex].Flip();
						faceHandlesMap[vertex].GlueFirstToSecond(dfsChildHandlesMap[childVertex]);
					}
				}
			}

			// Up until this point, we've flipped bicomps lazily by setting
			// flipped[v] to true if the bicomp rooted at v was flipped (the
			// lazy aspect of this flip is that all descendents of that vertex
			// need to have their orientations reversed as well). Now, we
			// traverse the DFS tree by DFS number and perform the actual
			// flipping as needed
			foreach (var vertex in verticesByDFSNumber)
			{
				var vertexFlipped = flipped[vertex];
				var parentFlipped = flipped[parentMap[vertex]];

				if (vertexFlipped && !parentFlipped)
				{
					faceHandlesMap[vertex].Flip();
				}
				else if (parentFlipped && !vertexFlipped)
				{
					faceHandlesMap[vertex].Flip();
					flipped[vertex] = true;
				}
				else
				{
					flipped[vertex] = false;
				}
			}

			// If there are any self-loops in the graph, they were flagged
			// during the walkup, and we should add them to the embedding now.
			// Adding a self loop anywhere in the embedding could never
			// invalidate the embedding, but they would complicate the traversal
			// if they were added during the walkup/walkdown.
			foreach (var edge in selfLoops)
			{
				faceHandlesMap[edge.Source].PushSecond(edge);
			}
		}

		private void CleanupNew()
		{
			// If the graph isn't biconnected, we'll still have entries
			// in the separated_dfs_child_list for some vertices. Since
			// these represent articulation points, we can obtain a
			// planar embedding no matter what order we embed them in.
			foreach (var vertex in transformedGraph.Vertices)
			{
				if (vertex.SeparatedDFSChildList.Count != 0)
				{
					foreach (var childVertex in vertex.SeparatedDFSChildList)
					{
						childVertex.DFSChildHandle.Flip();
						vertex.FaceHandle.GlueFirstToSecond(childVertex.DFSChildHandle);
					}
				}
			}

			// Up until this point, we've flipped bicomps lazily by setting
			// flipped[v] to true if the bicomp rooted at v was flipped (the
			// lazy aspect of this flip is that all descendents of that vertex
			// need to have their orientations reversed as well). Now, we
			// traverse the DFS tree by DFS number and perform the actual
			// flipping as needed
			foreach (var vertex in verticesByDFSNumberNew)
			{
				var vertexFlipped = vertex.Flipped;
				var parentFlipped = vertex.Parent.Flipped;

				if (vertexFlipped && !parentFlipped)
				{
					vertex.FaceHandle.Flip();
				}
				else if (parentFlipped && !vertexFlipped)
				{
					vertex.FaceHandle.Flip();
					vertex.Flipped = true;
				}
				else
				{
					vertex.Flipped = false;
				}
			}

			// If there are any self-loops in the graph, they were flagged
			// during the walkup, and we should add them to the embedding now.
			// Adding a self loop anywhere in the embedding could never
			// invalidate the embedding, but they would complicate the traversal
			// if they were added during the walkup/walkdown.
			foreach (var edge in selfLoopsNew)
			{
				edge.Source.FaceHandle.PushSecond(edge);
			}
		}

		private void Dump()
		{
			// Console.WriteLine();
			// Console.WriteLine("-- DUMP START --");

			foreach (var vertex in transformedGraph.Vertices)
			{
				var faceHandle = vertex.FaceHandle;
				// Console.WriteLine($"{vertex.Value} - {faceHandle}");
			}

			// Console.WriteLine("-- DUMP END --");
			// Console.WriteLine();
		}

		private bool Pertinent(int vertex, int otherVertex)
		{
			// w is pertinent with respect to v if there is a backedge (v,w) or if
			// w is the root of a bicomp that contains a pertinent vertex.

			return backedgeFlag[vertex] == dfsNumberMap[otherVertex] || pertinentRootsMap[vertex].Count != 0;
		}

		private bool Pertinent(Vertex<T> vertex, Vertex<T> otherVertex)
		{
			// w is pertinent with respect to v if there is a backedge (v,w) or if
			// w is the root of a bicomp that contains a pertinent vertex.

			return vertex.BackedgeFlag == otherVertex.DFSNumber || vertex.PertinentRoots.Count != 0;
		}

		private bool ExternallyActive(int vertex, int otherVertex)
		{
			// Let a be any proper depth-first search ancestor of v. w is externally
			// active with respect to v if there exists a backedge (a,w) or a
			// backedge (a,w_0) for some w_0 in a descendent bicomp of w.

			var otherVertexDFSNumber = dfsNumberMap[otherVertex];

			return leastAncestorMap[vertex] < otherVertexDFSNumber
			       || (separatedDFSChildListMap[vertex].Count != 0 &&
			           lowPointMap[separatedDFSChildListMap[vertex].First.Value] < otherVertexDFSNumber);
		}

		private bool ExternallyActive(Vertex<T> vertex, Vertex<T> otherVertex)
		{
			// Let a be any proper depth-first search ancestor of v. w is externally
			// active with respect to v if there exists a backedge (a,w) or a
			// backedge (a,w_0) for some w_0 in a descendent bicomp of w.

			var otherVertexDFSNumber = otherVertex.DFSNumber;

			return vertex.LeastAncestor < otherVertexDFSNumber
			       || (vertex.SeparatedDFSChildList.Count != 0 &&
			           vertex.SeparatedDFSChildList.First.Value.LowPoint < otherVertexDFSNumber);
		}

		private bool InternallyActive(int vertex, int otherVertex)
		{
			return Pertinent(vertex, otherVertex) && !ExternallyActive(vertex, otherVertex);
		}

		private bool InternallyActive(Vertex<T> vertex, Vertex<T> otherVertex)
		{
			return Pertinent(vertex, otherVertex) && !ExternallyActive(vertex, otherVertex);
		}

		private void RemoveVertexFromSeparatedDFSChildList(int vertex)
		{
			var toDelete = separatedNodeInParentList[vertex];
			var list = separatedDFSChildListMap[parentMap[vertex]];

			list.Remove(toDelete);
		}

		private void RemoveVertexFromSeparatedDFSChildList(Vertex<T> vertex)
		{
			var toDelete = vertex.SeparatedNodeInParentList;
			var list = vertex.Parent.SeparatedDFSChildList;

			list.Remove(toDelete);
		}



		public IEnumerable<int> IterateFirstSide(int vertex, bool visitLead = true)
		{
			var face = faceHandlesMap[vertex];

			return IterateFace(face, x => x.FirstVertex, visitLead);
		}

		public IEnumerable<int> IterateFirstSide(FaceHandle<int> face, bool visitLead = true)
		{
			return IterateFace(face, x => x.FirstVertex, visitLead);
		}

		public IEnumerable<int> IterateSecondSide(int vertex, bool visitLead = true)
		{
			var face = faceHandlesMap[vertex];

			return IterateFace(face, x => x.SecondVertex, visitLead);
		}

		public IEnumerable<int> IterateSecondSide(FaceHandle<int> face, bool visitLead = true)
		{
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

				face = faceHandlesMap[lead];

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
			}
		}

		public IEnumerable<int> IterateFace(int lead, int follow, bool visitLead = true)
		{
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

				var face = faceHandlesMap[lead];

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
			}
		}

		// Always visits follow!
		public IEnumerable<int> IterateBothSides(int vertex)
		{
			var face = faceHandlesMap[vertex];

			return IterateBothSides(face);
		}

		/// <summary>
		/// Iterates over both sides of the face in parallel until the root is found.
		/// </summary>
		/// <remarks>
		/// Only "follow" vertices are returned. 
		/// 
		/// The first returned vertex is the anchor of a given face. Both iterators are then
		/// advanced so that anchor point is not returned twice. After that, both iterators
		/// alternate in returning the current follow vertex. If any of the iterators encounter
		/// the root vertex, the whole process ends.
		/// </remarks>
		/// <param name="face"></param>
		/// <returns></returns>
		public IEnumerable<int> IterateBothSides(FaceHandle<int> face)
		{
			var firstEnumerable = IterateFirstSide(face, false);
			var secondEnumerable = IterateSecondSide(face, false);
			var firstActive = false;

			using (var firstEnumerator = firstEnumerable.GetEnumerator())
			{
				using (var secondEnumerator = secondEnumerable.GetEnumerator())
				{
					if (!firstEnumerator.MoveNext() || !secondEnumerator.MoveNext())
					{
						yield break;
					}

					yield return firstEnumerator.Current;

					if (!firstEnumerator.MoveNext() || !secondEnumerator.MoveNext())
					{
						yield break;
					}

					while (true)
					{
						if (firstActive)
						{
							yield return firstEnumerator.Current;

							if (!firstEnumerator.MoveNext())
							{
								yield break;
							}
						}
						else
						{
							yield return secondEnumerator.Current;

							if (!secondEnumerator.MoveNext())
							{
								yield break;
							}
						}

						firstActive = !firstActive;
					}
				}
			}
		}

		private class MergeInfo
		{
			public int Vertex { get; }

			public bool ChoseFirstUpperPath { get; }

			public bool ChoseFirstLowerPath { get; }

			public MergeInfo(int vertex, bool choseFirstUpperPath, bool choseFirstLowerPath)
			{
				Vertex = vertex;
				ChoseFirstUpperPath = choseFirstUpperPath;
				ChoseFirstLowerPath = choseFirstLowerPath;
			}
		}

		private class MergeInfoNew
		{
			public Vertex<T> Vertex { get; }

			public bool ChoseFirstUpperPath { get; }

			public bool ChoseFirstLowerPath { get; }

			public MergeInfoNew(Vertex<T> vertex, bool choseFirstUpperPath, bool choseFirstLowerPath)
			{
				Vertex = vertex;
				ChoseFirstUpperPath = choseFirstUpperPath;
				ChoseFirstLowerPath = choseFirstLowerPath;
			}
		}













		public IEnumerable<Vertex<T>> IterateFirstSide(Vertex<T> vertex, bool visitLead = true)
		{
			return IterateFace(vertex.FaceHandle, x => x.FirstVertex, visitLead);
		}

		public IEnumerable<Vertex<T>> IterateFirstSide(FaceHandle<Vertex<T>> face, bool visitLead = true)
		{
			return IterateFace(face, x => x.FirstVertex, visitLead);
		}

		public IEnumerable<Vertex<T>> IterateSecondSide(Vertex<T> vertex, bool visitLead = true)
		{
			return IterateFace(vertex.FaceHandle, x => x.SecondVertex, visitLead);
		}

		public IEnumerable<Vertex<T>> IterateSecondSide(FaceHandle<Vertex<T>> face, bool visitLead = true)
		{
			return IterateFace(face, x => x.SecondVertex, visitLead);
		}

		public IEnumerable<Vertex<T>> IterateFace(FaceHandle<Vertex<T>> face, Func<FaceHandle<Vertex<T>>, Vertex<T>> leadSelector, bool visitLead = true)
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

				face = lead.FaceHandle;

				var first = face.FirstVertex;
				var second = face.SecondVertex;

				if (first == follow)
				{
					follow = lead;
					lead = second;
				}
				else if (second == follow)
				{
					follow = lead;
					lead = first;
				}
				else
				{
					yield break;
				}
			}
		}

		public IEnumerable<Vertex<T>> IterateFace(Vertex<T> lead, Vertex<T> follow, bool visitLead = true)
		{
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

				var face = lead.FaceHandle;

				var first = face.FirstVertex;
				var second = face.SecondVertex;

				if (first == follow)
				{
					follow = lead;
					lead = second;
				}
				else if (second == follow)
				{
					follow = lead;
					lead = first;
				}
				else
				{
					yield break;
				}
			}
		}

		// Always visits follow!
		public IEnumerable<Vertex<T>> IterateBothSides(Vertex<T> vertex)
		{
			return IterateBothSides(vertex.FaceHandle);
		}

		/// <summary>
		/// Iterates over both sides of the face in parallel until the root is found.
		/// </summary>
		/// <remarks>
		/// Only "follow" vertices are returned. 
		/// 
		/// The first returned vertex is the anchor of a given face. Both iterators are then
		/// advanced so that anchor point is not returned twice. After that, both iterators
		/// alternate in returning the current follow vertex. If any of the iterators encounter
		/// the root vertex, the whole process ends.
		/// </remarks>
		/// <param name="face"></param>
		/// <returns></returns>
		public IEnumerable<Vertex<T>> IterateBothSides(FaceHandle<Vertex<T>> face)
		{
			var firstEnumerable = IterateFirstSide(face, false);
			var secondEnumerable = IterateSecondSide(face, false);
			var firstActive = false;

			using (var firstEnumerator = firstEnumerable.GetEnumerator())
			{
				using (var secondEnumerator = secondEnumerable.GetEnumerator())
				{
					if (!firstEnumerator.MoveNext() || !secondEnumerator.MoveNext())
					{
						yield break;
					}

					yield return firstEnumerator.Current;

					if (!firstEnumerator.MoveNext() || !secondEnumerator.MoveNext())
					{
						yield break;
					}

					while (true)
					{
						if (firstActive)
						{
							yield return firstEnumerator.Current;

							if (!firstEnumerator.MoveNext())
							{
								yield break;
							}
						}
						else
						{
							yield return secondEnumerator.Current;

							if (!secondEnumerator.MoveNext())
							{
								yield break;
							}
						}

						firstActive = !firstActive;
					}
				}
			}
		}
	}
}