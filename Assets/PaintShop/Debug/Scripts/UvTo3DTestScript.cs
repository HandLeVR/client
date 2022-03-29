using UnityEngine;

public class UvTo3DTestScript : MonoBehaviour
{
   private Mesh mesh;
   private Renderer rend;
   
   void Start()
   {
      mesh = GetComponent<MeshFilter>().mesh;
      rend = GetComponent<Renderer>();
      /*Vector3[] vertices = mesh.vertices;
      Vector2[] uvs = new Vector2[vertices.Length];

      for (int i = 0; i < uvs.Length; i++)
      {
         uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
      }
      mesh.uv = uvs;
      for (int i=0; i<mesh.vertices.Length; i++){
         Vector3 norm = transform.TransformDirection(mesh.normals[i]);
         Vector3 vert = transform.TransformPoint(mesh.vertices[i]);
         Debug.DrawRay(vert, norm * 10, Color.red,100);
      }*/
      Debug.Log(mesh.uv.Length);
      Texture2D t = (Texture2D) rend.material.mainTexture;
      t.SetPixel(25,25,new Color(5f,5f,5f));
      Debug.Log(t.width);
      Debug.Log(t.height);
      double test = 0;
      for (int x = 0; x < t.width; x++)
      {
         for (int y = 0; y < t.height; y++)
         {
            Vector3 norm;
            Vector3 pos= UvTo3D(new Vector2(x / (float)t.width, (float)y / t.height),out norm);
            test += pos.x;
            //Debug.DrawRay(pos, norm, t.GetPixel(x,y),100);
         }
      }
      Debug.Log(test);
      Debug.Log(t.GetPixel(25,25));
   }
   Vector3 UvTo3D(Vector2 uv, out Vector3 normal)
   {
      int[] tris = mesh.triangles;
      Vector2[] uvs = mesh.uv;
      Vector3[] verts = mesh.vertices;
      normal = Vector3.zero;
      for (int i = 0; i < tris.Length; i += 3)
      {
         Vector2 u1 = uvs[tris[i]]; // get the triangle UVs
         Vector2 u2 = uvs[tris[i + 1]];
         Vector2 u3 = uvs[tris[i + 2]];
         // calculate triangle area - if zero, skip it
         float a = Area(u1, u2, u3);
         if (a == 0) continue;
         // calculate barycentric coordinates of u1, u2 and u3
         // if anyone is negative, point is outside the triangle: skip it
         float a1 = Area(u2, u3, uv) / a;
         if (a1 < 0) continue;
         float a2 = Area(u3, u1, uv) / a;
         if (a2 < 0) continue;
         float a3 = Area(u1, u2, uv) / a;
         if (a3 < 0) continue;
         // point inside the triangle - find mesh position by interpolation...
         Vector3 p3D = a1 * verts[tris[i]] + a2 * verts[tris[i + 1]] + a3 * verts[tris[i + 2]];
         normal = transform.TransformDirection(a1 *mesh.normals[tris[i]]+a2 * mesh.normals[tris[i+1]]+a3 * mesh.normals[tris[i+2]]);
         // and return it in world coordinates:
         return transform.TransformPoint(p3D);
      }

      // point outside any uv triangle: return Vector3.zero
      return Vector3.zero;
   }

   // calculate signed triangle area using a kind of "2D cross product":
   float Area(Vector2 p1, Vector2 p2, Vector2 p3) {
      Vector2 v1= p1 - p3;
      Vector2 v2= p2 - p3;
      return (v1.x * v2.y - v1.y * v2.x)/2;
   }
}
