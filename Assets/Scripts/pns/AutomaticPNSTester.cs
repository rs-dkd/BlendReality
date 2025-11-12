using UnityEngine;
using System.Collections.Generic;
using PolyhedralNetSplines;

/// <summary>
/// Automatic PNS Tester - No setup required!
/// Just add this component to any GameObject and press Play
/// It will automatically find and test all models in the scene
/// </summary>
public class AutomaticPNSTester : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Run tests automatically when scene starts")]
    public bool runOnStart = true;

    [Tooltip("Which tests to run")]
    public bool testPnSplineGeneration = true;
    public bool testVertexUpdate = true;
    public bool testExport = false; // Off by default as it creates files

    [Header("Export Settings (if enabled)")]
    public string exportDirectory = "PNSExports/";
    public bool exportToBV = true;
    public bool exportToIGS = false;
    public bool exportToSTEP = false;

    private void Start()
    {
        if (runOnStart)
        {
            RunAllTests();
        }
    }

    [ContextMenu("Run All Tests")]
    public void RunAllTests()
    {
        Debug.Log("+=======================================+");
        Debug.Log("|   AUTOMATIC PNS TESTING STARTED       |");
        Debug.Log("+=======================================+");
        Debug.Log("");

        // Get all models in scene
        List<ModelData> allModels = GetAllModels();

        if (allModels.Count == 0)
        {
            Debug.LogError(" No models found in scene!");
            Debug.LogWarning("Create a model using your VR tools first, then run this test again.");
            return;
        }

        Debug.Log($" Found {allModels.Count} model(s) in scene:");
        foreach (var model in allModels)
        {
            Debug.Log($"  - {model.modelName} (ID: {model.modelID})");
        }
        Debug.Log("");

        // Run tests on each model
        int testsPassed = 0;
        int testsFailed = 0;

        foreach (var model in allModels)
        {
            Debug.Log($"--- Testing Model: {model.modelName} ---");

            if (testPnSplineGeneration)
            {
                if (TestPnSplineGeneration(model))
                    testsPassed++;
                else
                    testsFailed++;
            }

            if (testVertexUpdate)
            {
                if (TestVertexUpdate(model))
                    testsPassed++;
                else
                    testsFailed++;
            }

            if (testExport)
            {
                if (TestExport(model))
                    testsPassed++;
                else
                    testsFailed++;
            }

            Debug.Log("");
        }

        // Summary
        Debug.Log("+=======================================+");
        Debug.Log("|   TEST SUMMARY                        |");
        Debug.Log("+=======================================+");
        Debug.Log($" Tests Passed: {testsPassed}");
        if (testsFailed > 0)
            Debug.LogWarning($" Tests Failed: {testsFailed}");
        else
            Debug.Log(" All tests passed!");
        Debug.Log("");
    }

    /// <summary>
    /// Get all ModelData components in scene
    /// </summary>
    private List<ModelData> GetAllModels()
    {
        List<ModelData> models = new List<ModelData>();

        // Try to get from ModelsManager first (more reliable)
        if (ModelsManager.Instance != null)
        {
            models = ModelsManager.Instance.GetAllModelsInScene();
        }

        // Fallback: Find all ModelData components
        if (models.Count == 0)
        {
            models.AddRange(FindObjectsOfType<ModelData>());
        }

        return models;
    }

    /// <summary>
    /// Test 1: PnSpline Generation
    /// </summary>
    private bool TestPnSplineGeneration(ModelData model)
    {
        Debug.Log("[TEST 1] PnSpline Generation...");

        try
        {
            PnSpline spline = model.GetPnSpline();

            if (spline == null)
            {
                Debug.LogError("    Failed: PnSpline is null");
                return false;
            }

            uint numPatches = spline.NumPatches;
            Debug.Log($"    Generated PnSpline with {numPatches} patches");

            // Verify patches
            bool allPatchesValid = true;
            for (uint i = 0; i < numPatches; i++)
            {
                PnSPatch patch = spline.GetPatch(i);
                if (!patch.IsValid)
                {
                    Debug.LogWarning($"    Patch {i} is invalid!");
                    allPatchesValid = false;
                }
            }

            if (allPatchesValid)
            {
                Debug.Log($"    All {numPatches} patches are valid");
            }

            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"    Failed with exception: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Test 2: Vertex Update
    /// </summary>
    private bool TestVertexUpdate(ModelData model)
    {
        Debug.Log("[TEST 2] Vertex Update...");

        try
        {
            int vertCount = model.GetVertCount();

            if (vertCount == 0)
            {
                Debug.LogWarning("    Model has no vertices, skipping test");
                return true; // Not a failure, just N/A
            }

            // Update first vertex (or first 3 if available)
            int numToUpdate = Mathf.Min(3, vertCount);
            int[] indices = new int[numToUpdate];
            for (int i = 0; i < numToUpdate; i++)
            {
                indices[i] = i;
            }

            // Small offset
            Vector3 offset = new Vector3(0.01f, 0.01f, 0.01f);
            model.AddOffsetToVerts(offset, indices);

            // Flush updates
            uint[] affectedPatches = model.FlushDirtyVertices();

            Debug.Log($"    Updated {numToUpdate} vertices");
            Debug.Log($"    {affectedPatches.Length} patches were affected");

            // Restore original position
            model.AddOffsetToVerts(-offset, indices);
            model.FlushDirtyVertices();

            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"    Failed with exception: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Test 3: Export
    /// </summary>
    private bool TestExport(ModelData model)
    {
        Debug.Log("[TEST 3] Export...");

        try
        {
            // Ensure directory exists
            if (!System.IO.Directory.Exists(exportDirectory))
            {
                System.IO.Directory.CreateDirectory(exportDirectory);
            }

            bool anyExported = false;
            string basePath = System.IO.Path.Combine(exportDirectory, model.modelName);

            if (exportToBV)
            {
                string path = basePath + ".bv";
                model.ExportToBV(path);
                Debug.Log($"    Exported to BV: {path}");
                anyExported = true;
            }

            if (exportToIGS)
            {
                string path = basePath + ".igs";
                model.ExportToIGS(path);
                Debug.Log($"    Exported to IGS: {path}");
                anyExported = true;
            }

            if (exportToSTEP)
            {
                string path = basePath + ".step";
                model.ExportToSTEP(path);
                Debug.Log($"    Exported to STEP: {path}");
                anyExported = true;
            }

            if (!anyExported)
            {
                Debug.LogWarning("    No export formats enabled");
            }

            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"    Failed with exception: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Quick info about all models
    /// </summary>
    [ContextMenu("Show All Models Info")]
    public void ShowAllModelsInfo()
    {
        Debug.Log("=== ALL MODELS INFO ===");

        var allModels = GetAllModels();

        if (allModels.Count == 0)
        {
            Debug.LogWarning("No models in scene");
            return;
        }

        foreach (var model in allModels)
        {
            Debug.Log($"\nModel: {model.modelName} (ID: {model.modelID})");
            Debug.Log($"  Position: {model.GetPosition()}");
            Debug.Log($"  Vertices: {model.GetVertCount()}");
            Debug.Log($"  Faces: {model.GetFacesCount()}");
            Debug.Log($"  Edges: {model.GetEdgesCount()}");

            try
            {
                var spline = model.GetPnSpline();
                Debug.Log($"  PnSpline Patches: {spline.NumPatches}");
            }
            catch
            {
                Debug.Log($"  PnSpline: Not yet generated");
            }
        }
    }

    /// <summary>
    /// Export all models
    /// </summary>
    [ContextMenu("Export All Models")]
    public void ExportAllModels()
    {
        Debug.Log("=== EXPORTING ALL MODELS ===");

        if (!System.IO.Directory.Exists(exportDirectory))
        {
            System.IO.Directory.CreateDirectory(exportDirectory);
        }

        var allModels = GetAllModels();

        foreach (var model in allModels)
        {
            Debug.Log($"\nExporting: {model.modelName}");

            if (exportToBV)
            {
                string path = System.IO.Path.Combine(exportDirectory, model.modelName + ".bv");
                model.ExportToBV(path);
                Debug.Log($"    BV: {path}");
            }

            if (exportToIGS)
            {
                string path = System.IO.Path.Combine(exportDirectory, model.modelName + ".igs");
                model.ExportToIGS(path);
                Debug.Log($"    IGS: {path}");
            }

            if (exportToSTEP)
            {
                string path = System.IO.Path.Combine(exportDirectory, model.modelName + ".step");
                model.ExportToSTEP(path);
                Debug.Log($"   STEP: {path}");
            }
        }

        Debug.Log($"\n All models exported to: {exportDirectory}");
    }
}