using UnityEngine;
using UnityEngine.InputSystem;


/// <summary>
/// Main buttons on the controller (Have to add here if you want to add more bottons fron the controllers)
/// </summary>
public enum ControllerInputType
{
    LeftPrimary, RightPrimary, LeftSecondary, RightSecondary, Trigger
}
/// <summary>
/// Main class for the UI Panel
/// Handles the opening closing, and dock side of the ui panels
/// </summary>
public class UIPanel : MonoBehaviour
{
    [Tooltip("Name of the panel")]
    [SerializeField] private string name;
    [Tooltip("Panel GO")]
    [SerializeField] private GameObject panel;
    [Tooltip("Which controller button toggles the panel")]
    [SerializeField] private ControllerInputType inputType;


    private DockSide side;
    private bool isPanelOpen = false;
    private InputAction menuAction;


    /// <summary>
    /// Setup the panel
    /// Hide it, and setup the controller button event
    /// </summary>
    void Start()
    {
        panel.SetActive(false);
        SetupInput();
    }
    /// <summary>
    ///Set dock side (left right center)
    /// </summary>
    public void SetSide(DockSide _side)
    {
        side = _side;
    }
    /// <summary>
    /// Get dock side
    /// </summary>
    public DockSide GetSide()
    {
        return side;
    }
    /// <summary>
    /// Toggle the panel
    /// </summary>
    public void ToggleMenu()
    {
        if (isPanelOpen)
            CloseMenu();
        else
            PanelManager.Instance.OpenPanel(this);
    }
    /// <summary>
    /// Show panel
    /// </summary>
    public void OpenMenu()
    {
        panel.SetActive(true);
        isPanelOpen = true;
    }
    /// <summary>
    /// Hide panel
    /// </summary>
    public void CloseMenu()
    {
        panel.SetActive(false);
        isPanelOpen = false;
    }

    /// <summary>
    /// Bind the button to the panel
    /// </summary>
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
        else if (inputType == ControllerInputType.Trigger)
        {
            menuAction.AddBinding("<XRController>{RightHand}/trigger");
        }
        menuAction.Enable();
        menuAction.performed += OnMenuButtonPressed;
    }
    /// <summary>
    /// Callback for the button
    /// </summary>
    void OnMenuButtonPressed(InputAction.CallbackContext context)
    {
        ToggleMenu();
    }
}