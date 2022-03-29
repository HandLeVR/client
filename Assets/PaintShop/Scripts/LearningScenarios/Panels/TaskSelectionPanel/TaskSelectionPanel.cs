using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using translator;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Displays all tasks and task collections assigned to the user.
/// </summary>
public class TaskSelectionPanel : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject taskListPanel;
    public TaskCollectionContainer taskCollectionElementPrefab;
    public TaskContainer taskElementPrefab;

    private TextMeshProUGUI _taskListPanelHeader;
    private Transform _taskListContent;
    private TaskClass _currentTaskClass;

    // needed to reopen task collection which where opened when leaving the panel
    private List<TaskCollectionContainer> _taskCollectionContainers;
    private List<long> _openTaskCollections;

    private Dictionary<TaskCollectionAssignment, List<TaskAssignment>> _taskCollectionAssignments;
    private List<TaskAssignment> _individualAssignments;

    private void Start()
    {
        _taskListPanelHeader = taskListPanel.transform.Find("Header").GetComponent<TextMeshProUGUI>();
        _taskListContent = taskListPanel.transform.FindDeepChild("Content");
        _taskCollectionContainers = new List<TaskCollectionContainer>();
        _openTaskCollections = new List<long>();
        ReturnToMainScreen();
    }

    private void OnDisable()
    {
        // open task collections which where open when leaving the panel
        _openTaskCollections = new List<long>();
        foreach (TaskCollectionContainer container in _taskCollectionContainers)
            if (container.IsOpen())
                _openTaskCollections.Add(container.taskCollection.id);
    }

    /// <summary>
    /// Opens the task list panel for the current task class.
    /// </summary>
    public void ShowTaskListPanel()
    {
        ShowTaskListPanel(_currentTaskClass);
    }

    /// <summary>
    /// Opens the task list panel for the given task class.
    /// This is needed to be able to call the method from the inspector.
    /// </summary>
    public void ShowTaskListPanel(string taskClassString)
    {
        ShowTaskListPanel((TaskClass)Enum.Parse(typeof(TaskClass), taskClassString));
    }

    /// <summary>
    /// Displays all task collections and tasks for the given task class which are assigned to the current user or to
    /// one of his user groups.
    /// </summary>
    private void ShowTaskListPanel(TaskClass taskClass)
    {
        if (_currentTaskClass != taskClass)
            _openTaskCollections = new List<long>();

        _currentTaskClass = taskClass;
        mainMenuPanel.SetActive(false);
        taskListPanel.SetActive(true);
        _taskListContent.transform.DestroyAllChildren();
        _taskListPanelHeader.text = TranslationController.Instance.Translate(taskClass.ToString());

        _taskCollectionAssignments = new Dictionary<TaskCollectionAssignment, List<TaskAssignment>>();
        _individualAssignments = new List<TaskAssignment>();

        PopupScreenController.Instance.ShowLoadingScreen("loading-tasks");

        // always try to upload task results which are not already uploaded 
        // that means if an upload did not work beforehand we will try again when the task list panel is opened
        DataController.Instance.UploadTaskResults(null,
            () =>
            {
                RestConnector.GetObject<List<Task>>("/users/" + DataController.Instance.CurrentUser.id + "/tasks",
                    tasks => InstantiateOwnTasks(tasks, InstantiateTaskCollection),
                    () => PopupScreenController.Instance.ShowConfirmationScreen("connection-error"));
            },
            () => PopupScreenController.Instance.ShowConfirmationScreen("connection-error"));
    }

    public void ReturnToMainScreen()
    {
        mainMenuPanel.SetActive(true);
        taskListPanel.SetActive(false);
    }

    public void StartTutorial()
    {
        PopupScreenController.Instance.ShowLoadingScreen("tutorial-loading", true);
        DataController.Instance.UpdateData(DataController.RequestType.Tasks, () =>
        {
            Task tutorial = DataController.Instance.tasks.Values.FirstOrDefault(task => task.name == "Tutorial");
            if (tutorial != null)
                LearningScenariosTaskController.Instance.LoadTask(tutorial);
            else
                PopupScreenController.Instance.ShowConfirmationScreen("tutorial-error");
        }, () => { PopupScreenController.Instance.ShowConfirmationScreen("connection-error"); });
    }


    /// <summary>
    /// Gets and displays all tasks and support infos assigned to a task collection.
    /// </summary>
    private void InstantiateTaskCollection()
    {
        RestConnector.GetUserTaskAssignments(DataController.Instance.CurrentUser, false, () =>
        {
            foreach (TaskAssignment assignment in DataController.Instance.CurrentUser.taskAssignments.Where(
                         assignment => assignment.task.taskClass == _currentTaskClass))
            {
                if (assignment.taskCollectionAssignment != null)
                {
                    if (!_taskCollectionAssignments.ContainsKey(assignment.taskCollectionAssignment))
                        _taskCollectionAssignments[assignment.taskCollectionAssignment] =
                            new List<TaskAssignment>();
                    _taskCollectionAssignments[assignment.taskCollectionAssignment].Add(assignment);
                }
                else
                {
                    _individualAssignments.Add(assignment);
                }
            }

            foreach (TaskCollectionAssignment taskCollectionAssignment in _taskCollectionAssignments.Keys)
                InstantiateTaskCollection(taskCollectionAssignment);
            foreach (TaskAssignment taskAssignment in _individualAssignments.Where(assignment =>
                         !assignment.task.valuesMissing))
                InstantiateTask(taskAssignment);
            PopupScreenController.Instance.ClosePopupScreen();
        }, () => PopupScreenController.Instance.ShowConfirmationScreen("connection-error"));
    }
    
    /// <summary>
    /// Creates all tasks on the screen for a task collection.
    /// </summary>
    private void InstantiateTaskCollection(TaskCollectionAssignment taskCollectionAssignment)
    {
        TaskCollectionContainer taskCollectionContainer =
            Instantiate(taskCollectionElementPrefab, _taskListContent.transform);
        List<TaskContainer> childs = new List<TaskContainer>();
        TaskCollectionElement prev = null;
        bool prevEnabled = true;
        taskCollectionAssignment.taskCollection.SortTaskCollectionElements();
        foreach (TaskCollectionElement taskCollectionElement in taskCollectionAssignment.taskCollection
                     .taskCollectionElements.Where(element => !element.task.valuesMissing))
        {
            // it is possible that some assignments of an task collection assignment are removed
            // so we only need the remaining ones
            TaskAssignment taskAssignment = _taskCollectionAssignments[taskCollectionAssignment]
                .Find(assignment => assignment.task.id == taskCollectionElement.task.id);
            if (taskAssignment == null)
                continue;
            bool executable = prev == null || !prev.mandatory || prevEnabled;
            TaskContainer child = Instantiate(taskElementPrefab, _taskListContent.transform);
            child.Instantiate(taskAssignment, executable);
            prevEnabled = child.finishedToggle.isOn;
            childs.Add(child);
            prev = taskCollectionElement;
        }

        taskCollectionContainer.Instantiate(taskCollectionAssignment.taskCollection, childs);
        taskCollectionContainer.ShowSubElements(
            _openTaskCollections.Contains(taskCollectionAssignment.taskCollection.id));
        _taskCollectionContainers.Add(taskCollectionContainer);
    }

    /// <summary>
    /// Creates all tasks on the screen in an own category which are created be the current user.
    /// </summary>
    private void InstantiateOwnTasks(List<Task> tasks, UnityAction onFinish)
    {
        List<Task> taskClassTasks = tasks.FindAll(task => task.taskClass == _currentTaskClass).ToList();
        if (taskClassTasks.Count > 0)
        {
            TaskCollectionContainer taskCollectionContainer =
                Instantiate(taskCollectionElementPrefab, _taskListContent.transform);
            List<TaskContainer> childs = new List<TaskContainer>();
            foreach (Task task in taskClassTasks)
            {
                TaskContainer child = Instantiate(taskElementPrefab, _taskListContent.transform);
                child.Instantiate(new TaskAssignment { id = -1, task = task }, true);
                childs.Add(child);
            }

            TaskCollection dummyTaskCollection = new TaskCollection
                { id = -1, name = "Eigene Aufgaben (Resultate werden nicht gespeichert)" };
            taskCollectionContainer.Instantiate(dummyTaskCollection, childs);
            taskCollectionContainer.ShowSubElements(_openTaskCollections.Contains(dummyTaskCollection.id));
            _taskCollectionContainers.Add(taskCollectionContainer);
        }

        onFinish.Invoke();
    }

    private void InstantiateTask(TaskAssignment taskAssignment)
    {
        TaskContainer taskContainer = Instantiate(taskElementPrefab, _taskListContent.transform);
        taskContainer.Instantiate(taskAssignment, true, true);
    }
}