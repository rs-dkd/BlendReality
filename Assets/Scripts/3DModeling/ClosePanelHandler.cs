using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ClosePanelHandler : MonoBehaviour
{
    [Header("Exit Confirmation")]
    public GameObject confirmationDialog;
    public Button confirmButton;
    public Button cancelButton;
    public CanvasGroup fadeCanvas;

    void Start()
    {
        if (confirmButton != null)
            confirmButton.onClick.AddListener(ConfirmExit);
        if (cancelButton != null)
            cancelButton.onClick.AddListener(CancelExit);
    }

    public void ShowExitConfirmation()
    {
        if (confirmationDialog != null)
        {
            confirmationDialog.SetActive(true);
            StartCoroutine(FadeInDialog());
        }
    }

    void ConfirmExit()
    {
        StartCoroutine(ExitWithAnimation());
    }

    void CancelExit()
    {
        if (confirmationDialog != null)
            confirmationDialog.SetActive(false);
    }

    IEnumerator FadeInDialog()
    {
        CanvasGroup dialogCanvas = confirmationDialog.GetComponent<CanvasGroup>();
        if (dialogCanvas == null)
            dialogCanvas = confirmationDialog.AddComponent<CanvasGroup>();

        float duration = 0.3f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            dialogCanvas.alpha = Mathf.Lerp(0f, 1f, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        dialogCanvas.alpha = 1f;
    }

    IEnumerator ExitWithAnimation()
    {
        yield return StartCoroutine(FadeOutApplication());

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
    }

    IEnumerator FadeOutApplication()
    {
        if (fadeCanvas == null) yield break;

        float duration = 1f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            fadeCanvas.alpha = Mathf.Lerp(0f, 1f, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        fadeCanvas.alpha = 1f;
    }
}