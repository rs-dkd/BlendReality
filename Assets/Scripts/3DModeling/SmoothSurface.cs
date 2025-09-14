using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ProBuilder;

public class SmoothSurface : MonoBehaviour
{
    public int uResolution = 15;
    public int vResolution = 15;
    public MeshRenderer meshRender;
    public MeshFilter meshFilter;
    public Transform trans;
    public ModelData modelData;


    public void SetPosition(Vector3 pos)
    {
        trans.position = pos;
    }

    public void SetScale(Vector3 scale)
    {
        trans.localScale = scale;
    }

    public void SetupModel(ModelData _modelData)
    {
        modelData = _modelData;
        trans = this.transform;
        meshFilter = this.AddComponent<MeshFilter>();
        meshRender = this.AddComponent<MeshRenderer>();

    }

    public void DeleteModel()
    {
        Destroy(this.gameObject);
    }

    public void UpdateMesh()
    {
        ProBuilderMesh original = modelData.GetEditModel();
        Mesh subdivided = CatmullClark.Subdivide(original);
        meshFilter.mesh = subdivided;
    }
}