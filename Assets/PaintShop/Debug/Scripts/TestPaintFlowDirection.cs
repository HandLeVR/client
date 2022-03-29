using UnityEngine;

/// <summary>
/// Visualizes the flow direction of paint.
/// </summary>
[ExecuteInEditMode]
public class TestPaintFlowDirection : MonoBehaviour
{
    private Mesh mesh;
    // Start is called before the first frame update
    void Start()
    {
        if (Application.isPlaying)
            mesh = GetComponent<MeshFilter>().mesh;
        if (Application.isEditor)
            mesh = GetComponent<MeshFilter>().sharedMesh;
    }

    private void Update()
    {
        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            gameObject.transform.TransformDirection(Vector3.up);
            Vector3 vertA = mesh.vertices[mesh.triangles[i]];
            Vector3 vertB = mesh.vertices[mesh.triangles[i + 1]];
            Vector3 vertC = mesh.vertices[mesh.triangles[i + 2]];

            Vector3 center = transform.TransformPoint((vertA + vertB + vertC) / 3);
            
            //Vector3 center = transform.localToWorldMatrix.MultiplyPoint((vertA + vertB + vertC) / 3);

            //Vector3 normal = RotatePointAroundPivot(mesh.normals[mesh.triangles[i]],Vector3.zero, transform.rotation);
            Vector3 normal = transform.TransformDirection(mesh.normals[mesh.triangles[i]]);
            
            float r_x = Vector3.Dot(new Vector3(1,0,0), normal) / new Vector3(1,0,0).magnitude * normal.magnitude;
            float r_y = Vector3.Dot(new Vector3(0,1,0), normal) / new Vector3(0,1,0).magnitude * normal.magnitude;
            float r_z = Vector3.Dot(new Vector3(0, 0, 1), normal) / new Vector3(0, 0, 1).magnitude * normal.magnitude;

            float a = Vector3.Dot(new Vector3(0, 1, 0), normal) / normal.magnitude;

            float corA = 1 - Mathf.Abs(a);
            Vector3 dripDir = new Vector3(r_x,r_y,r_z) * corA;

            Vector3 cross = Vector3.Cross(Vector3.up, dripDir);

            //Quaternion rot = Quaternion.AngleAxis(90, cross);

            //Vector3 corDripDir = rot * dripDir;
            Vector3 corDripDir = -Vector3.Cross(cross,dripDir);

            //float dripDir2_z = calc_z2(normal.x, normal.y, normal.z);
            
            //Vector3 dripDir2 = new Vector3(calc_x(normal.x, normal.y, normal.z, -1, dripDir2_z), -1, dripDir2_z);
            
            //Vector3 newDir = new Vector3(dripDir.x*dripDir2.x,dripDir.y*dripDir2.y,dripDir.z*dripDir2.z);

            Debug.DrawLine(center,center + corDripDir , Color.red);

            if (Application.isPlaying)
            {
                Vector3 tangent = transform.TransformDirection(mesh.tangents[mesh.triangles[i]]);

                Debug.DrawLine(center, center + tangent, Color.blue);

                float alpha = Mathf.Acos(Vector3.Dot(tangent, corDripDir) / (tangent.magnitude * corDripDir.magnitude));
                //float dot = corDripDir.x * tangent.y - corDripDir.y * tangent.x;
                //if (dot > 0)
                //    alpha = 2* Mathf.PI - alpha;
                Vector3 v = Vector3.Cross(normal, tangent);
                float dotV = Vector3.Dot(v, corDripDir);
                if (dotV > 0)
                    alpha = 2 * Mathf.PI - alpha;
                Debug.DrawLine(Vector3.zero, v, Color.yellow);

                //Debug.Log(alpha * (180/Math.PI) + " " + Mathf.Cos(alpha) + " " + Mathf.Sin(alpha));
                Vector3 corDripDirUV = new Vector3(corA * Mathf.Cos(alpha),corA * Mathf.Sin(alpha));
                Debug.DrawLine(Vector3.zero, corDripDirUV, Color.green);
                //Debug.Log(corDripDirUV);
                /*Debug.Log("normal: " +  normal.x + " " + normal.y + " " + normal.z);
                Debug.Log("tangent: " +  tangent.x + " " + tangent.y + " " + tangent.z);
                Debug.Log("corDripDirUV: " +  corDripDirUV.x + " " + corDripDirUV.y);*/
               
            }

        }
    }
    
    private Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion angle ) {
        Vector3 dir = point - pivot; // get point direction relative to pivot
        dir = angle * dir; // rotate it
        point = dir + pivot; // calculate rotated point
        return point; // return it
    }
    
    private float calc_z(float n1, float n2, float n3, float y)
    {
        float a = ((n2 * n3 * y) / (n1 * n1 * (1 + (n3 * n3) / (n1 * n1))));
        
        float b1 = n2*n2*n3*n3 - Mathf.Pow(n1,4) * (1 + (n3 * n3) / (n1 * n1));
        
        float b2 = Mathf.Pow(n1,4) * Mathf.Pow(1 + Mathf.Pow(n3/n1,2),2);

        float b = Mathf.Sqrt(b1 / b2 - 1);

        return -a + y * b;
    }

    private float calc_z2(float a, float b, float c)
    {
        float d = (2 * b * c) / (a * a + c * c);

        float e = Mathf.Sqrt((4 * b * b * c * c) / (Mathf.Pow(a, 4) + Mathf.Pow(c, 4) + 2 * a * a * c * c) - ((b * b) / (a * a)) + 2);

        return d + e;
    }

    private float calc_x(float n1, float n2, float n3, float y, float z)
    {
        return (y * n2 + z * n3) / n1;
    }
}
