using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MeshOperationsPanel : MonoBehaviour
{
    public TMP_Dropdown toolsDropdown;
    public GameObject panel;
    public Transform toolsParent;
    public OperationTool[] operationTools;

    public void Start()
    {
        operationTools = toolsParent.GetComponentsInChildren<OperationTool>();
        toolsDropdown.onValueChanged.AddListener(OnToolSelected);

        UpdateToolDropDown();

        ModelEditingPanel.Instance.OnEditModeChanged.AddListener(UpdateToolDropDown);
        SelectionManager.Instance.OnSelectionChanged.AddListener(SelectionUpdated);

        SelectionUpdated(new List<ModelData>());
        panel.SetActive(false);
        OnToolSelected(0);
    }

    public void SelectionUpdated(List<ModelData> models)
    {
        if(models.Count > 0)
        {
            panel.SetActive(true);
        }
        else
        {
            panel.SetActive(false);

        }
    }

    public void UpdateToolDropDown() {
        Debug.Log("ff");
        toolsDropdown.ClearOptions();

        toolsDropdown.options.Add(new TMP_Dropdown.OptionData("None"));
        for (int i = 0; i < operationTools.Length; i++)
        {
            if (operationTools[i].CanShowTool())
            {
        Debug.Log(operationTools[i].GetToolName());
                toolsDropdown.options.Add(new TMP_Dropdown.OptionData(operationTools[i].GetToolName()));
            }
        }

    }

    public void OnToolSelected(int index)
    {
        for (int i = 0; i < operationTools.Length; i++)
        {
            if (toolsDropdown.options[index].text == operationTools[i].GetToolName())
            {
                operationTools[i].ShowTool();
            }
            else
            {
                operationTools[i].HideTool();
            }
        }
    }
}
