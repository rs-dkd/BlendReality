using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InsetTool : OperationTool
{




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
