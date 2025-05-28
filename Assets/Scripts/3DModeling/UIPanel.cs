using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIPanel : MonoBehaviour
{
    [Header("Menu References")]
    public Canvas menuCanvas;
    public RectTransform menuPanel;
    public Button closeButton;

    [Header("Animation Settings")]
    public float animationDuration = 0.3f;
    public AnimationCurve easeInOut = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private bool isPanelOpen = false;
    private Vector3 closedScale = Vector3.zero;
    private Vector3 openScale = Vector3.one;

    void Start()
    {


        if (closeButton != null)
            closeButton.onClick.AddListener(() => ToggleMenu());

        menuCanvas.gameObject.SetActive(false);
        menuPanel.localScale = closedScale;
    }

    public void ToggleMenu()
    {
        if (isPanelOpen)
            CloseMenu();
        else
            OpenMenu();
    }

    public void OpenMenu()
    {
        menuCanvas.gameObject.SetActive(true);
        StartCoroutine(AnimateMenuScale(closedScale, openScale));
        StartCoroutine(AnimateMenuFade(0f, 1f));
        isPanelOpen = true;
    }

    public void CloseMenu()
    {
        StartCoroutine(AnimateMenuScale(openScale, closedScale));
        StartCoroutine(AnimateMenuFade(1f, 0f));
        isPanelOpen = false;
    }

    IEnumerator AnimateMenuScale(Vector3 fromScale, Vector3 toScale)
    {
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            float t = elapsedTime / animationDuration;
            float easedT = easeInOut.Evaluate(t);

            menuPanel.localScale = Vector3.Lerp(fromScale, toScale, easedT);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        menuPanel.localScale = toScale;

        if (toScale == closedScale)
        {
            menuCanvas.gameObject.SetActive(false);
        }
    }

    IEnumerator AnimateMenuFade(float fromAlpha, float toAlpha)
    {
        CanvasGroup canvasGroup = menuPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = menuPanel.gameObject.AddComponent<CanvasGroup>();

        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            float t = elapsedTime / animationDuration;
            canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = toAlpha;
    }
}