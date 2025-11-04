using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;


// Enum for key types on the VR keyboard
public enum KeyType
{
    Key,
    Backspace,
    Tab,
    CapLk,
    Shift,
    Enter,
    Space,
    RemoveAll,
}
/// <summary>
/// Controls a UI slider element, managing its value display,
/// unit system conversion, and interaction with an input field.
/// CONNECTED TO UI: Slider, InputField, Toggle, Text components
/// </summary>
public class VRKey : MonoBehaviour
{
    // --- Variables ---


    [Header("Key Configuration")]
    [Tooltip("Defines the key's function, like a letter or a special command.")]
    [SerializeField] private KeyType type = KeyType.Key;

    [Tooltip("The character displayed when the keyboard is in lower-case mode.")]
    [SerializeField] private string lowerKey = "a";

    [Tooltip("The character displayed when the keyboard is in upper-case mode.")]
    [SerializeField] private string upperKey = "A";

    [Header("Component References")]
    [Tooltip("Reference to the Button component for UI interaction.")]
    [SerializeField] private Button button;
    
    [Tooltip("Reference to the Text component displaying the key's character.")]
    [SerializeField] private TMP_Text keyText;

    // The keyboard controller this key reports to.
    private VRKeyboard keyboard;
    // The current case state (true if lower case, false if upper case).
    private bool isLower = true;


    // --- Getters and Setters ---

    /// <summary>
    /// Getter for key type
    /// </summary>
    public KeyType GetKeyType()
    {
        return type;
    }
    /// <summary>
    /// Getter for IsLower
    /// </summary>
    public bool GetIsLower()
    {
        return isLower;
    }
    /// <summary>
    /// Getter for Lower case key
    /// </summary>
    public string GetLowercase()
    {
        return lowerKey;
    }
    /// <summary>
    /// Getter for Upper case key
    /// </summary>
    public string GetUppercase()
    {
        return upperKey;
    }
    /// <summary>
    /// Getter key
    /// </summary>
    public string GetKeyString()
    {
        if (isLower)
        {
            return GetLowercase();
        }
        else
        {
            return GetUppercase();
        }
    }

    // --- Main Functions ---

    /// <summary>
    /// Keyboard calls setup to initialize the key with its keyboard reference
    /// </summary>
    public void Setup(VRKeyboard _keyboard)
    {
        keyboard = _keyboard;
        keyText.text = lowerKey;
        isLower = true;
    }

    /// <summary>
    /// Calls the keyboard to notify when button is pressed
    /// CONNECTED TO UI: button
    /// </summary>
    public void OnKeyPress()
    {
        if (keyboard != null)
        {
            keyboard.KeyPress(this);
        }
    }

    /// <summary>
    /// Keyboard can switch between lower and upper case when CAPS is pressed
    /// </summary>
    public void Switch()
    {
        isLower = !isLower;
        if (isLower)
        {
            keyText.text = lowerKey;
        }
        else
        {
            keyText.text = upperKey;
        }
    }


}