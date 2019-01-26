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

		private Dictionary<int, LinkedList<FaceHandle<int>>> pertinentRootsMap;

		private Dictionary<int, LinkedList<int>> separatedDFSChildListMap;

		private Dictionary<int, LinkedListNode<int>> separatedNodeInParentList;

		private List<IEdge<int>> selfLoops;

		private Dictionary<int, List<IEdge<int>>> backedges;

		private Dictionary<int, int> backedgeFlag;

		private Dictionary<int, int> visited;

		private Stack<MergeInfo> mergeStack;

		private Dictionary<int, bool> flipped;

		private IGraph<int> graph;

		public bool IsPlanar(IGraph<int> graph, out Dictionary<int, List<IEdge<int>>> embedding)
		{
			dfsNumberMap = new Dictionary<int, int>();
			parentMap = new Dictionary<int, int>();
			lowPointMap = new Dictionary<int, int>();
			leastAncestorMap = new Dictionary<int, int>();
			dfsEdgeMap = new Dictionary<int, IEdge<int>>();
			faceHandlesMap = new Dictionary<int, FaceHandle<int>>();
			dfsChildHandlesMap = new Dictionary<int, FaceHandle<int>>();
			pertinentRootsMap = new Dictionary<int, LinkedList<FaceHandle<int>>>();
			separatedDFSChildListMap = new Dictionary<int, LinkedList<int>>();
			separatedNodeInParentList = new Dictionary<int, LinkedListNode<int>>();
			canonicalDFSChildMap = new Dictionary<int, int>();
			selfLoops = new List<IEdge<int>>();
			backedges = new Dictionary<int, List<IEdge<int>>>();
			backedgeFlag = new Dictionary<int, int>();
			visited = new Dictionary<int, int>();
			mergeStack = new Stack<MergeInfo>();
			flipped = new Dictionary<int, bool>();
			this.graph = graph;

			var visitor = new DFSTraversalVisitor<int>(dfsNumberMap, parentMap, lowPointMap, leastAncestorMap, dfsEdgeMap);
			var dfsTraversal = new DFSTraversal();
			dfsTraversal.TraverseRecursive(graph, visitor);

			// Init backedges
			foreach (var vertex in graph.Vertices)
			{
				backedges.Add(vertex, new List<IEdge<int>>());
				visited[vertex] = Int32.MaxValue; // TODO: not ideal
				backedgeFlag[vertex] = graph.VerticesCount + 1;
				flipped[vertex] = false;
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
				pertinentRootsMap[vertex] = new LinkedList<FaceHandle<int>>();
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

				if (!Walkdown(vertex))
				{
					embedding = null;
					return false;
				}

				Dump();
			}

			Cleanup();
			Dump();

			embedding = new Dictionary<int, List<IEdge<int>>>();
			foreach (var vertex in graph.Vertices)
			{
				var faceHandle = faceHandlesMap[vertex];
				embedding[vertex] = faceHandle?.GetEdges().ToList();
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
						pertinentRootsMap[parent].AddLast(dfsChildHandlesMap[dfsChild]);
					}
					else
					{
						pertinentRootsMap[parent].AddFirst(dfsChildHandlesMap[dfsChild]);
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

		public bool Walkdown(int vertex)
		{
			Console.WriteLine($"Walkdown {vertex}");

			int w;

			mergeStack.Clear();

			while (pertinentRootsMap[vertex].Count != 0)
			{
				var rootFaceHandle = pertinentRootsMap[vertex].First.Value;
				pertinentRootsMap[vertex].RemoveFirst();
				var currentFaceHandle = rootFaceHandle;

				Console.WriteLine($"Pertinent root anchor {rootFaceHandle.Anchor}");

				while (true)
				{
					var firstSideVertex = NullVertex;
					var secondSideVertex = NullVertex;
					var firstTail = currentFaceHandle.Anchor;
					var secondTail = currentFaceHandle.Anchor;

					foreach (var faceVertex in IterateFirstSide(currentFaceHandle))
					{
						Console.WriteLine($"First side iteration {faceVertex}, pertinent = {Pertinent(faceVertex, vertex)}, externally active = {ExternallyActive(faceVertex, vertex)}");

						if (Pertinent(faceVertex, vertex) || ExternallyActive(faceVertex, vertex))
						{
							Console.WriteLine($"First side iteration pertinent or externally active");

							firstSideVertex = faceVertex;
							secondSideVertex = faceVertex;
							break;
						}

						firstTail = faceVertex;
					}

					if (firstSideVertex == NullVertex || firstSideVertex == currentFaceHandle.Anchor)
					{
						Console.WriteLine($"Break");
						break;
					}
						
					foreach (var faceVertex in IterateSecondSide(currentFaceHandle))
					{
						Console.WriteLine($"Second side iteration {faceVertex}, pertinent = {Pertinent(faceVertex, vertex)}, externally active = {ExternallyActive(faceVertex, vertex)}");

						if (Pertinent(faceVertex, vertex) || ExternallyActive(faceVertex, vertex))
						{
							Console.WriteLine($"Second side iteration pertinent or externally active");

							secondSideVertex = faceVertex;
							break;
						}

						secondTail = faceVertex;
						Console.WriteLine($"Second tail {faceVertex}");
					}

					var chosen = NullVertex;
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

						// TODO:
						// throw new NotImplementedException();


						// Otherwise, the fact that we didn't find a pertinent
						// vertex on this face is fine - we should set the
						// short-circuit edges and break out of this loop to
						// start looking at a different pertinent root.

						if (firstSideVertex == secondSideVertex)
						{
							if (firstTail != vertex)
							{
								var first = faceHandlesMap[firstTail].FirstVertex;
								var second = faceHandlesMap[firstTail].SecondVertex;

								var tmpFirstTail = firstTail;
								firstTail = first == firstSideVertex ? second : first;
								firstSideVertex = tmpFirstTail;
							}
							else if (secondTail != vertex)
							{
								var first = faceHandlesMap[secondTail].FirstVertex;
								var second = faceHandlesMap[secondTail].SecondVertex;

								var tmpSecondTrail = secondTail;
								secondTail = first == secondSideVertex ? second : first;
								secondSideVertex = tmpSecondTrail;
							}
							else
							{
								break;
							}
						}

						canonicalDFSChildMap[firstSideVertex] = canonicalDFSChildMap[rootFaceHandle.FirstVertex];
						canonicalDFSChildMap[secondSideVertex] = canonicalDFSChildMap[rootFaceHandle.SecondVertex];

						rootFaceHandle.SetFirstVertex(firstSideVertex);
						rootFaceHandle.SetSecondVertex(secondSideVertex);

						if (faceHandlesMap[firstSideVertex].FirstVertex == firstTail)
						{
							Console.WriteLine("Case aa");
							faceHandlesMap[firstSideVertex].SetFirstVertex(vertex);
						}
						else
						{
							Console.WriteLine("Case ab");
							faceHandlesMap[firstSideVertex].SetSecondVertex(vertex);
						}

						if (faceHandlesMap[secondSideVertex].FirstVertex == secondTail)
						{
							Console.WriteLine("Case ba");
							faceHandlesMap[secondSideVertex].SetFirstVertex(vertex);
						}
						else
						{
							Console.WriteLine("Case bb");
							faceHandlesMap[secondSideVertex].SetSecondVertex(vertex);
						}

						break;
					}

					Console.WriteLine($"Chosen {chosen}");

					// When we unwind the stack, we need to know which direction
					// we came down from on the top face handle

					var choseFirstLowerPath = (choseFirstUpperPath && faceHandlesMap[chosen].FirstVertex == firstTail)
					                          || (!choseFirstUpperPath && faceHandlesMap[chosen].FirstVertex == secondTail);

					//If there's a backedge at the chosen vertex, embed it now

					if (backedgeFlag[chosen] == dfsNumberMap[vertex])
					{
						w = chosen;

						backedgeFlag[chosen] = graph.VerticesCount + 1; // TODO: check if consistent
						// add_to_merge_points(chosen, StoreOldHandlesPolicy());

						foreach (var edge in backedges[chosen])
						{
							// add_to_embedded_edges(e, StoreOldHandlesPolicy());

							if (choseFirstLowerPath)
							{
								Console.WriteLine($"Push first {edge}");
								faceHandlesMap[chosen].PushFirst(edge);
							}
							else
							{
								Console.WriteLine($"Push second {edge}");
								faceHandlesMap[chosen].PushSecond(edge);
							}
						}
					}
					else
					{
						mergeStack.Push(new MergeInfo(chosen, choseFirstUpperPath, choseFirstLowerPath));
						currentFaceHandle = pertinentRootsMap[chosen].First.Value;
						continue;
					}

					//Unwind the merge stack to the root, merging all bicomps
					var bottomPathFollowsFirst = false;
					var topPathFollowsFirst = false;
					var nextBottomFollowsFirst = choseFirstUpperPath;

					var mergePoint = chosen;

					while (mergeStack.Count != 0)
					{
						bottomPathFollowsFirst = nextBottomFollowsFirst;
						var mergeInfo = mergeStack.Pop();

						mergePoint = mergeInfo.Vertex;
						nextBottomFollowsFirst = mergeInfo.ChoseFirstUpperPath;
						topPathFollowsFirst = mergeInfo.ChoseFirstLowerPath;

						var topHandle = faceHandlesMap[mergePoint];
						var bottomHandle = pertinentRootsMap[mergePoint].First.Value;
						var bottomDFSChild = canonicalDFSChildMap[pertinentRootsMap[mergePoint].First.Value.FirstVertex];

						RemoveVertexFromSeparatedDFSChildList(canonicalDFSChildMap[pertinentRootsMap[mergePoint].First.Value.FirstVertex]);

						pertinentRootsMap[mergePoint].RemoveFirst();

						// add_to_merge_points(top_handle.get_anchor(), StoreOldHandlesPolicy());

						if (topPathFollowsFirst && bottomPathFollowsFirst)
						{
							Console.WriteLine("Case 1");
							bottomHandle.Flip();
							topHandle.GlueFirstToSecond(bottomHandle);
						}
						else if (!topPathFollowsFirst && bottomPathFollowsFirst)
						{
							Console.WriteLine("Case 2");
							flipped[bottomDFSChild] = true;
							topHandle.GlueSecondToFirst(bottomHandle);
						}
						else if (topPathFollowsFirst && !bottomPathFollowsFirst)
						{
							Console.WriteLine("Case 3");
							flipped[bottomDFSChild] = true;
							topHandle.GlueFirstToSecond(bottomHandle);
						}
						else //!top_path_follows_first && !bottom_path_follows_first
						{
							Console.WriteLine("Case 4");
							bottomHandle.Flip();
							topHandle.GlueSecondToFirst(bottomHandle);
						}
					}

					//Finally, embed all edges (v,w) at their upper end points

					canonicalDFSChildMap[w] = canonicalDFSChildMap[rootFaceHandle.FirstVertex];

					//add_to_merge_points(root_face_handle.get_anchor(),
					//	StoreOldHandlesPolicy()
					//);

					foreach (var edge in backedges[chosen])
					{
						if (nextBottomFollowsFirst)
						{
							Console.WriteLine($"- Push first {edge}");
							rootFaceHandle.PushFirst(edge);
						}
						else
						{
							Console.WriteLine($"- Push second {edge}");
							rootFaceHandle.PushSecond(edge);
						}
					}

					backedges[chosen].Clear();
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
			foreach (var vertex in graph.Vertices)
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

		private void Dump()
		{
			Console.WriteLine();
			Console.WriteLine("-- DUMP START --");

			foreach (var faceHandle in faceHandlesMap)
			{
				Console.WriteLine($"{faceHandle.Key} - {faceHandle.Value}");
			}

			Console.WriteLine("-- DUMP END --");
			Console.WriteLine();
		}

		private bool Pertinent(int vertex, int otherVertex)
		{
			// w is pertinent with respect to v if there is a backedge (v,w) or if
			// w is the root of a bicomp that contains a pertinent vertex.

			return backedgeFlag[vertex] == dfsNumberMap[otherVertex] || pertinentRootsMap[vertex].Count != 0;
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

		private bool InternallyActive(int vertex, int otherVertex)
		{
			return Pertinent(vertex, otherVertex) && !ExternallyActive(vertex, otherVertex);
		}

		private void RemoveVertexFromSeparatedDFSChildList(int vertex)
		{
			var toDelete = separatedNodeInParentList[vertex];
			var list = separatedDFSChildListMap[parentMap[vertex]];

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
	}
}