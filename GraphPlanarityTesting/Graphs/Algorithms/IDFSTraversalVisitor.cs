namespace GraphPlanarityTesting.Graphs.Algorithms
{
	using DataStructures;

	public interface IDFSTraversalVisitor<T>
	{
		void InitializeVertex(T vertex, IGraph<T> graph);

		void StartVertex(T vertex, IGraph<T> graph);

		void DiscoverVertex(T vertex, IGraph<T> graph);

		void ExamineEdge(IEdge<T> edge, IGraph<T> graph);

		void TreeEdge(IEdge<T> edge, IGraph<T> graph);

		void BackEdge(IEdge<T> edge, IGraph<T> graph);

		void FinishEdge(IEdge<T> edge, IGraph<T> graph);

		void FinishVertex(T vertex, IGraph<T> graph);
	}
}