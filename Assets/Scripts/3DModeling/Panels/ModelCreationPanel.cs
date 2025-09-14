//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.ProBuilder;

//public enum AllowedShapeType
//{
//    Cube,
//    Sphere,
//    Plane,
//    Cone,
//    Cylinder
//}
//public class ModelCreationPanel : BasePanel
//{
//    public ModelData currentModel;
//    private ShapeType currentShapeType;
//    public UnityEngine.UI.Slider sizeSlider;
//    public UnityEngine.UI.Slider subDivisionSlider;
//    public UnityEngine.UI.Slider depthSubDivisionSlider;
//    public UnityEngine.UI.Slider heightSubDivisionSlider;
//    public UnityEngine.UI.Slider widthSubDivisionSlider;
//    public TMPro.TMP_Dropdown typeDropdown;
    

//    public void Awake()
//    {
//        typeDropdown.options = new List<TMPro.TMP_Dropdown.OptionData>();

//        foreach (AllowedShapeType shape in System.Enum.GetValues(typeof(AllowedShapeType)))
//        {
//            typeDropdown.options.Add(new TMPro.TMP_Dropdown.OptionData(shape.ToString()));
//        }

//        typeDropdown.value = 0;
//        typeDropdown.RefreshShownValue();
//    }

//    public void ModelTypeUpdated()
//    {
//        if (Enum.TryParse(((AllowedShapeType)typeDropdown.value).ToString(), out ShapeType shapeType))
//        {
//            currentShapeType = shapeType;
//        }
//        else
//        {
//            Debug.LogError("Incorrect Shape Type");
//            return;
//        }


//        subDivisionSlider.transform.parent.gameObject.SetActive(true);
//        widthSubDivisionSlider.transform.parent.gameObject.SetActive(true);
//        heightSubDivisionSlider.transform.parent.gameObject.SetActive(true);
//        depthSubDivisionSlider.transform.parent.gameObject.SetActive(true);

//        if (currentShapeType == ShapeType.Cube)
//        {
//            subDivisionSlider.transform.parent.gameObject.SetActive(false);
//        }
//        else if (currentShapeType == ShapeType.Plane)
//        {
//            subDivisionSlider.transform.parent.gameObject.SetActive(false);
//            heightSubDivisionSlider.transform.parent.gameObject.SetActive(false);
//        }
//        else if (currentShapeType == ShapeType.Sphere || currentShapeType == ShapeType.Cone || currentShapeType == ShapeType.Cylinder)
//        {
//            widthSubDivisionSlider.transform.parent.gameObject.SetActive(false);
//            heightSubDivisionSlider.transform.parent.gameObject.SetActive(false);
//            depthSubDivisionSlider.transform.parent.gameObject.SetActive(false);
//        }
//    }
//    public void FinalizeLastModel()
//    {
//        if (currentModel != null)
//        {
//            SelectionManager.Instance.RemoveModelFromSelection(currentModel);
//            currentModel.FinalizeEditModel();
//        }
//    }
//    public void CreateModel()
//    {
//        //FinalizeLastModel();
//        if (currentModel != null)
//        {
//            SelectionManager.Instance.RemoveModelFromSelection(currentModel);
//            currentModel.DeleteModel();
//            currentModel = null;
//        }

//        ProBuilderMesh mesh = null;
//        if (Enum.TryParse(((AllowedShapeType)typeDropdown.value).ToString(), out ShapeType shapeType))
//        {
//            currentShapeType = shapeType;
//        }
//        else
//        {
//            Debug.LogError("Incorrect Shape Type");
//            return;
//        }

//        mesh = CreateModelByType(currentShapeType);


//        currentModel = mesh.gameObject.AddComponent<ModelData>();

//        currentModel.SetupModel(mesh);
//        currentModel.SetPosition(new Vector3(0, 1, 2));
//        SizeSliderUpdated();
//        SelectionManager.Instance.SelectModel(currentModel);
//    }
//    public ProBuilderMesh CreateModelByType(ShapeType type)
//    {
//        ProBuilderMesh mesh = null;
//        if (currentShapeType == ShapeType.Cube) mesh = GenerateSubdividedCube(1, Mathf.RoundToInt(widthSubDivisionSlider.value), Mathf.RoundToInt(heightSubDivisionSlider.value), Mathf.RoundToInt(depthSubDivisionSlider.value), new Vector3(1, 1, 1));
//        else if (currentShapeType == ShapeType.Sphere) mesh = ShapeGenerator.GenerateIcosahedron(PivotLocation.Center, 1, Mathf.RoundToInt(subDivisionSlider.value));
//        else if (currentShapeType == ShapeType.Plane) mesh = ShapeGenerator.GeneratePlane(PivotLocation.Center, 1, 1, Mathf.RoundToInt(widthSubDivisionSlider.value), Mathf.RoundToInt(depthSubDivisionSlider.value), Axis.Up);
//        else if (currentShapeType == ShapeType.Cone) mesh = ShapeGenerator.GenerateCone(PivotLocation.Center, 1, 1, Mathf.RoundToInt(subDivisionSlider.value));
//        else if (currentShapeType == ShapeType.Cylinder) mesh = ShapeGenerator.GenerateCylinder(PivotLocation.Center, Mathf.RoundToInt(subDivisionSlider.value), 1,1, Mathf.RoundToInt(subDivisionSlider.value));

//        return mesh;
//    }


//    public void SizeSliderUpdated()
//    {
//        if (currentModel == null) return;
//        currentModel.SetScale(Vector3.one * sizeSlider.value);
//    }

//    public void SubDSliderUpdated()
//    {
//        if (currentModel == null) return;
//        ProBuilderMesh mesh = CreateModelByType(currentShapeType);
//        mesh.transform.position = currentModel.transform.position;
//        currentModel.UpdateMesh(mesh);
//        Destroy(mesh.gameObject);

//    }
//    public ProBuilderMesh GenerateSubdividedCube(float size, int subdivisionsWidth, int subdivisionsHeight, int subdivisionsDepth, Vector3 position)
//    {
//        if (subdivisionsWidth < 1) subdivisionsWidth = 1;
//        if (subdivisionsHeight < 1) subdivisionsHeight = 1;
//        if (subdivisionsDepth < 1) subdivisionsDepth = 1;


//        float halfSize = size / 2f;
//        List<Vector3> vertices = new List<Vector3>();
//        List<Face> faces = new List<Face>();

//        void CreateFace(Vector3 origin, Vector3 axis1, Vector3 axis2, int steps1, int steps2, bool invert = false)
//        {
//            int baseIndex = vertices.Count;

//            // Create grid of vertices
//            for (int y = 0; y <= steps2; y++)
//            {
//                for (int x = 0; x <= steps1; x++)
//                {
//                    Vector3 point = origin + (axis1 * x / steps1) + (axis2 * y / steps2);
//                    vertices.Add(point);
//                }
//            }

//            // Create triangles
//            for (int y = 0; y < steps2; y++)
//            {
//                for (int x = 0; x < steps1; x++)
//                {
//                    int i0 = baseIndex + y * (steps1 + 1) + x;
//                    int i1 = i0 + 1;
//                    int i2 = i0 + (steps1 + 1);
//                    int i3 = i2 + 1;

//                    if (invert)
//                    {
//                        faces.Add(new Face(new int[] { i0, i2, i1 }));
//                        faces.Add(new Face(new int[] { i1, i2, i3 }));
//                    }
//                    else
//                    {
//                        faces.Add(new Face(new int[] { i0, i1, i2 }));
//                        faces.Add(new Face(new int[] { i1, i3, i2 }));
//                    }
//                }
//            }
//        }

//        float s = size;

//        // Front (+Z)
//        CreateFace(new Vector3(-halfSize, -halfSize, halfSize), Vector3.right * s, Vector3.up * s, subdivisionsWidth, subdivisionsHeight, false);
//        // Back (-Z)
//        CreateFace(new Vector3(halfSize, -halfSize, -halfSize), -Vector3.right * s, Vector3.up * s, subdivisionsWidth, subdivisionsHeight, false);
//        // Left (-X)
//        CreateFace(new Vector3(-halfSize, -halfSize, -halfSize), Vector3.forward * s, Vector3.up * s, subdivisionsDepth, subdivisionsHeight, false);
//        // Right (+X)
//        CreateFace(new Vector3(halfSize, -halfSize, halfSize), -Vector3.forward * s, Vector3.up * s, subdivisionsDepth, subdivisionsHeight, false);
//        // Top (+Y)
//        CreateFace(new Vector3(-halfSize, halfSize, halfSize), Vector3.right * s, -Vector3.forward * s, subdivisionsWidth, subdivisionsDepth, false);
//        // Bottom (-Y)
//        CreateFace(new Vector3(-halfSize, -halfSize, -halfSize), Vector3.right * s, Vector3.forward * s, subdivisionsWidth, subdivisionsDepth, false);

//        // Create the mesh
//        ProBuilderMesh mesh = ProBuilderMesh.Create(vertices, faces);
//        mesh.transform.position = position;

//        mesh.ToMesh();
//        mesh.Refresh();
//        return mesh;
//    }

//}