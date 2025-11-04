using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

/// <summary>
/// Handles selection and management of input fields for VR keyboard input.
/// Only one input field can be active at a time.
/// </summary>
public class InputFieldManager : MonoBehaviour
{
    // --- Singleton instance ---
    public static InputFieldManager Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple instances of InputFieldManager detected! Destroying duplicate.");
            Destroy(gameObject);
        }

    }
    // --- Variables ---

    //Currently selected input field
    private KeyboardInputField currentInputField;


    // --- Main Functions ---

    /// <summary>
    /// Binds the VR keyboard to the newly selected input field, and handles deactivation of the previous one.
    /// End result is active keyboard on the new input field.
    /// If here was a previously active input field, it gets deactivated.
    /// </summary>
    public void SelectInputField(KeyboardInputField inputfield)
    {
        if (currentInputField != null)
        {
            currentInputField.DeactivateEdit();
        }
        currentInputField = inputfield;
        VRKeyboard.Instance.BindKeyboard(currentInputField);
    }
    /// <summary>
    /// Unbinds the VR keyboard if the deselected input field is the currently active one.
    /// End result is no active input field and disabled keyboard.
    /// </summary>
    public void DeselectInputField(KeyboardInputField inputfield)
    {
        if (currentInputField != null && currentInputField == inputfield)
        {
            inputfield.DeactivateEdit();
            currentInputField = null;
            VRKeyboard.Instance.UnbindKeyboard(currentInputField);
        }
    }
    /// <summary>
    /// Keyboard was closed
    /// Deselect the inputfield
    /// </summary>
    public void KeyboardClosed()
    {
        DeselectInputField(currentInputField);
    }
}
