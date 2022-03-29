using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using translator;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Base class for all menu controller. Provides methods used in the menu controller.
/// </summary>
public class BaseMenuController : MonoBehaviour
{
    public Button saveButton;
    
    private bool _unsavedChanges;
    
    protected readonly Color warningColor = new Color32(215, 11, 82, 64);
    protected readonly Color normalColor = Color.white;
    protected readonly Color disabledColor = new Color(200 / 255f, 200 / 255f, 200 / 255f, 128 / 255f);

    /// <summary>
    /// Sets the unsaved change and makes the save button interactable accordingly.
    /// </summary>
    public virtual void SetUnsavedChanges(bool newUnsavedChanges)
    {
        _unsavedChanges = newUnsavedChanges;
        if (saveButton)
            saveButton.interactable = newUnsavedChanges;
    }

    public virtual bool HasUnsavedChanges()
    {
        return _unsavedChanges;
    }

    /// <summary>
    /// Initializes a dropdown which is used for the selection of a task class.
    /// </summary>
    protected void SetTaskClassDropdown(TMP_Dropdown dropdown, TaskClass tc)
    {
        dropdown.options.Clear();
        foreach (TaskClass taskClass in Enum.GetValues(typeof(TaskClass)))
            dropdown.options.Add(
                new TMP_Dropdown.OptionData(TranslationController.Instance.Translate(taskClass.ToString())));
        SetDropdown(dropdown,
            dropdown.options.FindIndex(i => i.text.Equals(TranslationController.Instance.Translate(tc.ToString()))));
    }

    /// <summary>
    /// Initializes a dropdown.
    /// </summary>
    protected void SetDropdown(TMP_Dropdown dropdown, int value)
    {
        dropdown.value = value;
        dropdown.RefreshShownValue();
        dropdown.onValueChanged.AddListener(_ => SetUnsavedChanges(true));
    }

    /// <summary>
    /// Adds a the SetUnsavedChanges method as a callback for the event onValueChanged for all input fields in
    /// the given list of selectables.
    /// </summary>
    protected void AddSetUnsavedChangesListener(List<Selectable> selectables)
    {
        foreach (var selectable in selectables)
        {
            TMP_InputField inputField = selectable.GetComponent<TMP_InputField>();
            if (inputField != null)
                inputField.onValueChanged.AddListener(_ => SetUnsavedChanges(true));
        }
    }

    /// <summary>
    /// Sorts all children of the given transform by the given comparison operation.
    /// </summary>
    protected void SortListElements<T>(Transform list, Comparison<T> comparison) where T : Component
    {
        List<T> elements = list.GetComponentsInChildren<T>().ToList();
        elements.Sort(comparison);
        for (int i = 0; i < elements.Count; ++i)
            elements[i].transform.SetSiblingIndex(i);
        
    }
}
