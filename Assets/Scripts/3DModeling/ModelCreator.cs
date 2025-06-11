using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.UI;
using UnityEngine.UIElements;


public class ModelCreator : MonoBehaviour
{
    public ModelData currentModel;
    private ShapeType currentShapeType;
    public UnityEngine.UI.Slider sizeSlider;
    public UnityEngine.UI.Slider subDivisionSlider;
    public void FinalizeLastModel()
    {
        if (currentModel != null)
        {
            SelectionManager.Instance.RemoveModelFromSelection(currentModel);
            currentModel.FinalizeEditModel();
        }
    }
    public void CreateModel(string type)
    {
        //FinalizeLastModel();
        if (currentModel != null)
        {
            SelectionManager.Instance.RemoveModelFromSelection(currentModel);
            currentModel.DeleteModel();
            currentModel = null;
        }

        ProBuilderMesh mesh = null;
        if (Enum.TryParse(type, out ShapeType shapeType))
        {
            currentShapeType = shapeType;
        }
        else
        {
            Debug.LogError("Incorrect Shape Type");
            return;
        }

        mesh = CreateModelByType(currentShapeType);


        currentModel = mesh.gameObject.AddComponent<ModelData>();

        currentModel.SetupModel(mesh);
        currentModel.SetPosition(new Vector3(0, 1, 2));
        SizeSliderUpdated();
        SelectionManager.Instance.SelectModel(currentModel);
    }
    public ProBuilderMesh CreateModelByType(ShapeType type)
    {
        ProBuilderMesh mesh = null;
        if (currentShapeType == ShapeType.Cube) mesh = GenerateSubdividedCube(1, Mathf.RoundToInt(subDivisionSlider.value),new Vector3(1,1,1));
        else if (currentShapeType == ShapeType.Sphere) mesh = ShapeGenerator.GenerateIcosahedron(PivotLocation.Center, 1, Mathf.RoundToInt(subDivisionSlider.value));
        else if (currentShapeType == ShapeType.Plane) mesh = ShapeGenerator.GeneratePlane(PivotLocation.Center, 1, 1, Mathf.RoundToInt(subDivisionSlider.value), Mathf.RoundToInt(subDivisionSlider.value), Axis.Up);
        else if (currentShapeType == ShapeType.Cone) mesh = ShapeGenerator.GenerateCone(PivotLocation.Center, 1 ,1 , Mathf.RoundToInt(subDivisionSlider.value));

        return mesh;
    }


    public void SizeSliderUpdated()
    {
        currentModel.SetScale(Vector3.one * sizeSlider.value);
    }

    public void SubDSliderUpdated()
    {
        ProBuilderMesh mesh = CreateModelByType(currentShapeType);
        mesh.transform.position = currentModel.transform.position;
        currentModel.UpdateMesh(mesh);
        Destroy(mesh.gameObject);

    }
    public ProBuilderMesh GenerateSubdividedCube(float size, int subdivisions, Vector3 position)
    {
        float halfSize = size / 2f;
        int resolution = Mathf.Max(1, subdivisions);
        List<Vector3> vertices = new List<Vector3>();
        List<Face> faces = new List<Face>();

        // Helper to create triangle faces instead of quads
        void CreateFace(Vector3 origin, Vector3 axis1, Vector3 axis2, bool invert = false)
        {
            int baseIndex = vertices.Count;

            // Add grid of vertices
            for (int y = 0; y <= resolution; y++)
            {
                for (int x = 0; x <= resolution; x++)
                {
                    Vector3 point = origin + (axis1 * x / resolution) + (axis2 * y / resolution);
                    vertices.Add(point);
                }
            }

            // Add triangles
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    int i0 = baseIndex + y * (resolution + 1) + x;
                    int i1 = i0 + 1;
                    int i2 = i0 + (resolution + 1);
                    int i3 = i2 + 1;

                    if (invert)
                    {
                        faces.Add(new Face(new int[] { i0, i2, i1 }));
                        faces.Add(new Face(new int[] { i1, i2, i3 }));
                    }
                    else
                    {
                        faces.Add(new Face(new int[] { i0, i1, i2 }));
                        faces.Add(new Face(new int[] { i1, i3, i2 }));
                    }
                }
            }
        }

        // Front (+Z)
        CreateFace(new Vector3(-halfSize, -halfSize, halfSize), Vector3.right * size, Vector3.up * size, false);

        // Back (-Z)// Front (+Z)
        CreateFace(new Vector3(-halfSize, -halfSize, halfSize), Vector3.right * size, Vector3.up * size, false);

        // Back (-Z)
        CreateFace(new Vector3(halfSize, -halfSize, -halfSize), -Vector3.right * size, Vector3.up * size, false);

        // Left (-X)
        CreateFace(new Vector3(-halfSize, -halfSize, -halfSize), Vector3.forward * size, Vector3.up * size, false);

        // Right (+X)
        CreateFace(new Vector3(halfSize, -halfSize, halfSize), -Vector3.forward * size, Vector3.up * size, false);

        // Top (+Y)
        CreateFace(new Vector3(-halfSize, halfSize, halfSize), Vector3.right * size, -Vector3.forward * size, false);

        // Bottom (-Y)
        CreateFace(new Vector3(-halfSize, -halfSize, -halfSize), Vector3.right * size, Vector3.forward * size, false);

        // Build the mesh
        ProBuilderMesh mesh = ProBuilderMesh.Create(vertices, faces);
        mesh.transform.position = position;

        mesh.ToMesh();
        mesh.Refresh();
        return mesh;
    }

}
