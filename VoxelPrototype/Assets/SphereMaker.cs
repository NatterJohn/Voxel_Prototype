using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class SphereMaker : MonoBehaviour
{
    public Material sphereMaterial;

    
    //[SerializeField] bool behindTheScenes = false;
    float radiusSphere = 6;
    /// <summary>
    /// This is the width in units of all the voxels
    /// </summary>
    float gridWidthActual = 15;
    /// <summary>
    /// The number of voxels in a gridWidthActual
    /// </summary>
    float voxelResolution = 25;
    //[SerializeField] int gridWidth;
    //[SerializeField] int gridHeight;
    private float heightTreshold = 0.5f;
    private float[,,] gridPoints;
    

    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private Mesh mesh;
    public VoxelScript[,,] voxels;


    void Start()
    {
        /*radiusSphere = 8.5f;
        gridWidth = 16;
        gridHeight = 16;*/
        
        GenerateVoxels(gridWidthActual,voxelResolution);
        CreateSphere(Vector3.zero, radiusSphere);
        MarchCubes();
        SetMesh();
    }

    private void SetMesh()
    {
        if (mesh == null)
        {
            mesh = new Mesh();
        }

        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
    }

    private void GenerateVoxels(float gridWidthActual, float voxelResolution)
    {
        float voxelWidth = gridWidthActual / voxelResolution;
        voxels = new VoxelScript[(int)voxelResolution + 2, (int)voxelResolution + 2, (int)voxelResolution + 2];
        

        for (int ix = 0; ix < voxelResolution; ix++)
            for (int iy = 0; iy < voxelResolution; iy++)
                for (int iz = 0; iz < voxelResolution; iz++)
                { 
                    Vector3 voxelPosition = voxelPositionFromIndex(ix, iy, iz);
                    GameObject newVoxel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Collider col =newVoxel.GetComponent<Collider>();
                    Destroy(col);
                    newVoxel.transform.localScale = voxelWidth * Vector3.one;
                    Renderer renderer = newVoxel.GetComponent<Renderer>();
                    renderer.material = sphereMaterial;
                    VoxelScript voxel = newVoxel.AddComponent<VoxelScript>();
                    voxel.SetPosition(voxelPosition);
                    voxel.SetActive(true);
                    voxels[ix,iy,iz] = voxel;
                    print(ix.ToString() + iy.ToString() + iz.ToString());
                }
              
            
           
        
    }

    private Vector3 voxelPositionFromIndex(int ix, int iy, int iz)
    {
        float voxelWidth = gridWidthActual / voxelResolution;
        return new Vector3( -(gridWidthActual - 1f)/2f + ((float) ix * voxelWidth), -(gridWidthActual - 1f) / 2f + ((float)iy * voxelWidth), -(gridWidthActual - 1f) / 2f + ((float)iz * voxelWidth) );
    }

    void CreateSphere(Vector3 pos, float radius)
    {
        
        // Loop through each possible voxel position
        for (int x = 0; x < voxelResolution; x++)
        {
            for (int y = 0; y < voxelResolution; y++)
            {
                for (int z = 0; z < voxelResolution; z++)
                {
                    VoxelScript voxel = voxels[x, y, z];
                    if (voxel == null) continue;

                    // Check if the voxel position is inside the sphere radius
                    float distance = Vector3.Distance(voxel.transform.position, pos);
                    print(distance.ToString());
                    if (distance <= radius)
                    {
                        voxel.SetActive(true);
                    }
                    else
                    {
                        voxel.SetActive(false);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Iterates through the entire 3D grid to generate the procedural mesh.
    /// </summary>
    private void MarchCubes()
    {
        // Reset mesh data before rebuilding
        vertices.Clear();
        triangles.Clear();

        // Loop through each voxel in the 3D grid
        for (int x = 0; x < gridWidthActual; x++)
        {
            for (int y = 0; y < gridWidthActual; y++)
            {
                for (int z = 0; z < gridWidthActual; z++)
                {
                    // Retrieve the specific voxel and its 8 corner positions
                    VoxelScript voxel = voxels[x, y, z];
                    List<Vector3> cubeCorners = voxel.GetCorners();

                    // (Optional/Debug) Calculate world-space corner coordinates 
                    for (int i = 0; i < 8; i++)
                    {
                        Vector3Int corner = new Vector3Int(x, y, z) + MarchingTable.Corners[i];
                    }

                    // Process this individual cube to find and create triangles
                    MarchCube(new Vector3(x, y, z), cubeCorners);
                }
            }
        }
    }

    /// <summary>
    /// Creates an 8-bit index representing which corners are "inside" the mesh.
    /// </summary>
    private int GetConfigIndex(List<Vector3> cubeCorners)
    {
        int configIndex = 0;

        for (int i = 0; i < 8; i++)
        {
            if (cubeCorners[i].y > heightTreshold)
            {
                // Use bitwise OR to set the i-th bit to 1
                configIndex |= 1 << i;
            }
        }

        return configIndex;
    }

    /// <summary>
    /// Triangulates a single cube based on its corner configuration.
    /// </summary>
    private void MarchCube(Vector3 position, List<Vector3> cubeCorners)
    {
        int configIndex = GetConfigIndex(cubeCorners);

        if (configIndex == 0 || configIndex == 255)
        {
            return;
        }

        int edgeIndex = 0;
        for (int t = 0; t < 5; t++)
        {
            for (int v = 0; v < 3; v++)
            {
                // Get the edge ID from the lookup table
                int triTableValue = MarchingTable.Triangles[configIndex, edgeIndex];

                // -1 indicates there are no more triangles to draw for this configuration
                if (triTableValue == -1)
                {
                    return;
                }

                // Identify the two corners that form the edge where the vertex will be placed
                Vector3 edgeStart = position + MarchingTable.Edges[triTableValue, 0];
                Vector3 edgeEnd = position + MarchingTable.Edges[triTableValue, 1];

                // Calculate the midpoint of the edge to place the vertex
                Vector3 vertex = (edgeStart + edgeEnd) / 2;

                // Add the calculated vertex to the mesh list
                vertices.Add(vertex);
                // Add the index to the triangle list (winding order is handled by the table)
                triangles.Add(vertices.Count - 1);

                edgeIndex++;
            }
        }
    }
}
