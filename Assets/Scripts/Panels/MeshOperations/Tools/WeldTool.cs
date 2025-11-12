using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.UI;

/// <summary>
/// Weld Tool
/// Welds selected (faces,edges,verts) vertices together. Also can combine selected objects into a single object
/// </summary>
public class WeldTool : OperationTool
{
    [Header("UI Elements")]
    [Tooltip("Weld by distance slider")]
    [SerializeField] private SliderUI distSlider;
    [Tooltip("Distance Parent GO to show hide if Object mode")]
    [SerializeField] private GameObject distParentGO;
    [Tooltip("Weld by distance toggle")]
    [SerializeField] private Toggle distToggle;

    /// <summary>
    /// Setup listener for the editMode (if obj or else)
    /// </summary>
    private void Start()
    {
        ModelEditingPanel.Instance.OnEditModeChanged.AddListener(OnEditModeChanged);
        OnEditModeChanged();
    }

    /// <summary>
    /// If obj hide the weld by distance
    /// </summary>
    public void OnEditModeChanged()
    {
        if (ModelEditingPanel.Instance.GetEditMode() == EditMode.Object)
        {
            distParentGO.SetActive(false);
            distToggle.isOn = false;
        }
        else
        {
            distParentGO.SetActive(true);
            ToggleDist();
        }
    }
    /// <summary>
    /// Called from the Distance Toggle to show hide the slider
    /// </summary>
    public void ToggleDist()
    {
        if (distToggle.isOn)
        {
            distSlider.gameObject.SetActive(true);
        }
        else
        {
            distSlider.gameObject.SetActive(false);
        }
    }
    /// <summary>
    /// Called from the weld button 
    /// Welds verts, edges, faces or combines objects
    /// </summary>
    public void Weld()
    {
        //Check if can preform the tool
        if (CanPerformTool() == false) return;

        //If object mode combine the objects into a single object
        if (ModelEditingPanel.Instance.GetEditMode() == EditMode.Object)
        {
            List<ModelData> models = SelectionManager.Instance.GetSelectedModels();
            List<ProBuilderMesh> meshes = new List<ProBuilderMesh>();
            foreach(ModelData model in models)
            {
                ProBuilderMesh pbm = model.GetEditModel();
                if(pbm != null)
                {
                    meshes.Add(pbm);
                }
            }

            CombineMeshes.Combine(meshes, models[0].GetEditModel());

            //Delete old models
            for(int x = 1; x < models.Count; x++)
            {
                models[x].DeleteModel();
            }
        }
        else
        {
            //Threshold to weld the verts
            float threshold = 100000f;
            if (distToggle.isOn)
            {
                threshold = distSlider.GetValueAsFloat();
            }

            //Get verts from verts, edges, faces
            List<BaseControlPoint> points = ModelEditingPanel.Instance.GetControlPoints();
            List<int> allVertices = new List<int>();
            foreach (var point in points)
            {
                Debug.Log(point);
                Debug.Log(point.GetType());
                List<int> pointVerts = new List<int>();
                if (ModelEditingPanel.Instance.GetEditMode() == EditMode.Vertex) pointVerts = ((VertexControlPoint)point).GetVerts();
                else if (ModelEditingPanel.Instance.GetEditMode() == EditMode.Edge) pointVerts = ((EdgeControlPoint)point).GetEdgeVerts();
                else if (ModelEditingPanel.Instance.GetEditMode() == EditMode.Face) pointVerts = ((FaceControlPoint)point).GetFaceVerts();
                else return;

                foreach (int v in pointVerts)
                {
                    if (!allVertices.Contains(v))
                    {
                        allVertices.Add(v);
                    }
                }
            }

            //Get mesh
            ProBuilderMesh mesh = SelectionManager.Instance.GetFirstSelected().GetEditModel();

            if (mesh == null)
            {
                Debug.LogError("No ProBuilderMesh selected or available for welding.");
                return;
            }


            //Weld verts
            int[] newVertices = VertexEditing.WeldVertices(
                mesh,
                allVertices,
                threshold
            );

            //Refrech mesh
            mesh.ToMesh();
            SelectionManager.Instance.GetFirstSelected().UpdateMeshEdit();
            ModelEditingPanel.Instance.UpdateEditModel();
        }
    }

    /// <summary>
    /// Always true
    /// </summary>
    public override bool CanShowTool()
    {
        return true;
    }
    /// <summary>
    /// Only if object and has objects selected or (vert, edge, face) and has control points selected
    /// </summary>
    public override bool CanPerformTool()
    {
        if (
            (ModelEditingPanel.Instance.GetEditMode() == EditMode.Object && SelectionManager.Instance.GetSelectedModels().Count <= 1) ||
            ((ModelEditingPanel.Instance.GetEditMode() == EditMode.Vertex || ModelEditingPanel.Instance.GetEditMode() == EditMode.Edge || ModelEditingPanel.Instance.GetEditMode() == EditMode.Face)
            && ModelEditingPanel.Instance.GetControlPoints().Count == 0)
            )
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}