using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;



/// <summary>
/// This is a custom Grab Transformer that inherits all the features of the
/// XRGeneralGrabTransformer but adds position, rotation, and scale snapping on top,
/// with support for both World and Local coordinate spaces.
/// </summary>
public class SnappingGrabTransformer : XRGeneralGrabTransformer
{
    // Variables

    private bool m_SnapToGrid = false;
    private GizmoSpace snapSpace = GizmoSpace.World;
    private float m_GridSize = 0.5f;
 
    private bool m_SnapToAngle = false;
    private float m_SnapAngle = 45.0f;

    private bool m_SnapToScale = false;
    private float m_ScaleIncrement = 0.25f;

    private bool isSetup = false;



    /// <summary>
    /// On move apply snapping settings
    /// </summary>
    public void OnMoveSnapChanged(float snapVal)
    {
        m_GridSize = snapVal;

        if (m_GridSize == 0) m_SnapToGrid = false;
        else m_SnapToGrid = true;
    }
    /// <summary>
    /// On rotate apply snapping settings
    /// </summary>
    public void OnRotateSnapChanged(float snapVal)
    {
        m_SnapAngle = snapVal;

        if (m_SnapAngle == 0) m_SnapToAngle = false;
        else m_SnapToAngle = true;
    }
    /// <summary>
    /// On scale apply snapping settings
    /// </summary>
    public void OnScaleSnapChanged(float snapVal)
    {
        m_ScaleIncrement = snapVal;

        if (m_ScaleIncrement == 0) m_SnapToScale = false;
        else m_SnapToScale = true;
    }
    /// <summary>
    /// Gizmo space changed update it
    /// </summary>
    public void OnGizmoSpaceChanged()
    {
        snapSpace = ModelEditingPanel.Instance.GetGizmoSpace();
    }
    /// <summary>
    /// Process all snapping
    /// </summary>
    public override void Process(XRGrabInteractable grabInteractable, XRInteractionUpdateOrder.UpdatePhase updatePhase, ref Pose targetPose, ref Vector3 localScale)
    {
        if(isSetup == false)
        {
            ModelEditingPanel.Instance.OnMoveSnapChanged.AddListener(OnMoveSnapChanged);
            ModelEditingPanel.Instance.OnRotateSnapChanged.AddListener(OnRotateSnapChanged);
            ModelEditingPanel.Instance.OnScaleSnapChanged.AddListener(OnScaleSnapChanged);
            ModelEditingPanel.Instance.OnGizmoSpaceChanged.AddListener(OnGizmoSpaceChanged);


            OnMoveSnapChanged(ModelEditingPanel.Instance.GetMoveSnap());
            OnRotateSnapChanged(ModelEditingPanel.Instance.GetRotateSnap());
            OnScaleSnapChanged(ModelEditingPanel.Instance.GetScaleSnap());
            OnGizmoSpaceChanged();

            isSetup = true;
        }

        //First, call the base method to get the unsnapped target pose.
        base.Process(grabInteractable, updatePhase, ref targetPose, ref localScale);

        //Apply our custom snapping logic to the calculated pose.
        if (m_SnapToGrid && m_GridSize > 0)
        {
            targetPose.position = SnapPosition(grabInteractable, targetPose.position);
        }

        if (m_SnapToAngle && m_SnapAngle > 0)
        {
            targetPose.rotation = SnapRotation(grabInteractable, targetPose.rotation);
        }

        if (m_SnapToScale && m_ScaleIncrement > 0 && grabInteractable.interactorsSelecting.Count > 1)
        {
            localScale = SnapScale(localScale);
        }
    }
    /// <summary>
    /// Process position snap
    /// </summary>
    private Vector3 SnapPosition(XRGrabInteractable grabInteractable, Vector3 position)
    {
        if (snapSpace == GizmoSpace.World)
        {
            // World space snapping
            float x = Mathf.Round(position.x / m_GridSize) * m_GridSize;
            float y = Mathf.Round(position.y / m_GridSize) * m_GridSize;
            float z = Mathf.Round(position.z / m_GridSize) * m_GridSize;
            return new Vector3(x, y, z);
        }
        else // Local space snapping
        {
            Transform parent = grabInteractable.transform.parent;
            if (parent == null)
                return position; // Cannot snap locally without a parent

            // Convert world position to parent's local space
            Vector3 localPosition = parent.InverseTransformPoint(position);

            // Snap the local position values
            float x = Mathf.Round(localPosition.x / m_GridSize) * m_GridSize;
            float y = Mathf.Round(localPosition.y / m_GridSize) * m_GridSize;
            float z = Mathf.Round(localPosition.z / m_GridSize) * m_GridSize;

            // Convert the snapped local position back to world space
            return parent.TransformPoint(new Vector3(x, y, z));
        }
    }
    /// <summary>
    /// Process rotation snap
    /// </summary>
    private Quaternion SnapRotation(XRGrabInteractable grabInteractable, Quaternion rotation)
    {
        if (snapSpace == GizmoSpace.World)
        {
            // World space snapping
            Vector3 eulerRotation = rotation.eulerAngles;
            float x = Mathf.Round(eulerRotation.x / m_SnapAngle) * m_SnapAngle;
            float y = Mathf.Round(eulerRotation.y / m_SnapAngle) * m_SnapAngle;
            float z = Mathf.Round(eulerRotation.z / m_SnapAngle) * m_SnapAngle;
            return Quaternion.Euler(x, y, z);
        }
        else // Local space snapping
        {
            Transform parent = grabInteractable.transform.parent;
            if (parent == null)
                return rotation; // Cannot snap locally without a parent

            // Convert world rotation to parent's local space
            Quaternion localRotation = Quaternion.Inverse(parent.rotation) * rotation;

            // Snap the local euler angles
            Vector3 localEuler = localRotation.eulerAngles;
            float x = Mathf.Round(localEuler.x / m_SnapAngle) * m_SnapAngle;
            float y = Mathf.Round(localEuler.y / m_SnapAngle) * m_SnapAngle;
            float z = Mathf.Round(localEuler.z / m_SnapAngle) * m_SnapAngle;

            // Convert the snapped local rotation back to world space
            return parent.rotation * Quaternion.Euler(x, y, z);
        }
    }
    /// <summary>
    /// Process scale snap
    /// </summary>
    private Vector3 SnapScale(Vector3 scale)
    {
        float x = Mathf.Round(scale.x / m_ScaleIncrement) * m_ScaleIncrement;
        float y = Mathf.Round(scale.y / m_ScaleIncrement) * m_ScaleIncrement;
        float z = Mathf.Round(scale.z / m_ScaleIncrement) * m_ScaleIncrement;
        return new Vector3(x, y, z);
    }
}