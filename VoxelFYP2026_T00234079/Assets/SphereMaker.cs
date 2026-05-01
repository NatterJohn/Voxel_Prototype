using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(MeshRenderer))]
public class SphereMaker : MonoBehaviour
{
    //This material will be applied to the voxels
    public Material sphereMaterial;
    //The size of the grid the voxels appear in. If voxels are generated outside of this grid, they will be disabled.
    public float gridWidthActual = 15f;
    //Number that can exist in the grid along each dimension of the grid
    public int voxelResolution = 15;
    //Radius of the sphere to be generated
    public float radiusSphere = 6f;
    //Center of the sphere
    Vector3 sphereCenter = Vector3.zero;
    //Displays the voxels instead of the marched sphere when enabled
    public bool showVoxels = false;
    //Displays the wirefram of the voxel grid when enabled
    public bool showIsosurfaces = false;
    private Mesh mesh;
    private List<Vector3> vertices;
    private List<int> triangles;

    void Start()
    {
        //If the bool to show voxels is enabled, then display the voxels. Else, generated the marched sphere.
        if (showVoxels)
            GenerateVoxels();
        else
            MarchCubes();
    }
    void Update()
    {
        /*if (showVoxels)
            ShowVoxels();
        else
            GenerateMesh();*/
    }

    void MarchCubes()
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();
        //The size of each voxel is calculated by dividing the width of the voxel grid (gridWidthActual) by the resolution (voxelResolution)
        float voxelWidth = gridWidthActual / voxelResolution;
        //The center of the grid is calculated by halfing the total width
        float gridCenter = gridWidthActual * 0.5f;
        //All the voxels are looped through
        for (int x = 0; x < voxelResolution; x++)
        {
            for (int y = 0; y < voxelResolution; y++)
            {
                for (int z = 0; z < voxelResolution; z++)
                {
                    //Calculate the position of the voxel
                    Vector3 voxelPosition = new Vector3(x * voxelWidth - gridCenter, y * voxelWidth - gridCenter, z * voxelWidth - gridCenter);
                    //Determine which corners of the voxel are inside the sphere
                    int configIndex = GetConfigIndex(voxelPosition, voxelWidth);
                    //The first and last configurations are always empty
                    if (configIndex == 0 || configIndex == 255)
                        continue;

                    MarchCube(voxelPosition, voxelWidth, configIndex);
                }
            }
        }

        SetMesh();
    }

    int GetConfigIndex(Vector3 voxelPosition, float voxelWidth)
    {
        int configIndex = 0;

        for (int i = 0; i < 8; i++)
        {
            //Calculate the position of the corner
            Vector3 cornerPosition = voxelPosition + (Vector3)MarchingTable.Corners[i] * voxelWidth;
            // Sphere implicit function: (p - center)^2 - r^2
            float value = (cornerPosition - sphereCenter).sqrMagnitude - radiusSphere * radiusSphere;

            if (value < 0f)
                //Use bitwise OR to set the i-th bit to 1
                configIndex |= 1 << i;
        }

        return configIndex;
    }

    void MarchCube(Vector3 voxelPosition, float voxelWidth, int configIndex)
    {
        //Loop through all 15 triangle vertices allowed by the marching cube algorithm and -1
        for (int i = 0; i < 16; i++)
        {
            //Look up which edge of the cube the triangle vertex is on
            int edge = MarchingTable.Triangles[configIndex, i];
            //If the edge is -1 then there are no more edges for this configuration
            if (edge == -1)
                return;
            //Get the two corners that form this edge
            int a = MarchingTable.EdgeToCorner[edge, 0];
            int b = MarchingTable.EdgeToCorner[edge, 1];
            //Calculate positions of corners
            Vector3 p1 = voxelPosition + (Vector3)MarchingTable.Corners[a] * voxelWidth;
            Vector3 p2 = voxelPosition + (Vector3)MarchingTable.Corners[b] * voxelWidth;
            //Calculate how far each corner is from the sphere's surface
            float v1 = (p1 - sphereCenter).sqrMagnitude - radiusSphere * radiusSphere;
            float v2 = (p2 - sphereCenter).sqrMagnitude - radiusSphere * radiusSphere;
            //Calculate the interpolation factor where the surface crosses the edge.
            float t = v1 / (v1 - v2);
            //Clamp to avoid the floating points being too large for flat edges.
            t = Mathf.Clamp01(t);
            //Interpolate the two corners to get the exact surface for each edge.
            Vector3 v = Vector3.Lerp(p1, p2, t);
            //Add the vertex to the list
            vertices.Add(v);
            //Add the indices to form a triangle
            triangles.Add(vertices.Count - 1);
        }
    }

    void SetMesh()
    {
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        //Create a GameObject to represent the marched cubes
        GameObject marchedSphere = new GameObject("MarchedSphere");
        marchedSphere.AddComponent<MeshFilter>().mesh = mesh;
        marchedSphere.AddComponent<MeshRenderer>().material = sphereMaterial;
    }

    public void GenerateVoxels()
    {
        //The size of each voxel is calculated by dividing the width of the voxel grid (gridWidthActual) by the resolution (voxelResolution)
        float voxelWidth = gridWidthActual / voxelResolution;
        //The center of the grid is calculated by halfing the total width
        float gridCenter = gridWidthActual * 0.5f;
        //All the voxels are looped through
        for (int x = 0; x < voxelResolution; x++)
        {
            for (int y = 0; y < voxelResolution; y++)
            {
                for (int z = 0; z < voxelResolution; z++)
                {
                    //Calculate the position of the voxel
                    Vector3 voxelPosition = new Vector3(x * voxelWidth - gridCenter, y * voxelWidth - gridCenter, z * voxelWidth - gridCenter);
                    //Check if center of the voxel is within the sphere
                    float value = (voxelPosition - sphereCenter).sqrMagnitude - radiusSphere * radiusSphere;

                    if (value < 0f)
                    {
                        //Create a cube primitive to display the voxel
                        GameObject voxel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        Collider col = voxel.GetComponent<Collider>();
                        Destroy(col);
                        voxel.transform.position = voxelPosition;
                        voxel.transform.localScale = Vector3.one * voxelWidth;
                        voxel.GetComponent<Renderer>().material = sphereMaterial;
                    }
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!showIsosurfaces || !Application.isPlaying || mesh == null)
            return;

        Gizmos.color = Color.black;

        // Get mesh data
        var verts = mesh.vertices;
        var tris = mesh.triangles;

        // Draw each triangle edge
        for (int i = 0; i < tris.Length; i += 3)
        {
            Vector3 a = verts[tris[i]];
            Vector3 b = verts[tris[i + 1]];
            Vector3 c = verts[tris[i + 2]];

            Gizmos.DrawLine(a, b);
            Gizmos.DrawLine(b, c);
            Gizmos.DrawLine(c, a);
        }
    }

}





