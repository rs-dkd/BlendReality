using UnityEngine;

public class AxisIndicators : MonoBehaviour
{
    [SerializeField] private float axisLength = 1.0f;
    [SerializeField] private float lineWidth = 0.02f;
    [SerializeField] private Material xAxisMaterial;
    [SerializeField] private Material yAxisMaterial;
    [SerializeField] private Material zAxisMaterial;

    void Start()
    {
        CreateAxis("XAxis", Vector3.right, xAxisMaterial, Color.red);
        CreateAxis("YAxis", Vector3.up, yAxisMaterial, Color.green);
        CreateAxis("ZAxis", Vector3.forward, zAxisMaterial, Color.blue);
    }

    private void CreateAxis(string name, Vector3 direction, Material material, Color color)
    {
        GameObject axisObj = new GameObject(name);
        axisObj.transform.SetParent(transform);
        axisObj.transform.localPosition = Vector3.zero;

        LineRenderer line = axisObj.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.material = material;
        line.startColor = color;
        line.endColor = color;

        line.SetPosition(0, Vector3.zero);
        line.SetPosition(1, direction * axisLength);

        CapsuleCollider collider = line.gameObject.AddComponent<CapsuleCollider>();
        collider.radius = lineWidth * 2;
        collider.height = 1;
        Debug.Log(direction.x);
        if (direction.x != 0) collider.direction = 0;
        else if (direction.y != 0) collider.direction = 1;
        else collider.direction = 2;


        GameObject cone = CreateCone(color);
        cone.transform.SetParent(axisObj.transform);
        cone.transform.localPosition = direction * axisLength;
        cone.transform.localRotation = Quaternion.LookRotation(direction);
        cone.transform.localScale = Vector3.one * lineWidth * 5f;
    }

    private GameObject CreateCone(Color color)
    {
        GameObject cone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        cone.transform.localScale = new Vector3(1, 1, 1);

        MeshRenderer renderer = cone.GetComponent<MeshRenderer>();
        Material coneMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        coneMaterial.color = color;
        coneMaterial.EnableKeyword("_EMISSION");
        coneMaterial.SetColor("_EmissionColor", color);
        renderer.material = coneMaterial;

        return cone;
    }
}