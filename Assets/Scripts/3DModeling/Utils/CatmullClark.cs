using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;

public static class CatmullClark
{
    /// <summary>
    /// Subdivide a ProBuilder mesh and return a new smooth Mesh.
    /// This version includes correct handling for boundary edges and vertices.
    /// </summary>
    public static Mesh Subdivide(ProBuilderMesh pbMesh)
    {
        Vector3[] oldVerts = pbMesh.positions.ToArray();
        Face[] faces = pbMesh.faces.ToArray();

        // STEP 0: Merge vertices that are in the same position (Your code is correct)
        Dictionary<Vector3, int> mergedVerts = new Dictionary<Vector3, int>();
        List<Vector3> vertices = new List<Vector3>();
        int[] vertMap = new int[oldVerts.Length];
        float threshold = 0.00001f;

        for (int i = 0; i < oldVerts.Length; i++)
        {
            Vector3 pos = oldVerts[i];
            bool found = false;
            foreach (var kv in mergedVerts)
            {
                if ((kv.Key - pos).sqrMagnitude < threshold * threshold)
                {
                    vertMap[i] = kv.Value;
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                int index = vertices.Count;
                vertices.Add(pos);
                mergedVerts[pos] = index;
                vertMap[i] = index;
            }
        }

        Vector3[] facePoints = new Vector3[faces.Length];
        for (int f = 0; f < faces.Length; f++)
        {
            Vector3 sum = Vector3.zero;
            foreach (int vi in faces[f].indexes)
                sum += vertices[vertMap[vi]];
            facePoints[f] = sum / faces[f].indexes.Count;
        }

        Dictionary<(int, int), List<int>> edgeFaces = new Dictionary<(int, int), List<int>>();
        for (int f = 0; f < faces.Length; f++)
        {
            var face = faces[f];
            int n = face.indexes.Count;
            for (int i = 0; i < n; i++)
            {
                int v0 = vertMap[face.indexes[i]];
                int v1 = vertMap[face.indexes[(i + 1) % n]];
                var key = (Mathf.Min(v0, v1), Mathf.Max(v0, v1));

                if (!edgeFaces.ContainsKey(key))
                    edgeFaces[key] = new List<int>();
                edgeFaces[key].Add(f);
            }
        }

        Dictionary<(int, int), Vector3> edgePoints = new Dictionary<(int, int), Vector3>();
        foreach (var kvp in edgeFaces)
        {
            var edge = kvp.Key;
            var adjFaces = kvp.Value;

            if (adjFaces.Count == 1)
            {
                edgePoints[edge] = (vertices[edge.Item1] + vertices[edge.Item2]) / 2f;
            }
            else
            {
                Vector3 sum = vertices[edge.Item1] + vertices[edge.Item2];
                foreach (int fi in adjFaces)
                    sum += facePoints[fi];
                edgePoints[edge] = sum / (2f + adjFaces.Count); 
            }
        }

        Vector3[] newVerts = new Vector3[vertices.Count];
        Dictionary<int, List<int>> vertFaces = new Dictionary<int, List<int>>();
        Dictionary<int, List<(int, int)>> vertEdges = new Dictionary<int, List<(int, int)>>();

        for (int f = 0; f < faces.Length; f++)
        {
            var faceIndices = faces[f].indexes.Select(i => vertMap[i]).ToArray();
            for (int i = 0; i < faceIndices.Length; i++)
            {
                int v = faceIndices[i];
                if (!vertFaces.ContainsKey(v)) vertFaces[v] = new List<int>();
                if (!vertEdges.ContainsKey(v)) vertEdges[v] = new List<(int, int)>();

                vertFaces[v].Add(f);

                int vPrev = faceIndices[(i + faceIndices.Length - 1) % faceIndices.Length];
                vertEdges[v].Add((Mathf.Min(v, vPrev), Mathf.Max(v, vPrev)));
            }
        }

        for (int v = 0; v < vertices.Count; v++)
        {
            var incidentEdges = vertEdges[v].Distinct().ToList();
            bool isBoundaryVertex = false;
            foreach (var edge in incidentEdges)
            {
                if (edgeFaces[edge].Count == 1)
                {
                    isBoundaryVertex = true;
                    break;
                }
            }

            if (isBoundaryVertex)
            {

                Vector3 newPos = vertices[v] * 0.75f;
                int boundaryNeighbors = 0;
                foreach (var edge in incidentEdges)
                {
                    if (edgeFaces[edge].Count == 1) 
                    {
                        int neighbor = (edge.Item1 == v) ? edge.Item2 : edge.Item1;
                        newPos += vertices[neighbor] * 0.125f;
                        boundaryNeighbors++;
                    }
                }

                if (boundaryNeighbors < 2)
                {
                    newVerts[v] = vertices[v];
                }
                else
                {
                    newVerts[v] = newPos;
                }
            }
            else
            {
                Vector3 F = Vector3.zero; 
                foreach (int f_idx in vertFaces[v]) F += facePoints[f_idx];
                F /= vertFaces[v].Count;

                Vector3 R = Vector3.zero; 
                foreach (var e in incidentEdges) R += (vertices[e.Item1] + vertices[e.Item2]) / 2f;
                R /= incidentEdges.Count;

                int n = vertFaces[v].Count;
                newVerts[v] = (F + 2f * R + (n - 3) * vertices[v]) / n;
            }
        }

        List<Vector3> finalVerts = new List<Vector3>();
        List<int> finalTris = new List<int>();
        Dictionary<(int, int), int> edgeIndexMap = new Dictionary<(int, int), int>();
        int[] faceIndexMap = new int[faces.Length];
        int[] vertIndexMap = new int[newVerts.Length];

        foreach (var kvp in edgePoints)
        {
            edgeIndexMap[kvp.Key] = finalVerts.Count;
            finalVerts.Add(kvp.Value);
        }
        for (int f = 0; f < faces.Length; f++)
        {
            faceIndexMap[f] = finalVerts.Count;
            finalVerts.Add(facePoints[f]);
        }
        for (int v = 0; v < newVerts.Length; v++)
        {
            vertIndexMap[v] = finalVerts.Count;
            finalVerts.Add(newVerts[v]);
        }

        for (int f = 0; f < faces.Length; f++)
        {
            var face = faces[f];
            int n = face.indexes.Count;
            for (int i = 0; i < n; i++)
            {
                int v_orig = vertMap[face.indexes[i]];
                int v_next = vertMap[face.indexes[(i + 1) % n]];
                int v_prev = vertMap[face.indexes[(i + n - 1) % n]];

                int p_face = faceIndexMap[f];
                int p_v = vertIndexMap[v_orig];
                int p_e_next = edgeIndexMap[(Mathf.Min(v_orig, v_next), Mathf.Max(v_orig, v_next))];
                int p_e_prev = edgeIndexMap[(Mathf.Min(v_orig, v_prev), Mathf.Max(v_orig, v_prev))];

                finalTris.Add(p_v);
                finalTris.Add(p_e_next);
                finalTris.Add(p_face);

                finalTris.Add(p_face);
                finalTris.Add(p_e_prev);
                finalTris.Add(p_v);
            }
        }

        Mesh mesh = new Mesh { name = "SubdividedMesh" };
        mesh.vertices = finalVerts.ToArray();
        mesh.triangles = finalTris.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}