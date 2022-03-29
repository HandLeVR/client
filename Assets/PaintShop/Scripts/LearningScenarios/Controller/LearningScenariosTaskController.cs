using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

/// <summary>
/// Controls the sequence of sub tasks in the learning scenarios scene.
/// </summary>
public class LearningScenariosTaskController : Singleton<LearningScenariosTaskController>
{
    public TaskSelectionPanel taskSelectionPanel;
    public TaskPanel taskPanel;
    public GameObject repeatTutorialPanel;
    public VideoPlayerPanel videoPlayerPanel;

    [HideInInspector] public bool taskFinished;
    [HideInInspector] public TaskAssignment currentTaskAssignment;
    [HideInInspector] public Task currentTask;
    [HideInInspector] public int maxSubTaskIndex;
    [HideInInspector] public VRSubTaskController currentSubTaskController;
    [HideInInspector] public Coat selectedCoat;
    [HideInInspector] public List<WorkpieceData> workpieceData;

    private VRSubTaskController[] _subTaskControllers;
    private int _currentSubTaskIndex;
    private bool _taskContainsEvaluation;
    private Workpiece _firstWorkpiece;
    private SupportInfo _currentSupportInfo;

    private void Start()
    {
        // prepare temporary saving of workpieces and heightmaps
        workpieceData = new List<WorkpieceData>();

        // prepare scene
        ApplicationController.Instance.ExecuteAfterSprayGunSpawn(() =>
            ApplicationController.Instance.sprayGun.isDisabled = true);
        GameObject.Find("Monitor").SetActive(false);
        GameObject.Find("Spray Test Paper").SetActive(false);
        ApplicationController.Instance.currentWorkpieceGameObject.SetActive(false);
        PaintController.Instance.drawables[0] =
            GameObject.Find("Learning Scenario Spray Test Paper").GetComponent<CustomDrawable>();

        _subTaskControllers = GetComponentsInChildren<VRSubTaskController>(true);

        VirtualInstructorController.Instance.gameObject.SetActive(false);

        // the tutorial starts if we load the learning tasks scene without a logged in user
        if (DataController.Instance.CurrentUser == null)
            taskSelectionPanel.StartTutorial();
    }

    /// <summary>
    /// Finishes the current sub task and stats the next sub task or shows the task panel if it was the last sub task.
    /// </summary>
    public void FinishCurrentSubTask()
    {
        if (!IsLastSubTask())
        {
            _currentSubTaskIndex++;
            maxSubTaskIndex = Math.Max(_currentSubTaskIndex, maxSubTaskIndex);
            StartNextSubTask();
        }
        else
        {
            ActivateController("None");
            taskFinished = true;
            ShowTaskPanel(false);
        }
    }

    public bool IsLastSubTask()
    {
        return _currentSubTaskIndex == currentTask.subTasks.Count - 1;
    }

    /// <summary>
    /// Check whether the current sub task is the first sub task of the task without considering reset
    /// workpiece tasks (because there in this sub tasks no user action is needed).
    /// </summary>
    private bool IsFirstTask()
    {
        for (int i = 0; i < _currentSubTaskIndex; i++)
            if (currentTask.subTasks[i].type != "Reset Workpiece")
                return false;
        return true;
    }

    /// <summary>
    /// Returns to the last sub tasks or shows the task panel if it was the first task.
    /// </summary>
    public void ReturnToLastSubTask()
    {
        if (!IsFirstTask())
        {
            _currentSubTaskIndex--;
            SubTask currentSubTask = currentTask.subTasks[_currentSubTaskIndex];
            while (_subTaskControllers.FirstOrDefault(controller =>
                       controller.gameObject.name == currentSubTask.type) == null)
            {
                _currentSubTaskIndex--;
                currentSubTask = currentTask.subTasks[_currentSubTaskIndex];
            }

            // go one sub task further back if it is a reset workpiece sub task and still not the first sub task
            // but reset the workpiece to the state of the workpiece before the reset workpiece sub task
            if (!IsFirstTask() && currentSubTask.type == "Reset Workpiece")
            {
                _currentSubTaskIndex--;
                WorkpieceData data = workpieceData[^1];
                ApplicationController.Instance.SpawnWorkpiece(data.workpiece);
                PaintController.Instance.LoadCoat(data.coat, false);
                PaintController.Instance.LoadBaseCoat(data.baseCoat);
                EvaluationController.Instance.LoadEvaluationData(data.evaluationData);
                ApplicationController.Instance.currentWorkpieceGameObject.GetComponentInChildren<CustomDrawable>()
                    .LoadHeightmaps(data.texture);
                workpieceData.RemoveAt(workpieceData.Count - 1);
            }

            StartNextSubTask();
        }
        else
        {
            ActivateController("None");
            ShowTaskPanel(true);
        }
    }

    public bool SubTaskExecuted()
    {
        return maxSubTaskIndex > _currentSubTaskIndex;
    }

    public void StartTask()
    {
        _currentSubTaskIndex = 0;
        maxSubTaskIndex = 0;
        selectedCoat = null;
        workpieceData = new List<WorkpieceData>();
        StartNextSubTask();
    }

    /// <summary>
    /// Determines the controller for the next sub task an activates it.
    /// </summary>
    private void StartNextSubTask()
    {
        SubTask currentSubTask = currentTask.subTasks[_currentSubTaskIndex];
        VRSubTaskController nextSubTaskController = _subTaskControllers.First(controller =>
            controller.gameObject.name == currentSubTask.type);
        nextSubTaskController.subTask = currentSubTask;

        ActivateController(currentSubTask.type);
    }

    /// <summary>
    /// Loads all dependencies of a task and starts the task afterwards.
    /// </summary>
    public void LoadTask(TaskAssignment taskAssignment)
    {
        TaskPreparationController.Instance.PrepareMedia(taskAssignment.task,
            () => StartTask(taskAssignment));
    }

    public void LoadTask(Task task)
    {
        TaskPreparationController.Instance.PrepareMedia(task, () => StartTask(task));
    }

    private void StartTask(TaskAssignment taskAssignment)
    {
        StartTask(taskAssignment.task, taskAssignment);
    }

    private void StartTask(Task task, TaskAssignment taskAssignment = null)
    {
        currentTaskAssignment = taskAssignment;
        currentTask = task;
        taskFinished = false;
        _taskContainsEvaluation = currentTask.subTasks.Any(subTask => subTask.type == "Evaluation");
        SetFirstWorkpiece();
        ShowTaskPanel(false);
    }

    public void ShowTaskSelectionPanel()
    {
        LearningScenariosMonitorController.Instance.ChangePanel(taskSelectionPanel.gameObject);
        taskSelectionPanel.ShowTaskListPanel();
    }

    private void ReturnToMainScreen()
    {
        LearningScenariosMonitorController.Instance.ChangePanel(taskSelectionPanel.gameObject);
        taskSelectionPanel.ReturnToMainScreen();
    }

    private void ShowRepeatTutorialScreen()
    {
        LearningScenariosMonitorController.Instance.ChangePanel(repeatTutorialPanel.gameObject);
    }

    /// <summary>
    /// Activates the panel belonging to the current sub task.
    /// </summary>
    public void ShowTaskPanel(bool returned)
    {
        if (taskFinished && !returned && currentTaskAssignment != null && currentTaskAssignment.id != -1)
        {
            // creat task result with recording
            if (RecordingsToUpload())
                StartCoroutine(UploadRecording());
            // create task result without recording if task has no task result
            if (!currentTaskAssignment.task.HasPaintingTask())
                CreateTaskResult();
        }

        // don't show the the task panel if we are in a support info or in the tutorial
        if (currentTask.IsSupportInfo() || currentTaskAssignment == null)
        {
            if (returned || taskFinished)
            {
                // return to main screen if we are in the tutorial
                if (currentTaskAssignment == null)
                {
                    // tutorial was started from the login menu without login
                    if (DataController.Instance.CurrentUser == null)
                        ShowRepeatTutorialScreen();
                    // tutorial was started from the learning tasks scene
                    else
                        ReturnToMainScreen();
                }
                else
                    ShowTaskSelectionPanel();
            }
            else
                StartTask();
        }
        else
        {
            taskPanel.InitPanel(currentTask.name, taskFinished, _taskContainsEvaluation, ShowEvaluation);
            LearningScenariosMonitorController.Instance.ChangePanel(taskPanel.gameObject);
            if (!taskFinished && _firstWorkpiece != null)
                ApplicationController.Instance.SpawnWorkpiece(_firstWorkpiece);
        }
    }

    /// <summary>
    /// Activates the evaluation sub task.
    /// </summary>
    private void ShowEvaluation()
    {
        PaintEvaluationController evaluationController =
            (PaintEvaluationController)_subTaskControllers.First(controller =>
                controller.gameObject.name == "Evaluation");
        evaluationController.isSingle = true;
        ActivateController(evaluationController.name);
    }

    /// <summary>
    /// Uploads the results of the current task but waits until the last result is saved.
    /// </summary>
    private IEnumerator UploadRecording()
    {
        PopupScreenController.Instance.ShowLoadingScreen("uploading-recording");
        while (CreateRecordingController.Instance.isSavingFile)
            yield return null;
        DataController.Instance.UploadTaskResults(currentTaskAssignment,
            () => PopupScreenController.Instance.ShowConfirmationScreen("recording-upload-successful"),
            OnErrorUpload);
    }

    /// <summary>
    /// Creates a task result without a recording.
    /// </summary>
    private void CreateTaskResult()
    {
        RestConnector.Update<TaskResult>(null, UnityWebRequest.kHttpVerbPUT,
            "/taskAssignments/" + currentTaskAssignment.id + "/finishTask",
            _ => PopupScreenController.Instance.ShowConfirmationScreen("recording-upload-successful"), OnErrorUpload);
    }

    /// <summary>
    /// Checks whether there are recordings to upload.
    /// </summary>
    private bool RecordingsToUpload()
    {
        return CreateRecordingController.Instance.isSavingFile ||
               DataController.Instance.GetRecordingsToUpload().Length > 0;
    }

    private void OnErrorUpload()
    {
        PopupScreenController.Instance.ShowConfirmationScreen("recording-upload-failed");
    }

    private void ActivateController(string controllerName)
    {
        foreach (VRSubTaskController s in _subTaskControllers)
        {
            // set inactive first to call OnEnable Method after activation
            s.gameObject.SetActive(false);
            if (s.gameObject.name.Equals(controllerName))
            {
                currentSubTaskController = s;
                s.gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Searches for the first "Paint Workpiece" sub task to spawn the used workpiece at the start of the task. This
    /// is done to avoid an empty room.
    /// </summary>
    private void SetFirstWorkpiece()
    {
        SubTask subTask = currentTask.subTasks.FirstOrDefault(st => st.type == "Paint Workpiece");
        if (subTask == null)
            _firstWorkpiece = null;
        else
        {
            JObject jsonObject = JObject.Parse(subTask.properties);
            int workpieceId = (int)jsonObject.GetValue("workpieceId");
            _firstWorkpiece = DataController.Instance.workpieces[workpieceId];
        }
    }
}