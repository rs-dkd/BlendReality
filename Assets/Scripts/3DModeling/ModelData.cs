using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;
using System.Linq;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEditorInternal;
public class ModelData : MonoBehaviour
{
    public string modelName;
    public int modelID;
    private Material originalMaterial;
    private Material overrideMaterial;
    public MeshFilter meshFilter;
    public MeshRenderer meshRender;
    public MeshCollider meshCollider;
    public ProBuilderMesh editingModel;
    public Transform trans;
    private bool isSelected;
    //public SmoothSurface smoothedMesh;
    public XRGrabInteractable interactable;
    public XRGeneralGrabTransformer grabTransform;
    public EditMode editMode;
    public TransformType transformType;

    private void Start()
    {
        ModelEditingPanel.Instance.OnEditModeChanged.AddListener(OnEditModeChanged);
        TransformGizmo.Instance.OnTransformTypeChanged.AddListener(OnTransformTypeChanged);
    }
    public void DeleteModel()
    {
        interactable.selectEntered.RemoveListener(OnGrab);
        ModelEditingPanel.Instance.OnEditModeChanged.RemoveListener(OnEditModeChanged);
        ViewManager.Instance.OnShadingChanged.RemoveListener(ShadingUpdated);
        ModelsManager.Instance.UnTrackModel(this);
        Destroy(this.gameObject);
    }
    /// <summary>
    /// On Edit Mode Changed
    /// </summary>
    
    public void OnTransformTypeChanged()
    {
        transformType = TransformGizmo.Instance.transformType; 

        if (interactable == null) return;

        if (transformType == TransformType.Free)
        {
            interactable.trackPosition = true;
            interactable.trackRotation = true;
        }
        else
        {
            interactable.trackPosition = false;
            interactable.trackRotation = false;
        }
    }
    public void OnEditModeChanged()
    {
        editMode = ModelEditingPanel.Instance.currentEditMode;
        if (interactable == null) return;

        if (editMode == EditMode.Object)
        {
            interactable.enabled = true;
            grabTransform.enabled = true;

            OnTransformTypeChanged();
        }
        else if (editMode == EditMode.Pivot)
        {
            interactable.enabled = false;
            grabTransform.enabled = false;
        }
        else
        {
            interactable.enabled = false;
            grabTransform.enabled = false;
        }
    }
    /// <summary>
    /// SETTERS
    /// </summary>
    public void SetPosition(Vector3 pos)
    {
        trans.position = pos;
    }
    public void SetScale(Vector3 scale)
    {
        trans.localScale = scale;
    }
    /// <summary>
    /// GETTERS
    /// </summary>
    public ProBuilderMesh GetEditModel()
    {
        return editingModel;
    }
    public Mesh GetMesh()
    {
        return meshFilter.sharedMesh;
    }
    public int GetFacesCount()
    {
        return editingModel.faceCount;
    }
    public int GetEdgesCount()
    {
        return editingModel.edgeCount;
    }
    public int GetVertCount()
    {
        return editingModel.vertexCount;
    }
    public List<Vector3> GetVerts()
    {
        return editingModel.VerticesInWorldSpace().ToList();
    }
    public List<Face> GetFaces()
    {
        return editingModel.faces.ToList<Face>();
    }
    /// <summary>
    /// Creating and Updating Model Mesh
    /// </summary>
    public void SetupModel(ProBuilderMesh _editingModel)
    {
        editingModel = _editingModel;
       
        trans = this.transform;
        meshRender = this.GetComponent<MeshRenderer>();
        meshCollider = this.AddComponent<MeshCollider>();
        meshFilter = this.GetComponent<MeshFilter>();
        modelID = ModelsManager.Instance.TrackModel(this);

        Rigidbody rigid = this.AddComponent<Rigidbody>();
        rigid.isKinematic = true;

        interactable = this.AddComponent<XRGrabInteractable>();
        grabTransform = this.AddComponent<XRGeneralGrabTransformer>();
        interactable.selectEntered.AddListener(OnGrab);


        //if (smoothedMesh == null)
        //{
        //    GameObject obj = new GameObject("MyObject");
        //    obj.transform.parent = this.transform;
        //    obj.transform.localPosition = Vector3.zero;
        //    smoothedMesh = obj.AddComponent<SmoothSurface>();
        //    smoothedMesh.SetupModel(this);
        //}

        ShadingUpdated();
        ViewManager.Instance.OnShadingChanged.AddListener(ShadingUpdated);

        OnTransformTypeChanged();
        OnEditModeChanged();
    }
    public void UpdateMeshCreation(ProBuilderMesh mesh)
    {
        editingModel.Clear();

        editingModel.positions = mesh.positions;
        editingModel.faces = mesh.faces;

        editingModel.ToMesh();
        editingModel.Refresh();
    }
    public void UpdateMeshEdit()
    {
        editingModel.Refresh();
        //smoothedMesh.UpdateMesh();
    }
    public void FinalizeEditModel()
    {
        editingModel.ToMesh();
        editingModel.Refresh();
    }
    /// <summary>
    /// Select and Unselect Model
    /// </summary>
    public void SelectModel()
    {
        isSelected = true;
        ShadingUpdated();
    }
    public void UnSelectModel()
    {
        isSelected = false;
        ShadingUpdated();
    }


    private void OnGrab(SelectEnterEventArgs args)
    {
        Debug.Log("ggg");
        if(editMode == EditMode.Object && transformType == TransformType.Select)
        {
        Debug.Log(isSelected);
            if (isSelected)
            {
                SelectionManager.Instance.RemoveModelFromSelection(this);
            }
            else
            {
                SelectionManager.Instance.SelectModel(this);
            }
        }
    }


    public void ShadingUpdated()
    {
        meshRender.sharedMaterial = ViewManager.Instance.GetCurrentShading(isSelected);
    }








 






}
