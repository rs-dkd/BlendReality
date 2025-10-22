using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Unity.VisualScripting;
using System.Security.Cryptography;
using TMPro;
using static System.Net.Mime.MediaTypeNames;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class VRKeyboard : MonoBehaviour
{
    public static VRKeyboard Instance;
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


    public TMP_InputField inputField;
    public LazyFollow lazyFollow;


    public GameObject fullKeyboardPanel;
    public GameObject numberKeyboardPanel;

    public bool isNumber;

    public GameObject EnableKeyboard()
    {
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
    public void DisableKeyboard()
    {
        fullKeyboardPanel.SetActive(false);
        numberKeyboardPanel.SetActive(false);
    }
    public void BindKeyboard(SliderUI slider)
    {
        //keyboardPanel.SetActive(false);
        inputField = slider.inputField;
        lazyFollow.target = slider.GetKeyboardParent();

        isNumber = slider.IsInputNumber();

        GameObject keyboard = EnableKeyboard();
        RectTransform trans = keyboard.GetComponent<RectTransform>();
        trans.pivot = new Vector2(1, 0.5f);
        if (slider.isLeftPanel)
        {
            trans.pivot = new Vector2(0, 0.5f);
        }
        trans.localPosition = new Vector3();
    }
    public void UnbindKeyboard(SliderUI slider)
    {
        DisableKeyboard();
    }
    void Start()
    {
        VRKey[] keys = fullKeyboardPanel.transform.GetComponentsInChildren<VRKey>();
        for (int i = 0; i < keys.Length; i++)
        {
            keys[i].Setup(this);
        }
        VRKey[] keys2 = numberKeyboardPanel.transform.GetComponentsInChildren<VRKey>();
        for (int i = 0; i < keys2.Length; i++)
        {
            keys2[i].Setup(this);
        }
    }


    public void KeyPress(VRKey key)
    {
        Debug.Log(inputField);
        if (inputField == null) return;

        string input = "";

        switch (key.specialKey)
        {
            case SpecialKey.Key:
                input  = key.isLower ? key.lowerKey : key.upperKey;
                break;
            case SpecialKey.Tab:
                input = "\t";
                break;
            case SpecialKey.Enter:
                input  = "\n";
                break;
            case SpecialKey.Space:
                input = " ";
                break;
            case SpecialKey.Backspace:
                if (inputField.text.Length > 0)
                {
                    inputField.text = inputField.text.Substring(0, inputField.text.Length - 1);
                    return;
                }
                break;
            default:
                break;



        }



        if (inputField.contentType == TMP_InputField.ContentType.IntegerNumber)
        {
            input = System.Text.RegularExpressions.Regex.Replace(input, "[^0-9]", "");
        }
        else if (inputField.contentType == TMP_InputField.ContentType.DecimalNumber)
        {
            input = System.Text.RegularExpressions.Regex.Replace(input, "[^0-9.]", "");
        }

        inputField.text += input;
    }

    public void BindKeyboard(TMP_InputField fieldToBind, Transform followTarget, bool isNumeric = false)
    {
        inputField = fieldToBind;
        if (lazyFollow != null)
        {
            lazyFollow.target = followTarget;
        }
        isNumber = isNumeric;
        GameObject keyboard = EnableKeyboard();

        RectTransform trans = keyboard.GetComponent<RectTransform>();
        trans.localPosition = Vector3.zero;
    }

    public void UnbindKeyboard()
    {
        inputField = null;
        if (lazyFollow != null)
        {
            lazyFollow.target = null;
        }
        DisableKeyboard();
    }

}
