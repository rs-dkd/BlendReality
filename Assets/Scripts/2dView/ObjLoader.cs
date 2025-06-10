using UnityEngine;
using Dummiesman;

public class ObjLoader : MonoBehaviour
{
    [Header("Runtime OBJ Loader for Import")]
    [Tooltip(".obj file path")]
    public string objFilePath;
    [Tooltip(".mtl file path")]
    public string mtlFilePath = "";

    private void AddMeshCollidersRecursively(GameObject root)
    {
        MeshFilter[] meshFilters = root.GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter mf in meshFilters)
        {
            if (mf.sharedMesh != null)
            {
                MeshCollider mc = mf.gameObject.AddComponent<MeshCollider>();
                mc.sharedMesh = mf.sharedMesh;
            }
        }
    }

    public GameObject LoadObjForImport(string objPath, string mtlPath = "")
    {
        if (string.IsNullOrEmpty(objPath))
        {
            Debug.LogError("OBJ path is null!");
            return null;
        }

        GameObject go = string.IsNullOrEmpty(mtlPath)
            ? new OBJLoader().Load(objPath)
            : new OBJLoader().Load(objPath, mtlPath);

        if (go == null)
        {
            Debug.LogError($"OBJ load failed: {objPath}");
            return null;
        }

        go.transform.position = Vector3.zero;
        go.transform.rotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        go.name = "ImportedObj_" + go.name;

        AddMeshCollidersRecursively(go);

        return go;
    }

    public GameObject LoadObjAtRuntime(string objPath, string mtlPath = "")
    {
        GameObject go = LoadObjForImport(objPath, mtlPath);
        if (go != null)
        {
            DontDestroyOnLoad(go); // Only relevant for non object import stuff
        }
        return go;
    }
}