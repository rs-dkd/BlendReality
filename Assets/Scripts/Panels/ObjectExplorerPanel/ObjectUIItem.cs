using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Each model has a item in the object explorer planel
/// Allows the user to select, delete and change name
/// </summary>
public class ObjectUIItem : KeyboardInputField
{


    [Tooltip("Graphic to show hide to show if selected")]
    [SerializeField] private GameObject isSelectedGraphic;


    private ModelData modelData;


    /// <summary>
    /// Delete the UI - called when the model is deleted
    /// </summary>
    public void DeleteUI()
    {
        SelectionManager.Instance.OnSelectionChanged.RemoveListener(CheckIfSelected);
        Destroy(this.gameObject);
    }
    /// <summary>
    /// Setup the UI
    /// </summary>
    public void Setup(ModelData _modelData)
    {
        modelData = _modelData;
        inputField.text = modelData.GetName();
        RenameModel();

        SelectionManager.Instance.OnSelectionChanged.AddListener(CheckIfSelected);
    }
    /// <summary>
    /// Update the name of the model
    /// </summary>
    public void RenameModel()
    {
        modelData.UpdateName(inputField.text);
        SetValue(inputField.text);
    }
    /// <summary>
    /// Delete the model - called from the UI button
    /// </summary>
    public void DeleteModel()
    {
        modelData.DeleteModel();
    }
    /// <summary>
    /// Check if model is selected and show hide the graphic
    /// </summary>
    public void CheckIfSelected(List<ModelData> models)
    {
        if (modelData.GetIsSelected())
        {
            isSelectedGraphic.SetActive(true);
        }
        else
        {
            isSelectedGraphic.SetActive(false);
        }
    }
    /// <summary>
    /// Toggle selecting the model - called form the UI toggle
    /// </summary>
    public void ToggleSelection()
    {
        if (modelData.GetIsSelected())
        {
            isSelectedGraphic.SetActive(false);
            DeselectModel();
        }
        else
        {
            isSelectedGraphic.SetActive(true);
            SelectModel();
        }
    }
    /// <summary>
    /// Select the model - called from the toggle selction function
    /// </summary>
    public void SelectModel()
    {
        SelectionManager.Instance.AddModelToSelection(modelData);
    }
    /// <summary>
    /// Deselect the model - called from the toggle selction function
    /// </summary>
    public void DeselectModel()
    {
        SelectionManager.Instance.RemoveModelFromSelection(modelData);

    }
}
