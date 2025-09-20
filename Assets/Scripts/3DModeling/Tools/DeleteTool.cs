//// DeleteTool.cs
//using UnityEngine;
//using UnityEngine.ProBuilder;
//using UnityEngine.ProBuilder.MeshOperations;
//using System.Collections.Generic;
//using System.Linq;

//public class DeleteTool : MonoBehaviour
//{
//    public void DeleteSelectedElements(ModelData model, EditMode currentMode, List<ControlPoint> selectedControlPoints)
//    {
//        if (model == null || selectedControlPoints == null || selectedControlPoints.Count == 0)
//        {
//            Debug.LogWarning("Deletion failed");
//            return;
//        }

//        ProBuilderMesh pbMesh = model.editingModel;
//        bool meshWasModified = false;

//        switch (currentMode)
//        {
//            case EditMode.Face:
//                DeleteFaces(pbMesh, selectedControlPoints);
//                meshWasModified = true;
//                break;

//            case EditMode.Edge:
//                DeleteEdges(pbMesh, selectedControlPoints);
//                meshWasModified = true;
//                break;

//            case EditMode.Vertex:
//                DeleteVerticesByDeletingFaces(pbMesh, selectedControlPoints);
//                meshWasModified = true;
//                break;

//            case EditMode.Object:
//                model.DeleteModel();
//                return;
//        }

//        if (meshWasModified)
//        {
//            model.UpdateMeshEdit();
//            ModelEditingPanel.Instance.UpdateEditModel();
//        }
//    }

//    private void DeleteFaces(ProBuilderMesh pbMesh, List<ControlPoint> selectedFaceControlPoints)
//    {
//        var faceIndexesToDelete = selectedFaceControlPoints
//            .Where(cp => cp.editMode == EditMode.Face && cp.faceIndex != -1)
//            .Select(cp => cp.faceIndex);

//        if (faceIndexesToDelete.Any())
//        {
//            pbMesh.DeleteFaces(faceIndexesToDelete);
//        }
//    }

//    private void DeleteEdges(ProBuilderMesh pbMesh, List<ControlPoint> selectedEdgeControlPoints)
//    {
//        var edgesToDelete = new List<Edge>();
//        var allEdges = pbMesh.edges;

//        var selectedVertexIndices = new HashSet<int>(selectedEdgeControlPoints.SelectMany(cp => cp.vertices));

//        foreach (var edge in allEdges)
//        {
//            if (selectedVertexIndices.Contains(edge.a) && selectedVertexIndices.Contains(edge.b))
//            {
  
//                foreach (var cp in selectedEdgeControlPoints)
//                {
//                    if (cp.vertices.Contains(edge.a) && cp.vertices.Contains(edge.b))
//                    {
//                        edgesToDelete.Add(edge);
//                        break;
//                    }
//                }
//            }
//        }

//        if (edgesToDelete.Any())
//        {
//            pbMesh.DeleteEdges(edgesToDelete.Distinct());
//        }
//    }


//    private void DeleteVerticesByDeletingFaces(ProBuilderMesh pbMesh, List<ControlPoint> selectedVertexControlPoints)
//    {
//        var verticesToDelete = new HashSet<int>(selectedVertexControlPoints.SelectMany(cp => cp.vertices));

//        if (!verticesToDelete.Any()) return;

//        var facesToDelete = new List<Face>();
//        foreach (var face in pbMesh.faces)
//        {
//            if (face.indexes.Any(vertexIndex => verticesToDelete.Contains(vertexIndex)))
//            {
//                facesToDelete.Add(face);
//            }
//        }

//        if (facesToDelete.Any())
//        {
//            pbMesh.DeleteFaces(facesToDelete.Select(f => f.faceGroup)); 
//        }
//    }
//}