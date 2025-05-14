using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundEnv : MonoBehaviour
{
    [SerializeField] private Color backgroundColor = new Color(0.2f, 0.2f, 0.2f); // Blender's dark gray
    [SerializeField] private Color gridColor = new Color(0.5f, 0.5f, 0.5f, 0.3f); // Blender's grid
    [SerializeField] private Material skyboxMaterial;

    void Start()
    {
        RenderSettings.skybox = skyboxMaterial;
        RenderSettings.ambientLight = new Color(0.35f, 0.35f, 0.35f);
        Camera.main.backgroundColor = backgroundColor;
    }
}
