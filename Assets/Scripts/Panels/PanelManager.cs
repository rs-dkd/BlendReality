using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
public enum DockSide
{
    Left,
    Right,
    Center,
    World
}
/// <summary>
/// On Panel Changed Event
/// </summary>
public class FilePanelChangeEvent : UnityEvent<UIPanel> { }

/// <summary>
/// Controls all the main panels in the scene
/// </summary>
public class PanelManager : MonoBehaviour
{
    // Singleton Pattern
    public static PanelManager Instance;
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public FilePanelChangeEvent OnPanelChanged = new FilePanelChangeEvent();


    [Tooltip("Panel GO")]
    [SerializeField] private GameObject panelsPanel;
    [Tooltip("Panel in the left dock")]
    [SerializeField] private UIPanel[] leftSidePanels;
    [Tooltip("Panel in the right dock")]
    [SerializeField] private UIPanel[] rightSidePanels;
    [Tooltip("Panel in the center")]
    [SerializeField] private UIPanel[] centerSidePanels;


    /// <summary>
    /// Show hides all panels
    /// </summary>
    public void ToggleAllPanels()
    {
        panelsPanel.SetActive(!panelsPanel.activeSelf);
    }
    /// <summary>
    /// Setup the dock side for all the panels
    /// </summary>
    private void Start()
    {
        for (int i = 0; leftSidePanels.Length > i; i++)
        {
            leftSidePanels[i].SetSide(DockSide.Left);
        }
        for (int i = 0; rightSidePanels.Length > i; i++)
        {
            rightSidePanels[i].SetSide(DockSide.Right);
        }
        for (int i = 0; centerSidePanels.Length > i; i++)
        {
            centerSidePanels[i].SetSide(DockSide.Center);
        }
    }
    /// <summary>
    /// Locate the panel and show it, hide the rest in the dock side
    /// </summary>
    public void OpenPanel(UIPanel panel)
    {
        OnPanelChanged.Invoke(panel);
        if (panel.GetSide() == DockSide.Left)
        {
            for (int i = 0; leftSidePanels.Length > i; i++)
            {
                if (panel == leftSidePanels[i])
                {
                    leftSidePanels[i].OpenMenu();
                }
                else
                {
                    leftSidePanels[i].CloseMenu();
                }
            }
        }
        else if (panel.GetSide() == DockSide.Right)
        {
            for (int i = 0; rightSidePanels.Length > i; i++)
            {
                if (panel == rightSidePanels[i])
                {
                    rightSidePanels[i].OpenMenu();
                }
                else
                {
                    rightSidePanels[i].CloseMenu();
                }
            }
        }
        else if (panel.GetSide() == DockSide.Center)
        {
            for (int i = 0; centerSidePanels.Length > i; i++)
            {
                if (panel == centerSidePanels[i])
                {
                    centerSidePanels[i].OpenMenu();
                }
                else
                {
                    centerSidePanels[i].CloseMenu();
                }
            }
        }
    }
}