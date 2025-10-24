using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlignTool : OperationTool
{



    public void AlignToX()
    {
        List<ControlPoint> points = ModelEditingPanel.Instance.GetControlPoints();

        if (points.Count == 0) return;

        float totalX = 0f;
        foreach (ControlPoint point in points)
        {
            totalX += point.GetCurrentPosition().x;
        }

        float centerX = totalX / points.Count;


        for (int i = 0; i < points.Count; i++)
        {
            ControlPoint point = points[i];

            Vector3 pos = point.GetCurrentPosition();
            point.AddOffsetToControlPointAndVertsPosition(new Vector3(centerX - pos.x, 0,0));
        }
    }
    public void AlignToY()
    {
        List<ControlPoint> points = ModelEditingPanel.Instance.GetControlPoints();

        if (points.Count == 0) return;

        float totalY = 0f;
        foreach (ControlPoint point in points)
        {
            totalY += point.GetCurrentPosition().y;
        }

        float centerY = totalY / points.Count;


        for (int i = 0; i < points.Count; i++)
        {
            ControlPoint point = points[i];

            Vector3 pos = point.GetCurrentPosition();
            point.AddOffsetToControlPointAndVertsPosition(new Vector3(0, centerY - pos.y, 0));
        }
    }
    public void AlignToZ()
    {
        List<ControlPoint> points = ModelEditingPanel.Instance.GetControlPoints();

        if (points.Count == 0) return;

        float totalZ = 0f;
        foreach (ControlPoint point in points)
        {
            totalZ += point.GetCurrentPosition().z;
        }

        float centerZ = totalZ / points.Count;


        for (int i = 0; i < points.Count; i++)
        {
            ControlPoint point = points[i];

            Vector3 pos = point.GetCurrentPosition();
            point.AddOffsetToControlPointAndVertsPosition(new Vector3(0,0, centerZ - pos.z));
        }
    }

    public override bool CanShowTool()
    {
        if(ModelEditingPanel.Instance.GetEditMode() == EditMode.Object || ModelEditingPanel.Instance.GetEditMode() == EditMode.Pivot)
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
