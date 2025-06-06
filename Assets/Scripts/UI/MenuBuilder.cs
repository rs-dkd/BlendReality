using UnityEngine;
using UnityEngine.UI;

public class MenuBuilder : MonoBehaviour
{
    [Header("Menu Data")]
    public MenuItemData[] menuItems;

    [Header("Prefabs")]
    public GameObject menuItemPrefab;
    public GameObject separatorPrefab;

    [Header("Layout")]
    public RectTransform menuContent;
    public float itemSpacing = 10f;

    [Header("Icons")]
    public Sprite importIcon;
    public Sprite exportIcon;
    public Sprite materialIcon;
    public Sprite viewIcon;
    public Sprite toolIcon;
    public Sprite settingsIcon;

    void Start()
    {
        InitializeMenuItems();
        BuildMenu();
    }

    void InitializeMenuItems()
    {
        menuItems = new MenuItemData[]
        {
            new MenuItemData("Import Model", importIcon, false),
            new MenuItemData("Export Model", exportIcon, false),
            new MenuItemData("Material Settings", materialIcon, false),
            new MenuItemData("View Options", viewIcon, true),
            new MenuItemData("Tool Settings", toolIcon, false),
            new MenuItemData("Preferences", settingsIcon, false)
        };

        foreach (var item in menuItems)
        {
            if (item.isImplemented)
            {
                item.onSelect.AddListener(() => Debug.Log($"Selected: {item.itemName}"));
            }
        }
    }

    void BuildMenu()
    {
        foreach (Transform child in menuContent)
        {
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }

        foreach (var item in menuItems)
        {
            CreateMenuItem(item);
        }

        if (separatorPrefab != null)
            Instantiate(separatorPrefab, menuContent);
    }

    void CreateMenuItem(MenuItemData itemData)
    {
        if (menuItemPrefab == null)
        {
            Debug.LogError("Menu item prefab is not assigned!");
            return;
        }

        GameObject menuItem = Instantiate(menuItemPrefab, menuContent);
        MenuItemComponent component = menuItem.GetComponent<MenuItemComponent>();

        if (component != null)
        {
            component.SetupMenuItem(itemData);
        }
        else
        {
            Debug.LogError("MenuItemComponent not found on prefab!");
        }
    }
}