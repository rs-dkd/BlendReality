using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectUIItem : MonoBehaviour
{
    public ModelData modelData;
    public TMPro.TMP_InputField modelNameInputfield;
    public GameObject isSelectedGraphic;

    public void DeleteUI()
    {
        SelectionManager.Instance.OnSelectionChanged.RemoveListener(CheckIfSelected);
        Destroy(this.gameObject);
    }
    public void Setup(ModelData _modelData)
    {
        modelData = _modelData;
        modelNameInputfield.text = modelData.GetName();

        SelectionManager.Instance.OnSelectionChanged.AddListener(CheckIfSelected);
    }

    public void RenameModel()
    {
        modelData.UpdateName(modelNameInputfield.text);
    }

    public void DeleteModel()
    {
        modelData.DeleteModel();
    }

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

    public void ToggleSelection()
    {
        Debug.Log("feef");
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

    public void SelectModel()
    {
        SelectionManager.Instance.SelectModel(modelData);
    }

    public void DeselectModel()
    {
        SelectionManager.Instance.RemoveModelFromSelection(modelData);

    }
}
