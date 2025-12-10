using Dummiesman;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;

/// <summary>
/// Imports mesh with quad topology preservation
/// Allows to combine meshes into one
/// </summary>
public class MeshImporter : MonoBehaviour
{
    // Singleton pattern 
    public static MeshImporter Instance { get; private set; }
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    [Header("Quad Topology Settings")]
    [Tooltip("Enable quad topology preservation for PnS optimization")]
    public bool preserveQuadTopology = true;

    /// <summary>
    /// Loads the file and imports the meshes as separate or combined
    /// </summary>
    public void LoadFile(string name, string path, bool combineMesh)
    {
        // Parse quad topology BEFORE OBJLoader triangulates
        Dictionary<string, QuadTopologyManager.QuadTopologyData> quadTopologyCache = null;

        if (preserveQuadTopology && QuadTopologyManager.Instance != null)
        {
            quadTopologyCache = ParseQuadTopologyFromOBJ(path);
            Debug.Log($"[QuadImport] Pre-parsed quad topology for {quadTopologyCache.Count} sub-meshes");
        }

        // Use OBJLoader to import (this will triangulate)
        GameObject go = new OBJLoader().Load(path);
        MeshFilter[] meshFilters = go.GetComponentsInChildren<MeshFilter>();

        // Combine all meshes
        if (combineMesh)
        {
            CombineInstance[] combine = new CombineInstance[meshFilters.Length];

            for (int i = 0; i < meshFilters.Length; i++)
            {
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            }

            Mesh combinedMesh = new Mesh();
            combinedMesh.CombineMeshes(combine);
            combinedMesh.name = name + "_Combined";

            // For combined mesh, merge all quad topologies
            QuadTopologyManager.QuadTopologyData mergedQuadData = null;
            if (quadTopologyCache != null && quadTopologyCache.Count > 0)
            {
                mergedQuadData = MergeQuadTopologies(quadTopologyCache);
            }

            CreateModelDataFromMesh(combinedMesh, name, mergedQuadData);
        }
        // Create models separately
        else
        {
            int subMeshIndex = 0;
            foreach (MeshFilter meshFilter in meshFilters)
            {
                if (meshFilter.sharedMesh != null)
                {
                    // Correct mesh position to world space
                    Mesh worldSpaceMesh = new Mesh();

                    Mesh localMesh = meshFilter.sharedMesh;
                    Vector3[] localVerts = localMesh.vertices;
                    Vector3[] localNormals = localMesh.normals;

                    Vector3[] worldVerts = new Vector3[localVerts.Length];
                    Vector3[] worldNormals = new Vector3[localNormals.Length];

                    Transform meshTransform = meshFilter.transform;

                    for (int i = 0; i < localVerts.Length; i++)
                    {
                        worldVerts[i] = meshTransform.TransformPoint(localVerts[i]);
                        worldNormals[i] = meshTransform.TransformDirection(localNormals[i]);
                    }

                    worldSpaceMesh.vertices = worldVerts;
                    worldSpaceMesh.normals = worldNormals;
                    worldSpaceMesh.triangles = localMesh.triangles;
                    worldSpaceMesh.uv = localMesh.uv;

                    // Get corresponding quad topology if available
                    QuadTopologyManager.QuadTopologyData quadData = null;
                    if (quadTopologyCache != null && quadTopologyCache.ContainsKey(meshFilter.name))
                    {
                        quadData = quadTopologyCache[meshFilter.name];

                        // Transform quad vertices to world space
                        for (int i = 0; i < quadData.vertices.Count; i++)
                        {
                            quadData.vertices[i] = meshTransform.TransformPoint(quadData.vertices[i]);
                        }
                    }

                    CreateModelDataFromMesh(worldSpaceMesh, meshFilter.name, quadData);

                    Destroy(worldSpaceMesh);
                    subMeshIndex++;
                }
            }
        }

        // Clean up
        Destroy(go);
    }

    /// <summary>
    /// Parse quad topology from OBJ file BEFORE OBJLoader triangulates it
    /// Returns dictionary of submesh name -> quad topology data
    /// </summary>
    private Dictionary<string, QuadTopologyManager.QuadTopologyData> ParseQuadTopologyFromOBJ(string objFilePath)
    {
        var result = new Dictionary<string, QuadTopologyManager.QuadTopologyData>();

        try
        {
            string[] lines = System.IO.File.ReadAllLines(objFilePath);

            QuadTopologyManager.QuadTopologyData currentQuadData = null;
            string currentObjectName = "default";
            List<Vector3> globalVertices = new List<Vector3>();
            int vertexOffset = 0;

            foreach (string line in lines)
            {
                string trimmed = line.Trim();

                // Object/Group definition - start new submesh
                if (trimmed.StartsWith("o ") || trimmed.StartsWith("g "))
                {
                    // Save previous object if it exists
                    if (currentQuadData != null && currentQuadData.quadPolygons.Count > 0)
                    {
                        result[currentObjectName] = currentQuadData;
                    }

                    // Start new object
                    string[] parts = trimmed.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                    currentObjectName = parts.Length > 1 ? parts[1] : "unnamed";
                    currentQuadData = new QuadTopologyManager.QuadTopologyData(ModelData.GetNewModelID());
                    vertexOffset = globalVertices.Count;

                    Debug.Log($"[QuadParse] Started object: {currentObjectName}");
                }

                // Parse vertices
                if (trimmed.StartsWith("v "))
                {
                    string[] parts = trimmed.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 4)
                    {
                        float x = float.Parse(parts[1]);
                        float y = float.Parse(parts[2]);
                        float z = float.Parse(parts[3]);
                        Vector3 vertex = new Vector3(x, y, z);

                        globalVertices.Add(vertex);

                        // Add to current object's vertices
                        if (currentQuadData == null)
                        {
                            currentQuadData = new QuadTopologyManager.QuadTopologyData(ModelData.GetNewModelID());
                            currentObjectName = "default";
                        }
                        currentQuadData.vertices.Add(vertex);
                    }
                }
                // Parse faces (quads or triangles)
                else if (trimmed.StartsWith("f "))
                {
                    if (currentQuadData == null)
                    {
                        currentQuadData = new QuadTopologyManager.QuadTopologyData(ModelData.GetNewModelID());
                        currentObjectName = "default";
                    }

                    string[] parts = trimmed.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);

                    // Extract vertex indices (OBJ is 1-indexed)
                    List<int> faceVerts = new List<int>();
                    for (int i = 1; i < parts.Length; i++)
                    {
                        string[] vertData = parts[i].Split('/');
                        int vertIdx = int.Parse(vertData[0]) - 1; // Convert to 0-indexed

                        // Adjust for local vertex offset
                        vertIdx -= vertexOffset;

                        if (vertIdx >= 0 && vertIdx < currentQuadData.vertices.Count)
                        {
                            faceVerts.Add(vertIdx);
                        }
                    }

                    if (faceVerts.Count >= 3)
                    {
                        // Create quad polygon
                        int quadIdx = currentQuadData.quadPolygons.Count;
                        bool isTriangle = faceVerts.Count == 3;

                        QuadTopologyManager.QuadPolygon quad = new QuadTopologyManager.QuadPolygon(
                            quadIdx,
                            faceVerts.ToArray(),
                            new int[0], // Will be filled during mapping
                            isTriangle
                        );

                        currentQuadData.quadPolygons.Add(quad);
                    }
                }
            }

            // Save last object
            if (currentQuadData != null && currentQuadData.quadPolygons.Count > 0)
            {
                result[currentObjectName] = currentQuadData;
            }

            // Log statistics
            foreach (var kvp in result)
            {
                var data = kvp.Value;
                int quadCount = data.quadPolygons.Count(q => !q.isTriangle);
                int triCount = data.quadPolygons.Count(q => q.isTriangle);
                Debug.Log($"[QuadParse] {kvp.Key}: {quadCount} quads, {triCount} triangles");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[QuadParse] Error parsing quad topology: {e.Message}");
        }

        return result;
    }

    /// <summary>
    /// Merge multiple quad topology data into one (for combined meshes)
    /// </summary>
    private QuadTopologyManager.QuadTopologyData MergeQuadTopologies(
        Dictionary<string, QuadTopologyManager.QuadTopologyData> quadTopologies)
    {
        var merged = new QuadTopologyManager.QuadTopologyData(ModelData.GetNewModelID());

        int vertexOffset = 0;
        int quadOffset = 0;

        foreach (var kvp in quadTopologies)
        {
            var quadData = kvp.Value;

            // Add vertices
            merged.vertices.AddRange(quadData.vertices);

            // Add polygons with adjusted indices
            foreach (var poly in quadData.quadPolygons)
            {
                int[] adjustedVerts = new int[poly.vertexIndices.Length];
                for (int i = 0; i < poly.vertexIndices.Length; i++)
                {
                    adjustedVerts[i] = poly.vertexIndices[i] + vertexOffset;
                }

                var newPoly = new QuadTopologyManager.QuadPolygon(
                    quadOffset + poly.quadIndex,
                    adjustedVerts,
                    poly.triangleIndices,
                    poly.isTriangle
                );

                merged.quadPolygons.Add(newPoly);
            }

            vertexOffset += quadData.vertices.Count;
            quadOffset += quadData.quadPolygons.Count;
        }

        Debug.Log($"[QuadMerge] Merged into {merged.quadPolygons.Count} polygons from {quadTopologies.Count} submeshes");
        return merged;
    }

    /// <summary>
    /// Create probuilder mesh from the meshes with optional quad topology
    /// </summary>
    private void CreateModelDataFromMesh(Mesh mesh, string name, QuadTopologyManager.QuadTopologyData quadData = null)
    {
        GameObject meshObject = new GameObject(name);
        ProBuilderMesh proBuilderMesh = meshObject.AddComponent<ProBuilderMesh>();

        try
        {
            proBuilderMesh.positions = mesh.vertices;

            List<Face> faces = new List<Face>();
            for (int i = 0; i < mesh.triangles.Length; i += 3)
            {
                if (i + 2 < mesh.triangles.Length)
                {
                    int[] faceIndices = new int[] {
                        mesh.triangles[i],
                        mesh.triangles[i + 1],
                        mesh.triangles[i + 2]
                    };
                    if (faceIndices[0] >= mesh.vertices.Length ||
                        faceIndices[1] >= mesh.vertices.Length ||
                        faceIndices[2] >= mesh.vertices.Length ||
                        faceIndices[0] < 0 || faceIndices[1] < 0 || faceIndices[2] < 0)
                    {
                        Debug.LogWarning($"Invalid face indices: {faceIndices[0]}, {faceIndices[1]}, {faceIndices[2]} (vertex count: {mesh.vertices.Length})");
                        continue;
                    }

                    faces.Add(new Face(faceIndices));
                }
            }

            if (faces.Count == 0)
            {
                Debug.LogError("No valid faces created from raw data");
                Destroy(meshObject);
                return;
            }
            proBuilderMesh.faces = faces;

            if (mesh.uv != null && mesh.uv.Length > 0)
            {
                proBuilderMesh.textures = mesh.uv;
            }
            proBuilderMesh.sharedVertices = SharedVertex.GetSharedVerticesWithPositions(proBuilderMesh.positions);
            proBuilderMesh.ToMesh();
            proBuilderMesh.Refresh();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to create ProBuilder mesh: {e.Message}");
            Destroy(meshObject);
            return;
        }


        //Calculate the object's physical footprint radius
        mesh.RecalculateBounds();
        float objectRadius = Mathf.Max(mesh.bounds.extents.x, mesh.bounds.extents.z);

        //Find the lowest point for floor alignment
        float minLocalY = float.MaxValue;
        if (mesh.vertices.Length > 0)
        {
            foreach (var v in mesh.vertices)
            {
                if (v.y < minLocalY) minLocalY = v.y;
            }
        }
        else
        {
            minLocalY = 0f;
        }

        //Get User's Head Position and Forward Vector
        Vector3 spawnOrigin = Vector3.zero;
        Vector3 flatForward = Vector3.forward;

        if (Camera.main != null)
        {
            Transform cam = Camera.main.transform;
            spawnOrigin = cam.position;

            //Project forward vector onto the floor 
            flatForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
            if (flatForward == Vector3.zero) flatForward = Vector3.forward;
        }

        //Calculate Safe Spawn Position 
        float userPersonalSpace = 0.5f;
        float spawnDistance = userPersonalSpace + objectRadius + 1.0f; 

        Vector3 spawnPos = spawnOrigin + (flatForward * spawnDistance);

        //Snap to Floor
        spawnPos.y = -minLocalY;

        //Create ModelData model from the data
        GameObject myObject = new GameObject();
        ModelData modelData = myObject.AddComponent<ModelData>();

        //Pass calculated spawn position
        modelData.SetupModel(proBuilderMesh, spawnPos, name);

        //Integrate quad topology if available
        if (preserveQuadTopology && quadData != null && QuadTopologyManager.Instance != null)
        {
            //Update Quad Data to match the new World Position
            if (spawnPos != Vector3.zero)
            {
                for (int i = 0; i < quadData.vertices.Count; i++)
                {
                    quadData.vertices[i] += spawnPos;
                }
            }

            QuadTopologyManager.Instance.CacheQuadTopology(modelData.modelID, quadData);
            QuadTopologyManager.Instance.BuildTriangleToQuadMapping(modelData, quadData);

            if (modelData.enablePNSUpdates && PNSModelIntegration.Instance != null)
            {
                var spline = modelData.CreatePnSplineFromQuadTopology(forceRecreate: true);
                if (spline != null)
                {
                    PNSModelIntegration.Instance.CachePnSpline(modelData.modelID, spline);
                    Debug.Log($"[QuadImport] PnSpline created for {name}");
                }
            }
        }
        else if (modelData.enablePNSUpdates && PNSModelIntegration.Instance != null)
        {
            // Fallback reconstruction
            var reconstructedQuadData = QuadTopologyManager.Instance.ReconstructQuadTopologyFromTriangles(modelData);
            if (reconstructedQuadData != null)
            {
                var spline = modelData.CreatePnSplineFromQuadTopology(forceRecreate: true);
                if (spline != null)
                {
                    PNSModelIntegration.Instance.CachePnSpline(modelData.modelID, spline);
                }
            }
        }
    }
}