using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class BasePanel : MonoBehaviour
{

    [SerializeField] protected string title;
    [SerializeField] protected Button closeButton;
    [SerializeField] protected Image basePanelBackground;
    [SerializeField] protected RectTransform basePanel; 

    protected bool isClosed;
    protected TMPro.TMP_Text titleTMP;



    [Header("Animation Settings")]
    [SerializeField] protected float animationDuration = 0.3f;
    [SerializeField] protected AnimationCurve easeInOut = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Vector3 closedScale = Vector3.zero;
    private Vector3 openScale = Vector3.one;






    private void Awake()
    {
        titleTMP.text = title;
    }


    public void ShowPanel()
    {
        basePanel.gameObject.SetActive(true);
        StartCoroutine(AnimateMenuScale(closedScale, openScale));
        StartCoroutine(AnimateMenuFade(0f, 1f));
        isClosed = false;
    }

    public void HidePanel()
    {
        basePanel.gameObject.SetActive(false);
        StartCoroutine(AnimateMenuScale(openScale, closedScale));
        StartCoroutine(AnimateMenuFade(1f, 0f));
        isClosed = false;        
    }

    IEnumerator AnimateMenuScale(Vector3 fromScale, Vector3 toScale)
    {
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            float t = elapsedTime / animationDuration;
            float easedT = easeInOut.Evaluate(t);

            basePanel.localScale = Vector3.Lerp(fromScale, toScale, easedT);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        basePanel.localScale = toScale;

        if (toScale == closedScale)
        {
            basePanel.gameObject.SetActive(false);
        }
    }

    IEnumerator AnimateMenuFade(float fromAlpha, float toAlpha)
    {
        CanvasGroup canvasGroup = basePanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = basePanel.gameObject.AddComponent<CanvasGroup>();

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