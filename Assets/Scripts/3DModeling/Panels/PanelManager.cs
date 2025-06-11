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

    public Transform playerPanelsHolder;
    public Transform worldPanelsHolder;

    public Transform GetPlayerPanelsHolder()
    {
        return playerPanelsHolder;
    }
    public Transform GetWorldPlanelsHolder()
    {
        return worldPanelsHolder;
    }
}