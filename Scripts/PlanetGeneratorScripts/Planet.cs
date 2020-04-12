using System.Collections.Generic;
using Godot;
using Godot.Collections;

[Tool]
public class Planet : Spatial
{
    [Export(PropertyHint.Range, "2, 256")] private int resolution = 10;
    [Export] private Material material;
    private MeshInstance meshInstance;
    
    TerrainFace[] terrainFaces;

    public override void _Process(float delta)
    {
        if (!Engine.EditorHint)
        {
            if (Input.IsActionPressed("jump"))
            {
                CreatePlanet();
            }
        }
    }

    private void CreatePlanet()
    {
        Vector3[] directions = {Vector3.Up, Vector3.Down, Vector3.Left, Vector3.Right, Vector3.Forward, Vector3.Back};

        if (terrainFaces == null || terrainFaces.Length == 0)
        {
            terrainFaces = new TerrainFace[6];
        }
            
        for (int i = 0; i < 6; i++)
        {
            if (terrainFaces[i] != null)
            {
                terrainFaces[i].SetParams(resolution, directions[i]);
                terrainFaces[i].ConstrucMesh(true);
            }
            else
            {
                terrainFaces[i] = new TerrainFace(resolution, directions[i]);
                terrainFaces[i].Name = $"Face {i}";
                AddChild(terrainFaces[i]);
                terrainFaces[i].MaterialOverride = material;
                terrainFaces[i].ConstrucMesh(true);
            }
        }
    }
}