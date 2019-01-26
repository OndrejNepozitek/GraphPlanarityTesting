namespace GraphPlanarityTesting.PlanarityTesting.BoyerMyrvold
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Graphs.DataStructures;

	public class FaceHandle<T>
	{
		private T cachedFirstVertex;
		private T cachedSecondVertex;
		private T trueFirstVertex;
		private T trueSecondVertex;
		private IEdge<T> cachedFirstEdge;
		private IEdge<T> cachedSecondEdge;
		private T anchor;
		private LinkedList<IEdge<T>> edgeList;

		public T FirstVertex => cachedFirstVertex;

		public T SecondVertex => cachedSecondVertex;

		public T TrueFirstVertex => trueFirstVertex;

		public T TrueSecondVertex => trueSecondVertex;

		public IEdge<T> FirstEdge => cachedFirstEdge;

		public IEdge<T> SecondEdge => cachedSecondEdge;

		public T Anchor => anchor;

		public FaceHandle(T anchor)
		{
			this.anchor = anchor;
		}

		public FaceHandle(T anchor, IEdge<T> initialEdge)
		{
			var source = initialEdge.Source;
			var target = initialEdge.Target;
			var otherVertex = source.Equals(anchor) ? target : source;

			this.anchor = anchor;
			cachedFirstEdge = initialEdge;
			cachedSecondEdge = initialEdge;
			cachedFirstVertex = otherVertex;
			cachedSecondVertex = otherVertex;
			trueFirstVertex = otherVertex;
			trueSecondVertex = otherVertex;

			edgeList = new LinkedList<IEdge<T>>();
			edgeList.AddLast(initialEdge);
		}

		public void GlueFirstToSecond(FaceHandle<T> bottom)
		{
			if (edgeList == null)
			{
				edgeList = new LinkedList<IEdge<T>>(bottom.edgeList);
			}
			else
			{
				edgeList.PrependRange(bottom.edgeList); // TODO: slow
			}

			trueFirstVertex = bottom.trueFirstVertex;
			cachedFirstVertex = bottom.cachedFirstVertex;
			cachedFirstEdge = bottom.cachedFirstEdge;
		}

		public void GlueSecondToFirst(FaceHandle<T> bottom)
		{
			if (edgeList == null)
			{
				edgeList = new LinkedList<IEdge<T>>(bottom.edgeList);
			}
			else
			{
				edgeList.AppendRange(bottom.edgeList); // TODO: slow
			}

			trueSecondVertex = bottom.trueSecondVertex;
			cachedSecondVertex = bottom.cachedSecondVertex;
			cachedSecondEdge = bottom.cachedSecondEdge;
		}

		public void Flip()
		{
			edgeList = new LinkedList<IEdge<T>>(edgeList.Reverse());

			{
				var temp = trueFirstVertex;
				trueFirstVertex = trueSecondVertex;
				trueSecondVertex = temp;
			}

			{
				var temp = cachedFirstVertex;
				cachedFirstVertex = cachedSecondVertex;
				cachedSecondVertex = temp;
			}

			{
				var temp = cachedFirstEdge;
				cachedFirstEdge = cachedSecondEdge;
				cachedSecondEdge = temp;
			}
		}

		public IReadOnlyCollection<IEdge<T>> GetEdges()
		{
			return edgeList?.ToList().AsReadOnly();
		}

		public void SetFirstVertex(T vertex)
		{
			cachedFirstVertex = vertex;
		}

		public void SetSecondVertex(T vertex)
		{
			cachedSecondVertex = vertex;
		}

		public void PushFirst(IEdge<T> edge)
		{
			edgeList.AddFirst(edge);

			var otherVertex = edge.Source.Equals(anchor) ? edge.Target : edge.Source;

			cachedFirstVertex = otherVertex;
			trueFirstVertex = otherVertex;

			cachedFirstEdge = edge;
		}

		public void PushSecond(IEdge<T> edge)
		{
			edgeList.AddLast(edge);

			var otherVertex = edge.Source.Equals(anchor) ? edge.Target : edge.Source;

			cachedSecondVertex = otherVertex;
			trueSecondVertex = otherVertex;

			cachedSecondEdge = edge;
		}

		public override string ToString()
		{
			var s =
				$"Anchor {Anchor}, First {trueFirstVertex} {cachedFirstVertex}, Second {trueSecondVertex} {cachedSecondVertex}, ";

			if (edgeList != null)
			{
				foreach (var edge in edgeList)
				{
					s += $"{edge} ";
				}
			}
			else
			{
				s += "no edges";
			}

			return s;
		}
	}
}