using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

/// <summary>
/// Delete
/// Can delete faces, edges, verts, and objects
/// </summary>
public class DeleteTool : OperationTool
{



    /// <summary>
    /// Always can show tool
    /// </summary>
    public override bool CanShowTool()
    {
        return true;
    }
    /// <summary>
    /// Cant perform tool if object and no objs selected or if (face, vert, edge) and no control points selected
    /// </summary>
    public override bool CanPerformTool()
    {
        if (
            (ModelEditingPanel.Instance.GetEditMode() == EditMode.Object && SelectionManager.Instance.GetSelectedModels().Count == 0) ||
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


    /// <summary>
    /// Based on editModel preform correct delete operation
    /// </summary>
    public void DeleteSelectedElements()
    {
        //check if can preform the tool
        if (CanPerformTool() == false) return;



        EditMode editMode = ModelEditingPanel.Instance.GetEditMode();
        //Delete selected objects
        if (editMode == EditMode.Object)
        {
            List<ModelData> models = SelectionManager.Instance.GetSelectedModels();

            for (int i = models.Count - 1; i >= 0; i--)
            {
                models[i].DeleteModel();
            }

            SelectionManager.Instance.ClearSelection();

        }
        //Delete selected verts
        else if(editMode == EditMode.Vertex)
        {
            ModelData model = SelectionManager.Instance.GetFirstSelected();
            ProBuilderMesh pbMesh = model.GetEditModel();
            List<BaseControlPoint> selectedControlPoints = ModelEditingPanel.Instance.GetControlPoints();
            DeleteVertices(pbMesh, selectedControlPoints);
            model.UpdateMeshEdit();
            ModelEditingPanel.Instance.UpdateEditModel();
        }
        //Delete selected edges
        else if (editMode == EditMode.Edge)
        {
            ModelData model = SelectionManager.Instance.GetFirstSelected();
            ProBuilderMesh pbMesh = model.GetEditModel();
            List<BaseControlPoint> selectedControlPoints = ModelEditingPanel.Instance.GetControlPoints();
            DeleteEdges(pbMesh, selectedControlPoints);
            model.UpdateMeshEdit();
            ModelEditingPanel.Instance.UpdateEditModel();
        }
        //Delete selected faces
        else if (editMode == EditMode.Face)
        {
            ModelData model = SelectionManager.Instance.GetFirstSelected();
            ProBuilderMesh pbMesh = model.GetEditModel();
            List<BaseControlPoint> selectedControlPoints = ModelEditingPanel.Instance.GetControlPoints();
            DeleteFaces(pbMesh, selectedControlPoints);
            model.UpdateMeshEdit();
            ModelEditingPanel.Instance.UpdateEditModel();
        }




    }

    /// <summary>
    /// Deletes selected faces
    /// </summary>
    private void DeleteFaces(ProBuilderMesh pbMesh, List<BaseControlPoint> selectedFaceControlPoints)
    {
        List<Face> faces = new List<Face>();
        for (int i = 0; i < selectedFaceControlPoints.Count; i++)
        {
            faces.Add(((FaceControlPoint)selectedFaceControlPoints[i]).GetFace());
        }


        if (faces.Any())
        {
            pbMesh.DeleteFaces(faces);
            pbMesh.ToMesh();
            pbMesh.Refresh();
        }
    }
    /// <summary>
    /// Deletes selected edges and their faces
    /// </summary>
    private void DeleteEdges(ProBuilderMesh pbMesh, List<BaseControlPoint> selectedEdgesControlPoints)
    {
        var verticesToDelete = new HashSet<int>(selectedEdgesControlPoints.SelectMany(cp =>
        {

            return ((EdgeControlPoint)cp).GetEdgeVerts();
        }));

        if (!verticesToDelete.Any()) return;

        var facesToDelete = new List<Face>();
        foreach (var face in pbMesh.faces)
        {
            if (face.indexes.Any(vertexIndex => verticesToDelete.Contains(vertexIndex)))
            {
                facesToDelete.Add(face);
            }
        }



        if (facesToDelete.Any())
        {
            pbMesh.DeleteFaces(facesToDelete);
            pbMesh.ToMesh();
            pbMesh.Refresh();

        }
    }
    /// <summary>
    /// Deletes selected verts and their faces
    /// </summary>
    private void DeleteVertices(ProBuilderMesh pbMesh, List<BaseControlPoint> selectedVertexControlPoints)
    {
        var verticesToDelete = new HashSet<int>(selectedVertexControlPoints.SelectMany(cp =>
        {
            
            return ((VertexControlPoint)cp).GetVerts();
        }));

        if (!verticesToDelete.Any()) return;

        var facesToDelete = new List<Face>();
        foreach (var face in pbMesh.faces)
        {
            if (face.indexes.Any(vertexIndex => verticesToDelete.Contains(vertexIndex)))
            {
                facesToDelete.Add(face);
            }
        }



        if (facesToDelete.Any())
        {
            pbMesh.DeleteFaces(facesToDelete);
            pbMesh.ToMesh();
            pbMesh.Refresh();

        }
    }

}


