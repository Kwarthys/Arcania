using System;
using Godot;
using System.Collections.Generic;
using System.Reflection.Metadata;

public class QuadTree
{
    public TreeCell root { get; private set; }
    public const int CELL_MAX_ELEMENTS = 16;
    public const int CELL_MIN_ELEMENTS = 8;

    public struct TreeBox
    {
        public float x;
        public float y;
        public float w;
        public float h;

        public TreeBox(float _x, float _y, float _w, float _h)
        {
            x = _x;
            y = _y;
            w = _w;
            h = _h;
        }

        public TreeBox(float _xCenter, float _yCenter, float _radius)
        {
            x = _xCenter - _radius;
            y = _yCenter - _radius;
            w = 2.0f * _radius;
            h = w;
        }

        public override string ToString()
        {
            return x + ", " + y + " -> " + (x + w) + ", " + (y + h);
        }

    }

    public QuadTree(TreeBox _dimensions, EnemyManager _enemyManager)
    {
        root = new(null, _dimensions, _enemyManager);
    }

    public void SubmitElement(int _id, Vector2 _position) { root.SubmitElement(_id, _position); }
    public void SubmitElements(List<int> _indices) { root.SubmitElements(_indices); }
    public void CheckDepartures(List<int> _removedIndices) { root.CheckDepartures(_removedIndices); }
    public void CheckTrim() { root.CheckTrim(); }
    public void DrawDebug(Vector2 _offset) { root.DrawDebug(_offset); }

    public List<int> GetElementsIn(TreeBox _box)
    {
        List<int> elements = new();
        root.GetElementsIn(_box, elements);
        return elements;
    }


}
