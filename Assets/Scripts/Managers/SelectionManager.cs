using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class SelectionChangedEvent : UnityEvent<List<ModelData>> { }
/// <summary>
/// Manages the models that are selected and deselected
/// </summary>
public class SelectionManager : MonoBehaviour
{
    /// <summary>
    /// Singleton Pattern
    /// </summary>
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

    //Event called when selected models is updated
    public SelectionChangedEvent OnSelectionChanged = new SelectionChangedEvent();


    private List<ModelData> selectedModels = new List<ModelData>();


    /// <summary>
    /// Get selected models
    /// </summary>
    public List<ModelData> GetSelectedModels()
    {
        return selectedModels;
    }
    /// <summary>
    /// Clears all selected models
    /// </summary>
    public void ClearSelection()
    {
        for (int i = 0; i < selectedModels.Count; i++)
        {
            selectedModels[i].UnSelectModel();
        }
        selectedModels.Clear();
        OnSelectionChanged.Invoke(selectedModels);
    }
    /// <summary>
    /// Unselects all models and selects new model
    /// </summary>
    public void SelectModel(ModelData model)
    {
        for (int i = 0; i < selectedModels.Count; i++)
        {
            selectedModels[i].UnSelectModel();
        }
        selectedModels.Clear();

        selectedModels.Add(model);
        model.SelectModel();
        OnSelectionChanged.Invoke(selectedModels);
    }
    /// <summary>
    /// Removes model from selection
    /// </summary>
    public void RemoveModelFromSelection(ModelData model)
    {
        selectedModels.Remove(model);
        model.UnSelectModel();
        OnSelectionChanged.Invoke(selectedModels);
    }
    /// <summary>
    /// Adds model from selection
    /// </summary>
    public void AddModelToSelection(ModelData model)
    {
        selectedModels.Add(model);
        model.SelectModel();
        OnSelectionChanged.Invoke(selectedModels);
    }
    /// <summary>
    /// Gets the first selected model in selection
    /// </summary>
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
