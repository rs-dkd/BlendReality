using System;
using UnityEngine;
using UnityEngine.UI;

public class confirmationPanel : BasePanel
{
    private Action<bool> currentDecision;

    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;


    void Awake()
    {
        HidePanel();
        yesButton.onClick.AddListener(() => OnClick(true));
        noButton.onClick.AddListener(() => OnClick(false));
    }
    private new void ShowPanel()
    {
        base.ShowPanel();    
    }

    public void ShowPanel(string confirmationText, Action<bool> decision)
    {
        this.titleTMP.text = confirmationText;
        currentDecision = decision;
        ShowPanel();
    }

    private void OnClick(bool result)
    {
        HidePanel();
        currentDecision?.Invoke(result);
        currentDecision = null;
    }

}