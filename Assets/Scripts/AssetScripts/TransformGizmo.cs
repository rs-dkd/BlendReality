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
public class TransformTypeChangedEvent : UnityEvent { }

public class TransformGizmo : MonoBehaviour
{
    public static TransformGizmo Instance;
    public TransformTypeChangedEvent OnTransformTypeChanged = new TransformTypeChangedEvent();



    public TMP_Dropdown transformTypeDropdown;
    public TMP_Dropdown gizmoSpaceDropdown;



    public float moveSnap = 0;
    public float angleSnap = 0;
    public float scaleSnap = 0;

    public void UpdateMoveSnap(int newSnap)
    {
        moveSnap = newSnap;
    }
    public void UpdateRotateSnap(int newSnap)
    {
        angleSnap = newSnap;
    }

    public void UpdateScaleSnap(int newSnap)
    {
        scaleSnap = newSnap;
    }


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
    }

    



    public GizmoSpace gizmoSpace;
    public TransformType transformType;
    public Transform[] axes;

    public Vector3 previousPosition;
    public Transform currentAxis;

    public LineRenderer[] axesLineRenders;
    public CapsuleCollider[] axesCapsules;
    //public Vector3[] startingPositions;
    //public Quaternion[] startingRot;
    public XRGeneralGrabTransformer[] xrGrabTrans;
    public XRGrabInteractable[] xrGrabInters;


    private Quaternion initialRotation;
    private Vector3 initialDirection;


    public List<ControlPoint> controlPoints;


    public void SelectControlPoint(ControlPoint cp)
    {
        controlPoints.Clear();
        controlPoints.Add(cp);
        UpdateGizmoGroupCenter();
    }
    public void DeSelectControlPoint(ControlPoint cp)
    {
        if (controlPoints.Remove(cp))
        {
            UpdateGizmoGroupCenter();
        }

    }

    public void MultiSelectControlPoint(ControlPoint cp)
    {
        if(controlPoints.Contains(cp) == false)
        {
            controlPoints.Add(cp);
            UpdateGizmoGroupCenter();
        }
    }
    public void UpdateGizmoGroupCenter()
    {
        if(controlPoints.Count == 0)
        {
            gameObject.SetActive(false);
            return;
        }
        else
        {
            Vector3 sum = Vector3.zero;

            foreach (ControlPoint point in controlPoints)
            {
                sum += point.GetCurrentPosition();
            }

            this.transform.position = sum / controlPoints.Count;

        }

    }










    public void GizmoUpdated()
    {
        
    }


    public void SelectGizmoSpace()
    {
        gizmoSpace = (GizmoSpace)gizmoSpaceDropdown.value;
        GizmoUpdated();
    }
    public void SelectTransformMode()
    {
        transformType = (TransformType)transformTypeDropdown.value;
        OnTransformTypeChanged.Invoke();




        xrGrabInters[0].trackPosition = true;
        xrGrabInters[1].trackPosition = true;
        xrGrabInters[2].trackPosition = true;
        xrGrabInters[0].trackRotation = false;
        xrGrabInters[1].trackRotation = false;
        xrGrabInters[2].trackRotation = false;


        xrGrabTrans[0].permittedDisplacementAxes = XRGeneralGrabTransformer.ManipulationAxes.X;
        xrGrabTrans[1].permittedDisplacementAxes = XRGeneralGrabTransformer.ManipulationAxes.Y;
        xrGrabTrans[2].permittedDisplacementAxes = XRGeneralGrabTransformer.ManipulationAxes.Z;

        axes[0].gameObject.SetActive(true);

        if (transformType == TransformType.Move)
        {
            for (int i = 0; i < axesCapsules.Length; i++)
            {
                axesCapsules[i].enabled = true;
            }
        }
        else if (transformType == TransformType.Scale)
        {
            for(int i = 0; i < axesCapsules.Length; i++)
            {
                axesCapsules[i].enabled = false;
            }
        }
        else if (transformType == TransformType.Rotate)
        {
            axes[0].gameObject.SetActive(false);
            xrGrabTrans[0].permittedDisplacementAxes = XRGeneralGrabTransformer.ManipulationAxes.X | XRGeneralGrabTransformer.ManipulationAxes.Y;
            xrGrabTrans[1].permittedDisplacementAxes = XRGeneralGrabTransformer.ManipulationAxes.Y | XRGeneralGrabTransformer.ManipulationAxes.Z;
            xrGrabTrans[2].permittedDisplacementAxes = XRGeneralGrabTransformer.ManipulationAxes.Z | XRGeneralGrabTransformer.ManipulationAxes.X;

            xrGrabInters[0].trackPosition = false;
            xrGrabInters[1].trackPosition = false;
            xrGrabInters[2].trackPosition = false;

            xrGrabInters[0].trackRotation = true;
            xrGrabInters[1].trackRotation = true;
            xrGrabInters[2].trackRotation = true;
        }

        GizmoUpdated();
    }









    public void SelectAxis(Transform axis)
    {
        currentAxis = axis;
        previousPosition = currentAxis.position;


        if (transformType == TransformType.Rotate)
        {
            initialRotation = currentAxis.localRotation;

            Vector3 rotationAxis = GetAxisVector(axis);

            var interactor = axis.GetComponent<XRGrabInteractable>().firstInteractorSelecting;
            if (interactor == null) return;

            Plane rotationPlane = new Plane(rotationAxis, transform.position);

            Vector3 rayOrigin = interactor.transform.position;
            Vector3 rayDir = interactor.transform.forward;

            if (rotationPlane.Raycast(new Ray(rayOrigin, rayDir), out float hitDist))
            {
                Vector3 hitPoint = rayOrigin + rayDir * hitDist;

                Vector3 grabDir = (hitPoint - transform.position).normalized;

                initialDirection = Vector3.ProjectOnPlane(grabDir, rotationAxis).normalized;
            }
        }
    }

    public void DeSelectAxis()
    {
        currentAxis = null;
        ResetAxes();

    }

    public bool isReset = false;
    public void ResetAxes()
    {
        for (int i = 0; i < axesLineRenders.Length; i++)
        {
            axesLineRenders[i].SetPosition(0, new Vector3());
            axesCapsules[i].transform.localPosition = new Vector3();
        }
        isReset = true;
    }


    void Update()
    {
        if (currentAxis == null)
        {
            if(isReset == false)
            {
                ResetAxes();
            }
            return;
        }

        isReset = false;




        if (transformType == TransformType.Move)
        {
            Vector3 localPos = currentAxis.position - transform.position;

            if (moveSnap > 0)
            {
                localPos.x = Mathf.Round(localPos.x / moveSnap) * moveSnap;
                localPos.y = Mathf.Round(localPos.y / moveSnap) * moveSnap;
                localPos.z = Mathf.Round(localPos.z / moveSnap) * moveSnap;
            }

            Vector3 snappedPos = transform.position + localPos;

            Vector3 delta = snappedPos - previousPosition;

            foreach (var cp in controlPoints)
            {
                cp.AddOffsetToControlPointPosition(delta);
            }

            foreach (var axis in axes)
            {
                if (axis != currentAxis) axis.position = snappedPos;
            }

            previousPosition = snappedPos;
        }
        else if (transformType == TransformType.Scale)
        {
            if (currentAxis != null)
            {
                if (currentAxis == axes[0])//All axis
                {

                }
                else
                {
                    for (int i = 0; i < axesLineRenders.Length; i++)
                    {
                        axesLineRenders[i].SetPosition(0, -(axesCapsules[i].transform.localPosition));
                    }
                }
            }
          
        }
        else if (transformType == TransformType.Rotate)
        {
            Vector3 rotationAxis = GetAxisVector(currentAxis);

            var interactor = currentAxis.GetComponent<XRGrabInteractable>().firstInteractorSelecting;
            if (interactor == null) return;

            Plane rotationPlane = new Plane(rotationAxis, transform.position);

            Vector3 rayOrigin = interactor.transform.position;
            Vector3 rayDir = interactor.transform.forward;

            if (rotationPlane.Raycast(new Ray(rayOrigin, rayDir), out float hitDist))
            {
                Vector3 hitPoint = rayOrigin + rayDir * hitDist;
                Vector3 grabDir = (hitPoint - transform.position).normalized;
                Vector3 projected = Vector3.ProjectOnPlane(grabDir, rotationAxis).normalized;

                float angle = Vector3.SignedAngle(initialDirection, projected, rotationAxis);

                if (gizmoSpace == GizmoSpace.World)
                    transform.rotation = Quaternion.AngleAxis(angle, rotationAxis) * transform.rotation;
                else
                    transform.localRotation = initialRotation * Quaternion.AngleAxis(angle, rotationAxis);
            }
        }
    }



    private Vector3 GetAxisVector(Transform axis)
    {
        if (gizmoSpace == GizmoSpace.World)
        {
            if (axis == axes[0]) return Vector3.right;   
            if (axis == axes[1]) return Vector3.up;     
            if (axis == axes[2]) return Vector3.forward; 
        }
        else if (gizmoSpace == GizmoSpace.Local)
        {
            if (axis == axes[0]) return transform.right;   
            if (axis == axes[1]) return transform.up;      
            if (axis == axes[2]) return transform.forward;
        }

        return Vector3.up; 
    }















}
