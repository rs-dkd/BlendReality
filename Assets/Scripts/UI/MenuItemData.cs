using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class MenuItemData
{
    public string itemName;
    public Sprite icon;
    public UnityEvent onSelect;
    public bool isImplemented = false;

    public MenuItemData(string name, Sprite iconSprite, bool implemented = false)
    {
        itemName = name;
        icon = iconSprite;
        isImplemented = implemented;
        onSelect = new UnityEvent();
    }
}