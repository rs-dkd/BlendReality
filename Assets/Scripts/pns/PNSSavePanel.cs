using UnityEngine;
using System.IO;

/// <summary>
/// Export functionality within Settings
/// the current model's PnS representation as BV / IGES / STEP.
/// </summary>
public class PNSSavePanel : MonoBehaviour
{
    [Header("Export Settings")]
    [Tooltip("Base directory for exports. If empty, Application.persistentDataPath/PNSExports is used.")]
    public string exportDirectory = "";

    /// <summary>
    /// Get the first selected model in the scene.
    /// </summary>
    private ModelData GetCurrentModel()
    {
        if (SelectionManager.Instance == null) return null;
        return SelectionManager.Instance.GetFirstSelected();
    }

    private string EnsureDirectory()
    {
        string dir = exportDirectory;
        if (string.IsNullOrEmpty(dir))
        {
            dir = Path.Combine(Application.persistentDataPath, "PNSExports");
        }

        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        return dir;
    }

    private string BuildOutputPath(ModelData model, string extension)
    {
        string dir = EnsureDirectory();
        string name = string.IsNullOrEmpty(model.modelName) ? "Model" : model.modelName;
        return Path.Combine(dir, $"{name}_pns.{extension}");
    }

    public void SaveCurrentAsBV()
    {
        var model = GetCurrentModel();
        if (model == null)
        {
            Debug.LogWarning("PNS Export: No selected model for BV export.");
            return;
        }

        string path = BuildOutputPath(model, "bv");
        model.ExportToBV(path);
        Debug.Log($"PNS Export: BV saved to {path}");
    }

    public void SaveCurrentAsIGES()
    {
        var model = GetCurrentModel();
        if (model == null)
        {
            Debug.LogWarning("PNS Export: No selected model for IGES export.");
            return;
        }

        string path = BuildOutputPath(model, "igs");
        model.ExportToIGS(path);
        Debug.Log($"PNS Export: IGES saved to {path}");
    }

    public void SaveCurrentAsSTEP()
    {
        var model = GetCurrentModel();
        if (model == null)
        {
            Debug.LogWarning("PNS Export: No selected model for STEP export.");
            return;
        }

        string path = BuildOutputPath(model, "step");
        model.ExportToSTEP(path);
        Debug.Log($"PNS Export: STEP saved to {path}");
    }
}
