using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

/// <summary>
/// Object Control Point - Child Class
/// </summary>
public class ObjectControlPoint : BaseControlPoint
{
    /// <summary>
    /// Init the control point
    /// </summary>
    public void Init(ModelData _parent)
    {
        type = EditMode.Object;
        modelData = _parent;

        transform.SetParent(modelData.transform);

        SetupComponents();
        UpdatePositionAndNormalToMatchElements();

    }
    /// <summary>
    /// Update the position and normal of the object based on the modeldata
    /// </summary>
    public override void UpdatePositionAndNormalToMatchElements()
    {
        normal = modelData.GetForward();
        transform.position = modelData.GetPosition();

    }
    /// <summary>
    /// Add offset to the position
    /// </summary>
    public override void AddOffsetToElements(Vector3 offset)
    {
        previousPosition = this.transform.position;
    }
}

/// <summary>
/// Vertex Control Point - Child Class
/// </summary>
public class VertexControlPoint : BaseControlPoint
{
    private List<int> verts;
    /// <summary>
    /// Init the control point
    /// </summary>
    public void Init(List<int> _verts, ModelData _parent)
    {
        verts = _verts;
        type = EditMode.Vertex;
        modelData = _parent;

        transform.SetParent(modelData.transform);

        SetupComponents();
        UpdatePositionAndNormalToMatchElements();
    }
    /// <summary>
    /// Update the position and normal of the control point based on the modeldata verts
    /// </summary>
    public override void UpdatePositionAndNormalToMatchElements()
    {

        List<Vector3> allPositions = modelData.GetVerts();
        Vector3 vertCenter = Vector3.zero;
        foreach (var idx in verts)
            vertCenter += allPositions[idx];
        vertCenter /= verts.Count;

        Vector3 vertNormal = Vector3.up;


        normal = vertNormal;
        transform.position = vertCenter;

    }
    /// <summary>
    /// Get the verts of the element
    /// </summary>
    public List<int> GetVerts()
    {
        return verts;
    }
    /// <summary>
    /// Add offset to the position
    /// </summary>
    public override void AddOffsetToElements(Vector3 offset)
    {
        modelData.AddOffsetToVerts(offset, verts.ToArray());
        previousPosition = this.transform.position;
    }
}
/// <summary>
/// Edge Control Point - Child Class
/// </summary>
public class EdgeControlPoint : BaseControlPoint
{
    private List<int> edgeVerts;
    /// <summary>
    /// Init the control point
    /// </summary>
    public void Init(List<int> _edgeVerts, ModelData _parent)
    {
        edgeVerts = _edgeVerts;
        type = EditMode.Edge;
        modelData = _parent;

        transform.SetParent(modelData.transform);

        SetupComponents();
        UpdatePositionAndNormalToMatchElements();

    }
    /// <summary>
    /// Update the position and normal of the control point based on the modeldata verts
    /// </summary>
    public override void UpdatePositionAndNormalToMatchElements()
    {

        List<Vector3> allPositions = modelData.GetVerts();
        Vector3 edgeCenter = Vector3.zero;
        foreach (var idx in edgeVerts)
            edgeCenter += allPositions[idx];
        edgeCenter /= edgeVerts.Count;

        Vector3 edgeNormal = Vector3.up;


        normal = edgeNormal;
        transform.position = edgeCenter;

    }
    /// <summary>
    /// Get the verts of the element
    /// </summary>
    public List<int> GetEdgeVerts()
    {
        return edgeVerts;
    }
    /// <summary>
    /// Add offset to the position
    /// </summary>
    public override void AddOffsetToElements(Vector3 offset)
    {
        modelData.AddOffsetToVerts(offset, edgeVerts.ToArray());
        previousPosition = this.transform.position;
    }

}
/// <summary>
/// Face Control Point - Child Class
/// </summary>
public class FaceControlPoint : BaseControlPoint
{
    public Face face;
    public List<int> faceVerts;

    public void Init(List<int> _faceVerts, ModelData _parent, Face _face)
    {
        face = _face;
        faceVerts = _faceVerts;
        type = EditMode.Face;
        modelData = _parent;

        transform.SetParent(modelData.transform);

        SetupComponents();

        UpdatePositionAndNormalToMatchElements();
    }
    /// <summary>
    /// Get the verts of the element
    /// </summary>
    public List<int> GetFaceVerts()
    {
        return faceVerts;
    }
    /// <summary>
    /// Get the face
    /// </summary>
    public Face GetFace()
    {
        return face;
    }
    /// <summary>
    /// Update the position and normal of the control point based on the modeldata verts and face
    /// </summary>
    public override void UpdatePositionAndNormalToMatchElements()
    {
        
        List<Vector3> allPositions = modelData.GetVerts();
        Vector3 faceCenter = Vector3.zero;
        foreach (var idx in face.distinctIndexes)
            faceCenter += allPositions[idx];
        faceCenter /= face.distinctIndexes.Count;

        Vector3 faceNormal = UnityEngine.ProBuilder.Math.Normal(modelData.GetEditModel(), face);


        normal = faceNormal;
        transform.position = faceCenter;

    }
    /// <summary>
    /// Add offset to the position
    /// </summary>
    public override void AddOffsetToElements(Vector3 offset)
    {
        modelData.AddOffsetToVerts(offset, faceVerts.ToArray());
        previousPosition = this.transform.position;
    }
}
/// <summary>
/// Control Point - Base Class
/// </summary>
public abstract class BaseControlPoint : MonoBehaviour
{
    protected EditMode type = EditMode.Vertex;
    protected Vector3 normal;
    protected ModelData modelData;
    protected Vector3 previousPosition;

    private bool isGrabMove;
    private bool isSelected;
    private bool isSelectOne;

    private MeshRenderer meshRenderer;
    private Rigidbody rb;
    private XRGrabInteractable grabInteractable;
    private XRGeneralGrabTransformer grabTransformer;

    /// <summary>
    /// Add offset to the control point and its elements
    /// Allow child classes to implement
    /// </summary>
    public void MoveByOffset(Vector3 offset)
    {
        this.transform.position += offset;
        AddOffsetToElements(offset);
    }
    /// <summary>
    /// Update the position of the control point based on data from the modelData
    /// Allow child classes to implement
    /// </summary>
    public virtual void UpdatePositionAndNormalToMatchElements()
    {
        //Implemented in derived classes

    }

    /// <summary>
    /// Add offset to the elements
    /// Allow child classes to implement
    /// </summary>
    public virtual void AddOffsetToElements(Vector3 offset)
    {
        //Implemented in derived classes
        
    }

    /// <summary>
    /// Update the verts position if is grabbed and moving
    /// </summary>
    private void Update()
    {
        if (isGrabMove && isSelected)
        {
            ModelEditingPanel.Instance.MoveSelectedControlPointsByOffset(this.transform.position - previousPosition, this);
            previousPosition = this.transform.position;
        }
    }

    /// <summary>
    /// Get the control point position
    /// </summary>
    public Vector3 GetCurrentPosition()
    {
        return transform.position;
    }
    /// <summary>
    /// Control Point size updated - change my size
    /// </summary>
    private void ControlPointSizeChanged(float size)
    {
        this.transform.localScale = Vector3.one * size;
    }


    /// <summary>
    /// On Destroy clean up listeners
    /// </summary>
    private void OnDestroy()
    {
        ViewManager.Instance.OnControlPointSizeChanged.RemoveListener(ControlPointSizeChanged);
        ModelEditingPanel.Instance.OnTransformTypeChanged.RemoveListener(TransformTypeChanged);
        grabInteractable.selectEntered.RemoveListener(OnGrabStart);
        grabInteractable.selectExited.RemoveListener(OnGrabEnd);
    }
    /// <summary>
    /// Deactivate the control point and deselect it
    /// </summary>
    public void Deactivate()
    {
        this.transform.SetParent(ModelEditingPanel.Instance.transform);
        Deselect();
        this.gameObject.SetActive(false);
    }
    /// <summary>
    /// Setup the control points components
    /// </summary>
    public void SetupComponents()
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
            ModelEditingPanel.Instance.OnTransformTypeChanged.AddListener(TransformTypeChanged);
        }

        ControlPointSizeChanged(ViewManager.Instance.GetControlPointSize());

        Deselect();
        gameObject.SetActive(true);
    }

    /// <summary>
    /// On Free Mode allows easy grab and move control points
    /// </summary>
    public void TransformTypeChanged()
    {
        if (ModelEditingPanel.Instance.GetTransformType() == TransformType.Free)
        {
            SetAsMoveAble();
        }
        else
        {
            SetAsSelectedAble();
        }
    }
    /// <summary>
    /// Dont alllow user to grab and move control poiont
    /// </summary>
    public void SetAsSelectedAble()
    {
        grabInteractable.trackPosition = false;
        grabInteractable.trackRotation = false;
    }
    /// <summary>
    /// Allow user to grab and move control poiont
    /// </summary>
    public void SetAsMoveAble()
    {
        grabInteractable.trackPosition = true;
        grabInteractable.trackRotation = true;
    }
    /// <summary>
    /// Visual Feedback for Selection
    /// </summary>
    public void Select()
    {
        isSelected = true;
        meshRenderer.material = ViewManager.Instance.GetSelectedControlPointMaterial();
    }
    /// <summary>
    /// Visual Feedback for DeSelection
    /// </summary>
    public void Deselect()
    {
        isSelected = false;
        meshRenderer.material = ViewManager.Instance.GetUnselectedControlPointMaterial();
    }
    /// <summary>
    /// On Start Grabbing control point behavior
    /// </summary>
    private void OnGrabStart(UnityEngine.XR.Interaction.Toolkit.SelectEnterEventArgs args)
    {
        isSelectOne = false;

        
        if (isSelected)
        {
            ModelEditingPanel.Instance.DeSelectControlPoint(this);
        }
        else if(isSelected == false)
        {
            ModelEditingPanel.Instance.MultiSelectControlPoint(this);

            if (ModelEditingPanel.Instance.GetTransformType() == TransformType.Free)
            {
                isSelectOne = true;
                previousPosition = this.transform.position;
                isGrabMove = true;
            }
        }
    }
    /// <summary>
    /// On End Grabbing control point behavior
    /// </summary>
    private void OnGrabEnd(UnityEngine.XR.Interaction.Toolkit.SelectExitEventArgs args)
    {
        if (ModelEditingPanel.Instance.GetTransformType() == TransformType.Free || isSelectOne)
        {
            isSelectOne = false;
            isGrabMove = false;
            UpdatePositionAndNormalToMatchElements();
        }
    }

}