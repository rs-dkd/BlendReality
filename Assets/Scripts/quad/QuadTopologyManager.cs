using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;

/// <summary>
/// Maintains a parallel quad-dominant topology structure alongside Unity's triangulated mesh.
/// Provides mapping between triangle indices and quad polygon indices for PnS integration.
/// </summary>
public class QuadTopologyManager : MonoBehaviour
{
    private static QuadTopologyManager _instance;
    public static QuadTopologyManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<QuadTopologyManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("QuadTopologyManager");
                    _instance = go.AddComponent<QuadTopologyManager>();
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// Represents a quad polygon in the original topology
    /// </summary>
    [Serializable]
    public class QuadPolygon
    {
        public int quadIndex;           // Index in quad array
        public int[] vertexIndices;     // 4 vertices for quad, 3 for triangle
        public int[] triangleIndices;   // Triangle face indices that make up this quad (1 or 2)
        public bool isTriangle;         // True if this is actually a triangle

        public QuadPolygon(int quadIdx, int[] verts, int[] triIndices, bool isTri = false)
        {
            quadIndex = quadIdx;
            vertexIndices = verts;
            triangleIndices = triIndices;
            isTriangle = isTri;
        }
    }

    /// <summary>
    /// Quad topology data for a specific model
    /// </summary>
    [Serializable]
    public class QuadTopologyData
    {
        public int modelID;
        public List<Vector3> vertices;              // Shared vertices
        public List<QuadPolygon> quadPolygons;      // Quad/tri polygons
        public Dictionary<int, int> triangleToQuadMap;  // Maps triangle index -> quad index

        public QuadTopologyData(int id)
        {
            modelID = id;
            vertices = new List<Vector3>();
            quadPolygons = new List<QuadPolygon>();
            triangleToQuadMap = new Dictionary<int, int>();
        }
    }

    // Cache of quad topology data per model
    private Dictionary<int, QuadTopologyData> _topologyCache = new Dictionary<int, QuadTopologyData>();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Build quad topology from an OBJ file before Unity triangulates it
    /// </summary>
    public QuadTopologyData BuildQuadTopologyFromOBJ(string objFilePath, int modelID)
    {
        var quadData = new QuadTopologyData(modelID);

        try
        {
            string[] lines = System.IO.File.ReadAllLines(objFilePath);

            foreach (string line in lines)
            {
                string trimmed = line.Trim();

                // Parse vertices
                if (trimmed.StartsWith("v "))
                {
                    string[] parts = trimmed.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 4)
                    {
                        float x = float.Parse(parts[1]);
                        float y = float.Parse(parts[2]);
                        float z = float.Parse(parts[3]);
                        quadData.vertices.Add(new Vector3(x, y, z));
                    }
                }
                // Parse faces (quads or triangles)
                else if (trimmed.StartsWith("f "))
                {
                    string[] parts = trimmed.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    // Extract vertex indices (OBJ is 1-indexed, convert to 0-indexed)
                    List<int> faceVerts = new List<int>();
                    for (int i = 1; i < parts.Length; i++)
                    {
                        string[] vertData = parts[i].Split('/');
                        int vertIdx = int.Parse(vertData[0]) - 1;
                        faceVerts.Add(vertIdx);
                    }

                    // Create quad polygon
                    int quadIdx = quadData.quadPolygons.Count;
                    bool isTriangle = faceVerts.Count == 3;

                    QuadPolygon quad = new QuadPolygon(
                        quadIdx,
                        faceVerts.ToArray(),
                        new int[0], // Will be filled when we map to Unity triangles
                        isTriangle
                    );

                    quadData.quadPolygons.Add(quad);
                }
            }

            Debug.Log($"Loaded quad topology: {quadData.vertices.Count} vertices, {quadData.quadPolygons.Count} polygons");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing OBJ file: {e.Message}");
            return null;
        }

        _topologyCache[modelID] = quadData;
        return quadData;
    }

    /// <summary>
    /// Build triangle-to-quad mapping after Unity has triangulated the mesh
    /// </summary>
    public void BuildTriangleToQuadMapping(ModelData model, QuadTopologyData quadData)
    {
        if (model == null || quadData == null) return;

        ProBuilderMesh pbMesh = model.GetEditModel();
        if (pbMesh == null) return;

        quadData.triangleToQuadMap.Clear();

        // Get all triangle faces from ProBuilder
        var faces = pbMesh.faces;

        int triangleIndex = 0;

        // Map triangles back to quads
        foreach (var quad in quadData.quadPolygons)
        {
            List<int> mappedTriangles = new List<int>();

            if (quad.isTriangle)
            {
                // Single triangle maps to itself
                if (triangleIndex < faces.Count)
                {
                    mappedTriangles.Add(triangleIndex);
                    quadData.triangleToQuadMap[triangleIndex] = quad.quadIndex;
                    triangleIndex++;
                }
            }
            else
            {
                // Quad was split into 2 triangles
                if (triangleIndex < faces.Count - 1)
                {
                    mappedTriangles.Add(triangleIndex);
                    mappedTriangles.Add(triangleIndex + 1);

                    quadData.triangleToQuadMap[triangleIndex] = quad.quadIndex;
                    quadData.triangleToQuadMap[triangleIndex + 1] = quad.quadIndex;

                    triangleIndex += 2;
                }
            }

            quad.triangleIndices = mappedTriangles.ToArray();
        }

        Debug.Log($"Built mapping: {quadData.triangleToQuadMap.Count} triangles mapped to {quadData.quadPolygons.Count} quads");
    }

    /// <summary>
    /// Reconstruct quad topology from an already-triangulated ProBuilder mesh
    /// This is a fallback when we don't have access to the original OBJ
    /// </summary>
    public QuadTopologyData ReconstructQuadTopologyFromTriangles(ModelData model)
    {
        var quadData = new QuadTopologyData(model.modelID);
        ProBuilderMesh pbMesh = model.GetEditModel();

        if (pbMesh == null) return null;

        // Copy vertices
        quadData.vertices = pbMesh.positions.ToList();

        // Try to pair triangles into quads based on shared edges
        var faces = pbMesh.faces.ToList();
        bool[] processed = new bool[faces.Count];

        for (int i = 0; i < faces.Count; i++)
        {
            if (processed[i]) continue;

            Face face1 = faces[i];
            int[] verts1 = face1.indexes.ToArray();

            // Look for adjacent triangle that shares an edge
            bool foundPair = false;
            for (int j = i + 1; j < faces.Count; j++)
            {
                if (processed[j]) continue;

                Face face2 = faces[j];
                int[] verts2 = face2.indexes.ToArray();

                // Check if they share exactly 2 vertices (an edge)
                var sharedVerts = verts1.Intersect(verts2).ToArray();
                if (sharedVerts.Length == 2)
                {
                    // Merge into quad
                    var allVerts = verts1.Union(verts2).ToArray();

                    if (allVerts.Length == 4)
                    {
                        int quadIdx = quadData.quadPolygons.Count;
                        QuadPolygon quad = new QuadPolygon(
                            quadIdx,
                            ReorderQuadVertices(allVerts, pbMesh),
                            new int[] { i, j },
                            false
                        );

                        quadData.quadPolygons.Add(quad);
                        quadData.triangleToQuadMap[i] = quadIdx;
                        quadData.triangleToQuadMap[j] = quadIdx;

                        processed[i] = true;
                        processed[j] = true;
                        foundPair = true;
                        break;
                    }
                }
            }

            // If no pair found, treat as standalone triangle
            if (!foundPair)
            {
                int quadIdx = quadData.quadPolygons.Count;
                QuadPolygon tri = new QuadPolygon(
                    quadIdx,
                    verts1,
                    new int[] { i },
                    true
                );

                quadData.quadPolygons.Add(tri);
                quadData.triangleToQuadMap[i] = quadIdx;
                processed[i] = true;
            }
        }

        Debug.Log($"Reconstructed topology: {quadData.quadPolygons.Count} polygons " +
                  $"({quadData.quadPolygons.Count(q => !q.isTriangle)} quads, " +
                  $"{quadData.quadPolygons.Count(q => q.isTriangle)} triangles)");

        _topologyCache[model.modelID] = quadData;
        return quadData;
    }

    /// <summary>
    /// Reorder quad vertices to form a proper quad loop
    /// </summary>
    private int[] ReorderQuadVertices(int[] vertices, ProBuilderMesh mesh)
    {
        if (vertices.Length != 4) return vertices;

        Vector3[] positions = new Vector3[4];
        for (int i = 0; i < 4; i++)
        {
            positions[i] = mesh.positions[vertices[i]];
        }

        // Find the ordering that minimizes cross-edge lengths
        // Start with vertex 0, find nearest neighbor, then next nearest, etc.
        List<int> ordered = new List<int> { 0 };
        bool[] used = new bool[4];
        used[0] = true;

        for (int i = 0; i < 3; i++)
        {
            int last = ordered[ordered.Count - 1];
            float minDist = float.MaxValue;
            int nearest = -1;

            for (int j = 0; j < 4; j++)
            {
                if (used[j]) continue;

                float dist = Vector3.Distance(positions[last], positions[j]);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = j;
                }
            }

            if (nearest >= 0)
            {
                ordered.Add(nearest);
                used[nearest] = true;
            }
        }

        return ordered.Select(idx => vertices[idx]).ToArray();
    }

    /// <summary>
    /// Get quad topology data for a model
    /// </summary>
    public QuadTopologyData GetQuadTopology(int modelID)
    {
        return _topologyCache.ContainsKey(modelID) ? _topologyCache[modelID] : null;
    }

    /// <summary>
    /// Manually cache quad topology data for a model
    /// Used by MeshImporter to store pre-parsed quad data
    /// </summary>
    public void CacheQuadTopology(int modelID, QuadTopologyData quadData)
    {
        if (quadData != null)
        {
            quadData.modelID = modelID;
            _topologyCache[modelID] = quadData;
            Debug.Log($"[QuadCache] Cached quad topology for model {modelID}: {quadData.quadPolygons.Count} polygons");
        }
    }

    /// <summary>
    /// Update quad vertices when triangle vertices change
    /// </summary>
    public void UpdateQuadVerticesFromTriangles(ModelData model, int[] modifiedTriangleVertices)
    {
        var quadData = GetQuadTopology(model.modelID);
        if (quadData == null) return;

        // Update vertex positions in quad data
        var pbMesh = model.GetEditModel();
        var worldPositions = pbMesh.VerticesInWorldSpace().ToArray();

        foreach (int vertIdx in modifiedTriangleVertices)
        {
            if (vertIdx < quadData.vertices.Count)
            {
                quadData.vertices[vertIdx] = worldPositions[vertIdx];
            }
        }
    }

    /// <summary>
    /// Get the quad index for a given triangle index
    /// </summary>
    public int GetQuadIndexFromTriangle(int modelID, int triangleIndex)
    {
        var quadData = GetQuadTopology(modelID);
        if (quadData == null) return -1;

        return quadData.triangleToQuadMap.ContainsKey(triangleIndex)
            ? quadData.triangleToQuadMap[triangleIndex]
            : -1;
    }

    /// <summary>
    /// Export quad topology to OBJ format (preserving quad structure)
    /// </summary>
    public void ExportQuadTopologyToOBJ(int modelID, string outputPath)
    {
        var quadData = GetQuadTopology(modelID);
        if (quadData == null) return;

        try
        {
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(outputPath))
            {
                writer.WriteLine("# Quad topology export");
                writer.WriteLine($"# {quadData.vertices.Count} vertices, {quadData.quadPolygons.Count} polygons");
                writer.WriteLine();

                // Write vertices
                foreach (var vert in quadData.vertices)
                {
                    writer.WriteLine($"v {vert.x} {vert.y} {vert.z}");
                }

                writer.WriteLine();

                // Write faces (as quads or triangles)
                foreach (var poly in quadData.quadPolygons)
                {
                    string faceStr = "f";
                    foreach (int vertIdx in poly.vertexIndices)
                    {
                        faceStr += $" {vertIdx + 1}"; // OBJ is 1-indexed
                    }
                    writer.WriteLine(faceStr);
                }
            }

            Debug.Log($"Exported quad topology to: {outputPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error exporting quad topology: {e.Message}");
        }
    }

    /// <summary>
    /// Clear quad topology cache for a model
    /// </summary>
    public void RemoveQuadTopology(int modelID)
    {
        if (_topologyCache.ContainsKey(modelID))
        {
            _topologyCache.Remove(modelID);
        }
    }

    /// <summary>
    /// Clear all cached topologies
    /// </summary>
    public void ClearCache()
    {
        _topologyCache.Clear();
    }
}