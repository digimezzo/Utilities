using System;
using System.Drawing;

namespace Digimezzo.Utilities.ColorSpace
{
    // Ref from https://www.microsoft.com/msj/archive/s3f1a.htm
    public class Octree
    {
        #region DataStructure

        public class OctreeNode
        {
            internal OctreeNode()
            {
                this.children = new OctreeNode[8];
            }

            internal OctreeNode[] children;
            internal uint pixelCount;
            internal uint red;
            internal uint green;
            internal uint blue;

            public uint Red => IsLeaf ? red / pixelCount : throw new MemberAccessException("This node is a leaf node.");

            public uint Green => IsLeaf ? green / pixelCount : throw new MemberAccessException("This node is a leaf node.");

            public uint Blue => IsLeaf ? blue / pixelCount : throw new MemberAccessException("This node is a leaf node.");

            public bool IsLeaf { get; internal set; }

            public OctreeNode[] Children
            {
                get => IsLeaf ? throw new MemberAccessException("This node is a leaf node.") : children;
                internal set => children = value;
            }

            public OctreeNode Next { get; internal set; }
        }

        #endregion

        #region Variables

        private static uint COLOR_BITS;
        private static readonly byte[] MASK = { 0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01 };

        private readonly OctreeNode _root;
        private readonly OctreeNode[] _reducibleNode = new OctreeNode[9];
        private uint _leafCount;

        #endregion

        #region Public

        public static OctreeNode BuildOctree(string file, uint maxColors)
        {
            return BuildOctree(file, maxColors, 8);
        }

        // colorBits must set to 8 for RGB color and omit Alpha Channel
        private static OctreeNode BuildOctree(string file, uint maxColors, uint colorBits)
        {
            var img = new Bitmap(file);

            return BuildOctree(ref img, maxColors, colorBits);
        }

        public static OctreeNode BuildOctree(ref Bitmap image, uint maxColors)
        {
            return BuildOctree(ref image, maxColors, 8);
        }

        // colorBits must set to 8 for RGB color and omit Alpha Channel
        private static OctreeNode BuildOctree(ref Bitmap image, uint maxColors, uint colorBits)
        {
            return new Octree(ref image, maxColors, colorBits)._root;
        }

        #endregion

        #region Construction

        private Octree(ref Bitmap image, uint maxColors, uint colorBits)
        {
            COLOR_BITS = colorBits;

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    var color = image.GetPixel(x, y);
                    AddColor(ref _root, color.R, color.G, color.B, 0);

                    while (_leafCount > maxColors)
                        ReduceTree();
                }
            }
        }

        #endregion

        #region Private

        private void AddColor(ref OctreeNode node, byte r, byte g, byte b, int level)
        {
            if (node == null)
                node = CreateNode(level);

            if (node.IsLeaf)
            {
                node.pixelCount++;
                node.red += r;
                node.green += g;
                node.blue += b;
            }
            else
            {
                var shift = 7 - level;
                var nIndex = (((r & MASK[level]) >> shift) << 2) |
                             (((g & MASK[level]) >> shift) << 1) |
                             ((b & MASK[level]) >> shift);
                AddColor(ref node.Children[nIndex], r, g, b, level + 1);
            }
        }

        private OctreeNode CreateNode(int level)
        {
            var node = new OctreeNode { IsLeaf = (level == COLOR_BITS) };

            if (node.IsLeaf)
                _leafCount++;
            else
            {
                node.Next = _reducibleNode[level];
                _reducibleNode[level] = node;
            }

            return node;
        }

        private void ReduceTree()
        {
            uint i, children = 0;
            uint r = 0, g = 0, b = 0;

            // Find the deepest level containing at least one reducible node
            for (i = COLOR_BITS - 1; (i > 0) && (_reducibleNode[i] == null); i--) ;

            // Reduce the node most recently added to the list at level i
            var node = _reducibleNode[i];
            _reducibleNode[i] = node.Next;

            for (i = 0; i < COLOR_BITS; i++)
            {
                if (node.Children[i] == null) continue;
                r += node.Children[i].red;
                g += node.Children[i].green;
                b += node.Children[i].blue;
                node.pixelCount += node.Children[i].pixelCount;

                node.Children[i] = null;
                children++;
            }

            node.IsLeaf = true;
            node.red = r;
            node.green = g;
            node.blue = b;

            _leafCount -= (children - 1);
        }

        #endregion
    }
}
