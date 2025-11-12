using System.Collections.Generic;
using UnityEngine;
using PolyhedralNetSplines;

/// <summary>
/// Example usage of PNS integration in Unity
/// Attach to a GameObject to test PNS functionality
/// </summary>
public class PNSUsageExamples : MonoBehaviour
{
    [Header("Export Settings")]
    public string exportDirectory = "PNSExports/";

    [Header("Test Model")]
    public ModelData testModel;

    private void Start()
    {
        // Ensure export directory exists
        if (!System.IO.Directory.Exists(exportDirectory))
        {
            System.IO.Directory.CreateDirectory(exportDirectory);
        }
    }

    /// <summary>
    /// Example 1: Generate PnSpline for a model
    /// </summary>
    public void Example1_GeneratePnSpline()
    {
        if (testModel == null)
        {
            Debug.LogError("No test model assigned!");
            return;
        }

        // Get or create PnSpline
        PnSpline spline = testModel.GetPnSpline();

        Debug.Log($"Generated PnSpline with {spline.NumPatches} patches");

        // Access individual patches
        for (uint i = 0; i < spline.NumPatches; i++)
        {
            PnSPatch patch = spline.GetPatch(i);
            Debug.Log($"Patch {i}: Degree U={patch.DegreeU}, Degree V={patch.DegreeV}, Valid={patch.IsValid}");
        }
    }

    /// <summary>
    /// Example 2: Update vertices and refresh PnSpline efficiently
    /// </summary>
    public void Example2_UpdateVertices()
    {
        if (testModel == null) return;

        // Simulate vertex editing
        int[] modifiedVertices = new int[] { 0, 1, 2 }; // First 3 vertices

        Vector3 offset = new Vector3(0.1f, 0.1f, 0);
        testModel.AddOffsetToVerts(offset, modifiedVertices);

        // The PnSpline is automatically updated via MarkVerticesDirty
        // Flush the updates
        uint[] affectedPatches = testModel.FlushDirtyVertices();

        Debug.Log($"Modified {modifiedVertices.Length} vertices, " +
                  $"affecting {affectedPatches.Length} patches");
    }

    /// <summary>
    /// Example 3: Export model to different formats
    /// </summary>
    public void Example3_ExportModel()
    {
        if (testModel == null) return;

        string basePath = System.IO.Path.Combine(exportDirectory, testModel.modelName);

        // Export to BV format
        testModel.ExportToBV(basePath + ".bv");

        // Export to IGES format
        testModel.ExportToIGS(basePath + ".igs");

        // Export to STEP format
        testModel.ExportToSTEP(basePath + ".step");

        Debug.Log($"Exported {testModel.modelName} to {exportDirectory}");
    }

    /// <summary>
    /// Example 4: Access and visualize Bézier control points
    /// </summary>
    public void Example4_VisualizeBezierPoints()
    {
        if (testModel == null) return;

        var patches = PNSModelIntegration.Instance.GetPatches(testModel);

        foreach (var patch in patches)
        {
            List<Vector3> controlPoints = PNSModelIntegration.Instance.GetPatchControlPoints(patch);

            // Visualize control points
            foreach (var point in controlPoints)
            {
                Debug.DrawLine(testModel.GetPosition(), point, Color.cyan, 5f);
                // You could also instantiate debug spheres here
            }

            Debug.Log($"Patch has {controlPoints.Count} Bézier control points");
        }
    }

    /// <summary>
    /// Example 5: Batch export all models in scene
    /// </summary>
    public void Example5_ExportAllModels()
    {
        PNSModelIntegration.Instance.ExportAllModels(exportDirectory, "bv", degreeRaise: true);
        Debug.Log("Exported all models in scene");
    }

    /// <summary>
    /// Example 6: Use PatchBuilder for custom processing
    /// </summary>
    public void Example6_UsePatchBuilder()
    {
        if (testModel == null) return;

        // Get PNS data
        var (vertices, faces) = PNSModelIntegration.Instance.ModelDataToPNSData(testModel);

        // Convert to float for PNSControlMesh
        float[,] floatVerts = new float[vertices.GetLength(0), 3];
        for (int i = 0; i < vertices.GetLength(0); i++)
        {
            floatVerts[i, 0] = (float)vertices[i, 0];
            floatVerts[i, 1] = (float)vertices[i, 1];
            floatVerts[i, 2] = (float)vertices[i, 2];
        }

        // Create control mesh
        var controlMesh = PNSControlMesh.FromData(floatVerts, faces);

        // Get patch builders
        var builders = PNS.GetPatchBuilders(controlMesh);

        Debug.Log($"Found {builders.Count} patch builders");

        foreach (var builder in builders)
        {
            Debug.Log($"Builder: Type={builder.PatchType}, " +
                      $"NumPatches={builder.NumPatches}, " +
                      $"DegU={builder.DegU}, DegV={builder.DegV}");

            // Build patches
            var patches = builder.BuildPatches(controlMesh);

            foreach (var patch in patches)
            {
                // Process patch...
                var bbCoefs = patch.GetBBCoefs();
                Debug.Log($"Patch BB coefficients: {bbCoefs.GetLength(0)}x{bbCoefs.GetLength(1)}");
            }
        }

        controlMesh.Dispose();
    }

    /// <summary>
    /// Example 7: Interactive vertex editing with real-time PNS updates
    /// </summary>
    public void Example7_InteractiveEditing()
    {
        if (testModel == null) return;

        // Enable auto-flush for real-time updates
        testModel.autoFlushOnEdit = true;

        // Simulate interactive editing
        StartCoroutine(SimulateInteractiveEdit());
    }

    private System.Collections.IEnumerator SimulateInteractiveEdit()
    {
        int vertexCount = testModel.GetVertCount();

        for (int i = 0; i < 10; i++)
        {
            // Modify random vertices
            int randomVertex = Random.Range(0, vertexCount);
            Vector3 randomOffset = Random.insideUnitSphere * 0.1f;

            testModel.AddOffsetToVerts(randomOffset, new int[] { randomVertex });

            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log("Interactive editing simulation complete");
    }

    /// <summary>
    /// Example 8: Performance comparison - full rebuild vs incremental update
    /// </summary>
    public void Example8_PerformanceTest()
    {
        if (testModel == null) return;

        int[] testVertices = new int[] { 0, 1, 2, 3, 4 };

        // Test 1: Full rebuild
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        testModel.GetPnSpline(forceRecreate: true);
        stopwatch.Stop();
        long fullRebuildTime = stopwatch.ElapsedMilliseconds;

        // Test 2: Incremental update
        stopwatch.Restart();
        testModel.UpdatePnSpline(testVertices);
        stopwatch.Stop();
        long incrementalUpdateTime = stopwatch.ElapsedMilliseconds;

        Debug.Log($"Performance Test Results:\n" +
                  $"Full Rebuild: {fullRebuildTime}ms\n" +
                  $"Incremental Update: {incrementalUpdateTime}ms\n" +
                  $"Speedup: {(float)fullRebuildTime / incrementalUpdateTime}x");
    }

    // UI Button Methods (for testing in Inspector)
    [ContextMenu("Run Example 1: Generate PnSpline")]
    public void RunExample1() => Example1_GeneratePnSpline();

    [ContextMenu("Run Example 2: Update Vertices")]
    public void RunExample2() => Example2_UpdateVertices();

    [ContextMenu("Run Example 3: Export Model")]
    public void RunExample3() => Example3_ExportModel();

    [ContextMenu("Run Example 4: Visualize Bezier Points")]
    public void RunExample4() => Example4_VisualizeBezierPoints();

    [ContextMenu("Run Example 5: Export All Models")]
    public void RunExample5() => Example5_ExportAllModels();

    [ContextMenu("Run Example 6: Use Patch Builder")]
    public void RunExample6() => Example6_UsePatchBuilder();

    [ContextMenu("Run Example 7: Interactive Editing")]
    public void RunExample7() => Example7_InteractiveEditing();

    [ContextMenu("Run Example 8: Performance Test")]
    public void RunExample8() => Example8_PerformanceTest();
}