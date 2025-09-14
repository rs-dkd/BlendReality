using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.UI;
using UnityEngine.UIElements;

public enum EditMode
{
    Object,Pivot,Vertex,Edge,Face
}
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

    public TMP_Dropdown editModeDropdown;

    public void ChangeEditMode()
    {
        currentEditMode = (EditMode)editModeDropdown.value;
        StartEditModel();
    }

    public ModelData selectedModel;

    [Header("Visual Settings")]
    public Material surfaceMaterial;
    public Material controlPointMaterial;
    public Material controlPointGrabbedMaterial;
    public float controlPointSize = 0.05f;



    public List<ControlPoint> allControlPoints = new List<ControlPoint>();
    public List<ControlPoint> inUseControlPoints = new List<ControlPoint>();
    public EditMode currentEditMode = EditMode.Object;
    void Start()
    {
        if (surfaceMaterial == null)
        {
            surfaceMaterial = CreateDefaultSurfaceMaterial();
        }

        if (controlPointMaterial == null)
        {
            controlPointMaterial = CreateDefaultControlPointMaterial();
        }

        if (controlPointGrabbedMaterial == null)
        {
            controlPointGrabbedMaterial = CreateDefaultGrabbedMaterial();
        }
    }






    private Material CreateDefaultSurfaceMaterial()
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(0.2f, 0.7f, 1f, 0.7f);
        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
        return mat;
    }

    private Material CreateDefaultControlPointMaterial()
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Color.red;
        mat.SetFloat("_Metallic", 0.3f);
        mat.SetFloat("_Smoothness", 0.7f);
        return mat;
    }

    private Material CreateDefaultGrabbedMaterial()
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Color.yellow;
        mat.SetFloat("_Metallic", 0.8f);
        mat.SetFloat("_Smoothness", 0.9f);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", Color.yellow * 0.3f);
        return mat;
    }

    public void StartEditModel()
    {
        selectedModel = SelectionManager.Instance.GetFirstSelected();
        if (selectedModel == null) return;

        // Deactivate old control points
        foreach (var cp in inUseControlPoints)
            cp.Deactivate();
        inUseControlPoints.Clear();

        int controlPointIndex = 0;

        //if (currentEditMode == EditMode.Object)
        //{
        //    int[] verts;
        //    CreateOrReuseControlPoint(verts, EditMode.Object, selectedModel.trans.position, ref controlPointIndex, Vector3.up);
        //}
        //else if (currentEditMode == EditMode.Pivot)
        //{
        //    int[] verts;
        //    CreateOrReuseControlPoint(verts, EditMode.Pivot, selectedModel.trans.position, ref controlPointIndex, Vector3.up);
        //}
        if (currentEditMode == EditMode.Vertex)
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


    private void CreateControlPointForEdge(int[] vertexIndices, EditMode type, Vector3 position, ref int index, Vector3 _normal)
    {

    }
    private void CreateControlPointForFace(Face face, EditMode type, Vector3 position, ref int index, Vector3 _normal)
    {

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

            MeshRenderer renderer = controlPointGO.GetComponent<MeshRenderer>();
            if (renderer != null) renderer.material = controlPointMaterial;

            controlPointScript = controlPointGO.AddComponent<ControlPoint>();
            controlPointScript.normalMaterial = controlPointMaterial;
            controlPointScript.grabbedMaterial = controlPointGrabbedMaterial;
            //controlPointScript.SetNormalMaterial();

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

            MeshRenderer renderer = controlPointGO.GetComponent<MeshRenderer>();
            if (renderer != null) renderer.material = controlPointMaterial;

            controlPointScript = controlPointGO.AddComponent<ControlPoint>();
            controlPointScript.normalMaterial = controlPointMaterial;
            controlPointScript.grabbedMaterial = controlPointGrabbedMaterial;
            //controlPointScript.SetNormalMaterial();

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










}
