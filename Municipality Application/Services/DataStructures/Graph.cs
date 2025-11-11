namespace Municipality_Application.Services.DataStructures
{
    /// <summary>
    /// Represents a generic undirected graph using an adjacency list.
    /// </summary>
    /// <typeparam name="T">The type of vertices.</typeparam>
    public class Graph<T> where T : notnull
    {
        private readonly Dictionary<T, List<(T, double)>> _adjacencyList = new();

        /// <summary>
        /// Adds a vertex to the graph.
        /// </summary>
        public void AddVertex(T vertex)
        {
            if (!_adjacencyList.ContainsKey(vertex))
                _adjacencyList[vertex] = new List<(T, double)>();
        }

        /// <summary>
        /// Adds an undirected edge between two vertices with an optional weight.
        /// </summary>
        public void AddEdge(T from, T to, double weight = 1.0)
        {
            AddVertex(from);
            AddVertex(to);
            _adjacencyList[from].Add((to, weight));
            _adjacencyList[to].Add((from, weight));
        }

        /// <summary>
        /// Performs a breadth-first search (BFS) starting from the given vertex.
        /// </summary>
        public IEnumerable<T> Bfs(T start)
        {
            var visited = new HashSet<T>();
            var queue = new Queue<T>();
            queue.Enqueue(start);
            visited.Add(start);

            while (queue.Count > 0)
            {
                var vertex = queue.Dequeue();
                yield return vertex;

                foreach (var (neighbor, _) in _adjacencyList[vertex])
                {
                    if (visited.Add(neighbor))
                        queue.Enqueue(neighbor);
                }
            }
        }

        /// <summary>
        /// Performs a depth-first search (DFS) starting from the given vertex.
        /// </summary>
        public IEnumerable<T> Dfs(T start)
        {
            var visited = new HashSet<T>();
            var stack = new Stack<T>();
            stack.Push(start);

            while (stack.Count > 0)
            {
                var vertex = stack.Pop();
                if (visited.Add(vertex))
                {
                    yield return vertex;
                    foreach (var (neighbor, _) in _adjacencyList[vertex].AsEnumerable().Reverse())
                    {
                        if (!visited.Contains(neighbor))
                            stack.Push(neighbor);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the minimum spanning tree (MST) using Prim's algorithm.
        /// </summary>
        public List<(T from, T to, double weight)> GetMinimumSpanningTree()
        {
            var result = new List<(T, T, double)>();
            if (_adjacencyList.Count == 0)
                return result;

            var visited = new HashSet<T>();
            var minHeap = new SortedSet<(double, T, T)>(Comparer<(double, T, T)>.Create((a, b) =>
            {
                int cmp = a.Item1.CompareTo(b.Item1);
                if (cmp != 0) return cmp;
                cmp = Comparer<T>.Default.Compare(a.Item2, b.Item2);
                if (cmp != 0) return cmp;
                return Comparer<T>.Default.Compare(a.Item3, b.Item3);
            }));

            var start = _adjacencyList.Keys.First();
            visited.Add(start);

            foreach (var (neighbor, weight) in _adjacencyList[start])
                minHeap.Add((weight, start, neighbor));

            while (minHeap.Count > 0)
            {
                var (weight, from, to) = minHeap.Min;
                minHeap.Remove(minHeap.Min);

                if (visited.Contains(to))
                    continue;

                result.Add((from, to, weight));
                visited.Add(to);

                foreach (var (neighbor, w) in _adjacencyList[to])
                {
                    if (!visited.Contains(neighbor))
                        minHeap.Add((w, to, neighbor));
                }
            }

            return result;
        }
    }
}