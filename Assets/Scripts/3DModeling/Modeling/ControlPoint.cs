using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;
using System.Linq;

public class ControlPoint : MonoBehaviour
{
    public EditMode type = EditMode.Vertex;
    [Header("Control Point Data")]
    public int[] vertices;
    public Vector3 normal;

    private bool isBeingGrabbed = false;

    [Header("Visual Feedback")]
    public Material normalMaterial;
    public Material grabbedMaterial;
    public MeshRenderer meshRenderer;
    public Rigidbody rb;
    public XRGrabInteractable grabInteractable;
    public XRGeneralGrabTransformer grabTransformer;




    public Vector3 GetCurrentPosition()
    {
        return transform.position;
    }

    //Creates control point

    public void Initialize(int[] _vertices, Vector3 _normal, EditMode _type = EditMode.Vertex)
    {
        vertices = _vertices;
        type = _type;
        normal = _normal;


        if (rb == null)
        {
            SetupVRInteraction();
            SetupVisualFeedback();
        }
    }


    private void SetupVRInteraction()
    {
        grabInteractable = gameObject.AddComponent<XRGrabInteractable>();
        grabTransformer = gameObject.AddComponent<XRGeneralGrabTransformer>();
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;
        if (grabInteractable != null)
        {
            grabInteractable.trackPosition = false;
            grabInteractable.trackRotation = false;
            grabInteractable.trackScale = false;
            grabInteractable.throwOnDetach = false;
            grabInteractable.selectEntered.AddListener(OnGrabStart);
            //grabInteractable.selectExited.AddListener(OnGrabEnd);
        }

        ViewManager.Instance.OnControlPointSizeChanged.AddListener(ControlPointSizeChanged);
    }

    public void Deactivate()
    {
        ViewManager.Instance.OnControlPointSizeChanged.RemoveListener(ControlPointSizeChanged);
        this.gameObject.SetActive(false);
    }

    private void ControlPointSizeChanged(float size)
    {
        this.transform.localScale = Vector3.one * size;
    }

    private void SetupVisualFeedback()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        SetMaterialToDeselected();
    }

    public void SetMaterialToSelected()
    {
        meshRenderer.material = ViewManager.Instance.GetSelectedControlPointMaterial();
    }
    public void SetMaterialToDeselected()
    {
        meshRenderer.material = ViewManager.Instance.GetUnselectedControlPointMaterial();
    }



    public bool isSelected;
    private void OnGrabStart(UnityEngine.XR.Interaction.Toolkit.SelectEnterEventArgs args)
    {
        if(isSelected == false)
        {
            isSelected = true;
            TransformGizmo.Instance.MultiSelectControlPoint(this);
            SetMaterialToSelected();
        }
        else
        {
            isSelected = false;
            TransformGizmo.Instance.DeSelectControlPoint(this);
            SetMaterialToDeselected();
        }
    }

    public void AddOffsetToControlPointPosition(Vector3 offset)
    {
        this.transform.position += offset;
        ModelEditingPanel.Instance.AddOffsetToVertsPosition(vertices, offset);
    }





    //void Update()
    //{



    //    //Update surface while dragging
    //    //if (isBeingGrabbed && Vector3.Distance(transform.position, originalPosition) > 0.01f)
    //    //{
    //    //    ModelEditingPanel.Instance.OnControlPointMoved(vertices, originalPosition, transform.position);
    //    //    originalPosition = transform.position;
    //    //}
    //}
}