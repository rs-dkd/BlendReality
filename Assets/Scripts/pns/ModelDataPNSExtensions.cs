using UnityEngine;
using PolyhedralNetSplines;

/// <summary>
/// Extension to ModelData that adds PNS-specific functionality
/// Add this as a partial class or integrate into ModelData.cs
/// </summary>
public static class ModelDataPNSExtensions
{
    /// <summary>
    /// Generate or update the PnSpline for this model
    /// </summary>
    public static PnSpline GetPnSpline(this ModelData model, bool forceRecreate = false)
    {
        return PNSModelIntegration.Instance.GetOrCreatePnSpline(model, forceRecreate);
    }

    /// <summary>
    /// Update the PnSpline after vertex modifications
    /// Call this after editing vertices for efficient updates
    /// </summary>
    public static uint[] UpdatePnSpline(this ModelData model, int[] modifiedVertexIndices)
    {
        return PNSModelIntegration.Instance.UpdatePnSpline(model, modifiedVertexIndices);
    }

    /// <summary>
    /// Mark vertices as needing update (batch processing)
    /// </summary>
    public static void MarkVerticesDirty(this ModelData model, params int[] vertexIndices)
    {
        PNSModelIntegration.Instance.MarkVerticesDirty(model, vertexIndices);
    }

    /// <summary>
    /// Flush all pending vertex updates
    /// </summary>
    public static uint[] FlushDirtyVertices(this ModelData model)
    {
        return PNSModelIntegration.Instance.FlushDirtyVertices(model);
    }

    /// <summary>
    /// Export this model to BV format
    /// </summary>
    public static void ExportToBV(this ModelData model, string outputPath)
    {
        PNSModelIntegration.Instance.ExportToBV(model, outputPath);
    }

    /// <summary>
    /// Export this model to IGES format
    /// </summary>
    public static void ExportToIGS(this ModelData model, string outputPath)
    {
        PNSModelIntegration.Instance.ExportToIGS(model, outputPath);
    }

    /// <summary>
    /// Export this model to STEP format
    /// </summary>
    public static void ExportToSTEP(this ModelData model, string outputPath)
    {
        PNSModelIntegration.Instance.ExportToSTEP(model, outputPath);
    }
}