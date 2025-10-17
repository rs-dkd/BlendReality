using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;

public enum SpecialKey
{
    Key,
    Backspace,
    Tab,
    CapLk,
    Shift,
    Enter,
    Space,
}


public class VRKey : MonoBehaviour
{
    public SpecialKey specialKey = SpecialKey.Key;
    public string lowerKey = "a";
    public string upperKey = "A";

    private VRKeyboard keyboard;
    public Button button;
    public bool isLower = true;

    public TMP_Text keyText;

    public void Setup(VRKeyboard _keyboard)
    {
        Debug.Log("f");
        keyboard = _keyboard;

        keyText.text = lowerKey;
        isLower = true;
    }

 
    public void OnKeyPress()
    {
        if (keyboard != null)
        {
            keyboard.KeyPress(this);
        }
    }

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