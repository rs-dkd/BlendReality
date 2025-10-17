using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class InputFieldManager : MonoBehaviour
{
    public static InputFieldManager Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

    }

    public SliderUI currentSlider;
    public void SelectInputField(SliderUI newSlider)
    {
        if (currentSlider != null)
        {
            currentSlider.DeactivateEdit();
        }
        currentSlider = newSlider;
        VRKeyboard.Instance.BindKeyboard(currentSlider);
    }
    public void DeselectInputField(SliderUI newSlider)
    {
        if (currentSlider != null && currentSlider == newSlider)
        {
            newSlider.DeactivateEdit();
            currentSlider = null;
            VRKeyboard.Instance.UnbindKeyboard(currentSlider);
        }
    }
}
