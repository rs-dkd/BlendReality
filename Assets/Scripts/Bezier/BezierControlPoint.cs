using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

public class BezierControlPoint : MonoBehaviour
{
    [Header("Control Point Data")]
    public int surfaceID;      //Surface control point belongs to
    public int uIndex;         //Pos in the control grid (u coord)
    public int vIndex;         //Pos in the control grid (v coord)

    private BezierSurfaceManager surfaceManager;
    private Vector3 originalPosition;
    private bool isBeingGrabbed = false;

    [Header("Visual Feedback")]
    public Material normalMaterial;
    public Material grabbedMaterial;
    public MeshRenderer meshRenderer;

    //Creates control point
    public void Initialize(int _surfaceID, int _uIndex, int _vIndex, BezierSurfaceManager _surfaceManager)
    {
        surfaceID = _surfaceID;
        uIndex = _uIndex;
        vIndex = _vIndex;
        surfaceManager = _surfaceManager;
        originalPosition = transform.position;

        SetupVRInteraction();
        SetupVisualFeedback();
    }

    private void SetupVRInteraction()
    {
        XRGrabInteractable grabInteractable = gameObject.AddComponent<XRGrabInteractable>();
        XRGeneralGrabTransformer grabTransformer = gameObject.AddComponent<XRGeneralGrabTransformer>();
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrabStart);
            grabInteractable.selectExited.AddListener(OnGrabEnd);
        }
    }

    private void SetupVisualFeedback()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            Debug.LogError("No MeshRenderer found on control point! Make sure you're using a primitive sphere.");
            return;
        }
        if (normalMaterial == null)
        {
            Debug.LogWarning("No normal material assigned to control point. Creating default red material.");
            normalMaterial = new Material(Shader.Find("Standard"));
            normalMaterial.color = Color.red;
            normalMaterial.SetFloat("_Metallic", 0.3f);
        }

        if (grabbedMaterial == null)
        {
            Debug.LogWarning("No grabbed material assigned to control point. Creating default yellow material.");
            grabbedMaterial = new Material(Shader.Find("Standard"));
            grabbedMaterial.color = Color.yellow;
            grabbedMaterial.SetFloat("_Metallic", 0.7f);
            grabbedMaterial.SetFloat("_Smoothness", 0.9f);
        }
        if (meshRenderer != null && normalMaterial != null)
        {
            meshRenderer.material = normalMaterial;
        }
    }
    public void SetNormalMaterial()
    {
        if (meshRenderer != null && normalMaterial != null)
        {
            meshRenderer.material = normalMaterial;
        }
    }
    public void UpdateNormalMaterial(Material newNormalMaterial)
    {
        normalMaterial = newNormalMaterial;
        if (!isBeingGrabbed && meshRenderer != null && normalMaterial != null)
        {
            meshRenderer.material = normalMaterial;
        }
    }
    public void UpdateGrabbedMaterial(Material newGrabbedMaterial)
    {
        grabbedMaterial = newGrabbedMaterial;
        if (isBeingGrabbed && meshRenderer != null && grabbedMaterial != null)
        {
            meshRenderer.material = grabbedMaterial;
        }
    }
    private void OnGrabStart(UnityEngine.XR.Interaction.Toolkit.SelectEnterEventArgs args)
    {
        isBeingGrabbed = true;
        if (meshRenderer != null && grabbedMaterial != null)
        {
            meshRenderer.material = grabbedMaterial;
        }
        else if (grabbedMaterial == null)
        {
            Debug.LogWarning($"No grabbed material assigned to control point [{uIndex},{vIndex}]");
        }

        originalPosition = transform.position;

        Debug.Log($"Grabbed control point [{uIndex},{vIndex}] on surface {surfaceID}");
    }
    private void OnGrabEnd(UnityEngine.XR.Interaction.Toolkit.SelectExitEventArgs args)
    {
        isBeingGrabbed = false;
        if (meshRenderer != null && normalMaterial != null)
        {
            meshRenderer.material = normalMaterial;
        }
        else if (normalMaterial == null)
        {
            Debug.LogWarning($"No normal material assigned to control point [{uIndex},{vIndex}]");
        }
        if (surfaceManager != null)
        {
            surfaceManager.OnControlPointMoved(surfaceID, uIndex, vIndex, transform.position);
        }

        Debug.Log($"Released control point [{uIndex},{vIndex}] on surface {surfaceID}");
    }

    void Update()
    {
        //Update surface while dragging
        if (isBeingGrabbed && Vector3.Distance(transform.position, originalPosition) > 0.01f)
        {
            if (surfaceManager != null)
            {
                surfaceManager.OnControlPointMoved(surfaceID, uIndex, vIndex, transform.position);
            }
            originalPosition = transform.position;
        }
    }
}