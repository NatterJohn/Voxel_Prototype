using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SphereMaker : MonoBehaviour
{
    public Material sphereMaterial;

    private Container container;
    [SerializeField] bool behindTheScenes = false;
    [SerializeField] float radiusSphere;
    [SerializeField] int gridWidth;
    [SerializeField] int gridHeight;
    private float heightTreshold = 0.5f;

    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private Mesh mesh;
    private float[,,] heights;

    private MeshFilter meshFilter;

    void Start()
    {
        radiusSphere = 8.5f;
        gridWidth = 16;
        gridHeight = 16;
        meshFilter = GetComponent<MeshFilter>();
        /*StartCoroutine(TestAll());
        TestAll();*/
        GenerateVoxels();
        MarchCubes();
        SetMesh();
    }

    /*void Update()
    {

    }

    private IEnumerator TestAll()
    {
        while (true)
        {
            GenerateVoxels();
            MarchCubes();
            SetMesh();
        }
    }*/

    private void SetMesh()
    {
        if (mesh == null)
        {
            mesh = new Mesh();
            meshFilter.mesh = mesh;
        }

        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
    }

    private void GenerateVoxels()
    {
        if (container != null)
        {
            Destroy(container.gameObject);
        }

        GameObject sphereContainer = new GameObject("Container");
        sphereContainer.transform.parent = transform;
        container = sphereContainer.AddComponent<Container>();
        container.Initialize(sphereMaterial, Vector3.zero);

        Vector3 centerSphere = new Vector3(radiusSphere-1, radiusSphere-1, radiusSphere-1);

        for (int x = 0; x < (radiusSphere * 2); x++)
        {
            for (int y = 0; y < (radiusSphere * 2); y++)
            {
                for (int z = 0; z < (radiusSphere * 2); z++)
                {
                    Vector3 voxelPosition = new Vector3(x, y, z);
                    if (Vector3.Distance(voxelPosition, centerSphere) <= radiusSphere)
                    {
                        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight && z >= 0 && z < gridWidth)
                        {
                            container[voxelPosition] = new Voxel { ID = 1 };
                        }
                        
                    }
                }
            }
        }

        container.GenerateMesh();
        container.UploadMesh();
    }
    

    private void MarchCubes()
    {
        vertices.Clear();
        triangles.Clear();
        heights = new float[gridWidth + 1, gridHeight + 1, gridWidth + 1];
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                for (int z = 0; z < gridWidth; z++)
                {
                    float[] cubeCorners = new float[8];

                    for (int i = 0; i < 8; i++)
                    {
                        Vector3Int corner = new Vector3Int(x, y, z) + MarchingTable.Corners[i];
                        cubeCorners[i] = heights[corner.x, corner.y, corner.z];
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
    }
    private void OnDrawGizmosSelected()
    {
        if (!behindTheScenes || !Application.isPlaying)
        {
            return;
        }
        for (int x = 0; x < gridWidth + 1; x++)
        {
            for (int y = 0; y < gridHeight + 1; y++)
            {
                for (int z = 0; z < gridWidth + 1; z++)
                {
                    Gizmos.color = new Color(heights[x, y, z], heights[x, y, z], heights[x, y, z], 1);
                    Gizmos.DrawSphere(new Vector3(x , y , z), 0.1f);
                }
            }
        }
    }
}
