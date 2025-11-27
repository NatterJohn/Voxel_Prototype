using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class VoxelScript : MonoBehaviour
{
    float width = 1f;
    public bool _isactive = true;
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
        result.Add(transform.position + new Vector3(-hW, hW, hW));
        result.Add(transform.position + new Vector3(hW, hW, hW));
        result.Add(transform.position + new Vector3(-hW, -hW, hW));
        result.Add(transform.position + new Vector3(hW, -hW, hW));
        result.Add(transform.position + new Vector3(-hW, hW, -hW));
        result.Add(transform.position + new Vector3(hW, hW, -hW));
        result.Add(transform.position + new Vector3(-hW, -hW, -hW));
        result.Add(transform.position + new Vector3(hW, -hW, -hW));
        return result;

    }
    public void SetActive(bool active)
    {
        _isactive = active;
        _renderer.enabled = active;
    }
}
