using UnityEngine;
using UnityEngine.InputSystem;

public class UIPanelController : MonoBehaviour
{
    [Header("Input Action References")]
    public InputActionReference menuButtonAction;

    [Header("Menu Reference")]
    public UIPanel uiPanel;

    [Header("Alternative Input Setup")]
    public bool useDirectInputAction = true;

    private InputAction menuAction;

    void Start()
    {
        if (uiPanel == null)
            uiPanel = FindObjectOfType<UIPanel>();

        if (uiPanel == null)
        {
            Debug.LogError("VRMenu not found! Please assign it in the inspector.");
        }

        SetupInput();
    }

    void SetupInput()
    {
        if (useDirectInputAction)
        {
            menuAction = new InputAction("MenuButton", InputActionType.Button);
            menuAction.AddBinding("<XRController>{LeftHand}/primaryButton");
            menuAction.Enable();
            menuAction.performed += OnMenuButtonPressed;
        }
        else if (menuButtonAction != null)
        {
            menuButtonAction.action.performed += OnMenuButtonPressed;
            menuButtonAction.action.Enable();
        }
        else
        {
            Debug.LogError("No input method configured! Please assign InputActionReference or enable useDirectInputAction.");
        }
    }

    void OnMenuButtonPressed(InputAction.CallbackContext context)
    {
        if (uiPanel != null)
            uiPanel.ToggleMenu();
    }

    void OnEnable()
    {
        if (menuAction != null)
            menuAction.Enable();
        else if (menuButtonAction != null)
            menuButtonAction.action.Enable();
    }

    void OnDisable()
    {
        if (menuAction != null)
            menuAction.Disable();
        else if (menuButtonAction != null)
            menuButtonAction.action.Disable();
    }

    void OnDestroy()
    {
        if (menuAction != null)
        {
            menuAction.performed -= OnMenuButtonPressed;
            menuAction.Dispose();
        }
        else if (menuButtonAction != null)
        {
            menuButtonAction.action.performed -= OnMenuButtonPressed;
        }
    }
}