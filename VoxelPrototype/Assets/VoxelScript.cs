using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class VoxelScript : MonoBehaviour
{
    float width = 1f;
    public bool _isactive = false;
    Renderer _renderer;
    public byte ID;

    void Start()
    {
        _renderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public Vector3 SetPosition(Vector3 position)
    {
        transform.position = position;
        return transform.position;
    }
    public List<Vector3> GetCorners()
    {
        List<Vector3> result = new List<Vector3>();
        float hW = width / 2;
        result.Add(transform.position + new Vector3(-hW, -hW, -hW)); // 0 (0,0,0)
        result.Add(transform.position + new Vector3(hW, -hW, -hW)); // 1 (1,0,0)
        result.Add(transform.position + new Vector3(hW, hW, -hW)); // 2 (1,1,0)
        result.Add(transform.position + new Vector3(-hW, hW, -hW)); // 3 (0,1,0)
        result.Add(transform.position + new Vector3(-hW, -hW, hW)); // 4 (0,0,1)
        result.Add(transform.position + new Vector3(hW, -hW, hW)); // 5 (1,0,1)
        result.Add(transform.position + new Vector3(hW, hW, hW)); // 6 (1,1,1)
        result.Add(transform.position + new Vector3(-hW, hW, hW)); // 7 (0,1,1)

        return result;

    }
    public void SetActive(bool active)
    {
        _isactive = active;
        _renderer = GetComponent<Renderer>();
        _renderer.enabled = active;
    }
}
