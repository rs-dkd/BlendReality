using UnityEngine;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;

#region Data Structures

public enum CoordinateHandedness
{
    Left,
    Right
}

public class ObjExportOptions
{
    public CoordinateHandedness handedness = CoordinateHandedness.Right;
    public bool copyTextures = true;
    public bool applyTransforms = true;
    public bool includeVertexColors = false;
}

public class RuntimeModel
{
    public string name;
    public Vector3[] vertices;
    public Vector3[] normals;
    public Vector2[] uvs;
    public Color[] colors;
    public List<SubMeshData> submeshes;
    public Matrix4x4 transform;
    public Material[] materials;
}

public class SubMeshData
{
    public int[] indexes;
    public int materialIndex;
    public int meshTopology; //0: triangle 2: quad
}

#endregion

//obj exporter workflow
//1. checking multiple mesh or single
//2. construct runtime mesh model
//3. start writing mtl file
//3.1 construct material mapping
//3.2 copy texture if copytextures == true
//3.3 
public class OBJExporter : Singleton<OBJExporter>
{

    static Dictionary<string, string> s_TextureMapKeys = new Dictionary<string, string>
    {
        { "_MainTex", "map_Kd" },           // Albedo
        { "_MetallicGlossMap", "map_Pm" },  // Metallic
        { "_BumpMap", "bump" },             // Normal
        { "_ParallaxMap", "disp" },         // Height
        { "_EmissionMap", "map_Ke" },       // Emission
        { "_DetailMask", "map_d" },         // Detail Mask
    };

    #region Public API

    /// <summary>
    /// export probuilder mesh to obj file
    /// </summary>
    /// <param name="meshes">probuilder mesh list</param>
    /// <param name="filePath">full filepath with .obj extension</param>
    /// <param name="options">export options</param>
    /// <returns></returns>
    public bool ExportToObj(List<GameObject> meshes, string filePath, ObjExportOptions options = null)
    {
        if (meshes == null || meshes.Count == 0)
        {
            Debug.LogError("no meshes to export");
            return false;
        }

        //default options
        if (options == null)
            options = new ObjExportOptions();

        //construct runtimemodel
        List<RuntimeModel> models = new List<RuntimeModel>();
        foreach (var go in meshes)
        {
            var model = CreateModelFromGameObject(go);
            if (model != null)
                models.Add(model);
        }

        if (models.Count == 0)
        {
            Debug.LogError("没有有效的模型数据");
            return false;
        }

        //check ok, now export for real
        return DoExport(filePath, models, options);
    }

    public bool ExportSingle(GameObject go, string filePath, ObjExportOptions options = null)
    {
        return ExportToObj(new List<GameObject> { go }, filePath, options);
    }

    #endregion

    #region Core Export Logic
    private bool DoExport(string path, List<RuntimeModel> models, ObjExportOptions options)
    {
        string name = Path.GetFileNameWithoutExtension(path);
        string directory = Path.GetDirectoryName(path);

        // Ensure directory exists
        if (!Directory.Exists(directory))
        {
            try
            {
                Directory.CreateDirectory(directory);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to create directory: {directory}\n{e}");
                return false;
            }
        }

        try
        {
            // Generate content
            // Build mat map here instead of having it done twice
            Dictionary<Material, string> materialMap = BuildMaterialMap(models);

            string objContent = WriteObjContents(models, name, materialMap, options);
            string mtlContent = WriteMtlContents(models, materialMap, out List<string> texturePaths, options);

            // Write files
            File.WriteAllText(Path.Combine(directory, $"{name}.obj"), objContent);
            File.WriteAllText(Path.Combine(directory, $"{name}.mtl"), mtlContent);

            // Copy textures
            if (options.copyTextures && texturePaths != null)
            {
                CopyTextures(texturePaths, directory);
            }

            Debug.Log($"Successfully exported OBJ to: {path}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Export failed: {e}");
            return false;
        }
    }


    // Same as before but now don't rewrite mat map instead just reference existing one
    private string WriteMtlContents(List<RuntimeModel> models, Dictionary<Material, string> materialMap, out List<string> texturePaths, ObjExportOptions options)
    {
        texturePaths = new List<string>();
        StringBuilder sb = new StringBuilder();

        foreach (var kvp in materialMap)
        {
            Material mat = kvp.Key;
            string matName = kvp.Value;

            sb.AppendLine($"newmtl {matName}");

            // Texture maps
            if (mat.shader != null)
            {
                foreach (var texMapPair in s_TextureMapKeys)
                {
                    string unityPropName = texMapPair.Key;
                    string objKeyword = texMapPair.Value;

                    if (!mat.HasProperty(unityPropName))
                        continue;

                    Texture texture = mat.GetTexture(unityPropName);
                    if (texture == null)
                        continue;

                    string texPath = GetTexturePath(texture, options.copyTextures);
                    if (!string.IsNullOrEmpty(texPath))
                    {
                        string texFileName = Path.GetFileName(texPath);
                        sb.AppendLine($"{objKeyword} {texFileName}");

                        if (options.copyTextures && !texturePaths.Contains(texPath))
                            texturePaths.Add(texPath);
                    }
                }
            }
            // Color properties
            Color color = Color.white;
            if (mat.HasProperty("_BaseColor")) // URP/HDRP
                color = mat.GetColor("_BaseColor");
            else if (mat.HasProperty("_Color"))
                color = mat.color;

            sb.AppendLine($"Kd {color.r:F6} {color.g:F6} {color.b:F6}");
            sb.AppendLine($"d {color.a:F6}");
            sb.AppendLine();
        }
        return sb.ToString();
    }

    // Write the actual obj content based on general reqs
    private string WriteObjContents(List<RuntimeModel> models, string filename, Dictionary<Material, string> materialMap, ObjExportOptions options)
    {
        StringBuilder sb = new StringBuilder();

        // Header
        sb.AppendLine("# Exported from Unity by OBJExporter");
        sb.AppendLine($"mtllib {filename}.mtl");

        // Setup global list for the verts/normals/uvs
        // Point to data
        int vertexOffset = 0;
        int normalOffset = 0;
        int uvOffset = 0;

        foreach (var model in models)
        {
            sb.AppendLine($"\n# Object: {model.name}");
            sb.AppendLine($"o {model.name}");

            // Vertex data
            var vertices = model.vertices;
            var normals = model.normals;
            var uvs = model.uvs;

            // World transform
            if (options.applyTransforms)
            {
                vertices = model.vertices.Select(v => model.transform.MultiplyPoint3x4(v)).ToArray();
                normals = model.normals.Select(n => model.transform.MultiplyVector(n).normalized).ToArray();
            }

            // Write verts/normals/uvs to the file
            var vCount = AppendArrayVec3(sb, vertices, "v", true, out var vMap);
            var nCount = AppendArrayVec3(sb, normals, "vn", true, out var nMap);
            var uvCount = AppendArrayVec2(sb, uvs, "vt", true, out var uvMap);

            // Face data based on submesh
            foreach (var submesh in model.submeshes)
            {
                // Set mat to faces
                if (submesh.materialIndex < model.materials.Length)
                {
                    Material mat = model.materials[submesh.materialIndex];
                    if (materialMap.TryGetValue(mat, out string matName))
                    {
                        sb.AppendLine($"usemtl {matName}");
                    }
                }

                var indices = submesh.indexes;

                // Faces in OBJ are triangles so have to do it like this
                for (int i = 0; i < indices.Length; i += 3)
                {
                    // Get orig index from data
                    int i1 = indices[i];
                    int i2 = indices[i + 1];
                    int i3 = indices[i + 2];

                    // Make face string, have to add 1 to indices
                    // Three maps make up the new index based on coincident vertex merging
                    string f1 = $"{vMap[i1] + vertexOffset + 1}/{uvMap[i1] + uvOffset + 1}/{nMap[i1] + normalOffset + 1}";
                    string f2 = $"{vMap[i2] + vertexOffset + 1}/{uvMap[i2] + uvOffset + 1}/{nMap[i2] + normalOffset + 1}";
                    string f3 = $"{vMap[i3] + vertexOffset + 1}/{uvMap[i3] + uvOffset + 1}/{nMap[i3] + normalOffset + 1}";

                    sb.AppendLine($"f {f1} {f2} {f3}");
                }
            }

            // Update offsets for the next model
            vertexOffset += vCount;
            normalOffset += nCount;
            uvOffset += uvCount;
        }

        return sb.ToString();
    }

#endregion

    #region Helper Methods


    //create material mapping with runtime models, same material name will be unique by adding _number
    private Dictionary<Material, string> BuildMaterialMap(List<RuntimeModel> models)
    {
        Dictionary<Material, string> materialMap = new Dictionary<Material, string>();

        foreach (var model in models)
        {
            if (model.materials == null) continue;

            foreach (var mat in model.materials)
            {
                if (mat == null || materialMap.ContainsKey(mat))
                    continue;

                string matName = SanitizeName(mat.name);
                int counter = 1;
                string uniqueName = matName;

                while (materialMap.ContainsValue(uniqueName))
                {
                    uniqueName = $"{matName}_{counter++}";
                }

                materialMap.Add(mat, uniqueName);
            }
        }

        return materialMap;
    }


    //create runtimemodel from probuilder go
    private RuntimeModel CreateModelFromGameObject(GameObject go)
    {
        MeshFilter mf = go.GetComponent<MeshFilter>();
        MeshRenderer mr = go.GetComponent<MeshRenderer>();

        if (mf == null || mf.sharedMesh == null)
        {
            Debug.LogWarning($"GameObject {go.name} has no valid mesh");
            return null;
        }

        Mesh mesh = mf.sharedMesh;

        List<SubMeshData> submeshes = new List<SubMeshData>();
        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            submeshes.Add(new SubMeshData
            {
                indexes = mesh.GetTriangles(i),
                materialIndex = i,
                meshTopology = (int)mesh.GetTopology(i)
            });
        }

        RuntimeModel model = new RuntimeModel
        {
            name = SanitizeName(go.name),
            vertices = mesh.vertices,
            normals = mesh.normals,
            uvs = mesh.uv,
            colors = mesh.colors,
            submeshes = submeshes,
            transform = go.transform.localToWorldMatrix,
            materials = mr != null ? mr.sharedMaterials : new Material[0]
        };

        return model;
    }

    private string SanitizeName(string name)
    {
        return name.Replace(" ", "_").Replace("/", "_").Replace("\\", "_");
    }

    //this needs further work and talk (how to store the textures)
    private string GetTexturePath(Texture texture, bool needsCopy)
    {
        string texName = texture.name;

        // Common texture locations to check
        string[] possiblePaths = new string[]
        {
            Path.Combine(Application.dataPath, "Textures", texName + ".png"),
            Path.Combine(Application.dataPath, "Textures", texName + ".jpg"),
            Path.Combine(Application.streamingAssetsPath, "Textures", texName + ".png"),
            Path.Combine(Application.streamingAssetsPath, "Textures", texName + ".jpg"),
            Path.Combine(Application.dataPath, "Resources", texName + ".png"),
            Path.Combine(Application.dataPath, "Resources", texName + ".jpg"),
        };

        foreach (string path in possiblePaths)
        {
            if (File.Exists(path))
                return path;
        }

        // If texture is a RenderTexture or dynamically generated, might need special handling
        Debug.LogWarning($"Could not find file path for texture: {texName}");
        return null;
    }

    //copy texture from assets to destination directory
    private void CopyTextures(List<string> texturePaths, string destDir)
    {
        foreach (string srcPath in texturePaths)
        {
            if (!File.Exists(srcPath))
            {
                Debug.LogWarning($"Texture file not found: {srcPath}");
                continue;
            }

            string destPath = Path.Combine(destDir, Path.GetFileName(srcPath));

            if (!File.Exists(destPath))
            {
                try
                {
                    File.Copy(srcPath, destPath);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Failed to copy texture: {srcPath}\n{e}");
                }
            }
        }
    }

    static int AppendArrayVec2(StringBuilder sb, Vector2[] array, string prefix, bool mergeCoincident, out Dictionary<int, int> coincidentIndexMap)
    {
        coincidentIndexMap = new Dictionary<int, int>();

        if (array == null)
            return 0;

        Dictionary<IntVec2, int> common = new Dictionary<IntVec2, int>();
        int index = 0;

        for (int i = 0, c = array.Length; i < c; i++)
        {
            var texture = array[i];
            var key = new IntVec2(texture);
            int vertexIndex;

            if (mergeCoincident)
            {
                if (!common.TryGetValue(key, out vertexIndex))
                {
                    vertexIndex = index++;
                    common.Add(key, vertexIndex);
                }
                else
                {
                    coincidentIndexMap.Add(i, vertexIndex);
                    continue;
                }
            }
            else
            {
                vertexIndex = index++;
            }

            coincidentIndexMap.Add(i, vertexIndex);

            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}",
                prefix,
                texture.x,
                texture.y));
        }

        return index;
    }

    static int AppendArrayVec3(StringBuilder sb, Vector3[] array, string prefix, bool mergeCoincident, out Dictionary<int, int> coincidentIndexMap)
    {
        coincidentIndexMap = new Dictionary<int, int>();

        if (array == null)
            return 0;

        Dictionary<IntVec3, int> common = new Dictionary<IntVec3, int>();
        int index = 0;

        for (int i = 0, c = array.Length; i < c; i++)
        {
            var vec = array[i];
            var key = new IntVec3(vec);
            int vertexIndex;

            if (mergeCoincident)
            {
                if (!common.TryGetValue(key, out vertexIndex))
                {
                    vertexIndex = index++;
                    common.Add(key, vertexIndex);
                }
                else
                {
                    coincidentIndexMap.Add(i, vertexIndex);
                    continue;
                }
            }
            else
            {
                vertexIndex = index++;
            }

            coincidentIndexMap.Add(i, vertexIndex);

            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0} {1} {2} {3}",
                prefix,
                vec.x,
                vec.y,
                vec.z));
        }

        return index;
    }

    #endregion

}
