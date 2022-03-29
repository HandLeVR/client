using translator;
using UnityEngine.Events;

/// <summary>
/// Represents a task collection entry in the task collection menu.
/// </summary>
public class TaskCollectionTableElement : BasicTableElement
{
    public TaskCollection taskCollection;

    public void Init(TaskCollection taskCollection, UnityAction<TaskCollectionTableElement, bool> onClick,
        UnityAction<TaskCollectionTableElement, bool> onDelete)
    {
        this.taskCollection = taskCollection;

        text1.text = taskCollection.name;
        text2.text = TranslationController.Instance.Translate(taskCollection.taskClass.ToString());
        text3.text = taskCollection.taskCollectionElements.Count.ToString();
        containerButton.onClick.RemoveAllListeners();
        containerButton.onClick.AddListener(() => onClick(this, false));
        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(() => onDelete(this, false));
        deleteButton.gameObject.SetActive(taskCollection.permission.editable);
        infoButton.onClick.RemoveAllListeners();
        infoButton.onClick.AddListener(() => PopupScreenHandler.Instance.ShowInfos(taskCollection.permission));
    }
}
