using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// A custom Event that passes a float value when the slider is changed.
/// </summary>
public class SliderValueChangedEvent : UnityEvent<float> { }


/// <summary>
/// Controls a UI slider element, managing its value display,
/// unit system conversion, and interaction with an input field.
/// CONNECTED TO UI: Slider, InputField, Toggle, Text components
/// </summary>
public class SliderUI : KeyboardInputField
{
    // --- Variables ---


    //Event that passes a float value when the slider is changed
    public SliderValueChangedEvent OnSliderValueChangedEvent = new SliderValueChangedEvent();

    [Header("Component References")]

    [Tooltip("The main Unity Slider component.")]
    [SerializeField] private Slider slider;

    [Header("Configuration")]
    [Tooltip("The display title of the slider (e.g., 'Width').")]
    [SerializeField] private string title;

    [Tooltip("If checked, the slider will snap to integer values.")]
    [SerializeField] private bool isWholeNumbers;

    [Tooltip("If true, the value display follows the global unit system (Imperial/Metric).")]
    [SerializeField] private bool followsUnitSystem = true;



    // The current active unit system (set by ViewManager).
    private GridUnitSystem unitSystem;



    // --- Getters and Setters ---
    /// <summary>
    /// Get the slider value as float
    /// </summary>
    public float GetValueAsFloat()
    {
        return slider.value;
    }
    /// <summary>
    /// Set the slider value
    /// </summary>
    public void SetValue(float val)
    {
        slider.value = val;

        UpdateVisualValues();
    }
    /// <summary>
    /// Update the visual text using the unit system
    /// </summary>
    private void UpdateVisualValues()
    {

        if (ViewManager.Instance.GetUnitSystem() == GridUnitSystem.Imperial && followsUnitSystem)
        {
            string displayVal = MetricConverter.ToFeetAndInches(slider.value);
            valueText.text = displayVal;
            inputField.text = displayVal;
        }
        else
        {
            valueText.text = slider.value.ToString();
            inputField.text = slider.value.ToString();
        }
    }
    /// <summary>
    /// Set the value from a string
    /// </summary>
    public override void SetValue(string val)
    {
        if (ViewManager.Instance.GetUnitSystem() == GridUnitSystem.Imperial && followsUnitSystem)
        {
            SetValue(MetricConverter.ToMeters(val));
        }
        else
        {
            if(float.TryParse(val, out float convertedValue))
            {
                SetValue(convertedValue);
            }
        }


    }


    //Set sliders min and max values
    public void SetMinMax(float min, float max)
    {

        slider.minValue = min;
        slider.maxValue = max;

    }



    /// <summary>
    /// Show the entire object
    /// </summary>
    public void Show()
    {
        gameObject.SetActive(true);
    }
    /// <summary>
    /// Hide the entire object
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// If unit system changes, update the slider value display accordingly (Metric or imperial)
    /// </summary>
    public void OnUnitSystemChanged(GridUnitSystem _unitSystem)
    {
        unitSystem = _unitSystem;

        OnSliderUpdated();

    }

    /// <summary>
    /// Start Function, initializes the slider UI component and subscribes to unit system change events.
    /// </summary>
    void Start()
    {
        titleText.text = title;
        ViewManager.Instance.OnUnitSystemSizeChanged.AddListener(OnUnitSystemChanged);
        OnUnitSystemChanged(ViewManager.Instance.GetUnitSystem());
        slider.wholeNumbers = isWholeNumbers;
        OnEditToggle();
        UpdateVisualValues();
        slider.onValueChanged.AddListener(OnSliderValueChangedInternal);
    }
    /// <summary>
    /// Destroy Function, unsubscribes from unit system change events to prevent memory leaks.
    /// </summary>
    void OnDestroy()
    {
        if (ViewManager.Instance != null)
        {
            // Unsubscribe from the event
            ViewManager.Instance.OnUnitSystemSizeChanged.RemoveListener(OnUnitSystemChanged);
        }
    }
    /// <summary>
    /// Main function to handle slider value updates
    /// CONNECTED TO UI: Triggered when the slider UI changes
    /// </summary>
    public void OnSliderUpdated()
    {
        //slider.value = (float)Math.Round(slider.value, 3);
        UpdateVisualValues();

        OnSliderValueChangedEvent.Invoke(slider.value);
    }
    /// <summary>
    /// Main function to handle input field value updates
    /// CONNECTED TO UI: Triggered when the input field changes
    /// </summary>
    public void OnInputFieldUpdated()
    {
        // TODO: Connect input field and slider value properly with unit conversion
        valueText.text = inputField.text.ToString();
        if (float.TryParse(valueText.text, out float convertedValue))
        {
            OnSliderValueChangedEvent.Invoke(convertedValue);
        }
    }
    /// <summary>
    /// Update the displayed text based on the current unit system
    /// </summary>
    public void UpdateValueText()
    {
        // TODO: Connect input field and slider value properly with unit conversion
        if (unitSystem == GridUnitSystem.Imperial && followsUnitSystem)
        {
            valueText.text = MetricConverter.ToFeetAndInches(slider.value);
        }
        else
        {
            valueText.text = Math.Round(slider.value, 3).ToString();
        }


    }
    /// <summary>
    /// Get if it follows the unit system
    /// </summary>
    public override bool FollowsUnitSystem()
    {
        return followsUnitSystem;
    }
    /// <summary>
    /// Override for activate edit, adds updates the visual text too
    /// </summary>
    public override void ActivateEdit()
    {
        base.ActivateEdit();
        UpdateVisualValues();
    }

    private void OnSliderValueChangedInternal(float value)
    {
        UpdateVisualValues(); // This updates the text display
    }
}
