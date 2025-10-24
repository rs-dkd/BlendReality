using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;
using System.Linq;

//public class VertControlPoint : ControlPoint
//{
//    public int[] vertices;

//    public override void Initialize(int[] _vertices, Vector3 _normal)
//    {
//        vertices = _vertices;
//        type = _type;
//        normal = _normal;

//        Setup();

//    }


//}
//public class EdgeControlPoint : ControlPoint
//{
//    public int[] edges;

//}
//public class FaceControlPoint : ControlPoint
//{
//    public int[] faces;

//}
//public class PivotControlPoint : ControlPoint
//{


//}

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

    public virtual void Initialize(int[] _vertices, Vector3 _normal, EditMode _type = EditMode.Vertex)
    {
        vertices = _vertices;
        type = _type;
        normal = _normal;


        Setup();
    }

    public void Setup()
    {
        if (rb == null)
        {
            grabInteractable = gameObject.AddComponent<XRGrabInteractable>();
            grabTransformer = gameObject.AddComponent<XRGeneralGrabTransformer>();
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }
            rb.isKinematic = true;
            grabInteractable.trackScale = false;
            grabInteractable.throwOnDetach = false;
            SetAsSelectedAble();

            grabInteractable.selectEntered.AddListener(OnGrabStart);
            grabInteractable.selectExited.AddListener(OnGrabEnd);
            ViewManager.Instance.OnControlPointSizeChanged.AddListener(ControlPointSizeChanged);

            meshRenderer = GetComponent<MeshRenderer>();
            SetMaterialToDeselected();


            ModelEditingPanel.Instance.OnTransformTypeChanged.AddListener(TransformTypeChanged);
        }
    }


    public void TransformTypeChanged()
    {
        if(ModelEditingPanel.Instance.currentTransformType == TransformType.Free)
        {
            SetAsMoveAble();
        }
        else
        {
            SetAsSelectedAble();
        }
    }
    
    public void SetAsSelectedAble()
    {
        grabInteractable.trackPosition = false;
        grabInteractable.trackRotation = false;
    }
    public void SetAsMoveAble()
    {
        grabInteractable.trackPosition = true;
        grabInteractable.trackRotation = true;
    }


    public void Deactivate()
    {
        ViewManager.Instance.OnControlPointSizeChanged.RemoveListener(ControlPointSizeChanged);
        SetMaterialToDeselected();
        this.gameObject.SetActive(false);
    }

    private void ControlPointSizeChanged(float size)
    {
        this.transform.localScale = Vector3.one * size;
    }



    public void SetMaterialToSelected()
    {
        isSelected = true;
        meshRenderer.material = ViewManager.Instance.GetSelectedControlPointMaterial();
    }
    public void SetMaterialToDeselected()
    {
        isSelected = false;
        meshRenderer.material = ViewManager.Instance.GetUnselectedControlPointMaterial();
    }



    public bool isSelected;
    private void OnGrabStart(UnityEngine.XR.Interaction.Toolkit.SelectEnterEventArgs args)
    {
        isSelectOne = false;
        if (ModelEditingPanel.Instance.currentTransformType == TransformType.Select)
        {
            if (isSelected == false)
            {
                ModelEditingPanel.Instance.MultiSelectControlPoint(this);
            }
            else
            {
                ModelEditingPanel.Instance.DeSelectControlPoint(this);
            }
        }
        else if(ModelEditingPanel.Instance.currentTransformType == TransformType.Free)
        {
            isSelectOne = true;
            ModelEditingPanel.Instance.MultiSelectControlPoint(this);
            previousPosition = this.transform.position;
            isMovingVertex = true;
        }
        else if(ModelEditingPanel.Instance.GetControlPoints().Count == 0)
        {
            if (isSelected == false)
            {
                ModelEditingPanel.Instance.MultiSelectControlPoint(this);
            }
            else
            {
                ModelEditingPanel.Instance.DeSelectControlPoint(this);
            }
        }
    }
    public bool isSelectOne;
    private void OnGrabEnd(UnityEngine.XR.Interaction.Toolkit.SelectExitEventArgs args)
    {
        if (ModelEditingPanel.Instance.currentTransformType == TransformType.Free || isSelectOne)
        {
            isSelectOne = false;
            isMovingVertex = false;
        }
    }
    public bool isMovingVertex;
    public Vector3 previousPosition;
    private void Update()
    {
        if (isMovingVertex && isSelected)
        {
            SetVertexPosAsCPCurrentPos();
        }
    }

    public void AddOffsetToControlPointPosition(Vector3 offset)
    {
        this.transform.position += offset;
    }
    public void AddOffsetToControlPointAndVertsPosition(Vector3 offset)
    {
        AddOffsetToControlPointPosition(offset);
        ModelEditingPanel.Instance.AddOffsetToVertsPosition(vertices, offset);
    }
    public void SetVertexPosAsCPCurrentPos()
    {
        ModelEditingPanel.Instance.AddOffsetToSelectedControlPoints(this, this.transform.position - previousPosition);
        previousPosition = this.transform.position;
    }


}