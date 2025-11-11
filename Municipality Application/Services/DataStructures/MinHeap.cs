namespace Municipality_Application.Services.DataStructures
{
    /// <summary>
    /// Represents a generic min-heap data structure.
    /// </summary>
    /// <typeparam name="T">The type of elements stored, must implement IComparable&lt;T&gt;.</typeparam>
    public class MinHeap<T> where T : IComparable<T>
    {
        private readonly List<T> _elements = new();

        /// <summary>
        /// Gets the number of elements in the heap.
        /// </summary>
        public int Count => _elements.Count;

        /// <summary>
        /// Inserts a value into the min-heap.
        /// </summary>
        public void Insert(T value)
        {
            _elements.Add(value);
            HeapifyUp(_elements.Count - 1);
        }

        /// <summary>
        /// Returns the minimum element without removing it.
        /// </summary>
        public T Peek()
        {
            if (_elements.Count == 0)
                throw new InvalidOperationException("Heap is empty.");
            return _elements[0];
        }

        /// <summary>
        /// Extracts and returns the minimum element from the heap.
        /// </summary>
        public T ExtractMin()
        {
            if (_elements.Count == 0)
                throw new InvalidOperationException("Heap is empty.");
            T min = _elements[0];
            _elements[0] = _elements[^1];
            _elements.RemoveAt(_elements.Count - 1);
            HeapifyDown(0);
            return min;
        }

        private void HeapifyUp(int index)
        {
            while (index > 0)
            {
                int parent = (index - 1) / 2;
                if (_elements[index].CompareTo(_elements[parent]) >= 0)
                    break;
                (_elements[index], _elements[parent]) = (_elements[parent], _elements[index]);
                index = parent;
            }
        }

        private void HeapifyDown(int index)
        {
            int lastIndex = _elements.Count - 1;
            while (true)
            {
                int left = 2 * index + 1;
                int right = 2 * index + 2;
                int smallest = index;

                if (left <= lastIndex && _elements[left].CompareTo(_elements[smallest]) < 0)
                    smallest = left;
                if (right <= lastIndex && _elements[right].CompareTo(_elements[smallest]) < 0)
                    smallest = right;

                if (smallest == index)
                    break;

                (_elements[index], _elements[smallest]) = (_elements[smallest], _elements[index]);
                index = smallest;
            }
        }
    }
}