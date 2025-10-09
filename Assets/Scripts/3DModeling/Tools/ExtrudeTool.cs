// ExtrudeTool.cs
using UnityEngine;
using UnityEngine.ProBuilder;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder.MeshOperations;

public class ExtrudeTool : MonoBehaviour
{
    ///// <summary>
    ///// Performs inset and extrusion on the selected faces of a model.
    ///// </summary>
    //public void PerformExtrude(ModelData model, List<ControlPoint> selectedFaceControlPoints, float extrudeDistance, float insetAmount)
    //{
    //    if (model == null || selectedFaceControlPoints == null || selectedFaceControlPoints.Count == 0)
    //    {
    //        Debug.LogWarning("Extrusion failed: No model or faces selected.");
    //        return;
    //    }

    //    ProBuilderMesh pbMesh = model.editingModel;

    //    List<Face> facesToProcess = new List<Face>();
    //    foreach (var cp in selectedFaceControlPoints)
    //    {
    //        if (cp.editMode == EditMode.Face && cp.faceIndex >= 0 && cp.faceIndex < pbMesh.faces.Count)
    //        {
    //            facesToProcess.Add(pbMesh.faces[cp.faceIndex]);
    //        }
    //    }

    //    if (facesToProcess.Count == 0)
    //    {
    //        Debug.LogWarning("No valid faces found for extrusion.");
    //        return;
    //    }

    //    if (insetAmount > 0.001f)
    //    {
    //        Face[] insetFaces = pbMesh.Extrude(facesToProcess, ExtrudeMethod.FaceNormal, 0f).ToArray();

    //        if (insetFaces != null && insetFaces.Length > 0)
    //        {
    //            // ---- START: CORRECTED INSET LOGIC ----

    //            // 1. Get the unique vertex indices of the newly created faces.
    //            var newVertexIndices = insetFaces.SelectMany(f => f.distinctIndexes).Distinct().ToList();

    //            // 2. Get the current positions of these vertices. We use the mesh's positions array directly.
    //            var positions = pbMesh.positions;
    //            var targetPositions = newVertexIndices.Select(index => positions[index]).ToList();

    //            // 3. Calculate the center point of these vertices.
    //            Vector3 center = Vector3.zero;
    //            foreach (var pos in targetPositions)
    //            {
    //                center += pos;
    //            }
    //            center /= targetPositions.Count;

    //            // 4. Transform center to world space for accurate calculations.
    //            center = pbMesh.transform.TransformPoint(center);

    //            // 5. Loop through each vertex, calculate its new scaled position, and apply it.
    //            float scaleFactor = 1.0f - insetAmount;
    //            for (int i = 0; i < newVertexIndices.Count; i++)
    //            {
    //                int vertexIndex = newVertexIndices[i];

    //                // Convert local vertex position to world space.
    //                Vector3 worldPos = pbMesh.transform.TransformPoint(positions[vertexIndex]);

    //                // Calculate the new scaled position in world space.
    //                Vector3 newWorldPos = center + (worldPos - center) * scaleFactor;

    //                // A "shared index" refers to the unique vertex position that multiple triangles might share.
    //                // We use the 'lookup' table to find it from the regular vertex index.
    //                int sharedIndex = pbMesh.lookup[vertexIndex];

    //                // Apply the new position using the correct function.
    //                VertexPositioning.SetSharedVertexPosition(pbMesh, sharedIndex, newWorldPos);
    //            }

    //            // ---- END: CORRECTED INSET LOGIC ----

    //            // The faces to be extruded are now the newly created and scaled (inset) faces.
    //            facesToProcess = insetFaces.ToList();
    //        }
    //    }

    //    if (extrudeDistance != 0)
    //    {
    //        pbMesh.Extrude(facesToProcess, ExtrudeMethod.FaceNormal, extrudeDistance);
    //    }

    //    // Finalize the mesh and update related components.
    //    pbMesh.ToMesh();
    //    pbMesh.Refresh(RefreshMask.All);

    //    if (model.meshCollider != null)
    //    {
    //        model.meshCollider.sharedMesh = pbMesh.mesh;
    //    }

    //    ModelEditingPanel.Instance.UpdateEditModel();
    //}
}