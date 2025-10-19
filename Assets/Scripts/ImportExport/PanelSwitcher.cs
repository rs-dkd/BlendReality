using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelSwitcher : MonoBehaviour
{
    public ToggleGroupUI toggleGroupUI;
    public string[] options = { "Load", "Save", "Settings", "Statistics" };
    public GameObject[] optionPanels;

    void Start()
    {
        toggleGroupUI.Setup(options);
        toggleGroupUI.OnToggleGroupChanged.AddListener(OnPanelChanged);
    }

    public void OnPanelChanged(Toggle toggle)
    {
        for (int i = 0; i < options.Length; i++)
        {
            if (toggle.name == options[i])
            {
                optionPanels[i].SetActive(true);
            }
            else
            {
                optionPanels[i].SetActive(false);
            }
        }
    }

}
