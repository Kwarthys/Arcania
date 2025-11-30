using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class QuadTreeDebugManager : Node
{
	public static QuadTreeDebugManager Instance;
	[Export] private StandardMaterial3D debugMaterial;
	public override void _Ready()
	{
		Instance = this;
	}

	private int surfacePerMeshCounter = 0;
	private int currentMeshIndex = 0;
	private List<ImmediateMesh> debugMeshes = new();

	public static void DebugDrawSquare(Vector3 _a, Vector3 _b, Vector3 _c, Vector3 _d)
	{
		Instance?.DrawSquare(_a, _b, _c, _d);
	}

	public void DrawSquare(Vector3 _a, Vector3 _b, Vector3 _c, Vector3 _d)
	{
		if(surfacePerMeshCounter + 1 >= 256) // MAX_MESH_SURFACE = 256
		{
			currentMeshIndex++;
			surfacePerMeshCounter = 0;
		}

		ImmediateMesh mesh;

		if(currentMeshIndex >= debugMeshes.Count)
			mesh = InstantiateNewMesh();
		else
			mesh = debugMeshes[currentMeshIndex];

		mesh.SurfaceBegin(Mesh.PrimitiveType.Lines, debugMaterial);

		mesh.SurfaceAddVertex(_a);
		mesh.SurfaceAddVertex(_b);

		mesh.SurfaceAddVertex(_b);
		mesh.SurfaceAddVertex(_c);

		mesh.SurfaceAddVertex(_c);
		mesh.SurfaceAddVertex(_d);

		mesh.SurfaceAddVertex(_d);
		mesh.SurfaceAddVertex(_a);

		mesh.SurfaceEnd();

		surfacePerMeshCounter++;
	}

	public static void Reset() { Instance?.DoReset(); }

	public void DoReset()
	{
		surfacePerMeshCounter = 0;
		currentMeshIndex = 0;
		debugMeshes.ForEach((m) => m.ClearSurfaces());
	}

	private ImmediateMesh InstantiateNewMesh()
	{
		MeshInstance3D newMesh = new();
		debugMeshes.Add(new ImmediateMesh());
		newMesh.Mesh = debugMeshes.Last();
		AddChild(newMesh);

		GD.Print("Spawned a new Immediate Mesh (" + debugMeshes.Count + ")");

		return debugMeshes.Last(); // return for convenience
	}
}
