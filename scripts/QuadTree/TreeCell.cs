using System;
using Godot;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;

public class TreeCell
{
    public QuadTree.TreeBox boundingBox { get; private set; }
    private TreeCell parent = null; // we may not need this one
    private TreeCell[] children = new TreeCell[4];

    private List<int> elementsIndices = new();

    private EnemyManager unitsManager = null;

    public TreeCell(TreeCell _parent, QuadTree.TreeBox _boudingBox, EnemyManager _unitsManager)
    {
        boundingBox = _boudingBox;
        parent = _parent;
        unitsManager = _unitsManager;
    }

    public void SubmitElements(List<int> _indices)
    {
        foreach(int id in _indices)
            SubmitElement(id, unitsManager.GetPosition(id));
    }
    public bool SubmitElement(int _id, Vector2 _position)
    {
        if(Contains(_position) == false)
            return false; // This element is not for us (nor our children) to store

        if(children[0] == null)
        {
            // We don't have child cells, add to our list
            if(elementsIndices.Count == QuadTree.CELL_MAX_ELEMENTS)
            {
                // We don't have the room to add our element, split our elements in new children
                Split();
                return SubmitToChildren(_id, _position);
            }
            else
            {
                elementsIndices.Add(_id);
                return true;
            }
        }

        return SubmitToChildren(_id, _position);
    }

    public void CheckTrim()
    {
        if(children[0] == null)
            return; // No children to trim anyway

        for(int i = 0; i < 4; ++i)
            children[i].CheckTrim(); // Start trimming on each child

        // If after their own trimming all our chil are leafs, we can also trim
        for(int i = 0; i < 4; ++i)
        {
            if(children[i].children[0] != null)
            {
                // I any of our children has children, we can't trim
                return;
            }
        }

        // All children are leafs, check the sum of elements
        int elementsCount = 0;
        for(int i = 0; i < 4; ++i)
        {
            elementsCount += children[i].elementsIndices.Count;
            if(elementsCount > QuadTree.CELL_MIN_ELEMENTS)
                return; // Too many element for trimming
        }

        // If we reached this point we can trim
        for(int i = 0; i < 4; ++i)
        {
            children[i].elementsIndices.ForEach((id) => elementsIndices.Add(id));
            children[i] = null;
        }
    }

    public void GetElementsIn(QuadTree.TreeBox _box, List<int> _elements)
    {
        if(Intersects(_box) == false)
            return;

        if(children[0] == null)
        {
            _elements.AddRange(elementsIndices);
            return;
        }
        else
        {
            for(int i = 0; i < 4; ++i)
            {
                children[i].GetElementsIn(_box, _elements);
            }
        }
    }

    public void CheckDepartures(List<int> _removedIndices)
    {
        if(children[0] == null)
        {
            for(int i = elementsIndices.Count - 1; i >= 0; i--)
            {
                int id = elementsIndices[i];

                if(unitsManager.GetHealth(id) <= 0.0)
                {
                    elementsIndices.RemoveAt(i); // don't add to removed as it does not need to be reinserted
                    continue;
                }

                Vector2 pos = unitsManager.GetPosition(id);
                if(Contains(pos))
                    continue;

                elementsIndices.RemoveAt(i);
                _removedIndices.Add(id);
            }
        }
        else
        {
            for(int i = 0; i < 4; ++i)
                children[i].CheckDepartures(_removedIndices);
        }
    }

    public bool Contains(Vector2 _position)
    {
        return _position.X >= boundingBox.x                      // X not before the start of our box
        && _position.Y >= boundingBox.y                          // Y not before its start
        && _position.X <= boundingBox.x + boundingBox.w          // X not after its end
        && _position.Y <= boundingBox.y + boundingBox.h;         // Y not after its end
    }

    public bool Intersects(QuadTree.TreeBox _box)
    {
        return _box.x < boundingBox.x + boundingBox.w && _box.y < boundingBox.y + boundingBox.h // other box' start is before our end
            && _box.x + _box.w > boundingBox.x && _box.y + _box.h > boundingBox.y;              // other box's end is after our start
    }

    private bool SubmitToChildren(int _id, Vector2 _position)
    {
        for(int i = 0; i < 4; ++i)
        {
            if(children[i].SubmitElement(_id, _position))
                return true; // Get out once a child took the element
        }

        // Should not reach here
        GD.PrintErr("TreeCell.SubmitToChildren: No children took care of element " + _id + " at pos " + _position + " :: BBox: " + boundingBox);
        for(int i = 0; i < 4; ++i)
            GD.PrintErr("TreeCell.SubmitToChildren: childBBox" + children[i].boundingBox);

        return false;
    }

    private void Split()
    {
        // Splitting ourselft in 4 cells, A B C and D
        // A - B
        // C - D

        float w = boundingBox.w * 0.5f;
        float h = boundingBox.h * 0.5f;

        QuadTree.TreeBox aBox = new(boundingBox.x, boundingBox.y, w, h);
        QuadTree.TreeBox bBox = new(boundingBox.x + w, boundingBox.y, w, h);
        QuadTree.TreeBox cBox = new(boundingBox.x, boundingBox.y + h, w, h);
        QuadTree.TreeBox dBox = new(boundingBox.x + w, boundingBox.y + h, w, h);

        children[0] = new(this, aBox, unitsManager);
        children[1] = new(this, bBox, unitsManager);
        children[2] = new(this, cBox, unitsManager);
        children[3] = new(this, dBox, unitsManager);

        elementsIndices.ForEach((id) =>
        {
            Vector2 pos = unitsManager.GetPosition(id);
            for(int i = 0; i < 4; ++i)
            {
                if(children[i].SubmitElement(id, pos))
                    return; // Get out once a child took the element
            }

            GD.PrintErr("TreeCell.Split.ForEach: No children took care of element " + id);
        });

        elementsIndices.Clear();
    }

    public void DrawDebug(Vector2 _offset)
    {
        //  A - B
        //  |   |
        //  D - C 
        float height = 1.0f;
        Vector3 a = new(boundingBox.x + _offset.X, height, boundingBox.y + _offset.Y);
        Vector3 b = new(boundingBox.x + boundingBox.w + _offset.X, height, boundingBox.y + _offset.Y);
        Vector3 c = new(boundingBox.x + boundingBox.w + _offset.X, height, boundingBox.y + boundingBox.h + _offset.Y);
        Vector3 d = new(boundingBox.x + _offset.X, height, boundingBox.y + boundingBox.h + _offset.Y);

        DrawDebugManager.DebugDrawSquare(a, b, c, d);

        if(children[0] == null)
            return;

        for(int i = 0; i < 4; ++i)
            children[i].DrawDebug(_offset);
    }
}
