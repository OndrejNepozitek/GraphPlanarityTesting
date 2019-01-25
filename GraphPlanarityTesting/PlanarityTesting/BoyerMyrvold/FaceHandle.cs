namespace GraphPlanarityTesting.PlanarityTesting.BoyerMyrvold
{
	using System;
	using System.Collections.Generic;
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
		private List<IEdge<T>> edgeList;

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

			edgeList = new List<IEdge<T>> {initialEdge};
		}

		public void GlueFirstToSecond(FaceHandle<T> bottom)
		{
			throw new NotImplementedException();
		}

		public void GlueSecondToFirst(FaceHandle<T> bottom)
		{
			throw new NotImplementedException();
		}

		public void Flip()
		{
			throw new NotImplementedException();
		}

		public IReadOnlyCollection<IEdge<T>> GetEdges()
		{
			return edgeList.AsReadOnly();
		}

		private void SetFirstVertex(T vertex)
		{
			throw new NotImplementedException();
		}

		private void SetSecondVertex(T vertex)
		{
			throw new NotImplementedException();
		}

		private void PushFirst(IEdge<T> edge)
		{
			throw new NotImplementedException();
		}

		private void PushSecond(IEdge<T> edge)
		{
			throw new NotImplementedException();
		}
	}
}