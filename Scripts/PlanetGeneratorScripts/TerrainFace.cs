using Godot;
using System;

public class TerrainFace : MeshInstance
{
    private int resolution = 0;
    private Vector3 localUp = Vector3.Up;
    private Vector3 axisA;
    private Vector3 axisB;

    public TerrainFace(int newResolution, Vector3 newLocalUp, bool flipMesh = false)
    {
        resolution = newResolution;
        localUp = newLocalUp;
        axisA = new Vector3(localUp.y, localUp.z, localUp.x);

        axisB = flipMesh ? localUp.Cross(axisA) : localUp.Cross(-axisA);
    }

    public void UpdateParams(int newResolution, Vector3 newLocalUp, bool flipMesh = false)
    {
        resolution = newResolution;
        localUp = newLocalUp;
        axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        
        axisB = flipMesh ? localUp.Cross(axisA) : localUp.Cross(-axisA);
    }

    public void ConstrucMesh(bool useInBakedLight = false)
    {
        Vector3[] vertices = new Vector3[resolution * resolution];
        Vector3[] normals = new Vector3[resolution * resolution];
        Vector2[] uvs = new Vector2[resolution * resolution];
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];
        int triIndex = 0;
        
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = x + y * resolution;
                Vector2 percent = new Vector2(x, y) / (resolution - 1);
                Vector3 pointOnUnitCube = localUp + (percent.x - .5f) * 2 * axisA + (percent.y - .5f) * 2 * axisB;
                Vector3 pointOnUnitSphere = pointOnUnitCube.Normalized();
                vertices[i] = pointOnUnitSphere;
                normals[i] = pointOnUnitSphere;
                uvs[i] = new Vector2(x,y);

                if (x != resolution-1 && y != resolution-1)
                {
                    triangles[triIndex] = i;
                    triangles[triIndex + 1] = i + resolution + 1;
                    triangles[triIndex + 2] = i + resolution;
                    
                    triangles[triIndex + 3] = i;
                    triangles[triIndex + 4] = i + 1;
                    triangles[triIndex + 5] = i + resolution + 1;
                    
                    triIndex += 6;
                }
            }
        }
        
        MakeMesh(vertices, triangles, normals, uvs, useInBakedLight);
    }

    private void MakeMesh(Vector3[] vert, int[] indexes, Vector3[] normal, Vector2[] uv, bool useInBakedLight = false)
    {
        ArrayMesh arrayMesh = new ArrayMesh();
        
        var arrays = new Godot.Collections.Array();
        arrays.Resize((int)ArrayMesh.ArrayType.Max);
        arrays[(int)ArrayMesh.ArrayType.Vertex] = vert;
        arrays[(int)ArrayMesh.ArrayType.Normal] = normal;
        arrays[(int)ArrayMesh.ArrayType.TexUv] = uv;
        arrays[(int)ArrayMesh.ArrayType.Index] = indexes;
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
        arrayMesh.RegenNormalmaps();

        Mesh = arrayMesh;
        UseInBakedLight = useInBakedLight;
    }
}
