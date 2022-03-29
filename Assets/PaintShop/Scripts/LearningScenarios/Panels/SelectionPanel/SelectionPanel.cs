using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Manages the generation and selection of items in the selection panel. This panel is used in different sub tasks
/// where in item needs to be selected with the balls method.
/// </summary>
public class SelectionPanel : Singleton<SelectionPanel>
{
    public TextMeshProUGUI question;
    public Transform choiceItemContainer;
    public ChoiceItem choiceItemPrefab;
    public SelectionTable selectionTable;
    public CanvasGroup infoContainer;
    public Color wrongColor;
    public Color correctColor;
    public Color normalColor;
    public Color unselectedColor;
    public Color selectedColor;
    public float selectionDistance = 0.1f;

    [HideInInspector] public List<ChoiceItem> items;
    [HideInInspector] public ChoiceItem selectedChoice;

    private TextMeshProUGUI _infoText;
    private Image _infoBackground;
    private CanvasGroup _canvasGroup;
    private bool _isDirty;

    private void Update()
    {
        if (!_isDirty) return;
        question.text = question.text + " ";
        _isDirty = false;
    }

    private void Awake()
    {
        _infoText = infoContainer.GetComponentInChildren<TextMeshProUGUI>();
        _infoBackground = infoContainer.GetComponent<Image>();
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void FadeIn(string text, int ballNumber)
    {
        gameObject.SetActive(true);
        question.transform.parent.parent.gameObject.SetActive(!string.IsNullOrEmpty(text));
        question.text = text;
        choiceItemContainer.DestroyAllChildren();
        items = new List<ChoiceItem>();
        selectionTable.FadeIn(ballNumber);
        StartCoroutine(Lerp.Alpha(_canvasGroup, 1, 0.5f));
        infoContainer.alpha = 0;
        // WaitFor needed to solve problem where the size of the question container is not updated after settings the text.
        StartCoroutine(WaitFor.DoEndOfFrame(() => _isDirty = true));
    }

    public void FadeOut(bool fadeOutTable = true, UnityAction onComplete = null)
    {
        selectionTable.FadeOut(fadeOutTable);
        if (_canvasGroup.alpha > 0)
            StartCoroutine(Lerp.Alpha(_canvasGroup, 0, 0.5f,() => onComplete?.Invoke()));
    }

    public void ShowWarning(string text)
    {
        infoContainer.alpha = 1;
        _infoText.text = text;
        _infoBackground.color = wrongColor;
    }

    public void ShowInfo(string text)
    {
        infoContainer.alpha = 1;
        _infoText.text = text;
        _infoBackground.color = normalColor;
    }

    public void EvaluateAnswers()
    {
        foreach (ChoiceItem item in items)
            item.background.color = item.isCorrect ? correctColor : wrongColor;

        selectionTable.DisableBalls();
    }

    public void AddItem(string text, bool isCorrect = false, UnityAction onSelection = null)
    {
        ChoiceItem newItem = Instantiate(choiceItemPrefab, choiceItemContainer);
        newItem.text.text = text;
        newItem.isCorrect = isCorrect;
        newItem.onSelection = onSelection;
        items.Add(newItem);
        _isDirty = true;
    }

    /// <summary>
    /// Finds the choice (circle of the choice) the ball dragged by the user is closest to and checks whether the
    /// distance to the choice is closer than the min distance. If yes the circle is highlighted.
    /// </summary>
    public void CheckDistance(Transform transform)
    {
        ChoiceItem currentImage = null;
        float currentDistance = Mathf.Infinity;
        foreach (ChoiceItem choiceItem in items)
        {
            float distance = Vector3.Distance(transform.position, choiceItem.circle.transform.position);
            if (distance <= currentDistance && distance <= selectionDistance)
            {
                currentDistance = distance;
                currentImage = choiceItem;
                choiceItem.circle.color = selectedColor;
            }
        }

        selectedChoice = currentImage;
        UpdateSelected();
    }

    /// <summary>
    /// Highlights or the selected choice and the choices already containing a ball.
    /// </summary>
    public void UpdateSelected()
    {
        items.ForEach(item => item.circle.color = item.HasBall ? selectedColor : unselectedColor);
        if (selectedChoice)
            selectedChoice.circle.color = selectedColor;
    }
}