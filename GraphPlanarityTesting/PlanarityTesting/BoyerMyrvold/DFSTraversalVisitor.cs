namespace GraphPlanarityTesting.PlanarityTesting.BoyerMyrvold
{
	using System;
	using System.Collections.Generic;
	using Graphs.Algorithms;
	using Graphs.DataStructures;

	public class DFSTraversalVisitor<T> : BaseDFSTraversalVisitor<T>
	{
		public Dictionary<T, int> DFSNumber { get; }

		public Dictionary<T, T> Parent { get; }

		public Dictionary<T, int> LowPoint { get; }

		public Dictionary<T, int> LeastAncestor { get; }

		public Dictionary<T, IEdge<T>> DFSEdge { get; }

		private int count = 0;

		public DFSTraversalVisitor(Dictionary<T, int> dfsNumber, Dictionary<T, T> parent, Dictionary<T, int> lowPoint, Dictionary<T, int> leastAncestor, Dictionary<T, IEdge<T>> dfsEdge)
		{
			DFSNumber = dfsNumber;
			Parent = parent;
			LowPoint = lowPoint;
			LeastAncestor = leastAncestor;
			DFSEdge = dfsEdge;
		}

		public override void StartVertex(T vertex, IGraph<T> graph)
		{
			Parent[vertex] = vertex;
			LeastAncestor[vertex] = count;
		}

		public override void DiscoverVertex(T vertex, IGraph<T> graph)
		{
			LowPoint[vertex] = count;
			DFSNumber[vertex] = count;
			count++;
		}

		public override void TreeEdge(IEdge<T> edge, IGraph<T> graph)
		{
			var source = edge.Source;
			var target = edge.Target;

			Parent[target] = source;
			DFSEdge[target] = edge;
			LeastAncestor[target] = DFSNumber[source];
		}

		public override void BackEdge(IEdge<T> edge, IGraph<T> graph)
		{
			var source = edge.Source;
			var target = edge.Target;

			if (!target.Equals(Parent[source]))
			{
				var sourceLowPoint = LowPoint[source];
				var targetDFSNumber = DFSNumber[target];
				var sourceLeastAncestorDFSNumber = LeastAncestor[source];

				LowPoint[source] = Math.Min(sourceLowPoint, targetDFSNumber);
				LeastAncestor[source] = Math.Min(sourceLeastAncestorDFSNumber, targetDFSNumber);
			}
		}

		public override void FinishVertex(T vertex, IGraph<T> graph)
		{
			var lowPoint = LowPoint[vertex];
			var parent = Parent[vertex];
			var parentLowpoint = LowPoint[parent];

			if (!Parent[vertex].Equals(vertex))
			{
				LowPoint[parent] = Math.Min(lowPoint, parentLowpoint);
			}
		}
	}
}