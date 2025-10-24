using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using System;
public enum DockSide
{
    Left,
    Right,
}


//one panel on player left, one on right, for important system panels. 
//Temp panels like keyboard are stored in panelmanager in seperate panelholders
public class PanelManager : Singleton<PanelManager>
{

    private Transform playerPanelsHolderLeft;

    private Transform playerPanelsHolderRight;

    private BasePanel currentLeftDockingPanel;

    private BasePanel currentRightDockingPanel;

    private BasePanel keyboard;//idk how that keyboard works and what component you are using but just put it here

    private confirmationPanel confirmationPanel;//THE one confirmation panel
    private BasePanel othertempPanel; //other temp panel if necessary

    public void DockPanel(BasePanel panel, DockSide side)
    {
        if (side == DockSide.Left)
        {
            if (currentLeftDockingPanel != null)
            {
                currentLeftDockingPanel.HidePanel();
            }

            panel.transform.SetParent(playerPanelsHolderLeft, false);
            //adding local transform positioning if you want
            currentLeftDockingPanel = panel;

        }

        if (side == DockSide.Right)
        {
            if (currentRightDockingPanel != null)
            {
                currentRightDockingPanel.HidePanel();
            }

            panel.transform.SetParent(playerPanelsHolderRight, false);
            //adding local transform positioning if you want
            currentRightDockingPanel = panel;

        }

    }

    public void ShowKeyboard()
    {
        //can be moved and open/closed
    }

    public void HideKeyboard()
    {

    }

    public void UpdateKeyboardPosition(Vector3 newPos)
    {
        //is rotation also needed?
        keyboard.transform.position = newPos;
    }

    public void ShowConfirmationPanel(string confirmationText, Action<bool> decision)
    {
        confirmationPanel.ShowPanel(confirmationText, decision);
        //always pops up in the middle (no moving position method)
        //also need to lock other activity until this is resolved 
    }


}