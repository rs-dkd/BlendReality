using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;
using System.Linq;
public class ModelData : MonoBehaviour
{
    public string modelName;
    public int modelID;
    private Material originalMaterial;
    private Material overrideMaterial;
    public MeshFilter meshFilter;
    public MeshRenderer meshRender;
    public MeshCollider meshCollider;
    public ProBuilderMesh editingModel;
    public Transform trans;
    private bool isSelected;
    public SmoothSurface smoothedMesh;

    public void SetPosition(Vector3 pos)
    {
        trans.position = pos;
    }
    public void SetScale(Vector3 scale)
    {
        trans.localScale = scale;
    }




    public void UpdateMesh(ProBuilderMesh mesh)
    {
        editingModel.Clear();

        editingModel.positions = mesh.positions;
        editingModel.faces = mesh.faces;

        editingModel.ToMesh();
        editingModel.Refresh();
    }
    public void UpdateMeshEdit()
    {
        editingModel.Refresh();
        smoothedMesh.UpdateMesh();
    }
    public ProBuilderMesh GetEditModel()
    {
        return editingModel;
    }
    public Mesh GetMesh()
    {
        return meshFilter.sharedMesh;
    }
    public void FinalizeEditModel()
    {
        editingModel.ToMesh();
        editingModel.Refresh();
    }






    public void SetupModel(ProBuilderMesh _editingModel)
    {
        editingModel = _editingModel;
        trans = this.transform;
        meshRender = this.GetComponent<MeshRenderer>();
        meshCollider = this.AddComponent<MeshCollider>();
        meshFilter = this.GetComponent<MeshFilter>();
        modelID = ModelsManager.Instance.TrackModel(this);

        Rigidbody rigid = this.AddComponent<Rigidbody>();
        rigid.isKinematic = true;

        this.AddComponent<XRGrabInteractable>();
        this.AddComponent<XRGeneralGrabTransformer>();


        if(smoothedMesh == null)
        {
            GameObject obj = new GameObject("MyObject");
            obj.transform.parent = this.transform;
            obj.transform.localPosition = Vector3.zero;
            smoothedMesh = obj.AddComponent<SmoothSurface>();
            smoothedMesh.SetupModel(this);
        }

        ShadingUpdated();
        ViewManager.Instance.OnShadingChanged.AddListener(ShadingUpdated);
    }



    public void SelectModel()
    {
        isSelected = true;
        ShadingUpdated();
    }
    public void UnSelectModel()
    {
        isSelected = false;
        ShadingUpdated();
    }


    public void ShadingUpdated()
    {
        meshRender.sharedMaterial = ViewManager.Instance.GetCurrentShading(isSelected);
    }




    public void DeleteModel()
    {
        ViewManager.Instance.OnShadingChanged.RemoveListener(ShadingUpdated);
        ModelsManager.Instance.UnTrackModel(this);
        Destroy(this.gameObject);
    }



 
    public int GetFacesCount()
    {
        return editingModel.faceCount;
    }
    public int GetEdgesCount()
    {
        return editingModel.edgeCount;
    }
    public int GetVertCount()
    {
        return editingModel.vertexCount;
    }

    public List<Vector3> GetVerts()
    {
        return editingModel.VerticesInWorldSpace().ToList();
    }
    public List<Face> GetFaces()
    {
        return editingModel.faces.ToList<Face>();
    }





}
