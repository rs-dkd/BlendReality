// ExtrudeTool.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.UI;

/// <summary>
/// Extrude Tool
/// Extrudes a face or faces by distance, also can extrude separate or by group
/// ALso allows for subdivisions on the extrusion
/// </summary>

public class ExtrudeTool : OperationTool
{
    [Header("UI Elements")]
    [Tooltip("Extrude amount")]
    [SerializeField] private SliderUI slider;
    [Tooltip("Subdivion amount")]
    [SerializeField] private SliderUI subdivisionsSlider;
    [Tooltip("Toggle for extruding by group or separate faces")]
    [SerializeField] private Toggle byGroupToggle;



    /// <summary>
    /// Performs the Extrude operation on the selected faces.
    /// </summary>
    public void Extrude()
    {
        //Check if can preform the tool
        if (CanPerformTool() == false) return;

        //Get selected mesh
        ProBuilderMesh mesh = SelectionManager.Instance.GetFirstSelected().GetEditModel(); 
        if (mesh == null)
        {
            Debug.LogError("Extrude Tool: No target mesh found.");
            return;
        }

        //Get selected faces
        List<Face> faces = new List<Face>();
        List<BaseControlPoint> selectedControlPoints = ModelEditingPanel.Instance.GetControlPoints();
        for (int i = 0; i < selectedControlPoints.Count; i++)
        {
            faces.Add(((FaceControlPoint)selectedControlPoints[i]).GetFace());
        }

        //Extrude method (group or separate)
        ExtrudeMethod method = ExtrudeMethod.FaceNormal;
        if (byGroupToggle.isOn == false)
        {
            method = ExtrudeMethod.IndividualFaces;
        }
  

        //Number of extrudes
        int subdivisions = (int)subdivisionsSlider.GetValueAsFloat() + 1;
        //Get dist for each extrude
        float distanceForEach = slider.GetValueAsFloat() / subdivisions;
        bool hasExtruded = false;

        //Extrude by subdivions
        for (int i = 0; i < subdivisions; i++)
        {
            //Extrude
            Face[] newFaces = ExtrudeElements.Extrude(
                mesh,
                faces,
                method,
                distanceForEach
            );


            if (newFaces != null && newFaces.Length > 0)
            {
                hasExtruded = true;
            }
            else
            {
                Debug.LogError("Extrude Tool failed to create new faces.");
                return;
            }
        }



        if (hasExtruded == true)
        {
            //Refresh mesh and control points
            mesh.ToMesh();
            SelectionManager.Instance.GetFirstSelected().UpdateMeshEdit();
            ModelEditingPanel.Instance.UpdateEditModel();
        }
        else
        {
            Debug.LogError("Extrude Tool failed to create new faces.");
        }
    }

    /// <summary>
    /// Only if Editmode is face
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
    /// Only if Editmode is face and have selected control points
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
 