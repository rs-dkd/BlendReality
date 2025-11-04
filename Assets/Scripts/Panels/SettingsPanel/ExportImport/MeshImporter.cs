using Dummiesman;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;



/// <summary>
/// Imports mesh
/// Allows to combine meshes into one
/// </summary>
public class MeshImporter : MonoBehaviour
{
    //Singleton pattern 
    public static MeshImporter Instance { get; private set; }
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




    /// <summary>
    /// Loads the file and imports the meshes as separate or combined
    /// </summary>
    public void LoadFile(string name, string path, bool combineMesh)
    {
        //Uses OBJLoader to import the meshes
        GameObject go = new OBJLoader().Load(path);
        //OBJLoader separates the meshes into multiple meshfilters
        MeshFilter[] meshFilters = go.GetComponentsInChildren<MeshFilter>();

        //Combine all meshes
        if (combineMesh)
        {
            CombineInstance[] combine = new CombineInstance[meshFilters.Length];

            for (int i = 0; i < meshFilters.Length; i++)
            {
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            }

            Mesh combinedMesh = new Mesh();
            combinedMesh.CombineMeshes(combine);
            combinedMesh.name = name + "_Combined";

            Material[] materials = null;
            MeshRenderer firstRenderer = meshFilters[0].GetComponent<MeshRenderer>();
            if (firstRenderer != null)
            {
                materials = firstRenderer.materials;
            }
            CreateModelDataFromMesh(combinedMesh, name);


        }
        //Create models separately
        else
        {
            foreach (MeshFilter meshFilter in meshFilters)
            {
                if (meshFilter.sharedMesh != null)
                {
                    // --- Fix to correct meshes position ---
                    Mesh worldSpaceMesh = new Mesh();

              
                    Mesh localMesh = meshFilter.sharedMesh;
                    Vector3[] localVerts = localMesh.vertices;
                    Vector3[] localNormals = localMesh.normals;

                    Vector3[] worldVerts = new Vector3[localVerts.Length];
                    Vector3[] worldNormals = new Vector3[localNormals.Length];

                    Transform meshTransform = meshFilter.transform;

                    for (int i = 0; i < localVerts.Length; i++)
                    {
                        worldVerts[i] = meshTransform.TransformPoint(localVerts[i]);
                        worldNormals[i] = meshTransform.TransformDirection(localNormals[i]);
                    }

                    worldSpaceMesh.vertices = worldVerts;
                    worldSpaceMesh.normals = worldNormals;
                    worldSpaceMesh.triangles = localMesh.triangles;
                    worldSpaceMesh.uv = localMesh.uv;
                    // --- End Fix ---



                    CreateModelDataFromMesh(worldSpaceMesh, meshFilter.name);

                    Destroy(worldSpaceMesh);

                }
            }
        }

        //Clean up
        Destroy(go);




    }

    /// <summary>
    /// Create probuilder mesh from the meshes
    /// </summary>
    private void CreateModelDataFromMesh(Mesh mesh, string name)
    {
        GameObject meshObject = new GameObject(name);
        ProBuilderMesh proBuilderMesh = meshObject.AddComponent<ProBuilderMesh>();

        try
        {
            proBuilderMesh.positions = mesh.vertices;

            List<Face> faces = new List<Face>();
            for (int i = 0; i < mesh.triangles.Length; i += 3)
            {
                if (i + 2 < mesh.triangles.Length)
                {
                    int[] faceIndices = new int[] {
                                mesh.triangles[i],
                                mesh.triangles[i + 1],
                                mesh.triangles[i + 2]
                            };
                    if (faceIndices[0] >= mesh.vertices.Length ||
                        faceIndices[1] >= mesh.vertices.Length ||
                        faceIndices[2] >= mesh.vertices.Length ||
                        faceIndices[0] < 0 || faceIndices[1] < 0 || faceIndices[2] < 0)
                    {
                        Debug.LogWarning($"Invalid face indices: {faceIndices[0]}, {faceIndices[1]}, {faceIndices[2]} (vertex count: {mesh.vertices.Length})");
                        continue;
                    }

                    faces.Add(new Face(faceIndices));
                }
            }

            if (faces.Count == 0)
            {
                Debug.LogError("No valid faces created from raw data");
                Destroy(meshObject);
                return;
            }
            proBuilderMesh.faces = faces;

            if (mesh.uv != null && mesh.uv.Length > 0)
            {
                Debug.Log("Setting UVs from raw data");
                proBuilderMesh.textures = mesh.uv;
            }

            proBuilderMesh.ToMesh();

        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to create ProBuilder mesh from raw data: {name}. Error: {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
            Destroy(meshObject);
            return;
        }

        //Create our ModelData model from the data
        GameObject myObject = new GameObject();
        ModelData modelData = myObject.AddComponent<ModelData>();
        modelData.SetupModel(proBuilderMesh, new Vector3(), name);

        return;
    }


}
