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
    [Header("Current Model")]
    public ModelData currentModel;
    public ModelType currentShapeType;
    public TMP_Dropdown typeDropdown;

    [Header("UI Controls")]
    public bool hasUniformSize;
    public Toggle uniformSizeToggle;
    public Slider sizeSlider;
    public Slider xSizeSlider;
    public Slider ySizeSlider;
    public Slider zSizeSlider;

    public TMP_Text radiusText;
    public TMP_Text radiusSubDivText;

    public Slider subDivisionSlider;
    public Slider xSubDivisionSlider;
    public Slider ySubDivisionSlider;
    public Slider zSubDivisionSlider;

   

    void Start()
    {

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

    public void UniformSizeToggleUpdated()
    {
        Debug.Log("ff");
        if(hasUniformSize != uniformSizeToggle.isOn)
        {
            hasUniformSize = uniformSizeToggle.isOn;

            if (hasUniformSize == true)
            {
                sizeSlider.value = xSizeSlider.value;
            }
            else
            {
                xSizeSlider.value = sizeSlider.value;
                ySizeSlider.value = sizeSlider.value;
                zSizeSlider.value = sizeSlider.value;
            }
            UpdateUniformUI();
            TypeDropdownUpdated();
        }
    }

    public void UpdateUniformUI()
    {
        Debug.Log(typeDropdown);
        if (typeDropdown.value == 5) // plane
        {
            if (hasUniformSize)
            {
                sizeSlider.transform.parent.gameObject.SetActive(true);
                xSizeSlider.transform.parent.gameObject.SetActive(false);
                ySizeSlider.transform.parent.gameObject.SetActive(false);
                zSizeSlider.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                sizeSlider.transform.parent.gameObject.SetActive(false);
                xSizeSlider.transform.parent.gameObject.SetActive(true);
                ySizeSlider.transform.parent.gameObject.SetActive(false);
                zSizeSlider.transform.parent.gameObject.SetActive(true);
            }
        }
        else
        {
            if (hasUniformSize)
            {
                sizeSlider.transform.parent.gameObject.SetActive(true);
                xSizeSlider.transform.parent.gameObject.SetActive(false);
                ySizeSlider.transform.parent.gameObject.SetActive(false);
                zSizeSlider.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                sizeSlider.transform.parent.gameObject.SetActive(false);
                xSizeSlider.transform.parent.gameObject.SetActive(true);
                ySizeSlider.transform.parent.gameObject.SetActive(true);
                zSizeSlider.transform.parent.gameObject.SetActive(true);
            }
        }
    }

    public void UpdateSubDSlidersMinMax()
    {
        if (typeDropdown.value == 0)
        {
            xSubDivisionSlider.minValue = 1;
            xSubDivisionSlider.maxValue = 10;
            ySubDivisionSlider.minValue = 1;
            ySubDivisionSlider.maxValue = 10;
            zSubDivisionSlider.minValue = 1;
            zSubDivisionSlider.maxValue = 10;
        }
        else if (typeDropdown.value == 1)//sphere
        {
            subDivisionSlider.minValue = 0;
            subDivisionSlider.maxValue = 5;
        }
        else if (typeDropdown.value == 2 || typeDropdown.value == 3 || typeDropdown.value == 4)//cone, pipe, Cylinder
        {
            subDivisionSlider.minValue = 3;
            subDivisionSlider.maxValue = 10;
            ySubDivisionSlider.minValue = 0;
            ySubDivisionSlider.maxValue = 10;
        }
        else if (typeDropdown.value == 5 || typeDropdown.value == 6 || typeDropdown.value == 7)//planes
        {
            xSubDivisionSlider.minValue = 0;
            xSubDivisionSlider.maxValue = 10;
            zSubDivisionSlider.minValue = 0;
            zSubDivisionSlider.maxValue = 10;
        }








    }
    public void TypeDropdownUpdated()
    {








        if (typeDropdown.value == 0)//Cube
        {
            radiusText.text = "Width";
            radiusSubDivText.text = "Width Subdiv";

            subDivisionSlider.transform.parent.gameObject.SetActive(false);
            xSubDivisionSlider.transform.parent.gameObject.SetActive(true);
            ySubDivisionSlider.transform.parent.gameObject.SetActive(true);
            zSubDivisionSlider.transform.parent.gameObject.SetActive(true);



        }
        else if (typeDropdown.value == 1)//Sphere
        {
 
            subDivisionSlider.transform.parent.gameObject.SetActive(true);
            xSubDivisionSlider.transform.parent.gameObject.SetActive(false);
            ySubDivisionSlider.transform.parent.gameObject.SetActive(false);
            zSubDivisionSlider.transform.parent.gameObject.SetActive(false);


            subDivisionSlider.maxValue = 5;
            if (subDivisionSlider.value > 5) subDivisionSlider.value = 5;
        }
        else if (typeDropdown.value == 2)//pipe,
        {
            radiusText.text = "Radius";
            radiusSubDivText.text = "Radius Subdiv";

            subDivisionSlider.transform.parent.gameObject.SetActive(true);
            xSubDivisionSlider.transform.parent.gameObject.SetActive(false);
            ySubDivisionSlider.transform.parent.gameObject.SetActive(false);
            zSubDivisionSlider.transform.parent.gameObject.SetActive(false);
        }
        else if (typeDropdown.value == 3 || typeDropdown.value == 4)//cone,  Cylinder
        {
            radiusText.text = "Radius";
            radiusSubDivText.text = "Radius Subdiv";

            subDivisionSlider.transform.parent.gameObject.SetActive(true);
            xSubDivisionSlider.transform.parent.gameObject.SetActive(false);
            ySubDivisionSlider.transform.parent.gameObject.SetActive(true);
            zSubDivisionSlider.transform.parent.gameObject.SetActive(false);
        }
        else if (typeDropdown.value == 5 || typeDropdown.value == 6 || typeDropdown.value == 7)//planes
        {
            radiusText.text = "Width";
            radiusSubDivText.text = "Width Subdiv";

            subDivisionSlider.transform.parent.gameObject.SetActive(false);
            xSubDivisionSlider.transform.parent.gameObject.SetActive(true);
            ySubDivisionSlider.transform.parent.gameObject.SetActive(false);
            zSubDivisionSlider.transform.parent.gameObject.SetActive(true);
        }
        UpdateSubDSlidersMinMax();
        UpdateModel();
    }




    public void UpdateModel()
    {
        if (currentModel != null)
        {
            SelectionManager.Instance.RemoveModelFromSelection(currentModel);
            currentModel.DeleteModel();
            currentModel = null;
        }
        ProBuilderMesh mesh = null;
        currentShapeType = (ModelType)typeDropdown.value;

        mesh = CreateModelByType(currentShapeType);
        currentModel = mesh.gameObject.AddComponent<ModelData>();
        currentModel.SetupModel(mesh);
        currentModel.SetPosition(new Vector3(0, 1, 2));
        SizeSliderUpdated();
        SelectionManager.Instance.SelectModel(currentModel);
    }


    public ProBuilderMesh CreateModelByType(ModelType type)
    {
        ProBuilderMesh mesh = null;
        if (type == ModelType.Cube)
        {
            mesh = CreateSubdividedCube(Mathf.RoundToInt(xSubDivisionSlider.value), Mathf.RoundToInt(ySubDivisionSlider.value), Mathf.RoundToInt(zSubDivisionSlider.value));
        }
        else if (type == ModelType.Sphere) mesh = ShapeGenerator.GenerateIcosahedron(PivotLocation.Center, 1, Mathf.RoundToInt(subDivisionSlider.value));
        else if (type == ModelType.PlaneFlat) mesh = ShapeGenerator.GeneratePlane(PivotLocation.Center, 1, 1, Mathf.RoundToInt(xSubDivisionSlider.value), Mathf.RoundToInt(zSubDivisionSlider.value), Axis.Up);
        else if (type == ModelType.Cone) mesh = ShapeGenerator.GenerateCone(PivotLocation.Center, xSizeSlider.value, ySizeSlider.value, Mathf.RoundToInt(subDivisionSlider.value));
        else if (type == ModelType.Pipe) mesh = ShapeGenerator.GeneratePipe(PivotLocation.Center, xSizeSlider.value, ySizeSlider.value, zSizeSlider.value, Mathf.RoundToInt(subDivisionSlider.value), Mathf.RoundToInt(ySubDivisionSlider.value));
        else if (type == ModelType.Cylinder) mesh = ShapeGenerator.GenerateCylinder(PivotLocation.Center, Mathf.RoundToInt(subDivisionSlider.value), xSizeSlider.value, ySizeSlider.value, Mathf.RoundToInt(ySubDivisionSlider.value));
        else if (type == ModelType.PlaneDome)
        {
            mesh = GenerateDomePlane(1, 1, Mathf.RoundToInt(xSubDivisionSlider.value), Mathf.RoundToInt(zSubDivisionSlider.value), 1); 
        }
        else if (type == ModelType.PlaneWavy)
        {
            mesh = GenerateWavyPlane(1, 1, Mathf.RoundToInt(xSubDivisionSlider.value), Mathf.RoundToInt(zSubDivisionSlider.value), 1);
        }
        return mesh;
    }
    public static ProBuilderMesh CreateSubdividedCube(int xSub, int ySub, int zSub)
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





    public void SizeSliderUpdated()
    {
        if (currentModel != null)
        {
            Vector3 newScale = Vector3.one * sizeSlider.value;
            if (!hasUniformSize)
            {
                newScale = new Vector3(xSizeSlider.value, ySizeSlider.value, zSizeSlider.value);
            }

            currentModel.transform.localScale = newScale;
        }
    }

    public void SubDSliderUpdated()
    {
        if (currentModel != null)
        {
            ProBuilderMesh mesh = CreateModelByType(currentShapeType);
            mesh.transform.position = currentModel.transform.position;
            currentModel.UpdateMesh(mesh);
            Destroy(mesh.gameObject);
        }
    }


}