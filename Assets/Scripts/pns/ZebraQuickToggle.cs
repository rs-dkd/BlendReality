using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple quick toggle for zebra analysis.
/// Attach this to any Toggle UI element for instant zebra on/off control.
/// </summary>
[RequireComponent(typeof(Toggle))]
public class ZebraQuickToggle : MonoBehaviour
{
    private Toggle toggle;

    [Header("Optional Settings")]
    [SerializeField] private bool applyToAllModels = false;
    [SerializeField] private bool showSettingsOnEnable = false;
    [SerializeField] private GameObject settingsPanel;

    private void Awake()
    {
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    private void Start()
    {
        if (ZebraAnalysisManager.Instance != null)
        {
            toggle.SetIsOnWithoutNotify(ZebraAnalysisManager.Instance.IsZebraEnabled());
            ZebraAnalysisManager.Instance.OnZebraSettingsChanged.AddListener(UpdateToggleState);
        }
    }

    private void OnToggleChanged(bool enabled)
    {
        if (ZebraAnalysisManager.Instance == null)
        {
            Debug.LogError("ZebraAnalysisManager not found in scene!");
            toggle.SetIsOnWithoutNotify(false);
            return;
        }

        if (enabled)
        {
            ZebraAnalysisManager.Instance.EnableZebraAnalysis();

            if (showSettingsOnEnable && settingsPanel != null)
            {
                settingsPanel.SetActive(true);
            }
        }
        else
        {
            ZebraAnalysisManager.Instance.DisableZebraAnalysis();

            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
        }
    }

    private void UpdateToggleState()
    {
        if (ZebraAnalysisManager.Instance != null)
        {
            toggle.SetIsOnWithoutNotify(ZebraAnalysisManager.Instance.IsZebraEnabled());
        }
    }

    private void OnDestroy()
    {
        if (toggle != null)
        {
            toggle.onValueChanged.RemoveListener(OnToggleChanged);
        }

        if (ZebraAnalysisManager.Instance != null)
        {
            ZebraAnalysisManager.Instance.OnZebraSettingsChanged.RemoveListener(UpdateToggleState);
        }
    }
}