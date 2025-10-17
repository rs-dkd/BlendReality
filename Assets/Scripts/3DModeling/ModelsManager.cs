using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ModelsChangedEvent : UnityEvent<List<ModelData>> { }

public class ModelsManager : MonoBehaviour
{
    public static ModelsManager Instance { get; private set; }
    public ModelsChangedEvent OnModelsChanged = new ModelsChangedEvent();

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

    private int modelIndex;

    public List<ModelData> models = new List<ModelData>();

    public int TrackModel(ModelData model)
    {
        modelIndex++;
        models.Add(model);
        OnModelsChanged.Invoke(models);
        return modelIndex;
    }
    public void UnTrackModel(ModelData model)
    {
        if(model != null)models.Remove(model);
        OnModelsChanged.Invoke(models);
    }
    public List<ModelData> GetAllModelsInScene()
    {
        return models;
    }

}
