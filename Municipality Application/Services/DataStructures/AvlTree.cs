namespace Municipality_Application.Services.DataStructures
{
    /// <summary>
    /// Represents a generic self-balancing AVL tree.
    /// </summary>
    /// <typeparam name="T">The type of elements stored, must implement IComparable&lt;T&gt;.</typeparam>
    public class AvlTree<T> where T : IComparable<T>
    {
        /// <summary>
        /// Represents a node in the AVL tree.
        /// </summary>
        public class Node
        {
            public T Value { get; set; }
            public Node? Left { get; set; }
            public Node? Right { get; set; }
            public int Height { get; set; }

            public Node(T value)
            {
                Value = value;
                Height = 1;
            }
        }

        /// <summary>
        /// The root node of the AVL tree.
        /// </summary>
        public Node? Root { get; private set; }

        /// <summary>
        /// Inserts a value into the AVL tree.
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
            else
                return node; // Ignore duplicates

            node.Height = 1 + Math.Max(GetHeight(node.Left), GetHeight(node.Right));
            return Balance(node);
        }

        /// <summary>
        /// Finds a value in the AVL tree.
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
        /// Balances the AVL tree at the given node.
        /// </summary>
        private Node Balance(Node node)
        {
            int balance = GetBalance(node);

            // Left heavy
            if (balance > 1)
            {
                if (GetBalance(node.Left) < 0)
                    node.Left = RotateLeft(node.Left!);
                return RotateRight(node);
            }
            // Right heavy
            if (balance < -1)
            {
                if (GetBalance(node.Right) > 0)
                    node.Right = RotateRight(node.Right!);
                return RotateLeft(node);
            }
            return node;
        }

        private int GetHeight(Node? node) => node?.Height ?? 0;

        private int GetBalance(Node? node) => node == null ? 0 : GetHeight(node.Left) - GetHeight(node.Right);

        private Node RotateRight(Node y)
        {
            var x = y.Left!;
            var T2 = x.Right;

            x.Right = y;
            y.Left = T2;

            y.Height = 1 + Math.Max(GetHeight(y.Left), GetHeight(y.Right));
            x.Height = 1 + Math.Max(GetHeight(x.Left), GetHeight(x.Right));

            return x;
        }

        private Node RotateLeft(Node x)
        {
            var y = x.Right!;
            var T2 = y.Left;

            y.Left = x;
            x.Right = T2;

            x.Height = 1 + Math.Max(GetHeight(x.Left), GetHeight(x.Right));
            y.Height = 1 + Math.Max(GetHeight(y.Left), GetHeight(y.Right));

            return y;
        }
    }
}