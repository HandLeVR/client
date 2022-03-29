using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Base class for all support infos that use slides.
/// </summary>
public abstract class SlidePanel : MonoBehaviour
{
    public TextMeshProUGUI headerField;
    public TextMeshProUGUI currentPageTextField;
    public Button backButton;
    public Button nextButton;
    
    [HideInInspector] public bool showMediaName;
    
    protected int currentPage;

    private const string CurrentPageTextTemplate = "{0} / {1}";
    
    private void OnEnable()
    {
        currentPage = 0;
        headerField.gameObject.SetActive(showMediaName);
        currentPageTextField.gameObject.SetActive(GetSlideCount() > 1);
        backButton.gameObject.SetActive(GetSlideCount() > 1);
        nextButton.gameObject.SetActive(GetSlideCount() > 1);
        UpdatePanel();
    }
    
    public void NextPage()
    {
        currentPage++;
        UpdatePanel();
    }

    public void PrevPage()
    {
        currentPage--;
        UpdatePanel();
    }

    private void UpdatePanel()
    {
        SetSlideContent();
        currentPageTextField.text = String.Format(CurrentPageTextTemplate, currentPage + 1, GetSlideCount());
        backButton.interactable = currentPage > 0;
        nextButton.interactable = currentPage < GetSlideCount() - 1;
    }

    protected abstract int GetSlideCount();

    protected abstract void SetSlideContent();
}
