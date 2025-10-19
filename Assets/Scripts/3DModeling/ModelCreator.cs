using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEditor.ProBuilder;

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
public class ModelCreator : MonoBehaviour
{
    public ModelData currentModel;
    public ModelType currentShapeType;

    public bool hasUniformSize;
    public Toggle uniformSizeToggle;
    public SliderUI sizeSlider;
    public SliderUI xSizeSlider;
    public SliderUI ySizeSlider;
    public SliderUI zSizeSlider;

    public Transform meshCreationPoint;


    public SliderUI subDivisionSlider;
    public SliderUI xSubDivisionSlider;
    public SliderUI ySubDivisionSlider;
    public SliderUI zSubDivisionSlider;

    
    public ToggleGroupUI modelTypeToggleGroupUI;

    void Start()
    {
        Debug.Log(modelTypeToggleGroupUI);
        Debug.Log(modelTypeToggleGroupUI.OnToggleGroupChanged);
        modelTypeToggleGroupUI.OnToggleGroupChanged.AddListener(ModelTypeSelected);


        sizeSlider.OnSliderValueChangedEvent.AddListener(SizeSliderUpdated);
        xSizeSlider.OnSliderValueChangedEvent.AddListener(SizeSliderUpdated);
        ySizeSlider.OnSliderValueChangedEvent.AddListener(SizeSliderUpdated);
        zSizeSlider.OnSliderValueChangedEvent.AddListener(SizeSliderUpdated);

        subDivisionSlider.OnSliderValueChangedEvent.AddListener(SubDSliderUpdated);
        xSubDivisionSlider.OnSliderValueChangedEvent.AddListener(SubDSliderUpdated);
        ySubDivisionSlider.OnSliderValueChangedEvent.AddListener(SubDSliderUpdated);
        zSubDivisionSlider.OnSliderValueChangedEvent.AddListener(SubDSliderUpdated);


        UniformSizeToggleChanged();
    }



    public void FinalizeLastModel()
    {
        if (currentModel != null)
        {
            SelectionManager.Instance.RemoveModelFromSelection(currentModel);
            currentModel.FinalizeEditModel();
            currentModel = null;
        }
    }





    public void ModelTypeSelected(Toggle toggle)
    {
        Debug.Log(toggle);
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

                CreateOrUpdateModel();



            }
 
        }
    }

    public void ResetSliders()
    {
        sizeSlider.SetValue(1);
        xSizeSlider.SetValue(1);
        ySizeSlider.SetValue(1);
        zSizeSlider.SetValue(1);
        subDivisionSlider.SetValue(1);
        xSubDivisionSlider.SetValue(1);
        ySubDivisionSlider.SetValue(1);
        zSubDivisionSlider.SetValue(1);


    }



    public void UniformSizeToggleChanged()
    {
        UniformChangedUpdateSliderValues();
        UpdateSizeSlidersUI();
        UpdateSubDSlidersMinMax();
        UpdateSubDSliders();
    }




    private void UniformChangedUpdateSliderValues()
    {
        if(hasUniformSize != uniformSizeToggle.isOn)
        {
            hasUniformSize = uniformSizeToggle.isOn;

            if (hasUniformSize == true)
            {
                sizeSlider.SetValue(xSizeSlider.GetValue());
            }
            else
            {
                xSizeSlider.SetValue(sizeSlider.GetValue());
                ySizeSlider.SetValue(sizeSlider.GetValue());
                zSizeSlider.SetValue(sizeSlider.GetValue());
            }
        }
    }
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
            }
            else
            {
                sizeSlider.Hide();
                xSizeSlider.Show();
                ySizeSlider.Hide();
                zSizeSlider.Show();
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
            }
            else
            {
                sizeSlider.Hide();
                xSizeSlider.Show();
                ySizeSlider.Show();
                zSizeSlider.Show();
            }
        }
    }
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
            ySubDivisionSlider.Hide();
            zSubDivisionSlider.Hide();
        }
        else if (currentShapeType == ModelType.Cone || currentShapeType == ModelType.Cylinder)//cone,  Cylinder
        {

            subDivisionSlider.Show();
            xSubDivisionSlider.Hide();
            ySubDivisionSlider.Show();
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




    private void CreateOrUpdateModel()
    {
        if (currentModel != null)
        {
            
            currentModel.DeleteModel();
            currentModel = null;
        }
        ProBuilderMesh mesh = null;

        mesh = CreateModelByType(currentShapeType);
        currentModel = mesh.gameObject.AddComponent<ModelData>();
        currentModel.SetupModel(mesh);
        currentModel.SetPosition(meshCreationPoint.position);
        SizeSliderUpdated(1);
        SelectionManager.Instance.SelectModel(currentModel);
    }


    private ProBuilderMesh CreateModelByType(ModelType type)
    {
        ProBuilderMesh mesh = null;
        if (type == ModelType.Cube)
        {
            mesh = CreateSubdividedCube(Mathf.RoundToInt(xSubDivisionSlider.GetValue()), Mathf.RoundToInt(ySubDivisionSlider.GetValue()), Mathf.RoundToInt(zSubDivisionSlider.GetValue()));
        }
        else if (type == ModelType.Sphere) mesh = ShapeGenerator.GenerateIcosahedron(PivotLocation.Center, 1, Mathf.RoundToInt(subDivisionSlider.GetValue()));
        else if (type == ModelType.PlaneFlat) mesh = ShapeGenerator.GeneratePlane(PivotLocation.Center, 1, 1, Mathf.RoundToInt(xSubDivisionSlider.GetValue()), Mathf.RoundToInt(zSubDivisionSlider.GetValue()), Axis.Up);
        else if (type == ModelType.Cone) mesh = ShapeGenerator.GenerateCone(PivotLocation.Center, xSizeSlider.GetValue(), ySizeSlider.GetValue(), Mathf.RoundToInt(subDivisionSlider.GetValue()));
        else if (type == ModelType.Pipe) mesh = ShapeGenerator.GeneratePipe(PivotLocation.Center, xSizeSlider.GetValue(), ySizeSlider.GetValue(), zSizeSlider.GetValue(), Mathf.RoundToInt(subDivisionSlider.GetValue()), Mathf.RoundToInt(ySubDivisionSlider.GetValue()));
        else if (type == ModelType.Cylinder) mesh = ShapeGenerator.GenerateCylinder(PivotLocation.Center, Mathf.RoundToInt(subDivisionSlider.GetValue()), xSizeSlider.GetValue(), ySizeSlider.GetValue(), Mathf.RoundToInt(ySubDivisionSlider.GetValue()));
        else if (type == ModelType.PlaneDome)
        {
            mesh = GenerateDomePlane(1, 1, Mathf.RoundToInt(xSubDivisionSlider.GetValue()), Mathf.RoundToInt(zSubDivisionSlider.GetValue()), 1); 
        }
        else if (type == ModelType.PlaneWavy)
        {
            mesh = GenerateWavyPlane(1, 1, Mathf.RoundToInt(xSubDivisionSlider.GetValue()), Mathf.RoundToInt(zSubDivisionSlider.GetValue()), 1);
        }
        return mesh;
    }









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
            float y = heightFactor * domeHeight * 0.1f;

            vertices[i].y = y;
        }

        planeMesh.positions = vertices.ToList();
        planeMesh.ToMesh();
        planeMesh.Refresh(RefreshMask.All);

        return planeMesh;
    }





    private void SizeSliderUpdated(float val)
    {
        if (currentModel != null)
        {
            Vector3 newScale = Vector3.one * sizeSlider.GetValue();
            if (!hasUniformSize)
            {
                newScale = new Vector3(xSizeSlider.GetValue(), ySizeSlider.GetValue(), zSizeSlider.GetValue());
            }

            currentModel.transform.localScale = newScale;
        }
    }
    private void SubDSliderUpdated(float val)
    {
        Debug.Log("fwqfe");
        if (currentModel != null)
        {
            ProBuilderMesh mesh = CreateModelByType(currentShapeType);
            mesh.transform.position = currentModel.transform.position;
            currentModel.UpdateMeshCreation(mesh);
            Destroy(mesh.gameObject);
        }
    }


}