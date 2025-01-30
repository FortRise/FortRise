using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;


namespace FortRise;

/// <summary>
/// An asset utility used to pack rectangles in very efficient manner. Used for creating runtime <see cref="Monocle.Atlas"/> 
/// </summary>
public class TexturePacker<T>
{
    private struct Score(Rectangle rect, int width, int height)
    {
        public int S1 = (rect.Width * rect.Height) - width * height;
        public int S2 = Math.Min(rect.Width - width, rect.Height - height);
        public static Score Worst => new Score { S1 = int.MaxValue, S2 = int.MaxValue };

        public bool IsBetterThan(in Score score) 
        {
            return S1 < score.S1 || (S1 == score.S1 && S2 < score.S2);
        }
    }
    private struct Node(int x, int y, int w, int h) 
    {
        public Rectangle Rect = new Rectangle(x, y, w, h);
        public bool IsSplit;
        public int Left;
        public int Right;
        public int Top;
        public int Bottom;

        public int X => Rect.X;
        public int Y => Rect.Y;
        public int W => Rect.Width;
        public int H => Rect.Height;

        public int this[int index] 
        {
            get 
            {
                return index switch 
                {
                    0 => Left,
                    1 => Right,
                    2 => Top,
                    3 => Bottom,
                    _ => throw new IndexOutOfRangeException()
                };
            }
        }
    }

    /// <summary>
    /// A struct used to contain the data, and the width and the height of a rectangle.
    /// </summary>
    public struct Item(T data, int width, int height)
    {
        public T Data = data;
        public int Width = width;
        public int Height = height;

        public uint GetTotalSize() 
        {
            int area = Width * Height;
            int largestArea = Math.Max(Width, Height);
            return (uint)(area + largestArea);
        }
    }

    /// <summary>
    /// A struct that have been outputs after packing.
    /// </summary>
    public struct PackedItem(Rectangle rect, T data)
    {
        /// <summary>
        /// A rectangle with its position offset by a packer.
        /// </summary>
        public Rectangle Rect = rect;
        public T Data = data;
    }

    private List<Item> items = new List<Item>();
    private List<Node> nodes = new List<Node>();

    /// <summary>
    /// The maximum area size before it bails out.
    /// </summary>
    public int MaxSize { get; set; }

    /// <summary>
    /// Construct a <see cref="FortRise.TexturePacker{T}"/>.
    /// </summary>
    /// <param name="maxSize">A maximum area size before it bails out</param>
    public TexturePacker(int maxSize = 4096) 
    {
        MaxSize = maxSize;
    }

    /// <summary>
    /// Adds an <see cref="FortRise.TexturePacker{T}.Item"/> to pack.
    /// </summary>
    /// <param name="item">An <see cref="FortRise.TexturePacker{T}.Item"/> to add</param>
    public void Add(Item item) 
    {
        items.Add(item);
    }

    /// <summary>
    /// Pack all of the items and outputs all the results of a packed item. 
    /// This will also resets the <see cref="FortRise.TexturePacker{T}"/> state to be reused once again.
    /// </summary>
    /// <param name="packedItems">An output list of <see cref="FortRise.TexturePacker{T}.PackedItem"/></param>
    /// <param name="size">An output size</param>
    /// <returns>Returns true if succeed packing</returns>
    public bool Pack(out List<PackedItem> packedItems, out Point size) 
    {
        packedItems = new List<PackedItem>();
        if (items.Count == 0) 
        {
            size = Point.Zero;
            return false;
        }

        if (items.Count == 1) 
        {
            Item item = items[0];
            packedItems.Add(new PackedItem(new Rectangle(0, 0, item.Width, item.Height), item.Data));
            nodes.Add(new Node(0, 0, item.Width, item.Height));
            goto DONE;
        }

        items.Sort((x, y) => {
            uint a = x.GetTotalSize();
            uint b = y.GetTotalSize();
            
            return a < b ? 1 : a > b ? -1 : 0;
        });

        int sum = 0;
        for (int i = 0; i < items.Count; i++) 
        {
            var item = items[i];
            sum += item.Width * item.Height;
        }

        var pageSize = 2;
        while (pageSize * pageSize * 2 < sum) 
        {
            pageSize *= 2;
        }

        while (pageSize <= MaxSize) 
        {
            Span<Point> sizes = [
                new Point(pageSize, pageSize), 
                new Point(pageSize * 2, pageSize), 
                new Point(pageSize, pageSize * 2)
            ];

            for (int tries = 0; tries < sizes.Length; tries++)
            {
                var pointSize = sizes[tries];
                if (pointSize.X > MaxSize
                    || pointSize.Y > MaxSize
                    || pointSize.X * pointSize.Y < sum)
                {
                    continue;
                }
                nodes.Clear();
                nodes.Add(new Node(0, 0, pointSize.X, pointSize.Y));

                bool requiredRetry = false;
                for (int i = 0; i < items.Count; i++)
                {
                    Item item = items[i];

                    var nodeID = FindNode(0, item.Width, item.Height, out Score score);

                    if (nodeID == -1)
                    {
                        size = Point.Zero;
                        requiredRetry = true;
                        break;
                    }

                    var node = nodes[nodeID];

                    Rectangle rect = new Rectangle(node.X, node.Y, item.Width, item.Height);
                    SplitNode(0, rect);

                    packedItems.Add(new PackedItem(rect, item.Data));
                }
                if (!requiredRetry) 
                {
                    goto DONE;
                }
            }

            pageSize *= 2;
        }

        Logger.Error($"Max Size limit has reached: {pageSize}/{MaxSize}");
        size = new Point(pageSize, pageSize);

        nodes.Clear();
        items.Clear();
        return false;

        DONE:
        var root = nodes[0];

        var pageWidth = root.W;
        var pageHeight = root.H;
        
        size = new Point(pageWidth, pageHeight);

        // clean things up

        nodes.Clear();
        items.Clear();

        return true;
    }

    private bool HasRect(int nodeID, Rectangle rect)
    {
        var node = nodes[nodeID];
        if (!node.Rect.Contains(rect))
        {
            return false;
        }
        if (!node.IsSplit)
        {
            return true;
        }
        for (int i = 0; i < 4; i++)
        {
            int id = node[i];
            if (id > 0 && HasRect(id, rect))
            {
                return true;
            }
        }
        return false;
    }

    private void SplitNode(int nodeID, in Rectangle rect)
    {
        var node = nodes[nodeID];

        if (!node.Rect.Intersects(rect))
        {
            return;
        }
        if (node.IsSplit)
        {
            for (int i = 0; i < 4; i++) 
            {
                int id = node[i];
                if (id > 0) 
                {
                    SplitNode(id, in rect);
                }
            }
            return;
        }

        node.IsSplit = true;
        nodes[nodeID] = node;
        Rectangle newRect;
        if (rect.X > node.X && !HasRect(0, newRect = new Rectangle(node.X, node.Y, rect.X - node.X, node.H)))
        {
            node.Left = nodes.Count;
            AddNewNode(newRect);
        }
        if (rect.Right < node.Rect.Right && !HasRect(0, newRect = new Rectangle(rect.Right, node.Y, node.Rect.Right - rect.Right, node.H)))
        {
            node.Right = nodes.Count;
            AddNewNode(newRect);
        }
        if (rect.Y > node.Y && !HasRect(0, newRect = new Rectangle(node.X, node.Y, node.W, rect.Y - node.Y)))
        {
            node.Top = nodes.Count;
            AddNewNode(newRect);
        }
        if (rect.Bottom < node.Rect.Bottom && !HasRect(0, newRect = new Rectangle(node.X, rect.Bottom, node.W, node.Rect.Bottom - rect.Bottom)))
        {
            node.Bottom = nodes.Count;
            AddNewNode(newRect);
        }
        nodes[nodeID] = node;


        void AddNewNode(in Rectangle rect)
        {
            nodes.Add(new Node(rect.X, rect.Y, rect.Width, rect.Height));
        }
    }

    private int FindNode(int nodeID, int w, int h, out Score score)
    {
        Node node = nodes[nodeID];

        if (w > node.W || h > node.H)
        {
            score = Score.Worst;
            return -1;
        }

        if (!node.IsSplit)
        {
            score = new Score(node.Rect, w, h);
            return nodeID;
        }

        int contestingNode = -1;
        Score contestingScore = Score.Worst;
        for (int i = 0; i < 4; i++)
        {
            var n = node[i];
            if (n <= 0)
            {
                continue;
            }
            var otherN = FindNode(n, w, h, out Score otherScore);
            if (otherScore.IsBetterThan(contestingScore))
            {
                contestingScore = otherScore;
                contestingNode = otherN;
            }
        }

        score = contestingScore;
        return contestingNode;
    }
}