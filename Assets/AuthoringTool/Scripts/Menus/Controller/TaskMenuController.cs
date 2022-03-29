using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controller for the task menu.
/// </summary>
public class TaskMenuController : BaseMenuController
{
    public Transform taskList;
    public TaskTableElement taskTableElementPrefab;
    public TaskController taskCreationView;

    public TMP_InputField nameInputField;
    public TMP_InputField descriptionInputField;
    public TMP_Dropdown taskClassDropdown;
    public Toggle toggleIsPartTaskPractise;
    public Transform subTaskList;
    public SubTaskAssignableElement subTaskAssignableElementPrefab;
    public Button addButton;
    public Button importButton;
    public Button editButton;

    private TaskTableElement _currentContainer;

    private void Awake()
    {
        addButton.onClick.AddListener(() => OpenTaskCreationScene());
        importButton.onClick.AddListener(() => ImportExportController.Instance.ImportTask(AfterImport));
        editButton.interactable = false;
        editButton.onClick.AddListener(() => OpenTaskCreationScene(_currentContainer.task));
    }

    private void AfterImport()
    {
        ResetFields();
        taskList.DestroyImmediateAllChildren();
        MainScreenController.Instance.LoadData(DataController.RequestType.Tasks,
            () =>
            {
                InstantiateContainer();
                PopupScreenHandler.Instance.ShowMessage("popup-import-tasks", "popup-imported-tasks");
            });
    }

    void OnEnable()
    {
        taskCreationView.gameObject.SetActive(false);
        ResetFields();
        taskList.DestroyImmediateAllChildren();
        MainScreenController.Instance.LoadData(DataController.RequestType.Tasks, InstantiateContainer);
    }

    public override bool HasUnsavedChanges()
    {
        return taskCreationView.unsavedChanges || base.HasUnsavedChanges();
    }

    public override void SetUnsavedChanges(bool newUnsavedChanges)
    {
        base.SetUnsavedChanges(newUnsavedChanges);
        taskCreationView.unsavedChanges = newUnsavedChanges;
    }

    private void InstantiateContainer()
    {
        taskList.DestroyAllChildren();
        foreach (var task in DataController.Instance.tasks.Values)
        {
            TaskTableElement container = Instantiate(taskTableElementPrefab, taskList);
            container.Init(task, SetUpByTask, ExportTask, DeleteTask);
        }

        SortTasks();
    }

    private void ExportTask(Task task)
    {
        SelectionPopup.Instance.Init(typeof(Task), ImportExportController.Instance.ExportTasks,
            selectButtonLabel: "selection-popup-button-export");
        SelectionPopup.Instance.taskPreviewPanel.SetDeadlineSelectionActive(false);
        SelectionPopup.Instance.SelectElement(task.id);
    }

    private void DeleteTask(TaskTableElement container, bool confirmed)
    {
        if (!confirmed)
        {
            PopupScreenHandler.Instance.ShowConfirmation("popup-remove-task", "popup-remove-task-confirm",
                () => DeleteTask(container, true));
            return;
        }

        PopupScreenHandler.Instance.ShowLoadingScreen("popup-remove-task", "popup-removing-task");
        RestConnector.Delete(container.task, "/tasks/" + container.task.id, () =>
            {
                if (_currentContainer != null && container.task.id == _currentContainer.task.id)
                    ResetFields();
                Destroy(container.gameObject);
                PopupScreenHandler.Instance.ShowMessage("popup-remove-task", "popup-removed-task");
            }, conflict =>
            {
                PopupScreenHandler.Instance.ShowMessage("popup-remove-task",
                    conflict == "TASKASSIGNMENT" ? "popup-remove-task-user" : "popup-remove-task-task-collection");
            },
            PopupScreenHandler.Instance.ShowConnectionError);
    }

    /// <summary>
    /// Opens another Scene for creating (or editing) a new task.
    /// </summary>
    private void OpenTaskCreationScene(Task task = null)
    {
        if (task == null)
        {
            taskCreationView.gameObject.SetActive(true);
            taskCreationView.NewTask(true);
        }
        else
        {
            PopupScreenHandler.Instance.ShowLoadingScreen("popup-load-task", "popup-loading-task");
            RestConnector.GetObject<Task>("/tasks/" + task.id, updatedTask =>
            {
                PopupScreenHandler.Instance.Close();
                taskCreationView.gameObject.SetActive(true);
                taskCreationView.LoadTask(updatedTask);
            }, PopupScreenHandler.Instance.ShowConnectionError);
        }
    }

    /// <summary>
    /// Returns to the task view where all available tasks are listed.
    /// </summary>
    public void ReturnToTaskManagementView(bool confirmed)
    {
        if (taskCreationView.unsavedChanges && !confirmed)
        {
            PopupScreenHandler.Instance.ShowUnsavedChanges(() => ReturnToTaskManagementView(true));
            return;
        }

        taskCreationView.unsavedChanges = false;
        taskCreationView.gameObject.SetActive(false);
        MainScreenController.Instance.LoadData(DataController.RequestType.Tasks, InstantiateContainer);
        InstantiateContainer();
    }

    private void ResetFields()
    {
        _currentContainer = null;
        nameInputField.text = string.Empty;
        descriptionInputField.text = string.Empty;
        taskClassDropdown.value = 0;
        toggleIsPartTaskPractise.isOn = false;
        editButton.interactable = false;
        subTaskList.DestroyAllChildren();
        SetUnsavedChanges(false);
    }

    /// <summary>
    /// The fields get filled with information given by a task.
    /// </summary>
    private void SetUpByTask(TaskTableElement container)
    {
        _currentContainer = container;
        editButton.interactable = true;
        nameInputField.text = container.task.name;
        descriptionInputField.text = container.task.description;
        StartCoroutine(SetInputFieldWrapping());
        SetTaskClassDropdown(taskClassDropdown, container.task.taskClass);
        toggleIsPartTaskPractise.isOn = container.task.partTaskPractice;
        ShowIncludedElements();
    }

    private IEnumerator SetInputFieldWrapping()
    {
        yield return new WaitForEndOfFrame();
        descriptionInputField.textComponent.enableWordWrapping = true;
    }

    /// <summary>
    /// For each SubTask in the task's subtask list, a new game object is instantiated.
    /// </summary>
    private void ShowIncludedElements()
    {
        subTaskList.DestroyImmediateAllChildren();
        foreach (SubTask subtask in _currentContainer.task.subTasks)
            Instantiate(subTaskAssignableElementPrefab, subTaskList).Init(subtask);
    }

    private void SortTasks()
    {
        SortListElements<TaskTableElement>(taskList,
            (e1, e2) => String.Compare(e1.task.name, e2.task.name, StringComparison.CurrentCultureIgnoreCase));
    }
}