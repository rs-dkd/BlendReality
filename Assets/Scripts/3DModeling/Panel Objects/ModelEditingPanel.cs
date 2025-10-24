using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.ProBuilder;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Toggle = UnityEngine.UI.Toggle;

public enum EditMode
{
    Object,Pivot,Vertex,Edge,Face
}
public class ScaleSnapChangedEvent : UnityEvent<float> { }
public class RotateSnapChangedEvent : UnityEvent<float> { }
public class MoveSnapChangedEvent : UnityEvent<float> { }
public class ControlPointsChangedEvent : UnityEvent { }
public class GizmoSpaceChangedEvent : UnityEvent { }
public class TransformTypeChangedEvent : UnityEvent { }
public class EditModeChangedEvent : UnityEvent { }
public class ModelEditingPanel : MonoBehaviour
{
    public static ModelEditingPanel Instance;
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


    public ScaleSnapChangedEvent OnScaleSnapChanged = new ScaleSnapChangedEvent();
    public RotateSnapChangedEvent OnRotateSnapChanged = new RotateSnapChangedEvent();
    public MoveSnapChangedEvent OnMoveSnapChanged = new MoveSnapChangedEvent();
    public ControlPointsChangedEvent OnControlPointsChanged = new ControlPointsChangedEvent();
    public EditModeChangedEvent OnEditModeChanged = new EditModeChangedEvent();
    public TransformTypeChangedEvent OnTransformTypeChanged = new TransformTypeChangedEvent();
    public GizmoSpaceChangedEvent OnGizmoSpaceChanged = new GizmoSpaceChangedEvent();




    public ToggleGroupUI editModeToggleGroup;
    public ToggleGroupUI transformTypeToggleGroup;
    public ToggleGroupUI gizmoSpaceToggleGroup;

    public ToggleGroupUI moveSnapToggleGroup;
    public ToggleGroupUI rotateSnapToggleGroup;
    public ToggleGroupUI scaleSnapToggleGroup;



    public EditMode currentEditMode = EditMode.Object;
    public GizmoSpace currentGizmoSpace = GizmoSpace.World;
    public TransformType currentTransformType = TransformType.Free;


    public EditMode GetEditMode()
    {
        return currentEditMode;
    }

    public GameObject[] snappingOptions;
    public bool showSnap = false;
    public void SnapToggle()
    {
        showSnap = !showSnap;
        for (int i = 0; i < snappingOptions.Length; i++)
        {
            if (showSnap)
            {
                snappingOptions[i].gameObject.SetActive(true);
            }
            else
            {
                snappingOptions[i].gameObject.SetActive(false);

            }
        }

        if(showSnap == false)
        {
            UpdateMoveSnap(0);
            UpdateRotateSnap(0);
            UpdateScaleSnap(0);
        }
    }




    void Start()
    {
        editModeToggleGroup.OnToggleGroupChanged.AddListener(OnEditModeToggleGroupChanged);
        transformTypeToggleGroup.OnToggleGroupChanged.AddListener(OnTransformTypeToggleGroupChanged);
        gizmoSpaceToggleGroup.OnToggleGroupChanged.AddListener(OnGizmoSpaceToggleGroupChanged);


        moveSnapToggleGroup.Setup(moveSnapOptions);
        rotateSnapToggleGroup.Setup(rotateSnapOptions);
        scaleSnapToggleGroup.Setup(scaleSnapOptions);


        moveSnapToggleGroup.OnToggleGroupChanged.AddListener(OnMoveSnapToggleGroupChanged);
        rotateSnapToggleGroup.OnToggleGroupChanged.AddListener(OnRotateSnapToggleGroupChanged);
        scaleSnapToggleGroup.OnToggleGroupChanged.AddListener(OnScaleSnapToggleGroupChanged);

    }
    public String[] moveSnapOptions = new String[] { "None","1cm","10cm","1m" };
    public float[] moveSnapValues = new float[] { 0,0.01f,0.1f,1 };
    public String[] rotateSnapOptions = new String[] { "None", "5°", "15°", "45°" };
    public float[] rotateSnapValues = new float[] { 0, 5, 15, 45 };
    public String[] scaleSnapOptions = new String[] { "None","10%","50%","100%" };
    public float[] scaleSnapValues = new float[] { 0,0.1f,0.5f,1 };
    public void OnMoveSnapToggleGroupChanged(Toggle toggle)
    {
        for (int i = 0;i < moveSnapOptions.Length;i++)
        {
            if (moveSnapOptions[i] == toggle.name)
            {
                UpdateMoveSnap(moveSnapValues[i]);
            }
        }
    }
    public void OnRotateSnapToggleGroupChanged(Toggle toggle)
    {
        for (int i = 0; i < rotateSnapOptions.Length; i++)
        {
            if (rotateSnapOptions[i] == toggle.name)
            {
                UpdateMoveSnap(rotateSnapValues[i]);
            }
        }
    }
    public void OnScaleSnapToggleGroupChanged(Toggle toggle)
    {
        for (int i = 0; i < scaleSnapOptions.Length; i++)
        {
            if (scaleSnapOptions[i] == toggle.name)
            {
                UpdateMoveSnap(scaleSnapValues[i]);
            }
        }
    }



    public void OnGizmoSpaceToggleGroupChanged(Toggle toggle)
    {
        if (Enum.TryParse(toggle.name, out currentGizmoSpace))
        {
            OnGizmoSpaceChanged.Invoke();
        }
    }
    public void OnTransformTypeToggleGroupChanged(Toggle toggle)
    {
        if (Enum.TryParse(toggle.name, out currentTransformType))
        {
            OnTransformTypeChanged.Invoke();
        }
    }
    public void OnEditModeToggleGroupChanged(Toggle toggle)
    {
        if (Enum.TryParse(toggle.name, out currentEditMode))
        {
            OnEditModeChanged.Invoke();
            UpdateEditModel();
        }
    }



    public GameObject moveSnappingPanel;
    public GameObject rotateSnappingPanel;
    public GameObject scaleSnappingPanel;
    public void TransformTypeChanged()
    {
        moveSnappingPanel.SetActive(false);
        rotateSnappingPanel.SetActive(false);
        scaleSnappingPanel.SetActive(false);
        if (currentTransformType == TransformType.Move)
        {
            moveSnappingPanel.SetActive(true);

        }
        else if (currentTransformType == TransformType.Rotate)
        {
            rotateSnappingPanel.SetActive(true);
        }
        else if (currentTransformType == TransformType.Scale)
        {
            scaleSnappingPanel.SetActive(true);

        }
    }





    private float moveSnap = 0;
    private float rotateSnap = 0;
    private float scaleSnap = 0;
    public void UpdateMoveSnap(float newSnap)
    {
        moveSnap = newSnap;
        OnMoveSnapChanged.Invoke(moveSnap);
    }
    public void UpdateRotateSnap(float newSnap)
    {
        rotateSnap = newSnap;
        OnRotateSnapChanged.Invoke(rotateSnap);
    }
    public void UpdateScaleSnap(float newSnap)
    {
        scaleSnap = newSnap;
        OnScaleSnapChanged.Invoke(scaleSnap);
    }
    public float GetMoveSnap()
    {
        return moveSnap;
    }
    public float GetRotateSnap()
    {
        return rotateSnap;
    }
    public float GetScaleSnap()
    {
        return scaleSnap;
    }

    public float GetCurrentSnap()
    {
        if(currentTransformType == TransformType.Rotate) return rotateSnap;
        else if(currentTransformType == TransformType.Scale) return scaleSnap;
        else
        {
            return moveSnap;
        }
    }
























    public ModelData selectedModel;
    public float controlPointSize = 0.05f;
    public List<ControlPoint> allControlPoints = new List<ControlPoint>();
    public List<ControlPoint> inUseControlPoints = new List<ControlPoint>();





    public void UpdateEditModel()
    {


        // Deactivate old control points
        foreach (var cp in inUseControlPoints)
            cp.Deactivate();
        inUseControlPoints.Clear();


        selectedModel = SelectionManager.Instance.GetFirstSelected();
        if (selectedModel == null) return;

        int controlPointIndex = 0;





        if (currentEditMode == EditMode.Object)
        {

        }
        else if (currentEditMode == EditMode.Pivot)
        {

        }
        else if (currentEditMode == EditMode.Vertex)
        {
            List<Vector3> allPositions = selectedModel.GetVerts();
            Dictionary<Vector3, List<int>> uniqueVertGroups = new Dictionary<Vector3, List<int>>();
            for (int i = 0; i < allPositions.Count; i++)
            {
                Vector3 pos = allPositions[i];
                if (!uniqueVertGroups.ContainsKey(pos))
                    uniqueVertGroups[pos] = new List<int>();
                uniqueVertGroups[pos].Add(i);
            }

            foreach (var group in uniqueVertGroups)
            {



                CreateOrReuseControlPoint(group.Value.ToArray(), EditMode.Vertex, group.Key, ref controlPointIndex, Vector3.up);
            }
        }
        else if (currentEditMode == EditMode.Edge)
        {
            var faces = selectedModel.GetFaces();
            Dictionary<(Vector3, Vector3), List<int>> edgeVertexGroups = new Dictionary<(Vector3, Vector3), List<int>>();

            for (int f = 0; f < faces.Count; f++)
            {
                var face = faces[f];
                var faceVerts = face.distinctIndexes;

                for (int i = 0; i < faceVerts.Count; i++)
                {
                    int a = faceVerts[i];
                    int b = faceVerts[(i + 1) % faceVerts.Count];

                    Vector3 posA = selectedModel.GetVerts()[a];
                    Vector3 posB = selectedModel.GetVerts()[b];

                    (Vector3, Vector3) edgeKey = posA.sqrMagnitude < posB.sqrMagnitude ? (posA, posB) : (posB, posA);

                    if (!edgeVertexGroups.ContainsKey(edgeKey))
                    {
                        edgeVertexGroups[edgeKey] = new List<int>();
                    }

                    if (!edgeVertexGroups[edgeKey].Contains(a)) edgeVertexGroups[edgeKey].Add(a);
                    if (!edgeVertexGroups[edgeKey].Contains(b)) edgeVertexGroups[edgeKey].Add(b);
                }
            }

            foreach (var kvp in edgeVertexGroups)
            {
                Vector3 edgeCenter = (kvp.Key.Item1 + kvp.Key.Item2) / 2f;
                CreateOrReuseControlPoint(kvp.Value.ToArray(), EditMode.Edge, edgeCenter, ref controlPointIndex, Vector3.up);
            }
        }
        else if (currentEditMode == EditMode.Face)
        {
            List<Vector3> allPositions = selectedModel.GetVerts();
            var faces = selectedModel.GetFaces();
            foreach (var face in faces)
            {
                Vector3 faceCenter = Vector3.zero;
                foreach (var idx in face.distinctIndexes)
                    faceCenter += allPositions[idx];
                faceCenter /= face.distinctIndexes.Count;

                Vector3 faceNormal = UnityEngine.ProBuilder.Math.Normal(selectedModel.GetEditModel(), face);

                CreateOrReuseControlPoint(face.distinctIndexes.ToArray<int>(), EditMode.Face, faceCenter, ref controlPointIndex, faceNormal);
            }
        }
    }



    private void CreateControlPointForVertex(int[] vertexIndices, Vector3 position, ref int index, Vector3 _normal)
    {
        ControlPoint controlPointScript;
        controlPointScript = CreateOrReuseControlPoint(ref index);

        controlPointScript.transform.SetParent(selectedModel.trans);
        controlPointScript.transform.position = position;
        controlPointScript.gameObject.SetActive(true);

        controlPointScript.Initialize(vertexIndices, _normal, EditMode.Vertex);

        inUseControlPoints.Add(controlPointScript);

        index++;
    }
    private ControlPoint CreateOrReuseControlPoint(ref int index)
    {
        ControlPoint controlPointScript;
        GameObject controlPointGO;

        if (index < allControlPoints.Count)
        {
            controlPointScript = allControlPoints[index];
            controlPointGO = controlPointScript.gameObject;
        }
        else
        {
            controlPointGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            controlPointGO.name = $"ControlPoint_{index}";
            controlPointGO.transform.localScale = Vector3.one * controlPointSize;

            controlPointScript = controlPointGO.AddComponent<ControlPoint>();

            allControlPoints.Add(controlPointScript);
        }
        return controlPointScript;
    }


    private ControlPoint CreateOrReuseControlPoint(int[] vertexIndices, EditMode type, Vector3 position, ref int index, Vector3 _normal)
    {
        ControlPoint controlPointScript;
        GameObject controlPointGO;

        if (index < allControlPoints.Count)
        {
            controlPointScript = allControlPoints[index];
            controlPointGO = controlPointScript.gameObject;
        }
        else
        {
            controlPointGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            controlPointGO.name = $"ControlPoint_{index}";
            controlPointGO.transform.localScale = Vector3.one * controlPointSize;

            controlPointScript = controlPointGO.AddComponent<ControlPoint>();
            allControlPoints.Add(controlPointScript);
        }



        controlPointScript.transform.SetParent(selectedModel.trans);
        controlPointScript.transform.position = position;
        controlPointScript.gameObject.SetActive(true);

        controlPointScript.Initialize(vertexIndices, _normal, type);

        inUseControlPoints.Add(controlPointScript);

        index++;
        return controlPointScript;
    }
























    public void StopEditModel(ModelData model)
    {
        selectedModel = null;
        for (int i = 0; i < inUseControlPoints.Count; i++)
        {
            inUseControlPoints[i].transform.SetParent(this.transform);
            inUseControlPoints[i].Deactivate();
        }
    }

    public void AddOffsetToVertsPosition(int[] _verticesIndexes, Vector3 offset)
    {

        //Vector3[] newPositions = new Vector3[] { newPosition };
        UnityEngine.ProBuilder.VertexPositioning.TranslateVerticesInWorldSpace(selectedModel.editingModel, _verticesIndexes, offset);

        selectedModel.UpdateMeshEdit();
    }


    public void UpdateNonSelectedControlPointsPositions()
    {
        if (currentEditMode != EditMode.Edge && currentEditMode != EditMode.Face) return;

        List<Vector3> allPositions = selectedModel.GetVerts();
        foreach (ControlPoint cp in inUseControlPoints)
        {
            if (cp.vertices == null || cp.vertices.Length == 0 || cp.isSelected == true) continue;

            Vector3 newPosition = Vector3.zero;

            foreach (int vertexIndex in cp.vertices)
            {
                if (vertexIndex < allPositions.Count)
                {
                    newPosition += allPositions[vertexIndex];
                }
            }

            newPosition /= cp.vertices.Length;

            cp.transform.position = newPosition;
        }
    }



    public List<ControlPoint> controlPoints;
    public List<ControlPoint> GetControlPoints()
    {
        return controlPoints;
    }
    public void SelectControlPoint(ControlPoint cp)
    {
        controlPoints.Clear();
        cp.SetMaterialToSelected();
        controlPoints.Add(cp);
        OnControlPointsChanged.Invoke();
    }
    public void DeSelectControlPoint(ControlPoint cp)
    {
        if (controlPoints.Remove(cp))
        {
            cp.SetMaterialToDeselected();
            OnControlPointsChanged.Invoke();
        }

    }
    public void MultiSelectControlPoint(ControlPoint cp)
    {
        if (controlPoints.Contains(cp) == false)
        {
            cp.SetMaterialToSelected();
            controlPoints.Add(cp);
            OnControlPointsChanged.Invoke();
        }
    }

    public void AddOffsetToSelectedControlPoints(ControlPoint currentCP, Vector3 offset)
    {
        AddOffsetToVertsPosition(currentCP.vertices, offset);
        for (int i = 0; i < controlPoints.Count; i++)
        {
            if (controlPoints[i].isSelected && currentCP != controlPoints[i])
            {
                Debug.Log("Moving");
                AddOffsetToVertsPosition(controlPoints[i].vertices, offset);

                controlPoints[i].AddOffsetToControlPointPosition(offset);
            }
        }
    }




}
