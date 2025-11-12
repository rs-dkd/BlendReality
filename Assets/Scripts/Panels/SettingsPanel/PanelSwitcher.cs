using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Controls the tab layout for the Settings panel
/// </summary>
public class PanelSwitcher : MonoBehaviour
{
    [Tooltip("Toggle Group")]
    [SerializeField] private ToggleGroupUI toggleGroupUI;
    [Tooltip("GO[] of the panels")]
    [SerializeField] private GameObject[] optionPanels;
    [Tooltip("Names of the panels (make sure names match the go names")]
    [SerializeField] private string[] options = { "Load", "Save", "Settings", "Statistics" };


    /// <summary>
    /// Listens to the toggle group changes and calls the function
    /// </summary>
    void Start()
    {
        toggleGroupUI.OnToggleGroupChanged.AddListener(OnPanelChanged);
        //Setups the toggle group which creates the tabs
        toggleGroupUI.Setup(options);

        // Initialize - show first panel, hide the rest
        if (optionPanels.Length > 0)
        {
            for (int i = 0; i < optionPanels.Length; i++)
            {
                optionPanels[i].SetActive(i == 0);
            }
        }
    }
    /// <summary>
    /// Based on the name change the panel
    /// </summary>
    public void OnPanelChanged(Toggle toggle)
    {
        for (int i = 0; i < options.Length; i++)
        {
            if (toggle.name == options[i])
            {
                optionPanels[i].SetActive(true);
            }
            else
            {
                optionPanels[i].SetActive(false);
            }
        }
    }

}