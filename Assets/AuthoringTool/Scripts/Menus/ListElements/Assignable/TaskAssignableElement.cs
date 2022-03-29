using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Represents a task list element for the task collection menu.
/// </summary>
public class TaskAssignableElement : BasicAssignableElement
{
    public TaskCollectionElement taskCollectionElement;
    public Toggle mandatoryToggle;

    public void Init(TaskCollectionElement taskCollectionElement, bool modifiable,
        UnityAction<TaskAssignableElement> onDelete, UnityAction onChange)
    {
        this.taskCollectionElement = taskCollectionElement;

        nameInputField.text = taskCollectionElement.task.name;
        descriptionInputField.text = taskCollectionElement.task.description;

        if (onDelete != null)
        {
            indexLabel.text = (transform.GetSiblingIndex() + 1).ToString();
            deleteButton.onClick.AddListener(() => onDelete(this));
            deleteButton.interactable = modifiable;
            mandatoryToggle.gameObject.SetActive(true);
            mandatoryToggle.isOn = taskCollectionElement.mandatory;
            mandatoryToggle.onValueChanged.AddListener(_ => onChange.Invoke());
        }
        else
        {
            indexLabel.gameObject.SetActive(false);
            deleteButton.gameObject.SetActive(false);
            mandatoryToggle.gameObject.SetActive(false);
        }

        if (!modifiable)
            Destroy(GetComponent<BaseDragHandler>());
        nameInputField.textComponent.alignment = TextAlignmentOptions.Left;
        descriptionInputField.textComponent.alignment = TextAlignmentOptions.Left;
    }
}