namespace GraphPlanarityTesting.PlanarityTesting.BoyerMyrvold
{
	using System.Collections.Generic;

	public class BucketSort
	{
		public static T[] Sort<T>(Dictionary<T, int> elements, int bucketsCount)
		{
			var buckets = new List<T>[bucketsCount];

			for (int i = 0; i < bucketsCount; i++)
			{
				buckets[i] = new List<T>();
			}

			foreach (var element in elements)
			{
				buckets[element.Value].Add(element.Key);
			}

			var result = new List<T>();

			foreach (var bucket in buckets)
			{
				foreach (var element in bucket)
				{
					result.Add(element);
				}
			}

			return result.ToArray();
		}
	}
}