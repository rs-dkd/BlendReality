using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class SliderValueChangedEvent : UnityEvent<float> { }
public class SliderUI : MonoBehaviour
{
    public SliderValueChangedEvent OnSliderValueChangedEvent = new SliderValueChangedEvent();
    public TMP_Text valueText;
    public Slider slider;
    public TMP_InputField inputField;
    public GridUnitSystem unitSystem;

    public Toggle editToggle;
    public TMP_Text titleText;
    public string title;
    public bool isWholeNumbers;
    public GameObject editGO;
    public GameObject closeGO;
    public bool followsUnitSystem = true;

    public void SetTitle(string _title)
    {
        title = _title;
        titleText.text = title;
    }
    public void SetMinMax(float min, float max)
    {

        slider.minValue = min;
        slider.maxValue = max;

    }
    public void Show()
    {
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public float GetValue()
    {
        return slider.value;
    }
    public void SetValue(float val)
    {
        slider.value = val;
    }
    void Start()
    {
        titleText.text = title;
        ViewManager.Instance.OnUnitSystemSizeChanged.AddListener(OnUnitSystemChanged);
        OnUnitSystemChanged(ViewManager.Instance.unitSystem);
        slider.wholeNumbers = isWholeNumbers;
        OnEditToggle();
        UpdateValueText();
    }
    public void OnEditToggle()
    {
        if (editToggle.isOn)
        {
            titleText.gameObject.SetActive(false);
            inputField.gameObject.SetActive(true);
            closeGO.SetActive(true);
            editGO.SetActive(false);
        }
        else
        {
            titleText.gameObject.SetActive(true);
            inputField.gameObject.SetActive(false);
            closeGO.SetActive(false);
            editGO.SetActive(true);
        }
    }
    public void OnUnitSystemChanged(GridUnitSystem _unitSystem)
    {
        unitSystem = _unitSystem;
        OnSliderUpdated();

    }
    public void OnSliderUpdated()
    {
        UpdateValueText();

        OnSliderValueChangedEvent.Invoke(slider.value);
    }


    public void UpdateValueText()
    {

        if (unitSystem == GridUnitSystem.Imperial && followsUnitSystem)
        {
            valueText.text = MetricConverter.ToFeetAndInches(slider.value);
        }
        else
        {
            valueText.text = Math.Round(slider.value, 3).ToString();
        }
    }

    public void OnInputFieldUpdated()
    {
        valueText.text = inputField.text.ToString();
        if (float.TryParse(valueText.text, out float convertedValue))
        {
            OnSliderValueChangedEvent.Invoke(convertedValue);
        }
    }
}
