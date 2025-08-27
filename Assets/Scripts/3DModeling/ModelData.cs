using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

public class ModelData : MonoBehaviour
{
    public int faces;
    public int vertices;
    public int edges;
    private Material originalMaterial;
    private Material overrideMaterial;

    public MeshRenderer meshRender;
    public MeshCollider meshCollider;
    public ProBuilderMesh editingModel;
    public Transform trans;
    private bool isSelected;

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
        UpdateMaterial();
    }

    public void SelectModel()
    {
        isSelected = true;
        UpdateMaterial();
    }

    public void UnSelectModel()
    {
        isSelected = false;
        UpdateMaterial();
    }

    public ProBuilderMesh GetEditModel()
    {
        return editingModel;
    }

    public void FinalizeEditModel()
    {
        try
        {
            UpdateStats(editingModel.faceCount, editingModel.edgeCount, editingModel.vertexCount);
            editingModel.ToMesh();
            Debug.Log("Model finalized successfully without refresh");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Error finalizing model, skipping refresh: {e.Message}");
            UpdateStats(editingModel.faceCount, editingModel.edgeCount, editingModel.vertexCount);
            editingModel.ToMesh();
        }
    }

    public void SetupModel(ProBuilderMesh _editingModel)
    {
        editingModel = _editingModel;
        trans = this.transform;
        meshRender = this.GetComponent<MeshRenderer>();
        meshCollider = this.AddComponent<MeshCollider>();
        ModelsManager.Instance.TrackModel(this);
        SetOriginalMaterial(ModelsManager.Instance.GetDefaultMaterial());
        ViewManager.Instance.ChangeViewForModel(this);
        Rigidbody rigid = this.AddComponent<Rigidbody>();
        rigid.isKinematic = true;
        this.AddComponent<XRGrabInteractable>();
        this.AddComponent<XRGeneralGrabTransformer>();
    }

    public void DeleteModel()
    {
        ModelsManager.Instance.UnTrackModel(this);
        Destroy(this.gameObject);
    }

    public void SetOriginalMaterial(Material _material)
    {
        originalMaterial = _material;
        UpdateMaterial();
    }

    public void SetOverrideMaterial(Material _overrideMaterial)
    {
        overrideMaterial = _overrideMaterial;
        UpdateMaterial();
    }

    private void UpdateMaterial()
    {
        List<Material> materials = new List<Material>();
        if (ViewManager.Instance.GetViewType() == ViewType.Standard)
        {
            materials.Add(originalMaterial);
        }
        else
        {
            materials.Add(overrideMaterial);
        }
        if (isSelected)
        {
            materials.Add(ModelsManager.Instance.GetHighLightMaterial());
        }
        meshRender.SetMaterials(materials);
    }
    public void UpdateStats(int _faces, int _edges, int _vertices)
    {
        faces = _faces;
        vertices = _vertices;
        edges = _edges;
    }

    public int GetFaces()
    {
        return faces;
    }

    public int GetEdges()
    {
        return edges;
    }

    public int GetVerts()
    {
        return vertices;
    }
}