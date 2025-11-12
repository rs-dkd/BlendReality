using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ModelsChangedEvent : UnityEvent<List<ModelData>> { }
/// <summary>
/// Tracks and untracks all the models in the scene
/// </summary>
public class ModelsManager : MonoBehaviour
{
    /// <summary>
    /// Singleton Pattern
    /// </summary>
    public static ModelsManager Instance { get; private set; }
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

    //Event called when tracked models is updated
    public ModelsChangedEvent OnModelsChanged = new ModelsChangedEvent();


    private int modelIndex;
    private List<ModelData> models = new List<ModelData>();

    /// <summary>
    /// Called when inported or created model
    /// Adds model to tracked models
    /// </summary>
    public int TrackModel(ModelData model)
    {
        modelIndex++;
        models.Add(model);
        OnModelsChanged.Invoke(models);
        return modelIndex;
    }
    /// <summary>
    /// Called from delete model
    /// Remove model from tracked models
    /// </summary>
    public void UnTrackModel(ModelData model)
    {
        if(model != null)models.Remove(model);
        OnModelsChanged.Invoke(models);
    }
    /// <summary>
    /// Get all models in the scene
    /// </summary>
    public List<ModelData> GetAllModelsInScene()
    {
        return models;
    }

}
