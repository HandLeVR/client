using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using translator;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Class for displaying users in the middle section (all user list)
/// </summary>
public class UserMenuController : BaseMenuController
{
    public Transform userList;
    public UserTableElement userTableElementPrefab;
    public Button addUserButton;

    public TMP_InputField fullnameInputField;
    public TMP_InputField nameInputField;
    public TMP_InputField groupsInputField;
    public TMP_Dropdown roleDropdown;

    public Transform userTasksList;
    public UserTaskTableElement userTaskTableElementPrefab;

    public Button addTaskCollectionButton;
    public Button addTaskButton;

    private UserTableElement _currentContainer;
    private List<Selectable> _selectables;
    private string _missingValuesString;
    private string _pathToPasswordFile;

    private void Awake()
    {
        _pathToPasswordFile = Application.streamingAssetsPath + "/Passwörter.txt";

        _selectables = new List<Selectable>
        {
            nameInputField, fullnameInputField, roleDropdown, addTaskCollectionButton, addTaskButton, saveButton
        };
        addUserButton.onClick.AddListener(() => SetUpByUser(null, false));
        saveButton.onClick.AddListener(SaveSettings);
        addTaskButton.onClick.AddListener(() =>
            SelectionPopup.Instance.Init(typeof(Task), AssignTasks));
        addTaskCollectionButton.onClick.AddListener(() =>
            SelectionPopup.Instance.Init(typeof(TaskCollection), AssignTaskCollections));
        AddSetUnsavedChangesListener(_selectables);
    }

    private void OnEnable()
    {
        userList.DestroyImmediateAllChildren();
        MainScreenController.Instance.LoadData(DataController.RequestType.Users, InstantiateContainer);
    }

    private void ResetFields()
    {
        _selectables.ForEach(selectable => selectable.interactable = false);
        nameInputField.text = String.Empty;
        fullnameInputField.text = String.Empty;
        groupsInputField.text = String.Empty;
        userTasksList.DestroyAllChildren();
        SetUnsavedChanges(false);
    }

    private void InstantiateContainer()
    {
        userList.DestroyImmediateAllChildren();
        foreach (var user in DataController.Instance.users.Values)
        {
            UserTableElement userTableElement = Instantiate(userTableElementPrefab, userList);
            userTableElement.Init(user, SetUpByUser, DeleteOrDisableUser);
        }

        SortUsers();
        ResetFields();
    }

    /// <summary>
    /// Checks, if some fields aren't filled in correctly.
    /// </summary>
    /// <returns>returns true if something is missing, otherwise false</returns>
    private bool ValuesMissing()
    {
        _missingValuesString = "";
        if (nameInputField.text == "")
            _missingValuesString += " " + TranslationController.Instance.Translate("user-name-missing") + "\n";
        if (fullnameInputField.text == "")
            _missingValuesString += " " + TranslationController.Instance.Translate("full-user-name-missing") + "\n";
        return nameInputField.text == "" || fullnameInputField.text == "";
    }

    private bool InvalidUserName()
    {
        return nameInputField.text.Contains(" ");
    }

    /// <summary>
    /// The fields get filled with information given by a user as currentUser.
    /// </summary>
    private void SetUpByUser(UserTableElement userTableElement, bool confirmed)
    {
        if (HasUnsavedChanges() && !confirmed)
        {
            PopupScreenHandler.Instance.ShowUnsavedChanges(() => SetUpByUser(userTableElement, true));
            return;
        }

        _currentContainer = userTableElement;
        if (userTableElement != null)
        {
            PopupScreenHandler.Instance.ShowLoadingData();
            RestConnector.GetUserData(_currentContainer.user, () =>
            {
                ShowUserProperties(_currentContainer.user);
                PopupScreenHandler.Instance.Close();
            }, () =>
            {
                ResetFields();
                PopupScreenHandler.Instance.ShowConnectionError();
            });
        }
        else
        {
            ShowUserProperties(new User());
        }
    }

    /// <summary>
    /// Initializes the field on the right side by the given user.
    /// </summary>
    private void ShowUserProperties(User user, bool reloadTaskAssignments = true)
    {
        nameInputField.text = user.userName;
        fullnameInputField.text = user.fullName;
        groupsInputField.text = string.Join(", ", user.userGroups.Select(group => group.name));
        SetRoleDropdown(roleDropdown, user.role);
        _selectables.ForEach(selectable => selectable.interactable = true);

        if (DataController.Instance.CurrentUser.role != User.Role.Teacher)
        {
            nameInputField.interactable = false;
            fullnameInputField.interactable = false;
            roleDropdown.interactable = false;
            addUserButton.interactable = false;
        }

        addTaskButton.interactable = _currentContainer != null;
        addTaskCollectionButton.interactable = _currentContainer != null;

        if (reloadTaskAssignments)
        {
            userTasksList.DestroyAllChildren();
            user.taskAssignments.ForEach(CreateTaskFromAssignment);
        }

        SetUnsavedChanges(false);
    }

    /// <summary>
    /// Initializes a dropdown which is used for teh selection of roles.
    /// </summary>
    private void SetRoleDropdown(TMP_Dropdown dropdown, User.Role role)
    {
        dropdown.options.Clear();
        foreach (User.Role r in (User.Role[])Enum.GetValues(typeof(User.Role)))
            dropdown.options.Add(new TMP_Dropdown.OptionData(TranslationController.Instance.Translate(r.ToString())));
        SetDropdown(dropdown,
            dropdown.options.FindIndex(i => i.text.Equals(TranslationController.Instance.Translate(role.ToString()))));
    }

    private void CreateTaskFromAssignment(TaskAssignment assignment)
    {
        UserTaskTableElement container =
            Instantiate(userTaskTableElementPrefab, userTasksList);
        container.Init(assignment, assignment.taskCollectionAssignment,
            assignment.userGroupTaskAssignment?.userGroup, assignment.taskResults, ShowEvaluation,
            () => DeleteTaskAssignment(container, false));
    }

    private void DeleteTaskAssignment(UserTaskTableElement container, bool confirmed)
    {
        if (!confirmed)
        {
            PopupScreenHandler.Instance.ShowConfirmation("popup-remove-task-assignment",
                container.taskCollectionAssignment != null
                    ? string.Format(
                        TranslationController.Instance.Translate("popup-remove-task-assignment-task-collection"),
                        container.taskAssignment.task.name, container.taskCollectionAssignment.taskCollection.name)
                    : string.Format(
                        TranslationController.Instance.Translate("popup-remove-task-assignment-confirmation"),
                        container.taskAssignment.task.name),
                () => DeleteTaskAssignment(container, true));
            return;
        }

        PopupScreenHandler.Instance.ShowLoadingScreen("popup-user-remove-task", "popup-user-removing-task");

        if (container.taskCollectionAssignment == null)
            RestConnector.Delete(container.taskAssignment,
                "/users/" + _currentContainer.user.id + "/taskAssignment/" + container.taskAssignment.id,
                () =>
                {
                    Destroy(container.gameObject);
                    PopupScreenHandler.Instance.Close();
                }, null, () => PopupScreenHandler.Instance.ShowConnectionError(), false);
        else
            RestConnector.Delete(container.taskAssignment,
                "/users/" + _currentContainer.user.id + "/taskCollectionAssignment/" +
                container.taskCollectionAssignment.id, () => SetUpByUser(_currentContainer, true), null,
                () => PopupScreenHandler.Instance.ShowConnectionError(), false);
    }

    private void ShowEvaluation()
    {
        // TODO
    }

    private void AssignTasks()
    {
        List<Task> tasks = SelectionPopup.Instance.GetSelectedIds().Select(id => DataController.Instance.tasks[id])
            .ToList();
        List<TaskAssignment> assignments = new List<TaskAssignment>();
        tasks.ForEach(task =>
            assignments.Add(new TaskAssignment(-1, null, task, null, null, DeadlinePicker.Instance.selectedDate)));
        PopupScreenHandler.Instance.ShowLoadingScreen("popup-user-assign-task", "popup-user-assigning-task");
        RestConnector.Post(assignments, "/users/" + _currentContainer.user.id + "/taskAssignments",
            _ => SetUpByUser(_currentContainer, true), PopupScreenHandler.Instance.ShowConnectionError,
            updateLocal: false);
    }


    private void AssignTaskCollections()
    {
        List<TaskCollection> taskCollections = SelectionPopup.Instance.GetSelectedIds()
            .Select(id => DataController.Instance.taskCollections[id])
            .ToList();
        List<TaskCollectionAssignment> assignments = new List<TaskCollectionAssignment>();
        taskCollections.ForEach(taskCollection =>
            assignments.Add(new TaskCollectionAssignment(-1, taskCollection, DeadlinePicker.Instance.selectedDate)));
        PopupScreenHandler.Instance.ShowLoadingScreen("popup-user-assign-task", "popup-user-assigning-task");
        RestConnector.Post(assignments, "/users/" + _currentContainer.user.id + "/taskCollectionAssignments",
            _ => SetUpByUser(_currentContainer, true), PopupScreenHandler.Instance.ShowConnectionError,
            updateLocal: false);
    }

    private void DeleteOrDisableUser(UserTableElement container, bool confirmed)
    {
        if (container.user.permission.editable)
            DeleteUser(container, confirmed);
        else
            DisableUser(container, confirmed);
    }

    private void DeleteUser(UserTableElement container, bool confirmed)
    {
        if (!confirmed)
        {
            PopupScreenHandler.Instance.ShowConfirmation("popup-remove-user",
                "popup-remove-user-confirmation", () => DeleteUser(container, true));
            return;
        }

        PopupScreenHandler.Instance.ShowLoadingScreen("popup-remove-user", "popup-removing-user");

        RestConnector.Delete(container.user, "/users/" + container.user.id, () =>
        {
            if (_currentContainer != null && container.user.id == _currentContainer.user.id)
                ResetFields();
            Destroy(container.gameObject);
            PopupScreenHandler.Instance.ShowMessage("popup-remove-user", "popup-removed-user");
        }, null, () => PopupScreenHandler.Instance.ShowConnectionError());
    }

    private void DisableUser(UserTableElement container, bool confirmed)
    {
        if (!confirmed)
        {
            PopupScreenHandler.Instance.ShowConfirmation("popup-disable-user",
                "popup-disable-user-confirmation", () => DisableUser(container, true));
            return;
        }

        PopupScreenHandler.Instance.ShowLoadingScreen("popup-disable-user", "popup-disabling-user");

        RestConnector.DisableUser(container.user.id,
            () =>
            {
                if (_currentContainer != null && container.user.id == _currentContainer.user.id)
                    ResetFields();
                Destroy(container.gameObject);
                PopupScreenHandler.Instance.ShowMessage("popup-disable-user", "popup-disabled-user",
                    () =>
                    {
                        if (DataController.Instance.CurrentUser.id == container.user.id)
                            SceneManager.LoadScene("Login");
                        else
                            PopupScreenHandler.Instance.Close();
                    });
            },
            () => PopupScreenHandler.Instance.ShowConnectionError());
    }

    private void SaveSettings()
    {
        if (ValuesMissing())
        {
            PopupScreenHandler.Instance.ShowMissingValues("popup-user-missing-values", _missingValuesString);
            return;
        }

        if (InvalidUserName())
        {
            PopupScreenHandler.Instance.ShowMessage("popup-user-invalid-name", "popup-user-invalid-name-text");
            return;
        }

        PopupScreenHandler.Instance.ShowLoadingScreen("popup-save-user",
            _currentContainer == null ? "popup-creating-user" : "popup-updating-user");
        User currentUser = _currentContainer == null ? new User() : _currentContainer.user;
        currentUser.fullName = fullnameInputField.text;
        currentUser.userName = nameInputField.text;
        currentUser.role = Enum.GetValues(typeof(User.Role)).Cast<User.Role>().ToList()[roleDropdown.value];

        if (_currentContainer == null)
        {
            currentUser.password = PasswordGenerator.GenerateRandomAlphaNumericStr(8);
            currentUser.id = -1;
        }

        RestConnector.Update(currentUser,
            currentUser.id < 0 ? UnityWebRequest.kHttpVerbPOST : UnityWebRequest.kHttpVerbPUT, "/users", updatedUser =>
            {
                if (_currentContainer == null)
                    _currentContainer = Instantiate(userTableElementPrefab, userList);
                _currentContainer.Init(updatedUser, SetUpByUser, DeleteOrDisableUser);
                ShowUserProperties(updatedUser, false);
                SortUsers();
                _currentContainer.containerButton.Select();
                SetUnsavedChanges(false);
                if (currentUser.id < 0)
                {
                    using (TextWriter writer = new StreamWriter(_pathToPasswordFile, true))
                    {
                        writer.WriteLine(currentUser.userName + " " + currentUser.password);
                        writer.Close();
                    }

                    PopupScreenHandler.Instance.ShowUserCreated(currentUser.password, _pathToPasswordFile);
                }
                else
                {
                    PopupScreenHandler.Instance.ShowMessage("popup-save-user", "popup-updated-user");
                }

                if (updatedUser.id == DataController.Instance.CurrentUser.id)
                    DataController.Instance.CurrentUser = updatedUser;
            }, PopupScreenHandler.Instance.ShowConnectionError, _ => PopupScreenHandler.Instance.ShowMessage(
                "popup-save-user", "popup-user-duplicate-name"));
    }

    private void SortUsers()
    {
        SortListElements<UserTableElement>(userList,
            (e1, e2) => String.Compare(e1.user.fullName, e2.user.fullName, StringComparison.CurrentCultureIgnoreCase));
    }
}