using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.Windows;
using UnityEngine.XR.Interaction.Toolkit.UI;
using static System.Net.Mime.MediaTypeNames;

/// <summary>
/// Handles UI for a VR keyboard, managing key presses and input field binding.
/// </summary>
public class VRKeyboard : MonoBehaviour
{
    // --- Singleton instance ---
    public static VRKeyboard Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple instances of VRKeyboard detected! Destroying duplicate.");
            Destroy(gameObject);
        }

    }
    // --- Variables ---

    [Header("Keyboard Panels")]
    [Tooltip("Reference to the GameObject containing the full alphanumeric keyboard layout.")]
    [SerializeField] private GameObject fullKeyboardPanel;

    [Tooltip("Reference to the GameObject containing the number-only keyboard layout.")]
    [SerializeField] private GameObject numberKeyboardPanel;

    [Tooltip("Reference to the LazyFollow script that controls the keyboard's position.")]
    [SerializeField] private LazyFollow lazyFollow;
    // The currently active input field that the keyboard is typing into.
    private KeyboardInputField inputField;
    // Flag to determine which keyboard layout to enable (true for number, false for full).
    private bool isNumber;


    private VRKey[] fullKeyboardKeys;
    private VRKey[] numKeyboardKeys;
    public GameObject imperialKeysRow;



    // --- Functions ---

    /// <summary>
    /// Enables keyboard based on isNumber flag.
    /// </summary>
    public GameObject EnableKeyboard()
    {
        //set both keyboards inactive first
        DisableKeyboard();
        if (isNumber)
        {
            numberKeyboardPanel.SetActive(true);
            return numberKeyboardPanel;
        }
        else
        {
            fullKeyboardPanel.SetActive(true);
            return fullKeyboardPanel;
        }
    }
    /// <summary>
    /// Disables both keyboards.
    /// </summary>
    public void DisableKeyboard()
    {
        fullKeyboardPanel.SetActive(false);
        numberKeyboardPanel.SetActive(false);
    }

    /// <summary>
    /// Binds the VR keyboard to the specified input field from the slider.
    /// </summary>
    public void BindKeyboard(KeyboardInputField _inputfield)
    {
        inputField = _inputfield;
        isNumber = _inputfield.IsInputNumber();
        SetupKeyboardPosition(_inputfield);
        UpdateImperialKeysVisibility(ViewManager.Instance.GetUnitSystem());
    }
    /// <summary>
    /// Sets the keyboard position based on the slider's keyboard parent transform.
    /// Fixes pivot and local position for proper alignment.
    /// </summary>
    private void SetupKeyboardPosition(KeyboardInputField inputfield)
    {
        //updates the keyboard position target
        lazyFollow.target = inputfield.GetKeyboardParent();

        //Fixes pivot and local position based on left or right side
        GameObject keyboard = EnableKeyboard();
        RectTransform trans = keyboard.GetComponent<RectTransform>();
        trans.pivot = new Vector2(1, 0.5f);
        if (inputfield.IsLeftPanel())
        {
            trans.pivot = new Vector2(0, 0.5f);
        }
        trans.localPosition = new Vector3();
    }

    /// <summary>
    /// Unbinds keybaord from the specified input field from the slider.
    /// Just calls disable keyboard.
    /// </summary>
    public void UnbindKeyboard(KeyboardInputField inputfield = null)
    {
        DisableKeyboard();
        InputFieldManager.Instance.KeyboardClosed();
    }

    /// <summary>
    /// Hides keyboard
    /// </summary>
    public void CloseKeyboard()
    {
        UnbindKeyboard();
    }



    /// <summary>
    /// Handles setting up the number keyboard and full keyboard keys on start.
    /// </summary>
    void Start()
    {
        ViewManager.Instance.OnUnitSystemSizeChanged.AddListener(UpdateImperialKeysVisibility);
        


        fullKeyboardKeys = fullKeyboardPanel.transform.GetComponentsInChildren<VRKey>();
        for (int i = 0; i < fullKeyboardKeys.Length; i++)
        {
            fullKeyboardKeys[i].Setup(this);
        }
        numKeyboardKeys = numberKeyboardPanel.transform.GetComponentsInChildren<VRKey>();
        for (int i = 0; i < numKeyboardKeys.Length; i++)
        {
            numKeyboardKeys[i].Setup(this);
        }
    }

    /// <summary>
    /// Handles a key press event from a VRKey.
    /// VRKey calls this function when its button is pressed.
    /// </summary>
    public void KeyPress(VRKey key)
    {
        if (inputField == null)
        {
            Debug.LogWarning("VRKeyboard: No input field is currently bound to the keyboard.");
            return;
        }

        string input = "";

        switch (key.GetKeyType())
        {
            case KeyType.Key:
                input  = key.GetKeyString();
                break;
            case KeyType.Tab:
                input = "\t";
                break;
            case KeyType.Enter:
                //input  = "\n";
                //break;
                CloseKeyboard();
                return;
            case KeyType.RemoveAll:
                inputField.SetTempValue("");
                return;
            case KeyType.CapLk:
                for (int i = 0; i < fullKeyboardKeys.Length; i++)
                {
                    fullKeyboardKeys[i].Switch();
                }
                return;
            case KeyType.Space:
                input = " ";
                break;
            case KeyType.Backspace:
                //Check to make sure there is text to delete
                if (inputField.GetTempValue().Length > 0)
                {
                    inputField.SetTempValue(inputField.GetTempValue().Substring(0, inputField.GetTempValue().Length - 1));
                    //If backspace was pressed, we handle it here and return early
                }
                return;
            default:
                break;
        }

        //Filter out incorrect input based on input field content type
        input = RemoveIncorrectInput(input, inputField.GetInputfieldType());


        //add text to the input fields text
        inputField.SetTempValue(inputField.GetTempValue() + input);
    }
    /// <summary>
    /// Removes incorrect characters from input based on the input field's content type.
    /// TODO: Expand this to handle more content types as needed: alphanumeric, name, email, etc.
    /// </summary>
    private string RemoveIncorrectInput(string input, InputType contentType)
    {
        if (contentType == InputType.WholeNumber)
        {
            input = System.Text.RegularExpressions.Regex.Replace(input, "[^0-9]", "");
        }
        else if (contentType == InputType.Number)
        {
            input = System.Text.RegularExpressions.Regex.Replace(input, "[^0-9.'\"]", "");
        }
        return input;
    }
    /// <summary>
    /// Shows hides imperial keys ( ' " )
    /// </summary>
    public void UpdateImperialKeysVisibility(GridUnitSystem gridSystem)
    {

        if (gridSystem == GridUnitSystem.Imperial && inputField != null && inputField.FollowsUnitSystem())
        {
            imperialKeysRow.SetActive(true);
        }
        else
        {
            imperialKeysRow.SetActive(false);
        }
    }

}
