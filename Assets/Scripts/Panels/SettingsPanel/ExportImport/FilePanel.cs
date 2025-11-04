using SimpleFileBrowser;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The import and export panel in the settings
/// Manages both export and import
/// </summary>
public class FilePanel : MonoBehaviour
{

    [Header("UI Controls - File Selection")]
    [Tooltip("Load File Dropdown for recent paths")]
    [SerializeField] private TMP_Dropdown loadFileDropdown;
    [Tooltip("Save File Dropdown for recent paths")]
    [SerializeField] private TMP_Dropdown saveFileDropdown;
    [Tooltip("Selected Load Path Text")]
    [SerializeField] private TMP_Text currentLoadPath;
    [Tooltip("Selected Save Path Text")]
    [SerializeField] private TMP_Text currentSavePath;
    [Tooltip("Load the selected file button")]
    [SerializeField] private Button loadFileButton;
    [Tooltip("Save the selected file button")]
    [SerializeField] private Button saveFileButton;

    [Header("UI Controls - Import Settings")]
    [Tooltip("Combine models on load")]
    [SerializeField] private Toggle loadCombineMeshesToggle;
    [Tooltip("Combine models on save")]
    [SerializeField] private Toggle saveCombineMeshesToggle;
    [Tooltip("Save only the selected models")]
    [SerializeField] private Toggle saveSelectedOnlyToggle;


    //Recent paths
    private List<string> filePaths = new List<string>();
    private const string RecentPathsKey = "RecentOBJPaths";
    private const int MaxRecent = 4;


    /// <summary>
    /// Setup the file browser and load the recent paths
    /// </summary>
    void Start()
    {
        FileBrowser.SetFilters(true, new FileBrowser.Filter("3D Models", ".obj"));
        FileBrowser.SetDefaultFilter(".obj");
        FileBrowser.AddQuickLink("Users", "C:\\Users", null);

        if (loadFileDropdown != null)
            loadFileDropdown.onValueChanged.AddListener(UpdateLoadDropDownAndSelected);

        if (saveFileDropdown != null)
            saveFileDropdown.onValueChanged.AddListener(UpdateSaveDropDownAndSelected);

        LoadRecentPaths();
        UpdateLoadPanel();
        UpdateSavePanel();

    }
    /// <summary>
    /// Load the recent paths from player prefs
    /// </summary>
    private void LoadRecentPaths()
    {
        if (PlayerPrefs.HasKey(RecentPathsKey))
        {
            string saved = PlayerPrefs.GetString(RecentPathsKey);
            string[] paths = saved.Split('|');
            foreach (string entry in paths)
            {
                if (!string.IsNullOrEmpty(entry) && File.Exists(entry))
                    filePaths.Add(entry);
            }
        }
    }
    /// <summary>
    /// Add path to recent paths
    /// </summary>
    private void AddToRecentPaths(string fullPath)
    {
        if (filePaths.Contains(fullPath))
            filePaths.Remove(fullPath);

        filePaths.Insert(0, fullPath);

        if (filePaths.Count > MaxRecent)
            filePaths.RemoveRange(MaxRecent, filePaths.Count - MaxRecent);
    }
    /// <summary>
    /// Save the paths to player prefs
    /// </summary>
    private void SaveRecentPaths()
    {
        PlayerPrefs.SetString(RecentPathsKey, string.Join("|", filePaths.ToArray()));
        PlayerPrefs.Save();
    }



    /// <summary>
    /// Update the load panel
    /// </summary>
    private void UpdateLoadPanel()
    {
        if (loadFileDropdown == null) return;

        if (PlayerPrefs.HasKey(RecentPathsKey))
        {
            string saved = PlayerPrefs.GetString(RecentPathsKey);
            string[] paths = saved.Split('|');
            foreach (string entry in paths)
            {
                if (!string.IsNullOrEmpty(entry) && File.Exists(entry))
                    filePaths.Add(entry);
            }
        }


        loadFileDropdown.ClearOptions();
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

        foreach (string path in filePaths)
        {
            string nameOnly = Path.GetFileNameWithoutExtension(path);
            options.Add(new TMP_Dropdown.OptionData(nameOnly));
        }

        loadFileDropdown.AddOptions(options);
        loadFileDropdown.RefreshShownValue();


        UpdateLoadDropDownAndSelected(0);


    }
    /// <summary>
    /// Update the load dropdown - called after a new load file is selected
    /// </summary>
    private void UpdateLoadDropDownAndSelected(int index)
    {
        if (currentLoadPath == null) return;
        if (loadFileButton == null) return;

        if (filePaths.Count > 0 && loadFileDropdown != null && loadFileDropdown.value < filePaths.Count)
        {
            string selectedPath = filePaths[loadFileDropdown.value];
            currentLoadPath.text = "Current: " + Path.GetFileName(selectedPath);
        }
        else
        {
            currentLoadPath.text = "Current: (none selected)";
        }

        bool canLoad = filePaths.Count > 0 && loadFileDropdown != null && loadFileDropdown.value < filePaths.Count;
        loadFileButton.interactable = canLoad;
    }










    /// <summary>
    /// Update the save panel and its UI
    /// </summary>
    private void UpdateSavePanel()
    {
        if (saveFileDropdown == null) return;

        if (PlayerPrefs.HasKey(RecentPathsKey))
        {
            string saved = PlayerPrefs.GetString(RecentPathsKey);
            string[] paths = saved.Split('|');
            foreach (string entry in paths)
            {
                if (!string.IsNullOrEmpty(entry) && File.Exists(entry))
                    filePaths.Add(entry);
            }
        }


        saveFileDropdown.ClearOptions();
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

        foreach (string path in filePaths)
        {
            string nameOnly = Path.GetFileNameWithoutExtension(path);
            options.Add(new TMP_Dropdown.OptionData(nameOnly));
        }

        saveFileDropdown.AddOptions(options);
        saveFileDropdown.RefreshShownValue();


        UpdateSaveDropDownAndSelected(0);


    }

    /// <summary>
    /// Update the save dropdown to show resent save files
    /// </summary>
    private void UpdateSaveDropDownAndSelected(int index)
    {
        if (currentSavePath == null) return;
        if (saveFileButton == null) return;
   


        if (filePaths.Count > 0 && saveFileDropdown != null && saveFileDropdown.value < filePaths.Count)
        {
            string selectedPath = filePaths[saveFileDropdown.value];
            currentSavePath.text = "Current: " + Path.GetFileName(selectedPath);
        }
        else
        {
            currentSavePath.text = "Current: (none selected)";
        }

        bool canSave = filePaths.Count > 0 && saveFileDropdown != null && saveFileDropdown.value < filePaths.Count;
        saveFileButton.interactable = canSave;
    }


    /// <summary>
    /// Open Save File broswer pressed - show broswer and handle on success
    /// </summary>
    public void OnPickSaveFileButton()
    {
        PanelManager.Instance.ToggleAllPanels();
        FileBrowser.ShowSaveDialog(
            (paths) =>
            {

                string fullPath = paths[0];
                if (!fullPath.ToLower().EndsWith(".obj"))
                {
                    Debug.LogWarning("Selected file is not an OBJ file");
                    return;
                }

                AddToRecentPaths(fullPath);
                SaveRecentPaths();
                UpdateSavePanel();

                Debug.Log("Added to import list: " + Path.GetFileName(fullPath));
                PanelManager.Instance.ToggleAllPanels();
            },
            () => {
                Debug.Log("File selection canceled");
                PanelManager.Instance.ToggleAllPanels();

            },
            FileBrowser.PickMode.Files,
            false,
            null,
            "Save OBJ File",
            "Save"
        );
    }
    /// <summary>
    /// Open Load File broswer pressed - show broswer and handle on success
    /// </summary>
    public void OnPickLoadFileButton()
    {
        PanelManager.Instance.ToggleAllPanels();
        FileBrowser.ShowLoadDialog(
            (paths) =>
            {
                string fullPath = paths[0];
                if (!fullPath.ToLower().EndsWith(".obj"))
                {
                    Debug.LogWarning("Selected file is not an OBJ file");
                    return;
                }

                AddToRecentPaths(fullPath);
                SaveRecentPaths();



                UpdateLoadPanel();

                Debug.Log("Added to import list: " + Path.GetFileName(fullPath));
                PanelManager.Instance.ToggleAllPanels();
            },
            () => {
                Debug.Log("File selection canceled");
                PanelManager.Instance.ToggleAllPanels();

            },
            FileBrowser.PickMode.Files,
            false,
            null,
            "Select OBJ File",
            "Select"
        );
    }


    /// <summary>
    /// Save button pressed - save the file
    /// </summary>
    public void OnSaveButton()
    {
        if (saveFileDropdown == null || filePaths.Count == 0)
        {
            Debug.LogError("No valid file selected for import");
            return;
        }

        int index = saveFileDropdown.value;
        if (index < 0 || index >= filePaths.Count)
        {
            Debug.LogError("Invalid file index selected");
            return;
        }

        string objPath = filePaths[index];

        if (File.Exists(objPath))
        {
            // TODO : warn user that file will be overwritten
        }

        List<ModelData> models = ModelsManager.Instance.GetAllModelsInScene();

        if (models.Count == 0) return;

        if (saveSelectedOnlyToggle.isOn)
        {
            models = SelectionManager.Instance.GetSelectedModels();

        }

        MeshExporter.MeshToFile(models, objPath, Path.GetFileName(objPath), saveCombineMeshesToggle.isOn);
    }

    /// <summary>
    /// Load button pressed - load the file
    /// </summary>
    public void OnLoadButton()
    {
        if (loadFileDropdown == null || filePaths.Count == 0)
        {
            Debug.LogError("No valid file selected for import");
            return;
        }

        int index = loadFileDropdown.value;
        if (index < 0 || index >= filePaths.Count)
        {
            Debug.LogError("Invalid file index selected");
            return;
        }

        string objPath = filePaths[index];

        if (!File.Exists(objPath))
        {
            Debug.LogError($"File not found: {objPath}");
            return;
        }

        MeshImporter.Instance.LoadFile(Path.GetFileNameWithoutExtension(objPath), objPath, loadCombineMeshesToggle != null ? loadCombineMeshesToggle.isOn : false);

    }





}
