using UnityEngine;
using Dummiesman;

public class ObjLoader : MonoBehaviour
{
    [Header("Runtime OBJ Loader 配置")]
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

    public GameObject LoadObjAtRuntime(string objPath, string mtlPath = "")
    {
        if (string.IsNullOrEmpty(objPath))
        {
            Debug.LogError("OBJ is null！");
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
        go.name = "LoadedObj_" + go.name;

        AddMeshCollidersRecursively(go);

        DontDestroyOnLoad(go); //so the thing will not be destroyed

        return go;
    }
}