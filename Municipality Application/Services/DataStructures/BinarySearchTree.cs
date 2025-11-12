namespace Municipality_Application.Services.DataStructures
{
    /// <summary>
    /// Represents a generic binary search tree (BST).
    /// </summary>
    /// <typeparam name="T">The type of elements stored, must implement IComparable&lt;T&gt;.</typeparam>
    public class BinarySearchTree<T> where T : IComparable<T>
    {
        /// <summary>
        /// Represents a node in the binary search tree.
        /// </summary>
        public class Node
        {
            public T Value { get; set; }
            public Node? Left { get; set; }
            public Node? Right { get; set; }

            public Node(T value)
            {
                Value = value;
            }
        }

        /// <summary>
        /// The root node of the BST.
        /// </summary>
        public Node? Root { get; private set; }

        /// <summary>
        /// Inserts a value into the BST.
        /// </summary>
        public void Insert(T value)
        {
            Root = Insert(Root, value);
        }

        private Node Insert(Node? node, T value)
        {
            if (node == null)
                return new Node(value);

            int cmp = value.CompareTo(node.Value);
            if (cmp < 0)
                node.Left = Insert(node.Left, value);
            else if (cmp > 0)
                node.Right = Insert(node.Right, value);
            // Ignore duplicates
            return node;
        }

        /// <summary>
        /// Finds a value in the BST.
        /// </summary>
        public bool Find(T value)
        {
            return Find(Root, value);
        }

        private bool Find(Node? node, T value)
        {
            if (node == null)
                return false;
            int cmp = value.CompareTo(node.Value);
            if (cmp == 0)
                return true;
            if (cmp < 0)
                return Find(node.Left, value);
            return Find(node.Right, value);
        }

        /// <summary>
        /// Performs an in-order traversal of the BST.
        /// </summary>
        public IEnumerable<T> InOrderTraversal()
        {
            return InOrderTraversal(Root);
        }

        private IEnumerable<T> InOrderTraversal(Node? node)
        {
            if (node != null)
            {
                foreach (var v in InOrderTraversal(node.Left))
                    yield return v;
                yield return node.Value;
                foreach (var v in InOrderTraversal(node.Right))
                    yield return v;
            }
        }
    }
}