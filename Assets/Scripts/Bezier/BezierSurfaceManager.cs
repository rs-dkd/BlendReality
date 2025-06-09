using System.Collections.Generic;
using UnityEngine;

public class BezierSurfaceManager : MonoBehaviour
{
    [Header("Surface Management")]
    public List<BezierSurface> surfaces = new List<BezierSurface>();

    [Header("Visual Settings")]
    public Material surfaceMaterial;
    public Material controlPointMaterial;
    public Material controlPointGrabbedMaterial;
    public float controlPointSize = 0.05f;

    [Header("Display Options")]
    public bool showControlPoints = true;
    public bool showSurfaces = true;

    private Dictionary<int, GameObject> surfaceObjects = new Dictionary<int, GameObject>();
    private Dictionary<int, List<GameObject>> controlPointObjects = new Dictionary<int, List<GameObject>>();

    void Start()
    {
        if (surfaceMaterial == null)
        {
            surfaceMaterial = CreateDefaultSurfaceMaterial();
        }

        if (controlPointMaterial == null)
        {
            controlPointMaterial = CreateDefaultControlPointMaterial();
        }

        if (controlPointGrabbedMaterial == null)
        {
            controlPointGrabbedMaterial = CreateDefaultGrabbedMaterial();
        }
    }

    public BezierSurface CreateNewSurface(string surfaceType = "flat")
    {
        BezierSurface newSurface = new BezierSurface(4, 4);
        newSurface.surfaceID = surfaces.Count;
        SetupSurfaceShape(newSurface, surfaceType);
        surfaces.Add(newSurface);
        CreateSurfaceVisuals(newSurface);
        Debug.Log($"Created {surfaceType} Bezier surface with ID: {newSurface.surfaceID}");

        return newSurface;
    }
    private void SetupSurfaceShape(BezierSurface surface, string shapeType)
    {
        switch (shapeType.ToLower())
        {
            case "flat":
                break;

            case "dome":
                for (int u = 0; u < 4; u++)
                {
                    for (int v = 0; v < 4; v++)
                    {
                        Vector3 pos = surface.controlPoints[u, v];
                        float distanceFromCenter = Vector3.Distance(pos, Vector3.zero);
                        pos.y = Mathf.Max(0, 0.5f - distanceFromCenter * 0.2f);
                        surface.controlPoints[u, v] = pos;
                    }
                }
                break;

            case "wavy":
                for (int u = 0; u < 4; u++)
                {
                    for (int v = 0; v < 4; v++)
                    {
                        Vector3 pos = surface.controlPoints[u, v];
                        pos.y = Mathf.Sin(u * Mathf.PI * 0.5f) * Mathf.Cos(v * Mathf.PI * 0.5f) * 0.3f;
                        surface.controlPoints[u, v] = pos;
                    }
                }
                break;
        }
    }
    private void CreateSurfaceVisuals(BezierSurface surface)
    {
        // Create main surface object
        GameObject surfaceObj = new GameObject($"BezierSurface_{surface.surfaceID}");
        surfaceObj.transform.SetParent(transform);
        MeshFilter meshFilter = surfaceObj.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = surfaceObj.AddComponent<MeshRenderer>();
        meshRenderer.material = surfaceMaterial;
        surfaceObjects[surface.surfaceID] = surfaceObj;
        CreateControlPointSpheres(surface);
        UpdateSurfaceMesh(surface);
    }

    //Create grabbable control point spheres
    private void CreateControlPointSpheres(BezierSurface surface)
    {
        List<GameObject> controlPoints = new List<GameObject>();

        int uCount = surface.controlPoints.GetLength(0);
        int vCount = surface.controlPoints.GetLength(1);

        for (int u = 0; u < uCount; u++)
        {
            for (int v = 0; v < vCount; v++)
            {
                //Create sphere at control point position
                GameObject controlPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                controlPoint.name = $"ControlPoint_{surface.surfaceID}_{u}_{v}";
                controlPoint.transform.position = surface.controlPoints[u, v];
                controlPoint.transform.localScale = Vector3.one * controlPointSize;
                controlPoint.transform.SetParent(transform);
                MeshRenderer renderer = controlPoint.GetComponent<MeshRenderer>();
                if (renderer != null && controlPointMaterial != null)
                {
                    renderer.material = controlPointMaterial;
                }
                BezierControlPoint controlPointScript = controlPoint.AddComponent<BezierControlPoint>();
                controlPointScript.Initialize(surface.surfaceID, u, v, this);
                controlPointScript.normalMaterial = controlPointMaterial;
                controlPointScript.grabbedMaterial = controlPointGrabbedMaterial;

                controlPointScript.SetNormalMaterial();

                controlPoints.Add(controlPoint);
            }
        }

        controlPointObjects[surface.surfaceID] = controlPoints;
    }

    //Update surface when control point is moved
    public void OnControlPointMoved(int surfaceID, int u, int v, Vector3 newPosition)
    {
        if (surfaceID < surfaces.Count)
        {
            //Update the control point position in our data
            surfaces[surfaceID].controlPoints[u, v] = newPosition;
            //Mark surface as needing regeneration
            surfaces[surfaceID].isDirty = true;
            //Update the visual mesh
            UpdateSurfaceMesh(surfaces[surfaceID]);
        }
    }

    // Regenerates surface's visual mesh
    private void UpdateSurfaceMesh(BezierSurface surface)
    {
        if (!surfaceObjects.ContainsKey(surface.surfaceID)) return;
        Mesh newMesh = GenerateMeshFromSurface(surface);
        surfaceObjects[surface.surfaceID].GetComponent<MeshFilter>().mesh = newMesh;
        surface.isDirty = false;
    }
    //Converts BezierSurface data into Unity Mesh
    private Mesh GenerateMeshFromSurface(BezierSurface surface)
    {
        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        //Create vertex grid from sampling of Bezier surface
        for (int i = 0; i <= surface.uResolution; i++)
        {
            for (int j = 0; j <= surface.vResolution; j++)
            {
                float u = (float)i / surface.uResolution;
                float v = (float)j / surface.vResolution;

                Vector3 vertex = surface.EvaluateSurface(u, v);
                vertices.Add(vertex);
                uvs.Add(new Vector2(u, v));
            }
        }

        //Connect vertices with triangles
        for (int i = 0; i < surface.uResolution; i++)
        {
            for (int j = 0; j < surface.vResolution; j++)
            {
                int index = i * (surface.vResolution + 1) + j;

                triangles.Add(index);
                triangles.Add(index + surface.vResolution + 1);
                triangles.Add(index + 1);

                triangles.Add(index + 1);
                triangles.Add(index + surface.vResolution + 1);
                triangles.Add(index + surface.vResolution + 2);
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();

        return mesh;
    }

    //Remove surface
    public void RemoveSurface(int surfaceID)
    {
        if (surfaceID < surfaces.Count)
        {
            if (surfaceObjects.ContainsKey(surfaceID))
            {
                DestroyImmediate(surfaceObjects[surfaceID]);
                surfaceObjects.Remove(surfaceID);
            }

            if (controlPointObjects.ContainsKey(surfaceID))
            {
                foreach (var controlPoint in controlPointObjects[surfaceID])
                {
                    DestroyImmediate(controlPoint);
                }
                controlPointObjects.Remove(surfaceID);
            }
            surfaces.RemoveAt(surfaceID);
            for (int i = surfaceID; i < surfaces.Count; i++)
            {
                surfaces[i].surfaceID = i;
            }
        }
    }

    void Update()
    {
        //Toggle surface visibility
        foreach (var kvp in surfaceObjects)
        {
            kvp.Value.SetActive(showSurfaces);
        }

        //Toggle control point visibility
        foreach (var kvp in controlPointObjects)
        {
            foreach (var controlPoint in kvp.Value)
            {
                controlPoint.SetActive(showControlPoints);
            }
        }
    }

    //Create default materials
    private Material CreateDefaultSurfaceMaterial()
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(0.2f, 0.7f, 1f, 0.7f);
        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
        return mat;
    }

    private Material CreateDefaultControlPointMaterial()
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Color.red;
        mat.SetFloat("_Metallic", 0.3f);
        mat.SetFloat("_Smoothness", 0.7f);
        return mat;
    }

    private Material CreateDefaultGrabbedMaterial()
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Color.yellow;
        mat.SetFloat("_Metallic", 0.8f);
        mat.SetFloat("_Smoothness", 0.9f);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", Color.yellow * 0.3f);
        return mat;
    }

    //Get control point objects for pos
    public List<GameObject> GetControlPointObjects(int surfaceID)
    {
        if (controlPointObjects.ContainsKey(surfaceID))
        {
            return controlPointObjects[surfaceID];
        }
        return null;
    }
    public void RefreshControlPointMaterials(int surfaceID)
    {
        if (controlPointObjects.ContainsKey(surfaceID))
        {
            foreach (var controlPointObj in controlPointObjects[surfaceID])
            {
                BezierControlPoint controlPoint = controlPointObj.GetComponent<BezierControlPoint>();
                if (controlPoint != null)
                {
                    controlPoint.UpdateNormalMaterial(controlPointMaterial);
                }
            }
        }
    }
}