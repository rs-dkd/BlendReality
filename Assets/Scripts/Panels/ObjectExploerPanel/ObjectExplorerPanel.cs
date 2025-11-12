using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Shows all the models in the scene
/// Allows the user to select, delete and change name
/// </summary>
public class ObjectExplorerPanel : MonoBehaviour
{
    [Header("UI elements")]
    [Tooltip("Transform where to put the newly created objectsUIItems")]
    [SerializeField] private Transform objectsUIParent;
    [Tooltip("ObjectsUIItem Prefab")]
    [SerializeField] private GameObject objectsUIItemPrefab;

    private List<ObjectUIItem> objectUIItems = new List<ObjectUIItem>();

    /// <summary>
    /// Setup models listener
    /// </summary>
    private void Start()
    {
        ModelsManager.Instance.OnModelsChanged.AddListener(RefreshPanel);
    }
    /// <summary>
    /// Refresh the panel when tracked models change
    /// </summary>
    public void RefreshPanel(List<ModelData> models)
    {
        for (int i = 0;i< objectUIItems.Count; i++)
        {
            objectUIItems[i].DeleteUI();
        }
        objectUIItems.Clear();


        for (int i = 0;i<models.Count; i++)
        {
            GameObject newItemGO = Instantiate(objectsUIItemPrefab, objectsUIParent);
            ObjectUIItem newItem = newItemGO.GetComponent<ObjectUIItem>();
            newItem.Setup(models[i]);
            objectUIItems.Add(newItem);
        }

    }



}
