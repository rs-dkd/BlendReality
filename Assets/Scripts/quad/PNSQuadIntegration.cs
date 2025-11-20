using UnityEngine;
using UnityEngine.ProBuilder;

/// <summary>
/// Extension methods to integrate QuadTopologyManager with PNSModelIntegration
/// </summary>
public static class PNSQuadIntegration
{
    /// <summary>
    /// Convert quad topology to PNS control net format
    /// This is the key method that feeds quad structure to PnSpline instead of triangles
    /// </summary>
    public static (double[,] vertices, int[][] faces) QuadTopologyToPNSData(
        QuadTopologyManager.QuadTopologyData quadData)
    {
        if (quadData == null)
        {
            throw new System.ArgumentNullException(nameof(quadData));
        }

        // Convert vertices to double array
        double[,] vertices = new double[quadData.vertices.Count, 3];
        for (int i = 0; i < quadData.vertices.Count; i++)
        {
            vertices[i, 0] = quadData.vertices[i].x;
            vertices[i, 1] = quadData.vertices[i].y;
            vertices[i, 2] = quadData.vertices[i].z;
        }

        // Convert quad polygons to face indices
        int[][] faces = new int[quadData.quadPolygons.Count][];
        for (int i = 0; i < quadData.quadPolygons.Count; i++)
        {
            faces[i] = quadData.quadPolygons[i].vertexIndices;
        }

        Debug.Log($"[PNS-Quad] Converted {quadData.vertices.Count} verts, " +
                  $"{quadData.quadPolygons.Count} polygons for PnSpline");

        return (vertices, faces);
    }

    /// <summary>
    /// Create PnSpline from quad topology instead of triangulated mesh
    /// This is what you should call instead of the standard GetOrCreatePnSpline
    /// </summary>
    public static PolyhedralNetSplines.PnSpline CreatePnSplineFromQuadTopology(
        this ModelData model,
        bool forceRecreate = false)
    {
        if (model == null || PNSModelIntegration.Instance == null)
            return null;

        var quadData = QuadTopologyManager.Instance.GetQuadTopology(model.modelID);

        if (quadData == null)
        {
            Debug.LogWarning($"[PNS-Quad] No quad topology found for model {model.modelName}. " +
                           "Attempting to reconstruct from triangles...");
            quadData = QuadTopologyManager.Instance.ReconstructQuadTopologyFromTriangles(model);

            if (quadData == null)
            {
                Debug.LogError($"[PNS-Quad] Failed to create quad topology for {model.modelName}");
                return null;
            }
        }

        // Convert quad data to PNS format
        var (vertices, faces) = QuadTopologyToPNSData(quadData);

        // Create PnSpline using the quad-based face structure
        var config = PNSModelIntegration.Instance.config;
        var spline = new PolyhedralNetSplines.PnSpline(vertices, faces, config.degreeRaise);

        Debug.Log($"[PNS-Quad] Created PnSpline for {model.modelName}: " +
                  $"{spline.NumPatches} patches from {quadData.quadPolygons.Count} quads");

        return spline;
    }

    /// <summary>
    /// Update PnSpline when vertices move, using quad topology
    /// </summary>
    public static uint[] UpdatePnSplineFromQuadTopology(
        this ModelData model,
        int[] modifiedVertexIndices)
    {
        if (model == null || PNSModelIntegration.Instance == null)
            return new uint[0];

        var quadData = QuadTopologyManager.Instance.GetQuadTopology(model.modelID);
        if (quadData == null)
        {
            Debug.LogWarning($"[PNS-Quad] No quad topology for {model.modelName}, falling back to triangle update");
            return model.UpdatePnSpline(modifiedVertexIndices);
        }

        // Update vertex positions in quad data from ProBuilder mesh
        var pbMesh = model.GetEditModel();
        var worldPositions = pbMesh.VerticesInWorldSpace();

        Vector3[] modifiedPositions = new Vector3[modifiedVertexIndices.Length];
        for (int i = 0; i < modifiedVertexIndices.Length; i++)
        {
            int vertIdx = modifiedVertexIndices[i];
            modifiedPositions[i] = worldPositions[vertIdx];

            // Update in quad data cache
            if (vertIdx < quadData.vertices.Count)
            {
                quadData.vertices[vertIdx] = modifiedPositions[i];
            }
        }

        // Use existing PNS update mechanism
        return PNSModelIntegration.Instance.UpdatePnSplineWithPositions(
            model,
            modifiedVertexIndices,
            modifiedPositions);
    }
}