using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.ProBuilder;

public class PNSControlPointChangedEvent : UnityEvent { }

/// <summary>
/// Manages PnS control point editing in VR.
/// Creates interactive control points for PnS patches and handles their manipulation.
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
    /// Create control points for the base control mesh (original vertices)
    /// This creates control points only for the mesh vertices that control the PnS surface
    /// </summary>
    private void CreateControlPoints()
    {
        if (currentModel == null || PNSModelIntegration.Instance == null) return;

        ClearControlPoints();
        ClearControlNet();

        var spline = PNSModelIntegration.Instance.GetCachedPnSpline(currentModel);
        if (spline == null || spline.NumPatches == 0)
        {
            Debug.LogWarning($"No PnS patches available for {currentModel.modelName}");
            return;
        }

        // Get the original control mesh vertices (not the Bezier control points)
        var editMesh = currentModel.GetEditModel();
        if (editMesh == null)
        {
            Debug.LogWarning($"No edit mesh available for {currentModel.modelName}");
            return;
        }

        // Get world space positions
        List<Vector3> worldVerts = new List<Vector3>();
        foreach (var pos in editMesh.positions)
        {
            worldVerts.Add(editMesh.transform.TransformPoint(pos));
        }

        // Create control points for each vertex in the control mesh
        for (int i = 0; i < worldVerts.Count; i++)
        {
            PNSControlPoint cp = GetOrCreateControlPoint(i);
            // Store the vertex index as the control point identifier
            cp.Init(currentModel, (uint)i, 0, 0, worldVerts[i]);
            activeControlPoints.Add(cp);
        }

        // Create control net visualization if enabled (shows the mesh edges)
        if (showControlNet)
        {
            CreateControlNetForMesh();
        }

        Debug.Log($"Created {activeControlPoints.Count} PnS control mesh points for {currentModel.modelName}");
        OnPNSControlPointsChanged.Invoke();
    }

    /// <summary>
    /// Get or create a control point from the pool
    /// </summary>
    private PNSControlPoint GetOrCreateControlPoint(int index)
    {
        // Check if we have a pooled control point at this index
        if (index < controlPointPool.Count)
        {
            var existingCP = controlPointPool[index];

            // Check if the pooled control point still exists (wasn't destroyed)
            if (existingCP != null && existingCP.gameObject != null)
            {
                existingCP.gameObject.SetActive(true);
                return existingCP;
            }
            else
            {
                // Control point was destroyed, remove it from pool and create new one
                controlPointPool.RemoveAt(index);
            }
        }

        // Create new control point
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

        // Insert at the correct index to maintain pool ordering
        if (index < controlPointPool.Count)
        {
            controlPointPool[index] = newCP;
        }
        else
        {
            // Pad the pool if necessary
            while (controlPointPool.Count < index)
            {
                controlPointPool.Add(null);
            }
            controlPointPool.Add(newCP);
        }

        return newCP;
    }

    /// <summary>
    /// Create visual control net lines for the mesh (shows mesh edges)
    /// </summary>
    private void CreateControlNetForMesh()
    {
        if (!showControlNet || controlNetMaterial == null) return;
        if (currentModel == null) return;

        var editMesh = currentModel.GetEditModel();
        if (editMesh == null) return;

        var worldVerts = editMesh.VerticesInWorldSpace().ToList();
        var faces = editMesh.faces;

        var lineRenderers = new List<LineRenderer>();

        // Create lines for each edge in the mesh
        HashSet<(int, int)> drawnEdges = new HashSet<(int, int)>();

        foreach (var face in faces)
        {
            var indices = face.distinctIndexes;

            // Draw lines between consecutive vertices in the face
            for (int i = 0; i < indices.Count; i++)
            {
                int idx1 = indices[i];
                int idx2 = indices[(i + 1) % indices.Count];

                // Create edge key (always store smaller index first to avoid duplicates)
                var edgeKey = idx1 < idx2 ? (idx1, idx2) : (idx2, idx1);

                if (!drawnEdges.Contains(edgeKey))
                {
                    drawnEdges.Add(edgeKey);

                    Vector3[] points = new Vector3[2];
                    points[0] = worldVerts[idx1];
                    points[1] = worldVerts[idx2];

                    lineRenderers.Add(CreateLineRenderer(points));
                }
            }
        }

        // Store all line renderers under patch 0
        patchLineRenderers[0] = lineRenderers;

        Debug.Log($"Created {lineRenderers.Count} control net lines");
    }

    /// <summary>
    /// Create a line renderer for control net visualization
    /// </summary>
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
    /// Update control net lines for the mesh
    /// </summary>
    private void UpdateControlNetForMesh()
    {
        if (!showControlNet || !patchLineRenderers.ContainsKey(0)) return;
        if (currentModel == null) return;

        // Simply recreate the control net
        ClearControlNet();
        CreateControlNetForMesh();
    }

    /// <summary>
    /// Clear all control points
    /// </summary>
    private void ClearControlPoints()
    {
        foreach (var cp in activeControlPoints)
        {
            if (cp != null)
            {
                cp.Deactivate();
            }
        }
        activeControlPoints.Clear();
        selectedControlPoints.Clear();

        // Clean up destroyed control points from pool
        CleanupControlPointPool();
    }

    /// <summary>
    /// Remove destroyed control points from the pool
    /// </summary>
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

    /// <summary>
    /// Clear control net visualization
    /// </summary>
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
    /// Called when a control point is moved
    /// </summary>
    public void OnControlPointMoved(PNSControlPoint cp, Vector3 newWorldPosition)
    {
        if (currentModel == null || PNSModelIntegration.Instance == null) return;

        // The patchIndex is now actually the vertex index in the control mesh
        int vertexIndex = (int)cp.patchIndex;

        // First, update the actual control mesh vertex position
        var editMesh = currentModel.GetEditModel();
        if (editMesh != null)
        {
            // Convert world position to local space
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

        // Update the PnSpline with the new vertex position
        uint[] affectedPatches = PNSModelIntegration.Instance.UpdatePnSplineWithPositions(
            currentModel,
            new int[] { vertexIndex },
            new Vector3[] { newWorldPosition }
        );

        // Throttled surface re-evaluation
        if (realtimeUpdate)
        {
            frameCounter++;
            if (frameCounter % updateThrottleFrames == 0)
            {
                PNSModelIntegration.Instance.ApplyPnSSurfaceToModel(currentModel, samplesPerPatch);

                // Update control net
                if (showControlNet)
                {
                    UpdateControlNetForMesh();
                }
            }
        }
    }

    /// <summary>
    /// Called when a control point is released
    /// </summary>
    public void OnControlPointReleased(PNSControlPoint cp)
    {
        // Final surface update
        if (currentModel != null && PNSModelIntegration.Instance != null)
        {
            PNSModelIntegration.Instance.ApplyPnSSurfaceToModel(currentModel, samplesPerPatch);

            // Update control net
            if (showControlNet)
            {
                UpdateControlNetForMesh();
            }
        }

        cp.Deselect();
        selectedControlPoints.Remove(cp);
    }

    /// <summary>
    /// Refresh all control point positions from the current mesh
    /// </summary>
    private void RefreshAllControlPoints()
    {
        if (currentModel == null) return;

        var editMesh = currentModel.GetEditModel();
        if (editMesh == null) return;

        var worldVerts = editMesh.VerticesInWorldSpace().ToList();

        foreach (var cp in activeControlPoints)
        {
            int vertexIndex = (int)cp.patchIndex;
            if (vertexIndex < worldVerts.Count)
            {
                cp.transform.position = worldVerts[vertexIndex];
            }
        }
    }

    /// <summary>
    /// Refresh all control point positions for a specific patch - Not used I'm pretty sure
    /// </summary>
    private void RefreshControlPointsForPatch(uint patchIdx)
    {
        RefreshAllControlPoints();
    }

    /// <summary>
    /// Handle model selection changes
    /// </summary>
    private void OnModelSelectionChanged(List<ModelData> models)
    {
        if (isPNSEditMode)
        {
            // Refresh if selection changed
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

    /// <summary>
    /// Get current PnS edit mode state
    /// </summary>
    public bool IsPNSEditMode()
    {
        return isPNSEditMode;
    }

    /// <summary>
    /// Set whether to show control net
    /// </summary>
    public void SetShowControlNet(bool show)
    {
        showControlNet = show;

        if (isPNSEditMode)
        {
            if (show)
            {
                CreateControlNetForMesh();
            }
            else
            {
                ClearControlNet();
            }
        }
    }

    /// <summary>
    /// Set realtime update mode
    /// </summary>
    public void SetRealtimeUpdate(bool enabled)
    {
        realtimeUpdate = enabled;
    }

    /// <summary>
    /// Set the throttle frames (how often to update while dragging)
    /// </summary>
    public void SetUpdateThrottleFrames(float frames)
    {
        updateThrottleFrames = Mathf.RoundToInt(frames);
    }

    /// <summary>
    /// Set samples per patch for surface quality
    /// </summary>
    public void SetSamplesPerPatch(float samples)
    {
        samplesPerPatch = Mathf.RoundToInt(samples);
    }

    /// <summary>
    /// Called by UI toggle - enables or disables edit mode based on toggle state
    /// </summary>
    public void OnEditModeToggleChanged(bool enabled)
    {
        if (enabled)
        {
            EnablePNSEditMode();
        }
        else
        {
            DisablePNSEditMode();
        }
    }

    /// <summary>
    /// Get selected control points
    /// </summary>
    public List<PNSControlPoint> GetSelectedControlPoints()
    {
        return selectedControlPoints;
    }

    /// <summary>
    /// Get all active control points
    /// </summary>
    public List<PNSControlPoint> GetActiveControlPoints()
    {
        return activeControlPoints;
    }

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