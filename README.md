# GraphPlanarityTesting
[![NuGet](https://img.shields.io/nuget/v/GraphPlanarityTesting.svg)](https://www.nuget.org/packages/GraphPlanarityTesting)

C# implementation of the Boyer-Myrvold algorithm for planarity testing. Inspired by the implementaion from C++ Boost library. 

Even though the original algorithm works in *O(n)*, this implementation has worst-case time complexity *O(n<sup>2</sup>)*. It is caused by the fact that I use standard linked list instead of a more sophisticated data structure in one place of the algorihtm. The original paper also provides an algorithm for extracting Kuratowski subgraph if a given graph is not planar - this is not implemented in this library.

## Features
- Checks if a given undirected graph is planar or not
- Computes planar embedding if a given graph is planar
- Computes faces of a given planar embedding
- .NET Standard 2.0

## API

```csharp
public class BoyerMyrvold<T>
{
  /// <summary>
  /// Tries to get faces of a planar embedding of a given graph.
  /// </summary>
  /// <param name="graph">Graph</param>
  /// <param name="faces">Faces of a planar embedding if planar, null otherwise</param>
  /// <returns>True if planar, false otherwise</returns>
  public bool TryGetPlanarFaces(IGraph<T> graph, out PlanarFaces<T> faces);

  /// <summary>
  /// Checks if a given graph is planar.
  /// </summary>
  /// <param name="graph">Graph</param>
  /// <returns>True if planar, false otherwise</returns>
  public bool IsPlanar(IGraph<T> graph);

  /// <summary>
  /// Checks if a given graph is planar and provides a planar embedding if so.
  /// </summary>
  /// <param name="graph">Graph</param>
  /// <param name="embedding">Planar embedding if a given graph is planar, null otherwise</param>
  /// <returns>True if planar, false otherwise</returns>
  public bool IsPlanar(IGraph<T> graph, out PlanarEmbedding<T> embedding);
}
```

## Tests
The solution contains a project with basic functionality tests.

### `BoyerMyrvoldTests` class
- checks that if a graph is one big cycle, it is planar and there are exactly two faces in the embedding
- checks that if a random graph has more than *3n - 6* vertices, it is not planar

### `BoostComparisonTests` class
This class contains tests that check if the results of the algorithm are the same as in the Boost implementation. The repository contains contains 3 files with different random graphs together with the results from Boost.

- *10000_small_sparse_graphs.txt* - 10000 random graphs with 10-100 vertices and less than 3n edges.
- *5000_small_dense_graphs.txt* - 5000 random graphs with 10-100 vertices and any number of edges.
- *5000_large_sparse_graphs.txt* - 5000 random graphs with 100-1000 vertices and less than 3n edges.
