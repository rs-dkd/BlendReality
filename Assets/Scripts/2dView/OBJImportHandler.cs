using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.SceneManagement;
using Dummiesman;

[System.Serializable]
public class StoredImportData
{
    public string fileName;
    public string filePath;
    public bool combineMultipleMeshes;
}

[System.Serializable]
public class RawMeshData
{
    public string name;
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;
    public Vector3[] normals;
    public Material[] materials;

    public RawMeshData(string meshName, Mesh mesh, Material[] meshMaterials)
    {
        name = meshName;
        vertices = mesh.vertices;
        triangles = mesh.triangles;
        uvs = mesh.uv;
        normals = mesh.normals;
        materials = meshMaterials;
    }
}

public class OBJImportHandler : MonoBehaviour
{
    public static OBJImportHandler Instance { get; private set; }

    [Header("Import Settings")]
    private Vector3 defaultImportPosition = new Vector3(0, 1, 2);
    private Vector3 defaultImportScale = Vector3.one;
    public bool combineMultipleMeshes = false;

    [Header("Imported Object Storage")]
    private List<RawMeshData> importedObjects = new List<RawMeshData>();

    public static StoredImportData PendingImport = null;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        StartCoroutine(ProcessPendingImport());
    }

    private IEnumerator ProcessPendingImport()
    {
        yield return new WaitForSeconds(0.5f);

        if (PendingImport != null)
        {
            Debug.Log($"Processing pending import: {PendingImport.fileName}");

            StoredImportData importData = PendingImport;

            GameObject tempObj = LoadObjDirectly(importData.filePath);

            if (tempObj != null)
            {
                ProcessImportedObjectDirect(tempObj, importData.fileName, importData.combineMultipleMeshes);
            }
            PendingImport = null;

            yield return new WaitForSeconds(0.2f);
            NotifyModelCreator();
        }
        else
        {
            Debug.Log("No pending import found");
        }
    }

    private GameObject LoadObjDirectly(string objPath)
    {
        try
        {
            GameObject go = new OBJLoader().Load(objPath);
            if (go != null)
            {
                go.name = "TempImportedObj";
                Debug.Log($"Successfully loaded OBJ: {objPath}");
                return go;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load OBJ: {e.Message}");
        }

        return null;
    }

    public void ProcessImportedObjectDirect(GameObject importedObj, string baseName, bool combineMultipleMeshes)
    {
        Debug.Log($"Processing imported object: {baseName}");

        MeshFilter[] meshFilters = importedObj.GetComponentsInChildren<MeshFilter>();

        if (meshFilters.Length == 0)
        {
            Debug.LogWarning($"No meshes found in imported object: {baseName}");
            Destroy(importedObj);
            return;
        }

        Debug.Log($"Found {meshFilters.Length} mesh filters. Combine setting: {combineMultipleMeshes}");

        if (combineMultipleMeshes && meshFilters.Length > 1)
        {
            ProcessCombinedMeshes(meshFilters, baseName);
        }
        else
        {
            foreach (MeshFilter meshFilter in meshFilters)
            {
                if (meshFilter.sharedMesh != null)
                {
                    StoreImportedMesh(meshFilter.sharedMesh, meshFilter.name, meshFilter.GetComponent<MeshRenderer>());
                }
            }
        }

        Debug.Log($"Total objects stored after processing: {importedObjects.Count}");

        Destroy(importedObj);
    }

    private void ProcessCombinedMeshes(MeshFilter[] meshFilters, string baseName)
    {
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        for (int i = 0; i < meshFilters.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combine);
        combinedMesh.name = baseName + "_Combined";

        Material[] materials = null;
        MeshRenderer firstRenderer = meshFilters[0].GetComponent<MeshRenderer>();
        if (firstRenderer != null)
        {
            materials = firstRenderer.materials;
        }

        StoreImportedMesh(combinedMesh, baseName + "_Combined", firstRenderer);
    }

    private void StoreImportedMesh(Mesh mesh, string meshName, MeshRenderer renderer)
    {
        Debug.Log($"StoreImportedMesh called for: {meshName}");

        if (mesh == null)
        {
            Debug.LogError($"Cannot store null mesh for: {meshName}");
            return;
        }

        Debug.Log($"Mesh details - Vertices: {mesh.vertices.Length}, Triangles: {mesh.triangles.Length}, Name: {mesh.name}");

        Material[] materials = null;
        if (renderer != null)
        {
            materials = renderer.materials;
            Debug.Log($"Found {materials.Length} materials");
        }
        else
        {
            Debug.Log("No renderer found - using null materials");
        }

        // Create raw mesh data
        RawMeshData rawMeshData = new RawMeshData(meshName, mesh, materials);
        importedObjects.Add(rawMeshData);

        Debug.Log($"Successfully stored raw mesh data: {meshName}. Total stored: {importedObjects.Count}");
    }

    private void NotifyModelCreator()
    {
        EnhancedModelCreator modelCreator = FindObjectOfType<EnhancedModelCreator>();
        if (modelCreator != null)
        {
            modelCreator.OnImportedObjectsChanged();
            Debug.Log($"Notified ModelCreator - {importedObjects.Count} objects available");
        }
        else
        {
            Debug.LogWarning("EnhancedModelCreator not found!");
        }
    }

    // Create instance of stored imported obj by name
    public ModelData CreateImportedObject(string objectName)
    {
        RawMeshData targetObject = importedObjects.Find(obj => obj.name == objectName);
        if (targetObject == null)
        {
            Debug.LogError($"Imported object not found: {objectName}");
            return null;
        }

        return CreateModelFromRawMeshData(targetObject);
    }

    // Create instance of stored imported obj by index
    public ModelData CreateImportedObject(int index)
    {
        Debug.Log($"CreateImportedObject called with index: {index}");
        Debug.Log($"Available objects count: {importedObjects.Count}");

        if (index < 0 || index >= importedObjects.Count)
        {
            Debug.LogError($"Invalid imported object index: {index}, available: {importedObjects.Count}");
            return null;
        }

        RawMeshData targetObject = importedObjects[index];
        Debug.Log($"Target object: {targetObject.name}");
        Debug.Log($"Target vertices: {(targetObject.vertices != null ? targetObject.vertices.Length.ToString() : "NULL")}");
        Debug.Log($"Target triangles: {(targetObject.triangles != null ? targetObject.triangles.Length.ToString() : "NULL")}");

        return CreateModelFromRawMeshData(targetObject);
    }

    private ModelData CreateModelFromRawMeshData(RawMeshData rawMeshData)
    {
        Debug.Log($"Creating model from raw mesh data: {rawMeshData.name}");

        if (rawMeshData.vertices == null || rawMeshData.vertices.Length == 0)
        {
            Debug.LogError($"Vertices array is null or empty for: {rawMeshData.name}");
            return null;
        }

        if (rawMeshData.triangles == null || rawMeshData.triangles.Length == 0)
        {
            Debug.LogError($"Triangles array is null or empty for: {rawMeshData.name}");
            return null;
        }

        Debug.Log($"Raw data - Vertices: {rawMeshData.vertices.Length}, Triangles: {rawMeshData.triangles.Length}, UVs: {(rawMeshData.uvs != null ? rawMeshData.uvs.Length : 0)}");

        GameObject meshObject = new GameObject(rawMeshData.name);
        ProBuilderMesh proBuilderMesh = meshObject.AddComponent<ProBuilderMesh>();

        try
        {
            Debug.Log("Setting positions from raw data...");
            proBuilderMesh.positions = rawMeshData.vertices;

            Debug.Log("Creating faces from raw triangles...");
            List<Face> faces = new List<Face>();
            for (int i = 0; i < rawMeshData.triangles.Length; i += 3)
            {
                if (i + 2 < rawMeshData.triangles.Length)
                {
                    int[] faceIndices = new int[] {
                        rawMeshData.triangles[i],
                        rawMeshData.triangles[i + 1],
                        rawMeshData.triangles[i + 2]
                    };
                    if (faceIndices[0] >= rawMeshData.vertices.Length ||
                        faceIndices[1] >= rawMeshData.vertices.Length ||
                        faceIndices[2] >= rawMeshData.vertices.Length ||
                        faceIndices[0] < 0 || faceIndices[1] < 0 || faceIndices[2] < 0)
                    {
                        Debug.LogWarning($"Invalid face indices: {faceIndices[0]}, {faceIndices[1]}, {faceIndices[2]} (vertex count: {rawMeshData.vertices.Length})");
                        continue;
                    }

                    faces.Add(new Face(faceIndices));
                }
            }

            if (faces.Count == 0)
            {
                Debug.LogError("No valid faces created from raw data");
                Destroy(meshObject);
                return null;
            }

            Debug.Log($"Setting {faces.Count} faces...");
            proBuilderMesh.faces = faces;

            if (rawMeshData.uvs != null && rawMeshData.uvs.Length > 0)
            {
                Debug.Log("Setting UVs from raw data...");
                proBuilderMesh.textures = rawMeshData.uvs;
            }

            Debug.Log("Calling ToMesh()...");
            proBuilderMesh.ToMesh();

            Debug.Log("Skipping Refresh() to avoid normals calculation issues...");
            Debug.Log($"Successfully created ProBuilder mesh with {faces.Count} faces");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to create ProBuilder mesh from raw data: {rawMeshData.name}. Error: {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
            Destroy(meshObject);
            return null;
        }

        Debug.Log("Setting transform...");
        meshObject.transform.position = defaultImportPosition;
        meshObject.transform.localScale = defaultImportScale;

        Debug.Log("Adding ModelData component...");
        ModelData modelData = meshObject.AddComponent<ModelData>();

        Debug.Log("Calling SetupModel...");
        modelData.SetupModel(proBuilderMesh);

        Debug.Log($"Successfully created instance of: {rawMeshData.name}");
        return modelData;
    }

    public List<string> GetImportedObjectNames()
    {
        List<string> names = new List<string>();
        foreach (RawMeshData obj in importedObjects)
        {
            names.Add(obj.name);
        }
        return names;
    }

    public int GetImportedObjectCount()
    {
        return importedObjects.Count;
    }

    public void ClearImportedObjects()
    {
        importedObjects.Clear();
        Debug.Log("Cleared all imported objects");
    }

    [ContextMenu("Debug Imported Objects")]
    public void DebugImportedObjects()
    {
        Debug.Log($"=== IMPORTED OBJECTS DEBUG ===");
        Debug.Log($"Total stored objects: {importedObjects.Count}");
        for (int i = 0; i < importedObjects.Count; i++)
        {
            RawMeshData obj = importedObjects[i];
            Debug.Log($"Object {i}: {obj.name}");
            Debug.Log($"  - Vertices: {(obj.vertices != null ? obj.vertices.Length.ToString() : "NULL")}");
            Debug.Log($"  - Triangles: {(obj.triangles != null ? obj.triangles.Length.ToString() : "NULL")}");
            Debug.Log($"  - UVs: {(obj.uvs != null ? obj.uvs.Length.ToString() : "NULL")}");
            Debug.Log($"  - Normals: {(obj.normals != null ? obj.normals.Length.ToString() : "NULL")}");
            Debug.Log($"  - Materials: {(obj.materials != null ? obj.materials.Length.ToString() : "NULL")}");
        }
        Debug.Log($"==============================");
    }

    public void SetCombineMeshes(bool combine)
    {
        combineMultipleMeshes = combine;
    }
}