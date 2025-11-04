using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Stores the text and toggle for a toggle item in a toggle group
/// </summary>
public class ToggleItem : MonoBehaviour
{
    // --- Variables ---
    [Header("Toggle")]
    [Tooltip("UI Text.")]
    [SerializeField] private TMP_Text text;
    [Tooltip("UI Toggle.")]
    [SerializeField] private Toggle toggle;

    // --- Functions ---
    /// <summary>
    /// Getter for text
    /// </summary>
    public string GetText()
    {
        return text.text;
    }
    /// <summary>
    /// Setter text
    /// </summary>
    public void SetText(string _text)
    {
        text.text = _text;
    }
    /// <summary>
    /// Getter Toggle
    /// </summary>
    public Toggle GetToggle()
    {
        return toggle;
    }
}
