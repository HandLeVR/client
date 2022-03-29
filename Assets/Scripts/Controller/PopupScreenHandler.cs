using System;
using System.Collections.Generic;
using TMPro;
using translator;
using UnityEngine;
using UnityEngine.Events;
using Application = UnityEngine.Application;
using Button = UnityEngine.UI.Button;

/// <summary>
/// This controller handles the popup used for show messages, show loading screens and request confirmations. 
/// </summary>
public class PopupScreenHandler : Singleton<PopupScreenHandler>
{
    public TextMeshProUGUI header;
    public TextMeshProUGUI text;
    public Button button1;
    public Button button2;
    public Button button3;
    public GameObject button4Container;
    public Button button4;
    public GameObject loading;
    public GameObject nameInput;
    public TMP_InputField nameInputField;
    public GameObject longText;
    public TextMeshProUGUI longTextField;
    public GameObject infos;
    public TextMeshProUGUI createdByText;
    public TextMeshProUGUI createdDateText;
    public TextMeshProUGUI lastEditedByText;
    public TextMeshProUGUI lastEditedDateText;
    public TextMeshProUGUI editableText;
    public TextMeshProUGUI removableText;

    private List<Button> buttons;

    public void Awake()
    {
        buttons = new List<Button> { button1, button2, button3, button4 };
    }

    /// <summary>
    /// Closes the popup screen.
    /// </summary>
    public void Close()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Shows the default loading screen.
    /// </summary>
    public void ShowLoadingData()
    {
        ShowLoadingScreen("popup-load-data", "popup-loading-data");
    }

    /// <summary>
    /// Shows the loading screen with a custom header and a custom message.
    /// </summary>
    public void ShowLoadingScreen(string headerText, string message)
    {
        InitPopup(headerText);
        SetLoading(message);
    }

    /// <summary>
    /// Shows the given message with the given header.
    /// </summary>
    public void ShowMessage(string headerText, string message)
    {
        ShowMessage(headerText, message, Close);
    }

    /// <summary>
    /// Shows the given message with the given header and calls the given action on confirmation.
    /// </summary>
    public void ShowMessage(string headerText, string message, UnityAction onConfirmation)
    {
        InitPopup(headerText);
        SetText(message);
        SetButton(button1, "popup-button-okay", onConfirmation);
    }

    /// <summary>
    /// Shows the given message with the given header and calls the given action on confirmation or calls nothing if
    /// the abort button is pressed.
    /// </summary>
    public void ShowConfirmation(string headerText, string message, UnityAction onConfirmation)
    {
        InitPopup(headerText);
        SetText(message);
        SetButton(button1, "popup-button-okay", () =>
        {
            Close();
            onConfirmation.Invoke();
        });
        SetButton(button2, "popup-button-abort", Close);
    }

    /// <summary>
    /// Shows the given message with the given header and allows to define custom buttons.
    /// </summary>
    public void ShowConfirmation(string headerText, string message, params ButtonConfig[] buttonConfig)
    {
        InitPopup(headerText);
        SetText(message);
        for (int i = 0; i < buttons.Count && i < buttonConfig.Length; i++)
            buttonConfig[i].SetButton(buttons[i]);
    }

    /// <summary>
    /// Shows the given message with the given header and calls the given action if "yes" is pressed and calls nothing if "no" is pressed.
    /// </summary>
    public void ShowYesNo(string headerText, string message, UnityAction onConfirmation)
    {
        InitPopup(headerText);
        SetText(message);
        SetButton(button1, "popup-button-yes", () =>
        {
            Close();
            onConfirmation.Invoke();
        });
        SetButton(button2, "popup-button-no", Close);
    }

    /// <summary>
    /// Shows the default connection error message.
    /// </summary>
    public void ShowConnectionError()
    {
        InitPopup("popup-connection-error");
        SetText("popup-connection-error-try-later");
        SetButton(button1, "popup-button-okay", Close);
    }

    /// <summary>
    /// Shows the default missing values messages in which the given string is added.
    /// </summary>
    public void ShowMissingValues(string headerText, string missingValuesString)
    {
        InitPopup(headerText);
        text.gameObject.SetActive(true);
        text.text = TranslationController.Instance.Translate("popup-missing-values-text") + "\n \n" +
                    missingValuesString;
        SetButton(button1, "popup-button-okay", Close);
    }

    /// <summary>
    /// Prompts the user to enter a new name and calls the action on confirmation.
    /// </summary>
    public void ShowCopyTask(string copyName, UnityAction<string> copyAction)
    {
        InitPopup("popup-save-task");
        nameInput.SetActive(true);
        nameInputField.text = copyName;
        SetButton(button1, "popup-button-copy", () => copyAction(nameInputField.text));
        SetButton(button2, "popup-button-abort", Close);
    }

    /// <summary>
    /// Shows the created user screen showing the given password and allowing the user to open the password file
    /// by clicking a button.
    /// </summary>
    public void ShowUserCreated(string password, string filePath)
    {
        InitPopup("popup-save-user");
        text.gameObject.SetActive(true);
        text.text = String.Format(TranslationController.Instance.Translate("popup-saved-user-1"), password) + "\n \n" +
                    String.Format(TranslationController.Instance.Translate("popup-saved-user-2"), filePath);
        SetButton(button1, "popup-open-file", () => Application.OpenURL(filePath));
        SetButton(button2, "popup-button-okay", Close);
    }

    /// <summary>
    /// Shows the default unsaved changes screen.
    /// </summary>
    public void ShowUnsavedChanges(UnityAction onConfirmation)
    {
        InitPopup("popup-unsaved-changes");
        SetText("popup-unsaved-changes-confirm");
        SetButton(button1, "popup-button-okay", () =>
        {
            Close();
            onConfirmation.Invoke();
        });
        SetButton(button2, "popup-button-abort", Close);
    }

    /// <summary>
    /// Opens the remove recording screen where the user can determine were the recording will be removed. 
    /// </summary>
    /// <param name="onServer">Called if the server button is pressed.</param>
    /// <param name="onLocal">Called if the local button is pressed.</param>
    /// <param name="onBoth">Called if the both button is pressed.</param>
    public void ShowRemoveRecordingConfirmation(UnityAction onServer, UnityAction onLocal, UnityAction onBoth)
    {
        InitPopup("popup-remove-recording");
        SetText("popup-remove-recording-where");
        SetButton(button1, "popup-button-server", onServer);
        SetButton(button2, "popup-button-local", onLocal);
        SetButton(button3, "popup-button-everywhere", onBoth);
        button4Container.SetActive(true);
        SetButton(button4, "popup-button-abort", Close);
    }

    /// <summary>
    /// Prompts the user to confirm exiting the application.
    /// </summary>
    public void ShowExitApplication()
    {
        InitPopup("popup-exit-application");
        SetText("popup-exit-application-confirm");
        SetButton(button1, "popup-button-okay", () =>
        {
            Close();
            Application.Quit();
        });
        SetButton(button2, "popup-button-abort", Close);
    }

    /// <summary>
    /// Shows the terms of service screen.
    /// </summary>
    public void ShowTermsOfService(params ButtonConfig[] buttonConfig)
    {
        InitPopup("popup-terms-of-service");
        string content = System.IO.File.ReadAllText(DataController.Instance.privacyFilePath);
        longTextField.text = content;
        longText.SetActive(true);
        for (int i = 0; i < buttons.Count && i < buttonConfig.Length; i++)
            buttonConfig[i].SetButton(buttons[i]);
    }

    /// <summary>
    /// Opens the popup to show additional information for an entity.
    /// </summary>
    /// <param name="permission">The permission object used to display all information</param>
    /// <param name="alwaysEditable">If true, Permission.editable only affects the removable text</param>
    /// <param name="fallBackUser">Only set in UserTableElement to handle the case that a user made changes to his own account</param>
    public void ShowInfos(Permission permission, bool alwaysEditable = false, User fallBackUser = null)
    {
        InitPopup("popup-info");
        infos.SetActive(true);
        createdByText.text = String.IsNullOrEmpty(permission.createdByFullName) && fallBackUser != null
            ? fallBackUser.fullName
            : permission.createdByFullName;
        createdByText.text = permission.createdByFullName;
        createdDateText.text = permission.createdDate.ToString("dd.MM.yyyy HH:mm:ss");
        lastEditedByText.text = String.IsNullOrEmpty(permission.lastEditedByFullName) && fallBackUser != null
            ? fallBackUser.fullName
            : permission.lastEditedByFullName;
        lastEditedDateText.text = permission.lastEditedDate.ToString("dd.MM.yyyy HH:mm:ss");
        editableText.text = alwaysEditable || permission.editable ? "Ja" : "Nein";
        removableText.text = permission.editable ? "Ja" : "Nein";
        SetButton(button1, "popup-button-okay", Close);
    }

    /// <summary>
    /// Inits the default popup screen. The given header text is translated.
    /// </summary>
    private void InitPopup(string headerText)
    {
        gameObject.SetActive(true);
        header.text = TranslationController.Instance.Translate(headerText);
        button1.gameObject.SetActive(false);
        button2.gameObject.SetActive(false);
        button3.gameObject.SetActive(false);
        loading.SetActive(false);
        nameInput.SetActive(false);
        text.gameObject.SetActive(false);
        button4Container.SetActive(false);
        longText.SetActive(false);
        infos.SetActive(false);
    }

    /// <summary>
    /// Sets the label and the on click event of a button. The given text is translated.
    /// </summary>
    private void SetButton(Button button, string label, UnityAction call)
    {
        button.gameObject.SetActive(true);
        button.GetComponentInChildren<TextMeshProUGUI>().text = TranslationController.Instance.Translate(label);
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(call);
    }

    /// <summary>
    /// Sets the text of the popup. The text is translated.
    /// </summary>
    private void SetText(string message)
    {
        text.gameObject.SetActive(true);
        text.text = TranslationController.Instance.Translate(message);
    }

    /// <summary>
    /// Sets the loading screen. The given text is translated.
    /// </summary>
    private void SetLoading(string loadingText)
    {
        loading.SetActive(true);
        loading.GetComponentInChildren<TextMeshProUGUI>().text = TranslationController.Instance.Translate(loadingText);
    }

    /// <summary>
    /// Contains a button configuration with a label and an on click event.
    /// </summary>
    public class ButtonConfig
    {
        private readonly UnityAction onClick;
        private readonly string label;

        public ButtonConfig(UnityAction onClick, string label)
        {
            this.onClick = onClick;
            this.label = label;
        }

        /// <summary>
        /// Transfers the configuration to the button.
        /// </summary>
        public void SetButton(Button button)
        {
            button.gameObject.SetActive(true);
            button.GetComponentInChildren<TextMeshProUGUI>().text = TranslationController.Instance.Translate(label);
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(onClick);
        }
    }
}