using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Receives change notifications from ModelEditingPanel and forwards them to PNSModelIntegration
/// for incremental PnSpline updates + throttled surface re-evaluation.
/// </summary>
[DefaultExecutionOrder(50)]
public class PNSInteractionController : MonoBehaviour
{
    [Header("Realtime Surface Refresh")]
    [Tooltip("Re-evaluate the PnS surface every N frames while dragging.")]
    public int reevaluateEveryNFrames = 3;

    [Tooltip("Samples per patch edge when re-evaluating the PnS surface.")]
    public int samplesPerPatch = 8;

    [Tooltip("When true, we push explicit world positions to PnSpline instead of letting the integrator pull from ProBuilder each time.")]
    public bool pushExplicitPositions = true;

    private int _frameCounter;

    // Scratch buffers reused each frame to avoid GC
    private readonly List<int> _movedIndices = new List<int>(256);
    private readonly List<Vector3> _movedWorldPositions = new List<Vector3>(256);

    /// <summary>
    /// Call this while control-points are being dragged to update the native PnSpline.
    /// </summary>
    public void OnControlPointsOffsetApplied(ModelData model, IReadOnlyCollection<int> modifiedVertIndices)
    {
        if (model == null || modifiedVertIndices == null || modifiedVertIndices.Count == 0) return;
        if (PNSModelIntegration.Instance == null) return;

        _movedIndices.Clear();
        _movedWorldPositions.Clear();

        foreach (var idx in modifiedVertIndices)
        {
            _movedIndices.Add(idx);

            if (pushExplicitPositions)
            {
                Vector3 pos = model.GetVerts()[idx];
                _movedWorldPositions.Add(pos);
            }
        }

        if (pushExplicitPositions)
        {
            PNSModelIntegration.Instance.UpdatePnSplineWithPositions(
                model,
                _movedIndices.ToArray(),
                _movedWorldPositions.ToArray());
        }
        else
        {
            PNSModelIntegration.Instance.UpdatePnSpline(
                model,
                _movedIndices.ToArray());
        }

        // Throttled re-evaluation while dragging
        _frameCounter++;
        if (_frameCounter % Mathf.Max(1, reevaluateEveryNFrames) == 0)
        {
            PNSModelIntegration.Instance.ApplyPnSSurfaceToModel(model, samplesPerPatch);
        }
    }

    /// <summary>
    /// Optional finalization hook to re-evaluate the surface once more on grab end.
    /// </summary>
    public void OnGrabEnded(ModelData model)
    {
        if (model == null || PNSModelIntegration.Instance == null) return;
        PNSModelIntegration.Instance.ApplyPnSSurfaceToModel(model, samplesPerPatch);
    }
}