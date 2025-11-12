using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


/// <summary>
/// Event triggered when a toggle in the group is changed
/// </summary>
public class ToggleGroupChangedEvent : UnityEvent<Toggle> { }
/// <summary>
/// Controls creating a list of toggles and managing their events
/// </summary>
public class ToggleGroupUI : MonoBehaviour
{
    // --- Variables ---

    [Header("Key Configuration")]
    [Tooltip("List of options to create toggles from")]
    [SerializeField] private string[] options;
    [Tooltip("Toggle Group")]
    [SerializeField] private ToggleGroup toggleGroup;
    [Tooltip("Parent transform of the options items")]
    [SerializeField] private Transform optionsParent;
    [Tooltip("Toggle Item Prefab")]
    [SerializeField] private GameObject togglePrefab;
    [Tooltip("On Toggle Event: Called when selected toggle is changed")]
    public ToggleGroupChangedEvent OnToggleGroupChanged = new ToggleGroupChangedEvent();
    [Tooltip("Force one to be active or not")]
    [SerializeField] private bool allowOff = false;
    [Tooltip("Automatically create options on awake or allow creation to be handled by script")]
    [SerializeField] private bool createOnAwake = true;



    /// <summary>
    /// Allow for the creation of the options on awake or by calling setup
    /// </summary>
    private void Awake()
    {
        if (createOnAwake)
        {
            Setup();
        }
    }
    /// <summary>
    /// Main function: handles creating the toggles from the options list and the events
    /// </summary>
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
            toggleItem.GetToggle().group = toggleGroup;
            toggleItem.SetText(value);

            if (activeToggle == null) activeToggle = toggleItem.GetToggle();

            toggleItem.GetToggle().onValueChanged.AddListener((isOn) => {
                OnToggleSelected(toggleItem.GetToggle());
            });

 
        }

        if(allowOff == false)
        {
            activeToggle.isOn = true;
            OnToggleSelected(activeToggle);
        }
        

    }
    /// <summary>
    /// Invokes the on toggle changed event when a toggle is selected
    /// </summary>
    public void OnToggleSelected(Toggle toggle)
    {
        if (toggle.isOn)
        {
            OnToggleGroupChanged.Invoke(toggle);
        }
    }
}
