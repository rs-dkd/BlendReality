using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class MenuItemComponent : MonoBehaviour
{
    [Header("UI References")]
    public Image iconImage;
    public Text itemText;
    public Button button;
    public Image backgroundImage;

    [Header("Visual States")]
    public Color normalColor = new Color(0.18f, 0.18f, 0.18f, 0.8f);
    public Color hoverColor = new Color(0.17f, 0.5f, 0.86f, 0.8f);
    public Color disabledColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);

    private MenuItemData itemData;

    public void SetupMenuItem(MenuItemData data)
    {
        itemData = data;
        itemText.text = data.itemName;
        iconImage.sprite = data.icon;

        if (data.isImplemented)
        {
            button.onClick.AddListener(() => data.onSelect.Invoke());
            SetupHoverEffects();
        }
        else
        {
            button.interactable = false;
            backgroundImage.color = disabledColor;
            itemText.color = Color.grey;
        }
    }

    void SetupHoverEffects()
    {
        EventTrigger trigger = gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry hoverEnter = new EventTrigger.Entry();
        hoverEnter.eventID = EventTriggerType.PointerEnter;
        hoverEnter.callback.AddListener((data) => OnHoverEnter());
        trigger.triggers.Add(hoverEnter);

        EventTrigger.Entry hoverExit = new EventTrigger.Entry();
        hoverExit.eventID = EventTriggerType.PointerExit;
        hoverExit.callback.AddListener((data) => OnHoverExit());
        trigger.triggers.Add(hoverExit);
    }

    void OnHoverEnter()
    {
        backgroundImage.color = hoverColor;
        StartCoroutine(ScaleAnimation(1f, 1.05f, 0.1f));
    }

    void OnHoverExit()
    {
        backgroundImage.color = normalColor;
        StartCoroutine(ScaleAnimation(1.05f, 1f, 0.1f));
    }

    IEnumerator ScaleAnimation(float from, float to, float duration)
    {
        float elapsedTime = 0f;
        Vector3 fromScale = Vector3.one * from;
        Vector3 toScale = Vector3.one * to;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            transform.localScale = Vector3.Lerp(fromScale, toScale, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localScale = toScale;
    }
}