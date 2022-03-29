using translator;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Represents a task entry in the task menu.
/// </summary>
public class TaskTableElement : BasicTableElement
{
    public Button exportButton;

    public Task task;

    public void Init(Task task, UnityAction<TaskTableElement> onClick, UnityAction<Task> onExport,
        UnityAction<TaskTableElement, bool> onDelete)
    {
        this.task = task;

        text1.text = task.name;
        text2.text = TranslationController.Instance.Translate(task.taskClass.ToString());
        text3.text = task.subTasks.Count.ToString();
        containerButton.onClick.RemoveAllListeners();
        containerButton.onClick.AddListener(() => onClick(this));
        exportButton.onClick.RemoveAllListeners();
        exportButton.onClick.AddListener(() => onExport(task));
        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(() => onDelete(this, false));
        deleteButton.gameObject.SetActive(task.permission.editable);
        infoButton.onClick.RemoveAllListeners();
        infoButton.onClick.AddListener(() => PopupScreenHandler.Instance.ShowInfos(task.permission));
    }
}
