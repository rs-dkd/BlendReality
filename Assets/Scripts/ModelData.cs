using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Linq;
using UnityEngine.XR.Interaction.Toolkit;
public class ModelData : MonoBehaviour
{
    public static int modelIDCounter = 0;
    public static int GetNewModelID()
    {
        modelIDCounter++;
        return modelIDCounter;
    }


    public string modelName;
    public int modelID;

    public MeshFilter meshFilter;
    public MeshRenderer meshRender;
    public MeshCollider meshCollider;
    public ProBuilderMesh editingModel;
    private bool isSelected;
    public XRGrabInteractable interactable;
    public SnappingGrabTransformer grabTransform;



    public bool GetIsSelected()
    {
        return isSelected;
    }
    public void UpdateName(string name)
    {
        modelName = name;
    }
    public string GetName()
    {
        return modelName;
    }
    private void Start()
    {
        ModelEditingPanel.Instance.OnEditModeChanged.AddListener(OnEditModeChanged);
        ModelEditingPanel.Instance.OnTransformTypeChanged.AddListener(OnTransformTypeChanged);
        ViewManager.Instance.OnShadingChanged.AddListener(ShadingUpdated);
        SelectionManager.Instance.OnSelectionChanged.AddListener(OnSelectionChanged);

    }
    public void DeleteModel()
    {
        if (interactable != null) interactable.selectEntered.RemoveListener(OnGrab);

        ModelEditingPanel.Instance.OnEditModeChanged.RemoveListener(OnEditModeChanged);
        ViewManager.Instance.OnShadingChanged.RemoveListener(ShadingUpdated);
        ModelEditingPanel.Instance.OnTransformTypeChanged.RemoveListener(OnTransformTypeChanged);
        SelectionManager.Instance.OnSelectionChanged.RemoveListener(OnSelectionChanged);

        ModelsManager.Instance.UnTrackModel(this);
        SelectionManager.Instance.RemoveModelFromSelection(this);
        Destroy(this.gameObject);
    }
    /// <summary>
    /// On Edit Mode Changed
    /// </summary>
    
    public void OnTransformTypeChanged()
    {

        if (interactable == null) return;
        if (grabTransform == null) return;

        if (ModelEditingPanel.Instance.GetTransformType() == TransformType.Free && ModelEditingPanel.Instance.GetEditMode() == EditMode.Object)
        {
            interactable.enabled = true;
            grabTransform.enabled = true;
            interactable.trackPosition = true;
            interactable.trackRotation = true;
        }
        else if (ModelEditingPanel.Instance.GetTransformType() == TransformType.Select && ModelEditingPanel.Instance.GetEditMode() == EditMode.Object)
        {
            interactable.enabled = true;
            grabTransform.enabled = true;
            interactable.trackPosition = false;
            interactable.trackRotation = false;
        }
        else
        {
            interactable.enabled = false;
            grabTransform.enabled = false;
            interactable.trackPosition = false;
            interactable.trackRotation = false;
        }
    }
    public void OnEditModeChanged()
    {
        OnTransformTypeChanged();
    }


    public void OnSelectionChanged(List<ModelData> models)
    {
        if(isSelected)
        {

        }
        else
        {

        }
    }




    /// <summary>
    /// SETTERS
    /// </summary>
    /// 

    public void SetPosition(Vector3 pos)
    {
        editingModel.transform.position = pos;
    }
    public Vector3 GetPosition()
    {
        return editingModel.transform.position;
    }
    public Vector3 GetForward()
    {
        return editingModel.transform.forward;
    }
    public void SetScale(Vector3 scale)
    {
        editingModel.transform.localScale = scale;
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
    public MeshFilter GetMeshFilter()
    {
        return meshFilter;
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
    public void SetupModel(ProBuilderMesh _editingModel, Vector3 meshPos, string name = "NewObject")
    {
        editingModel = _editingModel;
        SetPosition(meshPos);
        editingModel.gameObject.transform.parent = this.transform;

        modelName = name;

        meshRender = editingModel.GetComponent<MeshRenderer>();
        meshCollider = editingModel.gameObject.AddComponent<MeshCollider>();
        meshFilter = editingModel.GetComponent<MeshFilter>();
        modelID = ModelsManager.Instance.TrackModel(this);

        Rigidbody rigid = editingModel.gameObject.AddComponent<Rigidbody>();
        rigid.isKinematic = true;

        interactable = editingModel.gameObject.AddComponent<XRGrabInteractable>();
        grabTransform = editingModel.gameObject.AddComponent<SnappingGrabTransformer>();
        interactable.selectEntered.AddListener(OnGrab);
        interactable.throwOnDetach = false;




        ShadingUpdated();
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
        Debug.Log("UpdateMeshCreation");
    }
    public void UpdateMeshEdit()
    {
        editingModel.Refresh();
        editingModel.Refresh();

        Debug.Log("UpdateMeshEdit");
        //smoothedMesh.UpdateMesh();
    }
    public void FinalizeEditModel()
    {
        editingModel.ToMesh();
        editingModel.Refresh();
        Debug.Log("FinalizeEditModel");
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
        if(ModelEditingPanel.Instance.GetEditMode() == EditMode.Object && ModelEditingPanel.Instance.GetTransformType() == TransformType.Select)
        {
            if (isSelected)
            {
                SelectionManager.Instance.RemoveModelFromSelection(this);
            }
            else
            {
                SelectionManager.Instance.AddModelToSelection(this);
            }
        }
    }




    public void ShadingUpdated()
    {
        meshRender.sharedMaterial = ViewManager.Instance.GetCurrentShading(isSelected);
    }





    public void AddOffsetToVerts(Vector3 offset, int[] verts)
    {
        UnityEngine.ProBuilder.VertexPositioning.TranslateVerticesInWorldSpace(GetEditModel(), verts, offset);
        UpdateMeshEdit();
    }









}
