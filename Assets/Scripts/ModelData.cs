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

    // PNS Integration
    [Header("PNS Settings")]
    public bool enablePNSUpdates = true; // Toggle PNS updates on/off
    public bool autoFlushOnEdit = false; // Automatically flush dirty vertices on edit
    private List<int> pendingVertexUpdates = new List<int>(); // Track vertices to update


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

        // Initiliaze PNS if enabled
        if (enablePNSUpdates && PNSModelIntegration.Instance != null)
        {
            StartCoroutine(InitializePNSDelayed());
        }

    }

    // Delayed initialization to ensure mesh is fully set up
    private IEnumerator InitializePNSDelayed()
    {
        yield return new WaitForEndOfFrame();
        if (editingModel != null)
        {
            this.GetPnSpline(); // Create initial PnSpline
            Debug.Log($"PnSpline initialized for {modelName}");
        }
    }

    public void DeleteModel()
    {
        if (interactable != null) interactable.selectEntered.RemoveListener(OnGrab);

        ModelEditingPanel.Instance.OnEditModeChanged.RemoveListener(OnEditModeChanged);
        ViewManager.Instance.OnShadingChanged.RemoveListener(ShadingUpdated);
        ModelEditingPanel.Instance.OnTransformTypeChanged.RemoveListener(OnTransformTypeChanged);
        SelectionManager.Instance.OnSelectionChanged.RemoveListener(OnSelectionChanged);

        // Clean up PNS cache
        if (PNSModelIntegration.Instance != null)
        {
            PNSModelIntegration.Instance.RemovePnSplineFromCache(modelID);
        }

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
        meshCollider = editingModel.AddComponent<MeshCollider>();
        meshFilter = editingModel.GetComponent<MeshFilter>();
        modelID = ModelsManager.Instance.TrackModel(this);

        Rigidbody rigid = editingModel.AddComponent<Rigidbody>();
        rigid.isKinematic = true;

        interactable = editingModel.AddComponent<XRGrabInteractable>();
        grabTransform = editingModel.AddComponent<SnappingGrabTransformer>();
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

        // PNS Integration: Recreate PnSpline for topology changes
        if (enablePNSUpdates && PNSModelIntegration.Instance != null)
        {
            this.GetPnSpline(forceRecreate: true);
            Debug.Log($"PnSpline recreated for {modelName} due to topology change");
        }

        Debug.Log("UpdateMeshCreation");
    }
    public void UpdateMeshEdit()
    {
        editingModel.Refresh();
        editingModel.Refresh();

        // PNS Integration: Auto-flush if enabled
        if (enablePNSUpdates && autoFlushOnEdit && PNSModelIntegration.Instance != null)
        {
            FlushPendingPNSUpdates();
        }

        Debug.Log("UpdateMeshEdit");
        //smoothedMesh.UpdateMesh();
    }
    public void FinalizeEditModel()
    {
        editingModel.ToMesh();
        editingModel.Refresh();

        // PNS Integration: Flush all pending updates
        if (enablePNSUpdates && PNSModelIntegration.Instance != null)
        {
            FlushPendingPNSUpdates();
        }

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

        // PNS Integration: Mark vertices as dirty
        if (enablePNSUpdates && PNSModelIntegration.Instance != null)
        {
            foreach (int vertIndex in verts)
            {
                if (!pendingVertexUpdates.Contains(vertIndex))
                {
                    pendingVertexUpdates.Add(vertIndex);
                }
            }
            this.MarkVerticesDirty(verts);
        }

        UpdateMeshEdit();
    }

    #region PNS Integration Methods

    /// <summary>
    /// Flush all pending PNS vertex updates
    /// </summary>
    public void FlushPendingPNSUpdates()
    {
        if (pendingVertexUpdates.Count > 0)
        {
            uint[] affectedPatches = this.FlushDirtyVertices();
            Debug.Log($"PNS Update: {affectedPatches.Length} patches affected");
            pendingVertexUpdates.Clear();
        }
    }

    /// <summary>
    /// Manually update specific vertices in the PnSpline
    /// </summary>
    public void UpdatePNSVertices(int[] vertexIndices)
    {
        if (!enablePNSUpdates || PNSModelIntegration.Instance == null) return;

        uint[] affectedPatches = this.UpdatePnSpline(vertexIndices);
        Debug.Log($"PNS Update: Updated {vertexIndices.Length} vertices, " +
                  $"affecting {affectedPatches.Length} patches");
    }

    /// <summary>
    /// Get the current PnSpline for this model
    /// </summary>
    public PolyhedralNetSplines.PnSpline GetCurrentPnSpline()
    {
        if (!enablePNSUpdates || PNSModelIntegration.Instance == null) return null;
        return this.GetPnSpline();
    }

    #endregion

}
