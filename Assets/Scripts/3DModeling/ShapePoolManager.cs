//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.ProBuilder;
//using UnityEngine.ProBuilder.Shapes;

//public class ShapePoolManager : MonoBehaviour
//{
//    public static ShapePoolManager Instance;

//    private Dictionary<string, Queue<ProBuilderMesh>> pool = new Dictionary<string, Queue<ProBuilderMesh>>();
//    private Dictionary<string, ProBuilderMesh> prefabCache = new Dictionary<string, ProBuilderMesh>();

//    public enum ShapeTypeSimple { Cube, Sphere, Plane, Cone }

//    public int maxSubdivision = 8;

//    void Awake()
//    {
//        if (Instance == null) Instance = this;
//        else Destroy(gameObject);

//        DontDestroyOnLoad(gameObject);
//        InitializePool();
//    }

//    void InitializePool()
//    {
//        for (int sub = 0; sub <= maxSubdivision; sub++)
//        {
//            foreach (ShapeTypeSimple shape in System.Enum.GetValues(typeof(ShapeTypeSimple)))
//            {
//                string key = GetKey(shape, sub);
//                pool[key] = new Queue<ProBuilderMesh>();
//                prefabCache[key] = CreateShapePrefab(shape, sub);
//            }
//        }
//    }

//    string GetKey(ShapeTypeSimple shape, int subdivisions)
//    {
//        return $"{shape}_{subdivisions}";
//    }

//    ProBuilderMesh CreateShapePrefab(ShapeTypeSimple shape, int sub)
//    {
//#if UNITY_EDITOR
//        var shapeComponent = new ProBuilderMesh();

//        Shape shapeParams = new Shape();
//        shapeParams.shapeType = UnityEngine.ProBuilder.ShapeType.Cube;

//        switch (shape)
//        {
//            case ShapeTypeSimple.Cube:
//                shapeParams.shapeType = UnityEngine.ProBuilder.ShapeType.Cube;
//                shapeParams.size = Vector3.one;
//                shapeParams.cubeSubdivisions = new Vector3Int(sub, sub, sub);
//                break;

//            case ShapeTypeSimple.Sphere:
//                shapeParams.shapeType = UnityEngine.ProBuilder.ShapeType.Sphere;
//                shapeParams.radius = 0.5f;
//                shapeParams.rows = sub;
//                shapeParams.columns = sub;
//                break;

//            case ShapeTypeSimple.Plane:
//                shapeParams.shapeType = UnityEngine.ProBuilder.ShapeType.Plane;
//                shapeParams.size = new Vector3(1, 0, 1);
//                shapeParams.planeSubdivisions = new Vector2Int(sub, sub);
//                break;

//            case ShapeTypeSimple.Cone:
//                shapeParams.shapeType = UnityEngine.ProBuilder.ShapeType.Cone;
//                shapeParams.radius = 0.5f;
//                shapeParams.height = 1;
//                shapeParams.coneSides = Mathf.Max(sub, 3);
//                break;
//        }

//        var go = new GameObject($"{shape}_{sub}");
//        var pb = go.AddComponent<ProBuilderMesh>();
//        ShapeGenerator.GenerateShape(pb, shapeParams);
//        pb.ToMesh();
//        pb.Refresh();
//        pb.gameObject.SetActive(false);
//        go.transform.SetParent(transform);
//        return pb;
//#else
//        return null;
//#endif
//    }

//    public ProBuilderMesh GetShape(ShapeTypeSimple shape, int subdivisions, Vector3 position, Vector3 scale)
//    {
//        string key = GetKey(shape, subdivisions);

//        if (!pool.ContainsKey(key)) return null;

//        ProBuilderMesh mesh;
//        if (pool[key].Count > 0)
//        {
//            mesh = pool[key].Dequeue();
//        }
//        else
//        {
//            mesh = Instantiate(prefabCache[key], transform);
//        }

//        mesh.gameObject.SetActive(true);
//        mesh.transform.position = position;
//        mesh.transform.localScale = scale;
//        return mesh;
//    }

//    public void ReturnShape(ProBuilderMesh mesh, ShapeTypeSimple shape, int subdivisions)
//    {
//        mesh.gameObject.SetActive(false);
//        pool[GetKey(shape, subdivisions)].Enqueue(mesh);
//    }
//}