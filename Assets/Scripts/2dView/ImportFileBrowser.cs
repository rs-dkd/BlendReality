using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SimpleFileBrowser;
using TMPro;
using System.IO;
using UnityEngine.UI;

public class ImportFileBrowser : MonoBehaviour
{
    private const string RecentPathsKey = "RecentOBJPaths";
    private const int MaxRecent = 10;

    [SerializeField] private ObjLoader objLoader;

    [SerializeField] private TMP_Dropdown fileDropdown;
    [SerializeField] private TMP_Text currentLoadPath;
    [SerializeField] private Button loadBlenderButton;

    private List<string> filePaths = new List<string>();

    void Start()
    {
        // loading local pref
        if (PlayerPrefs.HasKey(RecentPathsKey))
        {
            string saved = PlayerPrefs.GetString(RecentPathsKey);
            foreach (var entry in saved.Split('|'))
            {
                if (!string.IsNullOrEmpty(entry))
                    filePaths.Add(entry);
            }
        }

        RefreshDropdown();

        //filter to be obj
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Models", ".obj"));
        FileBrowser.SetDefaultFilter(".obj");
        FileBrowser.AddQuickLink("Users", "C:\\Users", null);

        fileDropdown.onValueChanged.AddListener(OnDropdownChanged);

        if (filePaths.Count > 0)
        {
            fileDropdown.value = 0;
            currentLoadPath.text = "currentpath: " + filePaths[0];
            Debug.Log(filePaths[0]);
            loadBlenderButton.interactable = true;
        }
        else
        {
            currentLoadPath.text = "currentpath: (none)";
            loadBlenderButton.interactable = false;
        }
    }

    public void OnPickFilesButton()
    {
        FileBrowser.ShowLoadDialog(
            (paths) =>
            {
                string fullPath = paths[0];
                if (!fullPath.ToLower().EndsWith(".obj")) return;

                if (filePaths.Contains(fullPath))
                    filePaths.Remove(fullPath);
                filePaths.Insert(0, fullPath);

                if (filePaths.Count > MaxRecent)
                    filePaths.RemoveRange(MaxRecent, filePaths.Count - MaxRecent);

                RefreshDropdown();

                PlayerPrefs.SetString(RecentPathsKey, string.Join("|", filePaths));
                PlayerPrefs.Save();

                fileDropdown.value = 0;
                currentLoadPath.text = "currentpath: " + filePaths[0];
                loadBlenderButton.interactable = true;

                Debug.Log("Added: " + fullPath);
            },
            () => { Debug.Log("User canceled"); },
            FileBrowser.PickMode.Files,
            false,
            null,
            "Select Files",
            "Open"
        );
    }

    private void RefreshDropdown()
    {
        fileDropdown.ClearOptions();
        var options = new List<TMP_Dropdown.OptionData>();
        foreach (var path in filePaths)
        {
            string nameOnly = Path.GetFileNameWithoutExtension(path);
            options.Add(new TMP_Dropdown.OptionData(nameOnly));
        }
        fileDropdown.AddOptions(options);
        fileDropdown.RefreshShownValue();
    }

    private void OnDropdownChanged(int index)
    {
        if (index < 0 || index >= filePaths.Count) return;
        currentLoadPath.text = "currentpath: " + filePaths[index];
        loadBlenderButton.interactable = true;
    }

    public void OnLoadBlenderSceneButton()
    {
        int index = fileDropdown.value;
        if (index < 0 || index >= filePaths.Count) return;

        string objPath = filePaths[index];

        GameObject loaded = objLoader.LoadObjAtRuntime(objPath);

        if (loaded != null)
        {
            Debug.Log("OBJ imported");

            //anonymous function because there's no handlers on vr scene
            SceneManager.sceneLoaded += (scene, mode) =>
            {
                GameObject parent = GameObject.Find("LoadedObj");
                if (parent != null)
                {
                    loaded.transform.SetParent(parent.transform, true);
                    Debug.Log("OBJ loaded");
                }
                else
                {
                    Debug.LogWarning("cant find LoadedObj");
                }

                SceneManager.sceneLoaded -= (scene2, mode2) => { };
            };

            // 切换场景
            SceneManager.LoadScene("BlenderScene");
        }
        else
        {
            Debug.LogError("obj import failed");
        }
    }
}