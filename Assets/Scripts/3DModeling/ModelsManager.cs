using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelsManager : MonoBehaviour
{
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

    public Material defaultMaterial;
    public Material highlightMaterial;
    public Material GetHighLightMaterial()
    {
        return highlightMaterial;
    }
    public Material GetDefaultMaterial()
    {
        return defaultMaterial;
    }

    public List<ModelData> models = new List<ModelData>();

    public void TrackModel(ModelData model)
    {
        models.Add(model);
    }
    public void UnTrackModel(ModelData model)
    {
        models.Remove(model);
    }
    public List<ModelData> GetAllModelsInScene()
    {
        return models;
    }

}
