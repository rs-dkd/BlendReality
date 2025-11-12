using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.UI;

/// <summary>
/// Bevel
/// Extrudes a face or faces and scales them for a bevel effect
/// </summary>
public class BevelTool : OperationTool
{

    [Header("UI Elements")]
    [Tooltip("Extrude amount")]
    [SerializeField] private SliderUI extrudeSlider;
    [Tooltip("Amount (distance) to inset by")]
    [SerializeField] private SliderUI insetSlider;
    [Tooltip("Toggle for bevel by group or separate faces")]
    [SerializeField] private Toggle byGroupToggle;


    /// <summary>
    /// Performs the Bevel operation on the selected faces.
    /// </summary>
    public void Bevel()
    {
        //Check if can preform the tool
        if (CanPerformTool() == false) return;

        ProBuilderMesh mesh = SelectionManager.Instance.GetFirstSelected().GetEditModel();

        if (mesh == null)
        {
            Debug.LogError("Inset Tool: No target mesh found.");
            return;
        }

        // Get the currently selected faces
        List<Face> faces = new List<Face>();
        List<BaseControlPoint> selectedControlPoints = ModelEditingPanel.Instance.GetControlPoints();
        for (int i = 0; i < selectedControlPoints.Count; i++)
        {
            faces.Add(((FaceControlPoint)selectedControlPoints[i]).GetFace());
        }
        if (faces.Count == 0)
        {
            Debug.LogWarning("Inset Tool: No faces selected.");
            return;
        }


        //By group or separate
        ExtrudeMethod method = ExtrudeMethod.FaceNormal;
        if (byGroupToggle.isOn == false)
        {
            method = ExtrudeMethod.IndividualFaces;
        }

        //Inset amount
        float insetAmount = insetSlider.GetValueAsFloat();
        float scaleFactor = 1.0f - insetAmount;
        if (scaleFactor < 0f) scaleFactor = 0f;

        //Extrude first
        Face[] newSideFaces = ExtrudeElements.Extrude(
            mesh,
            faces,
            method,
            extrudeSlider.GetValueAsFloat()
        );

        bool hasOperated = false;

        //Scale for bevel
        if (newSideFaces != null && newSideFaces.Length > 0)
        {
            // Get all vertices from the mesh
            IList<Vertex> vertices = mesh.GetVertices();
            //Get all *distinct* indices from both the new top faces and side faces
            int[] topFaceIndices = faces.SelectMany(x => x.distinctIndexes).Distinct().ToArray();
            int[] sideFaceIndices = newSideFaces.SelectMany(x => x.distinctIndexes).Distinct().ToArray();
            //Combine into one list
            HashSet<int> allInvolvedIndices = new HashSet<int>(topFaceIndices);
            allInvolvedIndices.UnionWith(sideFaceIndices);

            //Positins of the top faces verts
            HashSet<Vector3> topVertexPositions = new HashSet<Vector3>();
            foreach (int index in topFaceIndices)
            {
                topVertexPositions.Add((Vector3)vertices[index].position);
            }

            //Get all top face verts and the verts from the side faces that are in the same position (The top verts of the side faces)
            List<int> allVerticesToScale = new List<int>();
            foreach (int index in allInvolvedIndices)
            {
                if (topVertexPositions.Contains((Vector3)vertices[index].position))
                {
                    allVerticesToScale.Add(index);
                }
            }

            //Final vert array
            int[] distinctVertexIndices = allVerticesToScale.Distinct().ToArray();



            // Calculate the pivot point (center) of these vertices
            Vector3 pivot = Vector3.zero;
            foreach (int index in distinctVertexIndices)
            {
                pivot += (Vector3)vertices[index].position;
            }
            pivot /= distinctVertexIndices.Length;

            //Scale each vertex towards the pivot
            foreach (int index in distinctVertexIndices) 
            {
                Vector3 pos = (Vector3)vertices[index].position;
                Vector3 dir = pos - pivot;
                pos = pivot + (dir * scaleFactor);
                vertices[index].position = pos;
            }

            //Apply all vertex changes back to the mesh
            mesh.SetVertices(vertices);

            hasOperated = true;
        }
        else
        {
            Debug.LogError("Inset Tool failed to create new faces during the extrude-by-0 step.");
            return;
        }

        //Apply changes to the mesh
        if (hasOperated == true)
        {
            //Refresh mesh and control points
            mesh.ToMesh();
            SelectionManager.Instance.GetFirstSelected().UpdateMeshEdit();
            ModelEditingPanel.Instance.UpdateEditModel();
        }
    }


    /// <summary>
    /// Show tool if face edit mode is selected
    /// </summary>
    public override bool CanShowTool()
    {
        if (ModelEditingPanel.Instance.GetEditMode() == EditMode.Face)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    /// <summary>
    /// Can perform tool only if face and has selected control points 
    /// </summary>
    public override bool CanPerformTool()
    {
        if (ModelEditingPanel.Instance.GetControlPoints().Count != 0 && ModelEditingPanel.Instance.GetEditMode() == EditMode.Face)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}