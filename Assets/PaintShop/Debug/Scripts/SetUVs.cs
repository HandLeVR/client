using UnityEngine;

public class SetUVs : MonoBehaviour
{
    private Mesh mesh;
    
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        mesh.uv = mesh.uv2;
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
    }
}
