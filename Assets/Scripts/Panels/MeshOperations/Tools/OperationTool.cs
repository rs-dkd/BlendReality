using UnityEngine;

/// <summary>
/// Abstract class for operation tool
/// </summary>
public abstract class OperationTool: MonoBehaviour {

    [Header("Tool Name Config")]
    [Tooltip("Tool name")]
    [SerializeField] private string toolName;
    [Tooltip("Panel GameObject")]
    [SerializeField] private GameObject panel;

    /// <summary>
    /// Get tool name
    /// </summary>
    public string GetToolName()
    {
        return toolName;
    }
    /// <summary>
    /// Show the tool panel
    /// </summary>
    public void ShowTool()
    {
        panel.SetActive(true);
    }
    /// <summary>
    /// Hide the tool panel
    /// </summary>
    public void HideTool()
    {

        panel.SetActive(false);
    }
    /// <summary>
    /// Allows to show hide tool in the mesh operations dropdown
    /// </summary>
    public virtual bool CanShowTool()
    {
        return true;
    }
    /// <summary>
    /// A check if the user can preform the tool
    /// </summary>
    public virtual bool CanPerformTool()
    {

        return true;
    }

}
