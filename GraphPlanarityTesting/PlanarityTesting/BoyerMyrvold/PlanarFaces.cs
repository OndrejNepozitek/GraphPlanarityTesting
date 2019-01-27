namespace GraphPlanarityTesting.PlanarityTesting.BoyerMyrvold
{
	using System.Collections.Generic;

	public class PlanarFaces<T>
	{
		public List<List<T>> Faces { get; }

		public PlanarFaces(List<List<T>> faces)
		{
			Faces = faces;
		}
	}
}