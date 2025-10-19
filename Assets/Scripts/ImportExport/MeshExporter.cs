using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public class MeshExporter : MonoBehaviour
{
    public static void MeshToFile(ModelData model, string filePath)
    {
        MeshFilter mf = model.GetMeshFilter();
        Mesh mesh = model.GetMesh();
        StringBuilder sb = new StringBuilder();

        //write verts
        foreach (Vector3 v in mesh.vertices)
        {
            //transform the vertices from local to world space
            Vector3 worldV = mf.transform.TransformPoint(v);
            sb.AppendFormat("v {0} {1} {2}\n", worldV.x, worldV.y, worldV.z);
        }

        //write normals
        sb.Append("\n");
        foreach (Vector3 n in mesh.normals)
        {
            //transform the normals from local to world space
            Vector3 worldN = mf.transform.rotation * n;
            sb.AppendFormat("vn {0} {1} {2}\n", worldN.x, worldN.y, worldN.z);
        }

        //write UVs
        sb.Append("\n");
        foreach (Vector2 uv in mesh.uv)
        {
            sb.AppendFormat("vt {0} {1}\n", uv.x, uv.y);
        }

        //write faces
        sb.Append("\n");
        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            int[] triangles = mesh.GetTriangles(i);
            for (int j = 0; j < triangles.Length; j += 3)
            {
                // OBJ indices start at 1, so add 1 to each
                int i1 = triangles[j] + 1;
                int i2 = triangles[j + 1] + 1;
                int i3 = triangles[j + 2] + 1;

                // Format: f v/vt/vn v/vt/vn v/vt/vn
                sb.AppendFormat("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n", i1, i2, i3);
            }
        }

        // 5. Write the file
        File.WriteAllText(filePath, sb.ToString());
    }
}
