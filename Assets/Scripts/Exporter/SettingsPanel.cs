using UnityEngine;
using TMPro;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class SettingsPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject mainSettingsContent;
    [SerializeField] private GameObject saveDialogPanel;
    [SerializeField] private TMP_InputField filenameInput;

    void Start()
    {
        if (saveDialogPanel != null)
        {
            saveDialogPanel.SetActive(false);
        }
    }

    public void OnExportButtonClicked()
    {
        ModelData selectedModel = SelectionManager.Instance.GetFirstSelected();
        string defaultName = (selectedModel != null) ? SanitizeForFilename(selectedModel.name) : "MyExportedModel";

        filenameInput.text = defaultName;
        saveDialogPanel.SetActive(true);
        mainSettingsContent.SetActive(false);
        VRKeyboard.Instance.BindKeyboard(filenameInput, saveDialogPanel.transform, false);
        filenameInput.ActivateInputField();
    }

    public void ConfirmExport()
    {
        List<ModelData> selectedModels = SelectionManager.Instance.GetSelectedModels();
        if (selectedModels == null || selectedModels.Count == 0)
        {
            Debug.LogWarning("Export cancelled: No models are selected.");
            CancelExport();
            return;
        }

        string filename = filenameInput.text;
        if (string.IsNullOrWhiteSpace(filename))
        {
            filename = "DefaultExport";
        }

        if (!filename.EndsWith(".obj", System.StringComparison.OrdinalIgnoreCase))
        {
            filename += ".obj";
        }

        List<GameObject> gameObjectsToExport = selectedModels.Select(model => model.gameObject).ToList();
        string exportPath = Path.Combine(Application.persistentDataPath, filename);
        ObjExportOptions options = new ObjExportOptions { applyTransforms = true, copyTextures = true };

        bool success = OBJExporter.Instance.ExportToObj(gameObjectsToExport, exportPath, options);

        if (success)
        {
            Debug.Log($"Model exported successfully to {exportPath}");
        }
        else
        {
            Debug.LogError($"Failed to export model to {exportPath}");
        }

        saveDialogPanel.SetActive(false);
        mainSettingsContent.SetActive(true);
        VRKeyboard.Instance.UnbindKeyboard();
    }
    public void CancelExport()
    {
        saveDialogPanel.SetActive(false);
        mainSettingsContent.SetActive(true);
        VRKeyboard.Instance.UnbindKeyboard();
    }

    private string SanitizeForFilename(string name)
    {
        return Path.GetInvalidFileNameChars().Aggregate(name, (current, c) => current.Replace(c.ToString(), "_"));
    }
}