using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

/// <summary>
/// Represents an interactive control point for a PnS patch.
/// Allows direct manipulation of PnS Bezier control points in VR.
/// </summary>
public class PNSControlPoint : MonoBehaviour
{
    [Header("Control Point Data")]
    public ModelData parentModel;
    public uint patchIndex;
    public uint uIndex;
    public uint vIndex;

    private Vector3 previousPosition;
    private bool isGrabbing;
    private bool isSelected;

    private MeshRenderer meshRenderer;
    private Rigidbody rb;
    private XRGrabInteractable grabInteractable;
    private XRGeneralGrabTransformer grabTransformer;

    /// <summary>
    /// Initialize the PnS control point
    /// </summary>
    public void Init(ModelData model, uint patchIdx, uint u, uint v, Vector3 worldPosition)
    {
        parentModel = model;
        patchIndex = patchIdx;
        uIndex = u;
        vIndex = v;

        transform.position = worldPosition;
        transform.SetParent(model.transform);

        SetupComponents();
    }

    /// <summary>
    /// Setup VR interaction components
    /// </summary>
    private void SetupComponents()
    {
        // Setup renderer
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }

        // Setup rigidbody for XR interaction
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;
        rb.useGravity = false;

        // Setup XR grab interaction
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable == null)
        {
            grabInteractable = gameObject.AddComponent<XRGrabInteractable>();
        }

        grabTransformer = GetComponent<XRGeneralGrabTransformer>();
        if (grabTransformer == null)
        {
            grabTransformer = gameObject.AddComponent<XRGeneralGrabTransformer>();
        }

        if (grabInteractable != null)
        {
            grabInteractable.trackPosition = true;
            grabInteractable.trackRotation = false;
            grabInteractable.trackScale = false;
            grabInteractable.throwOnDetach = false;

            // Register event listeners
            grabInteractable.selectEntered.AddListener(OnGrabStart);
            grabInteractable.selectExited.AddListener(OnGrabEnd);
        }

        // Subscribe to view settings (only if ViewManager exists)
        if (ViewManager.Instance != null)
        {
            ViewManager.Instance.OnControlPointSizeChanged.AddListener(OnSizeChanged);
            OnSizeChanged(ViewManager.Instance.GetControlPointSize());
        }
        else
        {
            // Default size if no ViewManager
            transform.localScale = Vector3.one * 0.05f;
        }

        UpdateVisuals();
    }

    /// <summary>
    /// Update control point visuals based on selection state
    /// </summary>
    private void UpdateVisuals()
    {
        if (meshRenderer == null || ViewManager.Instance == null) return;

        meshRenderer.material = isSelected
            ? ViewManager.Instance.GetSelectedControlPointMaterial()
            : ViewManager.Instance.GetUnselectedControlPointMaterial();
    }

    /// <summary>
    /// Handle size changes from ViewManager
    /// </summary>
    private void OnSizeChanged(float size)
    {
        transform.localScale = Vector3.one * size;
    }

    /// <summary>
    /// Select this control point
    /// </summary>
    public void Select()
    {
        isSelected = true;
        UpdateVisuals();
    }

    /// <summary>
    /// Deselect this control point
    /// </summary>
    public void Deselect()
    {
        isSelected = false;
        UpdateVisuals();
    }

    /// <summary>
    /// Called when user grabs this control point
    /// </summary>
    private void OnGrabStart(UnityEngine.XR.Interaction.Toolkit.SelectEnterEventArgs args)
    {
        previousPosition = transform.position;
        isGrabbing = true;

        // Notify the PnS editing panel
        if (PNSEditingPanel.Instance != null)
        {
            PNSEditingPanel.Instance.OnControlPointGrabbed(this);
        }
    }

    /// <summary>
    /// Called when user releases this control point
    /// </summary>
    private void OnGrabEnd(UnityEngine.XR.Interaction.Toolkit.SelectExitEventArgs args)
    {
        isGrabbing = false;

        // Notify the PnS editing panel
        if (PNSEditingPanel.Instance != null)
        {
            PNSEditingPanel.Instance.OnControlPointReleased(this);
        }
    }

    /// <summary>
    /// Update loop - propagate position changes to PnS
    /// </summary>
    private void Update()
    {
        if (isGrabbing && Vector3.Distance(transform.position, previousPosition) > 0.0001f)
        {
            // Notify about movement
            if (PNSEditingPanel.Instance != null)
            {
                PNSEditingPanel.Instance.OnControlPointMoved(this, transform.position);
            }

            previousPosition = transform.position;
        }
    }

    /// <summary>
    /// Update position from PnS patch data
    /// </summary>
    public void UpdatePositionFromPatch()
    {
        if (parentModel == null || PNSModelIntegration.Instance == null) return;

        var spline = PNSModelIntegration.Instance.GetCachedPnSpline(parentModel);
        if (spline == null || patchIndex >= spline.NumPatches) return;

        using (var patch = spline.GetPatch(patchIndex))
        {
            if (patch.IsValid && uIndex <= patch.DegreeU && vIndex <= patch.DegreeV)
            {
                double x = patch[uIndex, vIndex, 0];
                double y = patch[uIndex, vIndex, 1];
                double z = patch[uIndex, vIndex, 2];

                transform.position = new Vector3((float)x, (float)y, (float)z);
            }
        }
    }

    /// <summary>
    /// Deactivate and return to pool
    /// </summary>
    public void Deactivate()
    {
        isSelected = false;
        isGrabbing = false;
        UpdateVisuals();
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabStart);
            grabInteractable.selectExited.RemoveListener(OnGrabEnd);
        }

        if (ViewManager.Instance != null)
        {
            ViewManager.Instance.OnControlPointSizeChanged.RemoveListener(OnSizeChanged);
        }
    }
}