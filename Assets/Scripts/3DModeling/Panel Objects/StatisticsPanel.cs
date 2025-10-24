using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatisticsPanel : MonoBehaviour
{
    public TMP_Text fpsText;
    public float updateInterval = 0.5f;
    [Range(0f, 1f)] public float smoothing = 0.0f;

    public int goodFPS = 50;
    public int warnFPS = 30;

    private float timeLeft;
    private float accum = 0f;
    private int frames = 0;
    private float displayedFPS = 0f;



    public TMP_Text totalObjectsText;
    public TMP_Text totalVertsText;
    public TMP_Text totalFacesText;
    public TMP_Text objectFacesText;
    public TMP_Text objectEdgesText;
    public TMP_Text objectVertsText;


    void Start()
    {
        if (fpsText == null)
        {
            Debug.LogError("FPSCounter_TMP: assign a TMP_Text component in the inspector.");
            enabled = false;
            return;
        }
        timeLeft = updateInterval;

        SelectionManager.Instance.OnSelectionChanged.AddListener(HandleSelectionChanged);
        ModelsManager.Instance.OnModelsChanged.AddListener(HandleModelsChanged);

        SelectionManager.Instance.ClearSelection();
        ModelsManager.Instance.UnTrackModel(null);
    }
    public void HandleModelsChanged(List<ModelData> currentModels)
    {
        totalObjectsText.text = currentModels.Count.ToString();


        int objectFaces = 0;
        int objectVerts = 0;
        for (int i = 0; i < currentModels.Count; i++)
        {
            objectFaces += currentModels[i].GetFacesCount();
            objectVerts += currentModels[i].GetVertCount();
        }

        totalFacesText.text = objectFaces.ToString();
        totalVertsText.text = objectVerts.ToString();
    }
    public void HandleSelectionChanged(List<ModelData> currentSelection)
    {
        int objectFaces = 0;
        int objectEdges = 0;
        int objectVerts = 0;
        for (int i = 0; i < currentSelection.Count; i++)
        {
            objectFaces += currentSelection[i].GetFacesCount();
            objectEdges += currentSelection[i].GetEdgesCount();
            objectVerts += currentSelection[i].GetVertCount();
        }

        objectFacesText.text = objectFaces.ToString();
        objectEdgesText.text = objectEdges.ToString();
        objectVertsText.text = objectVerts.ToString();
    }


    




    void Update()
    {
        float fps = (1f / Mathf.Max(Time.unscaledDeltaTime, 1e-6f));
        if (smoothing > 0f)
            displayedFPS = Mathf.Lerp(displayedFPS, fps, smoothing);
        else
        {
            accum += fps;
            frames++;
        }

        timeLeft -= Time.unscaledDeltaTime;
        if (timeLeft <= 0f)
        {
            if (smoothing == 0f)
                displayedFPS = (frames > 0) ? (accum / frames) : 0f;

            int f = Mathf.RoundToInt(displayedFPS);
            float ms = (displayedFPS > 0f) ? (1000f / displayedFPS) : 0f;
            fpsText.text = $"<b>{f} fps</b>\n{ms:0.0} ms";

            // color by thresholds
            if (f >= goodFPS) fpsText.color = Color.green;
            else if (f >= warnFPS) fpsText.color = Color.yellow;
            else fpsText.color = Color.red;

            accum = 0f;
            frames = 0;
            timeLeft = updateInterval;
        }
    }





}