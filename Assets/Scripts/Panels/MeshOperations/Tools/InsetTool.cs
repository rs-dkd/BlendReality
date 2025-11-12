using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.UI;

/// <summary>
/// Inset tool
/// Only for faces
/// </summary>
public class InsetTool : OperationTool
{
    [Header("UI Elements")]
    [Tooltip("Extrude amount")]
    [SerializeField] private SliderUI slider;
    [Tooltip("Toggle for extruding by group or separate faces")]
    [SerializeField] private Toggle byGroupToggle;


    /// <summary>
    /// Performs the Inset operation on the selected faces.
    /// </summary>
    public void Inset()
    {
        //todo same as bevel tool but no extrude
    }

    /// <summary>
    /// Only for face
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
    /// Only if face and has selected control points
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