using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.UI;


public class ModelCreator : MonoBehaviour
{
    public ModelData currentModel;
    private ShapeType currentShapeType;
    public Slider sizeSlider;
    public Slider subDivisionSlider;
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
        FinalizeLastModel();


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
        currentModel.SetPosition(new Vector3(0, 1, -1));
        SizeSliderUpdated();
        SelectionManager.Instance.SelectModel(currentModel);
    }
    public ProBuilderMesh CreateModelByType(ShapeType type)
    {
        ProBuilderMesh mesh = null;
        if (currentShapeType == ShapeType.Cube) mesh = ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(1,1,1));
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


}
