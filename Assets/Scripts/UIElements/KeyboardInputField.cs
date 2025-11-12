using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


public enum InputType
{
    Text,
    WholeNumber,
    Number,
}
/// <summary>
/// Wrapper to handle vr keyboard and inputfield
/// </summary>
public abstract class KeyboardInputField : MonoBehaviour
{
    [Tooltip("The TMP Input Field used for direct value entry.")]
    [SerializeField] protected TMP_InputField inputField;

    [Tooltip("Toggle button used to activate/deactivate the input field.")]
    [SerializeField] protected Toggle editToggle;

    [Tooltip("Text displaying the slider's title/label.")]
    [SerializeField] protected TMP_Text titleText;

    [Tooltip("Text displaying the slider's value.")]
    [SerializeField] protected TMP_Text valueText;

    [Tooltip("GameObject containing the regular view (e.g., the 'edit' icon).")]
    [SerializeField] protected GameObject editGO;

    [Tooltip("GameObject containing the editing view (e.g., the 'close' icon).")]
    [SerializeField] protected GameObject closeGO;
    [Tooltip("Set this to true if the entire slider object is located on the left panel.")]
    [SerializeField] private bool isLeftPanel;


    [Tooltip("Transform used as a parent for the keyboard when the slider is on the right.")]
    [SerializeField] private Transform keyboardTransLeft;

    [Tooltip("Transform used as a parent for the keyboard when the slider is on the left.")]
    [SerializeField] private Transform keyboardTransRight;

    [SerializeField] private bool hideUIOnStopEdit = true;

    [Tooltip("Either text, whole number, or number (that follows the unit system, metric or imperial)")]
    [SerializeField] private InputType contentType;


    /// <summary>
    /// Setup content type on start
    /// </summary>
    private void Start()
    {
        if(contentType == InputType.WholeNumber)
        {
            inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
        }
        else
        {
            inputField.contentType = TMP_InputField.ContentType.Standard;
        }
    }
    /// <summary>
    /// Places the vr keyboard on the left or right side of the panel
    /// </summary>
    public bool IsLeftPanel()
    {
        return isLeftPanel;
    }
    //Get position parent (Left or right side)
    public Transform GetKeyboardParent()
    {
        if (isLeftPanel)
        {
            return keyboardTransRight;
        }
        else
        {
            return keyboardTransLeft;
        }

    }

    
    /// <summary>
    /// Used by the VR Keyboard to determine if the input field is for numbers only
    /// </summary>
    public bool IsInputNumber()
    {
        if (contentType == InputType.WholeNumber || contentType == InputType.Number)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    /// <summary>
    /// Does the input follow the unit system or is it a normal whole number
    /// </summary>
    public virtual bool FollowsUnitSystem()
    {
        return false;
    }
    /// <summary>
    /// Get input field
    /// </summary>
    public TMP_InputField GetInputfield()
    {
        return inputField;
    }
    /// <summary>
    /// Get content type
    /// </summary>
    public InputType GetInputfieldType()
    {
        return contentType;
    }
    /// <summary>
    /// Show the inputfield and bind the vr keyboard
    /// </summary>
    public void OnEdit()
    {
        ActivateEdit();
        InputFieldManager.Instance.SelectInputField(this);
    }
    /// <summary>
    /// Hide the inputfield and unbind the vr keyboard
    /// </summary>
    public void OnStopEdit()
    {
        DeactivateEdit();
        InputFieldManager.Instance.DeselectInputField(this);
    }
    /// <summary>
    /// Toggle the edit inputfield
    /// </summary>
    public void OnEditToggle()
    {
        if (editToggle.isOn)
        {
            OnEdit();

        }
        else
        {
            OnStopEdit();
        }
    }
    /// <summary>
    /// Deactivate the edit visuals
    /// </summary>
    public void DeactivateEdit()
    {
        SetValue(GetTempValue());

        if (hideUIOnStopEdit)
        {
            if (titleText) titleText.gameObject.SetActive(true);
            inputField.gameObject.SetActive(false);
            if (closeGO) closeGO.SetActive(false);
            if (editGO) editGO.SetActive(true);
        }
        if (editToggle) editToggle.isOn = false;
    }
    /// <summary>
    /// Activate the edit visuals
    /// </summary>
    public virtual void ActivateEdit()
    {
        if (hideUIOnStopEdit)
        {
            if (titleText) titleText.gameObject.SetActive(false);
            inputField.gameObject.SetActive(true);
            if (closeGO) closeGO.SetActive(true);
            if (editGO) editGO.SetActive(false);
        }
        if (editToggle) editToggle.isOn = true;
    }
    /// <summary>
    /// Get the value
    /// </summary>
    public virtual string GetValue()
    {
        return valueText.text;
    }
    /// <summary>
    /// Set the value
    /// </summary>
    public virtual void SetValue(string val)
    {
        valueText.text = val;
        inputField.text = val;
    }

    /// <summary>
    /// Get the temporary value (Thats is being inputed from the vr keyboard)
    /// </summary>
    public virtual string GetTempValue()
    {
        return inputField.text;
    }
    /// <summary>
    /// Set the temporary value (Thats is being inputed from the vr keyboard)
    /// </summary>
    public virtual void SetTempValue(string val)
    {
        inputField.text = val;
    }

    /// <summary>
    /// Get title
    /// </summary>
    public String GetTitle()
    {
        return titleText.text;
    }
    /// <summary>
    /// Set title
    /// </summary>
    public void SetTitle(string _title)
    {
        titleText.text = _title;
    }

}
