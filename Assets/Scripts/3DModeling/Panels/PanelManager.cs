using UnityEngine;
using System.Collections.Generic;
public enum DockSide
{
    Left,
    Right,
    World
}
public class PanelManager : MonoBehaviour
{
    public static PanelManager Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }


    public UIPanel[] leftSidePanels;
    public UIPanel[] rightSidePanels;

    private void Start()
    {
        for (int i = 0; leftSidePanels.Length > i; i++)
        {
            leftSidePanels[i].SetSide(true);
        }
        for (int i = 0; rightSidePanels.Length > i; i++)
        {
            rightSidePanels[i].SetSide(false);
        }
    }

    public void OpenPanel(UIPanel panel)
    {
        Debug.Log(panel);
        if(panel.GetSide() == true)
        {
            for (int i = 0; leftSidePanels.Length > i; i++)
            {
                Debug.Log(leftSidePanels[i]);
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
        else
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
    }
}