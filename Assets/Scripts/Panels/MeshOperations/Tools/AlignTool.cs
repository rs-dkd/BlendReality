using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Mesh operation
/// Aligns in the x, y, or z axis
/// Allows aligning selected verts, edges, faces, or models
/// </summary>
public class AlignTool : OperationTool
{


    /// <summary>
    /// Align selected control points or objects to an axis
    /// </summary>
    public void AlignToAxis(string axis)
    {
        //check if can preform the tool
        if (CanPerformTool() == false) return;

        float total = 0f;
        int count = 0;
        List<Vector3> positions = new List<Vector3>();

        //Align objects to an axis
        if (ModelEditingPanel.Instance.GetEditMode() == EditMode.Object)
        {
            List<ModelData> models = SelectionManager.Instance.GetSelectedModels();
            if (models.Count == 0) return;
            foreach (ModelData model in models)
            {
                if(axis=="X")total += model.GetPosition().x;
                else if(axis=="Y")total += model.GetPosition().y;
                else if(axis=="Z")total += model.GetPosition().z;
                positions.Add(model.GetPosition());
            }
            count = models.Count;

            float center = total / count;

            for (int i = 0; i < count; i++)
            {
                if (axis == "X") models[i].SetPosition(new Vector3(center, models[i].GetPosition().y, models[i].GetPosition().z));
                else if (axis == "Y") models[i].SetPosition(new Vector3(models[i].GetPosition().x, center, models[i].GetPosition().z));
                else if (axis == "Z") models[i].SetPosition(new Vector3(models[i].GetPosition().x, models[i].GetPosition().y, center));


            }
        }
        else
        {
            //Align verts, center of edges, and center of faces
            List<BaseControlPoint> points = ModelEditingPanel.Instance.GetControlPoints();
            if (points.Count == 0) return;
            foreach (BaseControlPoint point in points)
            {
                if (axis == "X") total += point.GetCurrentPosition().x;
                else if (axis == "Y") total += point.GetCurrentPosition().y;
                else if (axis == "Z") total += point.GetCurrentPosition().z;

                positions.Add(point.GetCurrentPosition());
            }
            count = points.Count;

            float center = total / count;


            for (int i = 0; i < count; i++)
            {
                if (axis == "X") points[i].MoveByOffset(new Vector3(center - positions[i].x, 0, 0));
                else if (axis == "Y") points[i].MoveByOffset(new Vector3(0, center - positions[i].y, 0));
                else if (axis == "Z") points[i].MoveByOffset(new Vector3(0, 0, center - positions[i].z));
            }
        }
    }

    /// <summary>
    /// Align selected control points to the center in the X axis
    /// </summary>
    public void AlignToX()
    {
        AlignToAxis("X");
    }



    /// <summary>
    /// Align selected control points to the center in the Y axis
    /// </summary>
    public void AlignToY()
    {
        AlignToAxis("Y");
    }
    /// <summary>
    /// Align selected control points to the center in the Z axis
    /// </summary>
    public void AlignToZ()
    {
        AlignToAxis("Z");
    }

    /// <summary>
    /// Show tool if is not Object or Pivot edit mode
    /// </summary>
    public override bool CanShowTool()
    {
        return true;
    }
    /// <summary>
    /// Cant perform tool if object or pivot model or if no control points selected
    /// </summary>
    public override bool CanPerformTool()
    {
        if (
            (ModelEditingPanel.Instance.GetEditMode() == EditMode.Object && SelectionManager.Instance.GetSelectedModels().Count == 0) ||
            ((ModelEditingPanel.Instance.GetEditMode() == EditMode.Vertex || ModelEditingPanel.Instance.GetEditMode() == EditMode.Edge || ModelEditingPanel.Instance.GetEditMode() == EditMode.Face)
            && ModelEditingPanel.Instance.GetControlPoints().Count == 0)
            )
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
