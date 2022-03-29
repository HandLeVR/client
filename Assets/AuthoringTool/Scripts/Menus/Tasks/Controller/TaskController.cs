using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using translator;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

/// <summary>
/// Controls the creation and modification of tasks.
/// </summary>
public class TaskController : Singleton<TaskController>
{
    public TMP_InputField nameTextField;
    public TMP_InputField descriptionTextField;
    public TMP_Dropdown taskClassDropdown;
    public TMP_InputField goalTimeInputField;
    public Toggle exerciseToggle;
    public Transform subTasksPanel;
    public Transform subTaskContainerPanel;
    public Button saveButton;
    public Button copyButton;

    [HideInInspector] public bool unsavedChanges;

    private Dictionary<string, BaseSettingsPanel> _settingsPanels;
    private Task _currentTask;

    private void Awake()
    {
        foreach (TaskClass taskClass in Enum.GetValues(typeof(TaskClass)))
            taskClassDropdown.options.Add(
                new TMP_Dropdown.OptionData(TranslationController.Instance.Translate(taskClass.ToString())));

        _settingsPanels = new Dictionary<string, BaseSettingsPanel>();
        Resources.FindObjectsOfTypeAll<BaseSettingsPanel>().ToList()
            .ForEach(p => _settingsPanels.Add(p.subTaskUnityId, p));

        saveButton.onClick.AddListener(() => OpenSaveMenu(false));
        copyButton.onClick.AddListener(() => OpenSaveMenu(true));
    }

    private void OpenSaveMenu(bool createCopy)
    {
        OpenSaveMenu(ValuesMissing(), SaveTask, createCopy ? nameTextField.text + " (Kopie)" : null);
    }

    /// <summary>
    /// Opens the save menu which allows to override a task or save it as a copy.
    /// </summary>
    private void OpenSaveMenu(bool missingValues, UnityAction<bool, string> onConfirmation, string copyName)
    {
        if (String.IsNullOrEmpty(nameTextField.text))
            PopupScreenHandler.Instance.ShowMessage("popup-save-task", "popup-save-task-missing-name");
        else if (missingValues)
            PopupScreenHandler.Instance.ShowConfirmation("popup-save-task",
                "popup-save-task-missing-values",
                new PopupScreenHandler.ButtonConfig(
                    () => OpenSaveMenu(false, onConfirmation, copyName),
                    copyName != null ? "popup-button-copy" : "popup-button-save"),
                new PopupScreenHandler.ButtonConfig(PopupScreenHandler.Instance.Close, "popup-button-abort"));
        // task already exists
        else if (_currentTask != null)
        {
            if (copyName != null)
                PopupScreenHandler.Instance.ShowCopyTask(copyName, newName => onConfirmation(true, newName));
            else
                PopupScreenHandler.Instance.ShowConfirmation("popup-save-task",
                    "popup-save-task-overwrite-task",
                    new PopupScreenHandler.ButtonConfig(() => onConfirmation(false, null), "popup-button-overwrite"),
                    new PopupScreenHandler.ButtonConfig(PopupScreenHandler.Instance.Close, "popup-button-abort"));
        }
        else
            onConfirmation(false, null);
    }

    /// <summary>
    /// De(activates) the save buttons in dependence of the state of the task.
    /// </summary>
    private void SetSaveButtonsInteractable()
    {
        saveButton.interactable =
            _currentTask == null || _currentTask.permission == null || _currentTask.permission.editable;
        copyButton.interactable = _currentTask != null;
    }

    /// <summary>
    /// Save the task on the server.
    /// </summary>
    private void SaveTask(bool newTask, string taskName)
    {
        Task task = new Task
        {
            name = taskName ?? nameTextField.text,
            description = descriptionTextField.text,
            partTaskPractice = exerciseToggle.isOn,
            subTasks = new List<SubTask>()
        };

        // create sub task from the sub task elements placed in the list
        foreach (Transform child in subTasksPanel)
        {
            BaseSubTaskContainer subTaskContainer = child.GetComponent<BaseSubTaskContainer>();
            if (subTaskContainer == null)
                continue;

            SubTask subTask = subTaskContainer.SubTaskData.Copy();
            task.subTasks.Add(subTask);
            subTaskContainer.SubTaskData = subTask;
        }

        if (taskClassDropdown.value != 0)
            task.taskClass = Enum.GetValues(typeof(TaskClass)).Cast<TaskClass>().ToList()[taskClassDropdown.value - 1];

        // -1 as an id tells the server to generate a new id
        task.id = -1;
        if (newTask)
            task.id = -1;
        else if (_currentTask != null)
            task.id = _currentTask.id;

        task.valuesMissing = ValuesMissing();

        DetermineUsedElements(task);

        PopupScreenHandler.Instance.ShowLoadingScreen("popup-save-task", "popup-saving-task");

        RestConnector.Update(task, task.id < 0 ? UnityWebRequest.kHttpVerbPOST : UnityWebRequest.kHttpVerbPUT,
            "/tasks",
            updatedTask =>
            {
                unsavedChanges = false;
                _currentTask = updatedTask;
                PopupScreenHandler.Instance.ShowMessage("popup-save-task", "popup-save-task-finished");
                SetSaveButtonsInteractable();
                nameTextField.text = _currentTask.name;
            }, () => PopupScreenHandler.Instance.ShowMessage("popup-load-data", "popup-initial-connection-error"),
            conflict =>
            {
                PopupScreenHandler.Instance.ShowMessage("popup-saving-error",
                    conflict == "TASKRESULT" ? "popup-task-existing-results" : "popup-task-existing-name");
            },
            elementName =>
            {
                PopupScreenHandler.Instance.ShowMessage("popup-saving-error",
                    String.Format(TranslationController.Instance.Translate("popup-task-element-not-found"),
                        elementName));
            }
        );
    }

    /// <summary>
    /// Recursively goes through the json of the task to determine the used media elements, coats, recording and workpieces.
    /// </summary>
    private void DetermineUsedElements(Task task)
    {
        task.usedMedia = new HashSet<Media>();
        task.usedCoats = new HashSet<Coat>();
        task.usedRecordings = new HashSet<Recording>();
        task.usedWorkpieces = new HashSet<Workpiece>();
        foreach (SubTask subTask in task.subTasks)
        {
            JObject jObject = JObject.Parse(subTask.properties);
            CheckDependenciesRec(jObject, task);
        }
    }

    private void CheckDependenciesRec(JObject jObject, Task task)
    {
        foreach (KeyValuePair<string, JToken> pair in jObject)
        {
            if (pair.Key == "audioId" || pair.Key == "finalAudioId" || pair.Key == "imageId" || pair.Key == "videoId" ||
                pair.Key == "reminderAudioId" || pair.Key == "finalReminderAudioId")
            {
                task.usedMedia.Add(DataController.Instance.media[(long)pair.Value]);
            }
            else if (pair.Key == "coatId" || pair.Key == "baseCoatId")
            {
                long coatId = (long)pair.Value;
                if (coatId >= 0)
                    task.usedCoats.Add(DataController.Instance.coats[coatId]);
            }
            else if (pair.Key == "recordingId")
            {
                task.usedRecordings.Add(DataController.Instance.remoteRecordings[(long)pair.Value]);
            }
            else if (pair.Key == "workpieceId")
            {
                task.usedWorkpieces.Add(DataController.Instance.workpieces[(long)pair.Value]);
            }
            else if (pair.Value is JObject newJObject)
            {
                CheckDependenciesRec(newJObject, task);
            }
            else if (pair.Value is JArray newJArray)
            {
                foreach (var jToken in newJArray)
                {
                    try
                    {
                        CheckDependenciesRec(JObject.Parse(jToken.ToString()), task);
                    }
                    catch
                    {
                        // ignored because we reached a primitive field
                    }
                }
            }
            else
            {
                try
                {
                    CheckDependenciesRec(JObject.Parse(pair.Value.ToString()), task);
                }
                catch
                {
                    // ignored because we reached a primitive field
                }
            }
        }
    }

    /// <summary>
    /// Sets all input fields an creates the sub task list in dependence of the given task.
    /// </summary>
    public void LoadTask(Task task)
    {
        nameTextField.text = task.name;
        descriptionTextField.text = task.description;
        taskClassDropdown.value = taskClassDropdown.options.FindIndex(i =>
            i.text.Equals(TranslationController.Instance.Translate(task.taskClass.ToString())));
        exerciseToggle.isOn = task.partTaskPractice;

        subTaskContainerPanel.DestroyImmediateAllChildren("Preview");
        HideAllPanel();
        if (task.subTasks != null)
        {
            foreach (SubTask subTask in task.subTasks)
                SubTaskController.Instance.CreateSubTaskComponent(subTask, subTaskContainerPanel);
            SubTaskController.Instance.UpdateSubTaskNumbers();
        }

        _currentTask = task;
        unsavedChanges = false;
        DetermineUsedElements(task);

        if (!task.permission.editable)
            PopupScreenHandler.Instance.ShowMessage("popup-attention", "popup-task-not-modifiable");
        SetSaveButtonsInteractable();
    }

    /// <summary>
    /// Resets all fields and clears the list of sub tasks.
    /// </summary>
    public void NewTask(bool confirmed)
    {
        if (!confirmed)
            PopupScreenHandler.Instance.ShowConfirmation("popup-save-task",
                "popup-save-task-unsaved-changes", () =>
                {
                    PopupScreenHandler.Instance.Close();
                    NewTask(true);
                });
        else
        {
            _currentTask = null;
            subTaskContainerPanel.DestroyImmediateAllChildren("Preview");
            HideAllPanel();
            nameTextField.text = "";
            descriptionTextField.text = "";
            taskClassDropdown.value = 0;
            goalTimeInputField.text = "";
            exerciseToggle.isOn = false;
        }

        unsavedChanges = false;
        SetSaveButtonsInteractable();
    }

    public void HideAllPanel()
    {
        _settingsPanels.Values.ToList().ForEach(p => p.gameObject.SetActive(false));
    }

    /// <summary>
    /// Activates the settings panel for a selected sub task.
    /// </summary>
    public void ShowCorrespondingSettingPanel(string subTaskUnityId, BaseSubTaskContainer relatedSubTaskContainer)
    {
        _settingsPanels.Values.ToList().ForEach(p => p.gameObject.SetActive(p.subTaskUnityId == subTaskUnityId));
        _settingsPanels[subTaskUnityId].relatedSubTaskContainer = relatedSubTaskContainer;
        _settingsPanels[subTaskUnityId].SetUpFromSubTask();
    }

    /// <summary>
    /// Returns the currently active settings panel.
    /// </summary>
    public BaseSettingsPanel GetActiveSettingsPanel()
    {
        foreach (BaseSettingsPanel panel in _settingsPanels.Values)
            if (panel.gameObject.activeSelf)
                return panel;
        return null;
    }

    /// <summary>
    /// Checks whether settings of the task or in a sub task are missing.
    /// </summary>
    /// <returns></returns>
    private bool ValuesMissing()
    {
        bool settingsMissing = BasicSettingMissing();
        foreach (Transform child in subTasksPanel)
            if (child.GetComponent<BaseSubTaskContainer>())
                settingsMissing |= child.GetComponent<BaseSubTaskContainer>().ValuesMissing();

        return settingsMissing;
    }

    /// <summary>
    /// Determines whether needed settings are missing.
    /// </summary>
    private bool BasicSettingMissing()
    {
        return String.IsNullOrEmpty(nameTextField.text) || taskClassDropdown.value == 0;
    }
}