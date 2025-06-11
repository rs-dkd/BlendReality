//using UnityEngine;
//using System.Collections.Generic;
//public enum DockSide
//{
//    Left,
//    Right,
//    World
//}
//public class PanelManager : MonoBehaviour
//{
//    public static PanelManager Instance;

//    public Transform xrHead;

//    public Transform leftDockAnchor;
//    public Transform rightDockAnchor;

//    private BasePanel leftExpanded;
//    private BasePanel rightExpanded;

//    private List<BasePanel> leftDocked = new List<BasePanel>();
//    private List<BasePanel> rightDocked = new List<BasePanel>();

//    void Awake()
//    {
//        if (Instance == null) Instance = this;
//        else Destroy(gameObject);
//    }

//    public void DockPanel(BasePanel panel, DockSide side)
//    {
//        if (panel.dockSide == side) return;

//        Undock(panel);

//        panel.dockSide = side;
//        panel.Undock();

//        if (side == DockSide.Left)
//        {
//            leftDocked.Add(panel);
//            ExpandPanel(panel, DockSide.Left);
//            //PositionDockedPanels(leftDocked, leftDockAnchor);
//        }
//        else if (side == DockSide.Right)
//        {
//            rightDocked.Add(panel);
//            ExpandPanel(panel, DockSide.Right);
//            //PositionDockedPanels(rightDocked, rightDockAnchor);
//        }
//    }

//    public void DockToWorld(BasePanel panel)
//    {
//        Undock(panel);
//        panel.dockSide = DockSide.World;
//        panel.DockPanel(panel.transform.position, panel.transform.rotation);
//    }

//    private void Undock(BasePanel panel)
//    {
//        if (leftDocked.Contains(panel)) leftDocked.Remove(panel);
//        if (rightDocked.Contains(panel)) rightDocked.Remove(panel);
//    }
//    private void MinimizePanel(BasePanel panel, DockSide side)
//    {
//        if (side == DockSide.Left && leftExpanded == panel)
//        {
//            leftExpanded.ToggleMinimize();
//            leftExpanded = null;
//        }

//        if (side == DockSide.Right && rightExpanded == panel)
//        {
//            rightExpanded.ToggleMinimize();
//            rightExpanded = null;

//        }
    
//        UpdateAllViews();
//    }
//    private void ExpandPanel(BasePanel panel, DockSide side)
//    {
//        if (side == DockSide.Left && leftExpanded != null)
//            leftExpanded.ToggleMinimize();

//        if (side == DockSide.Right && rightExpanded != null)
//            rightExpanded.ToggleMinimize();

//        panel.isMinimized = false;
//        panel.OpenPanel();

//        if (side == DockSide.Left)
//            leftExpanded = panel;
//        else
//            rightExpanded = panel;

//        UpdateAllViews();
//    }

//    public void PanelTitleClicked(BasePanel panel)
//    {
//        if (panel.isMinimized)
//            ExpandPanel(panel, panel.dockSide);
//    }


//    private void UpdateAllViews()
//    {
//        foreach (var p in leftDocked.Concat(rightDocked))
//        {
//            p.UpdateView();
//        }
//    }
//}