using Godot;
using System;
using System.Collections.Generic;
using System.Data;

public class ConstructionQueue
{
    private BuildingsManager manager = null;

    public ConstructionQueue(BuildingsManager _buildingsManager)
    {
        manager = _buildingsManager;
    }

    public struct BuildCommand
    {
        public BuildCommand(Vector2I _gridPos, string _buildingName, Node3D _ghost)
        {
            gridPos = _gridPos;
            buildingName = _buildingName;
            ghost = _ghost;
        }
        public Vector2I gridPos;
        public string buildingName;
        public Node3D ghost;
    }

    private Queue<BuildCommand> queue = new();
    public int GetSize() { return queue.Count; }

    public void Advance()
    {
        if(queue.Count > 0)
        {
            manager.OnConstructionQueueAdvance(queue.Dequeue(), this);
        }
    }

    public void AddItem(Vector2I _gridPos, string _buildingName, Node3D _ghost)
    {
        queue.Enqueue(new BuildCommand(_gridPos, _buildingName, _ghost));
    }
}
