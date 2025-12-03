using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.ProBuilder;
using Toggle = UnityEngine.UI.Toggle;

public enum EditMode
{
    Object, Vertex, Edge, Face
}
public class ScaleSnapChangedEvent : UnityEvent<float> { }
public class RotateSnapChangedEvent : UnityEvent<float> { }
public class MoveSnapChangedEvent : UnityEvent<float> { }
public class ControlPointsChangedEvent : UnityEvent { }
public class GizmoSpaceChangedEvent : UnityEvent { }
public class TransformTypeChangedEvent : UnityEvent { }
public class EditModeChangedEvent : UnityEvent { }


/// <summary>
/// Model Editor Panel
/// Controls - edit mode (object, vert, edge, face), transform gizmo type (free, select, move, rotate, scale), snapping
/// </summary>
public class ModelEditingPanel : MonoBehaviour
{
    //Singleton Pattern
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

    //Events
    public ScaleSnapChangedEvent OnScaleSnapChanged = new ScaleSnapChangedEvent();
    public RotateSnapChangedEvent OnRotateSnapChanged = new RotateSnapChangedEvent();
    public MoveSnapChangedEvent OnMoveSnapChanged = new MoveSnapChangedEvent();
    public ControlPointsChangedEvent OnControlPointsChanged = new ControlPointsChangedEvent();
    public EditModeChangedEvent OnEditModeChanged = new EditModeChangedEvent();
    public TransformTypeChangedEvent OnTransformTypeChanged = new TransformTypeChangedEvent();
    public GizmoSpaceChangedEvent OnGizmoSpaceChanged = new GizmoSpaceChangedEvent();

    //Edit Mode
    [Tooltip("Edit Mode Toggle Group")]
    [SerializeField] private ToggleGroupUI editModeToggleGroup;

    private EditMode currentEditMode = EditMode.Object;
    private String[] editModeOptions = new String[] { "Object", "Vertex", "Edge", "Face" };

    //Transform type
    [Tooltip("Transform Type Toggle Group")]
    [SerializeField] private ToggleGroupUI transformTypeToggleGroup;

    private TransformType currentTransformType = TransformType.Free;
    private String[] transformTypeOptions = new String[] { "Select", "Free", "Move" }; // "Rotate", "Scale"

    //Gizmo Space
    [Tooltip("Gizmo Space (local, world) Toggle Group")]
    [SerializeField] private ToggleGroupUI gizmoSpaceToggleGroup;

    private GizmoSpace currentGizmoSpace = GizmoSpace.World;
    private String[] gizmoSpaceOptions = new String[] { "World" };// "Local"

    //snapping
    [Tooltip("Move Snap UI Group")]
    [SerializeField] private GameObject moveSnappingPanel;
    [Tooltip("Rotate Snap UI Group")]
    [SerializeField] private GameObject rotateSnappingPanel;
    [Tooltip("Scale Snap UI Group")]
    [SerializeField] private GameObject scaleSnappingPanel;

    [Tooltip("Move Snap Toggle Group")]
    [SerializeField] private ToggleGroupUI moveSnapToggleGroup;
    [Tooltip("Rotate Snap Toggle Group")]
    [SerializeField] private ToggleGroupUI rotateSnapToggleGroup;
    [Tooltip("Scale Snap Toggle Group")]
    [SerializeField] private ToggleGroupUI scaleSnapToggleGroup;

    [Tooltip("All snapping options GOs")]
    [SerializeField] private GameObject[] snappingOptions;

    private bool showSnap = false;
    private float moveSnap = 0;
    private float rotateSnap = 0;
    private float scaleSnap = 0;

    private String[] moveSnapOptions = new String[] { "None", "1cm", "10cm", "1m" };
    private float[] moveSnapValues = new float[] { 0, 0.01f, 0.1f, 1 };
    private String[] rotateSnapOptions = new String[] { "None", "5 ", "15 ", "45 " };
    private float[] rotateSnapValues = new float[] { 0, 5, 15, 45 };
    private String[] scaleSnapOptions = new String[] { "None", "10%", "50%", "100%" };
    private float[] scaleSnapValues = new float[] { 0, 0.1f, 0.5f, 1 };


    //Control Point Size
    private float controlPointSize = 0.05f;



    //Different control points by type
    private List<VertexControlPoint> vertexControlPointPool = new List<VertexControlPoint>();
    private List<EdgeControlPoint> edgeControlPointPool = new List<EdgeControlPoint>();
    private List<FaceControlPoint> faceControlPointPool = new List<FaceControlPoint>();
    private List<ObjectControlPoint> objectControlPointPool = new List<ObjectControlPoint>();
    private List<BaseControlPoint> activeControlPoints = new List<BaseControlPoint>();
    private List<BaseControlPoint> selectedControlPoints = new List<BaseControlPoint>();


    /// <summary>
    /// Setup events and listeners
    /// </summary>
    void Start()
    {
        SelectionManager.Instance.OnSelectionChanged.AddListener(ModelSelectionChanged);


        editModeToggleGroup.OnToggleGroupChanged.AddListener(OnEditModeToggleGroupChanged);
        transformTypeToggleGroup.OnToggleGroupChanged.AddListener(OnTransformTypeToggleGroupChanged);
        gizmoSpaceToggleGroup.OnToggleGroupChanged.AddListener(OnGizmoSpaceToggleGroupChanged);


        editModeToggleGroup.Setup(editModeOptions);
        transformTypeToggleGroup.Setup(transformTypeOptions);
        gizmoSpaceToggleGroup.Setup(gizmoSpaceOptions);



        moveSnapToggleGroup.OnToggleGroupChanged.AddListener(OnMoveSnapToggleGroupChanged);
        rotateSnapToggleGroup.OnToggleGroupChanged.AddListener(OnRotateSnapToggleGroupChanged);
        scaleSnapToggleGroup.OnToggleGroupChanged.AddListener(OnScaleSnapToggleGroupChanged);


        moveSnapToggleGroup.Setup(moveSnapOptions);
        rotateSnapToggleGroup.Setup(rotateSnapOptions);
        scaleSnapToggleGroup.Setup(scaleSnapOptions);

    }

    /// <summary>
    /// Get Edit Mode
    /// </summary>
    public EditMode GetEditMode()
    {
        return currentEditMode;
    }
    /// <summary>
    /// Get Transform type
    /// </summary>
    public TransformType GetTransformType()
    {
        return currentTransformType;
    }
    /// <summary>
    /// Get Gizmo Space
    /// </summary>
    public GizmoSpace GetGizmoSpace()
    {
        return currentGizmoSpace;
    }
    /// <summary>
    /// Get current snap value
    /// </summary>
    public float GetCurrentSnap()
    {
        if (currentTransformType == TransformType.Rotate) return GetRotateSnap();
        else if (currentTransformType == TransformType.Scale) return GetScaleSnap();
        else return GetMoveSnap();
    }
    /// <summary>
    /// Get move snap value
    /// </summary>
    public float GetMoveSnap()
    {
        return moveSnap;
    }
    /// <summary>
    /// Get rotate snap value
    /// </summary>
    public float GetRotateSnap()
    {
        return rotateSnap;
    }
    /// <summary>
    /// Get scale snap value
    /// </summary>
    public float GetScaleSnap()
    {
        return scaleSnap;
    }

    /// <summary>
    /// Toggle Snap options
    /// </summary>
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

        if (showSnap == false)
        {
            UpdateMoveSnap(0);
            UpdateRotateSnap(0);
            UpdateScaleSnap(0);
        }
    }

    /// <summary>
    /// Selected models changed update control points
    /// </summary>
    public void ModelSelectionChanged(List<ModelData> models)
    {
        UpdateEditModel();
    }


    /// <summary>
    /// Move Snap Changed - Update
    /// </summary>
    public void OnMoveSnapToggleGroupChanged(Toggle toggle)
    {
        for (int i = 0; i < moveSnapOptions.Length; i++)
        {
            if (moveSnapOptions[i] == toggle.name)
            {
                UpdateMoveSnap(moveSnapValues[i]);
            }
        }
    }
    /// <summary>
    /// Rotate Snap Changed - Update
    /// </summary>
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
    /// <summary>
    /// Scale Snap Changed - Update
    /// </summary>
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
    /// <summary>
    /// Update the move snap value and invoke
    /// </summary>
    public void UpdateMoveSnap(float newSnap)
    {
        moveSnap = newSnap;
        OnMoveSnapChanged.Invoke(moveSnap);
    }
    /// <summary>
    /// Update the rotate snap value and invoke
    /// </summary>
    public void UpdateRotateSnap(float newSnap)
    {
        rotateSnap = newSnap;
        OnRotateSnapChanged.Invoke(rotateSnap);
    }
    /// <summary>
    /// Update the scale snap value and invoke
    /// </summary>
    public void UpdateScaleSnap(float newSnap)
    {
        scaleSnap = newSnap;
        OnScaleSnapChanged.Invoke(scaleSnap);
    }

    /// <summary>
    /// Gizmo Space changed - invoke
    /// </summary>
    public void OnGizmoSpaceToggleGroupChanged(Toggle toggle)
    {
        if (Enum.TryParse(toggle.name, out currentGizmoSpace))
        {
            OnGizmoSpaceChanged.Invoke();
        }
    }
    /// <summary>
    /// Transform type changed - invoke
    /// </summary>
    public void OnTransformTypeToggleGroupChanged(Toggle toggle)
    {
        if (Enum.TryParse(toggle.name, out currentTransformType))
        {
            OnTransformTypeChanged.Invoke();
        }
    }
    /// <summary>
    /// edit mode changed - invoke
    /// </summary>
    public void OnEditModeToggleGroupChanged(Toggle toggle)
    {
        if (Enum.TryParse(toggle.name, out currentEditMode))
        {
            UpdateEditModel();
            OnEditModeChanged.Invoke();
        }
    }
    /// <summary>
    /// Transform type changed - update UI
    /// </summary>
    public void TransformTypeChanged()
    {
        moveSnappingPanel.SetActive(false);
        rotateSnappingPanel.SetActive(false);
        scaleSnappingPanel.SetActive(false);
        if (currentTransformType == TransformType.Move)
        {
            moveSnappingPanel.SetActive(true);

        }
        //else if (currentTransformType == TransformType.Rotate)
        //{
        //    rotateSnappingPanel.SetActive(true);
        //}
        //else if (currentTransformType == TransformType.Scale)
        //{
        //    scaleSnappingPanel.SetActive(true);

        //}
    }
    /// <summary>
    /// When model changes or created call this to create the control points when its on the correct editmode
    /// </summary>
    public void UpdateEditModel()
    {
        //reset control points
        foreach (var cp in activeControlPoints)
            cp.Deactivate();
        activeControlPoints.Clear();
        selectedControlPoints.Clear();

        //Get first selected model
        ModelData firstSelectedModel = SelectionManager.Instance.GetFirstSelected();
        if (firstSelectedModel == null) return;

        //Update the mesh to make sure its good
        firstSelectedModel.UpdateMeshEdit();

        int controlPointIndex = 0;

        //Editmode is object
        if (currentEditMode == EditMode.Object)
        {
            List<ModelData> selectedModels = SelectionManager.Instance.GetSelectedModels();
            foreach (var model in selectedModels)
            {
                CreateOrReuseControlPoint(new List<int>(), ref controlPointIndex, EditMode.Object, model);
            }


        }
        //Editmode is vertex
        else if (currentEditMode == EditMode.Vertex)
        {
            List<Vector3> allPositions = firstSelectedModel.GetVerts();
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
                CreateOrReuseControlPoint(group.Value, ref controlPointIndex, EditMode.Vertex, firstSelectedModel);
            }
        }
        //Editmode is edge
        else if (currentEditMode == EditMode.Edge)
        {
            var faces = firstSelectedModel.GetFaces();
            Dictionary<(Vector3, Vector3), List<int>> edgeVertexGroups = new Dictionary<(Vector3, Vector3), List<int>>();

            for (int f = 0; f < faces.Count; f++)
            {
                var face = faces[f];
                var faceVerts = face.distinctIndexes;

                for (int i = 0; i < faceVerts.Count; i++)
                {
                    int a = faceVerts[i];
                    int b = faceVerts[(i + 1) % faceVerts.Count];

                    Vector3 posA = firstSelectedModel.GetVerts()[a];
                    Vector3 posB = firstSelectedModel.GetVerts()[b];

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
                CreateOrReuseControlPoint(kvp.Value, ref controlPointIndex, EditMode.Edge, firstSelectedModel);
            }
        }
        //Editmode is face
        else if (currentEditMode == EditMode.Face)
        {
            List<Vector3> allPositions = firstSelectedModel.GetVerts();
            var faces = firstSelectedModel.GetFaces();
            foreach (var face in faces)
            {
                CreateOrReuseControlPoint(face.distinctIndexes.ToList(), ref controlPointIndex, EditMode.Face, firstSelectedModel, face); ;
            }
        }
    }
    /// <summary>
    /// Create the control point for the element
    /// </summary>
    private BaseControlPoint CreateOrReuseControlPoint(List<int> vertexIndices, ref int index, EditMode mode, ModelData model, Face face = null)
    {
        BaseControlPoint controlPointScript = null;
        GameObject controlPointGO;

        if (mode == EditMode.Vertex && index < vertexControlPointPool.Count)
        {
            controlPointScript = vertexControlPointPool[index];
            controlPointGO = controlPointScript.gameObject;
        }
        else if (mode == EditMode.Edge && index < edgeControlPointPool.Count)
        {
            controlPointScript = edgeControlPointPool[index];
            controlPointGO = controlPointScript.gameObject;
        }
        else if (mode == EditMode.Face && index < faceControlPointPool.Count)
        {
            controlPointScript = faceControlPointPool[index];
            controlPointGO = controlPointScript.gameObject;
        }
        else if (mode == EditMode.Object && index < objectControlPointPool.Count)
        {
            controlPointScript = objectControlPointPool[index];
            controlPointGO = controlPointScript.gameObject;
        }
        else
        {
            controlPointGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            controlPointGO.name = $"ControlPoint_{index}";
            controlPointGO.transform.localScale = Vector3.one * controlPointSize;


            if (mode == EditMode.Vertex)
            {
                controlPointScript = controlPointGO.AddComponent<VertexControlPoint>();
                vertexControlPointPool.Add((VertexControlPoint)controlPointScript);
            }
            else if (mode == EditMode.Edge)
            {
                controlPointScript = controlPointGO.AddComponent<EdgeControlPoint>();
                edgeControlPointPool.Add((EdgeControlPoint)controlPointScript);
            }
            else if (mode == EditMode.Face)
            {
                controlPointScript = controlPointGO.AddComponent<FaceControlPoint>();
                faceControlPointPool.Add((FaceControlPoint)controlPointScript);
            }
            else if (mode == EditMode.Object)
            {
                controlPointScript = controlPointGO.AddComponent<ObjectControlPoint>();
                objectControlPointPool.Add((ObjectControlPoint)controlPointScript);
            }
        }



        if (mode == EditMode.Vertex)
        {
            ((VertexControlPoint)controlPointScript).Init(vertexIndices, model);
        }
        else if (mode == EditMode.Edge)
        {
            ((EdgeControlPoint)controlPointScript).Init(vertexIndices, model);
        }
        else if (mode == EditMode.Face)
        {
            ((FaceControlPoint)controlPointScript).Init(vertexIndices, model, face);
        }
        else if (mode == EditMode.Object)
        {
            ((ObjectControlPoint)controlPointScript).Init(model);
        }


        index++;
        activeControlPoints.Add(controlPointScript);
        return controlPointScript;
    }



    /// <summary>
    /// Get the selected control points
    /// </summary>
    public List<BaseControlPoint> GetControlPoints()
    {
        return selectedControlPoints;
    }
    /// <summary>
    /// Reset selection and add control point to it
    /// </summary>
    public void SelectControlPoint(BaseControlPoint cp)
    {
        selectedControlPoints.Clear();
        cp.Select();
        selectedControlPoints.Add(cp);
        OnControlPointsChanged.Invoke();
    }
    /// <summary>
    /// Remove control point to selecteion
    /// </summary>
    public void DeSelectControlPoint(BaseControlPoint cp)
    {
        if (selectedControlPoints.Remove(cp))
        {
            cp.Deselect();
            OnControlPointsChanged.Invoke();
        }

    }
    /// <summary>
    /// Add control point to selection
    /// </summary>
    public void MultiSelectControlPoint(BaseControlPoint cp)
    {
        if (selectedControlPoints.Contains(cp) == false)
        {
            cp.Select();
            selectedControlPoints.Add(cp);
            OnControlPointsChanged.Invoke();
        }
    }
    /// <summary>
    /// Stop editting the model remove all control points
    /// </summary>
    public void StopEditModel(ModelData model)
    {
        for (int i = 0; i < activeControlPoints.Count; i++)
        {
            activeControlPoints[i].Deactivate();
        }
    }




    /// <summary>
    /// Moves selected control points by offset and moves the correct verts on the model
    /// </summary>
    public void MoveSelectedControlPointsByOffset(Vector3 offset, BaseControlPoint grabbedAndMovedControlPoint = null)
    {
        if (currentEditMode == EditMode.Vertex)
        {
            for (int i = 0; i < selectedControlPoints.Count; i++)
            {
                SelectionManager.Instance.GetFirstSelected().AddOffsetToVerts(offset, ((VertexControlPoint)selectedControlPoints[i]).GetVerts().ToArray());
                if (grabbedAndMovedControlPoint != selectedControlPoints[i]) selectedControlPoints[i].UpdatePositionAndNormalToMatchElements();
                else Debug.Log("Skipping update for grabbed control point");
            }
        }
        else if (currentEditMode == EditMode.Edge)
        {
            HashSet<int> distinctVerts = new HashSet<int>();

            for (int i = 0; i < selectedControlPoints.Count; i++)
            {
                List<int> edgeVerts = ((EdgeControlPoint)selectedControlPoints[i]).GetEdgeVerts();
                distinctVerts.UnionWith(edgeVerts);
            }

            SelectionManager.Instance.GetFirstSelected().AddOffsetToVerts(offset, distinctVerts.ToArray());


            for (int i = 0; i < activeControlPoints.Count; i++)
            {
                if (grabbedAndMovedControlPoint != activeControlPoints[i]) activeControlPoints[i].UpdatePositionAndNormalToMatchElements();
                else Debug.Log("Skipping update for grabbed control point");
            }

        }
        else if (currentEditMode == EditMode.Face)
        {
            HashSet<int> distinctVerts = new HashSet<int>();

            for (int i = 0; i < selectedControlPoints.Count; i++)
            {
                List<int> faceVerts = ((FaceControlPoint)selectedControlPoints[i]).GetFaceVerts();
                distinctVerts.UnionWith(faceVerts);
            }

            SelectionManager.Instance.GetFirstSelected().AddOffsetToVerts(offset, distinctVerts.ToArray());

            for (int i = 0; i < activeControlPoints.Count; i++)
            {
                if (grabbedAndMovedControlPoint != activeControlPoints[i]) activeControlPoints[i].UpdatePositionAndNormalToMatchElements();
                else Debug.Log("Skipping update for grabbed control point");
            }
        }
    }



    [Header("PNS Integration")]
    [Tooltip("Samples per patch edge when applying PnS from Mesh Editor Panel.")]
    [SerializeField] private int pnsSamplesPerPatch = 8;

    /// <summary>
    /// Called by Mesh Editor Panel button: apply PnS smoothing to the currently selected model.
    /// </summary>
    public void OnApplyPnSButtonClicked()
    {
        if (SelectionManager.Instance == null)
        {
            Debug.LogWarning("PNS: No SelectionManager instance.");
            return;
        }

        ModelData current = SelectionManager.Instance.GetFirstSelected();
        if (current == null)
        {
            Debug.LogWarning("PNS: No model selected to apply PnS.");
            return;
        }

        if (PNSModelIntegration.Instance == null)
        {
            Debug.LogWarning("PNS: PNSModelIntegration.Instance is null.");
            return;
        }

        PNSModelIntegration.Instance.ApplyPnSSurfaceToModel(current, pnsSamplesPerPatch);
    }

}