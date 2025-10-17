using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem;


public enum ControllerInputType
{
    LeftPrimary, RightPrimary, LeftSecondary, RightSecondary
}
public class UIPanel : MonoBehaviour
{
    public GameObject panel;
    private bool isPanelOpen = false;
    public ControllerInputType inputType;
    private InputAction menuAction;
    public bool isLeftSide;
    void Start()
    {


        panel.SetActive(false);
        SetupInput();
    }
    public void SetSide(bool _isLeftSide)
    {
        isLeftSide = _isLeftSide;
    }
    public bool GetSide()
    {
        return isLeftSide;
    }
    public void ToggleMenu()
    {
        if (isPanelOpen)
            CloseMenu();
        else
            PanelManager.Instance.OpenPanel(this);
    }

    public void OpenMenu()
    {
        Debug.Log("Open" + gameObject.name);
        panel.SetActive(true);
        isPanelOpen = true;
    }

    public void CloseMenu()
    {
        Debug.Log("Close" + gameObject.name);
        panel.SetActive(false);
        isPanelOpen = false;
    }




    public InputActionReference menuButtonAction;


    void SetupInput()
    {
        menuAction = new InputAction("MenuButton", InputActionType.Button);
        if (inputType == ControllerInputType.LeftPrimary)
        {
            menuAction.AddBinding("<XRController>{LeftHand}/primaryButton");
        }
        else if (inputType == ControllerInputType.RightPrimary)
        {
            menuAction.AddBinding("<XRController>{RightHand}/primaryButton");
        }
        else if (inputType == ControllerInputType.LeftSecondary)
        {
            menuAction.AddBinding("<XRController>{LeftHand}/secondaryButton");
        }
        else if (inputType == ControllerInputType.RightSecondary)
        {
            menuAction.AddBinding("<XRController>{RightHand}/secondaryButton");
        }
        menuAction.Enable();
        menuAction.performed += OnMenuButtonPressed;
    }

    void OnMenuButtonPressed(InputAction.CallbackContext context)
    {
        ToggleMenu();
    }
}