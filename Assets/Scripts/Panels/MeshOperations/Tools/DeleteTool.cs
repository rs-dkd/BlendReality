using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

/// <summary>
/// Delete Tool
/// Manually reconstructs the face list to avoid topology crashes.
/// Automatically deletes the GameObject if no geometry remains.
/// </summary>
public class DeleteTool : OperationTool
{
    public override bool CanShowTool()
    {
        return true;
    }

    public override bool CanPerformTool()
    {
        // Check Object Mode selection
        if (ModelEditingPanel.Instance.GetEditMode() == EditMode.Object &&
            SelectionManager.Instance.GetSelectedModels().Count == 0)
            return false;

        // Check Element Mode selection
        if ((ModelEditingPanel.Instance.GetEditMode() != EditMode.Object) &&
            ModelEditingPanel.Instance.GetControlPoints().Count == 0)
            return false;

        return true;
    }

    /// <summary>
    /// Entry point for the tool
    /// </summary>
    public void DeleteSelectedElements()
    {
        if (!CanPerformTool()) return;

        EditMode editMode = ModelEditingPanel.Instance.GetEditMode();

        if (editMode == EditMode.Object)
        {
            List<ModelData> models = SelectionManager.Instance.GetSelectedModels();
            for (int i = models.Count - 1; i >= 0; i--)
            {
                models[i].DeleteModel();
            }
            SelectionManager.Instance.ClearSelection();
        }
        else
        {
            ModelData model = SelectionManager.Instance.GetFirstSelected();
            ProBuilderMesh pbMesh = model.GetEditModel();
            List<BaseControlPoint> selection = ModelEditingPanel.Instance.GetControlPoints();

            if (editMode == EditMode.Vertex)
            {
                DeleteVerticesManual(model, pbMesh, selection);
            }
            else if (editMode == EditMode.Edge)
            {
                DeleteEdgesManual(model, pbMesh, selection);
            }
            else if (editMode == EditMode.Face)
            {
                DeleteFacesManual(model, pbMesh, selection);
            }

            //Only update if the model wasn't destroyed
            if (model != null && model.gameObject != null)
            {
                model.UpdateMeshEdit();
                ModelEditingPanel.Instance.UpdateEditModel();
            }
        }
    }

    private void DeleteFacesManual(ModelData model, ProBuilderMesh pbMesh, List<BaseControlPoint> selectedPoints)
    {
        HashSet<Face> facesToDelete = new HashSet<Face>();
        foreach (var cp in selectedPoints)
        {
            facesToDelete.Add(((FaceControlPoint)cp).GetFace());
        }

        PerformManualRebuild(model, pbMesh, facesToDelete);
    }

    private void DeleteEdgesManual(ModelData model, ProBuilderMesh pbMesh, List<BaseControlPoint> selectedPoints)
    {
        HashSet<Face> facesToDelete = new HashSet<Face>();

        //Iterate through each selected edge individually
        foreach (var cp in selectedPoints)
        {
            EdgeControlPoint edgeCP = (EdgeControlPoint)cp;

            //Get the two vertices that define this specific edge
            List<int> specificEdgeVerts = edgeCP.GetEdgeVerts().ToList();

            if (specificEdgeVerts.Count < 2) continue;

            //Find faces that contain both vertices of this edge
            foreach (var face in pbMesh.faces)
            {
                bool containsV1 = face.distinctIndexes.Contains(specificEdgeVerts[0]);
                bool containsV2 = face.distinctIndexes.Contains(specificEdgeVerts[1]);

                if (containsV1 && containsV2)
                {
                    facesToDelete.Add(face);
                }
            }
        }

        PerformManualRebuild(model, pbMesh, facesToDelete);
    }

    private void DeleteVerticesManual(ModelData model, ProBuilderMesh pbMesh, List<BaseControlPoint> selectedPoints)
    {
        HashSet<int> vertIndices = new HashSet<int>();
        foreach (var cp in selectedPoints)
        {
            vertIndices.UnionWith(((VertexControlPoint)cp).GetVerts());
        }

        HashSet<Face> facesToDelete = GetFacesTouchingVerts(pbMesh, vertIndices);
        PerformManualRebuild(model, pbMesh, facesToDelete);
    }

    private void PerformManualRebuild(ModelData model, ProBuilderMesh pbMesh, HashSet<Face> facesToDelete)
    {
        if (facesToDelete.Count == 0) return;

        List<Face> remainingFaces = new List<Face>();

        foreach (var face in pbMesh.faces)
        {
            if (!facesToDelete.Contains(face))
            {
                remainingFaces.Add(face);
            }
        }

        if (remainingFaces.Count == 0)
        {
            DeleteModelSafely(model);
            return;
        }

        pbMesh.faces = remainingFaces;

        pbMesh.RemoveUnusedVertices();

  
        if (pbMesh.vertexCount < 3)
        {
            DeleteModelSafely(model);
            return;
        }

        try
        {
            pbMesh.ToMesh();
            pbMesh.Refresh();
        }
        catch (System.Exception e)
        {
            DeleteModelSafely(model);
        }
    }

    /// <summary>
    /// Helper to ensure we don't try to access a destroyed object
    /// </summary>
    private void DeleteModelSafely(ModelData model)
    {
        SelectionManager.Instance.ClearSelection();
        if (model != null)
        {
            model.DeleteModel();
        }
    }

    /// <summary>
    /// Helper: Finds all faces that use any of the provided vertex indices
    /// </summary>
    private HashSet<Face> GetFacesTouchingVerts(ProBuilderMesh pbMesh, HashSet<int> vertIndices)
    {
        HashSet<Face> touchedFaces = new HashSet<Face>();

        foreach (var face in pbMesh.faces)
        {
            if (face.distinctIndexes.Any(i => vertIndices.Contains(i)))
            {
                touchedFaces.Add(face);
            }
        }
        return touchedFaces;
    }
}