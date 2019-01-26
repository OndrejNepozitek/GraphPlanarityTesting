namespace GraphPlanarityTesting.PlanarityTesting.PlanarFaceTraversal
{
	using Graphs.DataStructures;

	public interface IPlanarFaceTraversalVisitor<T>
	{
		void BeginTraversal();

		void BeginFace();

		void NextEdge(IEdge<T> edge);

		void NextVertex(T vertex);

		void EndFace();

		void EndTraversal();
	}
}