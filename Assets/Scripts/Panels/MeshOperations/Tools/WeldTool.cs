using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// Weld Tool
/// Welds selected (faces,edges,verts) vertices together. 
/// Automatically deletes objects if the weld results in invalid geometry 
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

    private void Start()
    {
        ModelEditingPanel.Instance.OnEditModeChanged.AddListener(OnEditModeChanged);
        OnEditModeChanged();
    }

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

    public void ToggleDist()
    {
        distSlider.gameObject.SetActive(distToggle.isOn);
    }

    public void Weld()
    {
        //Check if can perform the tool
        if (CanPerformTool() == false) return;

        if (ModelEditingPanel.Instance.GetEditMode() == EditMode.Object)
        {
            List<ModelData> models = new List<ModelData>(SelectionManager.Instance.GetSelectedModels());

            if (models.Count < 2) return;

            List<ProBuilderMesh> meshes = new List<ProBuilderMesh>();
            foreach (ModelData model in models)
            {
                ProBuilderMesh pbm = model.GetEditModel();
                if (pbm != null)
                {
                    meshes.Add(pbm);
                }
            }

            //Combine everything into the first model
            CombineMeshes.Combine(meshes, models[0].GetEditModel());

            //Validate the result immediately
            //If the combine failed or created bad geo, this will clean it up
            ValidateAndRefreshMesh(models[0]);

            //Delete old models
            for (int x = 1; x < models.Count; x++)
            {
                DeleteModelSafely(models[x]);
            }

            //Reselect the combined model if it survived validation
            if (models[0] != null)
            {
                SelectionManager.Instance.ClearSelection();
                models[0].SelectModel();
            }
        }
        else
        {
            float threshold = 100000f; //Default high threshold implies "weld everything selected"
            if (distToggle.isOn)
            {
                threshold = distSlider.GetValueAsFloat();
            }

            List<BaseControlPoint> points = ModelEditingPanel.Instance.GetControlPoints();
            List<int> allVertices = new List<int>();

            foreach (var point in points)
            {
                List<int> pointVerts = new List<int>();
                if (ModelEditingPanel.Instance.GetEditMode() == EditMode.Vertex) pointVerts = ((VertexControlPoint)point).GetVerts();
                else if (ModelEditingPanel.Instance.GetEditMode() == EditMode.Edge) pointVerts = ((EdgeControlPoint)point).GetEdgeVerts();
                else if (ModelEditingPanel.Instance.GetEditMode() == EditMode.Face) pointVerts = ((FaceControlPoint)point).GetFaceVerts();
                else continue;

                foreach (int v in pointVerts)
                {
                    if (!allVertices.Contains(v)) allVertices.Add(v);
                }
            }

            ModelData targetModel = SelectionManager.Instance.GetFirstSelected();
            if (targetModel == null) return;

            ProBuilderMesh mesh = targetModel.GetEditModel();
            if (mesh == null) return;

            //Perform the Weld
            VertexEditing.WeldVertices(mesh, allVertices, threshold);

            //Validate the result
            //If the weld collapsed the mesh into a single point, this will delete the object safely
            ValidateAndRefreshMesh(targetModel);
        }
    }


    /// <summary>
    /// Checks if the mesh is valid after an operation. 
    /// Explicitly removes collapsed geometry before updating the collider.
    /// </summary>
    private void ValidateAndRefreshMesh(ModelData model)
    {
        if (model == null) return;
        ProBuilderMesh mesh = model.GetEditModel();
        if (mesh == null) return;

        mesh.RemoveUnusedVertices();

        UnityEngine.ProBuilder.MeshOperations.MeshValidation.RemoveDegenerateTriangles(mesh);

 
        mesh.faces = mesh.faces.Where(f => f.indexes.Count > 0).ToList();

        mesh.RemoveUnusedVertices();

 
        if (mesh.faceCount == 0 || mesh.vertexCount < 3)
        {
            DeleteModelSafely(model);
            return;
        }

        try
        {
            mesh.ToMesh();
            mesh.Refresh();

            model.UpdateMeshEdit();
            ModelEditingPanel.Instance.UpdateEditModel();
        }
        catch (System.Exception e)
        {
            DeleteModelSafely(model);
        }
    }

    private void DeleteModelSafely(ModelData model)
    {
        if (model != null)
        {
            //Ensure we don't leave a ghost selection reference
            SelectionManager.Instance.RemoveModelFromSelection(model);
            model.DeleteModel();
        }
    }

    public override bool CanShowTool()
    {
        return true;
    }

    public override bool CanPerformTool()
    {
        if (ModelEditingPanel.Instance.GetEditMode() == EditMode.Object)
        {
            return SelectionManager.Instance.GetSelectedModels().Count > 1;
        }
        else
        {
            return ModelEditingPanel.Instance.GetControlPoints().Count > 0;
        }
    }
}