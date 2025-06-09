using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ViewType
{
    Standard, Wireframe, Unlit, Clay
}
public class ViewManager : MonoBehaviour
{
    public static ViewManager Instance { get; private set; }

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

    public Material standardOverride;
    public Material wireframeOverride;
    public Material unlitOverride;
    public Material clayOverride;

    public TMPro.TMP_Dropdown viewDropdown;
    public ViewType currentViewType;
    public ViewType GetViewType()
    {
        return currentViewType;
    }
    public void ChangeView()
    {
        currentViewType = (ViewType)viewDropdown.value;
        List<ModelData> models = ModelsManager.Instance.GetAllModelsInScene();
        for (int i = 0; i < models.Count; i++)
        {
            ChangeViewForModel(models[i]);
        }
 
    }
    public void ChangeViewForModel(ModelData model)
    {
        if (currentViewType == ViewType.Standard)
        {
            model.SetOverrideMaterial(standardOverride);
        }
        if (currentViewType == ViewType.Wireframe)
        {
            model.SetOverrideMaterial(wireframeOverride);
        }
        if (currentViewType == ViewType.Clay)
        {
            model.SetOverrideMaterial(clayOverride);
        }
        if (currentViewType == ViewType.Unlit)
        {
            model.SetOverrideMaterial(unlitOverride);
        }

    }

}
