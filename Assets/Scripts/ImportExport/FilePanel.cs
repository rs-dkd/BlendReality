using SimpleFileBrowser;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using static SimpleFileBrowser.FileBrowser;

public class FilePanel : MonoBehaviour
{
    //public SimpleFileBrowser.FileBrowser fileBrowser;


    //public void LoadFile()
    //{
    //    SimpleFileBrowser.FileBrowser.ShowLoadDialog(OnLoadSuccess, OnCancel, PickMode.Files);
    //}
    //public void SaveFile()
    //{
    //    SimpleFileBrowser.FileBrowser.ShowSaveDialog(OnSaveSuccess, OnCancel, PickMode.Files);
    //}

    //public void OnLoadSuccess(string[] paths)
    //{
    //    Debug.Log(paths);
    //}
    //public void OnSaveSuccess(string[] paths)
    //{
    //    Debug.Log(paths);
    //}
    //public void OnCancel()
    //{

    //}


    public void SaveSelectedObjects()
    {
        if(SelectionManager.Instance.GetSelectedModels().Count == 0)
        {
            Debug.LogWarning("No models selected to export.");
            return;
        }
        else
        {
            //MeshExporter.MeshToFile();

        }
    }










    private const string RecentPathsKey = "RecentOBJPaths";
    private const int MaxRecent = 10;

    [Header("UI Controls - File Selection")]
    [SerializeField] private TMP_Dropdown fileDropdown;
    [SerializeField] private TMP_Text currentLoadPath;
    [SerializeField] private Button loadBlenderButton;

    [Header("UI Controls - Import Settings")]
    [SerializeField] private Toggle combineMeshesToggle;

    [Header("Import Settings")]
    private Vector3 defaultPosition = new Vector3(0, 1, 2);

    private List<string> filePaths = new List<string>();

    void Start()
    {
        LoadRecentPaths();
        SetupFileBrowser();
        SetupUI();
        RefreshDropdown();
        UpdateLoadButton();
    }

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

    private void SetupFileBrowser()
    {
        FileBrowser.SetFilters(true, new FileBrowser.Filter("3D Models", ".obj"));
        FileBrowser.SetDefaultFilter(".obj");
        FileBrowser.AddQuickLink("Users", "C:\\Users", null);
    }

    private void SetupUI()
    {
        // Dropdown
        if (fileDropdown != null)
            fileDropdown.onValueChanged.AddListener(OnDropdownChanged);

        // Combine mesh toggle
        if (combineMeshesToggle != null)
            combineMeshesToggle.onValueChanged.AddListener(OnCombineMeshesChanged);
    }

    public void OnPickFilesButton()
    {
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
                RefreshDropdown();
                SaveRecentPaths();

                if (fileDropdown != null)
                {
                    fileDropdown.value = 0;
                }
                UpdateCurrentPathDisplay();
                UpdateLoadButton();

                Debug.Log("Added to import list: " + Path.GetFileName(fullPath));
            },
            () => { Debug.Log("File selection canceled"); },
            FileBrowser.PickMode.Files,
            false,
            null,
            "Select OBJ File",
            "Select"
        );
    }

    private void AddToRecentPaths(string fullPath)
    {
        if (filePaths.Contains(fullPath))
            filePaths.Remove(fullPath);

        filePaths.Insert(0, fullPath);

        if (filePaths.Count > MaxRecent)
            filePaths.RemoveRange(MaxRecent, filePaths.Count - MaxRecent);
    }

    private void SaveRecentPaths()
    {
        PlayerPrefs.SetString(RecentPathsKey, string.Join("|", filePaths.ToArray()));
        PlayerPrefs.Save();
    }

    private void RefreshDropdown()
    {
        if (fileDropdown == null) return;

        fileDropdown.ClearOptions();
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

        foreach (string path in filePaths)
        {
            string nameOnly = Path.GetFileNameWithoutExtension(path);
            options.Add(new TMP_Dropdown.OptionData(nameOnly));
        }

        fileDropdown.AddOptions(options);
        fileDropdown.RefreshShownValue();
    }

    private void UpdateCurrentPathDisplay()
    {
        if (currentLoadPath == null) return;

        if (filePaths.Count > 0 && fileDropdown != null && fileDropdown.value < filePaths.Count)
        {
            string selectedPath = filePaths[fileDropdown.value];
            currentLoadPath.text = "Current: " + Path.GetFileName(selectedPath);
        }
        else
        {
            currentLoadPath.text = "Current: (none selected)";
        }
    }

    private void UpdateLoadButton()
    {
        if (loadBlenderButton == null) return;

        bool canLoad = filePaths.Count > 0 && fileDropdown != null && fileDropdown.value < filePaths.Count;
        loadBlenderButton.interactable = canLoad;
    }

    private void OnDropdownChanged(int index)
    {
        UpdateCurrentPathDisplay();
        UpdateLoadButton();


    }

    private void OnCombineMeshesChanged(bool value)
    {
        Debug.Log($"Combine meshes set to: {value}");
    }

    public void OnLoadModelingSceneButton()
    {
        if (fileDropdown == null || filePaths.Count == 0)
        {
            Debug.LogError("No valid file selected for import");
            return;
        }

        int index = fileDropdown.value;
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

        MeshImporter.Instance.LoadFile(Path.GetFileNameWithoutExtension(objPath), objPath, combineMeshesToggle != null ? combineMeshesToggle.isOn : false);

        //// Store import data in static var for VR scene
        //OBJImportHandler.PendingImport = new StoredImportData
        //{
        //    fileName = Path.GetFileNameWithoutExtension(objPath),
        //    filePath = objPath,
        //    combineMultipleMeshes = combineMeshesToggle != null ? combineMeshesToggle.isOn : false
        //};

        ////Debug.Log($"Stored pending import: {OBJImportHandler.PendingImport.fileName}");

        ////SceneManager.LoadScene("ModelingScene");
    }

    public void ResetImportSettings()
    {
        if (combineMeshesToggle != null) combineMeshesToggle.isOn = false;
    }
}
