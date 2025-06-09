using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }



    public List<ModelData> selectedModels = new List<ModelData>();
    public void ClearSelection()
    {
        for (int i = 0; i < selectedModels.Count; i++)
        {
            selectedModels[i].UnSelectModel();
        }
        selectedModels.Clear();
    }

    public void SelectModel(ModelData model)
    {
        ClearSelection();
        selectedModels.Add(model);
        model.SelectModel();
    }

    public void RemoveModelFromSelection(ModelData model)
    {
        ClearSelection();
        selectedModels.Remove(model);
        model.UnSelectModel();
    }
    public void AddModelToSelection(ModelData model)
    {
        selectedModels.Add(model);
        model.SelectModel();
    }
}
