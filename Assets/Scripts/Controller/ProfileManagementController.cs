using TMPro;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

/// <summary>
/// Allows the user to modify his profile.
/// </summary>
public class ProfileManagementController : Singleton<ProfileManagementController>
{
    public TMP_InputField nameInputField;
    public TMP_InputField userNameInputField;
    public TMP_InputField currentPasswordInputField;
    public TMP_InputField newPasswordInputField;
    public TMP_InputField repeatedPasswordInputField;
    public TMP_Dropdown securityQuestionDropdown;
    public TMP_InputField securityAnswerInputField;
    public TMP_InputField currentPasswordDeleteProfileInputField;

    private void Start()
    {
        nameInputField.text = string.Empty;
        userNameInputField.text = string.Empty;
        
        // we need to update the current user for the case his deletable state changed because of changes he made in this session
        PopupScreenHandler.Instance.ShowLoadingData();
        RestConnector.GetObject<User>("/users/" + DataController.Instance.CurrentUser.id, user =>
        {
            DataController.Instance.CurrentUser = user;
            nameInputField.text = DataController.Instance.CurrentUser.fullName;
            userNameInputField.text = DataController.Instance.CurrentUser.userName;
            ResetPasswordFields();
            PopupScreenHandler.Instance.Close();
        }, PopupScreenHandler.Instance.ShowConnectionError);
    }

    private void ResetPasswordFields()
    {
        currentPasswordInputField.text = string.Empty;
        newPasswordInputField.text = string.Empty;
        repeatedPasswordInputField.text = string.Empty;
        securityAnswerInputField.text = string.Empty;
        securityQuestionDropdown.value = 0;
    }

    public void ChangeName()
    {
        if (string.IsNullOrEmpty(nameInputField.text))
        {
            PopupScreenHandler.Instance.ShowMessage("edit-profile-values-missing", "edit-profile-full-name-missing");
            return;
        }

        if (string.IsNullOrEmpty(userNameInputField.text))
        {
            PopupScreenHandler.Instance.ShowMessage("edit-profile-values-missing", "edit-profile-name-missing");
            return;
        }

        if (userNameInputField.text.Contains(" "))
        {
            PopupScreenHandler.Instance.ShowMessage("edit-profile-values-wrong", "edit-profile-invalid-name-text");
            return;
        }

        PopupScreenHandler.Instance.ShowLoadingScreen("edit-profile-save-changes", "edit-profile-save-changes-text");
        User currentUser = new User(DataController.Instance.CurrentUser)
        {
            fullName = nameInputField.text,
            userName = userNameInputField.text
        };

        RestConnector.Update(currentUser, UnityWebRequest.kHttpVerbPUT, "/users", updatedUser =>
        {
            DataController.Instance.CurrentUser = updatedUser;
            PopupScreenHandler.Instance.ShowMessage("edit-profile-save-changes", "edit-profile-changes-saved");
        }, PopupScreenHandler.Instance.ShowConnectionError, _ => PopupScreenHandler.Instance.ShowMessage(
            "edit-profile-save-changes", "edit-profile-user-duplicate-name"));
    }

    public void ChangePassword()
    {
        if (currentPasswordInputField.text.Length < 8)
        {
            PopupScreenHandler.Instance.ShowMessage("edit-profile-values-wrong",
                "edit-profile-current-password-too-short");
            return;
        }

        if (newPasswordInputField.text != repeatedPasswordInputField.text)
        {
            PopupScreenHandler.Instance.ShowMessage("edit-profile-values-wrong", "edit-profile-identical-password");
            return;
        }

        if (repeatedPasswordInputField.text.Length < 8)
        {
            PopupScreenHandler.Instance.ShowMessage("edit-profile-values-wrong", "edit-profile-length-password");
            return;
        }

        if (securityQuestionDropdown.value == 0)
        {
            PopupScreenHandler.Instance.ShowMessage("edit-profile-values-wrong", "edit-profile-no-question-selected");
            return;
        }

        if (securityAnswerInputField.text.Length == 0)
        {
            PopupScreenHandler.Instance.ShowMessage("edit-profile-values-missing",
                "edit-profile-new-password-no-answer");
            return;
        }

        PopupScreenHandler.Instance.ShowLoadingScreen("edit-profile-save-changes", "edit-profile-save-changes-text");

        RestConnector.ValidatePassword(DataController.Instance.CurrentUser.id, currentPasswordInputField.text,
            correct =>
            {
                if (correct)
                    RestConnector.UpdateUserPassword(null, newPasswordInputField.text,
                        securityQuestionDropdown.options[securityQuestionDropdown.value].text,
                        securityAnswerInputField.text,
                        DataController.Instance.CurrentUser.id, () =>
                        {
                            ResetPasswordFields();
                            PopupScreenHandler.Instance.ShowMessage("edit-profile-save-changes",
                                "edit-profile-changes-saved");
                        },
                        PopupScreenHandler.Instance.ShowConnectionError);
                else
                    PopupScreenHandler.Instance.ShowMessage("edit-profile-values-wrong", "edit-profile-password-wrong");
            }, PopupScreenHandler.Instance.ShowConnectionError);
    }

    public void Return()
    {
        gameObject.SetActive(false);
    }

    public void DeleteOrDisableUser()
    {
        if (currentPasswordDeleteProfileInputField.text.Length < 8)
        {
            PopupScreenHandler.Instance.ShowMessage("edit-profile-values-wrong",
                "edit-profile-current-password-too-short");
            return;
        }
        
        RestConnector.ValidatePassword(DataController.Instance.CurrentUser.id,
            currentPasswordDeleteProfileInputField.text,
            correct =>
            {
                if (correct)
                    DeleteOrDisableUser(false);
                else
                    PopupScreenHandler.Instance.ShowMessage("edit-profile-values-wrong", "edit-profile-password-wrong");
            }, PopupScreenHandler.Instance.ShowConnectionError);
    }

    /// <summary>
    /// Deletes the user if he has not created or modified any elements. Otherwise the account gets disabled.
    /// </summary>
    private void DeleteOrDisableUser(bool confirmed)
    {
        if (DataController.Instance.CurrentUser.permission.editable)
            DeleteUser(confirmed);
        else
            DisableUser(confirmed);
    }

    private void DeleteUser(bool confirmed)
    {
        if (!confirmed)
        {
            PopupScreenHandler.Instance.ShowConfirmation("edit-profile-remove-user",
                "edit-profile-remove-user-confirmation", () => DeleteUser(true));
            return;
        }

        PopupScreenHandler.Instance.ShowLoadingScreen("edit-profile-remove-user", "edit-profile-removing-user");

        RestConnector.Delete(DataController.Instance.CurrentUser, "/users/" + DataController.Instance.CurrentUser.id,
            () =>
            {
                PopupScreenHandler.Instance.ShowMessage("edit-profile-remove-user", "edit-profile-removed-user",
                    Logout);
            }, null, () => PopupScreenHandler.Instance.ShowConnectionError());
    }

    private void DisableUser(bool confirmed)
    {
        if (!confirmed)
        {
            PopupScreenHandler.Instance.ShowConfirmation("edit-profile-disable-user",
                "edit-profile-disable-user-confirmation", () => DisableUser(true));
            return;
        }

        PopupScreenHandler.Instance.ShowLoadingScreen("edit-profile-disable-user", "edit-profile-disabling-user");

        RestConnector.DisableUser(DataController.Instance.CurrentUser.id,
            () =>
            {
                PopupScreenHandler.Instance.ShowMessage("edit-profile-disable-user", "edit-profile-disabled-user",
                    Logout);
            },
            () => PopupScreenHandler.Instance.ShowConnectionError());
    }

    private void Logout()
    {
        DataController.Instance.CurrentUser = null;
        DataController.Instance.CurrentAccessToken = null;
        DataController.Instance.CurrentRefreshToken = null;
        SceneManager.LoadScene("Login");
    }
}