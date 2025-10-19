using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class OperationTool: MonoBehaviour {

    public string toolName;
    public GameObject panel;
    public string GetToolName()
    {
        return toolName;
    }

    public void ShowTool()
    {
        panel.SetActive(true);
    }

    public void HideTool()
    {

        panel.SetActive(false);
    }

    public virtual bool CanShowTool()
    {
        return true;
    }

    public virtual bool CanPerformTool()
    {

        return true;
    }

}
