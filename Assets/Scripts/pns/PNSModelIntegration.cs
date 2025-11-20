using System;
using System.Collections.Generic;
using System.Linq;
using Stopwatch = System.Diagnostics.Stopwatch;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.Profiling;
using PolyhedralNetSplines;

/// <summary>
/// Manages integration between Unity ProBuilder meshes and Polyhedral Net Splines library
/// Handles conversion, caching, and updating of PnSpline objects
/// </summary>
public class PNSModelIntegration : MonoBehaviour
{
    /// <summary>
    /// Singleton Pattern
    /// </summary>
    public static PNSModelIntegration Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Cache of PnSpline objects for each model
    private Dictionary<int, PnSpline> pnSplineCache = new Dictionary<int, PnSpline>();

    // Track which vertices have been modified for efficient updates
    private Dictionary<int, HashSet<int>> dirtyVertices = new Dictionary<int, HashSet<int>>();

    /// <summary>
    /// Configuration for PNS generation
    /// </summary>
    [Serializable]
    public class PNSConfig
    {
        [Header("PNS behavior")]
        public bool degreeRaise = true;          // Raise degree to 3 by default
        public bool autoUpdateOnEdit = true;     // Automatically update PnSpline when mesh changes
        public bool cacheSplines = true;         // Cache PnSplines for performance

        [Header("Timing / Logging")]
        public bool enableTiming = true;         // Print ms timings to Console
        public bool showGCAllocs = true;         // Include GC alloc deltas in timing
        public bool enableProfilerSamples = false; // Wrap heavy sections with Profiler.Begin/EndSample
        public LogVerbosity logLevel = LogVerbosity.Info;
    }

    public enum LogVerbosity { ErrorsOnly = 0, Warnings = 1, Info = 2, Verbose = 3 }

    public PNSConfig config = new PNSConfig();

    private void Start()
    {
        // Subscribe to model changes if needed
        if (ModelsManager.Instance != null)
        {
            ModelsManager.Instance.OnModelsChanged.AddListener(OnModelsChanged);
        }
    }

    private void OnModelsChanged(List<ModelData> models)
    {
        // Clean up cache for removed models
        var currentModelIDs = new HashSet<int>(models.Select(m => m.modelID));
        var cachedIDs = pnSplineCache.Keys.ToList();

        foreach (var id in cachedIDs)
        {
            if (!currentModelIDs.Contains(id))
            {
                RemovePnSplineFromCache(id);
            }
        }
    }

    #region Timing & Logging Helpers

    private struct ScopeTimer : IDisposable
    {
        private readonly string name;
        private readonly bool enabled;
        private readonly bool showGC;
        private readonly bool profiler;
        private readonly LogVerbosity level;
        private readonly PNSModelIntegration owner;
        private readonly Stopwatch sw;
        private readonly long gcBefore;

        public ScopeTimer(string name, PNSModelIntegration owner, LogVerbosity levelOverride = LogVerbosity.Info)
        {
            this.name = name;
            this.owner = owner;
            this.enabled = owner != null && owner.config.enableTiming;
            this.showGC = owner != null && owner.config.showGCAllocs;
            this.profiler = owner != null && owner.config.enableProfilerSamples;
            this.level = (owner != null) ? owner.config.logLevel : LogVerbosity.Info;

            if (profiler) Profiler.BeginSample(name);

            sw = enabled ? Stopwatch.StartNew() : null;
            gcBefore = (enabled && showGC) ? Profiler.GetTotalAllocatedMemoryLong() : 0;
        }

        public void Dispose()
        {
            if (sw != null)
            {
                sw.Stop();
                long gcAfter = showGC ? Profiler.GetTotalAllocatedMemoryLong() : 0;
                double ms = sw.Elapsed.TotalMilliseconds;
                string gc = showGC ? $" | GC {(gcAfter - gcBefore) / 1024f:0.0} KB" : "";
                if (owner != null && owner.config.logLevel >= level)
                {
                    Debug.Log($"[PNS] {name} took {ms:0.3} ms{gc}");
                }
            }
            if (profiler) Profiler.EndSample();
        }
    }

    private bool LogEnabled(LogVerbosity v) => config.logLevel >= v;

    private void LogInfo(string msg)
    {
        if (LogEnabled(LogVerbosity.Info)) Debug.Log("[PNS] " + msg);
    }

    private void LogWarn(string msg)
    {
        if (LogEnabled(LogVerbosity.Warnings)) Debug.LogWarning("[PNS] " + msg);
    }

    private void LogVerbose(string msg)
    {
        if (LogEnabled(LogVerbosity.Verbose)) Debug.Log("[PNS-VERBOSE] " + msg);
    }

    #endregion

    #region Conversion Methods

    /// <summary>
    /// Convert ProBuilderMesh to PNS control net data format
    /// </summary>
    public (double[,] vertices, int[][] faces) ProBuilderToPNSData(ProBuilderMesh mesh)
    {
        if (mesh == null)
        {
            throw new ArgumentNullException(nameof(mesh));
        }

        using (new ScopeTimer("Convert ProBuilder -> PNS data", this, LogVerbosity.Info))
        {
            // Get vertices in world space and convert to list for Count property
            var worldVertsList = mesh.VerticesInWorldSpace().ToList();

            // Convert to double array [vertexCount, 3]
            double[,] vertices = new double[worldVertsList.Count, 3];
            for (int i = 0; i < worldVertsList.Count; i++)
            {
                vertices[i, 0] = worldVertsList[i].x;
                vertices[i, 1] = worldVertsList[i].y;
                vertices[i, 2] = worldVertsList[i].z;
            }

            // Convert faces
            var faces = mesh.faces;
            int[][] faceIndices = new int[faces.Count][];

            for (int i = 0; i < faces.Count; i++)
            {
                var faceIndexes = faces[i].indexes;
                faceIndices[i] = new int[faceIndexes.Count];
                for (int j = 0; j < faceIndexes.Count; j++)
                {
                    faceIndices[i][j] = faceIndexes[j];
                }
            }

            LogVerbose($"Converted verts={worldVertsList.Count}, faces={faces.Count}");
            return (vertices, faceIndices);
        }
    }

    /// <summary>
    /// Convert ModelData to PNS control net format
    /// </summary>
    public (double[,] vertices, int[][] faces) ModelDataToPNSData(ModelData model)
    {
        return ProBuilderToPNSData(model.GetEditModel());
    }

    #endregion

    #region PnSpline Creation and Management

    /// <summary>
    /// Create or update PnSpline for a model
    /// </summary>
    public PnSpline GetOrCreatePnSpline(ModelData model, bool forceRecreate = false)
    {
        if (model == null)
        {
            throw new ArgumentNullException(nameof(model));
        }

        if (!forceRecreate && config.cacheSplines && pnSplineCache.ContainsKey(model.modelID))
        {
            return pnSplineCache[model.modelID];
        }

        // Validate mesh before creating PnSpline
        var (vertices, faces) = ModelDataToPNSData(model);

        if (vertices.GetLength(0) < 3)
        {
            LogWarn($"Model {model.modelName} has too few vertices ({vertices.GetLength(0)}) for PnSpline generation");
        }

        if (faces.Length < 1)
        {
            LogWarn($"Model {model.modelName} has no faces for PnSpline generation");
        }

        using (new ScopeTimer($"PnSpline ctor ({model.modelName})", this, LogVerbosity.Info))
        {
            // Create new PnSpline
            PnSpline spline = new PnSpline(vertices, faces, config.degreeRaise);

            // Warn if no patches were generated
            if (spline.NumPatches == 0)
            {
                LogWarn($"PnSpline for {model.modelName} has 0 patches. " +
                        $"Mesh may be too simple or have incompatible topology. " +
                        $"Vertices: {vertices.GetLength(0)}, Faces: {faces.Length}. " +
                        $"PNS requires more complex geometry (typically 4+ faces).");
            }

            // Cache it (even if 0 patches, to avoid repeated warnings)
            if (config.cacheSplines)
            {
                if (pnSplineCache.ContainsKey(model.modelID))
                {
                    pnSplineCache[model.modelID]?.Dispose();
                }
                pnSplineCache[model.modelID] = spline;

                if (!dirtyVertices.ContainsKey(model.modelID))
                {
                    dirtyVertices[model.modelID] = new HashSet<int>();
                }
            }

            LogInfo($"Built PnSpline for '{model.modelName}' | patches={spline.NumPatches} | degreeRaise={config.degreeRaise}");
            return spline;
        }
    }

    /// <summary>
    /// Efficiently update existing PnSpline when vertices change
    /// Only regenerates affected patches
    /// </summary>
    public uint[] UpdatePnSpline(ModelData model, int[] modifiedVertexIndices)
    {
        if (model == null)
        {
            throw new ArgumentNullException(nameof(model));
        }

        // Check if model has cached PnSpline
        if (!pnSplineCache.ContainsKey(model.modelID))
        {
            LogWarn($"No cached PnSpline for model {model.modelName} (ID: {model.modelID}). Creating new one.");
            GetOrCreatePnSpline(model);

            // If still not in cache (if caching disabled), return empty
            if (!pnSplineCache.ContainsKey(model.modelID))
            {
                return Array.Empty<uint>();
            }
        }

        PnSpline spline = pnSplineCache[model.modelID];

        // Check if spline has patches
        if (spline.NumPatches == 0)
        {
            LogWarn($"PnSpline for {model.modelName} has 0 patches. Cannot update vertices. " +
                    "Model geometry may be too simple for PNS.");
            return Array.Empty<uint>();
        }

        var mesh = model.GetEditModel();
        var worldVertsList = mesh.VerticesInWorldSpace().ToList();

        // Prepare updated vertices
        double[,] updatedVertices = new double[modifiedVertexIndices.Length, 3];
        uint[] updateIndices = new uint[modifiedVertexIndices.Length];

        for (int i = 0; i < modifiedVertexIndices.Length; i++)
        {
            int vertIndex = modifiedVertexIndices[i];
            if (vertIndex < worldVertsList.Count)
            {
                updatedVertices[i, 0] = worldVertsList[vertIndex].x;
                updatedVertices[i, 1] = worldVertsList[vertIndex].y;
                updatedVertices[i, 2] = worldVertsList[vertIndex].z;
                updateIndices[i] = (uint)vertIndex;
            }
        }

        using (new ScopeTimer($"PnSpline.UpdateControlMesh ({model.modelName}) n={updateIndices.Length}", this, LogVerbosity.Info))
        {
            // Update the control mesh - returns indices of affected patches
            uint[] affectedPatches = spline.UpdateControlMesh(updatedVertices, updateIndices);

            LogInfo($"Updated {affectedPatches.Length} patches for model {model.modelName} (nVerts={updateIndices.Length})");
            return affectedPatches;
        }
    }

    /// <summary>
    /// Update PnSpline with explicit world positions (for control point manipulation)
    /// </summary>
    public uint[] UpdatePnSplineWithPositions(ModelData model, int[] modifiedVertexIndices, Vector3[] worldPositions)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));
        if (modifiedVertexIndices == null || worldPositions == null)
            return Array.Empty<uint>();
        if (modifiedVertexIndices.Length != worldPositions.Length)
            throw new ArgumentException("indices and positions length mismatch");

        // Ensure spline exists
        if (!pnSplineCache.ContainsKey(model.modelID))
            GetOrCreatePnSpline(model);
        if (!pnSplineCache.ContainsKey(model.modelID))
            return Array.Empty<uint>();

        var spline = pnSplineCache[model.modelID];
        if (spline.NumPatches == 0)
        {
            LogWarn($"PnSpline for {model.modelName} has 0 patches. Cannot update vertices.");
            return Array.Empty<uint>();
        }

        // Build native payload
        double[,] updated = new double[modifiedVertexIndices.Length, 3];
        uint[] indices = new uint[modifiedVertexIndices.Length];

        for (int i = 0; i < modifiedVertexIndices.Length; i++)
        {
            Vector3 p = worldPositions[i];
            updated[i, 0] = p.x;
            updated[i, 1] = p.y;
            updated[i, 2] = p.z;
            indices[i] = (uint)modifiedVertexIndices[i];
        }

        using (new ScopeTimer($"PnSpline.UpdateControlMesh (explicit) n={indices.Length}", this, LogVerbosity.Info))
        {
            return spline.UpdateControlMesh(updated, indices);
        }
    }

    /// <summary>
    /// Convenience: update a single control point and (optionally) re-evaluate output mesh immediately.
    /// </summary>
    public void UpdateSingleControlPoint(ModelData model, int controlIndex, Vector3 worldPos, bool reEvaluate = true, int? samples = null)
    {
        var changed = UpdatePnSplineWithPositions(model,
            new[] { controlIndex },
            new[] { worldPos });

        LogVerbose($"Control point {controlIndex} updated; affected patches={changed.Length}");

        if (reEvaluate)
            ApplyPnSSurfaceToModel(model, samples);
    }

    /// <summary>
    /// Mark vertices as dirty for batch updating later
    /// </summary>
    public void MarkVerticesDirty(ModelData model, params int[] vertexIndices)
    {
        if (!dirtyVertices.ContainsKey(model.modelID))
        {
            dirtyVertices[model.modelID] = new HashSet<int>();
        }

        foreach (int idx in vertexIndices)
        {
            dirtyVertices[model.modelID].Add(idx);
        }
    }

    /// <summary>
    /// Apply all dirty vertex updates for a model
    /// </summary>
    public uint[] FlushDirtyVertices(ModelData model)
    {
        if (!dirtyVertices.ContainsKey(model.modelID) || dirtyVertices[model.modelID].Count == 0)
        {
            return Array.Empty<uint>();
        }

        int[] dirtyIndices = dirtyVertices[model.modelID].ToArray();
        dirtyVertices[model.modelID].Clear();

        return UpdatePnSpline(model, dirtyIndices);
    }

    /// <summary>
    /// Get cached PnSpline for a model (null if not cached)
    /// </summary>
    public PnSpline GetCachedPnSpline(ModelData model)
    {
        if (model == null || !pnSplineCache.ContainsKey(model.modelID))
        {
            return null;
        }
        return pnSplineCache[model.modelID];
    }

    /// <summary>
    /// Remove PnSpline from cache
    /// </summary>
    public void RemovePnSplineFromCache(int modelID)
    {
        if (pnSplineCache.ContainsKey(modelID))
        {
            pnSplineCache[modelID]?.Dispose();
            pnSplineCache.Remove(modelID);
        }

        if (dirtyVertices.ContainsKey(modelID))
        {
            dirtyVertices.Remove(modelID);
        }
    }

    #endregion

    #region Export Methods

    public void ExportToBV(ModelData model, string outputPath, bool degreeRaise = true)
    {
        try
        {
            using (new ScopeTimer($"Export BV ({model.modelName})", this, LogVerbosity.Info))
            {
                var (vertices, faces) = ModelDataToPNSData(model);

                // Convert double[,] to float[,] for PNSControlMesh
                float[,] floatVertices = new float[vertices.GetLength(0), 3];
                for (int i = 0; i < vertices.GetLength(0); i++)
                {
                    floatVertices[i, 0] = (float)vertices[i, 0];
                    floatVertices[i, 1] = (float)vertices[i, 1];
                    floatVertices[i, 2] = (float)vertices[i, 2];
                }

                using (new ScopeTimer("  PNSControlMesh.FromData", this, LogVerbosity.Verbose))
                {
                    var controlMesh = PNSControlMesh.FromData(floatVertices, faces);
                    using (new ScopeTimer("  PNS.CreateBV", this, LogVerbosity.Verbose))
                    {
                        PNS.CreateBV(controlMesh, outputPath, degreeRaise);
                    }
                    controlMesh.Dispose();
                }

                LogInfo($"Exported {model.modelName} to {outputPath}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[PNS] Failed to export model to BV: {e.Message}");
        }
    }

    public void ExportToIGS(ModelData model, string outputPath, bool degreeRaise = true)
    {
        try
        {
            using (new ScopeTimer($"Export IGS ({model.modelName})", this, LogVerbosity.Info))
            {
                var (vertices, faces) = ModelDataToPNSData(model);

                float[,] floatVertices = new float[vertices.GetLength(0), 3];
                for (int i = 0; i < vertices.GetLength(0); i++)
                {
                    floatVertices[i, 0] = (float)vertices[i, 0];
                    floatVertices[i, 1] = (float)vertices[i, 1];
                    floatVertices[i, 2] = (float)vertices[i, 2];
                }

                using (new ScopeTimer("  PNSControlMesh.FromData", this, LogVerbosity.Verbose))
                {
                    var controlMesh = PNSControlMesh.FromData(floatVertices, faces);
                    using (new ScopeTimer("  PNS.CreateIGS", this, LogVerbosity.Verbose))
                    {
                        PNS.CreateIGS(controlMesh, outputPath, degreeRaise);
                    }
                    controlMesh.Dispose();
                }

                LogInfo($"Exported {model.modelName} to {outputPath}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[PNS] Failed to export model to IGS: {e.Message}");
        }
    }

    public void ExportToSTEP(ModelData model, string outputPath, bool degreeRaise = true)
    {
        try
        {
            using (new ScopeTimer($"Export STEP ({model.modelName})", this, LogVerbosity.Info))
            {
                var (vertices, faces) = ModelDataToPNSData(model);

                float[,] floatVertices = new float[vertices.GetLength(0), 3];
                for (int i = 0; i < vertices.GetLength(0); i++)
                {
                    floatVertices[i, 0] = (float)vertices[i, 0];
                    floatVertices[i, 1] = (float)vertices[i, 1];
                    floatVertices[i, 2] = (float)vertices[i, 2];
                }

                using (new ScopeTimer("  PNSControlMesh.FromData", this, LogVerbosity.Verbose))
                {
                    var controlMesh = PNSControlMesh.FromData(floatVertices, faces);
                    using (new ScopeTimer("  PNS.CreateSTEP", this, LogVerbosity.Verbose))
                    {
                        PNS.CreateSTEP(controlMesh, outputPath, degreeRaise);
                    }
                    controlMesh.Dispose();
                }

                LogInfo($"Exported {model.modelName} to {outputPath}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[PNS] Failed to export model to STEP: {e.Message}");
        }
    }

    public void ExportAllModels(string directory, string format = "bv", bool degreeRaise = true)
    {
        var models = ModelsManager.Instance.GetAllModelsInScene();

        using (new ScopeTimer($"ExportAll format={format} count={models.Count}", this, LogVerbosity.Info))
        {
            foreach (var model in models)
            {
                string filename = $"{model.modelName}.{format}";
                string fullPath = System.IO.Path.Combine(directory, filename);

                switch (format.ToLower())
                {
                    case "bv":
                        ExportToBV(model, fullPath, degreeRaise);
                        break;
                    case "igs":
                        ExportToIGS(model, fullPath, degreeRaise);
                        break;
                    case "step":
                        ExportToSTEP(model, fullPath, degreeRaise);
                        break;
                    default:
                        LogWarn($"Unknown format: {format}");
                        break;
                }
            }
        }
    }

    #endregion

    #region Patch Access and Visualization

    /// <summary>
    /// Get all patches from a model's PnSpline
    /// </summary>
    public List<PnSPatch> GetPatches(ModelData model)
    {
        var spline = GetCachedPnSpline(model);
        if (spline == null)
        {
            spline = GetOrCreatePnSpline(model);
        }

        using (new ScopeTimer($"GetPatches ({model.modelName})", this, LogVerbosity.Verbose))
        {
            List<PnSPatch> patches = new List<PnSPatch>();
            for (uint i = 0; i < spline.NumPatches; i++)
            {
                patches.Add(spline.GetPatch(i));
            }
            LogVerbose($"Fetched {patches.Count} patches");
            return patches;
        }
    }

    /// <summary>
    /// Get Bezier control points from a specific patch
    /// Useful for visualization or further processing
    /// </summary>
    public List<Vector3> GetPatchControlPoints(PnSPatch patch)
    {
        using (new ScopeTimer("GetPatchControlPoints", this, LogVerbosity.Verbose))
        {
            List<Vector3> controlPoints = new List<Vector3>();

            for (uint u = 0; u <= patch.DegreeU; u++)
            {
                for (uint v = 0; v <= patch.DegreeV; v++)
                {
                    double x = patch[u, v, 0];
                    double y = patch[u, v, 1];
                    double z = patch[u, v, 2];
                    controlPoints.Add(new Vector3((float)x, (float)y, (float)z));
                }
            }

            return controlPoints;
        }
    }

    #endregion

    #region Mesh Evaluation / Apply to Model

    [Header("PNS Mesh Evaluation")]
    [Tooltip("Number of samples per patch edge when evaluating the PnS surface.")]
    public int defaultSamplesPerPatch = 8;

    /// <summary>
    /// Evaluate the PnSpline for this model and assign the smooth PnS surface to the display MeshFilter.
    /// </summary>
    public void ApplyPnSSurfaceToModel(ModelData model, int? overrideSamplesPerPatch = null)
    {
        if (model == null)
        {
            Debug.LogWarning("PNS Apply: model is null.");
            return;
        }

        var editModel = model.GetEditModel();
        if (editModel == null)
        {
            Debug.LogWarning($"PNS Apply: model {model.modelName} has no edit mesh.");
            return;
        }

        int samples = Mathf.Max(1, overrideSamplesPerPatch ?? defaultSamplesPerPatch);

        using (new ScopeTimer($"ApplyPnSSurface ({model.modelName}) samples={samples}", this, LogVerbosity.Info))
        {
            // Get or create PnSpline for this model
            PnSpline spline = GetOrCreatePnSpline(model);
            if (spline == null || spline.NumPatches == 0)
            {
                LogWarn($"PNS Apply: PnSpline not available or empty for {model.modelName}.");
                return;
            }

            // Accumulate evaluated geometry
            var positions = new List<Vector3>();
            var triangles = new List<int>();

            using (new ScopeTimer("  Evaluate patches", this, LogVerbosity.Info))
            {
                for (uint patchIndex = 0; patchIndex < spline.NumPatches; patchIndex++)
                {
                    using (PnSPatch patch = spline.GetPatch(patchIndex))
                    {
                        if (!patch.IsValid)
                            continue;

                        AppendEvaluatedPatch(patch, samples, positions, triangles);
                    }
                }
            }

            var meshFilter = model.GetMeshFilter();
            if (meshFilter != null)
            {
                var t = meshFilter.transform;
                for (int i = 0; i < positions.Count; i++)
                {
                    positions[i] = t.InverseTransformPoint(positions[i]);
                }
            }

            // Build a Unity Mesh
            Mesh outMesh = new Mesh();
            outMesh.indexFormat = positions.Count > 65535
                ? UnityEngine.Rendering.IndexFormat.UInt32
                : UnityEngine.Rendering.IndexFormat.UInt16;

            using (new ScopeTimer("  Build Unity Mesh", this, LogVerbosity.Info))
            {
                outMesh.SetVertices(positions);
                outMesh.SetTriangles(triangles, 0, true);
                outMesh.RecalculateNormals();
                outMesh.RecalculateBounds();
            }

            using (new ScopeTimer("  Assign to MeshFilter/Collider", this, LogVerbosity.Info))
            {
                // Safety belt: temporarily disable autoUpdateOnEdit in case other listeners react to mesh change
                bool prevAuto = config.autoUpdateOnEdit;
                config.autoUpdateOnEdit = false;
                try
                {
                    if (meshFilter != null)
                    {
                        meshFilter.sharedMesh = outMesh;

                        var collider = meshFilter.GetComponent<MeshCollider>();
                        if (collider != null)
                        {
                            collider.sharedMesh = null;
                            collider.sharedMesh = outMesh;
                        }
                    }
                }
                finally
                {
                    config.autoUpdateOnEdit = prevAuto;
                }
            }

            LogInfo($"PNS Apply: assigned evaluated PnS surface to {model.modelName}. verts={positions.Count}, tris={triangles.Count / 3}");
        }
    }

    /// <summary>
    /// Sample a PnS patch on a regular grid and append vertices and triangle indices.
    /// This evaluates a tensor-product (quad) patch and emits 2 triangles per grid quad.
    /// </summary>
    private void AppendEvaluatedPatch(
        PnSPatch patch,
        int samplesPerEdge,
        List<Vector3> positions,
        List<int> triangles)
    {
        int du = (int)patch.DegreeU;
        int dv = (int)patch.DegreeV;

        if (du < 1 || dv < 1)
            return;

        int resU = Mathf.Max(1, samplesPerEdge);
        int resV = Mathf.Max(1, samplesPerEdge);

        int[,] vertIndex = new int[resU + 1, resV + 1];

        // Generate vertices
        for (int iu = 0; iu <= resU; iu++)
        {
            float u = (float)iu / resU;
            for (int iv = 0; iv <= resV; iv++)
            {
                float v = (float)iv / resV;

                Vector3 p = EvaluatePatchPoint(patch, u, v);

                vertIndex[iu, iv] = positions.Count;
                positions.Add(p);
            }
        }

        // Generate triangles (2 per grid quad)
        for (int iu = 0; iu < resU; iu++)
        {
            for (int iv = 0; iv < resV; iv++)
            {
                int v00 = vertIndex[iu, iv];
                int v10 = vertIndex[iu + 1, iv];
                int v01 = vertIndex[iu, iv + 1];
                int v11 = vertIndex[iu + 1, iv + 1];

                // Triangle 1
                triangles.Add(v00);
                triangles.Add(v10);
                triangles.Add(v01);

                // Triangle 2
                triangles.Add(v10);
                triangles.Add(v11);
                triangles.Add(v01);
            }
        }
    }

    /// <summary>
    /// Evaluate a tensor-product Bezier surface at (u,v) via de Casteljau.
    /// Uses the PnSPatch control net directly.
    /// </summary>
    private Vector3 EvaluatePatchPoint(PnSPatch patch, float u, float v)
    {
        int du = (int)patch.DegreeU;
        int dv = (int)patch.DegreeV;

        // Temporary buffers
        Vector3[] column = new Vector3[dv + 1];
        Vector3[] row = new Vector3[du + 1];

        // First, evaluate in v for each fixed u
        for (int iu = 0; iu <= du; iu++)
        {
            for (int j = 0; j <= dv; j++)
            {
                double x = patch[(uint)iu, (uint)j, 0];
                double y = patch[(uint)iu, (uint)j, 1];
                double z = patch[(uint)iu, (uint)j, 2];
                column[j] = new Vector3((float)x, (float)y, (float)z);
            }

            // de Casteljau along v
            for (int r = 1; r <= dv; r++)
            {
                for (int j = 0; j <= dv - r; j++)
                {
                    column[j] = (1f - v) * column[j] + v * column[j + 1];
                }
            }

            row[iu] = column[0];
        }

        // Then de Casteljau along u on the reduced row[]
        for (int r = 1; r <= du; r++)
        {
            for (int i = 0; i <= du - r; i++)
            {
                row[i] = (1f - u) * row[i] + u * row[i + 1];
            }
        }

        return row[0];
    }

    #endregion

    #region Control Point Access

    /// <summary>
    /// Get the world position of a specific control point in a patch.
    /// Useful for creating interactive control points.
    /// </summary>
    public Vector3? GetPatchControlPointPosition(ModelData model, uint patchIndex, uint uIndex, uint vIndex)
    {
        if (model == null) return null;

        var spline = GetCachedPnSpline(model);
        if (spline == null || patchIndex >= spline.NumPatches) return null;

        using (var patch = spline.GetPatch(patchIndex))
        {
            if (!patch.IsValid || uIndex > patch.DegreeU || vIndex > patch.DegreeV)
                return null;

            double x = patch[uIndex, vIndex, 0];
            double y = patch[uIndex, vIndex, 1];
            double z = patch[uIndex, vIndex, 2];

            return new Vector3((float)x, (float)y, (float)z);
        }
    }

    /// <summary>
    /// Get all control point positions for a specific patch.
    /// Returns a 2D array indexed by [u, v].
    /// </summary>
    public Vector3[,] GetAllPatchControlPoints(ModelData model, uint patchIndex)
    {
        if (model == null) return null;

        var spline = GetCachedPnSpline(model);
        if (spline == null || patchIndex >= spline.NumPatches) return null;

        using (var patch = spline.GetPatch(patchIndex))
        {
            if (!patch.IsValid) return null;

            uint degreeU = patch.DegreeU;
            uint degreeV = patch.DegreeV;

            Vector3[,] controlPoints = new Vector3[degreeU + 1, degreeV + 1];

            for (uint u = 0; u <= degreeU; u++)
            {
                for (uint v = 0; v <= degreeV; v++)
                {
                    double x = patch[u, v, 0];
                    double y = patch[u, v, 1];
                    double z = patch[u, v, 2];
                    controlPoints[u, v] = new Vector3((float)x, (float)y, (float)z);
                }
            }

            return controlPoints;
        }
    }

    /// <summary>
    /// Update a specific control point in a patch.
    /// This directly modifies the PnSpline's Bezier control net.
    /// </summary>
    public bool UpdatePatchControlPoint(ModelData model, uint patchIndex, uint uIndex, uint vIndex, Vector3 worldPosition, bool reEvaluate = false)
    {
        if (model == null) return false;

        var spline = GetCachedPnSpline(model);
        if (spline == null || patchIndex >= spline.NumPatches) return false;

        using (var patch = spline.GetPatch(patchIndex))
        {
            if (!patch.IsValid || uIndex > patch.DegreeU || vIndex > patch.DegreeV)
                return false;

            // This requires a method in the PnSpline library to set individual control points
            // If not available, we need to use UpdateControlMesh with the control point index

            // Calculate linear index for this control point in the global control mesh
            int linearIndex = GetLinearControlPointIndex(model, patchIndex, uIndex, vIndex);
            if (linearIndex < 0) return false;

            // Use the existing update method
            UpdatePnSplineWithPositions(model, new[] { linearIndex }, new[] { worldPosition });

            if (reEvaluate)
            {
                ApplyPnSSurfaceToModel(model);
            }

            return true;
        }
    }

    /// <summary>
    /// Calculate the linear (global) index of a control point given its patch and (u,v) indices.
    /// This maps from (patchIndex, u, v) -> global control point index.
    /// </summary>
    private int GetLinearControlPointIndex(ModelData model, uint patchIndex, uint uIndex, uint vIndex)
    {
        if (model == null) return -1;

        var spline = GetCachedPnSpline(model);
        if (spline == null || patchIndex >= spline.NumPatches) return -1;

        int index = 0;

        // Count all control points in patches before this one
        for (uint p = 0; p < patchIndex; p++)
        {
            using (var patch = spline.GetPatch(p))
            {
                if (patch.IsValid)
                {
                    index += (int)((patch.DegreeU + 1) * (patch.DegreeV + 1));
                }
            }
        }

        // Add the offset within the current patch
        using (var currentPatch = spline.GetPatch(patchIndex))
        {
            if (!currentPatch.IsValid || uIndex > currentPatch.DegreeU || vIndex > currentPatch.DegreeV)
                return -1;

            index += (int)(uIndex * (currentPatch.DegreeV + 1) + vIndex);
        }

        return index;
    }

    /// <summary>
    /// Get the total number of control points across all patches for a model.
    /// </summary>
    public int GetTotalControlPointCount(ModelData model)
    {
        if (model == null) return 0;

        var spline = GetCachedPnSpline(model);
        if (spline == null) return 0;

        int totalCount = 0;

        for (uint i = 0; i < spline.NumPatches; i++)
        {
            using (var patch = spline.GetPatch(i))
            {
                if (patch.IsValid)
                {
                    totalCount += (int)((patch.DegreeU + 1) * (patch.DegreeV + 1));
                }
            }
        }

        return totalCount;
    }

    #endregion

    #region Cache Management
      
    /// <summary>
    /// Manually cache a PnSpline for a model
    /// Used by quad topology system to cache pre-created splines
    /// </summary>
    public void CachePnSpline(int modelID, PnSpline spline)
    {
        if (config.cacheSplines)
        {
            if (pnSplineCache.ContainsKey(modelID))
            {
                pnSplineCache[modelID]?.Dispose();
            }
            pnSplineCache[modelID] = spline;
            LogInfo($"Cached PnSpline for model ID {modelID}");
        }
    }
    #endregion

    #region Cleanup

    private void OnDestroy()
    {
        using (new ScopeTimer("Dispose cached PnSplines", this, LogVerbosity.Info))
        {
            // Dispose all cached splines
            foreach (var spline in pnSplineCache.Values)
            {
                spline?.Dispose();
            }
            pnSplineCache.Clear();
            dirtyVertices.Clear();
        }
    }

    #endregion
}