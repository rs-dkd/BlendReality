using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;



public class ToggleGroupChangedEvent : UnityEvent<Toggle> { }
public class ToggleGroupUI : MonoBehaviour
{
    public string[] options;
    public ToggleGroup toggleGroup;
    public Transform optionsParent;
    public GameObject togglePrefab;
    public ToggleGroupChangedEvent OnToggleGroupChanged = new ToggleGroupChangedEvent();
    public bool allowOff = false;
    public bool createOnAwake = true;
    public void OnToggleSelected(Toggle toggle)
    {
        if (toggle.isOn)
        {
            OnToggleGroupChanged.Invoke(toggle);
        }
    }

    private void Awake()
    {
        if (createOnAwake)
        {
            Setup();
        }
    }
    public void Setup(string[] _options = null) { 
        if(_options != null)
        {
            options = _options;
        }

        Toggle activeToggle = null;
        toggleGroup.allowSwitchOff = allowOff;
        foreach (String value in options)
        {
            GameObject toggleInstance = Instantiate(togglePrefab, optionsParent);
            toggleInstance.name = value.ToString(); 

            ToggleItem toggleItem = toggleInstance.GetComponent<ToggleItem>();
            toggleItem.toggle.group = toggleGroup;
            toggleItem.text.text = value;

            if (activeToggle == null) activeToggle = toggleItem.toggle;

            toggleItem.toggle.onValueChanged.AddListener((isOn) => {
                OnToggleSelected(toggleItem.toggle);
            });
        }

        if(allowOff == false)
        {
            activeToggle.isOn = true;
            OnToggleSelected(activeToggle);
        }
        

    }
}
