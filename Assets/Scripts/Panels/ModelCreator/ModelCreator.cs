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
    /// 
    //TODO: Maybe monitor sleection change too show you can finalize the model and allow the user to easily create a new one
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
        if(hasUniformSize != uniformSizeToggle.isOn)
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
        if (currentShapeType == ModelType.PlaneFlat) // plane
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
            if (currentShapeType == ModelType.Pipe)//pipe,
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
            xSubDivisionSlider.SetMinMax(1,10);
            ySubDivisionSlider.SetMinMax(1,10);
            zSubDivisionSlider.SetMinMax(1,10);
        }
        else if (currentShapeType == ModelType.Sphere)//sphere
        {
            subDivisionSlider.SetMinMax(0,5);
        }
        else if (currentShapeType == ModelType.Cone || currentShapeType == ModelType.Pipe || currentShapeType == ModelType.Cylinder)//cone, pipe, Cylinder
        {
            subDivisionSlider.SetMinMax(3,10);
            ySubDivisionSlider.SetMinMax(0,10);
        }
        else if (currentShapeType == ModelType.PlaneFlat || currentShapeType == ModelType.PlaneDome || currentShapeType == ModelType.PlaneWavy)//planes
        {
            xSubDivisionSlider.SetMinMax(0,10);
            zSubDivisionSlider.SetMinMax(0,10);
        }
    }
    private void UpdateSubDSliders()
    {
        if (currentShapeType == ModelType.Cube)//Cube
        {
            subDivisionSlider.Hide();
            xSubDivisionSlider.Show();
            ySubDivisionSlider.Show();
            zSubDivisionSlider.Show();
        }
        else if (currentShapeType == ModelType.Sphere)//Sphere
        {
            subDivisionSlider.Show();
            xSubDivisionSlider.Hide();
            ySubDivisionSlider.Hide();
            zSubDivisionSlider.Hide();
        }
        else if (currentShapeType == ModelType.Pipe)//pipe,
        {

            subDivisionSlider.Show();
            xSubDivisionSlider.Hide();
            ySubDivisionSlider.Show();
            zSubDivisionSlider.Hide();
        }
        else if (currentShapeType == ModelType.Cylinder)// Cylinder
        {

            subDivisionSlider.Show();
            xSubDivisionSlider.Hide();
            ySubDivisionSlider.Show();
            zSubDivisionSlider.Hide();

        }
        else if (currentShapeType == ModelType.Cone)//cone,  
        {

            subDivisionSlider.Show();
            xSubDivisionSlider.Hide();
            ySubDivisionSlider.Hide();
            zSubDivisionSlider.Hide();

        }
        else if (currentShapeType == ModelType.PlaneFlat || currentShapeType == ModelType.PlaneDome || currentShapeType == ModelType.PlaneWavy)//planes
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
        //Delete the last model and reset
        if (currentModel != null)
        {
            currentModel.DeleteModel();
            currentModel = null;
        }
        ProBuilderMesh mesh = null;

        //Create model
        mesh = CreateModelByType(currentShapeType);
        //Setup model
        GameObject myObject = new GameObject();
        currentModel = myObject.AddComponent<ModelData>();
        currentModel.SetupModel(mesh, meshCreationPoint.position);

        SizeSliderUpdated(1);
        SelectionManager.Instance.SelectModel(currentModel);
    }
    /// <summary>
    /// Create probuilder mesh by type
    /// </summary>
    private ProBuilderMesh CreateModelByType(ModelType type)
    {
        ProBuilderMesh mesh = null;
        if (type == ModelType.Cube)
        {
            mesh = CreateSubdividedCube(Mathf.RoundToInt(xSubDivisionSlider.GetValueAsFloat()), Mathf.RoundToInt(ySubDivisionSlider.GetValueAsFloat()), Mathf.RoundToInt(zSubDivisionSlider.GetValueAsFloat()));
        }
        else if (type == ModelType.Sphere) mesh = ShapeGenerator.GenerateIcosahedron(PivotLocation.Center, 1, Mathf.RoundToInt(subDivisionSlider.GetValueAsFloat()));
        else if (type == ModelType.PlaneFlat) mesh = ShapeGenerator.GeneratePlane(PivotLocation.Center, 1, 1, Mathf.RoundToInt(xSubDivisionSlider.GetValueAsFloat()), Mathf.RoundToInt(zSubDivisionSlider.GetValueAsFloat()), Axis.Up);
        else if (type == ModelType.Cone) mesh = ShapeGenerator.GenerateCone(PivotLocation.Center, xSizeSlider.GetValueAsFloat(), ySizeSlider.GetValueAsFloat(), Mathf.RoundToInt(subDivisionSlider.GetValueAsFloat()));
        else if (type == ModelType.Pipe)
        {
            if(hasUniformSize)
            {
                mesh = ShapeGenerator.GeneratePipe(PivotLocation.Center, sizeSlider.GetValueAsFloat(), sizeSlider.GetValueAsFloat(), sizeSlider.GetValueAsFloat() / 2,
                Mathf.RoundToInt(subDivisionSlider.GetValueAsFloat()), Mathf.RoundToInt(ySubDivisionSlider.GetValueAsFloat()));
            }
            else
            {
                float thickness = thicknessSizeSlider.GetValueAsFloat();
                if(thickness > (xSizeSlider.GetValueAsFloat()) - 0.01f)
                {
                    thickness = (xSizeSlider.GetValueAsFloat()) - 0.01f;
                }
                mesh = ShapeGenerator.GeneratePipe(PivotLocation.Center, xSizeSlider.GetValueAsFloat(), ySizeSlider.GetValueAsFloat(), thickness,
Mathf.RoundToInt(subDivisionSlider.GetValueAsFloat()), Mathf.RoundToInt(ySubDivisionSlider.GetValueAsFloat()));
            }
        }


        else if (type == ModelType.Cylinder) mesh = ShapeGenerator.GenerateCylinder(PivotLocation.Center, Mathf.RoundToInt(subDivisionSlider.GetValueAsFloat()), xSizeSlider.GetValueAsFloat(), ySizeSlider.GetValueAsFloat(), Mathf.RoundToInt(ySubDivisionSlider.GetValueAsFloat()));
        else if (type == ModelType.PlaneDome)
        {
            mesh = GenerateDomePlane(1, 1, Mathf.RoundToInt(xSubDivisionSlider.GetValueAsFloat()), Mathf.RoundToInt(zSubDivisionSlider.GetValueAsFloat()), 1);
        }
        else if (type == ModelType.PlaneWavy)
        {
            mesh = GenerateWavyPlane(1, 1, Mathf.RoundToInt(xSubDivisionSlider.GetValueAsFloat()), Mathf.RoundToInt(zSubDivisionSlider.GetValueAsFloat()), 1);
        }

        return mesh;
    }
    /// <summary>
    /// Create a cube
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


        //Front (+Z)
        for (int y = 0; y <= ySub; y++) { for (int x = 0; x <= xSub; x++) { GetVertex(x * xStep, y * yStep, 1f); } }
        for (int y = 0; y < ySub; y++)
        {
            for (int x = 0; x < xSub; x++)
            {
                int v0 = vertDict[$"{x * xStep}_{y * yStep}_1"];
                int v1 = vertDict[$"{(x + 1) * xStep}_{y * yStep}_1"];
                int v2 = vertDict[$"{(x + 1) * xStep}_{(y + 1) * yStep}_1"];
                int v3 = vertDict[$"{x * xStep}_{(y + 1) * yStep}_1"];
                AddQuad(v0, v1, v2, v3,1);
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
                AddQuad(v0, v1, v2, v3,2);
            }
        }

        //Right (+X)
        for (int y = 0; y <= ySub; y++) { for (int z = 0; z <= zSub; z++) { GetVertex(1f, y * yStep, z * zStep); } }
        for (int y = 0; y < ySub; y++)
        {
            for (int z = 0; z < zSub; z++)
            {
                int v0 = vertDict[$"1_{y * yStep}_{z * zStep}"];
                int v1 = vertDict[$"1_{y * yStep}_{(z + 1) * zStep}"];
                int v2 = vertDict[$"1_{(y + 1) * yStep}_{(z + 1) * zStep}"];
                int v3 = vertDict[$"1_{(y + 1) * yStep}_{z * zStep}"];
                AddQuad(v0, v3, v2, v1,3);
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
                AddQuad(v0, v3, v2, v1,4);
            }
        }

        //Top (+Y)
        for (int x = 0; x <= xSub; x++) { for (int z = 0; z <= zSub; z++) { GetVertex(x * xStep, 1f, z * zStep); } }
        for (int x = 0; x < xSub; x++)
        {
            for (int z = 0; z < zSub; z++)
            {
                int v0 = vertDict[$"{x * xStep}_1_{z * zStep}"];
                int v1 = vertDict[$"{(x + 1) * xStep}_1_{z * zStep}"];
                int v2 = vertDict[$"{(x + 1) * xStep}_1_{(z + 1) * zStep}"];
                int v3 = vertDict[$"{x * xStep}_1_{(z + 1) * zStep}"];
                AddQuad(v0, v3, v2, v1,5);
            }
        }

        //Bottom (-Y)
        for (int x = 0; x <= xSub; x++) { for (int z = 0; z <= zSub; z++) { GetVertex(x * xStep, 0f, z * zStep); } }
        for (int x = 0; x < xSub; x++)
        {
            for (int z = 0; z < zSub; z++)
            {
                int v0 = vertDict[$"{(x + 1) * xStep}_0_{z * zStep}"];
                int v1 = vertDict[$"{x * xStep}_0_{z * zStep}"];
                int v2 = vertDict[$"{x * xStep}_0_{(z + 1) * zStep}"];
                int v3 = vertDict[$"{(x + 1) * xStep}_0_{(z + 1) * zStep}"];
                AddQuad(v0, v3, v2, v1,6);
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
    /// Create a wavy plane
    /// </summary>
    private ProBuilderMesh GenerateWavyPlane(float width, float depth, int widthSubdivisions, int depthSubdivisions, float waveHeight)
    {
        widthSubdivisions = Mathf.Max(1, widthSubdivisions);
        depthSubdivisions = Mathf.Max(1, depthSubdivisions);

        ProBuilderMesh planeMesh = ShapeGenerator.GeneratePlane(PivotLocation.Center, width, depth, widthSubdivisions, depthSubdivisions, Axis.Up);

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
    /// Create a dome plane
    /// </summary>
    private ProBuilderMesh GenerateDomePlane(float width, float depth, int widthSubdivisions, int depthSubdivisions, float domeHeight)
    {
        widthSubdivisions = Mathf.Max(1, widthSubdivisions);
        depthSubdivisions = Mathf.Max(1, depthSubdivisions);

        ProBuilderMesh planeMesh = ShapeGenerator.GeneratePlane(PivotLocation.Center, width, depth, widthSubdivisions, depthSubdivisions, Axis.Up);

        var vertices = planeMesh.positions.ToArray();
        float radiusX = width / 2f;
        float radiusZ = depth / 2f;

        if (radiusX == 0f || radiusZ == 0f) return planeMesh;

        for (int i = 0; i < vertices.Length; i++)
        {
            float normalizedX = vertices[i].x / radiusX;
            float normalizedZ = vertices[i].z / radiusZ;

            float ellipticalDist = Mathf.Sqrt(normalizedX * normalizedX + normalizedZ * normalizedZ);

            if (ellipticalDist > 1f)
            {
                ellipticalDist = 1f;
            }

            float heightFactor = Mathf.Cos(ellipticalDist * Mathf.PI / 2f);
            float y = heightFactor * domeHeight;

            vertices[i].y = y;
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
            if (currentShapeType == ModelType.Pipe)//pipe,
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