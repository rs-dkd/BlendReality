using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.UI;
using TMPro;

public class EnhancedModelCreator : MonoBehaviour
{
    [Header("Current Model")]
    public ModelData currentModel;
    private ShapeType currentShapeType;

    [Header("UI Controls")]
    public Slider sizeSlider;
    public Slider subDivisionSlider;

    [Header("Imported Objects UI")]
    public TMP_Dropdown importedObjectsDropdown;
    public Button createImportedObjectButton;
    public TMP_Text importedObjectsCountText;

    [Header("Bezier Surface System")]
    public BezierSurfaceManager bezierManager;
    private BezierSurface currentBezierSurface;

    [Header("PNS Integration")]
    public PNSIntegration pnsIntegration;
    public Button convertToPNSButton;
    public Toggle degreeRaiseToggle;
    public TMP_Dropdown exportFormatDropdown;

    void Start()
    {
        SetupImportedObjectsUI();
        SetupPNSUI();
        RefreshImportedObjectsList();
    }

    private void SetupPNSUI()
    {
        // Initialize PNS integration if not assigned
        if (pnsIntegration == null)
        {
            pnsIntegration = FindObjectOfType<PNSIntegration>();
            if (pnsIntegration == null)
            {
                GameObject pnsObject = new GameObject("PNS Integration");
                pnsIntegration = pnsObject.AddComponent<PNSIntegration>();
                Debug.Log("Created PNSIntegration component");
            }
        }

        // Setup PNS UI controls
        if (convertToPNSButton != null)
        {
            convertToPNSButton.onClick.AddListener(ConvertCurrentModelToPNS);
        }

        if (degreeRaiseToggle != null)
        {
            degreeRaiseToggle.onValueChanged.AddListener(OnDegreeRaiseChanged);
            degreeRaiseToggle.isOn = pnsIntegration.degreeRaise;
        }

        if (exportFormatDropdown != null)
        {
            exportFormatDropdown.ClearOptions();
            exportFormatDropdown.AddOptions(new List<string> { "bv", "igs", "step" });
            exportFormatDropdown.onValueChanged.AddListener(OnExportFormatChanged);
        }
    }

    private void SetupImportedObjectsUI()
    {
        if (createImportedObjectButton != null)
        {
            createImportedObjectButton.onClick.AddListener(CreateSelectedImportedObject);
        }

        if (importedObjectsDropdown != null)
        {
            importedObjectsDropdown.onValueChanged.AddListener(OnImportedObjectDropdownChanged);
        }
    }

    public void RefreshImportedObjectsList()
    {
        Debug.Log("RefreshImportedObjectsList called");

        if (OBJImportHandler.Instance == null)
        {
            Debug.Log("OBJImportHandler.Instance is null");
            UpdateImportedObjectsUI(new List<string>(), 0);
            return;
        }

        List<string> importedNames = OBJImportHandler.Instance.GetImportedObjectNames();
        int count = OBJImportHandler.Instance.GetImportedObjectCount();

        Debug.Log($"Found {count} imported objects: {string.Join(", ", importedNames)}");

        UpdateImportedObjectsUI(importedNames, count);
    }

    private void UpdateImportedObjectsUI(List<string> objectNames, int count)
    {
        if (importedObjectsDropdown != null)
        {
            importedObjectsDropdown.ClearOptions();

            if (objectNames.Count > 0)
            {
                List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
                foreach (string name in objectNames)
                {
                    options.Add(new TMP_Dropdown.OptionData(name));
                }
                importedObjectsDropdown.AddOptions(options);
                importedObjectsDropdown.interactable = true;
            }
            else
            {
                importedObjectsDropdown.AddOptions(new List<string> { "No imported objects" });
                importedObjectsDropdown.interactable = false;
            }
        }

        if (createImportedObjectButton != null)
        {
            createImportedObjectButton.interactable = count > 0;
        }

        if (importedObjectsCountText != null)
        {
            importedObjectsCountText.text = $"Imported Objects: {count}";
        }
    }

    private void OnImportedObjectDropdownChanged(int index)
    {
        Debug.Log($"Selected imported object index: {index}");
    }

    public void CreateSelectedImportedObject()
    {
        if (OBJImportHandler.Instance == null)
        {
            Debug.LogError("OBJImportHandler not found!");
            return;
        }

        if (importedObjectsDropdown == null || importedObjectsDropdown.options.Count == 0)
        {
            Debug.LogError("No imported objects available!");
            return;
        }
        FinalizeLastModel();

        // Create selected imported obj
        int selectedIndex = importedObjectsDropdown.value;
        ModelData newModel = OBJImportHandler.Instance.CreateImportedObject(selectedIndex);

        if (newModel != null)
        {
            currentModel = newModel;
            if (sizeSlider != null)
            {
                SizeSliderUpdated();
            }
            SelectionManager.Instance.SelectModel(currentModel);
            UpdatePNSButtonState();

            Debug.Log($"Created imported object: {importedObjectsDropdown.options[selectedIndex].text}");
        }
        else
        {
            Debug.LogError("Failed to create imported object!");
        }
    }

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
        //Check if Bezier request
        if (type.StartsWith("Bezier"))
        {
            string bezierType = type.Replace("Bezier", "").ToLower();
            if (string.IsNullOrEmpty(bezierType)) bezierType = "flat";
            CreateBezierSurface(bezierType);
            return;
        }
        if (currentModel != null)
        {
            SelectionManager.Instance.RemoveModelFromSelection(currentModel);
            currentModel.DeleteModel();
            currentModel = null;
        }

        ClearCurrentBezierSurface();

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

        if (sizeSlider != null)
        {
            SizeSliderUpdated();
        }

        SelectionManager.Instance.SelectModel(currentModel);
        UpdatePNSButtonState();
    }

    public void CreateBezierSurface(string surfaceType = "flat")
    {
        //Init Bezier manager
        if (bezierManager == null)
        {
            bezierManager = FindObjectOfType<BezierSurfaceManager>();
            if (bezierManager == null)
            {
                Debug.LogError("BezierSurfaceManager not found! Please add one to the scene.");
                return;
            }
        }

        if (currentModel != null)
        {
            SelectionManager.Instance.RemoveModelFromSelection(currentModel);
            currentModel.DeleteModel();
            currentModel = null;
        }

        //Clear existing Bezier surfaces
        ClearCurrentBezierSurface();
        //Create Bezier surface
        currentBezierSurface = bezierManager.CreateNewSurface(surfaceType);
        PositionBezierSurface(currentBezierSurface, new Vector3(0, 1, 2));
        UpdatePNSButtonState();

        Debug.Log($"Created Bezier {surfaceType} surface");
    }

    private void ClearCurrentBezierSurface()
    {
        if (currentBezierSurface != null && bezierManager != null)
        {
            bezierManager.RemoveSurface(currentBezierSurface.surfaceID);
            currentBezierSurface = null;
        }
    }

    private void PositionBezierSurface(BezierSurface surface, Vector3 offset)
    {
        //Match control points to offset
        int uCount = surface.controlPoints.GetLength(0);
        int vCount = surface.controlPoints.GetLength(1);

        for (int u = 0; u < uCount; u++)
        {
            for (int v = 0; v < vCount; v++)
            {
                surface.controlPoints[u, v] += offset;
            }
        }
        //Set surface to dirty (so it regenerates)
        surface.isDirty = true;

        //Update all visual control point positions
        if (bezierManager != null)
        {
            //Get control point objects and update their pos
            var controlPointObjects = bezierManager.GetControlPointObjects(surface.surfaceID);
            if (controlPointObjects != null)
            {
                int index = 0;
                for (int u = 0; u < uCount; u++)
                {
                    for (int v = 0; v < vCount; v++)
                    {
                        if (index < controlPointObjects.Count)
                        {
                            controlPointObjects[index].transform.position = surface.controlPoints[u, v];
                        }
                        index++;
                    }
                }
            }

            //Start surface mesh regeneration
            bezierManager.OnControlPointMoved(surface.surfaceID, 0, 0, surface.controlPoints[0, 0]);
        }
    }

    public ProBuilderMesh CreateModelByType(ShapeType type)
    {
        ProBuilderMesh mesh = null;

        // Create quad-based meshes where possible for better PNS results
        if (currentShapeType == ShapeType.Cube)
        {
            mesh = ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(1, 1, 1));
        }
        else if (currentShapeType == ShapeType.Sphere)
        {
            mesh = ShapeGenerator.GenerateIcosahedron(PivotLocation.Center, 1, Mathf.RoundToInt(subDivisionSlider.value));
        }
        else if (currentShapeType == ShapeType.Plane)
        {
            // Use quad-based plane for better PNS compatibility
            mesh = ShapeGenerator.GeneratePlane(PivotLocation.Center, 1, 1,
                Mathf.RoundToInt(subDivisionSlider.value), Mathf.RoundToInt(subDivisionSlider.value), Axis.Up);

            // Convert triangulated faces to quads where possible
            ConvertToQuads(mesh);
        }
        else if (currentShapeType == ShapeType.Cone)
        {
            mesh = ShapeGenerator.GenerateCone(PivotLocation.Center, 1, 1, Mathf.RoundToInt(subDivisionSlider.value));
        }

        return mesh;
    }

    private void ConvertToQuads(ProBuilderMesh mesh)
    {
        try
        {
            mesh.ToMesh();
            mesh.Refresh();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Could not convert to quads: {e.Message}");
        }
    }

    public void SizeSliderUpdated()
    {
        if (currentModel != null && sizeSlider != null)
        {
            currentModel.SetScale(Vector3.one * sizeSlider.value);
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

    // PNS Integration Methods
    public void ConvertCurrentModelToPNS()
    {
        if (pnsIntegration == null)
        {
            Debug.LogError("PNS Integration not found!");
            return;
        }

        if (currentModel != null)
        {
            ProBuilderMesh proBuilderMesh = currentModel.GetEditModel();
            if (proBuilderMesh != null)
            {
                string filename = $"model_{currentModel.GetInstanceID()}_{System.DateTime.Now.Ticks}";
                pnsIntegration.ConvertProBuilderToPNS(proBuilderMesh, filename);
                Debug.Log($"Converted current model to PNS: {filename}");
            }
            else
            {
                Debug.LogError("No ProBuilder mesh found in current model");
            }
        }
        else if (currentBezierSurface != null)
        {
            string filename = $"bezier_surface_{currentBezierSurface.surfaceID}_{System.DateTime.Now.Ticks}";
            pnsIntegration.ConvertBezierSurfaceToPNS(currentBezierSurface, filename);
            Debug.Log($"Converted current Bezier surface to PNS: {filename}");
        }
        else
        {
            Debug.LogWarning("No model or Bezier surface selected for PNS conversion");
        }
    }

    public void ConvertAllModelsToPNS()
    {
        if (pnsIntegration != null)
        {
            pnsIntegration.ConvertAllModels();
        }
    }

    public void TestPNSWithSimpleQuad()
    {
        if (pnsIntegration != null)
        {
            pnsIntegration.TestPNSWithSimpleQuad();
        }
        else
        {
            Debug.LogError("PNS Integration not found!");
        }
    }

    public void TestPNSWithComplexQuad()
    {
        if (pnsIntegration != null)
        {
            pnsIntegration.TestPNSWithComplexQuadMesh();
        }
        else
        {
            Debug.LogError("PNS Integration not found!");
        }
    }

    public void TestPNSWithInterestingGeometry()
    {
        if (pnsIntegration != null)
        {
            pnsIntegration.TestPNSWithInterestingGeometry();
        }
        else
        {
            Debug.LogError("PNS Integration not found!");
        }
    }

    public void TestPNSWithCylinderSurface()
    {
        if (pnsIntegration != null)
        {
            pnsIntegration.TestPNSWithCylinderSurface();
        }
        else
        {
            Debug.LogError("PNS Integration not found!");
        }
    }

    public void TestPNSWithLowPolySphere()
    {
        if (pnsIntegration != null)
        {
            pnsIntegration.TestPNSWithLowPolySphere();
        }
        else
        {
            Debug.LogError("PNS Integration not found!");
        }
    }

    public void TestPNSWithFacetedTorus()
    {
        if (pnsIntegration != null)
        {
            pnsIntegration.TestPNSWithFacetedTorus();
        }
        else
        {
            Debug.LogError("PNS Integration not found!");
        }
    }

    public void TestPNSWithSharpObject()
    {
        if (pnsIntegration != null)
        {
            pnsIntegration.TestPNSWithSharpObject();
        }
        else
        {
            Debug.LogError("PNS Integration not found!");
        }
    }

    public void DebugCurrentMeshTopology()
    {
        if (pnsIntegration != null && currentModel != null)
        {
            var mesh = currentModel.GetEditModel();
            if (mesh != null)
            {
                pnsIntegration.DebugProBuilderMesh(mesh);
            }
            else
            {
                Debug.LogError("No ProBuilder mesh found in current model");
            }
        }
        else
        {
            Debug.LogError("PNS Integration or current model not found!");
        }
    }

    private void OnDegreeRaiseChanged(bool value)
    {
        if (pnsIntegration != null)
        {
            pnsIntegration.SetDegreeRaise(value);
        }
    }

    private void OnExportFormatChanged(int index)
    {
        if (pnsIntegration != null && exportFormatDropdown != null)
        {
            string[] formats = { "bv", "igs", "step" };
            if (index >= 0 && index < formats.Length)
            {
                pnsIntegration.SetExportFormat(formats[index]);
            }
        }
    }

    private void UpdatePNSButtonState()
    {
        if (convertToPNSButton != null)
        {
            bool canConvert = (currentModel != null) || (currentBezierSurface != null);
            convertToPNSButton.interactable = canConvert;
        }
    }

    public void CreateBezierFlat() => CreateBezierSurface("flat");
    public void CreateBezierDome() => CreateBezierSurface("dome");
    public void CreateBezierWavy() => CreateBezierSurface("wavy");

    // Refresh imported objs
    public void OnImportedObjectsChanged()
    {
        Debug.Log("OnImportedObjectsChanged called");
        RefreshImportedObjectsList();
    }

    [ContextMenu("Debug Model Creator")]
    public void DebugModelCreator()
    {
        Debug.Log("=== MODEL CREATOR DEBUG ===");
        Debug.Log($"importedObjectsDropdown: {(importedObjectsDropdown != null ? "Assigned" : "NULL")}");
        Debug.Log($"createImportedObjectButton: {(createImportedObjectButton != null ? "Assigned" : "NULL")}");
        Debug.Log($"importedObjectsCountText: {(importedObjectsCountText != null ? "Assigned" : "NULL")}");
        Debug.Log($"pnsIntegration: {(pnsIntegration != null ? "Assigned" : "NULL")}");
        Debug.Log($"convertToPNSButton: {(convertToPNSButton != null ? "Assigned" : "NULL")}");

        if (OBJImportHandler.Instance != null)
        {
            Debug.Log($"OBJImportHandler found with {OBJImportHandler.Instance.GetImportedObjectCount()} objects");
        }
        else
        {
            Debug.Log("OBJImportHandler.Instance is NULL");
        }
        Debug.Log("===========================");
    }
}