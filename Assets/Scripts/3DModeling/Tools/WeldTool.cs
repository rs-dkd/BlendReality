using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeldTool : OperationTool
{




    public override bool CanShowTool()
    {
        if (ModelEditingPanel.Instance.GetEditMode() == EditMode.Object || ModelEditingPanel.Instance.GetEditMode() == EditMode.Pivot)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public override bool CanPerformTool()
    {
        if (ModelEditingPanel.Instance.GetControlPoints().Count == 0 || ModelEditingPanel.Instance.GetEditMode() == EditMode.Object || ModelEditingPanel.Instance.GetEditMode() == EditMode.Pivot)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}