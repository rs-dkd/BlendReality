using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;


[RequireComponent(typeof(Button))]
public class VRKey : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string key = "A";

    private VRKeyboard keyboard;
    private Button button;


    private Color originalColor;

    void Start()
    {
        keyboard = GetComponentInParent<VRKeyboard>();
        button = GetComponent<Button>();
        originalColor = button.image.color;

        button.onClick.AddListener(OnKeyPress);

        Text keyText = GetComponentInChildren<Text>();
        if (keyText != null)
        {
            keyText.text = key;
        }
    }

 
    private void OnKeyPress()
    {
        if (keyboard != null)
        {
            keyboard.KeyPress(key);
        }
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        button.image.color = Color.cyan; 
    }

 
    public void OnPointerExit(PointerEventData eventData)
    {
        button.image.color = originalColor;
    }
}