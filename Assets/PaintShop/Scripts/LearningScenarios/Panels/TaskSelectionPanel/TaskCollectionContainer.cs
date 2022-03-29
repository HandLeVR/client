using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Represents a task collection entry in the task list of the task selection panel.
/// </summary>
public class TaskCollectionContainer : MonoBehaviour
{
    public TextMeshProUGUI title;
    public TextMeshProUGUI buttonText;

    public TaskCollection taskCollection;
    private List<TaskContainer> subElements;

    public void Instantiate(TaskCollection taskCollection, List<TaskContainer> subElements)
    {
        this.taskCollection = taskCollection;
        this.subElements = subElements;
        title.text = taskCollection.name;
        subElements[subElements.Count - 1].SetToLastElement();
        ShowSubElements(false);
    }

    public void ShowSubElements()
    {
        ShowSubElements(!IsOpen());
    }

    public bool IsOpen()
    {
        return buttonText.text.Equals("- ");
    }

    public void ShowSubElements(bool show)
    {
        subElements.ForEach(s => s.gameObject.SetActive(show));
        buttonText.text = show ? "- " : "+";
    }
}
