using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using translator;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// Controller for the task collection menu.
/// </summary>
public class TaskCollectionMenuController : BaseMenuController
{
    public Transform taskCollectionList;
    public TaskCollectionTableElement taskCollectionTableElementPrefab;
    public Button addButton;
    public TMP_InputField nameInputField;
    public TMP_Dropdown taskClassDropdown;
    public TMP_InputField descriptionInputField;
    public Transform availableParent;
    public Transform includedParent;
    public TaskAssignableElement taskAssignableElementPrefab;

    private string _missingValuesString;
    private TaskCollectionTableElement _currentContainer;
    private TaskCollection _currentTaskCollection;
    private List<Selectable> _selectables;
    private TaskClass _currentTaskClass;

    void Awake()
    {
        _selectables = new List<Selectable>
        {
            nameInputField, taskClassDropdown, descriptionInputField, saveButton
        };
        addButton.onClick.AddListener(() => SetUpByTaskCollection(null, false));
        saveButton.onClick.AddListener(SaveSettings);
        taskClassDropdown.onValueChanged.AddListener(value =>
        {
            _currentTaskClass = Enum.GetValues(typeof(TaskClass)).Cast<TaskClass>().ToList()[value];
            availableParent.DestroyImmediateAllChildren();
            InstantiateAvailableTasks(true);
            CheckAndMarkWrongElementsByTaskClass();
        });
        AddSetUnsavedChangesListener(_selectables);
    }

    private void OnEnable()
    {
        _currentTaskClass = TaskClass.NewPartPainting;
        taskCollectionList.DestroyImmediateAllChildren();
        MainScreenController.Instance.LoadData(DataController.RequestType.TaskCollections, InstantiateAllContainers);
    }

    /// <summary>
    /// Handles a drop of a task element in the list containing all tasks belonging to a task collection.
    /// </summary>
    public void HandleDrop(int index)
    {
        TaskAssignableElement container = BaseDragHandler.draggedObject.GetComponent<TaskAssignableElement>();
        TaskAssignableElement newElement = AddIncludedTaskElement(container.taskCollectionElement, true);
        newElement.transform.SetSiblingIndex(index);
        SetUnsavedChanges(true);
        UpdateIndices();
    }

    /// <summary>
    /// Updates the index of all BasicAssignableElements in list of included tasks.
    /// </summary>
    private void UpdateIndices()
    {
        int add = 1;
        foreach (Transform child in includedParent)
        {
            BasicAssignableElement container = child.gameObject.GetComponent<BasicAssignableElement>();
            if (container != null)
                container.SetIndex(child.transform.GetSiblingIndex() + add);
            // we need te reduce the add value after the preview element because it is deactivated in the next frame
            else
                add--;
        }
    }

    /// <summary>
    /// Creates the list entries representing the available task collections.
    /// </summary>
    private void InstantiateAllContainers()
    {
        taskCollectionList.DestroyImmediateAllChildren();
        foreach (var taskCollection in DataController.Instance.taskCollections.Values)
        {
            TaskCollectionTableElement container =
                Instantiate(taskCollectionTableElementPrefab, taskCollectionList);
            container.Init(taskCollection, SetUpByTaskCollection, DeleteTaskCollection);
        }

        SortTaskCollections();
        ResetFields();
    }

    private void ResetFields()
    {
        _selectables.ForEach(selectable => selectable.interactable = false);
        nameInputField.text = String.Empty;
        descriptionInputField.text = String.Empty;
        includedParent.DestroyImmediateAllChildren("Preview");
        availableParent.DestroyImmediateAllChildren();
        SetUnsavedChanges(false);
    }

    private void DeleteTaskCollection(TaskCollectionTableElement container, bool confirmed)
    {
        if (!confirmed)
        {
            PopupScreenHandler.Instance.ShowConfirmation("popup-remove-task-collection",
                "popup-remove-task-collection-confirm", () => DeleteTaskCollection(container, true));
            return;
        }

        PopupScreenHandler.Instance.ShowLoadingScreen("popup-remove-task-collection", "popup-removing-task-collection");

        RestConnector.Delete(container.taskCollection, "/taskCollections/" + container.taskCollection.id, () =>
            {
                if (_currentContainer != null && container.taskCollection.id == _currentContainer.taskCollection.id)
                    ResetFields();
                Destroy(container.gameObject);
                PopupScreenHandler.Instance.ShowMessage("popup-remove-task-collection",
                    "popup-removed-task-collection");
            },
            _ => PopupScreenHandler.Instance.ShowMessage("popup-remove-task-collection",
                "popup-remove-task-collection-error"),
            PopupScreenHandler.Instance.ShowConnectionError);
    }

    /// <summary>
    /// Initializes all fields on the right side by the given task collection.
    /// </summary>
    private void SetUpByTaskCollection(TaskCollectionTableElement container, bool confirmed)
    {
        if (HasUnsavedChanges() && !confirmed)
        {
            PopupScreenHandler.Instance.ShowUnsavedChanges(() => SetUpByTaskCollection(container, true));
            return;
        }

        _currentContainer = container;
        _currentTaskClass = Enum.GetValues(typeof(TaskClass)).Cast<TaskClass>().ToList()[taskClassDropdown.value];
        if (container != null)
        {
            PopupScreenHandler.Instance.ShowLoadingData();
            RestConnector.GetObject<List<TaskCollectionAssignment>>(
                "/taskCollections/" + _currentContainer.taskCollection.id + "/taskCollectionAssignments",
                taskCollectionAssignments =>
                {
                    _currentContainer.taskCollection.taskCollectionAssignments = taskCollectionAssignments;
                    PopupScreenHandler.Instance.Close();
                    ShowTaskCollectionProperties();
                }, () =>
                {
                    ResetFields();
                    PopupScreenHandler.Instance.ShowConnectionError();
                });
        }
        else
        {
            ShowTaskCollectionProperties();
        }
    }
    
    private void ShowTaskCollectionProperties()
    {
        _currentTaskCollection = _currentContainer != null ? _currentContainer.taskCollection : new TaskCollection();
        nameInputField.text = _currentTaskCollection.name;
        descriptionInputField.text = _currentTaskCollection.description;
        SetTaskClassDropdown(taskClassDropdown, _currentTaskCollection.taskClass);
        bool modifiable = _currentTaskCollection.taskCollectionAssignments.Count == 0;
        availableParent.DestroyImmediateAllChildren();
        includedParent.DestroyImmediateAllChildren("Preview");
        InstantiateAvailableTasks(modifiable);
        InstantiateIncludedTasks(modifiable);
        _selectables.ForEach(selectable => selectable.interactable = modifiable);
        SetUnsavedChanges(false);
        if (!modifiable)
            PopupScreenHandler.Instance.ShowMessage("popup-attention", "popup-task-collection-not-modifiable");
    }

    private bool ValuesMissing()
    {
        _missingValuesString = "";
        if (nameInputField.text == "")
            _missingValuesString +=
                TranslationController.Instance.Translate("popup-task-collection-missing-name") + "\n";
        if (includedParent.childCount <= 1)
            _missingValuesString += TranslationController.Instance.Translate("popup-task-collection-no-tasks") + "\n";
        if (CheckAndMarkWrongElementsByTaskClass())
            _missingValuesString +=
                TranslationController.Instance.Translate("popup-task-collection-wrong-tasks") + "\n";
        return (nameInputField.text == "" || includedParent.childCount <= 1 || CheckAndMarkWrongElementsByTaskClass());
    }

    private void InstantiateAvailableTasks(bool modifiable)
    {
        if (_currentTaskCollection == null)
            return;

        availableParent.DestroyImmediateAllChildren();
        foreach (var task in DataController.Instance.tasks.Values)
            AddAvailableTaskElement(task, modifiable);

        SortAvailableTasks();
    }

    private void InstantiateIncludedTasks(bool modifiable)
    {
        includedParent.DestroyImmediateAllChildren("Preview");
        if (_currentTaskCollection.taskCollectionElements != null)
            _currentTaskCollection.taskCollectionElements.ForEach(element =>
                AddIncludedTaskElement(element, modifiable));
        UpdateIndices();
    }

    private void DeleteTaskCollectionElement(TaskAssignableElement container)
    {
        DestroyImmediate(container.gameObject);
        if (container.taskCollectionElement.task.taskClass == _currentTaskClass)
            AddAvailableTaskElement(container.taskCollectionElement.task, true);
        UpdateIndices();
        SortAvailableTasks();
        SetUnsavedChanges(true);
    }

    private void AddAvailableTaskElement(Task task, bool modifiable)
    {
        // only add tasks with the correct task class
        if (_currentTaskClass != task.taskClass)
            return;

        // don't add task as available if it is already included in the task collection
        if (_currentTaskCollection.taskCollectionElements.Exists(element => element.task.id == task.id))
            return;

        // don't add task as available if it is already included in the task collection (but not saved yet)
        foreach (Transform included in includedParent)
        {
            TaskAssignableElement element = included.GetComponent<TaskAssignableElement>();
            if (element != null && element.taskCollectionElement.task.id == task.id)
                return;
        }

        Instantiate(taskAssignableElementPrefab, availableParent)
            .Init(new TaskCollectionElement { id = -1, task = task }, modifiable, null, null);
    }

    private TaskAssignableElement AddIncludedTaskElement(TaskCollectionElement taskCollectionElement, bool modifiable)
    {
        TaskAssignableElement container = Instantiate(taskAssignableElementPrefab, includedParent);
        container.Init(taskCollectionElement, modifiable, DeleteTaskCollectionElement,
            () => SetUnsavedChanges(true));
        return container;
    }

    /// <summary>
    /// Marks the tasks assigned to the task collection which have a different task class than the task collection.
    /// </summary>
    /// <returns></returns>
    private bool CheckAndMarkWrongElementsByTaskClass()
    {
        bool wrong = false;
        foreach (Transform child in includedParent)
        {
            TaskAssignableElement container = child.GetComponent<TaskAssignableElement>();
            if (!container) // preview game object doesn't
                continue;

            TaskClass taskClass = Enum.GetValues(typeof(TaskClass)).Cast<TaskClass>().ToList()[taskClassDropdown.value];
            if (container.taskCollectionElement.task.taskClass != taskClass)
            {
                child.GetComponent<Image>().color = warningColor;
                wrong = true;
            }
            else
                child.GetComponent<Image>().color = normalColor;
        }

        return wrong;
    }

    /// <summary>
    /// Saves the task collection on the server.
    /// </summary>
    private void SaveSettings()
    {
        if (ValuesMissing())
        {
            PopupScreenHandler.Instance.ShowMissingValues("popup-task-collection-missing-values", _missingValuesString);
            return;
        }

        PopupScreenHandler.Instance.ShowLoadingScreen("popup-task-collection-save", "popup-task-collection-saving");

        TaskCollection taskCollection = new TaskCollection { id = -1 };
        // create copy of task collection if editing to avoid changing the properties of the original task collection
        // (changes would persist on client side even if the task collection cannot be saved on the server)
        if (_currentContainer != null)
            taskCollection = new TaskCollection(_currentContainer.taskCollection);
        taskCollection.name = nameInputField.text;
        taskCollection.description = descriptionInputField.text;
        taskCollection.taskClass =
            Enum.GetValues(typeof(TaskClass)).Cast<TaskClass>().ToList()[taskClassDropdown.value];
        taskCollection.taskCollectionElements = new List<TaskCollectionElement>();
        for (int i = 0; i < includedParent.childCount; i++)
        {
            TaskAssignableElement container = includedParent.GetChild(i).GetComponent<TaskAssignableElement>();
            if (!container) //preview doesn't
                continue;
            container.taskCollectionElement.index = i + 1;
            container.taskCollectionElement.mandatory = container.mandatoryToggle.isOn;
            taskCollection.taskCollectionElements.Add(container.taskCollectionElement);
        }

        RestConnector.Update(taskCollection,
            taskCollection.id < 0 ? UnityWebRequest.kHttpVerbPOST : UnityWebRequest.kHttpVerbPUT, "/taskCollections",
            updatedTaskCollection =>
            {
                if (_currentContainer == null)
                    _currentContainer = Instantiate(taskCollectionTableElementPrefab, taskCollectionList);
                _currentContainer.Init(updatedTaskCollection, SetUpByTaskCollection, DeleteTaskCollection);
                SortTaskCollections();
                _currentContainer.containerButton.Select();
                SetUnsavedChanges(false);
                PopupScreenHandler.Instance.ShowMessage("popup-task-collection-save", "popup-task-collection-saved");
            }, PopupScreenHandler.Instance.ShowConnectionError, _ => PopupScreenHandler.Instance.ShowMessage(
                "popup-task-collection-save", "popup-task-collection-saving-error"));
    }

    private void SortAvailableTasks()
    {
        SortListElements<TaskAssignableElement>(availableParent,
            (e1, e2) => String.Compare(e1.taskCollectionElement.task.name, e2.taskCollectionElement.task.name,
                StringComparison.CurrentCultureIgnoreCase));
    }

    private void SortTaskCollections()
    {
        SortListElements<TaskCollectionTableElement>(taskCollectionList,
            (e1, e2) => String.Compare(e1.taskCollection.name, e2.taskCollection.name,
                StringComparison.CurrentCultureIgnoreCase));
    }
}