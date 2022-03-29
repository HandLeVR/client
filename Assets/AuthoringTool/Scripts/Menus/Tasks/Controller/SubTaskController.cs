using System.Collections.Generic;
using translator;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class provides methods to creat sub task elements after dragging them into the list of sub tasks for a task.
/// </summary>
public class SubTaskController : Singleton<SubTaskController>
{
    public Transform availableSubTasksPanel;
    public Transform usedSubTasksPanel;
    public BaseSubTaskContainer basicSubTaskPrefab;
    public List<BaseSubTaskContainer> prefabs;

    [HideInInspector] public BaseSubTaskContainer currentSelection;

    void Start()
    {
        // instantiates all available sub task which can be dragged into the sub task list of the task
        foreach (BaseSubTaskContainer prefab in prefabs)
        {
            BaseSubTaskContainer subTaskContainer = Instantiate(basicSubTaskPrefab, availableSubTasksPanel);
            subTaskContainer.GetComponent<TooltipObject>().tooltipText =
                TranslationController.Instance.TranslateSubTaskType(prefab.type, true);
            SubTask subTask = new SubTask
                { name = TranslationController.Instance.TranslateSubTaskType(prefab.type), type = prefab.type };
            subTaskContainer.SubTaskData = subTask;
        }
    }

    /// <summary>
    /// When dragging, the original prefab is moved, so we need to create a clone at the originals positions to take the originals place.
    /// </summary>
    public void CloneSubTask(int siblingIndex)
    {
        BaseSubTaskContainer newOriginal = Instantiate(currentSelection, availableSubTasksPanel);
        newOriginal.SubTaskData = currentSelection.SubTaskData.Copy();
        newOriginal.transform.SetSiblingIndex(siblingIndex);
    }

    /// <summary>
    /// Creates a sub task element for the list of used sub tasks by a task.
    /// </summary>
    public BaseSubTaskContainer CreateSubTaskComponent(SubTask subTask, Transform parent)
    {
        BaseSubTaskContainer newPrefab = prefabs.Find(container => container.type == subTask.type);
        newPrefab = Instantiate(newPrefab, parent);
        newPrefab.SetUpBySubTask(subTask);
        newPrefab.SubTaskData = subTask;
        Button button = newPrefab.GetComponent<Button>();
        button.onClick.AddListener(() =>
            TaskController.Instance.ShowCorrespondingSettingPanel(subTask.type, newPrefab));
        button.Select();
        button.onClick.Invoke();
        return newPrefab;
    }

    /// <summary>
    /// Updates the index of the sub task in dependence of the position in the list.
    /// </summary>
    public void UpdateSubTaskNumbers()
    {
        int add = 1;
        foreach (Transform child in usedSubTasksPanel)
        {
            BaseSubTaskContainer subTaskContainer = child.gameObject.GetComponent<BaseSubTaskContainer>();
            if (subTaskContainer != null)
                subTaskContainer.ID = child.transform.GetSiblingIndex() + add;
            // we need te reduce the add value after the preview element because it is deactivated in the next frame
            else
                add--;
        }
    }
}