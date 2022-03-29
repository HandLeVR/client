using UnityEngine;

public class DrawNormals : MonoBehaviour
{
    void Start()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        Vector2[] uv2 = mesh.uv2;
        Vector3[] norms = mesh.normals;
        Debug.Log(vertices.Length);
        Debug.Log(uv2.Length);
      
        for (int i = 0; i < vertices.Length; i++)
        {
            Debug.Log("" + vertices[i] + " " + uv2[i]);
            Debug.DrawRay(vertices[i], norms[i] * 10, Color.red,100);
        }
    }
}
