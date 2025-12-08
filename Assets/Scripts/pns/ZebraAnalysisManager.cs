using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manages zebra stripe analysis visualization for PnS surfaces.
/// Zebra stripes reveal surface quality by showing reflection patterns.
/// </summary>
public class ZebraAnalysisManager : MonoBehaviour
{
    public static ZebraAnalysisManager Instance { get; private set; }

    [Header("Zebra Settings")]
    [SerializeField] private bool zebraEnabled = false;
    [SerializeField] private Material zebraMaterial;
    [SerializeField] private Color stripeColor1 = new Color(0.8f, 0.8f, 0.8f, 1f);
    [SerializeField] private Color stripeColor2 = new Color(0.1f, 0.1f, 0.1f, 1f);
    [SerializeField][Range(1, 50)] private int stripeCount = 10;
    [SerializeField][Range(0f, 1f)] private float stripeSharpness = 0.8f;
    [SerializeField] private ZebraDirection direction = ZebraDirection.Vertical;
    [SerializeField][Range(-180f, 180f)] private float rotation = 0f;
    [SerializeField][Range(0.1f, 5f)] private float scale = 1f;

    [Header("Advanced Settings")]
    [SerializeField] private bool useWorldSpace = true;
    [SerializeField] private bool animateStripes = false;
    [SerializeField] private float animationSpeed = 0.5f;

    public UnityEvent OnZebraSettingsChanged = new UnityEvent();

    public enum ZebraDirection
    {
        Vertical,
        Horizontal,
        Diagonal45,
        Diagonal135,
        Radial
    }

    private Dictionary<int, Material> originalMaterials = new Dictionary<int, Material>();
    private Dictionary<int, Material> zebraMaterials = new Dictionary<int, Material>();
    private List<ModelData> trackedModels = new List<ModelData>();

    private float animationOffset = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (zebraMaterial == null)
        {
            zebraMaterial = CreateDefaultZebraMaterial();
        }

        if (SelectionManager.Instance != null)
        {
            SelectionManager.Instance.OnSelectionChanged.AddListener(OnSelectionChanged);
        }

        if (PNSEditingPanel.Instance != null)
        {
            PNSEditingPanel.Instance.OnPNSControlPointsChanged.AddListener(OnPNSUpdated);
        }
    }

    private void Update()
    {
        if (animateStripes && zebraEnabled)
        {
            animationOffset += Time.deltaTime * animationSpeed;
            if (animationOffset > 1f) animationOffset -= 1f;
            UpdateAllZebraMaterials();
        }
    }

    #region Public API

    /// <summary>
    /// Enable zebra analysis for selected models
    /// </summary>
    public void EnableZebraAnalysis()
    {
        zebraEnabled = true;
        ApplyZebraToSelectedModels();
        OnZebraSettingsChanged.Invoke();
    }

    /// <summary>
    /// Disable zebra analysis
    /// </summary>
    public void DisableZebraAnalysis()
    {
        zebraEnabled = false;
        RestoreOriginalMaterials();
        OnZebraSettingsChanged.Invoke();
    }

    /// <summary>
    /// Toggle zebra analysis on/off
    /// </summary>
    public void ToggleZebraAnalysis()
    {
        if (zebraEnabled)
            DisableZebraAnalysis();
        else
            EnableZebraAnalysis();
    }

    public bool IsZebraEnabled() => zebraEnabled;

    /// <summary>
    /// Update zebra stripe count
    /// </summary>
    public void SetStripeCount(int count)
    {
        stripeCount = Mathf.Clamp(count, 1, 50);
        UpdateAllZebraMaterials();
        OnZebraSettingsChanged.Invoke();
    }

    /// <summary>
    /// Update stripe sharpness (0 = smooth gradient, 1 = sharp edge)
    /// </summary>
    public void SetStripeSharpness(float sharpness)
    {
        stripeSharpness = Mathf.Clamp01(sharpness);
        UpdateAllZebraMaterials();
        OnZebraSettingsChanged.Invoke();
    }

    /// <summary>
    /// Set zebra stripe direction
    /// </summary>
    public void SetDirection(ZebraDirection dir)
    {
        direction = dir;
        UpdateAllZebraMaterials();
        OnZebraSettingsChanged.Invoke();
    }

    /// <summary>
    /// Set zebra rotation angle
    /// </summary>
    public void SetRotation(float angle)
    {
        rotation = angle;
        UpdateAllZebraMaterials();
        OnZebraSettingsChanged.Invoke();
    }

    /// <summary>
    /// Set zebra scale
    /// </summary>
    public void SetScale(float scaleValue)
    {
        scale = Mathf.Max(0.1f, scaleValue);
        UpdateAllZebraMaterials();
        OnZebraSettingsChanged.Invoke();
    }

    /// <summary>
    /// Set stripe colors
    /// </summary>
    public void SetStripeColors(Color color1, Color color2)
    {
        stripeColor1 = color1;
        stripeColor2 = color2;
        UpdateAllZebraMaterials();
        OnZebraSettingsChanged.Invoke();
    }

    /// <summary>
    /// Enable/disable stripe animation
    /// </summary>
    public void SetAnimateStripes(bool animate)
    {
        animateStripes = animate;
        OnZebraSettingsChanged.Invoke();
    }

    /// <summary>
    /// Set animation speed
    /// </summary>
    public void SetAnimationSpeed(float speed)
    {
        animationSpeed = speed;
    }

    #endregion

    #region Material Management

    private Material CreateDefaultZebraMaterial()
    {
        Shader zebraShader = Shader.Find("Custom/ZebraStripes");
        if (zebraShader == null)
        {
            zebraShader = Shader.Find("Standard");
        }

        Material mat = new Material(zebraShader);
        mat.name = "ZebraMaterial";
        return mat;
    }

    private void ApplyZebraToSelectedModels()
    {
        if (SelectionManager.Instance == null) return;

        var selectedModels = SelectionManager.Instance.GetSelectedModels();

        foreach (var model in selectedModels)
        {
            ApplyZebraToModel(model);
        }
    }

    private void ApplyZebraToModel(ModelData model)
    {
        if (model == null) return;

        var renderer = model.meshRender;
        if (renderer == null) return;

        if (!originalMaterials.ContainsKey(model.modelID))
        {
            originalMaterials[model.modelID] = renderer.sharedMaterial;
        }

        Material zebraMat = GetOrCreateZebraMaterial(model.modelID);
        renderer.sharedMaterial = zebraMat;

        if (!trackedModels.Contains(model))
        {
            trackedModels.Add(model);
        }
    }

    private Material GetOrCreateZebraMaterial(int modelID)
    {
        if (zebraMaterials.ContainsKey(modelID))
        {
            return zebraMaterials[modelID];
        }

        Material mat = new Material(zebraMaterial);
        mat.name = $"ZebraMaterial_{modelID}";

        UpdateZebraMaterialProperties(mat);

        zebraMaterials[modelID] = mat;
        return mat;
    }

    private void UpdateAllZebraMaterials()
    {
        foreach (var kvp in zebraMaterials)
        {
            UpdateZebraMaterialProperties(kvp.Value);
        }
    }

    private void UpdateZebraMaterialProperties(Material mat)
    {
        if (mat == null) return;

        mat.SetColor("_StripeColor1", stripeColor1);
        mat.SetColor("_StripeColor2", stripeColor2);
        mat.SetFloat("_StripeCount", stripeCount);
        mat.SetFloat("_StripeSharpness", stripeSharpness);
        mat.SetFloat("_Rotation", rotation);
        mat.SetFloat("_Scale", scale);
        mat.SetFloat("_AnimationOffset", animationOffset);
        mat.SetInt("_Direction", (int)direction);
        mat.SetInt("_UseWorldSpace", useWorldSpace ? 1 : 0);
    }

    private void RestoreOriginalMaterials()
    {
        foreach (var model in trackedModels)
        {
            if (model == null || model.meshRender == null) continue;

            if (originalMaterials.ContainsKey(model.modelID))
            {
                model.meshRender.sharedMaterial = originalMaterials[model.modelID];
            }
        }

        trackedModels.Clear();
    }

    #endregion

    #region Event Handlers

    private void OnSelectionChanged(List<ModelData> models)
    {
        if (zebraEnabled)
        {
            foreach (var model in trackedModels.ToArray())
            {
                if (!models.Contains(model))
                {
                    if (originalMaterials.ContainsKey(model.modelID))
                    {
                        model.meshRender.sharedMaterial = originalMaterials[model.modelID];
                        originalMaterials.Remove(model.modelID);
                        zebraMaterials.Remove(model.modelID);
                    }
                    trackedModels.Remove(model);
                }
            }

            ApplyZebraToSelectedModels();
        }
    }

    private void OnPNSUpdated()
    {
    }

    #endregion

    #region Cleanup

    private void OnDestroy()
    {
        RestoreOriginalMaterials();

        foreach (var mat in zebraMaterials.Values)
        {
            if (mat != null) Destroy(mat);
        }

        zebraMaterials.Clear();
        originalMaterials.Clear();

        if (SelectionManager.Instance != null)
        {
            SelectionManager.Instance.OnSelectionChanged.RemoveListener(OnSelectionChanged);
        }

        if (PNSEditingPanel.Instance != null)
        {
            PNSEditingPanel.Instance.OnPNSControlPointsChanged.RemoveListener(OnPNSUpdated);
        }
    }

    #endregion

    #region Getters

    public int GetStripeCount() => stripeCount;
    public float GetStripeSharpness() => stripeSharpness;
    public ZebraDirection GetDirection() => direction;
    public float GetRotation() => rotation;
    public float GetScale() => scale;
    public Color GetStripeColor1() => stripeColor1;
    public Color GetStripeColor2() => stripeColor2;
    public bool GetAnimateStripes() => animateStripes;
    public float GetAnimationSpeed() => animationSpeed;

    #endregion
}