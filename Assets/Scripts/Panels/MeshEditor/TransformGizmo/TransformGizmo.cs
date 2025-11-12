using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Transformers;
public enum GizmoSpace
{
    World, 
    Local 
}
public enum TransformType
{
    Select,
    Free,
    Move,
    Rotate,
    Scale
}
/// <summary>
/// Handles the move, rotate, and scale tool in world and local space
/// </summary>
/// TODO: Finish Scale and Rotate and World and Local Space
public class TransformGizmo : MonoBehaviour
{
    //Singleton Pattern
    public static TransformGizmo Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        axisParent.gameObject.SetActive(false);
    }
    [Tooltip("Axes Parent Transform")]
    [SerializeField] private Transform axisParent;
    [Tooltip("The Axes[] transforms")]
    [SerializeField] private Transform[] axes;
    [Tooltip("The Axes[] Line Renderers")]
    [SerializeField] private LineRenderer[] axesLineRenders;
    [Tooltip("The Axes[] capsules colliders")]
    [SerializeField] private CapsuleCollider[] axesCapsules;
    [Tooltip("The Axes[] GrabTrans")]
    [SerializeField] private XRGeneralGrabTransformer[] xrGrabTrans;
    [Tooltip("The Axes[] GrabInteractables")]
    [SerializeField] private XRGrabInteractable[] xrGrabInters;

    [Tooltip("Info Panel GO")]
    [SerializeField] private GameObject transformInfoPanel;
    [Tooltip("Info Panel Title Text")]
    [SerializeField] private TMP_Text infoTitle;
    [Tooltip("Info Panel Value Text")]
    [SerializeField] private TMP_Text infoValue;



    private Vector3 previousPosition;
    private int currentAxis = -1;
    private bool isReset = false;
    private Quaternion initialRotation;
    private Vector3 initialDirection;
    private float initialGrabDistance;
    private List<Vector3> initialControlPointPositions = new List<Vector3>();
    private Vector3 startPosition;





    /// <summary>
    /// Show info panel
    /// </summary>
    public void ShowInfoPanel()
    {
        transformInfoPanel.SetActive(true);
    }
    /// <summary>
    /// Hide info panel
    /// </summary>
    public void HideInfoPanel()
    {
        transformInfoPanel.SetActive(false);
    }
    /// <summary>
    /// Update info panel on move rotate or scale change
    /// </summary>
    public void UpdateInfoPanel()
    {
        ShowInfoPanel();

        if (ModelEditingPanel.Instance.GetTransformType() == TransformType.Move)
        {
            Vector3 deltaPos = previousPosition - startPosition;
            infoTitle.text = "Translate X:\nTranslate Y:\nTranslate Z:";

            if (ViewManager.Instance.GetUnitSystem() == GridUnitSystem.Imperial)
            {
                infoValue.text = $"{MetricConverter.ToFeetAndInches(deltaPos.x)}\n{MetricConverter.ToFeetAndInches((float)Math.Round(deltaPos.y, 2))}\n{MetricConverter.ToFeetAndInches((float)Math.Round(deltaPos.z, 2))}";
            }
            else
            {
                infoValue.text = $"{(float)Math.Round(deltaPos.x, 2)}m\n{(float)Math.Round(deltaPos.y, 2)}m\n{(float)Math.Round(deltaPos.z, 2)}m";
            }
        }
    }
    /// <summary>
    /// Setup the listeners and events
    /// </summary>
    private void Start()
    {
        ModelEditingPanel.Instance.OnTransformTypeChanged.AddListener(TransformModeUpdated);
        ModelEditingPanel.Instance.OnGizmoSpaceChanged.AddListener(GizmoSpaceUpdated);
        ModelEditingPanel.Instance.OnControlPointsChanged.AddListener(ControlPointsUpdated);
        ModelEditingPanel.Instance.OnEditModeChanged.AddListener(ControlPointsUpdated);

        xrGrabInters[0].selectEntered.AddListener((args) => SelectAxis(0));
        xrGrabInters[0].selectExited.AddListener((args) => DeSelectAxis());


        xrGrabInters[1].selectEntered.AddListener((args) => SelectAxis(1));
        xrGrabInters[1].selectExited.AddListener((args) => DeSelectAxis());

        xrGrabInters[2].selectEntered.AddListener((args) => SelectAxis(2));
        xrGrabInters[2].selectExited.AddListener((args) => DeSelectAxis());

        xrGrabInters[3].selectEntered.AddListener((args) => SelectAxis(3));
        xrGrabInters[3].selectExited.AddListener((args) => DeSelectAxis());
    }

    /// <summary>
    /// Control points were updated - update the gizmo
    /// </summary>
    public void ControlPointsUpdated()
    {
        UpdateGizmoGroupCenter();
    }
    /// <summary>
    /// Update the gizmos positions based on the control points
    /// </summary>
    public void UpdateGizmoGroupCenter()
    {
        if(ModelEditingPanel.Instance.GetControlPoints().Count == 0)
        {
            axisParent.gameObject.SetActive(false);
            return;
        }
        else
        {
            
            Vector3 sum = Vector3.zero;

            foreach (BaseControlPoint point in ModelEditingPanel.Instance.GetControlPoints())
            {
                sum += point.GetCurrentPosition();
            }

            axisParent.position = sum / ModelEditingPanel.Instance.GetControlPoints().Count;


            if(ModelEditingPanel.Instance.GetTransformType() != TransformType.Select &&
                ModelEditingPanel.Instance.GetTransformType() != TransformType.Free)
            {
                axisParent.gameObject.SetActive(true);
            }
        }

    }
    /// <summary>
    /// Gizmo space updated - change the rotation of the gizmo based on world or local space and the selected elements
    /// </summary>
    public void GizmoSpaceUpdated()
    {
        //if(ModelEditingPanel.Instance.currentGizmoSpace == GizmoSpace.World)
        //{
        //    axisParent.rotation = Quaternion.identity;
        //}
        //else if(ModelEditingPanel.Instance.currentGizmoSpace == GizmoSpace.Local)
        //{
        //    if(ModelEditingPanel.Instance.GetControlPoints().Count > 0)
        //    {
        //        axisParent.rotation = ModelEditingPanel.Instance.GetControlPoints()[0].transform.rotation;
        //    }
        //}
    }
    /// <summary>
    /// Transform model updated - change to move, rotate or scale
    /// </summary>
    public void TransformModeUpdated()
    {
        if(ModelEditingPanel.Instance.GetTransformType() != TransformType.Scale || ModelEditingPanel.Instance.GetTransformType() != TransformType.Move || ModelEditingPanel.Instance.GetTransformType() != TransformType.Rotate)
        {
            axisParent.gameObject.SetActive(false);
        }
        else
        {
            axisParent.gameObject.SetActive(true);
        }

        xrGrabInters[0].trackPosition = true;
        xrGrabInters[1].trackPosition = true;
        xrGrabInters[2].trackPosition = true;
        xrGrabInters[3].trackPosition = true;

        xrGrabInters[0].trackRotation = false;
        xrGrabInters[1].trackRotation = false;
        xrGrabInters[2].trackRotation = false;
        xrGrabInters[3].trackRotation = false;


        xrGrabTrans[0].permittedDisplacementAxes = XRGeneralGrabTransformer.ManipulationAxes.All;
        xrGrabTrans[1].permittedDisplacementAxes = XRGeneralGrabTransformer.ManipulationAxes.X;
        xrGrabTrans[2].permittedDisplacementAxes = XRGeneralGrabTransformer.ManipulationAxes.Y;
        xrGrabTrans[3].permittedDisplacementAxes = XRGeneralGrabTransformer.ManipulationAxes.Z;

        axes[0].gameObject.SetActive(true);

        if (ModelEditingPanel.Instance.GetTransformType() == TransformType.Move)
        {
            for (int i = 0; i < axesCapsules.Length; i++)
            {
                axesCapsules[i].enabled = true;
            }
        }
        else if (ModelEditingPanel.Instance.GetTransformType() == TransformType.Scale)
        {
            for(int i = 0; i < axesCapsules.Length; i++)
            {
                axesCapsules[i].enabled = false;
            }
        }
        else if (ModelEditingPanel.Instance.GetTransformType() == TransformType.Rotate)
        {
            axes[0].gameObject.SetActive(false);
            xrGrabTrans[0].permittedDisplacementAxes = XRGeneralGrabTransformer.ManipulationAxes.All;

            xrGrabTrans[1].permittedDisplacementAxes = XRGeneralGrabTransformer.ManipulationAxes.X | XRGeneralGrabTransformer.ManipulationAxes.Y;
            xrGrabTrans[2].permittedDisplacementAxes = XRGeneralGrabTransformer.ManipulationAxes.Y | XRGeneralGrabTransformer.ManipulationAxes.Z;
            xrGrabTrans[3].permittedDisplacementAxes = XRGeneralGrabTransformer.ManipulationAxes.Z | XRGeneralGrabTransformer.ManipulationAxes.X;

            xrGrabInters[0].trackPosition = false;
            xrGrabInters[1].trackPosition = false;
            xrGrabInters[2].trackPosition = false;
            xrGrabInters[3].trackPosition = false;

            xrGrabInters[0].trackRotation = true;
            xrGrabInters[1].trackRotation = true;
            xrGrabInters[2].trackRotation = true;
            xrGrabInters[3].trackRotation = true;
        }

        UpdateGizmoGroupCenter();
    }
    /// <summary>
    /// Axis was selected by controller - setup the gizmo
    /// </summary>
    public void SelectAxis(int axis)
    {
        currentAxis = axis;
        previousPosition = axes[currentAxis].position;
        startPosition = previousPosition;

        if (ModelEditingPanel.Instance.GetTransformType() == TransformType.Scale)
        {
            //Add all axes state 


            initialGrabDistance = 1;

            initialControlPointPositions.Clear();
            foreach (BaseControlPoint point in ModelEditingPanel.Instance.GetControlPoints())
            {
                if (point != null)
                {
                    initialControlPointPositions.Add(point.GetCurrentPosition());
                }
            }
        }
        else if (ModelEditingPanel.Instance.GetTransformType() == TransformType.Rotate)
        {
            initialRotation = axes[currentAxis].localRotation;

            Vector3 rotationAxis = GetAxisVector(axes[currentAxis]);

            var interactor = axes[currentAxis].GetComponent<XRGrabInteractable>().firstInteractorSelecting;
            if (interactor == null) return;

            Plane rotationPlane = new Plane(rotationAxis, axisParent.position);

            Vector3 rayOrigin = interactor.transform.position;
            Vector3 rayDir = interactor.transform.forward;

            if (rotationPlane.Raycast(new Ray(rayOrigin, rayDir), out float hitDist))
            {
                Vector3 hitPoint = rayOrigin + rayDir * hitDist;

                Vector3 grabDir = (hitPoint - axisParent.position).normalized;

                initialDirection = Vector3.ProjectOnPlane(grabDir, rotationAxis).normalized;
            }
        }
    }
    /// <summary>
    /// Axis was deselected - reset the gizmo
    /// </summary>
    public void DeSelectAxis()
    {
        currentAxis = -1;
        ResetAxes();
        HideInfoPanel();
    }
    /// <summary>
    /// Reset the gizmo and its axes
    /// </summary>
    public void ResetAxes()
    {
        for (int i = 0; i < axes.Length; i++)
        {
            axes[i].localPosition = new Vector3();
        }

        for (int i = 0; i < axesLineRenders.Length; i++)
        {
            axesLineRenders[i].SetPosition(0, new Vector3());
            axesCapsules[i].transform.localPosition = new Vector3();
        }

        isReset = true;
        UpdateGizmoGroupCenter();
    }
    /// <summary>
    /// Update the gizmos positions and behavior based on the type
    /// </summary>
    void Update()
    {
        //None selected dont do anything
        if (currentAxis == -1)
        {
            if(isReset == false)
            {
                ResetAxes();
            }
            return;
        }

        isReset = false;

        //Move
        if (ModelEditingPanel.Instance.GetTransformType() == TransformType.Move)
        {
            Vector3 localPos = axes[currentAxis].position - axisParent.position;

            float snap = ModelEditingPanel.Instance.GetCurrentSnap();
            if (snap > 0)
            {
                localPos.x = Mathf.Round(localPos.x / snap) * snap;
                localPos.y = Mathf.Round(localPos.y / snap) * snap;
                localPos.z = Mathf.Round(localPos.z / snap) * snap;
            }

            Vector3 snappedPos = axisParent.position + localPos;

            Vector3 delta = snappedPos - previousPosition;

            ModelEditingPanel.Instance.MoveSelectedControlPointsByOffset(delta);

            for (int i = 0; i < axes.Length; i++)
            {
                if (i != currentAxis) axes[i].position = snappedPos;
            }

     


            previousPosition = snappedPos;

            UpdateInfoPanel();
        }
        //Scale
        else if (ModelEditingPanel.Instance.GetTransformType() == TransformType.Scale)
        {
            if (currentAxis != -1)
            {

                float currentDistance = Vector3.Distance(axesLineRenders[currentAxis-1].GetPosition(0), axesLineRenders[currentAxis-1].GetPosition(1));

                float scaleFactor = Mathf.Clamp(currentDistance, 0.1f, 1.5f);

                Vector3 finalScaleFactor = Vector3.one;
                if (currentAxis == 1) // X
                    finalScaleFactor.x = scaleFactor;
                else if (currentAxis == 2) // Y
                    finalScaleFactor.y = scaleFactor;
                else if (currentAxis == 3) // Z
                    finalScaleFactor.z = scaleFactor;
                else if (currentAxis == 0) // All
                    finalScaleFactor = new Vector3(scaleFactor, scaleFactor, scaleFactor);


                var controlPoints = ModelEditingPanel.Instance.GetControlPoints();
                for (int i = 0; i < controlPoints.Count; i++)
                {
                    if (i >= initialControlPointPositions.Count) break;

                    BaseControlPoint point = controlPoints[i];
                    if (point == null) continue;

                    Vector3 initialPos = initialControlPointPositions[i];

                    Vector3 initialRelativePos = initialPos - axisParent.position;
                    Vector3 newRelativePos = Vector3.Scale(initialRelativePos, finalScaleFactor);

                    controlPoints[i].MoveByOffset(controlPoints[i].GetCurrentPosition() - (axisParent.position + newRelativePos));

                }

                // 5. Update line renderers visuals
                for (int i = 1; i < axes.Length; i++) // Only update lines for X, Y, Z axes
                {
                    axesLineRenders[i-1].SetPosition(0, -axes[i].localPosition);
                }
            }

        }
        //Rotate
        else if (ModelEditingPanel.Instance.GetTransformType() == TransformType.Rotate)
        {
            Vector3 rotationAxis = GetAxisVector(axes[currentAxis]);

            var interactor = axes[currentAxis].GetComponent<XRGrabInteractable>().firstInteractorSelecting;
            if (interactor == null) return;

            Plane rotationPlane = new Plane(rotationAxis, axisParent.position);

            Vector3 rayOrigin = interactor.transform.position;
            Vector3 rayDir = interactor.transform.forward;

            if (rotationPlane.Raycast(new Ray(rayOrigin, rayDir), out float hitDist))
            {
                Vector3 hitPoint = rayOrigin + rayDir * hitDist;
                Vector3 grabDir = (hitPoint - axisParent.position).normalized;
                Vector3 projected = Vector3.ProjectOnPlane(grabDir, rotationAxis).normalized;

                float angle = Vector3.SignedAngle(initialDirection, projected, rotationAxis);

                if (ModelEditingPanel.Instance.GetGizmoSpace() == GizmoSpace.World)
                    axisParent.rotation = Quaternion.AngleAxis(angle, rotationAxis) * axisParent.rotation;
                else
                    axisParent.localRotation = initialRotation * Quaternion.AngleAxis(angle, rotationAxis);
            }
        }
    }
    /// <summary>
    /// Get the axis vector based on world or local space
    /// </summary>
    private Vector3 GetAxisVector(Transform axis)
    {
        if (ModelEditingPanel.Instance.GetGizmoSpace() == GizmoSpace.World)
        {
            if (axis == axes[1]) return Vector3.right;   
            if (axis == axes[2]) return Vector3.up;     
            if (axis == axes[3]) return Vector3.forward; 
        }
        else if (ModelEditingPanel.Instance.GetGizmoSpace() == GizmoSpace.Local)
        {
            if (axis == axes[1]) return axisParent.right;   
            if (axis == axes[2]) return axisParent.up;      
            if (axis == axes[3]) return axisParent.forward;
        }

        return Vector3.up; 
    }


}
