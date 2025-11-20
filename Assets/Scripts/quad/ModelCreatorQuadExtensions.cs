using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using System.IO;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Updates to ModelCreator to support quad topology preservation
/// Add these methods to your existing ModelCreator class
/// </summary>
public static class ModelCreatorQuadExtensions
{
    /// <summary>
    /// Import OBJ file with quad topology preservation
    /// Call this INSTEAD of the standard OBJ import
    /// </summary>
    public static ModelData ImportOBJWithQuadTopology(string objFilePath, Vector3 spawnPosition, string modelName = "ImportedModel")
    {
        if (!File.Exists(objFilePath))
        {
            Debug.LogError($"OBJ file not found: {objFilePath}");
            return null;
        }

        // STEP 1: Parse quad topology BEFORE Unity triangulates
        Debug.Log($"[QuadImport] Parsing quad topology from: {objFilePath}");

        // Create a temporary model ID for the topology
        int tempModelID = ModelData.GetNewModelID();

        var quadData = QuadTopologyManager.Instance.BuildQuadTopologyFromOBJ(objFilePath, tempModelID);

        if (quadData == null)
        {
            Debug.LogError($"[QuadImport] Failed to parse quad topology from {objFilePath}");
            return null;
        }

        Debug.Log($"[QuadImport] Captured {quadData.quadPolygons.Count} quads before triangulation");

        // STEP 2: Now let Unity/ProBuilder import and triangulate
        ProBuilderMesh pbMesh = ImportOBJToProBuilder(objFilePath);

        if (pbMesh == null)
        {
            Debug.LogError($"[QuadImport] Failed to import OBJ to ProBuilder");
            QuadTopologyManager.Instance.RemoveQuadTopology(tempModelID);
            return null;
        }

        // STEP 3: Create ModelData with the triangulated mesh
        ModelData model = CreateModelDataFromProBuilder(pbMesh, spawnPosition, modelName);

        if (model == null)
        {
            Debug.LogError($"[QuadImport] Failed to create ModelData");
            QuadTopologyManager.Instance.RemoveQuadTopology(tempModelID);
            return null;
        }

        // STEP 4: Transfer quad topology to the actual model ID
        quadData.modelID = model.modelID;
        QuadTopologyManager.Instance.RemoveQuadTopology(tempModelID);

        // STEP 5: Build triangle-to-quad mapping now that we have both structures
        QuadTopologyManager.Instance.BuildTriangleToQuadMapping(model, quadData);

        Debug.Log($"[QuadImport] Successfully imported {modelName} with quad topology preserved");
        Debug.Log($"[QuadImport] Quads: {quadData.quadPolygons.Count(q => !q.isTriangle)}, " +
                  $"Triangles: {quadData.quadPolygons.Count(q => q.isTriangle)}");

        // STEP 6: Initialize PnSpline using quad topology
        if (model.enablePNSUpdates && PNSModelIntegration.Instance != null)
        {
            var spline = model.CreatePnSplineFromQuadTopology(forceRecreate: true);

            if (spline != null)
            {
                // Cache it in the PNS system
                PNSModelIntegration.Instance.CachePnSpline(model.modelID, spline);
                Debug.Log($"[QuadImport] PnSpline created with {spline.NumPatches} patches");
            }
        }

        return model;
    }

    /// <summary>
    /// Standard ProBuilder OBJ import (Unity will triangulate internally)
    /// </summary>
    private static ProBuilderMesh ImportOBJToProBuilder(string objFilePath)
    {
        try
        {
            // Create a game object for the imported mesh
            GameObject go = new GameObject("ImportedMesh");
            ProBuilderMesh pbMesh = go.AddComponent<ProBuilderMesh>();

            // Use Unity's OBJ importer or a custom parser
            // This will triangulate the mesh
            Mesh unityMesh = LoadOBJAsUnityMesh(objFilePath);

            if (unityMesh == null)
            {
                Object.DestroyImmediate(go);
                return null;
            }

            // Convert to ProBuilder
            // Create faces from triangles
            var faces = new List<UnityEngine.ProBuilder.Face>();
            for (int i = 0; i < unityMesh.triangles.Length; i += 3)
            {
                faces.Add(new UnityEngine.ProBuilder.Face(new int[]
                {
                    unityMesh.triangles[i],
                    unityMesh.triangles[i + 1],
                    unityMesh.triangles[i + 2]
                }));
            }

            pbMesh.Clear();
            pbMesh.RebuildWithPositionsAndFaces(unityMesh.vertices, faces);
            pbMesh.ToMesh();
            pbMesh.Refresh();

            return pbMesh;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error importing OBJ: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Load OBJ as Unity Mesh (this will be triangulated)
    /// </summary>
    private static Mesh LoadOBJAsUnityMesh(string objFilePath)
    {
        // Use Unity's built-in OBJ loader or implement a simple parser
        // For now, using a basic implementation

        var vertices = new System.Collections.Generic.List<Vector3>();
        var triangles = new System.Collections.Generic.List<int>();

        string[] lines = File.ReadAllLines(objFilePath);

        foreach (string line in lines)
        {
            string trimmed = line.Trim();

            if (trimmed.StartsWith("v "))
            {
                string[] parts = trimmed.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 4)
                {
                    float x = float.Parse(parts[1]);
                    float y = float.Parse(parts[2]);
                    float z = float.Parse(parts[3]);
                    vertices.Add(new Vector3(x, y, z));
                }
            }
            else if (trimmed.StartsWith("f "))
            {
                string[] parts = trimmed.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);

                // Parse vertex indices
                int[] faceVerts = new int[parts.Length - 1];
                for (int i = 1; i < parts.Length; i++)
                {
                    string[] vertData = parts[i].Split('/');
                    faceVerts[i - 1] = int.Parse(vertData[0]) - 1; // OBJ is 1-indexed
                }

                // Triangulate face
                if (faceVerts.Length == 3)
                {
                    // Already a triangle
                    triangles.Add(faceVerts[0]);
                    triangles.Add(faceVerts[1]);
                    triangles.Add(faceVerts[2]);
                }
                else if (faceVerts.Length == 4)
                {
                    // Quad - split into 2 triangles
                    triangles.Add(faceVerts[0]);
                    triangles.Add(faceVerts[1]);
                    triangles.Add(faceVerts[2]);

                    triangles.Add(faceVerts[0]);
                    triangles.Add(faceVerts[2]);
                    triangles.Add(faceVerts[3]);
                }
                else if (faceVerts.Length > 4)
                {
                    // N-gon - fan triangulation from first vertex
                    for (int i = 1; i < faceVerts.Length - 1; i++)
                    {
                        triangles.Add(faceVerts[0]);
                        triangles.Add(faceVerts[i]);
                        triangles.Add(faceVerts[i + 1]);
                    }
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    /// <summary>
    /// Create ModelData from ProBuilderMesh
    /// </summary>
    private static ModelData CreateModelDataFromProBuilder(ProBuilderMesh pbMesh, Vector3 position, string name)
    {
        GameObject modelGO = new GameObject(name);
        modelGO.transform.position = position;

        ModelData model = modelGO.AddComponent<ModelData>();
        model.SetupModel(pbMesh, position, name);

        return model;
    }

    /// <summary>
    /// Export model back to OBJ with quad topology preserved
    /// </summary>
    public static void ExportModelToOBJWithQuads(ModelData model, string outputPath)
    {
        if (model == null)
        {
            Debug.LogError("Cannot export null model");
            return;
        }

        var quadData = QuadTopologyManager.Instance.GetQuadTopology(model.modelID);

        if (quadData != null)
        {
            // Export using quad topology
            QuadTopologyManager.Instance.ExportQuadTopologyToOBJ(model.modelID, outputPath);
            Debug.Log($"[QuadExport] Exported {model.modelName} with quad topology to {outputPath}");
        }
        else
        {
            // Fallback to triangulated export
            Debug.LogWarning($"[QuadExport] No quad topology for {model.modelName}, exporting triangulated mesh");
            ExportTriangulatedMeshToOBJ(model, outputPath);
        }
    }

    /// <summary>
    /// Fallback triangulated export
    /// </summary>
    private static void ExportTriangulatedMeshToOBJ(ModelData model, string outputPath)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(outputPath))
            {
                writer.WriteLine($"# Exported from Unity - {model.modelName}");
                writer.WriteLine();

                // Export vertices
                var verts = model.GetVerts();
                foreach (var v in verts)
                {
                    writer.WriteLine($"v {v.x} {v.y} {v.z}");
                }

                writer.WriteLine();

                // Export faces (as triangles)
                var faces = model.GetFaces();
                foreach (var face in faces)
                {
                    var indices = face.indexes;
                    if (indices.Count >= 3)
                    {
                        writer.Write("f");
                        foreach (int idx in indices)
                        {
                            writer.Write($" {idx + 1}"); // OBJ is 1-indexed
                        }
                        writer.WriteLine();
                    }
                }
            }

            Debug.Log($"Exported triangulated mesh to: {outputPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error exporting OBJ: {e.Message}");
        }
    }
}