using System;
using System.Collections.Generic;
using Avalonia;

namespace NodeEditor;

internal sealed class RTree<T>
{
    private const int DefaultMaxEntries = 16;
    private const int DefaultMinEntries = 8;

    internal sealed class Entry
    {
        public Rect Bounds;
        public T Item = default!;
        public Node? Parent;
        public Node? Child;
    }

    internal sealed class Node
    {
        public bool IsLeaf;
        public List<Entry> Entries;
        public Node? Parent;
        public Entry? ParentEntry;

        public Node(bool isLeaf, int capacity)
        {
            IsLeaf = isLeaf;
            Entries = new List<Entry>(capacity);
        }
    }

    private readonly int _maxEntries;
    private readonly int _minEntries;
    private Node? _root;

    public RTree(int maxEntries = DefaultMaxEntries, int minEntries = DefaultMinEntries)
    {
        if (maxEntries < 4)
        {
            maxEntries = 4;
        }

        if (minEntries < 2)
        {
            minEntries = 2;
        }

        if (minEntries > maxEntries / 2)
        {
            minEntries = maxEntries / 2;
        }

        _maxEntries = maxEntries;
        _minEntries = minEntries;
    }

    public void Clear()
    {
        _root = null;
    }

    public Entry Insert(T item, Rect bounds)
    {
        var entry = new Entry { Item = item, Bounds = bounds };
        Insert(entry);
        return entry;
    }

    public void Insert(Entry entry)
    {
        if (_root is null)
        {
            _root = new Node(isLeaf: true, _maxEntries);
            AddEntry(_root, entry);
            return;
        }

        var leaf = ChooseLeaf(_root, entry.Bounds);
        AddEntry(leaf, entry);

        if (leaf.Entries.Count > _maxEntries)
        {
            var split = SplitNode(leaf);
            AdjustTree(leaf, split);
        }
        else
        {
            AdjustTree(leaf, null);
        }
    }

    public void Update(Entry entry, Rect bounds)
    {
        Remove(entry);
        entry.Bounds = bounds;
        Insert(entry);
    }

    public void Remove(Entry entry)
    {
        var leaf = entry.Parent;
        if (leaf is null)
        {
            return;
        }

        if (!leaf.Entries.Remove(entry))
        {
            return;
        }

        entry.Parent = null;

        var reinserts = new List<Entry>();
        CondenseTree(leaf, reinserts);

        if (_root is { IsLeaf: false } && _root.Entries.Count == 1)
        {
            var child = _root.Entries[0].Child;
            if (child is not null)
            {
                child.Parent = null;
                child.ParentEntry = null;
                _root = child;
            }
        }

        if (_root is { Entries.Count: 0 })
        {
            _root = null;
        }

        foreach (var reinsert in reinserts)
        {
            Insert(reinsert);
        }
    }

    public IEnumerable<Entry> Search(Rect rect)
    {
        if (_root is null)
        {
            yield break;
        }

        var stack = new Stack<Node>();
        stack.Push(_root);

        while (stack.Count > 0)
        {
            var node = stack.Pop();
            foreach (var entry in node.Entries)
            {
                if (!rect.Intersects(entry.Bounds))
                {
                    continue;
                }

                if (node.IsLeaf)
                {
                    yield return entry;
                }
                else if (entry.Child is not null)
                {
                    stack.Push(entry.Child);
                }
            }
        }
    }

    private static void AddEntry(Node node, Entry entry)
    {
        node.Entries.Add(entry);
        entry.Parent = node;

        if (entry.Child is not null)
        {
            entry.Child.Parent = node;
            entry.Child.ParentEntry = entry;
        }
    }

    private Node ChooseLeaf(Node node, Rect bounds)
    {
        if (node.IsLeaf)
        {
            return node;
        }

        Entry? best = null;
        var bestEnlargement = double.PositiveInfinity;
        var bestArea = double.PositiveInfinity;

        foreach (var entry in node.Entries)
        {
            var area = Area(entry.Bounds);
            var combined = Union(entry.Bounds, bounds);
            var enlargement = Area(combined) - area;

            if (enlargement < bestEnlargement)
            {
                bestEnlargement = enlargement;
                bestArea = area;
                best = entry;
                continue;
            }

            if (Math.Abs(enlargement - bestEnlargement) < 0.0001 && area < bestArea)
            {
                bestArea = area;
                best = entry;
            }
        }

        if (best?.Child is null)
        {
            return node;
        }

        return ChooseLeaf(best.Child, bounds);
    }

    private void AdjustTree(Node node, Node? splitNode)
    {
        var current = node;
        var newNode = splitNode;

        while (true)
        {
            if (current.Parent is null)
            {
                if (newNode is not null)
                {
                    var newRoot = new Node(isLeaf: false, _maxEntries);
                    var entry1 = new Entry { Bounds = ComputeBounds(current), Child = current };
                    var entry2 = new Entry { Bounds = ComputeBounds(newNode), Child = newNode };
                    AddEntry(newRoot, entry1);
                    AddEntry(newRoot, entry2);
                    _root = newRoot;
                }
                else
                {
                    UpdateParentEntryBounds(current);
                }

                return;
            }

            UpdateParentEntryBounds(current);

            if (newNode is not null)
            {
                var parent = current.Parent;
                if (parent is not null)
                {
                    var entry = new Entry { Bounds = ComputeBounds(newNode), Child = newNode };
                    AddEntry(parent, entry);

                    if (parent.Entries.Count > _maxEntries)
                    {
                        var split = SplitNode(parent);
                        current = parent;
                        newNode = split;
                        continue;
                    }
                }

                newNode = null;
            }

            current = current.Parent;
        }
    }

    private void CondenseTree(Node node, List<Entry> reinserts)
    {
        var current = node;

        while (current.Parent is not null)
        {
            if (current.Entries.Count < _minEntries)
            {
                var parent = current.Parent;
                var parentEntry = current.ParentEntry;

                if (parent is not null && parentEntry is not null)
                {
                    parent.Entries.Remove(parentEntry);
                }

                CollectLeafEntries(current, reinserts);

                current.Parent = null;
                current.ParentEntry = null;

                current = parent ?? current;
                continue;
            }

            UpdateParentEntryBounds(current);
            current = current.Parent;
        }

        if (_root == current)
        {
            UpdateParentEntryBounds(current);
        }
    }

    private void CollectLeafEntries(Node node, List<Entry> reinserts)
    {
        if (node.IsLeaf)
        {
            foreach (var entry in node.Entries)
            {
                entry.Parent = null;
                reinserts.Add(entry);
            }

            node.Entries.Clear();
            return;
        }

        foreach (var entry in node.Entries)
        {
            if (entry.Child is null)
            {
                continue;
            }

            CollectLeafEntries(entry.Child, reinserts);
        }

        node.Entries.Clear();
    }

    private Node SplitNode(Node node)
    {
        var allEntries = new List<Entry>(node.Entries);
        node.Entries.Clear();

        var newNode = new Node(node.IsLeaf, _maxEntries);

        PickSeeds(allEntries, out var seed1, out var seed2);
        AddEntry(node, seed1);
        AddEntry(newNode, seed2);
        allEntries.Remove(seed1);
        allEntries.Remove(seed2);

        var bounds1 = seed1.Bounds;
        var bounds2 = seed2.Bounds;

        while (allEntries.Count > 0)
        {
            if (node.Entries.Count + allEntries.Count == _minEntries)
            {
                foreach (var entry in allEntries)
                {
                    AddEntry(node, entry);
                    bounds1 = Union(bounds1, entry.Bounds);
                }

                allEntries.Clear();
                break;
            }

            if (newNode.Entries.Count + allEntries.Count == _minEntries)
            {
                foreach (var entry in allEntries)
                {
                    AddEntry(newNode, entry);
                    bounds2 = Union(bounds2, entry.Bounds);
                }

                allEntries.Clear();
                break;
            }

            Entry? next = null;
            var maxDiff = double.NegativeInfinity;
            var nextEnlargement1 = 0.0;
            var nextEnlargement2 = 0.0;
            Rect nextUnion1 = default;
            Rect nextUnion2 = default;

            foreach (var entry in allEntries)
            {
                var union1 = Union(bounds1, entry.Bounds);
                var union2 = Union(bounds2, entry.Bounds);
                var enlargement1 = Area(union1) - Area(bounds1);
                var enlargement2 = Area(union2) - Area(bounds2);
                var diff = Math.Abs(enlargement1 - enlargement2);

                if (diff > maxDiff)
                {
                    maxDiff = diff;
                    next = entry;
                    nextEnlargement1 = enlargement1;
                    nextEnlargement2 = enlargement2;
                    nextUnion1 = union1;
                    nextUnion2 = union2;
                }
            }

            if (next is null)
            {
                break;
            }

            allEntries.Remove(next);

            if (nextEnlargement1 < nextEnlargement2)
            {
                AddEntry(node, next);
                bounds1 = nextUnion1;
                continue;
            }

            if (nextEnlargement2 < nextEnlargement1)
            {
                AddEntry(newNode, next);
                bounds2 = nextUnion2;
                continue;
            }

            var area1 = Area(bounds1);
            var area2 = Area(bounds2);

            if (area1 < area2)
            {
                AddEntry(node, next);
                bounds1 = nextUnion1;
                continue;
            }

            if (area2 < area1)
            {
                AddEntry(newNode, next);
                bounds2 = nextUnion2;
                continue;
            }

            if (node.Entries.Count <= newNode.Entries.Count)
            {
                AddEntry(node, next);
                bounds1 = nextUnion1;
            }
            else
            {
                AddEntry(newNode, next);
                bounds2 = nextUnion2;
            }
        }

        return newNode;
    }

    private static void PickSeeds(List<Entry> entries, out Entry seed1, out Entry seed2)
    {
        seed1 = entries[0];
        seed2 = entries[1];
        var maxWaste = double.NegativeInfinity;

        for (var i = 0; i < entries.Count - 1; i++)
        {
            for (var j = i + 1; j < entries.Count; j++)
            {
                var a = entries[i];
                var b = entries[j];
                var union = Union(a.Bounds, b.Bounds);
                var waste = Area(union) - Area(a.Bounds) - Area(b.Bounds);
                if (waste > maxWaste)
                {
                    maxWaste = waste;
                    seed1 = a;
                    seed2 = b;
                }
            }
        }
    }

    private static void UpdateParentEntryBounds(Node node)
    {
        if (node.ParentEntry is null)
        {
            return;
        }

        node.ParentEntry.Bounds = ComputeBounds(node);
    }

    private static Rect ComputeBounds(Node node)
    {
        var hasBounds = false;
        var bounds = default(Rect);

        foreach (var entry in node.Entries)
        {
            if (!hasBounds)
            {
                bounds = entry.Bounds;
                hasBounds = true;
                continue;
            }

            bounds = Union(bounds, entry.Bounds);
        }

        return bounds;
    }

    private static Rect Union(Rect a, Rect b)
    {
        var left = Math.Min(a.Left, b.Left);
        var top = Math.Min(a.Top, b.Top);
        var right = Math.Max(a.Right, b.Right);
        var bottom = Math.Max(a.Bottom, b.Bottom);
        return new Rect(new Point(left, top), new Point(right, bottom));
    }

    private static double Area(Rect rect)
    {
        var width = Math.Max(0.0, rect.Width);
        var height = Math.Max(0.0, rect.Height);
        return width * height;
    }
}
