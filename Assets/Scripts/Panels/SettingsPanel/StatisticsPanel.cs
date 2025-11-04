using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Manages the display of real-time application statistics, including FPS,
/// and geometric data (objects, vertices, faces) for the entire scene and selected models.
/// </summary>
public class StatisticsPanel : MonoBehaviour
{
    // --- Singleton instance ---
    public static StatisticsPanel Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple instances of StatisticsPanel detected! Destroying duplicate.");
            Destroy(gameObject);
        }

    }

    // --- Variables ---
    [Header("FPS Monitoring")]
    [Tooltip("Text component used to display the calculated frames per second")]
    [SerializeField] private TMP_Text fpsText;

    [Tooltip("The time interval between FPS updates.")]
    [SerializeField] private float updateInterval = 0.5f;

    [Tooltip("The amount of linear interpolation applied to the FPS value.")]
    [Range(0f, 1f)]
    [SerializeField] private float smoothing = 0.0f;

    [Tooltip("FPS value considered Good (Green).")]
    [SerializeField] private int goodFPS = 50;

    [Tooltip("FPS value considered Warning (Yellow).")]
    [SerializeField] private int warnFPS = 30;

    [Header("Scene Geometry Statistics")]
    [Tooltip("Text component displaying the total number of trackable objects in the scene.")]
    [SerializeField] private TMP_Text totalObjectsText;

    [Tooltip("Text component displaying the total number of vertices for all scene objects.")]
    [SerializeField] private TMP_Text totalVertsText;

    [Tooltip("Text component displaying the total number of faces/triangles for all scene objects.")]
    [SerializeField] private TMP_Text totalFacesText;

    [Header("Selection Geometry Statistics")]
    [Tooltip("Text component displaying the face count of the currently selected objects.")]
    [SerializeField] private TMP_Text objectFacesText;

    [Tooltip("Text component displaying the edge count of the currently selected objects.")]
    [SerializeField] private TMP_Text objectEdgesText;

    [Tooltip("Text component displaying the vertex count of the currently selected objects.")]
    [SerializeField] private TMP_Text objectVertsText;


    // Time remaining until the next FPS update.
    private float timeLeft;

    // Accumulated FPS over the current interval (used when smoothing is 0).
    private float accum = 0f;

    // Number of frames counted in the current interval (used when smoothing is 0).
    private int frames = 0;

    // The final FPS value currently being displayed.
    private float displayedFPS = 0f;


    // --- Main Functions ---

    void Start()
    {
        // Initialize the FPS countdown timer.
        timeLeft = updateInterval;

        // Subscribe to global events for real-time data updates.
        SelectionManager.Instance.OnSelectionChanged.AddListener(HandleSelectionChanged);
        ModelsManager.Instance.OnModelsChanged.AddListener(HandleModelsChanged);

        // Run initial updates to populate the panel with current data.
        SelectionManager.Instance.ClearSelection();
        ModelsManager.Instance.UnTrackModel(null); 
    }

    /// <summary>
    /// Called once per frame. Handles FPS calculation and periodic UI update.
    /// </summary>
    void Update()
    {
        // Calculate raw frames per second
        float fps = (1f / Mathf.Max(Time.unscaledDeltaTime, 1e-6f)); 

        // Check if smoothing is enabled
        if (smoothing > 0f)
        {
            //Smooth the FPS using Linear Interpolation (Lerp)
            displayedFPS = Mathf.Lerp(displayedFPS, fps, smoothing);
        }
        else
        {
            //Accumulate data for frame averaging
            accum += fps;
            frames++;
        }

        timeLeft -= Time.unscaledDeltaTime;

        //Time to update the display
        if (timeLeft <= 0f)
        {
            if (smoothing == 0f)
            {
                // Calculate the average FPS for the interval
                displayedFPS = (frames > 0) ? (accum / frames) : 0f;
            }

            int f = Mathf.RoundToInt(displayedFPS);
            float ms = (displayedFPS > 0f) ? (1000f / displayedFPS) : 0f;

            // Format and display the FPS and milliseconds
            fpsText.text = $"<b>{f} fps</b>\n{ms:0.0} ms";

            //Color code the FPS text based on thresholds
            if (f >= goodFPS)
                fpsText.color = Color.green;
            else if (f >= warnFPS)
                fpsText.color = Color.yellow;
            else
                fpsText.color = Color.red;

            //Reset counters for the next interval
            accum = 0f;
            frames = 0;
            timeLeft = updateInterval;
        }
    }

    /// <summary>
    /// Called when the GameObject is being destroyed. Unsubscribes from events.
    /// </summary>
    void OnDestroy()
    {
        if (SelectionManager.Instance != null)
        {
            SelectionManager.Instance.OnSelectionChanged.RemoveListener(HandleSelectionChanged);
        }
        if (ModelsManager.Instance != null)
        {
            ModelsManager.Instance.OnModelsChanged.RemoveListener(HandleModelsChanged);
        }
    }


    // --- Event Handlers ---


    // TODO: Add functions to update stats on mesh operations that add or remove verts, edges, faces


    /// <summary>
    /// Updates the total object counts and geometry data when the list of trackable models changes.
    /// </summary>
    public void HandleModelsChanged(List<ModelData> currentModels)
    {
        totalObjectsText.text = currentModels.Count.ToString();

        int totalFaces = 0;
        int totalVerts = 0;

        // Sum up geometry counts from all models
        for (int i = 0; i < currentModels.Count; i++)
        {
            totalFaces += currentModels[i].GetFacesCount();
            totalVerts += currentModels[i].GetVertCount();
        }

        totalFacesText.text = totalFaces.ToString();
        totalVertsText.text = totalVerts.ToString();
    }

    /// <summary>
    /// Updates the selection statistics (faces, edges, verts) when the selection changes.
    /// </summary>
    public void HandleSelectionChanged(List<ModelData> currentSelection)
    {
        int objectFaces = 0;
        int objectEdges = 0;
        int objectVerts = 0;

        // Sum up geometry counts from selected models
        for (int i = 0; i < currentSelection.Count; i++)
        {
            objectFaces += currentSelection[i].GetFacesCount();
            objectEdges += currentSelection[i].GetEdgesCount();
            objectVerts += currentSelection[i].GetVertCount();
        }

        // Update the display texts for selected geometry
        objectFacesText.text = objectFaces.ToString();
        objectEdgesText.text = objectEdges.ToString();
        objectVertsText.text = objectVerts.ToString();
    }
}