using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;



public static class MetricConverter
{
    private const float InchesPerMeter = 39.3701f;
    private const int InchesPerFoot = 12;

    public static string ToFeetAndInches(float meters)
    {
        if (meters <= 0)
        {
            return "0\"";
        }

        float totalInches = meters * InchesPerMeter;

        if (totalInches < InchesPerFoot)
        {
            return $"{Mathf.RoundToInt(totalInches)}\"";
        }
        else
        {
            int feet = Mathf.FloorToInt(totalInches / InchesPerFoot);
            int inches = Mathf.RoundToInt(totalInches % InchesPerFoot);

            if (inches == InchesPerFoot)
            {
                feet++;
                inches = 0;
            }

            return $"{feet}' {inches}\"";
        }
    }
}




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
public enum ShadingType
{
    Standard, Wireframe, Unlit, Clay, HiddenLine
}

[System.Serializable]
public class ShadingChangedEvent : UnityEvent { }
public class ControlPointSizeChangedEvent : UnityEvent<float> { }
public class UnitSystemChangedEvent : UnityEvent<GridUnitSystem> { }

public class ViewManager : MonoBehaviour
{
    public static ViewManager Instance { get; private set; }
    public ShadingChangedEvent OnShadingChanged = new ShadingChangedEvent();
    public ControlPointSizeChangedEvent OnControlPointSizeChanged = new ControlPointSizeChangedEvent();
    public UnitSystemChangedEvent OnUnitSystemSizeChanged = new UnitSystemChangedEvent();

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

    public Material hiddenLineMaterial;
    public Material standardMaterial;
    public Material wireframeMaterial;
    public Material unlitMaterial;
    public Material clayMaterial;

    public Material selectedHiddenLineMaterial;
    public Material selectedStandardMaterial;
    public Material selectedWireframeMaterial;
    public Material selectedUnlitMaterial;
    public Material selectedClayMaterial;
    public Material controlPointSelected;
    public Material controlPointDeselected;






    public GameObject gridGO;
    public void ToggleGrid()
    {
        gridGO.SetActive(!gridGO.activeSelf);
    }




    public float controlPointSize = 0.1f;
    public SliderUI controlPointSizeSlider;
    public void ControlPointSizeUpdated(float value)
    {
        controlPointSize = value;
        OnControlPointSizeChanged.Invoke(controlPointSize);
    }
    public Material GetSelectedControlPointMaterial()
    {
        return controlPointSelected;
    }
    public Material GetUnselectedControlPointMaterial()
    {
        return controlPointDeselected;
    }




    public bool showBackface = false;
    public Toggle backfaceToggle;
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








    public TMPro.TMP_Dropdown shadingDropdown;
    public ShadingType currentShadingType;
    public ShadingType GetShadingType()
    {
        return currentShadingType;
    }
    public ToggleGroupUI shadingToggleGroup;




    public void OnShadingToggleSelected(Toggle toggle)
    {
        if (Enum.TryParse(toggle.name, out currentShadingType))
        {
            OnShadingChanged.Invoke();
        }
    }


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





    public ToggleGroupUI unitSystemToggleGroup;
    public void OnUnitSystemToggleSelected(Toggle toggle)
    {
        if (Enum.TryParse(toggle.name, out unitSystem))
        {
            OnUnitSystemSizeChanged.Invoke(unitSystem);
        }
    }


    public GridUnitSystem unitSystem = GridUnitSystem.Metric;
    public ImperialDisplayMode imperialMode = ImperialDisplayMode.Adaptive;
    public Camera targetCamera;
    public Material gridMaterial;
    public float gridScaleFactor = 3.0f;

    private static readonly int MajorGridSizeID = Shader.PropertyToID("_MajorGridSize");
    private static readonly int MinorGridSizeID = Shader.PropertyToID("_GridSize");
    private static readonly int GridCenterID = Shader.PropertyToID("_GridCenter");


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
            const float METERS_TO_FEET = 3.28084f;
            const float FEET_TO_METERS = 1.0f / METERS_TO_FEET;

            if (imperialMode == ImperialDisplayMode.Adaptive)
            {
                const float YARDS_TO_FEET = 3f;
                float heightInFeet = height * METERS_TO_FEET;

                float feetInchesThreshold = 15f;
                float yardsFeetThreshold = 300f;

                if (heightInFeet < feetInchesThreshold)
                {
                    majorGridSpacing = 1f * FEET_TO_METERS; 
                    baseSpacing = majorGridSpacing / 12f;  
                }
                else if (heightInFeet < yardsFeetThreshold)
                {
                    majorGridSpacing = YARDS_TO_FEET * FEET_TO_METERS; 
                    baseSpacing = 1f * FEET_TO_METERS;                 
                }
                else
                {
                    float heightInYards = heightInFeet / YARDS_TO_FEET;
                    float power = Mathf.Floor(Mathf.Log10(heightInYards / gridScaleFactor));
                    float baseSpacingInYards = Mathf.Pow(10, power);

                    baseSpacing = (baseSpacingInYards * YARDS_TO_FEET) * FEET_TO_METERS;
                    majorGridSpacing = baseSpacing * 10f;
                }
            }
            else 
            {
                float heightInFeet = height * METERS_TO_FEET;
                float power = Mathf.Floor(Mathf.Log10(heightInFeet / gridScaleFactor));
                float baseSpacingInFeet = Mathf.Pow(10, power);
                float majorSpacingInFeet = baseSpacingInFeet * 10f;

                baseSpacing = baseSpacingInFeet / METERS_TO_FEET;
                majorGridSpacing = majorSpacingInFeet / METERS_TO_FEET;
            }
        }
        else 
        {
            float power = Mathf.Floor(Mathf.Log10(height / gridScaleFactor));
            baseSpacing = Mathf.Pow(10, power);
            majorGridSpacing = baseSpacing * 10f;
        }

        gridMaterial.SetFloat(MinorGridSizeID, 1.0f / baseSpacing);
        gridMaterial.SetFloat(MajorGridSizeID, 1.0f / majorGridSpacing);

        Vector3 gridCenter = new Vector3(
            Mathf.Round(pointOnPlane.x / majorGridSpacing) * majorGridSpacing,
            transform.position.y,
            Mathf.Round(pointOnPlane.z / majorGridSpacing) * majorGridSpacing
        );
        gridMaterial.SetVector(GridCenterID, gridCenter);
    }
}
