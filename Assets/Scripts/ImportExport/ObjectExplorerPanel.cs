using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectExplorerPanel : MonoBehaviour
{
    public Transform objectsUIParent;
    public GameObject objectsUIItemPrefab;

    public List<ObjectUIItem> objectUIItems = new List<ObjectUIItem>();


    private void Start()
    {
        ModelsManager.Instance.OnModelsChanged.AddListener(RefreshPanel);
    }

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
