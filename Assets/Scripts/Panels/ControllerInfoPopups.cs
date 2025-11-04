using UnityEngine;

/// <summary>
/// Controls showing hiding the info popups for the controller buttons
/// </summary>
public class ControllerInfoPopups : MonoBehaviour
{
    [Tooltip("All the callouts")]
    [SerializeField] private Unity.VRTemplate.Callout[] callouts;

    /// <summary>
    /// On panel changed, check if settings and show hide callouts 
    /// </summary>
    private void Start()
    {
        PanelManager.Instance.OnPanelChanged.AddListener(OnPanelChanged);
    }
    /// <summary>
    /// On panel changed, check if settings and show hide callouts 
    /// </summary>
    public void OnPanelChanged(UIPanel panel)
    {
        if(panel.name == "Settings")
        {
            ShowPopups();
        }
        else
        {
            HidePopups();
        }
    }
    /// <summary>
    /// Show callouts 
    /// </summary>
    public void ShowPopups()
    {
        for (int i = 0; i < callouts.Length; i++)
        {
            callouts[i].TurnOnStuff();
        }
    }
    /// <summary>
    /// Hide callouts 
    /// </summary>
    public void HidePopups()
    {
        for (int i = 0; i < callouts.Length; i++)
        {
            callouts[i].TurnOffStuff();
        }
    }
}
