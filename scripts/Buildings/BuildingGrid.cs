using Godot;
using System;

public class BuildingGrid
{
	private int[] indices;
	private int w;
	private int h;

	private const int FREE_SPACE = -1;

	public void Initialize(int _width, int _height)
	{
		indices = new int[_width * _height];
		w = _width;
		h = _height;

		for(int i = 0; i < indices.Length; ++i)
			indices[i] = FREE_SPACE;
	}

	public void AddBuilding(int index, BoundingBoxI bbox)
	{
		for(int j = bbox.y; j < bbox.y + bbox.h; ++j)
		{
			for(int i = bbox.x; i < bbox.x + bbox.w; ++i)
			{
				indices[CoordToIndex(i, j)] = index;
			}
		}
	}

	public bool Available(BoundingBoxI bbox)
	{
		for(int j = bbox.y; j < bbox.y + bbox.h; ++j)
		{
			for(int i = bbox.x; i < bbox.x + bbox.w; ++i)
			{
				if(indices[CoordToIndex(i, j)] != FREE_SPACE)
					return false;
			}
		}
		return true;
	}

	public bool AddIfAvailable(int index, BoundingBoxI bbox)
	{
		if(Available(bbox))
		{
			AddBuilding(index, bbox);
			return true;
		}
		return false;
	}

	private Vector2I IndexToCoord(int _i)
	{
		int y = _i / w;
		int x = _i % w;
		return new(x, y);
	}

	private int CoordToIndex(Vector2I _xy) { return _xy.Y * w + _xy.X; }
	private int CoordToIndex(int _x, int _y) { return _y * w + _x; }
}

public class BoundingBoxI
{
	public int x = 0;
	public int y = 0;
	public int w = 0;
	public int h = 0;
	public BoundingBoxI() { }
	public BoundingBoxI(int _x, int _y, int _w, int _h)
	{
		x = _x;
		y = _y;
		w = _w;
		h = _h;
	}
}