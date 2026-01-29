using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class SphereMaker : MonoBehaviour
{
    public Material sphereMaterial;

    private Container container;
    //[SerializeField] bool behindTheScenes = false;
    float radiusSphere = 6;
    /// <summary>
    /// This is the width in units of all the voxels
    /// </summary>
    int gridWidthActual = 15;
    /// <summary>
    /// The number of voxels in a gridWidthActual
    /// </summary>
    int voxelResolution = 15;
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
      //  MarchCubes();
      //  SetMesh();
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

    private void GenerateVoxels(int gridWidthActual, int voxelResolution)
    {
        float voxelWidth = gridWidthActual / voxelResolution;
        voxels = new VoxelScript[voxelResolution + 2, voxelResolution + 2, voxelResolution + 2];
        

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
        return new Vector3( -(voxelResolution - 1f)/2f + ((float) ix * voxelWidth), -(voxelResolution - 1f) / 2f + ((float)iy * voxelWidth), -(voxelResolution - 1f) / 2f + ((float)iz * voxelWidth) );
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
    //            if (container != null)
    //    {
    //        Destroy(container.gameObject);
    //    }
    //Vector3 centerSphere = new Vector3(radiusSphere - 1, radiusSphere - 1, radiusSphere - 1);
    //GameObject sphereContainer = new GameObject("Container");
    //sphereContainer.transform.parent = transform;
    //container = sphereContainer.AddComponent<Container>();
    //container.Initialize(sphereMaterial, centerSphere);



    //// Loop through each possible voxel position
    //for (int x = 0; x < radiusSphere * 2; x++)
    //{
    //    for (int y = 0; y < radiusSphere * 2; y++)
    //    {
    //        for (int z = 0; z < radiusSphere * 2; z++)
    //        {
    //            Vector3 voxelPosition = new Vector3(x, y, z);
    //            // Check if the voxel position is inside the sphere radius
    //            if (Vector3.Distance(voxelPosition, centerSphere) <= radiusSphere)
    //            {
    //                // Check if the voxel is within the voxel space
    //                if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight && z >= 0 && z < gridWidth)
    //                {
    //                    // Create the voxel
    //                    GameObject newVoxel = GameObject.CreatePrimitive(PrimitiveType.Cube);
    //                    Renderer renderer = newVoxel.GetComponent<Renderer>();
    //                    renderer.material = sphereMaterial;
    //                    VoxelScript voxel = newVoxel.AddComponent<VoxelScript>();
    //                    voxel.SetPosition(voxelPosition);
    //                    container[voxelPosition] = voxel;
    //                }
    //            }
    //        }
    //    }
    //}
    /*private void MarchCubes()
    {
        vertices.Clear();
        triangles.Clear();
       // heights = new float[gridWidthActual + 1, gridWidthActual + 1, gridWidthActual + 1];
        for (int x = 0; x < gridWidthActual; x++)
        {
            for (int y = 0; y < gridWidthActual; y++)
            {
                for (int z = 0; z < gridWidthActual; z++)
                {
                    float[] cubeCorners = new float[8];

                    for (int i = 0; i < 8; i++)
                    {
                        Vector3Int corner = new Vector3Int(x, y, z) + MarchingTable.Corners[i];
                    //    cubeCorners[i] = heights[corner.x, corner.y, corner.z];
                    }

                    MarchCube(new Vector3(x, y, z), cubeCorners);
                }
            }
        }
    }
    private int GetConfigIndex(float[] cubeCorners)
    {
        int configIndex = 0;

        for (int i = 0; i < 8; i++)
        {
            if (cubeCorners[i] > heightTreshold)
            {
                configIndex |= 1 << i;
            }
        }

        return configIndex;
    }
    private void MarchCube(Vector3 position, float[] cubeCorners)
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
                int triTableValue = MarchingTable.Triangles[configIndex, edgeIndex];

                if (triTableValue == -1)
                {
                    return;
                }

                Vector3 edgeStart = position + MarchingTable.Edges[triTableValue, 0];
                Vector3 edgeEnd = position + MarchingTable.Edges[triTableValue, 1];

                Vector3 vertex = (edgeStart + edgeEnd) / 2;

                vertices.Add(vertex);
                triangles.Add(vertices.Count - 1);

                edgeIndex++;
            }
        }
    }*/
    /*private void OnDrawGizmosSelected()
    {
        if (!behindTheScenes || !Application.isPlaying)
        {
            return;
        }

        for (float x = -gridWidthActual / 2; x < gridWidthActual / 2; x += voxelWidth)
        {
            for (float y = -gridWidthActual / 2; y < gridWidthActual / 2; y += voxelWidth)
            {
                for (float z = -gridWidthActual / 2; z < gridWidthActual / 2; z += voxelWidth)
                {
                    gridPoints = new float[gridWidthActual + 1, gridWidthActual + 1, gridWidthActual + 1];
                    Gizmos.color = new Color(gridPoints[x, y, z], gridPoints[x, y, z], gridPoints[x, y, z], 1);
                    Gizmos.DrawSphere(new Vector3(x - 0.5f , y - 0.5f, z - 0.5f), 0.1f);
                }
            }
        }
    }*/
}
