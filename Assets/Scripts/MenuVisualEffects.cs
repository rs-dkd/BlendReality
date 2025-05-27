using UnityEngine;
using UnityEngine.UI;

public class MenuVisualEffects : MonoBehaviour
{
    [Header("Background Effects")]
    public Image backgroundPanel;
    public float pulseSpeed = 2f;
    public float pulseIntensity = 0.1f;

    private Color baseColor;

    void Start()
    {
        if (backgroundPanel != null)
            baseColor = backgroundPanel.color;
    }

    void Update()
    {
        if (backgroundPanel != null)
        {
            // Subtle background pulse
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity;
            backgroundPanel.color = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a + pulse);
        }
    }
}