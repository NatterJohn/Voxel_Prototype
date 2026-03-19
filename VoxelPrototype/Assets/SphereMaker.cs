using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(MeshRenderer))]
public class SphereMaker : MonoBehaviour
{
    public Material sphereMaterial;
    GameObject marchCube;
    
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
    
    Vector3 centreSphere = Vector3.zero;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private Mesh mesh;
    public VoxelScript[,,] voxels;
    int intIndex = 1;

    void Start()
    {
        /*radiusSphere = 8.5f;
        gridWidth = 16;
        gridHeight = 16;*/
        
        //GenerateVoxels(gridWidthActual,voxelResolution);
        //CreateSphere(centreSphere, radiusSphere);
        //MarchCubes();
        
        //SetMesh();
        //CreateUnityGameObject();

    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) {
            print("gameobject for index " + intIndex.ToString());
            vertices = new List<Vector3>();
            triangles = new List<int>();
            testMarchCube(intIndex);
            intIndex += 1;
            SetMesh();
            if (marchCube != null) { 
                Destroy(marchCube);
            }
            marchCube = CreateUnityGameObject();
        }   
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
    public GameObject CreateUnityGameObject()
    {
        // Create the GameObject
        GameObject MarchedSphere = new GameObject("MarchingCubesMesh");

        // Add components
        MeshFilter meshFilter = MarchedSphere.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = MarchedSphere.AddComponent<MeshRenderer>();

        // Assign the mesh you built earlier
        meshFilter.mesh = mesh;

        // Assign material
        meshRenderer.material = sphereMaterial;

        // Optional: add collider
        MeshCollider meshCollider = MarchedSphere.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;

        return MarchedSphere;
    }


    private void MarchCubes()
    {
        vertices.Clear();
        triangles.Clear();

        for (int x = 0; x < voxelResolution; x++)
        {
            for (int y = 0; y < voxelResolution; y++)
            {
                for (int z = 0; z < voxelResolution; z++)
                {
                    VoxelScript voxel = voxels[x, y, z];
                    List<Vector3> cubeCorners = voxel.GetCorners();

                    /*for (int i = 0; i < 8; i++)
                    {
                        Vector3Int corner = new Vector3Int(x, y, z) + MarchingTable.Corners[i];
                    }*/

                    MarchCube(new Vector3(x, y, z), cubeCorners);
                }
            }
        }
    }

    private int GetConfigIndex(List<Vector3> cubeCorners)
    {
        int configIndex = 0;

        for (int i = 0; i < 8; i++)
        {
            if (Vector3.Distance(cubeCorners[i], centreSphere) < radiusSphere)
            {
                // Use bitwise OR to set the i-th bit to 1
                configIndex |= 1 << i;
            }
            
        }

        return configIndex;
    }

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
    }
    private void testMarchCube(int i)
    {
        int edgeIndex = 0;
        for (int t = 0; t < 5; t++)
        {
            for (int v = 0; v < 3; v++)
            {
                int triTableValue = MarchingTable.Triangles[i, edgeIndex];

                if (triTableValue == -1)
                {
                    return;
                }

                Vector3 edgeStart = MarchingTable.Edges[triTableValue, 0];
                Vector3 edgeEnd = MarchingTable.Edges[triTableValue, 1];

                Vector3 vertex = (edgeStart + edgeEnd) / 2;

                vertices.Add(vertex);
                triangles.Add(vertices.Count - 1);

                edgeIndex++;
            }
        }
    }
}
