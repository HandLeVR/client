using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Handles the process for changes the password
/// </summary>
public class PasswordMenuController : MonoBehaviour
{
    public TMP_InputField changePasswordField;
    public TMP_InputField repeatPasswordField;
    public TextMeshProUGUI infoText;
    public TextMeshProUGUI questionText;
    public TMP_Dropdown questionDropdown;
    public TMP_InputField input;
    public Button confirmButton;
    public Button cancelButton;

    private long _currentUserId;
    private string _currentPassword;
    private string _currentSecurityAnswer;
    private UnityAction _onCancel;

    private readonly string AnswerQuestionInfoString =
        "Beantworten Sie bitte folgende Sicherheitsfrage, um Ihr Passwort ändern zu können.";

    private readonly string SelectQuestionInfoString =
        "Wählen Sie bitte eine Sicherheitsfrage aus und beantworten Sie diese, damit Sie Ihr Passwort ändern können, falls Sie es vergessen haben.";

    private readonly string EnterNicknameInfoString =
        "Geben Sie bitte Ihren Nutzernamen ein, um Ihr Passwort zu ändern.";

    private readonly string EnterPasswordInfoString =
        "Geben Sie bitte das neue Passwort ein. Es muss mindestens 8 Zeichen lang sein.";

    private readonly string EnterFirstPasswordInfoString =
        "Sie haben sich das erste Mal angemeldet. Bitte ändern Sie Ihr Passwort. Das Passwort muss mindestens 8 Zeichen lang sein.";

    private readonly string AnswerInputString = "Antwort eingeben";
    private readonly string UsernameInputString = "Nutzernamen eingeben";

    private void Awake()
    {
        cancelButton.onClick.AddListener(Cancel);
    }

    private void OnEnable()
    {
        _currentSecurityAnswer = "";
    }

    /// <summary>
    /// Starts the process to change the password for the first time. That means the user already logged in with the
    /// initial password and he does not need to enter the user name and the security answer.
    /// </summary>
    public void SetFirstPassword(UnityAction onCancel)
    {
        gameObject.SetActive(true);
        _onCancel = onCancel;
        ShowSetPasswordScreen(EnterFirstPasswordInfoString);
    }

    /// <summary>
    /// Starts the process to change the password if the user forgot it. That means he needs the enter his user name
    /// and the answer of the security question to be able to change the password.
    /// </summary>
    /// <param name="onCancel"></param>
    public void ChangePassword(UnityAction onCancel)
    {
        gameObject.SetActive(true);
        _onCancel = onCancel;
        ShowEnterUserNameScreen();
    }

    /// <summary>
    /// Closes the screen and returns to the login screen.
    /// </summary>
    private void Cancel()
    {
        gameObject.SetActive(false);
        _onCancel.Invoke();
    }

    /// <summary>
    /// Shows the screen where the user needs to enter his user name.
    /// </summary>
    private void ShowEnterUserNameScreen()
    {
        ResetAll();
        infoText.gameObject.SetActive(true);
        infoText.text = EnterNicknameInfoString;
        input.gameObject.SetActive(true);
        input.placeholder.GetComponent<TextMeshProUGUI>().text = UsernameInputString;
        confirmButton.onClick.AddListener(GetSecurityQuestion);
    }
    
    /// <summary>
    /// Shows the screen where the user needs to answer the security question.
    /// </summary>
    private void ShowAnswerQuestionScreen(string securityQuestion)
    {
        ResetAll();
        infoText.gameObject.SetActive(true);
        infoText.text = AnswerQuestionInfoString;
        questionText.gameObject.SetActive(true);
        questionText.text = securityQuestion;
        input.gameObject.SetActive(true);
        input.placeholder.GetComponent<TextMeshProUGUI>().text = AnswerInputString;
        confirmButton.onClick.AddListener(ValidateSecurityAnswer);
    }
    
    /// <summary>
    /// Shows the screen where the user needs to select a new security question.
    /// </summary>
    private void ShowSelectQuestionScreen()
    {
        ResetAll();
        infoText.gameObject.SetActive(true);
        infoText.text = SelectQuestionInfoString;
        questionDropdown.gameObject.SetActive(true);
        input.gameObject.SetActive(true);
        input.placeholder.GetComponent<TextMeshProUGUI>().text = AnswerInputString;
        confirmButton.onClick.AddListener(UpdateUserPassword);
    }

    
    /// <summary>
    /// Shows the screen where the user can change his password.
    /// </summary>
    private void ShowSetPasswordScreen(string info)
    {
        ResetAll();
        infoText.gameObject.SetActive(true);
        infoText.text = info;
        changePasswordField.gameObject.SetActive(true);
        repeatPasswordField.gameObject.SetActive(true);
        confirmButton.onClick.AddListener(ContinueToSecurityQuestion);
    }

    private void ResetAll()
    {
        changePasswordField.gameObject.SetActive(false);
        repeatPasswordField.gameObject.SetActive(false);
        infoText.gameObject.SetActive(false);
        questionText.gameObject.SetActive(false);
        questionDropdown.gameObject.SetActive(false);
        input.gameObject.SetActive(false);
        questionDropdown.value = 0;
        changePasswordField.text = String.Empty;
        repeatPasswordField.text = String.Empty;
        input.text = String.Empty;
        confirmButton.onClick.RemoveAllListeners();
    }

    /// <summary>
    /// Checks whether the password fields are filled out correctly and then continues the the security question screen.
    /// </summary>
    private void ContinueToSecurityQuestion()
    {
        if (changePasswordField.text != repeatPasswordField.text)
        {
            PopupScreenHandler.Instance.ShowMessage("popup-wrong-input", "popup-identical-password");
            return;
        }

        if (changePasswordField.text.Length < 8)
        {
            PopupScreenHandler.Instance.ShowMessage("popup-wrong-input", "popup-length-password");
            return;
        }

        // current password is saved to be able to send it the server in the next screen
        _currentPassword = changePasswordField.text;
        ShowSelectQuestionScreen();
    }

    /// <summary>
    /// Checks whether the user name entered exists and then gets the security question of the user from the server.
    /// </summary>
    private void GetSecurityQuestion()
    {
        if (input.text.Length == 0)
        {
            PopupScreenHandler.Instance.ShowMessage("popup-wrong-input", "popup-user-invalid-name");
            return;
        }

        PopupScreenHandler.Instance.ShowLoadingScreen("popup-server-request", "popup-please-wait");
        RestConnector.GetSecurityQuestion(input.text,
            (userId, securityQuestion) =>
            {
                _currentUserId = userId;
                ShowAnswerQuestionScreen(securityQuestion);
                PopupScreenHandler.Instance.Close();
            },
            () => PopupScreenHandler.Instance.ShowMessage("popup-new-password-user-does-not-exist",
                "popup-new-password-user-does-not-exist-text"), PopupScreenHandler.Instance.ShowConnectionError);
    }

    /// <summary>
    /// Checks whether a security question is selected and an answer is given and then updates the password and
    /// the security question.
    /// </summary>
    private void UpdateUserPassword()
    {
        if (questionDropdown.value == 0)
        {
            PopupScreenHandler.Instance.ShowMessage("popup-wrong-input", "popup-new-password-no-question-selected");
            return;
        }

        if (input.text.Length == 0)
        {
            PopupScreenHandler.Instance.ShowMessage("popup-wrong-input", "popup-new-password-no-answer");
            return;
        }

        PopupScreenHandler.Instance.ShowLoadingScreen("popup-server-request", "popup-please-wait");
        // _currentSecurityAnswer is empty if the user changes the password for the first time
        RestConnector.UpdateUserPassword(_currentSecurityAnswer, _currentPassword,
            questionDropdown.options[questionDropdown.value].text, input.text,
            DataController.Instance.CurrentUser == null ? _currentUserId : DataController.Instance.CurrentUser.id, () =>
                PopupScreenHandler.Instance.ShowMessage(
                    "popup-password-changed", "popup-password-changed-text",
                    () =>
                    {
                        if (_currentSecurityAnswer.Length == 0)
                            SceneManager.LoadScene("MainMenu");
                        else
                        {
                            PopupScreenHandler.Instance.Close();
                            Cancel();
                        }
                    }),
            PopupScreenHandler.Instance.ShowConnectionError);
    }

    /// <summary>
    /// Validates the security answer and moves on to the change password screen if it was correct.
    /// </summary>
    private void ValidateSecurityAnswer()
    {
        if (input.text.Length == 0)
        {
            PopupScreenHandler.Instance.ShowMessage("popup-wrong-input", "popup-new-password-no-answer");
            return;
        }

        PopupScreenHandler.Instance.ShowLoadingScreen("popup-server-request", "popup-please-wait");
        RestConnector.ValidateSecurityAnswer(_currentUserId, input.text,
            correct =>
            {
                if (correct)
                {
                    // save for change password request because we use the security answer for authorization
                    _currentSecurityAnswer = input.text;
                    ShowSetPasswordScreen(EnterPasswordInfoString);
                    PopupScreenHandler.Instance.Close();
                }
                else
                    PopupScreenHandler.Instance.ShowMessage("popup-new-password-wrong-answer",
                        "popup-new-password-wrong-answer-text");
            }, PopupScreenHandler.Instance.ShowConnectionError);
    }
}