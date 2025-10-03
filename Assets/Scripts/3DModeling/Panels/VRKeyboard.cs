using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class VRKeyboard : MonoBehaviour
{

    public Text inputField;


    public GameObject fullKeyboard;

    public GameObject numpad;

    private bool isCaps = false;

    void Start()
    {
        fullKeyboard.SetActive(true);
        numpad.SetActive(false);
    }


    public void KeyPress(string key)
    {
        if (inputField == null) return;

        switch (key)
        {
            case "Backspace":
                if (inputField.text.Length > 0)
                {
                    inputField.text = inputField.text.Substring(0, inputField.text.Length - 1);
                }
                break;
            case "Caps":
                isCaps = !isCaps;
                ToggleCaps(isCaps);
                break;
            case "Enter":
                Debug.Log("Enter key pressed: " + inputField.text);
                break;
            default:
                inputField.text += isCaps ? key.ToUpper() : key.ToLower();
                break;
        }
    }


    private void ToggleCaps(bool caps)
    {
        VRKey[] keys = fullKeyboard.GetComponentsInChildren<VRKey>();
        foreach (VRKey key in keys)
        {
            if (key.key.Length == 1 && char.IsLetter(key.key[0]))
            {
                Text keyText = key.GetComponentInChildren<Text>();
                if (keyText != null)
                {
                    keyText.text = caps ? keyText.text.ToUpper() : keyText.text.ToLower();
                }
            }
        }
    }

    public void SwitchLayout(bool useNumpad)
    {
        fullKeyboard.SetActive(!useNumpad);
        numpad.SetActive(useNumpad);
    }
}
