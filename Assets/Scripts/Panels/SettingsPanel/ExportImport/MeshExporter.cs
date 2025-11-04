using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

/// <summary>
/// Imports mesh
/// Allows to combine meshes into one
/// </summary>
public class MeshExporter : MonoBehaviour
{
    /// <summary>
    /// Exports a list of models to a single .obj file.
    /// </summary>
    public static void MeshToFile(List<ModelData> models, string filePath, string fileName, bool combineModels = false)
    {
        if (models == null || models.Count == 0)
        {
            Debug.LogError("No models to export.");
            return;
        }

        StringBuilder sb = new StringBuilder();

        // Separate offsets are CRITICAL for a valid .obj file.
        int vertexOffset = 0;
        int normalOffset = 0;
        int uvOffset = 0;

        // This tracks every name actually written to the file to guarantee uniqueness.
        HashSet<string> writtenNames = new HashSet<string>();

        sb.Append("# Exported from BlendReality\n");
        sb.AppendFormat("# Models: {0}\n", models.Count);

        if (combineModels)
        {
            sb.Append("o " + fileName + "\n");
        }

        //Iterate through each model
        foreach (ModelData model in models)
        {
            if (model.GetMeshFilter() == null || model.GetMeshFilter().sharedMesh == null)
            {
                Debug.LogWarning($"Skipping model '{model.GetName()}' due to missing MeshFilter or Mesh.");
                continue;
            }

            Mesh mesh = model.GetMeshFilter().sharedMesh; 
            Transform transform = model.meshFilter.transform;

            //Check what data this specific mesh has
            bool hasNormals = mesh.normals != null && mesh.normals.Length > 0;
            bool hasUVs = mesh.uv != null && mesh.uv.Length > 0;

            //Unique names (Models have to have unique names or they are combined)
            if (!combineModels)
            {
                string baseName = model.GetName();
                string uniqueName = baseName;
                int count = 1;

                //Keep checking new names until we find one that hasn't been written yet
                while (writtenNames.Contains(uniqueName))
                {
                    uniqueName = $"{baseName}_{count}";
                    count++;
                }

                //Add the new unique name to our tracker and write it to the file
                writtenNames.Add(uniqueName);
                sb.AppendFormat("o {0}\n", uniqueName);
            }

            //Write Vertices
            foreach (Vector3 v in mesh.vertices)
            {
                Vector3 worldV = transform.TransformPoint(v);
                sb.AppendFormat("v {0} {1} {2}\n", -worldV.x, worldV.y, worldV.z);
            }
            sb.Append("\n");

            //Write Normals
            if (hasNormals)
            {
                foreach (Vector3 n in mesh.normals)
                {
                    Vector3 worldN = transform.TransformDirection(n);
                    sb.AppendFormat("vn {0} {1} {2}\n", -worldN.x, worldN.y, worldN.z);
                }
                sb.Append("\n");
            }

            //Write UVs 
            if (hasUVs)
            {
                foreach (Vector2 uv in mesh.uv)
                {
                    sb.AppendFormat("vt {0} {1}\n", uv.x, uv.y);
                }
                sb.Append("\n");
            }

            //Write Faces 
            string faceFormat;
            if (hasNormals && hasUVs)
                faceFormat = "f {0}/{1}/{2} {3}/{4}/{5} {6}/{7}/{8}\n";
            else if (hasNormals)
                faceFormat = "f {0}//{1} {2}//{3} {4}//{5}\n";
            else if (hasUVs)
                faceFormat = "f {0}/{1} {2}/{3} {4}/{5}\n";
            else
                faceFormat = "f {0} {1} {2}\n";

            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                int[] triangles = mesh.GetTriangles(i);
                for (int j = 0; j < triangles.Length; j += 3)
                {
                    //Get the vertex indices from the triangles array
                    //(OBJ indices start at 1, not 0)
                    int v1 = triangles[j] + 1;
                    int v2 = triangles[j + 1] + 1;
                    int v3 = triangles[j + 2] + 1;

                    //Write faces using the correct format and separate offsets
                    if (hasNormals && hasUVs)
                    {
                        sb.AppendFormat(faceFormat,
                            v1 + vertexOffset, v1 + uvOffset, v1 + normalOffset,
                            v2 + vertexOffset, v2 + uvOffset, v2 + normalOffset,
                            v3 + vertexOffset, v3 + uvOffset, v3 + normalOffset);
                    }
                    else if (hasNormals) // v//vn
                    {
                        sb.AppendFormat(faceFormat,
                            v1 + vertexOffset, v1 + normalOffset,
                            v2 + vertexOffset, v2 + normalOffset,
                            v3 + vertexOffset, v3 + normalOffset);
                    }
                    else if (hasUVs) // v/vt
                    {
                        sb.AppendFormat(faceFormat,
                            v1 + vertexOffset, v1 + uvOffset,
                            v2 + vertexOffset, v2 + uvOffset,
                            v3 + vertexOffset, v3 + uvOffset);
                    }
                    else // v
                    {
                        sb.AppendFormat(faceFormat,
                            v1 + vertexOffset,
                            v2 + vertexOffset,
                            v3 + vertexOffset);
                    }
                }
            }
            sb.Append("\n");

            //Update the separate offsets for the next model
            vertexOffset += mesh.vertexCount;
            if (hasNormals) normalOffset += mesh.normals.Length;
            if (hasUVs) uvOffset += mesh.uv.Length;
        }

        //Write the file
        File.WriteAllText(filePath, sb.ToString());
        Debug.Log($"Successfully exported {models.Count} models to {filePath}");
    }
}