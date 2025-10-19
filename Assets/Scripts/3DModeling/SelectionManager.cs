using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class SelectionChangedEvent : UnityEvent<List<ModelData>> { }

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance { get; private set; }
    public SelectionChangedEvent OnSelectionChanged = new SelectionChangedEvent();

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

    public List<ModelData> GetSelectedModels()
    {
        return selectedModels;
    }
    public void ClearSelection()
    {
        for (int i = 0; i < selectedModels.Count; i++)
        {
            selectedModels[i].UnSelectModel();
        }
        selectedModels.Clear();
        OnSelectionChanged.Invoke(selectedModels);
    }

    public void SelectModel(ModelData model)
    {
        ClearSelection();
        selectedModels.Add(model);
        model.SelectModel();
        OnSelectionChanged.Invoke(selectedModels);
    }

    public void RemoveModelFromSelection(ModelData model)
    {
        ClearSelection();
        selectedModels.Remove(model);
        model.UnSelectModel();
        OnSelectionChanged.Invoke(selectedModels);
    }
    public void AddModelToSelection(ModelData model)
    {
        selectedModels.Add(model);
        model.SelectModel();
        OnSelectionChanged.Invoke(selectedModels);
    }

    public ModelData GetFirstSelected()
    {
        if(selectedModels.Count > 0)
        {
            return selectedModels[0];
        }
        else
        {
            return null;
        }
    }
}
