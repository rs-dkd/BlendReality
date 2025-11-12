using UnityEngine;
using UnityEngine.ProBuilder;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Diagnostic tool to analyze mesh compatibility with PNS
/// Helps understand why a mesh might generate 0 patches
/// </summary>
public class PNSMeshDiagnostics : MonoBehaviour
{
    [Header("Model to Analyze")]
    public GameObject modelObject;

    [ContextMenu("Analyze Mesh")]
    public void AnalyzeMesh()
    {
        if (modelObject == null)
        {
            Debug.LogError("No model object assigned!");
            return;
        }

        ModelData model = modelObject.GetComponent<ModelData>();
        if (model == null)
        {
            Debug.LogError("GameObject doesn't have ModelData component!");
            return;
        }

        Debug.Log("=======================================");
        Debug.Log($"PNS MESH DIAGNOSTICS: {model.modelName}");
        Debug.Log("=======================================");

        ProBuilderMesh mesh = model.GetEditModel();
        if (mesh == null)
        {
            Debug.LogError("Model has no ProBuilderMesh!");
            return;
        }

        // Basic stats
        Debug.Log("\n--- BASIC MESH INFO ---");
        Debug.Log($"Vertices: {mesh.vertexCount}");
        Debug.Log($"Faces: {mesh.faceCount}");
        Debug.Log($"Edges: {mesh.edgeCount}");

        // Detailed face analysis
        Debug.Log("\n--- FACE ANALYSIS ---");
        var faces = mesh.faces;
        var faceTypes = new Dictionary<int, int>(); // sides -> count

        foreach (var face in faces)
        {
            int sides = face.indexes.Count;
            if (!faceTypes.ContainsKey(sides))
                faceTypes[sides] = 0;
            faceTypes[sides]++;
        }

        foreach (var kvp in faceTypes.OrderBy(x => x.Key))
        {
            string faceName = GetFaceName(kvp.Key);
            Debug.Log($"  {kvp.Value}x {faceName} ({kvp.Key} sides)");
        }

        // Vertex valence analysis
        Debug.Log("\n--- VERTEX VALENCE ---");
        var valences = CalculateVertexValences(mesh);
        var valenceDistribution = new Dictionary<int, int>();

        foreach (var valence in valences)
        {
            if (!valenceDistribution.ContainsKey(valence))
                valenceDistribution[valence] = 0;
            valenceDistribution[valence]++;
        }

        foreach (var kvp in valenceDistribution.OrderBy(x => x.Key))
        {
            Debug.Log($"  {kvp.Value} vertices with valence {kvp.Key}");
        }

        // PNS compatibility check
        Debug.Log("\n--- PNS COMPATIBILITY ---");
        bool isCompatible = true;

        if (mesh.vertexCount < 4)
        {
            Debug.LogWarning("  x Too few vertices (need at least 4)");
            isCompatible = false;
        }
        else
        {
            Debug.Log($"  v Sufficient vertices ({mesh.vertexCount})");
        }

        if (mesh.faceCount < 1)
        {
            Debug.LogWarning("  x No faces");
            isCompatible = false;
        }
        else
        {
            Debug.Log($"  v Has faces ({mesh.faceCount})");
        }

        // Check for isolated vertices
        var connectedVerts = new HashSet<int>();
        foreach (var face in faces)
        {
            foreach (var idx in face.indexes)
            {
                connectedVerts.Add(idx);
            }
        }

        int isolatedVerts = mesh.vertexCount - connectedVerts.Count;
        if (isolatedVerts > 0)
        {
            Debug.LogWarning($"  ! {isolatedVerts} isolated vertices (not connected to any face)");
        }
        else
        {
            Debug.Log("  v All vertices connected to faces");
        }

        // Check mesh topology type
        Debug.Log("\n--- TOPOLOGY TYPE ---");
        bool allQuads = faceTypes.Count == 1 && faceTypes.ContainsKey(4);
        bool allTris = faceTypes.Count == 1 && faceTypes.ContainsKey(3);
        bool mixed = faceTypes.Count > 1;

        if (allQuads)
        {
            Debug.Log("  * Pure quad mesh (best for PNS)");
        }
        else if (allTris)
        {
            Debug.Log("  * Pure triangle mesh (may generate fewer patches)");
        }
        else if (mixed)
        {
            Debug.Log("  * Mixed polygon mesh (good for PNS)");
        }

        // PNS requirements explanation
        Debug.Log("\n--- PNS REQUIREMENTS ---");
        Debug.Log("PNS generates smooth surfaces for subdivision-style topology:");
        Debug.Log("  * Works best with quad-dominant meshes");
        Debug.Log("  * Needs well-connected vertices (valence 3-6)");
        Debug.Log("  * Simple primitives (single face) may not generate patches");
        Debug.Log("  * Minimum complexity: ~6 vertices, 4+ faces");

        // Try to generate PnSpline and report
        Debug.Log("\n--- PNS GENERATION TEST ---");
        try
        {
            var spline = model.GetPnSpline();
            if (spline.NumPatches == 0)
            {
                Debug.LogWarning($"  x Generated 0 patches");
                Debug.LogWarning("  Possible reasons:");
                Debug.LogWarning("    - Mesh is too simple (e.g., single quad)");
                Debug.LogWarning("    - Topology not recognized by PNS");
                Debug.LogWarning("    - All faces are degenerate");
                Debug.LogWarning("  Suggestion: Add more subdivisions or complexity");
            }
            else
            {
                Debug.Log($"  v Generated {spline.NumPatches} patches successfully!");

                // Sample first few patches
                int samplesToShow = Mathf.Min(5, (int)spline.NumPatches);
                Debug.Log($"\n  First {samplesToShow} patches:");
                for (uint i = 0; i < samplesToShow; i++)
                {
                    var patch = spline.GetPatch(i);
                    Debug.Log($"    Patch {i}: U-degree={patch.DegreeU}, V-degree={patch.DegreeV}, Valid={patch.IsValid}");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"  x Failed to generate PnSpline: {e.Message}");
        }

        // Summary
        Debug.Log("\n=======================================");
        if (isCompatible)
        {
            Debug.Log("VERDICT: Mesh appears compatible with PNS");
        }
        else
        {
            Debug.LogWarning("VERDICT: Mesh may not be compatible with PNS");
        }
        Debug.Log("=======================================\n");
    }

    private string GetFaceName(int sides)
    {
        switch (sides)
        {
            case 3: return "Triangles";
            case 4: return "Quads";
            case 5: return "Pentagons";
            case 6: return "Hexagons";
            default: return $"{sides}-gons";
        }
    }

    private List<int> CalculateVertexValences(ProBuilderMesh mesh)
    {
        var valences = new int[mesh.vertexCount];

        foreach (var face in mesh.faces)
        {
            foreach (var idx in face.indexes)
            {
                valences[idx]++;
            }
        }

        return valences.ToList();
    }

    [ContextMenu("Analyze All Models")]
    public void AnalyzeAllModels()
    {
        var allModels = ModelsManager.Instance.GetAllModelsInScene();

        Debug.Log($"\n+=======================================+");
        Debug.Log($"| ANALYZING {allModels.Count} MODELS");
        Debug.Log($"+=======================================+\n");

        foreach (var model in allModels)
        {
            modelObject = model.gameObject;
            AnalyzeMesh();
        }
    }

    [ContextMenu("Show Simple Fix")]
    public void ShowSimpleFix()
    {
        Debug.Log("\n+=======================================+");
        Debug.Log("| HOW TO FIX 0 PATCHES ISSUE");
        Debug.Log("+=======================================+");
        Debug.Log("");
        Debug.Log("If your model generates 0 patches, try:");
        Debug.Log("");
        Debug.Log("1. ADD MORE SUBDIVISIONS");
        Debug.Log("    - In your model creator, increase subdivision slider");
        Debug.Log("    - PNS needs at least 4-6 faces minimum");
        Debug.Log("");
        Debug.Log("2. USE QUAD-BASED PRIMITIVES");
        Debug.Log("    - Cubes work well (6 quad faces)");
        Debug.Log("    - Spheres with subdivisions work well");
        Debug.Log("    - Single triangles/quads won't generate patches");
        Debug.Log("");
        Debug.Log("3. AVOID VERY SIMPLE GEOMETRY");
        Debug.Log("    - Single plane: 0 patches");
        Debug.Log("    - Subdivided plane (2x2): May generate patches");
        Debug.Log("    - Cube: Generates patches");
        Debug.Log("");
        Debug.Log("4. CHECK YOUR SUBDIVISION LEVEL");
        Debug.Log("    - The SubD slider in your console log");
        Debug.Log("    - Try increasing it for more geometry");
        Debug.Log("");
    }
}