using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.ProBuilder;

public class PNSControlPointChangedEvent : UnityEvent { }

/// <summary>
/// Manages PnS control point editing in VR.
/// Creates interactive control points for PnS patches and handles their manipulation.
/// Prioritizes QuadTopologyData to ensure imported meshes work correctly.
/// </summary>
public class PNSEditingPanel : MonoBehaviour
{
    // Singleton
    public static PNSEditingPanel Instance { get; private set; }

    [Header("Control Point Settings")]
    [Tooltip("Prefab for PnS control points (should have a sphere mesh)")]
    [SerializeField] private GameObject controlPointPrefab;

    [Tooltip("Show control points for all patches or just selected patches")]
    [SerializeField] private bool showAllPatches = true;

    [Tooltip("Update surface in real-time while dragging (more expensive)")]
    [SerializeField] private bool realtimeUpdate = true;

    [Tooltip("Frames between surface updates while dragging")]
    [SerializeField] private int updateThrottleFrames = 3;

    [Tooltip("Samples per patch edge for surface evaluation")]
    [SerializeField] private int samplesPerPatch = 8;

    [Header("Visualization")]
    [Tooltip("Show control net lines between control points")]
    [SerializeField] private bool showControlNet = true;

    [SerializeField] private Material controlNetMaterial;
    [SerializeField] private float controlNetLineWidth = 0.01f;

    // Events
    public PNSControlPointChangedEvent OnPNSControlPointsChanged = new PNSControlPointChangedEvent();

    // State
    private ModelData currentModel;
    private bool isPNSEditMode = false;
    private int frameCounter = 0;

    // Control point pool and tracking
    private List<PNSControlPoint> controlPointPool = new List<PNSControlPoint>();
    private List<PNSControlPoint> activeControlPoints = new List<PNSControlPoint>();
    private List<PNSControlPoint> selectedControlPoints = new List<PNSControlPoint>();
    private Dictionary<uint, List<LineRenderer>> patchLineRenderers = new Dictionary<uint, List<LineRenderer>>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Listen to selection changes
        if (SelectionManager.Instance != null)
        {
            SelectionManager.Instance.OnSelectionChanged.AddListener(OnModelSelectionChanged);
        }
    }

    /// <summary>
    /// Enable PnS editing mode for the selected model
    /// </summary>
    public void EnablePNSEditMode()
    {
        if (SelectionManager.Instance == null) return;

        ModelData selected = SelectionManager.Instance.GetFirstSelected();
        if (selected == null)
        {
            Debug.LogWarning("No model selected for PnS editing");
            return;
        }

        currentModel = selected;
        isPNSEditMode = true;

        // Ensure PnS exists for this model
        if (PNSModelIntegration.Instance != null)
        {
            PNSModelIntegration.Instance.GetOrCreatePnSpline(currentModel);
        }

        CreateControlPoints();
    }

    /// <summary>
    /// Disable PnS editing mode
    /// </summary>
    public void DisablePNSEditMode()
    {
        isPNSEditMode = false;
        ClearControlPoints();
        ClearControlNet();
        currentModel = null;
    }

    /// <summary>
    /// Toggle PnS editing mode
    /// </summary>
    public void TogglePNSEditMode()
    {
        if (isPNSEditMode)
        {
            DisablePNSEditMode();
        }
        else
        {
            EnablePNSEditMode();
        }
    }

    /// <summary>
    /// Create control points.
    /// Checks for QuadTopologyData first to handle imported meshes correctly.
    /// </summary>
    private void CreateControlPoints()
    {
        if (currentModel == null || PNSModelIntegration.Instance == null) return;

        ClearControlPoints();
        ClearControlNet();

        // 1. Try to get Quad Topology (Imported Mesh)
        var quadData = QuadTopologyManager.Instance != null
            ? QuadTopologyManager.Instance.GetQuadTopology(currentModel.modelID)
            : null;

        List<Vector3> controlVerts = new List<Vector3>();
        bool usingQuadTopology = false;

        if (quadData != null)
        {
            // Case A: Imported Mesh with Quad Topology
            // The vertices in quadData are now in World Space
            controlVerts = quadData.vertices;
            usingQuadTopology = true;
            Debug.Log($"[PNS-Edit] Creating control points from QuadTopology ({controlVerts.Count} verts)");
        }
        else
        {
            // Case B: Native ProBuilder Mesh (Primitives)
            var editMesh = currentModel.GetEditModel();
            if (editMesh == null)
            {
                Debug.LogWarning($"No edit mesh available for {currentModel.modelName}");
                return;
            }

            // Convert ProBuilder local verts to World Space
            foreach (var pos in editMesh.positions)
            {
                controlVerts.Add(editMesh.transform.TransformPoint(pos));
            }
            Debug.Log($"[PNS-Edit] Creating control points from ProBuilder Mesh ({controlVerts.Count} verts)");
        }

        // 2. Instantiate Control Points
        for (int i = 0; i < controlVerts.Count; i++)
        {
            PNSControlPoint cp = GetOrCreateControlPoint(i);
            // For QuadTopology, 'i' is the math index. For ProBuilder, 'i' is the visual index.
            // Both map 1:1 to the PnS Spline created by their respective creators.
            cp.Init(currentModel, (uint)i, 0, 0, controlVerts[i]);
            activeControlPoints.Add(cp);
        }

        // 3. Create Visualization
        if (showControlNet)
        {
            CreateControlNetForMesh(quadData);
        }

        OnPNSControlPointsChanged.Invoke();
    }

    /// <summary>
    /// Create visual control net lines.
    /// Uses QuadPolygons if available for cleaner visualization.
    /// </summary>
    private void CreateControlNetForMesh(QuadTopologyManager.QuadTopologyData quadData = null)
    {
        if (!showControlNet || controlNetMaterial == null) return;
        if (currentModel == null) return;

        var lineRenderers = new List<LineRenderer>();
        HashSet<(int, int)> drawnEdges = new HashSet<(int, int)>();

        if (quadData != null)
        {
            // Case A: Draw Quad Topology (Imported)
            // Vertices are already in World Space
            foreach (var poly in quadData.quadPolygons)
            {
                var indices = poly.vertexIndices;
                for (int i = 0; i < indices.Length; i++)
                {
                    int idx1 = indices[i];
                    int idx2 = indices[(i + 1) % indices.Length];

                    // Sort key to avoid drawing edge 1-2 and 2-1 separately
                    var edgeKey = idx1 < idx2 ? (idx1, idx2) : (idx2, idx1);

                    if (!drawnEdges.Contains(edgeKey))
                    {
                        drawnEdges.Add(edgeKey);
                        Vector3[] points = new Vector3[] { quadData.vertices[idx1], quadData.vertices[idx2] };
                        lineRenderers.Add(CreateLineRenderer(points));
                    }
                }
            }
        }
        else
        {
            // Case B: Draw ProBuilder Topology (Primitives)
            var editMesh = currentModel.GetEditModel();
            if (editMesh == null) return;

            var worldVerts = editMesh.VerticesInWorldSpace().ToList();
            var faces = editMesh.faces;

            foreach (var face in faces)
            {
                var indices = face.distinctIndexes;
                for (int i = 0; i < indices.Count; i++)
                {
                    int idx1 = indices[i];
                    int idx2 = indices[(i + 1) % indices.Count];

                    var edgeKey = idx1 < idx2 ? (idx1, idx2) : (idx2, idx1);

                    if (!drawnEdges.Contains(edgeKey))
                    {
                        drawnEdges.Add(edgeKey);
                        Vector3[] points = new Vector3[] { worldVerts[idx1], worldVerts[idx2] };
                        lineRenderers.Add(CreateLineRenderer(points));
                    }
                }
            }
        }

        // Store lines under patch 0 (global bucket)
        patchLineRenderers[0] = lineRenderers;
        Debug.Log($"Created {lineRenderers.Count} control net lines");
    }

    /// <summary>
    /// Get or create a control point from the pool
    /// </summary>
    private PNSControlPoint GetOrCreateControlPoint(int index)
    {
        if (index < controlPointPool.Count)
        {
            var existingCP = controlPointPool[index];
            if (existingCP != null && existingCP.gameObject != null)
            {
                existingCP.gameObject.SetActive(true);
                return existingCP;
            }
            else
            {
                controlPointPool.RemoveAt(index);
            }
        }

        GameObject cpGO;
        if (controlPointPrefab != null)
        {
            cpGO = Instantiate(controlPointPrefab, transform);
        }
        else
        {
            cpGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cpGO.transform.SetParent(transform);
        }

        cpGO.name = $"PNS_CP_{index}";
        var newCP = cpGO.AddComponent<PNSControlPoint>();

        if (index < controlPointPool.Count)
        {
            controlPointPool[index] = newCP;
        }
        else
        {
            while (controlPointPool.Count < index)
            {
                controlPointPool.Add(null);
            }
            controlPointPool.Add(newCP);
        }

        return newCP;
    }

    private LineRenderer CreateLineRenderer(Vector3[] points)
    {
        var go = new GameObject("ControlNetLine");
        go.transform.SetParent(transform);

        var lr = go.AddComponent<LineRenderer>();
        lr.material = controlNetMaterial;
        lr.startWidth = controlNetLineWidth;
        lr.endWidth = controlNetLineWidth;
        lr.positionCount = points.Length;
        lr.SetPositions(points);
        lr.useWorldSpace = true;

        return lr;
    }

    /// <summary>
    /// Called when a control point is grabbed
    /// </summary>
    public void OnControlPointGrabbed(PNSControlPoint cp)
    {
        if (!selectedControlPoints.Contains(cp))
        {
            selectedControlPoints.Add(cp);
            cp.Select();
        }
    }

    /// <summary>
    /// Called when a control point is moved.
    /// Handles QuadTopology data updates correctly.
    /// </summary>
    public void OnControlPointMoved(PNSControlPoint cp, Vector3 newWorldPosition)
    {
        if (currentModel == null || PNSModelIntegration.Instance == null) return;

        // The 'patchIndex' on the CP is actually the VERTEX index in the control mesh
        int vertexIndex = (int)cp.patchIndex;

        // Check if we are using Quad Topology (Imported)
        var quadData = QuadTopologyManager.Instance?.GetQuadTopology(currentModel.modelID);

        if (quadData != null)
        {
            // Case A: Update Quad Data
            if (vertexIndex < quadData.vertices.Count)
            {
                quadData.vertices[vertexIndex] = newWorldPosition;
            }
            // Note: We DO NOT update the ProBuilder mesh here. The indices are mismatched.
            // The visual update relies entirely on ApplyPnSSurfaceToModel below.
        }
        else
        {
            // Case B: Update ProBuilder Mesh (Native Primitive)
            var editMesh = currentModel.GetEditModel();
            if (editMesh != null)
            {
                Vector3 localPosition = editMesh.transform.InverseTransformPoint(newWorldPosition);
                // Update the control mesh vertex
                var positions = editMesh.positions.ToArray();
                if (vertexIndex < positions.Length)
                {
                    positions[vertexIndex] = localPosition;
                    editMesh.positions = positions;
                    editMesh.Refresh();
                }
            }
        }

        // 2. Update the PnSpline Math
        PNSModelIntegration.Instance.UpdatePnSplineWithPositions(
            currentModel,
            new int[] { vertexIndex },
            new Vector3[] { newWorldPosition }
        );

        // 3. Throttled Visual Update
        if (realtimeUpdate)
        {
            frameCounter++;
            if (frameCounter % updateThrottleFrames == 0)
            {
                // Regenerate the smooth surface
                PNSModelIntegration.Instance.ApplyPnSSurfaceToModel(currentModel, samplesPerPatch);

                // Update lines
                if (showControlNet)
                {
                    // Quick dirty update: Clear and redraw lines 
                    // (Optimization: In future, only move lines connected to this vertex)
                    ClearControlNet();
                    CreateControlNetForMesh(quadData);
                }
            }
        }
    }

    /// <summary>
    /// Called when a control point is released
    /// </summary>
    public void OnControlPointReleased(PNSControlPoint cp)
    {
        if (currentModel != null && PNSModelIntegration.Instance != null)
        {
            // Final high-quality update
            PNSModelIntegration.Instance.ApplyPnSSurfaceToModel(currentModel, samplesPerPatch);

            if (showControlNet)
            {
                ClearControlNet();
                var quadData = QuadTopologyManager.Instance?.GetQuadTopology(currentModel.modelID);
                CreateControlNetForMesh(quadData);
            }
        }

        cp.Deselect();
        selectedControlPoints.Remove(cp);
    }

    private void ClearControlPoints()
    {
        foreach (var cp in activeControlPoints)
        {
            if (cp != null) cp.Deactivate();
        }
        activeControlPoints.Clear();
        selectedControlPoints.Clear();
        CleanupControlPointPool();
    }

    private void CleanupControlPointPool()
    {
        for (int i = controlPointPool.Count - 1; i >= 0; i--)
        {
            if (controlPointPool[i] == null || controlPointPool[i].gameObject == null)
            {
                controlPointPool.RemoveAt(i);
            }
        }
    }

    private void ClearControlNet()
    {
        foreach (var kvp in patchLineRenderers)
        {
            foreach (var lr in kvp.Value)
            {
                if (lr != null) Destroy(lr.gameObject);
            }
        }
        patchLineRenderers.Clear();
    }

    // --- Helpers ---

    private void OnModelSelectionChanged(List<ModelData> models)
    {
        if (isPNSEditMode)
        {
            ModelData newSelection = SelectionManager.Instance?.GetFirstSelected();
            if (newSelection != currentModel)
            {
                DisablePNSEditMode();
                if (newSelection != null)
                {
                    EnablePNSEditMode();
                }
            }
        }
    }

    public bool IsPNSEditMode()
    {
        return isPNSEditMode;
    }

    public void SetShowControlNet(bool show)
    {
        showControlNet = show;
        if (isPNSEditMode)
        {
            if (show)
            {
                var quadData = QuadTopologyManager.Instance?.GetQuadTopology(currentModel.modelID);
                CreateControlNetForMesh(quadData);
            }
            else
            {
                ClearControlNet();
            }
        }
    }

    public void SetRealtimeUpdate(bool enabled) => realtimeUpdate = enabled;
    public void SetUpdateThrottleFrames(float frames) => updateThrottleFrames = Mathf.RoundToInt(frames);
    public void SetSamplesPerPatch(float samples) => samplesPerPatch = Mathf.RoundToInt(samples);

    public void OnEditModeToggleChanged(bool enabled)
    {
        if (enabled) EnablePNSEditMode();
        else DisablePNSEditMode();
    }

    public List<PNSControlPoint> GetSelectedControlPoints() => selectedControlPoints;
    public List<PNSControlPoint> GetActiveControlPoints() => activeControlPoints;

    private void OnDestroy()
    {
        ClearControlPoints();
        ClearControlNet();
        if (SelectionManager.Instance != null)
        {
            SelectionManager.Instance.OnSelectionChanged.RemoveListener(OnModelSelectionChanged);
        }
    }
}