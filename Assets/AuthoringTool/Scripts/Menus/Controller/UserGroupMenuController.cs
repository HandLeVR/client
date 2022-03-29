using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using translator;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// Controller for the user group menu.
/// </summary>
public class UserGroupMenuController : BaseMenuController
{
    public Transform userGroupList;
    public UserGroupTableElement userGroupTableElementPrefab;
    public Button newButton;

    public TMP_InputField nameInputField;
    public TMP_InputField userCountInputField;

    public Transform includedUsersList;
    public Transform availableUsersList;
    public Transform includedTasksList;

    public UserAssignableElement userAssignableElementPrefab;
    public UserGroupTaskTableElement userGroupTaskTableElementPrefab;
    public Button addTaskCollectionButton;
    public Button addTaskButton;

    private UserGroupTableElement _currentContainer;
    private List<Selectable> _selectables;
    private string _missingValuesString;

    void Awake()
    {
        _selectables = new List<Selectable>
        {
            nameInputField, saveButton, addTaskCollectionButton, addTaskButton
        };
        newButton.onClick.AddListener(() => SetUpByGroup(null, false));
        saveButton.onClick.AddListener(SaveGroup);
        nameInputField.onValueChanged.AddListener(_ => SetUnsavedChanges(true));
        addTaskButton.onClick.AddListener(() =>
            SelectionPopup.Instance.Init(typeof(Task), AssignTasks));
        addTaskCollectionButton.onClick.AddListener(() =>
            SelectionPopup.Instance.Init(typeof(TaskCollection), AssignTaskCollections));
    }

    void OnEnable()
    {
        ResetFields();
        userGroupList.DestroyImmediateAllChildren();
        MainScreenController.Instance.LoadData(DataController.RequestType.UserGroups, InstantiateContainer);
    }

    /// <summary>
    /// Instantiates the list with all available user groups.
    /// </summary>
    private void InstantiateContainer()
    {
        userGroupList.DestroyImmediateAllChildren();
        foreach (UserGroup userGroup in DataController.Instance.userGroups.Values)
            AddGroupManagementContainer(userGroup);
        SortGroups();
    }

    private UserGroupTableElement AddGroupManagementContainer(UserGroup userGroup)
    {
        UserGroupTableElement container = Instantiate(userGroupTableElementPrefab, userGroupList);
        container.Init(userGroup, SetUpByGroup, DeleteGroup);
        return container;
    }

    private void DeleteGroup(UserGroupTableElement container, bool confirmed)
    {
        if (!confirmed)
        {
            PopupScreenHandler.Instance.ShowConfirmation("popup-remove-group", "popup-remove-group-confirm",
                () => DeleteGroup(container, true));
            return;
        }

        PopupScreenHandler.Instance.ShowLoadingScreen("popup-remove-group", "popup-removing-group");


        RestConnector.Delete(container.userGroup, "/userGroups/" + container.userGroup.id, () =>
        {
            if (container != null && _currentContainer != null &&
                _currentContainer.userGroup.id == container.userGroup.id)
                ResetFields();
            Destroy(container.gameObject);
            PopupScreenHandler.Instance.ShowMessage("popup-remove-group", "popup-removed-group");
        }, null, () => PopupScreenHandler.Instance.ShowConnectionError());
    }

    private void ResetFields()
    {
        _selectables.ForEach(selectable => selectable.interactable = false);
        nameInputField.text = String.Empty;
        userCountInputField.text = String.Empty;
        availableUsersList.DestroyAllChildren();
        includedUsersList.DestroyAllChildren("Preview");
        includedTasksList.DestroyAllChildren();
        SetUnsavedChanges(false);
    }

    private void SaveGroup()
    {
        if (ValuesMissing())
        {
            PopupScreenHandler.Instance.ShowMissingValues("popup-save-group", _missingValuesString);
            return;
        }

        PopupScreenHandler.Instance.ShowLoadingScreen("popup-save-group", "popup-saving-group");
        UserGroup group = _currentContainer != null
            ? _currentContainer.userGroup
            : new UserGroup { id = -1 };
        group.name = nameInputField.text;

        RestConnector.Update(group,
            group.id < 0 ? UnityWebRequest.kHttpVerbPOST : UnityWebRequest.kHttpVerbPUT, "/userGroups", newGroup =>
            {
                if (_currentContainer == null)
                    _currentContainer = AddGroupManagementContainer(newGroup);
                else
                    _currentContainer.Init(newGroup, SetUpByGroup, DeleteGroup);
                SetUpByGroup(_currentContainer, true);
                SortGroups();
                _currentContainer.containerButton.Select();
                PopupScreenHandler.Instance.ShowMessage("popup-save-group", "popup-saved-group");
            }, () => PopupScreenHandler.Instance.ShowConnectionError(), _ =>
                PopupScreenHandler.Instance.ShowMessage("popup-save-group", "popup-existing-group"));
    }

    private bool ValuesMissing()
    {
        // the name seems to be the only attribute needed
        _missingValuesString = "";
        if (nameInputField.text == "")
            _missingValuesString += " " + TranslationController.Instance.Translate("group-name-missing") + "\n";
        return nameInputField.text == "";
    }

    /// <summary>
    /// The fields get filled with information given by a group as currentGroup.
    /// </summary>
    private void SetUpByGroup(UserGroupTableElement userGroupTableElement, bool confirmed)
    {
        if (HasUnsavedChanges() && !confirmed)
        {
            PopupScreenHandler.Instance.ShowUnsavedChanges(() => SetUpByGroup(userGroupTableElement, true));
            return;
        }

        _currentContainer = userGroupTableElement;
        if (userGroupTableElement != null)
        {
            PopupScreenHandler.Instance.ShowLoadingData();
            RestConnector.GetObject<List<UserGroupTaskAssignment>>(
                "/userGroups/" + _currentContainer.userGroup.id + "/userGroupTaskAssignments",
                userGroupTaskAssignments =>
                {
                    _currentContainer.userGroup.userGroupTaskAssignments = userGroupTaskAssignments;
                    ShowUserGroupProperties(_currentContainer.userGroup);
                    PopupScreenHandler.Instance.Close();
                }, () =>
                {
                    ResetFields();
                    PopupScreenHandler.Instance.ShowConnectionError();
                });
        }
        else
        {
            ShowUserGroupProperties(new UserGroup());
        }
    }

    private void ShowUserGroupProperties(UserGroup userGroup)
    {
        nameInputField.text = userGroup.name;
        userCountInputField.text = userGroup.users.Count.ToString();
        availableUsersList.DestroyAllChildren();
        includedUsersList.DestroyAllChildren();
        includedTasksList.DestroyAllChildren();
        if (_currentContainer != null)
        {
            ShowUsers(userGroup.users);
            ShowAssignedTasksAndCollections();
        }

        _selectables.ForEach(selectable => selectable.interactable = true);
        addTaskButton.interactable = _currentContainer != null;
        addTaskCollectionButton.interactable = _currentContainer != null;
        SetUnsavedChanges(false);
    }

    /// <summary>
    /// For each Task in the group's task and taskCollection list, a new game object is instantiated.
    /// </summary>
    private void ShowAssignedTasksAndCollections()
    {
        foreach (UserGroupTaskAssignment assignment in _currentContainer.userGroup
            .userGroupTaskAssignments)
            AddTaskElement(assignment);
    }

    /// <summary>
    /// Displays all users of a group.
    /// </summary>
    /// <param name="users"></param>
    private void ShowUsers(List<User> users)
    {
        foreach (User user in DataController.Instance.users.Values)
            AddUserElement(user, users.Find(user1 => user.id == user1.id) != null);

        SortUsers();
    }

    private void AssignTasks()
    {
        AssignTaskOrTaskCollection(
            SelectionPopup.Instance.GetSelectedIds().Select(id => DataController.Instance.tasks[id]).ToList(), null);
    }


    private void AssignTaskCollections()
    {
        AssignTaskOrTaskCollection(null,
            SelectionPopup.Instance.GetSelectedIds().Select(id => DataController.Instance.taskCollections[id])
                .ToList());
    }

    private void AssignTaskOrTaskCollection(List<Task> tasks, List<TaskCollection> taskCollections)
    {
        PopupScreenHandler.Instance.ShowLoadingData();
        DateTime? deadline = DeadlinePicker.Instance.selectedDate;
        List<UserGroupTaskAssignment> assignments = new List<UserGroupTaskAssignment>();
        tasks?.ForEach(task =>
            assignments.Add(new UserGroupTaskAssignment(-1, _currentContainer.userGroup, task, null, deadline)));
        taskCollections?.ForEach(taskCollection =>
            assignments.Add(new UserGroupTaskAssignment(-1, _currentContainer.userGroup, null, taskCollection,
                deadline)));
        RestConnector.Post(assignments,
            "/userGroups/" + _currentContainer.userGroup.id + "/userGroupTaskAssignments",
            newAssignments =>
            {
                foreach (var newAssignment in newAssignments)
                {
                    AddTaskElement(newAssignment);
                    _currentContainer.userGroup.userGroupTaskAssignments.Add(newAssignment);
                }

                PopupScreenHandler.Instance.Close();
            }, PopupScreenHandler.Instance.ShowConnectionError, updateLocal: false);
    }

    private void DeleteTaskAssignment(UserGroupTaskTableElement container, bool confirmed)
    {
        if (!confirmed)
        {
            PopupScreenHandler.Instance.ShowYesNo("popup-group-remove-assignment",
                "popup-group-remove-assignment-confirm",
                () => DeleteTaskAssignment(container, true));
            return;
        }

        PopupScreenHandler.Instance.ShowLoadingData();
        RestConnector.Delete(container.userGroupTaskAssignment,
            "/userGroups/" + _currentContainer.userGroup.id + "/userGroupTaskAssignments/" +
            container.userGroupTaskAssignment.id,
            () =>
            {
                PopupScreenHandler.Instance.Close();
                Destroy(container.gameObject);
                _currentContainer.userGroup.userGroupTaskAssignments.Remove(
                    _currentContainer.userGroup.userGroupTaskAssignments.Find(assignment =>
                        assignment.id == container.userGroupTaskAssignment.id));
            }, null, PopupScreenHandler.Instance.ShowConnectionError, false);
    }

    private void AddTaskElement(UserGroupTaskAssignment assignment)
    {
        UserGroupTaskTableElement container = Instantiate(userGroupTaskTableElementPrefab, includedTasksList);
        container.Init(assignment, () => DeleteTaskAssignment(container, false));
    }

    private void AddUserElement(User user, bool isIncluded)
    {
        UserAssignableElement assignableElement =
            Instantiate(userAssignableElementPrefab, isIncluded ? includedUsersList : availableUsersList);
        if (isIncluded)
            assignableElement.Init(user, null, () => DeleteOrAddUser(assignableElement, false));
        else
            assignableElement.Init(user, () => DeleteOrAddUser(assignableElement, true), null);
    }

    private void DeleteOrAddUser(UserAssignableElement container, bool addUser)
    {
        PopupScreenHandler.Instance.ShowLoadingData();
        if (addUser)
            AddUser(container, false, "");
        else
        {
            RestConnector.GetUserTaskAssignments(container.user, false, () => DeleteUser(container, false, ""),
                PopupScreenHandler.Instance.ShowConnectionError);
        }
    }

    private void AddUser(UserAssignableElement container, bool confirmed, string pathExtension)
    {
        if (_currentContainer.userGroup.userGroupTaskAssignments.Count > 0 && !confirmed)
            PopupScreenHandler.Instance.ShowConfirmation("popup-group-add-user", "popup-group-add-user-confirm",
                new PopupScreenHandler.ButtonConfig(() => AddUser(container, true, "/addTasks"), "popup-button-yes"),
                new PopupScreenHandler.ButtonConfig(() => AddUser(container, true, ""), "popup-button-no"));
        else
            RestConnector.Post(container.user,
                "/userGroups/" + _currentContainer.userGroup.id + "/users" + pathExtension,
                _ => FinishDeleteOrAddUser(container, true), PopupScreenHandler.Instance.ShowConnectionError,
                updateLocal: false);
    }

    private void DeleteUser(UserAssignableElement container, bool confirmed, string pathExtension)
    {
        if (container.user.taskAssignments.Any(assignment =>
            assignment.userGroupTaskAssignment?.userGroup.id == _currentContainer.userGroup.id) && !confirmed)
            PopupScreenHandler.Instance.ShowConfirmation("popup-group-remove-user", "popup-group-assigned-tasks",
                new PopupScreenHandler.ButtonConfig(() => DeleteUser(container, true, "/removeTasks"),
                    "popup-button-remove"),
                new PopupScreenHandler.ButtonConfig(() => DeleteUser(container, true, ""), "popup-button-convert"));
        else
            RestConnector.Delete(container.user,
                "/userGroups/" + _currentContainer.userGroup.id + "/users/" + container.user.id + pathExtension,
                () => FinishDeleteOrAddUser(container, false), null, PopupScreenHandler.Instance.ShowConnectionError,
                false);
    }

    private void FinishDeleteOrAddUser(UserAssignableElement container, bool addUser)
    {
        DestroyImmediate(container.gameObject);
        AddUserElement(container.user, addUser);
        if (addUser)
            _currentContainer.userGroup.users.Add(container.user);
        else
            _currentContainer.userGroup.users.RemoveAll(user => container.user.id == user.id);
        _currentContainer.UpdateUserCount();
        SortUsers();
        userCountInputField.text = includedUsersList.GetComponentsInChildren<UserAssignableElement>().Length.ToString();
        PopupScreenHandler.Instance.Close();
    }

    private void SortUsers()
    {
        SortUsers(availableUsersList);
        SortUsers(includedUsersList);
    }

    private void SortUsers(Transform list)
    {
        SortListElements<UserAssignableElement>(list,
            (e1, e2) => String.Compare(e1.user.fullName, e2.user.fullName, StringComparison.CurrentCultureIgnoreCase));
    }

    private void SortGroups()
    {
        SortListElements<UserGroupTableElement>(userGroupList,
            (e1, e2) => String.Compare(e1.userGroup.name, e2.userGroup.name,
                StringComparison.CurrentCultureIgnoreCase));
    }
}