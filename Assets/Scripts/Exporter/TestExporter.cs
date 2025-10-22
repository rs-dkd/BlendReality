using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class TestExporter : MonoBehaviour
{
    public List<GameObject> objectsToExport;
    public string exportFilename = "MyExportedModel.obj";

    [ContextMenu("Execute Export")]
    public void TestExport()
    {
        if (objectsToExport == null || objectsToExport.Count == 0)
        {
            Debug.LogError("No GameObjects assigned to export!");
            return;
        }

        string exportPath = Path.Combine(Application.persistentDataPath, exportFilename);

        OBJExporter exporter = OBJExporter.Instance;

        ObjExportOptions options = new ObjExportOptions
        {
            applyTransforms = true,
            copyTextures = true
        };

        bool success = exporter.ExportToObj(objectsToExport, exportPath, options);

        if (success)
        {
            Debug.Log($"Successfully exported to: {exportPath}");
        }
        else
        {
            Debug.LogError("Export failed. Check the console for errors.");
        }
    }
}