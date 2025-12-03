using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.UI;
using System.Linq;

public enum ModelType
{
    Cube,
    Sphere,
    Cone,
    Pipe,
    Cylinder,
    PlaneFlat,
    PlaneWavy,
    PlaneDome
}

/// <summary>
/// Handles creating objects
/// Also manages the panels creation settings
/// </summary>
public class ModelCreator : MonoBehaviour
{
    // Singleton pattern
    public static ModelCreator Instance;
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

    [Tooltip("Uniform Size Toggle")]
    [SerializeField] private Toggle uniformSizeToggle;
    [Tooltip("Uniform Size Slider")]
    [SerializeField] private SliderUI sizeSlider;
    [Tooltip("Size in X Slider")]
    [SerializeField] private SliderUI xSizeSlider;
    [Tooltip("Size in Y Slider")]
    [SerializeField] private SliderUI ySizeSlider;
    [Tooltip("Size in Z Slider")]
    [SerializeField] private SliderUI zSizeSlider;
    [Tooltip("Thickness (For Pipe) Slider")]
    [SerializeField] private SliderUI thicknessSizeSlider;

    [Tooltip("Sub D Slider (For sphere)")]
    [SerializeField] private SliderUI subDivisionSlider;
    [Tooltip("Sub D in X Slider")]
    [SerializeField] private SliderUI xSubDivisionSlider;
    [Tooltip("Sub D in Y Slider")]
    [SerializeField] private SliderUI ySubDivisionSlider;
    [Tooltip("Sub D in Z Slider")]
    [SerializeField] private SliderUI zSubDivisionSlider;

    [Tooltip("Mesh Type Toggle Group")]
    [SerializeField] private ToggleGroupUI modelTypeToggleGroupUI;

    [Tooltip("Mesh Creation point - (has a lazy follow)")]
    [SerializeField] private Transform meshCreationPoint;

    private ModelData currentModel;
    private ModelType currentShapeType;
    private bool hasUniformSize;

    /// <summary>
    /// Setup listeners and events, and the UI
    /// </summary>
    void Start()
    {
        modelTypeToggleGroupUI.OnToggleGroupChanged.AddListener(ModelTypeSelected);

        sizeSlider.OnSliderValueChangedEvent.AddListener(SizeSliderUpdated);
        xSizeSlider.OnSliderValueChangedEvent.AddListener(SizeSliderUpdated);
        ySizeSlider.OnSliderValueChangedEvent.AddListener(SizeSliderUpdated);
        zSizeSlider.OnSliderValueChangedEvent.AddListener(SizeSliderUpdated);
        thicknessSizeSlider.OnSliderValueChangedEvent.AddListener(SizeSliderUpdated);

        subDivisionSlider.OnSliderValueChangedEvent.AddListener(SubDSliderUpdated);
        xSubDivisionSlider.OnSliderValueChangedEvent.AddListener(SubDSliderUpdated);
        ySubDivisionSlider.OnSliderValueChangedEvent.AddListener(SubDSliderUpdated);
        zSubDivisionSlider.OnSliderValueChangedEvent.AddListener(SubDSliderUpdated);

        PanelManager.Instance.OnPanelChanged.AddListener(PanelChanged);

        UniformSizeToggleChanged();
    }

    /// <summary>
    /// If changed to the edit panel - finalize the model
    /// </summary>
    public void PanelChanged(UIPanel panel)
    {
        if (panel.name == "EditPanel")
        {
            FinalizeLastModel();
        }
    }

    /// <summary>
    /// Finalize model and set current model to null - allows user to create a new model instead of overwriting the last one
    /// </summary>
    public void FinalizeLastModel()
    {
        if (currentModel != null)
        {
            currentModel.FinalizeEditModel();
            currentModel = null;
        }
    }

    /// <summary>
    /// Update the UI based on the Model type selected and create a new model
    /// </summary>
    public void ModelTypeSelected(Toggle toggle)
    {
        ModelType type = currentShapeType;
        if (Enum.TryParse(toggle.name, out currentShapeType))
        {
            if (type != currentShapeType || toggle.isOn)
            {
                ResetSliders();
                UniformChangedUpdateSliderValues();
                UpdateSizeSlidersUI();
                UpdateSubDSlidersMinMax();
                UpdateSubDSliders();
                UpdateSizeSlidersMinMax();

                CreateOrUpdateModel();
            }
        }
    }

    /// <summary>
    /// Reset all the sliders on model type changed
    /// </summary>
    public void ResetSliders()
    {
        sizeSlider.SetValue(1);
        xSizeSlider.SetValue(1);
        ySizeSlider.SetValue(1);
        zSizeSlider.SetValue(1);
        thicknessSizeSlider.SetValue(0.5f);
        subDivisionSlider.SetValue(1);
        xSubDivisionSlider.SetValue(1);
        ySubDivisionSlider.SetValue(1);
        zSubDivisionSlider.SetValue(1);
    }

    /// <summary>
    /// The uniform size toggle updated - update the sliders and UI
    /// </summary>
    public void UniformSizeToggleChanged()
    {
        UniformChangedUpdateSliderValues();
        UpdateSizeSlidersUI();
        UpdateSubDSlidersMinMax();
        UpdateSizeSlidersMinMax();
        UpdateSubDSliders();
    }

    /// <summary>
    /// The uniform size toggle updated - update the sliders and UI
    /// </summary>
    private void UniformChangedUpdateSliderValues()
    {
        if (hasUniformSize != uniformSizeToggle.isOn)
        {
            hasUniformSize = uniformSizeToggle.isOn;

            if (hasUniformSize == true)
            {
                sizeSlider.SetValue(xSizeSlider.GetValueAsFloat());
            }
            else
            {
                xSizeSlider.SetValue(sizeSlider.GetValueAsFloat());
                ySizeSlider.SetValue(sizeSlider.GetValueAsFloat());
                zSizeSlider.SetValue(sizeSlider.GetValueAsFloat());
                thicknessSizeSlider.SetValue(sizeSlider.GetValueAsFloat() / 2);
            }
        }
    }

    /// <summary>
    /// Update the size sliders
    /// </summary>
    private void UpdateSizeSlidersUI()
    {
        if (currentShapeType == ModelType.PlaneFlat)
        {
            if (hasUniformSize)
            {
                sizeSlider.Show();
                xSizeSlider.Hide();
                ySizeSlider.Hide();
                zSizeSlider.Hide();
                thicknessSizeSlider.Hide();
            }
            else
            {
                sizeSlider.Hide();
                xSizeSlider.Show();
                ySizeSlider.Hide();
                zSizeSlider.Show();
                thicknessSizeSlider.Hide();
            }
        }
        else
        {
            if (currentShapeType == ModelType.Pipe)
            {
                if (hasUniformSize)
                {
                    sizeSlider.Show();
                    xSizeSlider.Hide();
                    ySizeSlider.Hide();
                    zSizeSlider.Hide();
                    thicknessSizeSlider.Hide();
                }
                else
                {
                    sizeSlider.Hide();
                    xSizeSlider.Show();
                    ySizeSlider.Show();
                    zSizeSlider.Hide();
                    thicknessSizeSlider.Show();
                }
            }
            else
            {
                if (hasUniformSize)
                {
                    sizeSlider.Show();
                    xSizeSlider.Hide();
                    ySizeSlider.Hide();
                    zSizeSlider.Hide();
                    thicknessSizeSlider.Hide();
                }
                else
                {
                    sizeSlider.Hide();
                    xSizeSlider.Show();
                    ySizeSlider.Show();
                    zSizeSlider.Show();
                    thicknessSizeSlider.Hide();
                }
            }
        }
    }

    /// <summary>
    /// Update the size sliders min max
    /// </summary>
    private void UpdateSizeSlidersMinMax()
    {
        sizeSlider.SetMinMax(0.1f, 2);
        xSizeSlider.SetMinMax(0.1f, 2);
        ySizeSlider.SetMinMax(0.1f, 2);
        zSizeSlider.SetMinMax(0.1f, 2);
        thicknessSizeSlider.SetMinMax(0.1f, 2);
    }

    /// <summary>
    /// Update the sub D sliders
    /// </summary>
    private void UpdateSubDSlidersMinMax()
    {
        if (currentShapeType == ModelType.Cube)
        {
            xSubDivisionSlider.SetMinMax(1, 10);
            ySubDivisionSlider.SetMinMax(1, 10);
            zSubDivisionSlider.SetMinMax(1, 10);
        }
        else if (currentShapeType == ModelType.Sphere)
        {
            subDivisionSlider.SetMinMax(0, 3);
        }
        else if (currentShapeType == ModelType.Cone || currentShapeType == ModelType.Pipe || currentShapeType == ModelType.Cylinder)
        {
            subDivisionSlider.SetMinMax(3, 10);
            ySubDivisionSlider.SetMinMax(0, 10);
        }
        else if (currentShapeType == ModelType.PlaneFlat || currentShapeType == ModelType.PlaneDome || currentShapeType == ModelType.PlaneWavy)
        {
            xSubDivisionSlider.SetMinMax(0, 10);
            zSubDivisionSlider.SetMinMax(0, 10);
        }
    }

    private void UpdateSubDSliders()
    {
        if (currentShapeType == ModelType.Cube)
        {
            subDivisionSlider.Hide();
            xSubDivisionSlider.Show();
            ySubDivisionSlider.Show();
            zSubDivisionSlider.Show();
        }
        else if (currentShapeType == ModelType.Sphere)
        {
            subDivisionSlider.Show();
            xSubDivisionSlider.Hide();
            ySubDivisionSlider.Hide();
            zSubDivisionSlider.Hide();
        }
        else if (currentShapeType == ModelType.Pipe)
        {
            subDivisionSlider.Show();
            xSubDivisionSlider.Hide();
            ySubDivisionSlider.Show();
            zSubDivisionSlider.Hide();
        }
        else if (currentShapeType == ModelType.Cylinder)
        {
            subDivisionSlider.Show();
            xSubDivisionSlider.Hide();
            ySubDivisionSlider.Show();
            zSubDivisionSlider.Hide();
        }
        else if (currentShapeType == ModelType.Cone)
        {
            subDivisionSlider.Show();
            xSubDivisionSlider.Hide();
            ySubDivisionSlider.Hide();
            zSubDivisionSlider.Hide();
        }
        else if (currentShapeType == ModelType.PlaneFlat || currentShapeType == ModelType.PlaneDome || currentShapeType == ModelType.PlaneWavy)
        {
            subDivisionSlider.Hide();
            xSubDivisionSlider.Show();
            ySubDivisionSlider.Hide();
            zSubDivisionSlider.Show();
        }
    }

    /// <summary>
    /// Create or update a model called when the model type is changed
    /// </summary>
    private void CreateOrUpdateModel()
    {
        if (currentModel != null)
        {
            currentModel.DeleteModel();
            currentModel = null;
        }

        ProBuilderMesh mesh = CreateModelByType(currentShapeType);

        GameObject myObject = new GameObject();
        currentModel = myObject.AddComponent<ModelData>();
        currentModel.SetupModel(mesh, meshCreationPoint.position);

        SizeSliderUpdated(1);
        SelectionManager.Instance.SelectModel(currentModel);
    }

    /// <summary>
    /// Create probuilder mesh by type - NOW WITH WELDED VERTICES
    /// </summary>
    private ProBuilderMesh CreateModelByType(ModelType type)
    {
        ProBuilderMesh mesh = null;

        if (type == ModelType.Cube)
        {
            mesh = CreateSubdividedCube(
                Mathf.RoundToInt(xSubDivisionSlider.GetValueAsFloat()),
                Mathf.RoundToInt(ySubDivisionSlider.GetValueAsFloat()),
                Mathf.RoundToInt(zSubDivisionSlider.GetValueAsFloat())
            );
        }
        else if (type == ModelType.Sphere)
        {
            mesh = GenerateWeldedSphere(
                0.5f,
                Mathf.RoundToInt(subDivisionSlider.GetValueAsFloat())
            );
        }
        else if (type == ModelType.PlaneFlat)
        {
            mesh = GenerateWeldedPlane(
                1f, 1f,
                Mathf.RoundToInt(xSubDivisionSlider.GetValueAsFloat()),
                Mathf.RoundToInt(zSubDivisionSlider.GetValueAsFloat())
            );
        }
        else if (type == ModelType.Cone)
        {
            mesh = GenerateWeldedCone(
                1f, 1f,
                Mathf.RoundToInt(subDivisionSlider.GetValueAsFloat())
            );
        }
        else if (type == ModelType.Pipe)
        {
            if (hasUniformSize)
            {
                mesh = GenerateWeldedPipe(
                    sizeSlider.GetValueAsFloat(),
                    sizeSlider.GetValueAsFloat(),
                    sizeSlider.GetValueAsFloat() / 2,
                    Mathf.RoundToInt(subDivisionSlider.GetValueAsFloat()),
                    Mathf.RoundToInt(ySubDivisionSlider.GetValueAsFloat())
                );
            }
            else
            {
                float thickness = thicknessSizeSlider.GetValueAsFloat();
                if (thickness > (xSizeSlider.GetValueAsFloat()) - 0.01f)
                {
                    thickness = (xSizeSlider.GetValueAsFloat()) - 0.01f;
                }
                mesh = GenerateWeldedPipe(
                    xSizeSlider.GetValueAsFloat(),
                    ySizeSlider.GetValueAsFloat(),
                    thickness,
                    Mathf.RoundToInt(subDivisionSlider.GetValueAsFloat()),
                    Mathf.RoundToInt(ySubDivisionSlider.GetValueAsFloat())
                );
            }
        }
        else if (type == ModelType.Cylinder)
        {
            mesh = GenerateWeldedCylinder(
                1f, 1f,
                Mathf.RoundToInt(subDivisionSlider.GetValueAsFloat()),
                Mathf.RoundToInt(ySubDivisionSlider.GetValueAsFloat())
            );
        }
        else if (type == ModelType.PlaneDome)
        {
            mesh = GenerateWeldedDomePlane(
                1f, 1f,
                Mathf.RoundToInt(xSubDivisionSlider.GetValueAsFloat()),
                Mathf.RoundToInt(zSubDivisionSlider.GetValueAsFloat()),
                1f
            );
        }
        else if (type == ModelType.PlaneWavy)
        {
            mesh = GenerateWeldedWavyPlane(
                1f, 1f,
                Mathf.RoundToInt(xSubDivisionSlider.GetValueAsFloat()),
                Mathf.RoundToInt(zSubDivisionSlider.GetValueAsFloat()),
                1f
            );
        }

        return mesh;
    }

    /// <summary>
    /// Create a cube with properly welded vertices
    /// </summary>
    private static ProBuilderMesh CreateSubdividedCube(int xSub, int ySub, int zSub)
    {
        xSub = Mathf.Max(1, xSub);
        ySub = Mathf.Max(1, ySub);
        zSub = Mathf.Max(1, zSub);

        GameObject go = new GameObject("ProceduralCube");
        ProBuilderMesh mesh = go.AddComponent<ProBuilderMesh>();

        List<Vertex> vertices = new List<Vertex>();
        List<Face> faces = new List<Face>();

        float xStep = 1f / xSub;
        float yStep = 1f / ySub;
        float zStep = 1f / zSub;

        Dictionary<string, int> vertDict = new Dictionary<string, int>();

        int GetVertex(float x, float y, float z)
        {
            string key = $"{x}_{y}_{z}";
            if (vertDict.TryGetValue(key, out int index))
                return index;

            Vertex v = new Vertex { position = new Vector3(x - 0.5f, y - 0.5f, z - 0.5f), uv0 = new Vector2(x, y) };
            index = vertices.Count;
            vertices.Add(v);
            vertDict[key] = index;
            return index;
        }

        void AddQuad(int v0, int v1, int v2, int v3, int smoothingGroup)
        {
            faces.Add(new Face(new int[] { v0, v1, v2 }) { smoothingGroup = smoothingGroup });
            faces.Add(new Face(new int[] { v0, v2, v3 }) { smoothingGroup = smoothingGroup });
        }

        // Front (+Z)
        for (int y = 0; y <= ySub; y++) { for (int x = 0; x <= xSub; x++) { GetVertex(x * xStep, y * yStep, 1f); } }
        for (int y = 0; y < ySub; y++)
        {
            for (int x = 0; x < xSub; x++)
            {
                int v0 = vertDict[$"{x * xStep}_{y * yStep}_1"];
                int v1 = vertDict[$"{(x + 1) * xStep}_{y * yStep}_1"];
                int v2 = vertDict[$"{(x + 1) * xStep}_{(y + 1) * yStep}_1"];
                int v3 = vertDict[$"{x * xStep}_{(y + 1) * yStep}_1"];
                AddQuad(v0, v1, v2, v3, 1);
            }
        }

        // Back (-Z)
        for (int y = 0; y <= ySub; y++) { for (int x = 0; x <= xSub; x++) { GetVertex(x * xStep, y * yStep, 0f); } }
        for (int y = 0; y < ySub; y++)
        {
            for (int x = 0; x < xSub; x++)
            {
                int v0 = vertDict[$"{(x + 1) * xStep}_{y * yStep}_0"];
                int v1 = vertDict[$"{x * xStep}_{y * yStep}_0"];
                int v2 = vertDict[$"{x * xStep}_{(y + 1) * yStep}_0"];
                int v3 = vertDict[$"{(x + 1) * xStep}_{(y + 1) * yStep}_0"];
                AddQuad(v0, v1, v2, v3, 2);
            }
        }

        // Right (+X)
        for (int y = 0; y <= ySub; y++) { for (int z = 0; z <= zSub; z++) { GetVertex(1f, y * yStep, z * zStep); } }
        for (int y = 0; y < ySub; y++)
        {
            for (int z = 0; z < zSub; z++)
            {
                int v0 = vertDict[$"1_{y * yStep}_{z * zStep}"];
                int v1 = vertDict[$"1_{y * yStep}_{(z + 1) * zStep}"];
                int v2 = vertDict[$"1_{(y + 1) * yStep}_{(z + 1) * zStep}"];
                int v3 = vertDict[$"1_{(y + 1) * yStep}_{z * zStep}"];
                AddQuad(v0, v3, v2, v1, 3);
            }
        }

        // Left (-X)
        for (int y = 0; y <= ySub; y++) { for (int z = 0; z <= zSub; z++) { GetVertex(0f, y * yStep, z * zStep); } }
        for (int y = 0; y < ySub; y++)
        {
            for (int z = 0; z < zSub; z++)
            {
                int v0 = vertDict[$"0_{y * yStep}_{(z + 1) * zStep}"];
                int v1 = vertDict[$"0_{y * yStep}_{z * zStep}"];
                int v2 = vertDict[$"0_{(y + 1) * yStep}_{z * zStep}"];
                int v3 = vertDict[$"0_{(y + 1) * yStep}_{(z + 1) * zStep}"];
                AddQuad(v0, v3, v2, v1, 4);
            }
        }

        // Top (+Y)
        for (int x = 0; x <= xSub; x++) { for (int z = 0; z <= zSub; z++) { GetVertex(x * xStep, 1f, z * zStep); } }
        for (int x = 0; x < xSub; x++)
        {
            for (int z = 0; z < zSub; z++)
            {
                int v0 = vertDict[$"{x * xStep}_1_{z * zStep}"];
                int v1 = vertDict[$"{(x + 1) * xStep}_1_{z * zStep}"];
                int v2 = vertDict[$"{(x + 1) * xStep}_1_{(z + 1) * zStep}"];
                int v3 = vertDict[$"{x * xStep}_1_{(z + 1) * zStep}"];
                AddQuad(v0, v3, v2, v1, 5);
            }
        }

        // Bottom (-Y)
        for (int x = 0; x <= xSub; x++) { for (int z = 0; z <= zSub; z++) { GetVertex(x * xStep, 0f, z * zStep); } }
        for (int x = 0; x < xSub; x++)
        {
            for (int z = 0; z < zSub; z++)
            {
                int v0 = vertDict[$"{(x + 1) * xStep}_0_{z * zStep}"];
                int v1 = vertDict[$"{x * xStep}_0_{z * zStep}"];
                int v2 = vertDict[$"{x * xStep}_0_{(z + 1) * zStep}"];
                int v3 = vertDict[$"{(x + 1) * xStep}_0_{(z + 1) * zStep}"];
                AddQuad(v0, v3, v2, v1, 6);
            }
        }

        var positions = vertices.Select(v => v.position).ToArray();
        mesh.RebuildWithPositionsAndFaces(positions, faces);
        Smoothing.ApplySmoothingGroups(mesh, faces, 180f);
        Normals.CalculateTangents(mesh);
        mesh.ToMesh();
        mesh.Refresh(RefreshMask.All);

        return mesh;
    }

    /// <summary>
    /// Generate a properly welded icosphere
    /// </summary>
    private ProBuilderMesh GenerateWeldedSphere(float radius, int subdivisions)
    {
        subdivisions = Mathf.Clamp(subdivisions, 0, 3);

        GameObject go = new GameObject("WeldedIcosphere");
        ProBuilderMesh mesh = go.AddComponent<ProBuilderMesh>();

        List<Vector3> positions = new List<Vector3>();
        List<int> indices = new List<int>();
        Dictionary<string, int> vertDict = new Dictionary<string, int>();

        int GetOrCreateVertex(Vector3 pos)
        {
            string key = $"{pos.x:F6}_{pos.y:F6}_{pos.z:F6}";
            if (vertDict.TryGetValue(key, out int index))
                return index;

            index = positions.Count;
            positions.Add(pos.normalized * radius);
            vertDict[key] = index;
            return index;
        }

        int GetMidpoint(int v1, int v2)
        {
            Vector3 mid = (positions[v1] + positions[v2]) / 2f;
            return GetOrCreateVertex(mid);
        }

        // Create initial icosahedron
        float t = (1f + Mathf.Sqrt(5f)) / 2f;

        GetOrCreateVertex(new Vector3(-1, t, 0));
        GetOrCreateVertex(new Vector3(1, t, 0));
        GetOrCreateVertex(new Vector3(-1, -t, 0));
        GetOrCreateVertex(new Vector3(1, -t, 0));
        GetOrCreateVertex(new Vector3(0, -1, t));
        GetOrCreateVertex(new Vector3(0, 1, t));
        GetOrCreateVertex(new Vector3(0, -1, -t));
        GetOrCreateVertex(new Vector3(0, 1, -t));
        GetOrCreateVertex(new Vector3(t, 0, -1));
        GetOrCreateVertex(new Vector3(t, 0, 1));
        GetOrCreateVertex(new Vector3(-t, 0, -1));
        GetOrCreateVertex(new Vector3(-t, 0, 1));

        // Initial 20 faces
        List<int[]> triangles = new List<int[]>
        {
            new int[] {0, 11, 5}, new int[] {0, 5, 1}, new int[] {0, 1, 7}, new int[] {0, 7, 10}, new int[] {0, 10, 11},
            new int[] {1, 5, 9}, new int[] {5, 11, 4}, new int[] {11, 10, 2}, new int[] {10, 7, 6}, new int[] {7, 1, 8},
            new int[] {3, 9, 4}, new int[] {3, 4, 2}, new int[] {3, 2, 6}, new int[] {3, 6, 8}, new int[] {3, 8, 9},
            new int[] {4, 9, 5}, new int[] {2, 4, 11}, new int[] {6, 2, 10}, new int[] {8, 6, 7}, new int[] {9, 8, 1}
        };

        // Subdivide
        for (int i = 0; i < subdivisions; i++)
        {
            List<int[]> newTriangles = new List<int[]>();
            foreach (var tri in triangles)
            {
                int a = GetMidpoint(tri[0], tri[1]);
                int b = GetMidpoint(tri[1], tri[2]);
                int c = GetMidpoint(tri[2], tri[0]);

                newTriangles.Add(new int[] { tri[0], a, c });
                newTriangles.Add(new int[] { tri[1], b, a });
                newTriangles.Add(new int[] { tri[2], c, b });
                newTriangles.Add(new int[] { a, b, c });
            }
            triangles = newTriangles;
        }

        // Create faces
        List<Face> faces = new List<Face>();
        foreach (var tri in triangles)
        {
            faces.Add(new Face(tri));
        }

        mesh.RebuildWithPositionsAndFaces(positions.ToArray(), faces);
        mesh.ToMesh();
        mesh.Refresh(RefreshMask.All);

        return mesh;
    }

    /// <summary>
    /// Generate a properly welded cylinder
    /// </summary>
    private ProBuilderMesh GenerateWeldedCylinder(float radius, float height, int segments, int heightSegments)
    {
        segments = Mathf.Max(3, segments);
        heightSegments = Mathf.Max(1, heightSegments);

        GameObject go = new GameObject("WeldedCylinder");
        ProBuilderMesh mesh = go.AddComponent<ProBuilderMesh>();

        List<Vector3> positions = new List<Vector3>();
        List<Face> faces = new List<Face>();
        Dictionary<string, int> vertDict = new Dictionary<string, int>();

        int GetVertex(float x, float y, float z)
        {
            string key = $"{x:F4}_{y:F4}_{z:F4}";
            if (vertDict.TryGetValue(key, out int index))
                return index;

            index = positions.Count;
            positions.Add(new Vector3(x, y, z));
            vertDict[key] = index;
            return index;
        }

        float halfHeight = height / 2f;

        for (int h = 0; h <= heightSegments; h++)
        {
            float y = -halfHeight + (height * h / heightSegments);
            for (int s = 0; s < segments; s++)
            {
                float angle = (float)s / segments * Mathf.PI * 2f;
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;
                GetVertex(x, y, z);
            }
        }

        for (int h = 0; h < heightSegments; h++)
        {
            for (int s = 0; s < segments; s++)
            {
                float y0 = -halfHeight + (height * h / heightSegments);
                float y1 = -halfHeight + (height * (h + 1) / heightSegments);

                float angle0 = (float)s / segments * Mathf.PI * 2f;
                float angle1 = (float)((s + 1) % segments) / segments * Mathf.PI * 2f;

                int v0 = vertDict[$"{Mathf.Cos(angle0) * radius:F4}_{y0:F4}_{Mathf.Sin(angle0) * radius:F4}"];
                int v1 = vertDict[$"{Mathf.Cos(angle1) * radius:F4}_{y0:F4}_{Mathf.Sin(angle1) * radius:F4}"];
                int v2 = vertDict[$"{Mathf.Cos(angle1) * radius:F4}_{y1:F4}_{Mathf.Sin(angle1) * radius:F4}"];
                int v3 = vertDict[$"{Mathf.Cos(angle0) * radius:F4}_{y1:F4}_{Mathf.Sin(angle0) * radius:F4}"];

                faces.Add(new Face(new int[] { v0, v1, v2 }));
                faces.Add(new Face(new int[] { v0, v2, v3 }));
            }
        }

        int centerTop = GetVertex(0, halfHeight, 0);
        int centerBottom = GetVertex(0, -halfHeight, 0);

        for (int s = 0; s < segments; s++)
        {
            float angle0 = (float)s / segments * Mathf.PI * 2f;
            float angle1 = (float)((s + 1) % segments) / segments * Mathf.PI * 2f;

            int vTop0 = vertDict[$"{Mathf.Cos(angle0) * radius:F4}_{halfHeight:F4}_{Mathf.Sin(angle0) * radius:F4}"];
            int vTop1 = vertDict[$"{Mathf.Cos(angle1) * radius:F4}_{halfHeight:F4}_{Mathf.Sin(angle1) * radius:F4}"];
            faces.Add(new Face(new int[] { centerTop, vTop1, vTop0 }));

            int vBot0 = vertDict[$"{Mathf.Cos(angle0) * radius:F4}_{-halfHeight:F4}_{Mathf.Sin(angle0) * radius:F4}"];
            int vBot1 = vertDict[$"{Mathf.Cos(angle1) * radius:F4}_{-halfHeight:F4}_{Mathf.Sin(angle1) * radius:F4}"];
            faces.Add(new Face(new int[] { centerBottom, vBot0, vBot1 }));
        }

        mesh.RebuildWithPositionsAndFaces(positions.ToArray(), faces);
        mesh.ToMesh();
        mesh.Refresh(RefreshMask.All);

        return mesh;
    }

    /// <summary>
    /// Generate a properly welded cone
    /// </summary>
    private ProBuilderMesh GenerateWeldedCone(float radius, float height, int segments)
    {
        segments = Mathf.Max(3, segments);

        GameObject go = new GameObject("WeldedCone");
        ProBuilderMesh mesh = go.AddComponent<ProBuilderMesh>();

        List<Vector3> positions = new List<Vector3>();
        List<Face> faces = new List<Face>();

        positions.Add(new Vector3(0, height / 2f, 0));

        int centerBottom = positions.Count;
        positions.Add(new Vector3(0, -height / 2f, 0));

        for (int s = 0; s < segments; s++)
        {
            float angle = (float)s / segments * Mathf.PI * 2f;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            positions.Add(new Vector3(x, -height / 2f, z));
        }

        for (int s = 0; s < segments; s++)
        {
            int v0 = 0;
            int v1 = 2 + s;
            int v2 = 2 + ((s + 1) % segments);
            faces.Add(new Face(new int[] { v0, v1, v2 }));
        }

        for (int s = 0; s < segments; s++)
        {
            int v0 = centerBottom;
            int v1 = 2 + s;
            int v2 = 2 + ((s + 1) % segments);
            faces.Add(new Face(new int[] { v0, v2, v1 }));
        }

        mesh.RebuildWithPositionsAndFaces(positions.ToArray(), faces);
        mesh.ToMesh();
        mesh.Refresh(RefreshMask.All);

        return mesh;
    }

    /// <summary>
    /// Generate a properly welded pipe (hollow cylinder)
    /// </summary>
    private ProBuilderMesh GenerateWeldedPipe(float outerRadius, float height, float thickness, int segments, int heightSegments)
    {
        segments = Mathf.Max(3, segments);
        heightSegments = Mathf.Max(0, heightSegments);
        float innerRadius = Mathf.Max(0.01f, outerRadius - thickness);

        GameObject go = new GameObject("WeldedPipe");
        ProBuilderMesh mesh = go.AddComponent<ProBuilderMesh>();

        List<Vector3> positions = new List<Vector3>();
        List<Face> faces = new List<Face>();
        Dictionary<string, int> vertDict = new Dictionary<string, int>();

        int GetVertex(float x, float y, float z)
        {
            string key = $"{x:F4}_{y:F4}_{z:F4}";
            if (vertDict.TryGetValue(key, out int index))
                return index;

            index = positions.Count;
            positions.Add(new Vector3(x, y, z));
            vertDict[key] = index;
            return index;
        }

        float halfHeight = height / 2f;
        int totalHeightSegments = heightSegments + 1;

        // Generate all vertices (outer and inner rings)
        for (int h = 0; h <= totalHeightSegments; h++)
        {
            float y = -halfHeight + (height * h / totalHeightSegments);

            for (int s = 0; s < segments; s++)
            {
                float angle = (float)s / segments * Mathf.PI * 2f;
                float cosA = Mathf.Cos(angle);
                float sinA = Mathf.Sin(angle);

                // Outer vertex
                GetVertex(cosA * outerRadius, y, sinA * outerRadius);

                // Inner vertex
                GetVertex(cosA * innerRadius, y, sinA * innerRadius);
            }
        }

        // Create outer side faces
        for (int h = 0; h < totalHeightSegments; h++)
        {
            for (int s = 0; s < segments; s++)
            {
                float y0 = -halfHeight + (height * h / totalHeightSegments);
                float y1 = -halfHeight + (height * (h + 1) / totalHeightSegments);
                float angle0 = (float)s / segments * Mathf.PI * 2f;
                float angle1 = (float)((s + 1) % segments) / segments * Mathf.PI * 2f;

                int v0 = vertDict[$"{Mathf.Cos(angle0) * outerRadius:F4}_{y0:F4}_{Mathf.Sin(angle0) * outerRadius:F4}"];
                int v1 = vertDict[$"{Mathf.Cos(angle1) * outerRadius:F4}_{y0:F4}_{Mathf.Sin(angle1) * outerRadius:F4}"];
                int v2 = vertDict[$"{Mathf.Cos(angle1) * outerRadius:F4}_{y1:F4}_{Mathf.Sin(angle1) * outerRadius:F4}"];
                int v3 = vertDict[$"{Mathf.Cos(angle0) * outerRadius:F4}_{y1:F4}_{Mathf.Sin(angle0) * outerRadius:F4}"];

                faces.Add(new Face(new int[] { v0, v1, v2 }));
                faces.Add(new Face(new int[] { v0, v2, v3 }));
            }
        }

        // Create inner side faces (reversed winding for inward-facing normals)
        for (int h = 0; h < totalHeightSegments; h++)
        {
            for (int s = 0; s < segments; s++)
            {
                float y0 = -halfHeight + (height * h / totalHeightSegments);
                float y1 = -halfHeight + (height * (h + 1) / totalHeightSegments);
                float angle0 = (float)s / segments * Mathf.PI * 2f;
                float angle1 = (float)((s + 1) % segments) / segments * Mathf.PI * 2f;

                int v0 = vertDict[$"{Mathf.Cos(angle0) * innerRadius:F4}_{y0:F4}_{Mathf.Sin(angle0) * innerRadius:F4}"];
                int v1 = vertDict[$"{Mathf.Cos(angle1) * innerRadius:F4}_{y0:F4}_{Mathf.Sin(angle1) * innerRadius:F4}"];
                int v2 = vertDict[$"{Mathf.Cos(angle1) * innerRadius:F4}_{y1:F4}_{Mathf.Sin(angle1) * innerRadius:F4}"];
                int v3 = vertDict[$"{Mathf.Cos(angle0) * innerRadius:F4}_{y1:F4}_{Mathf.Sin(angle0) * innerRadius:F4}"];

                faces.Add(new Face(new int[] { v0, v2, v1 }));
                faces.Add(new Face(new int[] { v0, v3, v2 }));
            }
        }

        // Create top and bottom ring caps
        for (int s = 0; s < segments; s++)
        {
            float angle0 = (float)s / segments * Mathf.PI * 2f;
            float angle1 = (float)((s + 1) % segments) / segments * Mathf.PI * 2f;

            // Top ring
            int vTopOuter0 = vertDict[$"{Mathf.Cos(angle0) * outerRadius:F4}_{halfHeight:F4}_{Mathf.Sin(angle0) * outerRadius:F4}"];
            int vTopOuter1 = vertDict[$"{Mathf.Cos(angle1) * outerRadius:F4}_{halfHeight:F4}_{Mathf.Sin(angle1) * outerRadius:F4}"];
            int vTopInner0 = vertDict[$"{Mathf.Cos(angle0) * innerRadius:F4}_{halfHeight:F4}_{Mathf.Sin(angle0) * innerRadius:F4}"];
            int vTopInner1 = vertDict[$"{Mathf.Cos(angle1) * innerRadius:F4}_{halfHeight:F4}_{Mathf.Sin(angle1) * innerRadius:F4}"];

            faces.Add(new Face(new int[] { vTopOuter0, vTopOuter1, vTopInner1 }));
            faces.Add(new Face(new int[] { vTopOuter0, vTopInner1, vTopInner0 }));

            // Bottom ring
            int vBotOuter0 = vertDict[$"{Mathf.Cos(angle0) * outerRadius:F4}_{-halfHeight:F4}_{Mathf.Sin(angle0) * outerRadius:F4}"];
            int vBotOuter1 = vertDict[$"{Mathf.Cos(angle1) * outerRadius:F4}_{-halfHeight:F4}_{Mathf.Sin(angle1) * outerRadius:F4}"];
            int vBotInner0 = vertDict[$"{Mathf.Cos(angle0) * innerRadius:F4}_{-halfHeight:F4}_{Mathf.Sin(angle0) * innerRadius:F4}"];
            int vBotInner1 = vertDict[$"{Mathf.Cos(angle1) * innerRadius:F4}_{-halfHeight:F4}_{Mathf.Sin(angle1) * innerRadius:F4}"];

            faces.Add(new Face(new int[] { vBotOuter0, vBotInner1, vBotOuter1 }));
            faces.Add(new Face(new int[] { vBotOuter0, vBotInner0, vBotInner1 }));
        }

        mesh.RebuildWithPositionsAndFaces(positions.ToArray(), faces);
        mesh.ToMesh();
        mesh.Refresh(RefreshMask.All);

        return mesh;
    }

    /// <summary>
    /// Generate a properly welded plane
    /// </summary>
    private ProBuilderMesh GenerateWeldedPlane(float width, float depth, int widthSegments, int depthSegments)
    {
        widthSegments = Mathf.Max(1, widthSegments);
        depthSegments = Mathf.Max(1, depthSegments);

        GameObject go = new GameObject("WeldedPlane");
        ProBuilderMesh mesh = go.AddComponent<ProBuilderMesh>();

        List<Vector3> positions = new List<Vector3>();
        List<Face> faces = new List<Face>();

        float halfWidth = width / 2f;
        float halfDepth = depth / 2f;

        for (int z = 0; z <= depthSegments; z++)
        {
            for (int x = 0; x <= widthSegments; x++)
            {
                float px = -halfWidth + (width * x / widthSegments);
                float pz = -halfDepth + (depth * z / depthSegments);
                positions.Add(new Vector3(px, 0, pz));
            }
        }

        for (int z = 0; z < depthSegments; z++)
        {
            for (int x = 0; x < widthSegments; x++)
            {
                int v0 = z * (widthSegments + 1) + x;
                int v1 = z * (widthSegments + 1) + x + 1;
                int v2 = (z + 1) * (widthSegments + 1) + x + 1;
                int v3 = (z + 1) * (widthSegments + 1) + x;

                faces.Add(new Face(new int[] { v0, v1, v2 }));
                faces.Add(new Face(new int[] { v0, v2, v3 }));
            }
        }

        mesh.RebuildWithPositionsAndFaces(positions.ToArray(), faces);
        mesh.ToMesh();
        mesh.Refresh(RefreshMask.All);

        return mesh;
    }

    /// <summary>
    /// Generate a properly welded dome plane
    /// </summary>
    private ProBuilderMesh GenerateWeldedDomePlane(float width, float depth, int widthSegments, int depthSegments, float domeHeight)
    {
        ProBuilderMesh planeMesh = GenerateWeldedPlane(width, depth, widthSegments, depthSegments);

        var vertices = planeMesh.positions.ToArray();
        float radiusX = width / 2f;
        float radiusZ = depth / 2f;

        if (radiusX == 0f || radiusZ == 0f) return planeMesh;

        for (int i = 0; i < vertices.Length; i++)
        {
            float normalizedX = vertices[i].x / radiusX;
            float normalizedZ = vertices[i].z / radiusZ;
            float ellipticalDist = Mathf.Sqrt(normalizedX * normalizedX + normalizedZ * normalizedZ);

            if (ellipticalDist > 1f) ellipticalDist = 1f;

            float heightFactor = Mathf.Cos(ellipticalDist * Mathf.PI / 2f);
            vertices[i].y = heightFactor * domeHeight;
        }

        planeMesh.positions = vertices.ToList();
        planeMesh.ToMesh();
        planeMesh.Refresh(RefreshMask.All);

        return planeMesh;
    }

    /// <summary>
    /// Generate a properly welded wavy plane
    /// </summary>
    private ProBuilderMesh GenerateWeldedWavyPlane(float width, float depth, int widthSegments, int depthSegments, float waveHeight)
    {
        ProBuilderMesh planeMesh = GenerateWeldedPlane(width, depth, widthSegments, depthSegments);

        var vertices = planeMesh.positions.ToArray();
        for (int i = 0; i < vertices.Length; i++)
        {
            float x = vertices[i].x;
            float z = vertices[i].z;
            float y = Mathf.Sin(x) * Mathf.Sin(z) * waveHeight;
            vertices[i] = new Vector3(x, y, z);
        }

        planeMesh.positions = vertices.ToList();
        planeMesh.ToMesh();
        planeMesh.Refresh(RefreshMask.All);

        return planeMesh;
    }

    /// <summary>
    /// Size slider changed - scale the model
    /// </summary>
    private void SizeSliderUpdated(float val)
    {
        if (currentModel != null)
        {
            if (currentShapeType == ModelType.Pipe)
            {
                ProBuilderMesh mesh = CreateModelByType(currentShapeType);
                mesh.transform.position = currentModel.GetPosition();
                currentModel.UpdateMeshCreation(mesh);
                Destroy(mesh.gameObject);
            }
            else
            {
                Vector3 newScale = Vector3.one * sizeSlider.GetValueAsFloat();
                if (!hasUniformSize)
                {
                    newScale = new Vector3(xSizeSlider.GetValueAsFloat(), ySizeSlider.GetValueAsFloat(), zSizeSlider.GetValueAsFloat());
                }

                currentModel.SetScale(newScale);
            }
        }
    }

    /// <summary>
    /// Sub d was changed - recreate the model
    /// </summary>
    private void SubDSliderUpdated(float val)
    {
        if (currentModel != null)
        {
            ProBuilderMesh mesh = CreateModelByType(currentShapeType);
            mesh.transform.position = currentModel.GetPosition();
            currentModel.UpdateMeshCreation(mesh);
            Destroy(mesh.gameObject);
        }
    }
}