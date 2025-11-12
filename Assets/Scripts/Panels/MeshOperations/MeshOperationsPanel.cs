using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.UI;

/// <summary>
/// Handles showing hiding the mesh operations on the dropdown and the panel itself
/// </summary>
public class MeshOperationsPanel : MonoBehaviour
{
 
    [Tooltip("The tools dropdown")]
    [SerializeField] private TMP_Dropdown toolsDropdown;
    [Tooltip("The tools parent to ff")]
    [SerializeField] private Transform toolsParent;
    [Tooltip("Mesh ops main panel to hide and show")]
    [SerializeField] private GameObject mainPanelGO;
    [Tooltip("Lazy Follow the selected object")]
    [SerializeField] private LazyFollow lazyFollow;




    private OperationTool[] operationTools;


    /// <summary>
    /// Setups the events and listeners and gets the op panels
    /// </summary>
    public void Start()
    {
        operationTools = toolsParent.GetComponentsInChildren<OperationTool>();
        toolsDropdown.onValueChanged.AddListener(OnToolSelected);

        UpdateToolDropDown();

        ModelEditingPanel.Instance.OnEditModeChanged.AddListener(UpdateToolDropDown);

        SelectionManager.Instance.OnSelectionChanged.AddListener(ToggleMeshOpPanel);
        PanelManager.Instance.OnPanelChanged.AddListener(PanelChanged);
    }
    /// <summary>
    /// On panel change show or hide the mesh op panel if it is the edit panel
    /// </summary>
    public void PanelChanged(UIPanel panel)
    {
        if(panel.name == "MeshCreation")
        {
            mainPanelGO.SetActive(false);
        }
        else if(panel.name == "EditPanel")
        {
            if (toolsDropdown.options.Count <= 1)
            {
                mainPanelGO.SetActive(false);
            }
            else
            {
                mainPanelGO.SetActive(true);
            }
        }
    }
    /// <summary>
    /// Selection changed update mesh ops panel
    /// </summary>
    public void ToggleMeshOpPanel(List<ModelData> models)
    {
        UpdateToolDropDown();
    }
    /// <summary>
    /// Update the dropdown
    /// </summary>
    public void UpdateToolDropDown() {
        toolsDropdown.ClearOptions();

        toolsDropdown.options.Add(new TMP_Dropdown.OptionData("None"));

        if (SelectionManager.Instance.GetSelectedModels().Count != 0)
        {
            for (int i = 0; i < operationTools.Length; i++)
            {
                if (operationTools[i].CanShowTool())
                {
                    toolsDropdown.options.Add(new TMP_Dropdown.OptionData(operationTools[i].GetToolName()));
                }

            }
        }


        if (toolsDropdown.options.Count <= 1)
        {
            mainPanelGO.SetActive(false);
        }
        else
        {
            mainPanelGO.SetActive(true);
        }

        OnToolSelected(0);
    }
    /// <summary>
    /// Dropdown item selected show and hide the correct tool panel
    /// </summary>
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
