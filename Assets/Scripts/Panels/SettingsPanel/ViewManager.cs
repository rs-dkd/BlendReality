using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


/// <summary>
/// Utility to convert from meters to imperial and back
/// </summary>
public static class MetricConverter
{
    private const float InchesPerMeter = 39.3701f;
    private const int InchesPerFoot = 12;

    /// <summary>
    /// Takes a float in meters and converts to a imperial string (1' 5")
    /// </summary>
    public static string ToFeetAndInches(float meters)
    {
        // Handle the zero case first
        if (Mathf.Approximately(meters, 0f))
        {
            return "0\"";
        }

        // Check for negative and store the prefix
        string prefix = meters < 0 ? "-" : "";

        // Use the absolute value for all calculations
        float absMeters = Mathf.Abs(meters);
        float totalInches = absMeters * InchesPerMeter;

        if (totalInches < InchesPerFoot)
        {
            // Return with prefix, e.g., "-4\""
            return $"{prefix}{Mathf.RoundToInt(totalInches)}\"";
        }
        else
        {
            int feet = Mathf.FloorToInt(totalInches / InchesPerFoot);
            int inches = Mathf.RoundToInt(totalInches % InchesPerFoot);

            //rounding up (e.g., 11.9" becomes 12")
            if (inches == InchesPerFoot)
            {
                feet++;
                inches = 0;
            }

            // Return with prefix, e.g., "-5' 2\""
            return $"{prefix}{feet}' {inches}\"";
        }
    }

    /// <summary>
    /// Takes a imperial string and converts it to float in meters
    /// </summary>
    public static float ToMeters(string imperial)
    {
        if (string.IsNullOrWhiteSpace(imperial))
        {
            return 0;
        }

        float feet = 0;
        float inches = 0;

        var match = Regex.Match(imperial, @"^\s*(?:(-?[0-9\.]+)\s*'\s*)?(?:(-?[0-9\.]+)\s*""\s*)?$");

        if (match.Groups[1].Success)
        {
            // float.TryParse will correctly parse "-5" or "-1.5"
            float.TryParse(match.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out feet);
        }

        if (match.Groups[2].Success)
        {
            float.TryParse(match.Groups[2].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out inches);
        }

        float totalInches = (feet * InchesPerFoot) + inches;

        return totalInches / InchesPerMeter;
    }
}


/// <summary>
/// Enum for Metric and Imperial
/// </summary>
public enum GridUnitSystem
{
    Metric,
    Imperial
}
public enum ImperialDisplayMode
{
    DecimalFeet,
    Adaptive
}
/// <summary>
/// Enum for shader types
/// </summary>
public enum ShadingType
{
    Standard, Wireframe, Unlit, Clay, HiddenLine
}

[System.Serializable]
public class ShadingChangedEvent : UnityEvent { }
public class ControlPointSizeChangedEvent : UnityEvent<float> { }
public class UnitSystemChangedEvent : UnityEvent<GridUnitSystem> { }
/// <summary>
/// Settings manager
/// Handles - shading, control point size, grid system, light
/// </summary>
public class ViewManager : MonoBehaviour
{
    //Singleton Pattern
    public static ViewManager Instance { get; private set; }

    /// <summary>
    /// Setup the listeners and backface
    /// </summary>
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        BackfaceUpdated();

        shadingToggleGroup.OnToggleGroupChanged.AddListener(OnShadingToggleSelected);
        unitSystemToggleGroup.OnToggleGroupChanged.AddListener(OnUnitSystemToggleSelected);
        controlPointSizeSlider.OnSliderValueChangedEvent.AddListener(ControlPointSizeUpdated);
    }


    //Events
    public ShadingChangedEvent OnShadingChanged = new ShadingChangedEvent();
    public ControlPointSizeChangedEvent OnControlPointSizeChanged = new ControlPointSizeChangedEvent();
    public UnitSystemChangedEvent OnUnitSystemSizeChanged = new UnitSystemChangedEvent();

    [Tooltip("Hidden Line Material")]
    [SerializeField] private Material hiddenLineMaterial;
    [Tooltip("Standard Material")]
    [SerializeField] private Material standardMaterial;
    [Tooltip("Wireframe Material")]
    [SerializeField] private Material wireframeMaterial;
    [Tooltip("Unlit Material")]
    [SerializeField] private Material unlitMaterial;
    [Tooltip("Clay Material")]
    [SerializeField] private Material clayMaterial;

    [Tooltip("Selected Hidden Line Material")]
    [SerializeField] private Material selectedHiddenLineMaterial;
    [Tooltip("Selected Standard Material")]
    [SerializeField] private Material selectedStandardMaterial;
    [Tooltip("Selected Wireframe Material")]
    [SerializeField] private Material selectedWireframeMaterial;
    [Tooltip("Selected Unlit Material")]
    [SerializeField] private Material selectedUnlitMaterial;
    [Tooltip("Selected Clay Material")]
    [SerializeField] private Material selectedClayMaterial;

    [Tooltip("Control Point Selected Material")]
    [SerializeField] private Material controlPointSelected;
    [Tooltip("Control Point DeSelected Material")]
    [SerializeField] private Material controlPointDeselected;

    [Tooltip("Shaders Toggle Group")]
    [SerializeField] private ToggleGroupUI shadingToggleGroup;
    [Tooltip("Unit System Toggle Group")]
    [SerializeField] private ToggleGroupUI unitSystemToggleGroup;


    [Tooltip("Grid Material")]
    [SerializeField] private Material gridMaterial;


    [Tooltip("Control point slider")]
    [SerializeField] private SliderUI controlPointSizeSlider;
    [Tooltip("Control point starting size")]
    [SerializeField] private float controlPointSize = 0.1f;


    [Tooltip("Light GO")]
    [SerializeField] private GameObject lightGO;
    [Tooltip("Grid GO")]
    [SerializeField] private GameObject gridGO;

    [Tooltip("Backface Toggle")]
    [SerializeField] private Toggle backfaceToggle;
    [Tooltip("Main Camera")]
    [SerializeField] private Camera targetCamera;

   



    private ShadingType currentShadingType;
    private bool showBackface = false;
    private GridUnitSystem unitSystem = GridUnitSystem.Metric;
    private ImperialDisplayMode imperialMode = ImperialDisplayMode.Adaptive;

    private static readonly int MajorGridSizeID = Shader.PropertyToID("_MajorGridSize");
    private static readonly int MinorGridSizeID = Shader.PropertyToID("_GridSize");
    private static readonly int GridCenterID = Shader.PropertyToID("_GridCenter");




    /// <summary>
    /// Get Unit System
    /// </summary>
    public GridUnitSystem GetUnitSystem()
    {
        return unitSystem;
    }

    /// <summary>
    /// Toggle dir light
    /// </summary>
    public void ToggleLight()
    {
        lightGO.SetActive(!lightGO.activeSelf);
    }
    /// <summary>
    /// Toggle grid lines
    /// </summary>
    public void ToggleGrid()
    {
        gridGO.SetActive(!gridGO.activeSelf);
    }
    /// <summary>
    /// Update control point size
    /// </summary>
    public void ControlPointSizeUpdated(float value)
    {
        controlPointSize = value;
        OnControlPointSizeChanged.Invoke(controlPointSize);
    }
    /// <summary>
    /// Get the Control point size
    /// </summary>
    public float GetControlPointSize()
    {
        return controlPointSize;
    }
    /// <summary>
    /// Get the Control point selected material
    /// </summary>
    public Material GetSelectedControlPointMaterial()
    {
        return controlPointSelected;
    }
    /// <summary>
    /// Get the Control point deselected material
    /// </summary>
    public Material GetUnselectedControlPointMaterial()
    {
        return controlPointDeselected;
    }
    /// <summary>
    /// Toggle backface for all the materials
    /// </summary>
    public void BackfaceUpdated()
    {
        showBackface = backfaceToggle.isOn;
        int cullValue = showBackface ? (int)UnityEngine.Rendering.CullMode.Off : (int)UnityEngine.Rendering.CullMode.Back;

        float showBackfacesFloat = showBackface ? 1f : 0f;

        if (unlitMaterial) unlitMaterial.SetFloat("_Cull", (float)cullValue);
        if (selectedUnlitMaterial) selectedUnlitMaterial.SetFloat("_Cull", (float)cullValue);



        if (standardMaterial) standardMaterial.SetInt("_Cull", cullValue);
        if (clayMaterial) clayMaterial.SetInt("_Cull", cullValue);
        if (selectedStandardMaterial) selectedStandardMaterial.SetInt("_Cull", cullValue);
        if (selectedClayMaterial) selectedClayMaterial.SetInt("_Cull", cullValue);

        if (hiddenLineMaterial) hiddenLineMaterial.SetFloat("_ShowBackfaces", showBackfacesFloat);
        if (wireframeMaterial) wireframeMaterial.SetFloat("_ShowBackfaces", showBackfacesFloat);
        if (selectedHiddenLineMaterial) selectedHiddenLineMaterial.SetFloat("_ShowBackfaces", showBackfacesFloat);
        if (selectedWireframeMaterial) selectedWireframeMaterial.SetFloat("_ShowBackfaces", showBackfacesFloat);

        OnShadingChanged.Invoke();
    }
    /// <summary>
    /// Get current shader type
    /// </summary>
    public ShadingType GetShadingType()
    {
        return currentShadingType;
    }
    /// <summary>
    /// Inoke the event on shader changed
    /// </summary>
    public void OnShadingToggleSelected(Toggle toggle)
    {
        if (Enum.TryParse(toggle.name, out currentShadingType))
        {
            OnShadingChanged.Invoke();
        }
    }
    /// <summary>
    /// Get current shader material
    /// </summary>
    public Material GetCurrentShading(bool isSelected)
    {
        if (isSelected)
        {
            if (currentShadingType == ShadingType.Wireframe)
            {
                return selectedWireframeMaterial;
            }
            else if (currentShadingType == ShadingType.Clay)
            {
                return selectedClayMaterial;
            }
            else if (currentShadingType == ShadingType.Unlit)
            {
                return selectedUnlitMaterial;
            }
            else if (currentShadingType == ShadingType.HiddenLine)
            {
                return selectedHiddenLineMaterial;
            }
            return selectedStandardMaterial;
        }
        else
        {
            if (currentShadingType == ShadingType.Wireframe)
            {
                return wireframeMaterial;
            }
            else if (currentShadingType == ShadingType.Clay)
            {
                return clayMaterial;
            }
            else if (currentShadingType == ShadingType.Unlit)
            {
                return unlitMaterial;
            }
            else if (currentShadingType == ShadingType.HiddenLine)
            {
                return hiddenLineMaterial;
            }
            return standardMaterial;
        }

    }
    /// <summary>
    /// Inoke event on unit system toggle changed
    /// </summary>
    public void OnUnitSystemToggleSelected(Toggle toggle)
    {
        if (Enum.TryParse(toggle.name, out unitSystem))
        {
            OnUnitSystemSizeChanged.Invoke(unitSystem);
        }
    }

    /// <summary>
    /// Update the center of the grid to fade it out
    /// </summary>
    void Update()
    {
        Plane gridPlane = new Plane(transform.up, transform.position);
        Vector3 pointOnPlane = gridPlane.ClosestPointOnPlane(targetCamera.transform.position);
        float height = Vector3.Distance(targetCamera.transform.position, pointOnPlane);
        height = Mathf.Max(0.1f, height);

        float baseSpacing;
        float majorGridSpacing;

        if (unitSystem == GridUnitSystem.Imperial)
        {
            baseSpacing = 0.0254f;
            majorGridSpacing = 0.3048f;
        }
        else
        {
            baseSpacing = 0.1f;
            majorGridSpacing = 1;
        }

        gridMaterial.SetFloat(MinorGridSizeID, baseSpacing);
        gridMaterial.SetFloat(MajorGridSizeID, majorGridSpacing);

        Vector3 gridCenter = new Vector3(
        Mathf.Round(pointOnPlane.x / majorGridSpacing) * majorGridSpacing,
        transform.position.y,
        Mathf.Round(pointOnPlane.z / majorGridSpacing) * majorGridSpacing
);
        gridMaterial.SetVector(GridCenterID, gridCenter);

    }
}
